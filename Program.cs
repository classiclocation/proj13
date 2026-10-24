var urls = new[]
{
    "https://example.com/",
    "https://example.org/",
    "https://example.net/"
};

const int maxParallelDownloads = 3;

using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(30));

Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellation.Cancel();
    Console.WriteLine("\nCancellation requested");
};

using var client = new HttpClient();

Console.WriteLine($"Downloading {urls.Length} URLs (max {maxParallelDownloads} at once). Press Ctrl+C to cancel");

var options = new ParallelOptions
{
    MaxDegreeOfParallelism = maxParallelDownloads,
    CancellationToken = cancellation.Token
};

try
{
    await Parallel.ForEachAsync(urls, options, async (url, token) =>
    {
        Console.WriteLine($"Starting: {url}");

        using var response = await client.GetAsync(url, token);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(token);
        Console.WriteLine($"Done: {url} ({content.Length:N0} characters)");
    });

    Console.WriteLine("All downloads completed.");
}
catch (OperationCanceledException)
{
    Console.WriteLine("Download cancelled or timed out.");
}
catch (Exception exception)
{
    Console.WriteLine($"Download error: {exception.Message}");
}