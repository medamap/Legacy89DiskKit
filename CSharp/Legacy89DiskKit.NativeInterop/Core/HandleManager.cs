using System.Collections.Concurrent;
using Legacy89DiskKit.Application.DiskImage;
using Legacy89DiskKit.NativeInterop.Types;

namespace Legacy89DiskKit.NativeInterop.Core;

/// <summary>
/// Manages DiskService instances and maps them to integer handles.
/// </summary>
public static class HandleManager
{
    private static int _nextHandle = 1;
    private static readonly ConcurrentDictionary<int, HandleEntry> _entries = new();
    private static readonly object _lock = new();

    public static int Register(DiskService service)
    {
        return Register(service, new HandleMetadata("register", false));
    }

    public static int Register(DiskService service, HandleMetadata metadata)
    {
        lock (_lock)
        {
            int handle = _nextHandle++;
            _entries[handle] = new HandleEntry(service, metadata);
            return handle;
        }
    }

    public static bool TryGet(int handle, out DiskService? service)
    {
        if (_entries.TryGetValue(handle, out var entry))
        {
            service = entry.Service;
            return true;
        }

        service = null;
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
            entry.Service.Dispose();
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

    private sealed record HandleEntry(DiskService Service, HandleMetadata Metadata);
}
