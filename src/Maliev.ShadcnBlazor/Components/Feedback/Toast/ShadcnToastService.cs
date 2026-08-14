namespace Maliev.ShadcnBlazor.Components.Feedback.Toast;

/// <summary>A deterministic, <see cref="TimeProvider"/>-backed toast manager.</summary>
public sealed class ShadcnToastService(TimeProvider timeProvider) : IShadcnToastService, IDisposable
{
    private readonly TimeSpan? _defaultDuration = TimeSpan.FromSeconds(5);
    private readonly TimeSpan _exitDuration;
    private readonly object _gate = new();
    private readonly List<Entry> _entries = [];
    private long _sequence;
    private long _generation;
    private bool _disposed;
    private bool _globallyPaused;
    public ShadcnToastService(TimeProvider timeProvider, TimeSpan? defaultDuration, TimeSpan? exitDuration = null) : this(timeProvider)
    {
        if (defaultDuration is { } value && value < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(defaultDuration));
        if (exitDuration is { } exit && exit < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(exitDuration));
        _defaultDuration = defaultDuration == TimeSpan.Zero ? null : defaultDuration ?? TimeSpan.FromSeconds(5);
        _exitDuration = exitDuration ?? TimeSpan.Zero;
    }

    public event Action? Changed;
    public IReadOnlyList<ShadcnToastItem> Items { get { lock (_gate) return _entries.OrderByDescending(entry => entry.Item.Priority).ThenBy(entry => entry.Sequence).Select(entry => entry.Item).ToArray(); } }

    public string Show(ShadcnToastOptions options)
    {
        Validate(options);
        var id = Interlocked.Increment(ref _sequence).ToString(System.Globalization.CultureInfo.InvariantCulture);
        lock (_gate)
        {
            ThrowIfDisposed();
            var entry = CreateEntry(id, options, _sequence, Interlocked.Increment(ref _generation));
            if (_globallyPaused) entry.PauseReasons.Add("document");
            _entries.Add(entry);
            if (entry.PauseReasons.Count == 0) Arm(entry, entry.Remaining);
        }
        Notify();
        return id;
    }

    public bool Update(string id, ShadcnToastOptions options)
    {
        Validate(options);
        lock (_gate)
        {
            var index = _entries.FindIndex(entry => entry.Item.Id == id);
            if (index < 0) return false;
            _entries[index].Timer?.Dispose();
            var replacement = CreateEntry(id, options, _entries[index].Sequence, Interlocked.Increment(ref _generation));
            replacement.PauseReasons.UnionWith(_entries[index].PauseReasons);
            _entries[index] = replacement;
            if (replacement.PauseReasons.Count == 0) Arm(replacement, replacement.Remaining);
        }
        Notify();
        return true;
    }

    public bool Dismiss(string id) => _exitDuration > TimeSpan.Zero ? BeginDismiss(id, _exitDuration) : DismissImmediately(id);

    private bool DismissImmediately(string id)
    {
        Action? dismissed;
        lock (_gate)
        {
            var index = _entries.FindIndex(entry => entry.Item.Id == id);
            if (index < 0) return false;
            var entry = _entries[index];
            _entries.RemoveAt(index);
            entry.Timer?.Dispose();
            dismissed = entry.Item.OnDismiss;
        }
        try { dismissed?.Invoke(); }
        finally { Notify(); }
        return true;
    }

    public bool BeginDismiss(string id, TimeSpan exitDuration)
    {
        if (exitDuration < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(exitDuration));
        if (exitDuration == TimeSpan.Zero) return DismissImmediately(id);
        bool changed;
        lock (_gate)
        {
            changed = BeginDismissLocked(id, expectedGeneration: null, exitDuration);
        }
        if (!changed) return false;
        Notify();
        return true;
    }

