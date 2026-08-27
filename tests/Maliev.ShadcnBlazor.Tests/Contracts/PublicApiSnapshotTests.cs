using System.Reflection;
using System.Runtime.CompilerServices;
using Maliev.ShadcnBlazor.Components.Primitives;
using Maliev.ShadcnBlazor.Components.Selection;
using Microsoft.AspNetCore.Components;

namespace Maliev.ShadcnBlazor.Tests.Contracts;

public sealed class PublicApiSnapshotTests
{
    private const string ThemingNamespace = "Maliev.ShadcnBlazor.Theming";
    private const string IconsNamespace = "Maliev.ShadcnBlazor.Components.Icons";

    private static readonly string[] OwnedNamespaces =
    [
        "Maliev.ShadcnBlazor.Components",
        "Maliev.ShadcnBlazor.Components.Actions",
        "Maliev.ShadcnBlazor.Components.Content",
        "Maliev.ShadcnBlazor.Components.Conversation",
        "Maliev.ShadcnBlazor.Components.DataDisplay",
        "Maliev.ShadcnBlazor.Components.Disclosure",
        "Maliev.ShadcnBlazor.Components.Direction",
        "Maliev.ShadcnBlazor.Components.Forms",
        "Maliev.ShadcnBlazor.Components.Icons",
        "Maliev.ShadcnBlazor.Components.Feedback",
        "Maliev.ShadcnBlazor.Components.Feedback.Toast",
        "Maliev.ShadcnBlazor.Components.Layout",
        "Maliev.ShadcnBlazor.Components.Navigation",
        "Maliev.ShadcnBlazor.Components.Navigation.Sidebar",
        "Maliev.ShadcnBlazor.Components.Overlays",
        "Maliev.ShadcnBlazor.Components.Primitives",
        "Maliev.ShadcnBlazor.Components.Selection",
        "Maliev.ShadcnBlazor.Components.Styling",
        "Maliev.ShadcnBlazor.Components.Typography"
    ];

    [Fact]
    public void SemanticFoundationPublicApiMatchesApprovedSnapshot()
    {
        var actual = BuildSnapshot();
        var snapshot = FindSnapshot();
        if (Environment.GetEnvironmentVariable("SHADCN_UPDATE_PUBLIC_API") == "1")
            File.WriteAllText(snapshot, actual);
        if (!File.Exists(snapshot))
            throw new InvalidOperationException(actual);
        var expected = File.ReadAllText(snapshot).Replace("\r\n", "\n", StringComparison.Ordinal);
        Assert.Equal(expected, actual);
    }

    private static string BuildSnapshot()
    {
        var lines = new List<string>();
        var types = typeof(ShadcnComponentBase).Assembly.GetExportedTypes()
            .Where(type => type.Namespace is not null &&
                           (OwnedNamespaces.Contains(type.Namespace, StringComparer.Ordinal) ||
                            string.Equals(type.Namespace, ThemingNamespace, StringComparison.Ordinal)))
            .OrderBy(type => type.FullName, StringComparer.Ordinal);

        foreach (var type in types)
        {
            if (type.IsEnum)
            {
                lines.Add($"enum {type.FullName}: {string.Join(", ", Enum.GetNames(type))}");
                continue;
            }

            lines.Add($"type {type.FullName}");
            var isThemeApi = string.Equals(type.Namespace, ThemingNamespace, StringComparison.Ordinal);
            var isContractApi = isThemeApi || string.Equals(type.Namespace, IconsNamespace, StringComparison.Ordinal);
            if (isContractApi)
            {
                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                             .Where(field => field.IsLiteral)
                             .OrderBy(field => field.Name, StringComparer.Ordinal))
                {
                    lines.Add($"  const {FriendlyName(field.FieldType)} {field.Name}");
                }
            }

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                         .Concat(isContractApi
                             ? type.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                             : [])
                         .Where(property => isContractApi || type == typeof(ShadcnSliderThumbAttributes) ||
                                            property.GetCustomAttribute<ParameterAttribute>() is not null ||
                                            type.IsValueType)
                         .OrderBy(property => property.Name, StringComparer.Ordinal))
            {
                var parameter = property.GetCustomAttribute<ParameterAttribute>();
                var suffix = parameter?.CaptureUnmatchedValues == true ? " [CaptureUnmatchedValues]" : string.Empty;
                lines.Add($"  {FriendlyName(property.PropertyType)} {property.Name}{suffix}");
            }

            if (isContractApi)
            {
                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                             .Where(method => !method.IsSpecialName &&
                                              method.GetCustomAttribute<CompilerGeneratedAttribute>() is null)
                             .OrderBy(method => method.Name, StringComparer.Ordinal)
                             .ThenBy(method => string.Join(',', method.GetParameters().Select(parameter => parameter.ParameterType.FullName)), StringComparer.Ordinal))
                {
                    var parameters = string.Join(", ", method.GetParameters()
                        .Select(parameter => $"{FriendlyName(parameter.ParameterType)} {parameter.Name}"));
                    lines.Add($"  method {FriendlyName(method.ReturnType)} {method.Name}({parameters})");
                }
            }
        }

        return string.Join('\n', lines) + "\n";
    }

    private static string FriendlyName(Type type)
    {
        if (type.IsGenericType)
        {
            var name = type.GetGenericTypeDefinition().Name.Split('`')[0];
            return $"{name}<{string.Join(", ", type.GetGenericArguments().Select(FriendlyName))}>";
        }

        if (type.IsArray)
            return $"{FriendlyName(type.GetElementType()!)}[]";

        return type.Name;
    }

    private static string FindSnapshot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx")))
            directory = directory.Parent;
        return Path.Combine(directory!.FullName, "tests", "Maliev.ShadcnBlazor.Tests", "Contracts", "public-api.txt");
    }
}
