using System.CommandLine;
using System.Diagnostics;
using System.Runtime.InteropServices;
using AIINInterfaces;
using AIINLib;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OsmSharp.Streams;

namespace AIINCli;

internal static class Program
{
    private const string OverpassUrl = "https://overpass-api.de/api/interpreter";

    private static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand(
            """
            AIIN CLI

            Example usage:
            $ AIINCli fetch data.osm
            $ AIINCli analyze data.osm data.json.gz
            $ AIINCli optimize data.json.gz data-opt.json.gz
            $ AIINCli visualize data-opt.json.gz --original data.json.gz
            $ AIINCli create-parcel-graph data-opt.json.gz parcel-graph.json.gz
            $ AIINCli visualize parcel-graph.json.gz --original data.json.gz
            $ AIINCli run-optimizer parcel-graph.json.gz best.json.gz
            $ AIINCli visualize data.json.gz --path best.json.gz
            """);

        var bboxOption = new Option<string>(
            name: "--bbox",
            description: "Bounding box",
            getDefaultValue: () => "49.98,19.84,50.12,20.03"
        );

        var outputFileArgument = new Argument<FileInfo>("outputFile", "Output file");
        var inputFileArgument = new Argument<FileInfo>("inputFile", "Input file");

        var fetchDataCommand = new Command("fetch", "Fetch data from Overpass API for the given bounding box")
        {
            bboxOption,
            outputFileArgument
        };

        var analyzeCommand = new Command("analyze", "Build the road graph")
        {
            inputFileArgument,
            outputFileArgument
        };

        var optimizeCommand = new Command("optimize", "Optimize the road graph")
        {
            inputFileArgument,
            outputFileArgument
        };

        var createParcelGraphCommand = new Command("create-parcel-graph", "Create the parcel graph")
        {
            inputFileArgument,
            outputFileArgument
        };

        var originalOption = new Option<FileInfo?>(
            name: "--original",
            description: "Show the original graph for comparison"
        );

        var pathOption = new Option<FileInfo?>(
            name: "--path",
            description: "Show a path on the graph"
        );

        var visualizeCommand = new Command("visualize", "Visualize graphs on a map")
        {
            inputFileArgument,
            originalOption,
            pathOption
        };

        var runOptimizerCommand = new Command("run-optimizer", "Run the TSP optimizer and save the best graph cycle")
        {
            inputFileArgument,
            outputFileArgument
        };

        fetchDataCommand.SetHandler(FetchData, bboxOption, outputFileArgument);
        analyzeCommand.SetHandler(Analyze, inputFileArgument, outputFileArgument);
        optimizeCommand.SetHandler(Optimize, inputFileArgument, outputFileArgument);
        createParcelGraphCommand.SetHandler(CreateParcelGraph, inputFileArgument, outputFileArgument);
        visualizeCommand.SetHandler(Visualize, inputFileArgument, originalOption, pathOption);
        runOptimizerCommand.SetHandler(RunOptimizer, inputFileArgument, outputFileArgument);
        rootCommand.AddCommand(fetchDataCommand);
        rootCommand.AddCommand(analyzeCommand);
        rootCommand.AddCommand(optimizeCommand);
        rootCommand.AddCommand(createParcelGraphCommand);
        rootCommand.AddCommand(visualizeCommand);
        rootCommand.AddCommand(runOptimizerCommand);

