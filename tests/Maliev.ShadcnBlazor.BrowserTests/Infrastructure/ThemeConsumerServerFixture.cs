using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Maliev.ShadcnBlazor.BrowserTests.Infrastructure;

public sealed class ThemeConsumerServerFixture : IAsyncLifetime
{
    private Process? _process;
    private Task? _stdout;
    private Task? _stderr;
    private readonly BoundedDiagnostics _diagnostics = new(16 * 1024);

    public Uri BaseUri { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var root = FindRoot();
        var project = Path.Combine(root, "samples", "Maliev.ShadcnBlazor.ThemeConsumer", "Maliev.ShadcnBlazor.ThemeConsumer.csproj");
        BaseUri = SelectBaseUri();
        _process = Process.Start(new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = root,
            Arguments = $"run --project \"{project}\" -c Release --no-build --urls {BaseUri}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        }) ?? throw new InvalidOperationException("Could not start the theme consumer host.");
        _stdout = DrainAsync(_process.StandardOutput);
        _stderr = DrainAsync(_process.StandardError);
        await WaitForReadinessAsync();
    }

    public async Task DisposeAsync()
    {
        if (_process is null)
            return;
        try
        {
            if (!_process.HasExited)
                _process.Kill(entireProcessTree: true);
            await _process.WaitForExitAsync();
            if (_stdout is not null && _stderr is not null)
                await Task.WhenAll(_stdout, _stderr);
        }
        finally
        {
            _process.Dispose();
            _process = null;
        }
    }

    private async Task WaitForReadinessAsync()
    {
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
        var deadline = DateTime.UtcNow.AddSeconds(60);
        while (DateTime.UtcNow < deadline)
        {
            if (_process is { HasExited: true })
                throw new InvalidOperationException($"Theme consumer host exited. {_diagnostics}");
            try
            {
                using var response = await http.GetAsync(BaseUri);
                if (response.IsSuccessStatusCode)
                    return;
            }
            catch (HttpRequestException) { }
            catch (TaskCanceledException) { }
            await Task.Delay(250);
        }
        throw new TimeoutException($"Theme consumer did not become ready at {BaseUri}. {_diagnostics}");
    }

    private async Task DrainAsync(StreamReader reader)
    {
        var buffer = new char[4096];
        int read;
        while ((read = await reader.ReadAsync(buffer.AsMemory())) > 0)
            _diagnostics.Append(buffer.AsSpan(0, read));
    }

    private static Uri SelectBaseUri()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return new Uri($"http://127.0.0.1:{port}");
    }

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException();
    }
}
