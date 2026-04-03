using Legacy89DiskKit.DiskOperation.Domain.Model;
using Legacy89DiskKit.FileSystem.Domain.Interface.Factory;

namespace Legacy89DiskKit.DiskOperation.Domain.Interface;

public interface IFileNameConverter
{
    string ConvertFileName(
        string sourceFileName,
        FileSystemType sourceType,
        FileSystemType destinationType,
        IEnumerable<string> existingFileNames);
    
    bool IsFileNameValid(
        string fileName,
        FileSystemType fileSystemType);
    
    FileNameConversionResult AnalyzeConversion(
        string sourceFileName,
        FileSystemType sourceType,
        FileSystemType destinationType);
}