using System;
using System.IO;
using System.Linq;
using Legacy89DiskKit.Infrastructure.DiskImage.Container;
using Legacy89DiskKit.Infrastructure.FileSystem.XDos;
using Legacy89DiskKit.Domain.DiskImage.Model;

var path = "/tmp/debug_plain_2dd.d88";
var c = D88DiskContainer.CreateNew(path, DiskType.TwoDD, "DBG");
Console.WriteLine("1. Sectors in memory after CreateNew: " + c.GetAllSectors().Count());

var bytes1 = File.ReadAllBytes(path);
var t1 = Enumerable.Range(0, 164).Count(i => BitConverter.ToInt32(bytes1, 0x20 + i * 4) != 0);
Console.WriteLine("2. Tracks in file after CreateNew: " + t1);

var fs = new XDosFileSystem(c);
fs.Format();
Console.WriteLine("3. Sectors in memory after Format: " + c.GetAllSectors().Count());
c.Save();

var bytes2 = File.ReadAllBytes(path);
var t2 = Enumerable.Range(0, 164).Count(i => BitConverter.ToInt32(bytes2, 0x20 + i * 4) != 0);
Console.WriteLine("4. Tracks in file after Format+Save: " + t2 + " (size=" + bytes2.Length + ")");
c.Dispose();