    public bool Pause(string id, string reason = "consumer")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        lock (_gate)
        {
            var entry = _entries.FirstOrDefault(item => item.Item.Id == id);
            if (entry is null || entry.Item.State == "closing") return false;
            if (!entry.PauseReasons.Add(reason)) return true;
            if (entry.Remaining is null || entry.Timer is null) return true;
            entry.Remaining -= timeProvider.GetElapsedTime(entry.ArmedAt);
            if (entry.Remaining < TimeSpan.Zero) entry.Remaining = TimeSpan.Zero;
            entry.Timer.Dispose(); entry.Timer = null;
        }
        return true;
    }

    public bool Resume(string id, string reason = "consumer")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        lock (_gate)
        {
            var entry = _entries.FirstOrDefault(item => item.Item.Id == id);
            if (entry is null || entry.Item.State == "closing" || !entry.PauseReasons.Remove(reason) || entry.Remaining is null) return false;
            if (entry.PauseReasons.Count > 0) return true;
            Arm(entry, entry.Remaining);
        }
        return true;
    }

    public async Task<bool> InvokeActionAsync(string id)
    {
        Func<Task>? action;
        lock (_gate)
        {
            var entry = _entries.FirstOrDefault(entry => entry.Item.Id == id);
            if (entry is null || entry.ActionStarted) return false;
            entry.ActionStarted = true;
            action = entry.Item.Action;
        }
        if (action is null) return false;
        try { await action(); }
        finally { Dismiss(id); }
        return true;
    }

    public async Task<string> PromiseAsync<T>(Task<T> operation, ShadcnToastOptions loading, Func<T, ShadcnToastOptions> success, Func<Exception, ShadcnToastOptions> error)
    {
        ArgumentNullException.ThrowIfNull(operation); ArgumentNullException.ThrowIfNull(success); ArgumentNullException.ThrowIfNull(error);
        var id = Show(loading with { Type = ShadcnToastType.Loading });
        try { var result = await operation; Update(id, success(result)); }
        catch (Exception exception) { Update(id, error(exception)); }
        return id;
    }

    public async Task<T> PromiseResultAsync<T>(Task<T> operation, ShadcnToastOptions loading, Func<T, ShadcnToastOptions> success, Func<Exception, ShadcnToastOptions> error)
    {
        ArgumentNullException.ThrowIfNull(operation); ArgumentNullException.ThrowIfNull(success); ArgumentNullException.ThrowIfNull(error);
        var id = Show(loading with { Type = ShadcnToastType.Loading });
        try { var result = await operation; Update(id, success(result)); return result; }
        catch (Exception exception) { Update(id, error(exception)); throw; }
    }

    public void Clear() { foreach (var id in Items.Select(item => item.Id).ToArray()) Dismiss(id); }
    public void PauseAll()
    {
        lock (_gate)
        {
            if (_globallyPaused) return;
            _globallyPaused = true;
            foreach (var entry in _entries) PauseLocked(entry, "document");
        }
    }
    public void ResumeAll()
    {
        lock (_gate)
        {
            if (!_globallyPaused) return;
            _globallyPaused = false;
            foreach (var entry in _entries) ResumeLocked(entry, "document");
        }
    }
    public void Dispose() { lock (_gate) { if (_disposed) return; _disposed = true; foreach (var entry in _entries) entry.Timer?.Dispose(); _entries.Clear(); } }

    private Entry CreateEntry(string id, ShadcnToastOptions options, long sequence, long generation)
    {
        var duration = options.Duration == TimeSpan.Zero ? null : options.Duration ?? (options.Type == ShadcnToastType.Loading ? null : _defaultDuration);
        var live = options.Urgent || options.Type == ShadcnToastType.Error ? "assertive" : "polite";
        return new Entry(new(id, options.Title, options.Description, options.Type, options.ActionLabel, options.Action, options.OnDismiss, live, duration, options.Priority, options.Icon), duration, sequence, generation);
    }
    private void Arm(Entry entry, TimeSpan? duration)
    {
        if (duration is null) return;
        entry.Remaining = duration;
        entry.ArmedAt = timeProvider.GetTimestamp();
        var generation = entry.Generation;
        entry.Timer = timeProvider.CreateTimer(_ => Expire(entry.Item.Id, generation), null, duration.Value, Timeout.InfiniteTimeSpan);
    }
    private void PauseLocked(Entry entry, string reason)
    {
        if (entry.Item.State == "closing" || !entry.PauseReasons.Add(reason) || entry.Remaining is null || entry.Timer is null) return;
        entry.Remaining -= timeProvider.GetElapsedTime(entry.ArmedAt);
        if (entry.Remaining < TimeSpan.Zero) entry.Remaining = TimeSpan.Zero;
        entry.Timer.Dispose();
        entry.Timer = null;
    }
    private void ResumeLocked(Entry entry, string reason)
    {
        if (entry.Item.State == "closing" || !entry.PauseReasons.Remove(reason) || entry.Remaining is null || entry.PauseReasons.Count > 0) return;
        Arm(entry, entry.Remaining);
    }
    private static void Validate(ShadcnToastOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(options.Title)) throw new ArgumentException("Toast title is required.", nameof(options));
        if (!Enum.IsDefined(options.Type)) throw new ArgumentOutOfRangeException(nameof(options.Type));
        if (!Enum.IsDefined(options.Priority)) throw new ArgumentOutOfRangeException(nameof(options.Priority));
        if (options.Duration is { } duration && duration < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(options.Duration));
        if (options.Action is not null && string.IsNullOrWhiteSpace(options.ActionLabel)) throw new ArgumentException("An action label is required when an action is supplied.", nameof(options));
    }
    private void Notify() => Changed?.Invoke();
    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);
    private void Expire(string id, long generation)
    {
        try
        {
            if (_exitDuration == TimeSpan.Zero) { RemoveLocked(id, generation, invokeDismissed: true); return; }
            bool changed;
            lock (_gate) changed = BeginDismissLocked(id, generation, _exitDuration);
            if (changed) Notify();
        }
        catch { /* Timer callbacks cannot surface consumer exceptions on the timer thread. */ }
    }
    private void RemoveClosing(string id, long generation)
    {
        try { RemoveLocked(id, generation, invokeDismissed: true); }
        catch { /* Timer callbacks cannot surface consumer exceptions on the timer thread. */ }
    }
    private bool BeginDismissLocked(string id, long? expectedGeneration, TimeSpan exitDuration)
    {
        var entry = _entries.FirstOrDefault(entry => entry.Item.Id == id);
        if (entry is null || entry.Item.State == "closing" || (expectedGeneration is not null && entry.Generation != expectedGeneration)) return false;
        entry.Timer?.Dispose();
        entry.Item = entry.Item with { State = "closing", Live = "off" };
        var generation = ++_generation;
        entry.Generation = generation;
        entry.Timer = timeProvider.CreateTimer(_ => RemoveClosing(id, generation), null, exitDuration, Timeout.InfiniteTimeSpan);
        return true;
    }
    private bool RemoveLocked(string id, long expectedGeneration, bool invokeDismissed)
    {
        Action? dismissed;
        lock (_gate)
        {
            var index = _entries.FindIndex(entry => entry.Item.Id == id && entry.Generation == expectedGeneration);
            if (index < 0) return false;
            var entry = _entries[index];
            _entries.RemoveAt(index);
            entry.Timer?.Dispose();
            dismissed = invokeDismissed ? entry.Item.OnDismiss : null;
        }
        try { dismissed?.Invoke(); }
        finally { Notify(); }
        return true;
    }
    private sealed class Entry(ShadcnToastItem item, TimeSpan? remaining, long sequence, long generation)
    {
        public ShadcnToastItem Item { get; set; } = item;
        public TimeSpan? Remaining { get; set; } = remaining;
        public long ArmedAt { get; set; }
        public long Sequence { get; } = sequence;
        public ITimer? Timer { get; set; }
        public HashSet<string> PauseReasons { get; } = new(StringComparer.Ordinal);
        public long Generation { get; set; } = generation;
        public bool ActionStarted { get; set; }
    }
}
