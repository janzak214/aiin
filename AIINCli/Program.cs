using System.CommandLine;
using AIINLib;
using Geo;
using Geo.Geodesy;
using OsmSharp.Streams;

namespace AIINCli;

internal static class Program
{
    private const string OverpassUrl = "https://overpass-api.de/api/interpreter";

    private static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("AIIN CLI");
        var bboxOption = new Option<string>(
            name: "--bbox",
            description: "Bounding box",
            getDefaultValue: () => "49.98,19.84,50.12,20.03"
        );

        var fileArgument = new Argument<FileInfo>("file", "Output file");
        var fetchDataCommand = new Command("fetch", "Fetch data from Overpass API for the given bounding box")
        {
            bboxOption,
            fileArgument
        };

        var inputFileArgument = new Argument<FileInfo>("file", "Input file");
        var analyzeCommand = new Command("analyze", "Analyze the downloaded data")
        {
            inputFileArgument
        };

        fetchDataCommand.SetHandler(FetchData, bboxOption, fileArgument);
        analyzeCommand.SetHandler(Analyze, inputFileArgument);
        rootCommand.AddCommand(fetchDataCommand);
        rootCommand.AddCommand(analyzeCommand);

        return await rootCommand.InvokeAsync(args);
    }

    private static async Task FetchData(string bbox, FileInfo fileInfo)
    {
        var client = new OverpassClient(OverpassUrl);
        using var response = await client.Fetch(
            $"""
             [timeout:25][bbox:{bbox}];

             node
                [amenity=parcel_locker]
                [operator=InPost]
                ->.parcel_lockers;

             way
                [highway~"^(motorway|motorway_link|trunk|trunk_link|primary|primary_link|secondary|secondary_link|tertiary|tertiary_link|unclassified|road|residential|track|service|living_street)$"]
                (if: 
                    (!is_tag("access") || t["access"] == "yes")
                    && (!is_tag("vehicle") || t["vehicle"] == "yes")
                    && (!is_tag("motor_vehicle") || t["motor_vehicle"] == "yes")
                    && (!is_tag("motor_car") || t["motor_car"] == "yes")
                )
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

    private static void Analyze(FileInfo fileInfo)
    {
        using var file = fileInfo.OpenRead();
        var source = new XmlOsmStreamSource(file);
        var (parcelLockers, roadNodes, roads) = Preprocessing.SplitByType(source);

        Console.WriteLine($"parcel lockers: {parcelLockers.Count}");
        Console.WriteLine($"road nodes: {roadNodes.Count}");
        var oneWayCount = roads.Count(x => x.OneWay);
        Console.WriteLine($"two-way roads: {roads.Count - oneWayCount}");
        Console.WriteLine($"one-way roads: {oneWayCount}");
        Console.WriteLine();
        Console.WriteLine("closest node for each parcel locker:");

        var watch = System.Diagnostics.Stopwatch.StartNew();
        var calculator = new SpheroidCalculator();
        var closest = parcelLockers
            .AsParallel()
            .Select(item =>
            {
                var (node, distance) = roadNodes
                    .Select(x => (
                        Node: x,
                        Distance: calculator.CalculateLength(
                            new CoordinateSequence([item.Position, x.Position])
                        )
                    ))
                    .MinBy(x => x.Distance);

                return (item, node, distance);
            }).ToList();
        watch.Stop();

        foreach (var x in closest)
        {
            Console.WriteLine(x);
        }

        Console.WriteLine($"took {watch.ElapsedMilliseconds / 1000}s");
    }
}