using Legacy89DiskKit.DiskImage.Domain.Interface.Factory;
using Legacy89DiskKit.DiskImage.Domain.Interface.Container;
using Legacy89DiskKit.FileSystem.Domain.Interface.Factory;
using Legacy89DiskKit.FileSystem.Domain.Interface.FileSystem;

namespace Legacy89DiskKit.CLI.Shell;

public class SlotManager : IDisposable
{
    private readonly IDiskContainerFactory _diskContainerFactory;
    private readonly IFileSystemFactory _fileSystemFactory;
    private readonly DiskSlot[] _slots;
    private int _currentSlot;

    public int CurrentSlot => _currentSlot;
    public DiskSlot CurrentDiskSlot => _slots[_currentSlot];
    public const int MaxSlots = 10;

    public SlotManager(IDiskContainerFactory diskContainerFactory, IFileSystemFactory fileSystemFactory)
    {
        _diskContainerFactory = diskContainerFactory;
        _fileSystemFactory = fileSystemFactory;
        _slots = new DiskSlot[MaxSlots];
        
        for (int i = 0; i < MaxSlots; i++)
        {
            _slots[i] = new DiskSlot(i);
        }
        
        _currentSlot = 0;
    }

    public bool IsSlotEmpty(int slotNumber)
    {
        ValidateSlotNumber(slotNumber);
        return _slots[slotNumber].IsEmpty;
    }

    public DiskSlot GetSlot(int slotNumber)
    {
        ValidateSlotNumber(slotNumber);
        return _slots[slotNumber];
    }

    public void SwitchToSlot(int slotNumber)
    {
        ValidateSlotNumber(slotNumber);
        _currentSlot = slotNumber;
    }

    public void MountDisk(int slotNumber, string diskPath, bool readOnly = false)
    {
        ValidateSlotNumber(slotNumber);
        
        var slot = _slots[slotNumber];
        slot.Unmount();

        var container = _diskContainerFactory.OpenDiskImage(diskPath, readOnly);
        
        IFileSystem fileSystem;
        string fileSystemType;
        
        try
        {
            if (readOnly)
            {
                fileSystem = _fileSystemFactory.OpenFileSystemReadOnly(container);
                fileSystemType = _fileSystemFactory.GuessFileSystemType(container).ToString();
            }
            else
            {
                var detectedType = _fileSystemFactory.GuessFileSystemType(container);
                fileSystem = _fileSystemFactory.OpenFileSystem(container, detectedType);
                fileSystemType = detectedType.ToString();
            }
        }
        catch
        {
            container.Dispose();
            throw;
        }

        slot.Mount(diskPath, container, fileSystem, fileSystemType);
    }

    public void UnmountSlot(int slotNumber)
    {
        ValidateSlotNumber(slotNumber);
        _slots[slotNumber].Unmount();
    }

    public void CreateNewDisk(int slotNumber, string diskPath, DiskType diskType, string diskName)
    {
        ValidateSlotNumber(slotNumber);
        
        var slot = _slots[slotNumber];
        slot.Unmount();

        var container = _diskContainerFactory.CreateNewDiskImage(diskPath, diskType, diskName);
        
        try
        {
            var fileSystemType = "Unformatted";
            slot.Mount(diskPath, container, null!, fileSystemType);
        }
        catch
        {
            container.Dispose();
            throw;
        }
    }

    public void FormatDisk(int slotNumber, FileSystemType fileSystemType)
    {
        ValidateSlotNumber(slotNumber);
        
        var slot = _slots[slotNumber];
        if (slot.IsEmpty)
            throw new InvalidOperationException($"Slot {slotNumber} is empty");

        if (slot.Container!.IsReadOnly)
            throw new InvalidOperationException($"Slot {slotNumber} is mounted read-only");

        var fileSystem = _fileSystemFactory.CreateFileSystem(slot.Container, fileSystemType);
        fileSystem.Format();
        
        slot.Mount(slot.DiskPath!, slot.Container, fileSystem, fileSystemType.ToString());
    }

    public IEnumerable<DiskSlot> GetAllSlots()
    {
        return _slots;
    }

    public IEnumerable<DiskSlot> GetMountedSlots()
    {
        return _slots.Where(s => !s.IsEmpty);
    }

    public string GetPrompt()
    {
        var slot = CurrentDiskSlot;
        if (slot.IsEmpty)
        {
            return $"Legacy89DiskKit [{_currentSlot}:Empty]>";
        }
        
        return $"Legacy89DiskKit [{_currentSlot}:{slot.GetDisplayName()}]>";
    }

    private void ValidateSlotNumber(int slotNumber)
    {
        if (slotNumber < 0 || slotNumber >= MaxSlots)
        {
            throw new ArgumentOutOfRangeException(nameof(slotNumber), 
                $"Slot number must be between 0 and {MaxSlots - 1}");
        }
    }

    public void Dispose()
    {
        for (int i = 0; i < MaxSlots; i++)
        {
            _slots[i].Unmount();
        }
    }
}