using OsmSharp.Streams;

namespace AIINLib;

public class OverpassClient
{
    private readonly HttpClient _httpClient = new();
    private readonly string _url;

    /// <summary>
    /// Initializes a new instance of the <see cref="OverpassClient"/> class with the specified URL.
    /// </summary>
    /// <param name="url">The URL of the Overpass API endpoint.</param>
    public OverpassClient(string url)
    {
        _url = url;
    }

    /// <summary>
    /// Fetches data from the Overpass API using the specified query.
    /// </summary>
    /// <param name="query">The Overpass API query to execute.</param>
    /// <returns>An <see cref="OsmStreamSource"/> containing the fetched data.</returns>
    /// <exception cref="HttpRequestException">Thrown when the HTTP request fails.</exception>
    public async Task<OsmStreamSource> Fetch(string query)
    {
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "data", query }
        });
        var response = await _httpClient.PostAsync(_url, content);
        response.EnsureSuccessStatusCode();
        return new XmlOsmStreamSource(await response.Content.ReadAsStreamAsync());
    }
}