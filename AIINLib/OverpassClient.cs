using OsmSharp.Streams;

namespace AIINLib;

public class OverpassClient(string url)
{
    private readonly HttpClient _httpClient = new();

    public async Task<OsmStreamSource> Fetch(string query)
    {
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "data", query }
        });
        var response = await _httpClient.PostAsync(url, content);
        response.EnsureSuccessStatusCode();
        return new XmlOsmStreamSource(await response.Content.ReadAsStreamAsync());
    }
}