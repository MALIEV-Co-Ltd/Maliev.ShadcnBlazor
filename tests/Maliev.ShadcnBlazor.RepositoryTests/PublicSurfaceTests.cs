using System.Diagnostics;

namespace Maliev.ShadcnBlazor.RepositoryTests;

public sealed class PublicSurfaceTests
{
    [Fact]
    public void TrackedFilesContainNoPrivateIdentifiers()
    {
        var root = RepositoryRoot.Find();
        var script = Path.Combine(root, "eng", "Verify-PublicSurface.ps1");

        var start = new ProcessStartInfo("pwsh")
        {
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
        };
        start.ArgumentList.Add("-NoProfile");
        start.ArgumentList.Add("-File");
        start.ArgumentList.Add(script);
        start.ArgumentList.Add("-Root");
        start.ArgumentList.Add(root);

        using var process = Process.Start(start)!;
        process.WaitForExit();

        var output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
        Assert.True(process.ExitCode == 0, output);
    }

}