        return await rootCommand.InvokeAsync(args);
    }

    private static async Task FetchData(string bbox, FileInfo fileInfo)
    {
        Console.WriteLine($"Downloading data for bounding box {bbox}");
        var client = new OverpassClient(OverpassUrl);
        using var response = await client.Fetch(
            $"""
             [timeout:25][bbox:{bbox}];

             node
                [amenity=parcel_locker]
                [operator=InPost]
                ->.parcel_lockers;

             way
                [highway~"^(motorway|motorway_link|trunk|trunk_link|primary|primary_link|secondary|secondary_link|tertiary|tertiary_link|unclassified|road|residential|living_street)$"]
                [access != no]
                [vehicle != no]
                [motor_vehicle != no]
                [motor_car != no]
                [area != yes]
                ->.roads;

             .roads > ->.road_nodes;

             (.parcel_lockers; .roads; .road_nodes;);
             out body qt;
             """);

        await using var file = fileInfo.Open(FileMode.Create, FileAccess.ReadWrite);
        var target = new XmlOsmStreamTarget(file);
        target.Initialize();
        target.RegisterSource(response.Where(x => x != null));
        target.Pull();
        target.Flush();
        target.Close();

        Console.WriteLine("Fetched data successfully");
    }

    private static void Analyze(FileInfo fileInfo, FileInfo outFileInfo)
    {
        var roadGraphBuilder = new RoadGraphBuilder();
        var serializer = new GraphSerializer();

        using var file = fileInfo.OpenRead();
        var source = new XmlOsmStreamSource(file);
        var (parcelLockers, roadNodes, roads) = Preprocessing.SplitByType(source);

        Console.WriteLine($"parcel lockers: {parcelLockers.Count}");
        Console.WriteLine($"road nodes: {roadNodes.Count}");
        var oneWayCount = roads.Count(x => x.OneWay);
        Console.WriteLine($"two-way roads: {roads.Count - oneWayCount}");
        Console.WriteLine($"one-way roads: {oneWayCount}");
        var graph = roadGraphBuilder.CreateRoadGraph(roads, roadNodes, parcelLockers);

        using var stream = outFileInfo.Create();
        serializer.Serialize(stream, graph);
    }


    private static void Optimize(FileInfo inFile, FileInfo outFile)
    {
        var roadGraphBuilder = new RoadGraphBuilder();
        var serializer = new GraphSerializer();

        using var stream = inFile.OpenRead();
        using var outStream = outFile.OpenWrite();

        var graph = serializer.Deserialize(stream);
        var optimized = roadGraphBuilder.OptimizeRoadGraph(graph);

        var parcelLockers = graph.Count(x => x is ParcelLockerGraphNode);
        var parcelLockersOpt = optimized.Count(x => x is ParcelLockerGraphNode);
        Console.WriteLine($"parcel lockers: {parcelLockers} => {parcelLockersOpt}");
        Console.WriteLine($"road nodes: {graph.Count - parcelLockers} => {optimized.Count - parcelLockersOpt}");

        serializer.Serialize(outStream, optimized);
    }

    private static void CreateParcelGraph(FileInfo inFile, FileInfo outFile)
    {
        var parcelLockerGraphBuilder = new ParcelLockerGraphBuilder();
        var serializer = new GraphSerializer();

        using var stream = inFile.OpenRead();
        using var outStream = outFile.OpenWrite();

        var graph = serializer.Deserialize(stream);
        var parcelGraph = parcelLockerGraphBuilder.CreateParcelLockerGraph(graph);

        var parcelLockers = parcelGraph.Count();
        Console.WriteLine($"parcel lockers: {parcelLockers}");
        foreach (var node in parcelGraph)
        {
            if (node.ConnectedNodes.Count != parcelLockers)
            {
                Console.WriteLine($"warning: parcel locker {node.Id} has {node.ConnectedNodes.Count} connections");
            }
        }


        serializer.Serialize(outStream, parcelGraph.Cast<GraphNode>().ToList());
    }

    private static void RunOptimizer(FileInfo inFile, FileInfo outFile)
    {
        var serializer = new GraphSerializer();
        using var stream = inFile.OpenRead();
        var graph = serializer.Deserialize(stream);

        var runner = new ProgramRunner(graph, LoggerFactory.Create(builder => builder.AddConsole()));
        var best = runner.Run();
        using var outStream = outFile.OpenWrite();
        serializer.SerializePath(outStream, best);
    }

    private static void Visualize(FileInfo fileInfo, FileInfo? original, FileInfo? cycle)
    {
        var serializer = new GraphSerializer();
        var parcelLockerGraphBuilder = new ParcelLockerGraphBuilder();
        var builder = WebApplication.CreateBuilder();
        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        var app = builder.Build();
        app.MapGet("/graph.json", async (context) =>
        {
            context.Response.Headers.ContentEncoding = "gzip";
            await Results.File(
                fileInfo.OpenRead(), contentType: "application/json"
            ).ExecuteAsync(context);
        });
        app.MapGet("/original.json", async (context) =>
        {
            if (original is { } file)
            {
                context.Response.Headers.ContentEncoding = "gzip";
                await Results.File(
                    file.OpenRead(), contentType: "application/json"
                ).ExecuteAsync(context);
            }
            else
            {
                await Results.NotFound().ExecuteAsync(context);
            }
        });
        app.MapGet("/path.json", async (context) =>
        {
            if (cycle is { } file)
            {
                var graph = serializer.Deserialize(fileInfo.OpenRead());
                var deserializedPath = serializer.DeserializePath(file.OpenRead(), graph);
                var (_, minNodeIndex) = deserializedPath.Select((node, i) => (node, i)).MinBy(x => x.node.Id);
                var orderedPath = deserializedPath.Skip(minNodeIndex).Concat(deserializedPath.Take(minNodeIndex))
                    .ToList();
                var expanded = parcelLockerGraphBuilder.ExpandPath(orderedPath, graph);
                MemoryStream result = new();
                serializer.SerializePath(result, expanded);
                context.Response.Headers.ContentEncoding = "gzip";

                await Results.Bytes(
                    result.GetBuffer(), contentType: "application/json"
                ).ExecuteAsync(context);
            }
            else
            {
                await Results.NotFound().ExecuteAsync(context);
            }
        });
        app.MapGet("/",
            () => Results.File(
                Path.Join(AppDomain.CurrentDomain.BaseDirectory, "visualization.html"),
                contentType: "text/html"
            )
        );
        app.MapGet("/style.json",
            () => Results.File(
                Path.Join(AppDomain.CurrentDomain.BaseDirectory, "style.json"),
                contentType: "application/json"
            )
        );
        app.Start();
        var url = app.Urls.First();
        Console.WriteLine($"Listening on {app.Urls.First()}");
        OpenBrowser(url);
        app.WaitForShutdown();
    }

    private static void OpenBrowser(string url)
    {
        ProcessStartInfo processStartInfo;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            processStartInfo = new ProcessStartInfo(url) { UseShellExecute = true };
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            processStartInfo = new ProcessStartInfo("xdg-open", url);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            processStartInfo = new ProcessStartInfo("open", url);
        }
        else
        {
            Console.WriteLine("Unable to open browser: platform not supported");
            return;
        }

        Process.Start(processStartInfo);
    }
}