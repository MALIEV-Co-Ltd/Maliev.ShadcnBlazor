using System.Globalization;

namespace Maliev.ShadcnBlazor.Components.Forms;

/// <summary>Provides display state to composed OTP slots.</summary>
public sealed class ShadcnInputOtpContext
{
    internal string Value { get; private set; } = string.Empty;
    internal int? ActiveIndex { get; private set; }
    internal void Update(string value, int? activeIndex) { Value = value; ActiveIndex = activeIndex; }
    internal string CharacterAt(int index)
    {
        var positions = StringInfo.ParseCombiningCharacters(Value);
        if (index < 0 || index >= positions.Length) return string.Empty;
        var end = index + 1 < positions.Length ? positions[index + 1] : Value.Length;
        return Value[positions[index]..end];
    }
}
