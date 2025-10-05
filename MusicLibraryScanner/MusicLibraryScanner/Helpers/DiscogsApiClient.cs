// Helpers/DiscogsApiClient.cs

using System.Net.Http.Headers;
using System.Text.Json;

namespace MusicLibraryScanner.Helpers {
    public class DiscogsApiClient {

        //TODO: Refactor: use var, invert if to reduce nesting
        //TODO: Reformat consistent with rest of project

        private readonly HttpClient _httpClient;
        private readonly int _minDelayMs = 1100; // ~1 request/sec to stay safe with rate limits

        public DiscogsApiClient(string personalAccessToken, string userAgent) {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://api.discogs.com/")
            };

            // User-Agent from config
            _httpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);

            // Token
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Discogs token={personalAccessToken}");
        }

        /// <summary>
        /// Look up a release by artist + album title (+ optional year).
        /// Handles rate limiting and retries.
        /// </summary>
        public async Task<int?> FindReleaseIdAsync(string artist, string album, int? year = null) {
            var url = $"https://api.discogs.com/database/search?artist={Uri.EscapeDataString(artist)}&release_title={Uri.EscapeDataString(album)}&type=release";
            if (year.HasValue) {
                url += $"&year={year.Value}";
            }

            int retries = 3;
            int delay = _minDelayMs;

            for (int attempt = 1; attempt <= retries; attempt++) {
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode) {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);

                    if (!doc.RootElement.TryGetProperty("results", out var results) || results.GetArrayLength() == 0) {
                        return null;
                    }

                    // Try exact year match first
                    if (year.HasValue) {
                        foreach (var result in results.EnumerateArray()) {
                            if (result.TryGetProperty("year", out var resultYear) && resultYear.GetInt32() == year.Value) {
                                if (result.TryGetProperty("id", out var idElement) && idElement.TryGetInt32(out var releaseId)) {
                                    await Task.Delay(_minDelayMs); // respect rate limit
                                    return releaseId;
                                }
                            }
                        }
                    }

                    // Otherwise return the first result
                    var first = results[0];
                    if (first.TryGetProperty("id", out var firstId) && firstId.TryGetInt32(out var fallbackId)) {
                        await Task.Delay(_minDelayMs); // respect rate limit
                        return fallbackId;
                    }

                    return null;
                }

                // Handle rate limit explicitly
                if ((int)response.StatusCode == 429) {
                    await Task.Delay(delay);
                    delay *= 2; // exponential backoff
                    continue;
                }

                // On other failures, don’t retry
                return null;
            }

            return null;
        }
    }
}
