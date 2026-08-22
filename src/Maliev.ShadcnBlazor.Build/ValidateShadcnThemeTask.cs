using Microsoft.Build.Framework;

namespace Maliev.ShadcnBlazor.Build;

/// <summary>Validates portable MALIEV Shadcn theme documents during a consumer build.</summary>
public sealed class ValidateShadcnThemeTask : Microsoft.Build.Utilities.Task
{
    /// <summary>Gets or sets the theme files to validate.</summary>
    [Required]
    public ITaskItem[] ThemeFiles { get; set; } = [];

    /// <summary>Gets or sets whether recoverable diagnostics should fail the build.</summary>
    public bool WarningsAsErrors { get; set; }

    /// <inheritdoc />
    public override bool Execute()
    {
        foreach (var item in ThemeFiles)
        {
            var file = item.GetMetadata("FullPath");
            if (string.IsNullOrWhiteSpace(file))
                file = item.ItemSpec;

            foreach (var diagnostic in ThemeDocumentBuildValidator.Validate(file))
            {
                var message = $"[{diagnostic.Path}] {diagnostic.Message}";
                if (diagnostic.IsWarning && !WarningsAsErrors)
                {
                    Log.LogWarning(null, diagnostic.Code, null, file, diagnostic.Line, diagnostic.Column,
                        0, 0, message);
                }
                else
                {
                    Log.LogError(null, diagnostic.Code, null, file, diagnostic.Line, diagnostic.Column,
                        0, 0, message);
                }
            }
        }

        return !Log.HasLoggedErrors;
    }
}
