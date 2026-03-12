using System.Text;
using Legacy89DiskKit.Domain.FileSystem.Interface.FileSystem;
using Legacy89DiskKit.Domain.FileSystem.Model;
using Legacy89DiskKit.Domain.CharacterEncoding.Interface;
using Legacy89DiskKit.Infrastructure.FileSystem.HuBasic;

using DomainAttr = Legacy89DiskKit.Domain.FileSystem.Model.FileAttributes;

namespace Legacy89DiskKit.Application.FileSystem;

public class FileTransferService
{
    private readonly ICharacterEncoder _encoder;

    public FileTransferService(ICharacterEncoder encoder)
    {
        _encoder = encoder;
    }

    public void ImportFile(IFileSystem fs, string hostPath, string diskFileName, bool isAscii = true)
    {
        byte[] diskData;

        if (isAscii)
        {
            string text = File.ReadAllText(hostPath);
            diskData = _encoder.EncodeText(text);
            
            // Ensure 0x1A terminator for Hu-BASIC text files
            if (diskData.Length == 0 || diskData[^1] != 0x1A)
            {
                var newData = new byte[diskData.Length + 1];
                Array.Copy(diskData, newData, diskData.Length);
                newData[^1] = 0x1A;
                diskData = newData;
            }
        }
        else
        {
            diskData = File.ReadAllBytes(hostPath);
        }

        var attributes = fs.CreateDefaultAttributes(isAscii);
        fs.WriteFile(diskFileName, diskData, attributes);
    }

    public void ExportFile(IFileSystem fs, string diskFileName, string hostPath, string? newlineOverride = null)
    {
        byte[] diskData = fs.ReadFile(diskFileName);
        
        // Find the file entry to check attributes
        var entry = fs.GetFiles().FirstOrDefault(f => f.FullName.Equals(diskFileName, StringComparison.OrdinalIgnoreCase));
        if (entry == null) throw new FileNotFoundException($"File not found on disk: {diskFileName}");

        if (entry.Attributes.IsAscii)
        {
            string newline = newlineOverride ?? Environment.NewLine;
            string text = _encoder.DecodeText(diskData, newline);
            var utf8WithoutBom = new UTF8Encoding(false);
            File.WriteAllText(hostPath, text, utf8WithoutBom);
        }
        else
        {
            File.WriteAllBytes(hostPath, diskData);
        }
    }
}
