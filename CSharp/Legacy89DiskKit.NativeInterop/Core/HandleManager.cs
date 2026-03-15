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
    private static readonly ConcurrentDictionary<int, DiskService> _services = new();
    private static readonly object _lock = new();

    public static int Register(DiskService service)
    {
        lock (_lock)
        {
            int handle = _nextHandle++;
            _services[handle] = service;
            return handle;
        }
    }

    public static bool TryGet(int handle, out DiskService? service)
    {
        return _services.TryGetValue(handle, out service);
    }

    public static bool Unregister(int handle)
    {
        if (_services.TryRemove(handle, out var service))
        {
            service.Dispose();
            return true;
        }
        return false;
    }

    public static bool IsRegistered(int handle)
    {
        return _services.ContainsKey(handle);
    }

    public static int GetOpenHandleCount()
    {
        return _services.Count;
    }

    public static void Clear()
    {
        foreach (var handle in _services.Keys)
        {
            Unregister(handle);
        }
    }
}
