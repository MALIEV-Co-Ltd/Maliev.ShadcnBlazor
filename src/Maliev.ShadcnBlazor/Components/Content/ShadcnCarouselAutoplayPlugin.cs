namespace Maliev.ShadcnBlazor.Components.Content;

/// <summary>Advances a carousel on a deterministic <see cref="TimeProvider"/> schedule.</summary>
public sealed class ShadcnCarouselAutoplayPlugin : IShadcnCarouselPlugin
{
    private readonly TimeProvider _timeProvider;
    private readonly TimeSpan _delay;
    private readonly bool _stopOnInteraction;
    private readonly object _gate = new();
    private IShadcnCarouselApi? _api;
    private ITimer? _timer;
    private bool _advancing;
    private long _generation;
    public event Action<Exception>? Failed;

    public ShadcnCarouselAutoplayPlugin(TimeProvider timeProvider, TimeSpan? delay = null, bool stopOnInteraction = true)
    {
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _delay = delay ?? TimeSpan.FromSeconds(4);
        if (_delay <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(delay));
        _stopOnInteraction = stopOnInteraction;
    }

    public void Initialize(IShadcnCarouselApi api) { lock (_gate) { _api = api ?? throw new ArgumentNullException(nameof(api)); ArmLocked(); } }
    public void Selected(int index)
    {
        lock (_gate)
        {
            if (_advancing) return;
            if (_stopOnInteraction) { _generation++; _timer?.Dispose(); _timer = null; }
            else ArmLocked();
        }
    }
    public ValueTask DisposeAsync() { lock (_gate) { _generation++; _timer?.Dispose(); _timer = null; _api = null; } return ValueTask.CompletedTask; }
    private void ArmLocked()
    {
        _timer?.Dispose();
        var generation = ++_generation;
        _timer = _timeProvider.CreateTimer(_ => AdvanceObserved(generation), null, _delay, Timeout.InfiniteTimeSpan);
    }
    private void AdvanceObserved(long generation)
    {
        try { AdvanceAsync(generation).GetAwaiter().GetResult(); }
        catch (Exception exception) { Failed?.Invoke(exception); }
    }
    private async Task AdvanceAsync(long generation)
    {
        IShadcnCarouselApi? api;
        lock (_gate) { if (_api is null || generation != _generation) return; api = _api; _advancing = true; }
        try { await api.DispatchAsync(api.NextAsync); }
        finally { lock (_gate) { _advancing = false; if (_api is not null && generation == _generation) ArmLocked(); } }
    }
}
