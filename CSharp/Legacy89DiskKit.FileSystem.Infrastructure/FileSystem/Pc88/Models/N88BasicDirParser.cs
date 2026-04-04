using System.Text;
using Legacy89DiskKit.FileSystem.Domain.Model;
using DomainAttr = Legacy89DiskKit.FileSystem.Domain.Model.FileAttributes;
using Legacy89DiskKit.CharacterEncoding.Domain.Interface;

namespace Legacy89DiskKit.FileSystem.Infrastructure.Pc88.Models;

public class N88BasicDirParser
{
    private readonly N88BasicConfiguration _config;

    public N88BasicDirParser(N88BasicConfiguration config)
    {
        _config = config;
    }

    public FileEntry Parse(byte[] entryData)
    {
        // 0-5: Name (6 chars)
        // 6-8: Ext (3 chars)
        string name = Encoding.ASCII.GetString(entryData, 0, 6).TrimEnd();
        string ext = Encoding.ASCII.GetString(entryData, 6, 3).TrimEnd();

        // 9: Attribute
        byte attrByte = entryData[9];
        bool isBinary = (attrByte & 0x01) != 0;
        bool isTokenized = (attrByte & 0x80) != 0;
        bool isAscii = !isTokenized && !isBinary;

        var attr = DomainAttr.None;
        if ((attrByte & 0x10) != 0) attr |= DomainAttr.ReadOnly;

        var extendedAttr = new ExtendedFileAttributes(
            attr,
            attrByte,
            isAscii,
            "PC88"
        );

        // 10: Start Cluster
        int startCluster = entryData[10];

        // 11-15: Reserved (unused)
        
        // Note: N88-BASIC directory does not store file size in bytes for all types.
        // For simplicity in this implementation, we might need to calculate it from clusters 
        // or rely on EOF markers in the future. For now, we use a placeholder or 0 if unknown.
        // Actually, FAT chain length * ClusterSize is a safe upper bound.
        var nameBytes = entryData.Take(6).ToArray();
        var extBytes = entryData.Skip(6).Take(3).ToArray();

        return new FileEntry(
            name,
            ext,
            0, // Size will be enriched by FileSystem
            null, // Date not stored in N88-BASIC dir
            DateTime.MinValue,
            extendedAttr,
            startCluster,
            null, null, null,
            nameBytes,
            extBytes
        );
    }

    public void WriteToBuffer(byte[] buffer, int offset, FileEntry entry)
    {
        // Clear entry
        Array.Clear(buffer, offset, 16);

        // Name (6 chars)
        byte[] nameBytes = Encoding.ASCII.GetBytes(entry.FileName.PadRight(6).Substring(0, 6));
        Array.Copy(nameBytes, 0, buffer, offset, 6);

        // Ext (3 chars)
        byte[] extBytes = Encoding.ASCII.GetBytes(entry.Extension.PadRight(3).Substring(0, 3));
        Array.Copy(extBytes, 0, buffer, offset + 6, 3);

        // Attribute
        buffer[offset + 9] = entry.Attributes.RawAttributes;

        // Start Cluster
        buffer[offset + 10] = (byte)entry.StartCluster;
    }
}
