using Legacy89DiskKit.Domain.Native;
using System.Collections.Concurrent;
using Legacy89DiskKit.NativeInterop.Types;

namespace Legacy89DiskKit.NativeInterop.Core;

/// <summary>
/// Manages native disk sessions and maps them to integer handles.
/// </summary>
public static class HandleManager
{
    private static int _nextHandle = 1;
    private static readonly ConcurrentDictionary<int, HandleEntry> _entries = new();
    private static readonly object _lock = new();

    public static int Register(INativeDiskSession session)
    {
        return Register(session, new HandleMetadata("register", false));
    }

    public static int Register(INativeDiskSession session, HandleMetadata metadata)
    {
        lock (_lock)
        {
            int handle = _nextHandle++;
            _entries[handle] = new HandleEntry(session, metadata);
            return handle;
        }
    }

    public static bool TryGet(int handle, out INativeDiskSession? session)
    {
        if (_entries.TryGetValue(handle, out var entry))
        {
            session = entry.Session;
            return true;
        }

        session = null;
        return false;
    }

    public static bool TryGetMetadata(int handle, out HandleMetadata metadata)
    {
        if (_entries.TryGetValue(handle, out var entry))
        {
            metadata = entry.Metadata;
            return true;
        }

        metadata = default;
        return false;
    }

    public static bool Unregister(int handle)
    {
        if (_entries.TryRemove(handle, out var entry))
        {
            entry.Session.Dispose();
            return true;
        }
        return false;
    }

    public static bool IsRegistered(int handle)
    {
        return _entries.ContainsKey(handle);
    }

    public static int GetOpenHandleCount()
    {
        return _entries.Count;
    }

    public static void Clear()
    {
        foreach (var handle in _entries.Keys)
        {
            Unregister(handle);
        }
    }

    private sealed record HandleEntry(INativeDiskSession Session, HandleMetadata Metadata);
}
