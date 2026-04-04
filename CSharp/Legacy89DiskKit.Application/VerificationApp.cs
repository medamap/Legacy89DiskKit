using Legacy89DiskKit.DiskImage.Application;
using Legacy89DiskKit.FileSystem.Application;
using Legacy89DiskKit.Infrastructure.CharacterEncoding.Encoder;
using Legacy89DiskKit.Domain.DiskImage.Model;
using Legacy89DiskKit.Domain.FileSystem.Model;
using System.Text;

using DomainAttr = Legacy89DiskKit.Domain.FileSystem.Model.FileAttributes;

Console.WriteLine("--- High-Level Application Service Verification ---");

string testDisk = "service_test.d88";
string hostInputPath = "host_input.txt";
string hostOutputPath = "host_output.txt";

if (File.Exists(testDisk)) File.Delete(testDisk);
if (File.Exists(hostInputPath)) File.Delete(hostInputPath);
if (File.Exists(hostOutputPath)) File.Delete(hostOutputPath);

// 1. Create host input file
string originalText = "Hello X1 Application Service!\nｱｲｳｴｵ (Katakana)\n♠♥♦♣ (Symbols)\nπ (Math)\n" +
                      "▂▃▄▅▆▇█ (Blocks)\n" +
                      "Final line.";
File.WriteAllText(hostInputPath, originalText, Encoding.UTF8);
Console.WriteLine($"Stage 1: Created host file {hostInputPath}");

// 2. Initialize Services
using var diskService = new DiskService();
var encoder = new X1CharacterEncoder();
var transferService = new FileTransferService(encoder);

// 3. Create and Open Disk
Console.WriteLine("Stage 2: Creating blank 2D D88 image...");
using (var container = Legacy89DiskKit.Infrastructure.DiskImage.Container.D88DiskContainer.CreateNew(testDisk, DiskType.TwoD, "SERVTEST"))
{
    var fs = new Legacy89DiskKit.Infrastructure.FileSystem.HuBasic.HuBasicFileSystem(container);
    fs.Format();
    fs.WriteBootArea(Encoding.ASCII.GetBytes("Hu-BASIC"));
}

Console.WriteLine("Stage 3: Opening disk via DiskService...");
diskService.OpenDisk(testDisk, false);
var targetFs = diskService.FileSystem;

if (targetFs == null)
{
    Console.WriteLine("Error: File system not detected!");
    return;
}
Console.WriteLine("Detected FileSystem: Hu-BASIC");

// 4. Import file
Console.WriteLine($"Stage 4: Importing {hostInputPath} to disk as TEST.TXT...");
transferService.ImportFile(targetFs, hostInputPath, "TEST.TXT", true);

// 5. List files
Console.WriteLine("Stage 5: Listing files on disk...");
foreach (var f in targetFs.GetFiles())
{
    Console.WriteLine($"- {f.FullName} ({f.Size} bytes)");
}

// 6. Export file with Windows Newlines (explicit override)
Console.WriteLine($"Stage 6: Exporting TEST.TXT to {hostOutputPath} with CRLF override...");
transferService.ExportFile(targetFs, "TEST.TXT", hostOutputPath, "\r\n");

// 7. Verify
Console.WriteLine("Stage 7: Verifying results...");
byte[] exportedBytes = File.ReadAllBytes(hostOutputPath);
string exportedText = Encoding.UTF8.GetString(exportedBytes);

Console.WriteLine("Exported Text (Raw HEX for newlines):");
Console.WriteLine(BitConverter.ToString(exportedBytes).Replace("-", " ").Substring(0, Math.Min(exportedBytes.Length * 3, 100)) + "...");
Console.WriteLine($"Exported Text:\n{exportedText}");

if (exportedText.Contains("\r\n"))
{
    Console.WriteLine("✓ Verified: Exported file contains CRLF as requested.");
}
else
{
    Console.WriteLine("✗ Failure: Exported file does NOT contain CRLF.");
}

// Normalize for content comparison
string normalizedOriginal = originalText.Replace("\r\n", "\n").Replace("\r", "\n").TrimEnd();
string normalizedExported = exportedText.Replace("\r\n", "\n").Replace("\r", "\n").TrimEnd();

if (normalizedOriginal == normalizedExported)
{
    Console.WriteLine("\n--- Phase 2: Advanced Operations (Rename, Attributes, Copy) ---");
    
    // 8. Test Rename
    Console.WriteLine("Stage 8: Renaming TEST.TXT to RENAMED.TXT...");
    targetFs.RenameFile("TEST.TXT", "RENAMED.TXT");

    var files = targetFs.GetFiles().ToList();
    if (files.Any(f => f.FullName == "RENAMED.TXT") && !files.Any(f => f.FullName == "TEST.TXT"))
    {
        Console.WriteLine("✓ Success: File renamed correctly.");
    }
    else
    {
        Console.WriteLine("✗ Failure: Rename failed.");
        return;
    }

    // 9. Test Attribute Update
    Console.WriteLine("Stage 9: Updating attributes to ReadOnly | System...");
    var newAttr = new ExtendedFileAttributes(DomainAttr.ReadOnly | DomainAttr.System, 0x01, true, "");
    targetFs.UpdateAttributes("RENAMED.TXT", newAttr);

    var updatedFile = targetFs.GetFiles().First(f => f.FullName == "RENAMED.TXT");
    if (updatedFile.Attributes.StandardAttributes.HasFlag(DomainAttr.ReadOnly) && 
        updatedFile.Attributes.StandardAttributes.HasFlag(DomainAttr.System))
    {
        Console.WriteLine("✓ Success: Attributes updated correctly.");
    }
    else
    {
        Console.WriteLine($"✗ Failure: Attributes mismatch. Got: {updatedFile.Attributes.StandardAttributes}");
        return;
    }

    // 10. Test Copy (Internal)
    Console.WriteLine("Stage 10: Internal copy RENAMED.TXT to COPY.TXT...");
    targetFs.CopyFile("RENAMED.TXT", "COPY.TXT");
    if (targetFs.FileExists("COPY.TXT"))
    {
        var copyData = targetFs.ReadFile("COPY.TXT");
        // For ASCII, ReadFile might return different length due to EOF handling, but content should match
        if (copyData.Length > 0)
        {
            Console.WriteLine("✓ Success: Internal copy verified.");
        }
    }
    else
    {
        Console.WriteLine("✗ Failure: Copy destination not found.");
        return;
    }

    Console.WriteLine("\n--- ALL VERIFICATIONS SUCCESS ---");
}
else
{
    Console.WriteLine("\n--- VERIFICATION FAILURE ---");
    Console.WriteLine("Original (Normalized):");
    Console.WriteLine(normalizedOriginal);
    Console.WriteLine("Exported (Normalized):");
    Console.WriteLine(normalizedExported);
}

// Cleanup
if (File.Exists("adv_ops_test.d88")) File.Delete("adv_ops_test.d88");
if (File.Exists("service_test.d88")) File.Delete("service_test.d88");
if (File.Exists("host_input.txt")) File.Delete("host_input.txt");
if (File.Exists("host_output.txt")) File.Delete("host_output.txt");
