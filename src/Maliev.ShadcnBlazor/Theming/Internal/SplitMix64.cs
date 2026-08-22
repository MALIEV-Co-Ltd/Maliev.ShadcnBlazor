namespace Maliev.ShadcnBlazor.Theming.Internal;

internal struct SplitMix64(ulong seed)
{
    private ulong _state = seed;

    internal ulong NextUInt64()
    {
        _state += 0x9e3779b97f4a7c15UL;
        var value = _state;
        value = (value ^ (value >> 30)) * 0xbf58476d1ce4e5b9UL;
        value = (value ^ (value >> 27)) * 0x94d049bb133111ebUL;
        return value ^ (value >> 31);
    }

    internal double NextUnitDouble() => (NextUInt64() >> 11) * (1d / 9007199254740992d);
}
