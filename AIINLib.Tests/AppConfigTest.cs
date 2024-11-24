

namespace AIINLib.Test;

[TestFixture]
public class AppConfigTest
{
    [TestFixture]
    public class AppConfigTests
    {
        [SetUp]
        public void Setup()
        {
            File.WriteAllText("appsettings.json", @"
            {
                ""geneticAlgorithmSettings"": {
                    ""MutationRate"": 0.05,
                    ""CrossoverRate"": 0.7,
                    ""PopulationSize"": 200,
                    ""TournamentSize"": 5,
                    ""MaxGenerations"": 1000
                }
            }");
        }

        [TearDown]
        public void Teardown()
        {
            if (File.Exists("appsettings.json"))
            {
                File.Delete("appsettings.json");
            }
        }

        [Test]
        public void ShouldLoadGeneticAlgorithmSettingsFromConfig()
        {
            Assert.That(AppConfig.GeneticAlgorithmSettings.MutationRate, Is.EqualTo(0.05));
            Assert.That(AppConfig.GeneticAlgorithmSettings.CrossoverRate, Is.EqualTo(0.7));
            Assert.That(AppConfig.GeneticAlgorithmSettings.PopulationSize, Is.EqualTo(200));
            Assert.That(AppConfig.GeneticAlgorithmSettings.TournamentSize, Is.EqualTo(5));
            Assert.That(AppConfig.GeneticAlgorithmSettings.MaxGenerations, Is.EqualTo(1000));
        }

        [Test]
        public void ShouldThrowExceptionIfConfigIsMissing()
        {
            if (File.Exists("appsettings.json"))
            {
                File.Delete("appsettings.json");
            }

            Assert.That(() =>
            {
                var errorProvider = AppConfig.GeneticAlgorithmSettings;
            }, Throws.Exception);
        }
    }
}