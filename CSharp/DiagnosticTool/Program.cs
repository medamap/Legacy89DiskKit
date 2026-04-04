using System;
using System.Text;
using Legacy89DiskKit.DiskImage.Infrastructure.Factory;
using Legacy89DiskKit.DiskImage.Domain.Interface.Container;

namespace DiagnosticTool;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0) return;
        string path = args[0];
        Console.WriteLine($"Checking: {path}");

        try
        {
            var factory = new DiskContainerFactory();
            using var container = factory.Open(path, true);
            var bootData = container.ReadSector(0, 0, 1);
            
            Console.WriteLine($"Boot sector size: {bootData.Length}");
            if (bootData.Length >= 8)
            {
                string hex = BitConverter.ToString(bootData, 0, Math.Min(32, bootData.Length));
                Console.WriteLine($"Hex: {hex}");
                string ascii = Encoding.ASCII.GetString(bootData, 0, Math.Min(32, bootData.Length)).Replace("\0", ".");
                Console.WriteLine($"ASCII: {ascii}");
                
                string signature = Encoding.ASCII.GetString(bootData, 0, 8);
                Console.WriteLine($"Signature: {signature}");
                Console.WriteLine($"Match HU-BASIC: {signature.StartsWith("Hu-BASIC", StringComparison.OrdinalIgnoreCase)}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
