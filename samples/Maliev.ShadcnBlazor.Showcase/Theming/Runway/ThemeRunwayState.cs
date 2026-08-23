using System.Threading;

namespace Maliev.ShadcnBlazor.Showcase.Theming.Runway;

public sealed record ThemeDemonstrationFrame(
    long Tick,
    double CapacityPercent,
    double UploadPercent,
    int FormStep,
    int ChatCharacters,
    bool ToastVisible,
    int ApprovalState);

public sealed class ThemeRunwayState : IAsyncDisposable
{
    private readonly PeriodicTimer timer = new(TimeSpan.FromMilliseconds(900));
    private readonly CancellationTokenSource cancellation = new();
    private Task? loop;
    private long tick;

    public ThemeDemonstrationFrame Frame => CreateFrame(tick, ReducedMotion);
    public bool PersistentPaused { get; private set; }
    public bool InteractionPaused { get; private set; }
    public bool ReducedMotion { get; private set; }
    public bool IsPaused => PersistentPaused || InteractionPaused || ReducedMotion;
    public event EventHandler? Changed;

    public void Start() => loop ??= RunAsync();

    public void SetPersistentPaused(bool paused)
    {
        if (PersistentPaused == paused) return;
        PersistentPaused = paused;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void SetInteractionPaused(bool paused)
    {
        if (InteractionPaused == paused) return;
        InteractionPaused = paused;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void SetReducedMotion(bool reduced)
    {
        if (ReducedMotion == reduced) return;
        ReducedMotion = reduced;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void AdvanceForTest()
    {
        if (!IsPaused) tick++;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void Reset()
    {
        tick = 0;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private async Task RunAsync()
    {
        try
        {
            while (await timer.WaitForNextTickAsync(cancellation.Token))
                AdvanceForTest();
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { }
    }

    private static ThemeDemonstrationFrame CreateFrame(long value, bool reduced) => reduced
        ? new(0, 82, 68, 2, 74, true, 1)
        : new(value, 58 + value % 37, (value * 13) % 101, (int)(value / 3 % 4), (int)Math.Min(104, (value * 7) % 112), value % 9 >= 6, (int)(value / 5 % 3));

    public async ValueTask DisposeAsync()
    {
        cancellation.Cancel();
        timer.Dispose();
        if (loop is not null)
            try { await loop; } catch (OperationCanceledException) { }
        cancellation.Dispose();
    }
}

