using Legacy89DiskKit.DiskImage.Domain.Interface.Factory;

namespace Legacy89DiskKit.CLI.Shell;

public class DebugCommand
{
    public static void DumpDiskHeader(string diskPath, IDiskContainerFactory diskContainerFactory)
    {
        try
        {
            using var container = diskContainerFactory.OpenDiskImage(diskPath, readOnly: true);
            
            Console.WriteLine($"Disk: {diskPath}");
            Console.WriteLine($"Type: {container.DiskType}");
            
            // Get sector information from the first sector
            var sectorInfo = container.GetAllSectors().FirstOrDefault();
            if (sectorInfo != null)
            {
                Console.WriteLine($"First sector size: {sectorInfo.Size} bytes");
            }
            Console.WriteLine();
            
            // Read first few sectors
            for (int track = 0; track < 2; track++)
            {
                for (int sector = 1; sector <= 3; sector++)
                {
                    try
                    {
                        var data = container.ReadSector(track, 0, sector);
                        Console.WriteLine($"Track {track}, Sector {sector} (First 256 bytes):");
                        DumpHex(data.Take(256).ToArray());
                        Console.WriteLine();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Track {track}, Sector {sector}: Error - {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error opening disk: {ex.Message}");
        }
    }

    private static void DumpHex(byte[] data)
    {
        for (int i = 0; i < data.Length; i += 16)
        {
            Console.Write($"{i:X4}: ");
            
            // Hex bytes
            for (int j = 0; j < 16; j++)
            {
                if (i + j < data.Length)
                    Console.Write($"{data[i + j]:X2} ");
                else
                    Console.Write("   ");
            }
            
            Console.Write(" ");
            
            // ASCII
            for (int j = 0; j < 16; j++)
            {
                if (i + j < data.Length)
                {
                    byte b = data[i + j];
                    if (b >= 0x20 && b < 0x7F)
                        Console.Write((char)b);
                    else
                        Console.Write('.');
                }
            }
            
            Console.WriteLine();
        }
    }
}