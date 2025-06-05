using Legacy89DiskKit.DiskImage.Domain.Exception;
using Legacy89DiskKit.DiskImage.Domain.Interface.Container;

namespace Legacy89DiskKit.DiskImage.Infrastructure.Container;

public class TwoDDiskContainer : IDiskContainer
{
    private readonly byte[] _diskData;
    private readonly string _filePath;
    private readonly bool _readOnly;

    private const int TRACKS = 40;
    private const int SIDES = 2;
    private const int SECTORS_PER_TRACK = 16;
    private const int BYTES_PER_SECTOR = 256;
    private const int TOTAL_SIZE = 327680; // 320KB

    public TwoDDiskContainer(string filePath, bool readOnly = true)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        _readOnly = readOnly;

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"2D disk image file not found: {filePath}");
        }

        _diskData = File.ReadAllBytes(filePath);

        if (_diskData.Length != TOTAL_SIZE)
        {
            throw new DiskImageException(
                $"Invalid 2D file size. Expected {TOTAL_SIZE} bytes, but got {_diskData.Length} bytes.");
        }
    }

    public byte[] ReadSector(int cylinder, int head, int sector)
    {
        return ReadSector(cylinder, head, sector, false);
    }

    public byte[] ReadSector(int cylinder, int head, int sector, bool allowCorrupted)
    {
        ValidateAddress(cylinder, sector, head);

        int offset = CalculateOffset(cylinder, head, sector);
        byte[] sectorData = new byte[BYTES_PER_SECTOR];
        Array.Copy(_diskData, offset, sectorData, 0, BYTES_PER_SECTOR);

        return sectorData;
    }

    public void WriteSector(int cylinder, int head, int sector, byte[] data)
    {
        if (_readOnly)
            throw new DiskImageException("Disk image is read-only");

        ValidateAddress(cylinder, sector, head);

        if (data.Length != BYTES_PER_SECTOR)
            throw new ArgumentException($"Sector size must be {BYTES_PER_SECTOR} bytes");

        int offset = CalculateOffset(cylinder, head, sector);
        Array.Copy(data, 0, _diskData, offset, BYTES_PER_SECTOR);
    }

    public bool SectorExists(int cylinder, int head, int sector)
    {
        try
        {
            ValidateAddress(cylinder, sector, head);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }

    public IEnumerable<SectorInfo> GetAllSectors()
    {
        for (int cylinder = 0; cylinder < TRACKS; cylinder++)
        {
            for (int head = 0; head < SIDES; head++)
            {
                for (int sector = 1; sector <= SECTORS_PER_TRACK; sector++)
                {
                    yield return new SectorInfo(cylinder, head, sector, BYTES_PER_SECTOR, false, false);
                }
            }
        }
    }

    public void Save()
    {
        if (_readOnly)
            throw new DiskImageException("Cannot save read-only disk image");

        File.WriteAllBytes(_filePath, _diskData);
    }

    public void SaveAs(string filePath)
    {
        File.WriteAllBytes(filePath, _diskData);
    }

    private int CalculateOffset(int cylinder, int head, int sector)
    {
        int linearSector = (cylinder * SIDES * SECTORS_PER_TRACK) +
                           (head * SECTORS_PER_TRACK) +
                           (sector - 1);

        return linearSector * BYTES_PER_SECTOR;
    }

    private void ValidateAddress(int cylinder, int sector, int head)
    {
        if (cylinder < 0 || cylinder >= TRACKS)
            throw new ArgumentOutOfRangeException(nameof(cylinder),
                $"Cylinder must be between 0 and {TRACKS - 1}");

        if (head < 0 || head >= SIDES)
            throw new ArgumentOutOfRangeException(nameof(head),
                "Head must be 0 or 1");

        if (sector < 1 || sector > SECTORS_PER_TRACK)
            throw new ArgumentOutOfRangeException(nameof(sector),
                $"Sector must be between 1 and {SECTORS_PER_TRACK}");
    }

    public string FilePath => _filePath;
    public bool IsReadOnly => _readOnly;
    public DiskType DiskType => DiskType.TwoD;

    public void Dispose()
    {
        // TwoDDiskContainerでは特に何もしない
    }
}