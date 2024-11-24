using Microsoft.Extensions.Configuration;

namespace AIINLib;
public class GeneticAlgorithmSettings
{
    public double MutationRate { get; init; }
    public double CrossoverRate { get; init; }
    
    public int PopulationSize { get; init; }
    public int TournamentSize { get; init; }
    public int MaxGenerations { get; init; }
}

public static class AppConfig
{
    public static GeneticAlgorithmSettings GeneticAlgorithmSettings { get; private set; }

    static AppConfig()
    {
        try
        {
            IConfigurationBuilder configurationBuilder = 
                new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            IConfigurationRoot configuration = configurationBuilder.Build();

            GeneticAlgorithmSettings = configuration.GetRequiredSection("geneticAlgorithmSettings")
                .Get<GeneticAlgorithmSettings>() ?? throw new InvalidOperationException("Can not read section 'GeneticAlgorithmSettings'");
        }
        catch (Exception e)
        {
            Console.WriteLine("Error while reading appsettings.json");
            throw;
        }
    }
}