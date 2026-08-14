namespace Maliev.ShadcnBlazor.Components.Feedback.Toast;

/// <summary>Creates and controls the application toast queue.</summary>
public interface IShadcnToastService
{
    IReadOnlyList<ShadcnToastItem> Items { get; }
    event Action? Changed;
    string Show(ShadcnToastOptions options);
    bool Update(string id, ShadcnToastOptions options);
    bool Dismiss(string id);
    bool BeginDismiss(string id, TimeSpan exitDuration);
    bool Pause(string id, string reason = "consumer");
    bool Resume(string id, string reason = "consumer");
    Task<bool> InvokeActionAsync(string id);
    Task<string> PromiseAsync<T>(Task<T> operation, ShadcnToastOptions loading, Func<T, ShadcnToastOptions> success, Func<Exception, ShadcnToastOptions> error);
    Task<T> PromiseResultAsync<T>(Task<T> operation, ShadcnToastOptions loading, Func<T, ShadcnToastOptions> success, Func<Exception, ShadcnToastOptions> error);
    void Clear();
}
