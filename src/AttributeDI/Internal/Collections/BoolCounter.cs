using System.Runtime.InteropServices;

namespace AttributeDI.Internal.Collections;

/// <summary>
/// A ref struct that keeps track of the number of <see langword="true"/> boolean values that have been set.
/// </summary>
[StructLayout(LayoutKind.Auto)]
internal ref struct BoolCounter
{
    private int _count;
    private readonly Span<bool> _counted;

    /// <summary>
    /// The number of <see langword="true"/> boolean values that have been set.
    /// </summary>
    internal readonly int Count => _count;

    internal BoolCounter(Span<bool> counted)
    {
        _counted = counted;
        _count = 0;
    }

    private readonly bool IndexHasFlag(in int flag)
    {
        return _counted[flag];
    }

    internal bool MarkFlag(int flag, bool value)
    {
        return value && this.MarkFlag(in flag);
    }
    internal bool MarkFlag(in int flag)
    {
        bool result = false;

        if (!this.IndexHasFlag(in flag))
        {
            result = true;
            _counted[flag] = result;
            _count++;
        }

        return result;
    }

    internal readonly bool MoveNext()
    {
        return _count < _counted.Length;
    }
}
