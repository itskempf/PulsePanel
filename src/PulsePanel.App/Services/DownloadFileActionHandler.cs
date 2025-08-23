using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PulsePanel.App.Services
{
    public class DownloadFileActionHandler : IActionHandler
    {
        public async Task ExecuteAsync(System.Collections.Generic.Dictionary<string, string> parameters, CancellationToken ct = default)
        {
            if (!parameters.TryGetValue("Url", out var url) || !parameters.TryGetValue("Destination", out var dest))
                throw new ArgumentException("DownloadFile requires Url and Destination parameters");

            using var client = new HttpClient();
            var data = await client.GetByteArrayAsync(url);
            Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
            await File.WriteAllBytesAsync(dest, data, ct);
        }
    }
}