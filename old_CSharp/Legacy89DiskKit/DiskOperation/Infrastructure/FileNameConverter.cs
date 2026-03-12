using Legacy89DiskKit.DiskOperation.Domain.Interface;
using Legacy89DiskKit.DiskOperation.Domain.Model;
using Legacy89DiskKit.DiskOperation.Domain.Exception;
using Legacy89DiskKit.FileSystem.Domain.Interface.Factory;
using System.Text.RegularExpressions;

namespace Legacy89DiskKit.DiskOperation.Infrastructure;

public class FileNameConverter : IFileNameConverter
{
    private readonly Dictionary<FileSystemType, FileNameRules> _fileNameRules = new()
    {
        [FileSystemType.HuBasic] = new FileNameRules { MaxBaseName = 8, MaxExtension = 3, HasExtension = true },
        [FileSystemType.N88Basic] = new FileNameRules { MaxBaseName = 6, MaxExtension = 0, HasExtension = false },
        [FileSystemType.Fat12] = new FileNameRules { MaxBaseName = 8, MaxExtension = 3, HasExtension = true },
        [FileSystemType.MsxDos] = new FileNameRules { MaxBaseName = 8, MaxExtension = 3, HasExtension = true },
        [FileSystemType.Cpm] = new FileNameRules { MaxBaseName = 8, MaxExtension = 3, HasExtension = true },
        [FileSystemType.Cdos] = new FileNameRules { MaxBaseName = 8, MaxExtension = 3, HasExtension = true }
    };

    public string ConvertFileName(
        string sourceFileName,
        FileSystemType sourceType,
        FileSystemType destinationType,
        IEnumerable<string> existingFileNames)
    {
        var result = AnalyzeConversion(sourceFileName, sourceType, destinationType);
        
        if (!result.RequiresConversion)
        {
            return sourceFileName;
        }

        var destRules = _fileNameRules[destinationType];
        var existingNames = new HashSet<string>(existingFileNames, StringComparer.OrdinalIgnoreCase);
        
        var (baseName, extension) = SplitFileName(sourceFileName);
        
        if (!destRules.HasExtension)
        {
            baseName = baseName + extension;
            extension = string.Empty;
        }
        
        baseName = TruncateBaseName(baseName, destRules.MaxBaseName);
        extension = TruncateExtension(extension, destRules.MaxExtension);
        
        var convertedName = BuildFileName(baseName, extension, destRules.HasExtension);
        
        if (!existingNames.Contains(convertedName))
        {
            return convertedName;
        }
        
        return GenerateUniqueFileName(baseName, extension, destRules, existingNames);
    }

    public bool IsFileNameValid(string fileName, FileSystemType fileSystemType)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        var rules = _fileNameRules[fileSystemType];
        var (baseName, extension) = SplitFileName(fileName);

        if (!rules.HasExtension && !string.IsNullOrEmpty(extension))
            return false;

        if (baseName.Length > rules.MaxBaseName)
            return false;

        if (rules.HasExtension && extension.Length > rules.MaxExtension)
            return false;

        return IsValidCharacters(fileName, fileSystemType);
    }

    public FileNameConversionResult AnalyzeConversion(
        string sourceFileName,
        FileSystemType sourceType,
        FileSystemType destinationType)
    {
        if (IsFileNameValid(sourceFileName, destinationType))
        {
            return new FileNameConversionResult
            {
                OriginalName = sourceFileName,
                ConvertedName = sourceFileName,
                RequiresConversion = false,
                ConversionType = FileNameConversionType.None
            };
        }

        var destRules = _fileNameRules[destinationType];
        var (baseName, extension) = SplitFileName(sourceFileName);
        var conversionType = FileNameConversionType.None;
        var reasons = new List<string>();

        if (!destRules.HasExtension && !string.IsNullOrEmpty(extension))
        {
            conversionType = FileNameConversionType.Truncated;
            reasons.Add("Destination file system does not support extensions");
        }

        if (baseName.Length > destRules.MaxBaseName)
        {
            conversionType = conversionType == FileNameConversionType.None 
                ? FileNameConversionType.Truncated 
                : FileNameConversionType.TruncatedAndRenamed;
            reasons.Add($"Base name exceeds {destRules.MaxBaseName} characters");
        }

        if (destRules.HasExtension && extension.Length > destRules.MaxExtension)
        {
            conversionType = conversionType == FileNameConversionType.None 
                ? FileNameConversionType.Truncated 
                : FileNameConversionType.TruncatedAndRenamed;
            reasons.Add($"Extension exceeds {destRules.MaxExtension} characters");
        }

        return new FileNameConversionResult
        {
            OriginalName = sourceFileName,
            ConvertedName = string.Empty,
            RequiresConversion = true,
            ConversionType = conversionType,
            ConversionReason = string.Join("; ", reasons)
        };
    }

    private (string baseName, string extension) SplitFileName(string fileName)
    {
        var lastDot = fileName.LastIndexOf('.');
        if (lastDot == -1 || lastDot == 0 || lastDot == fileName.Length - 1)
        {
            return (fileName, string.Empty);
        }

        return (fileName[..lastDot], fileName[(lastDot + 1)..]);
    }

    private string TruncateBaseName(string baseName, int maxLength)
    {
        if (baseName.Length <= maxLength)
            return baseName;

        return maxLength >= 4 ? baseName[..(maxLength - 3)] : baseName[..maxLength];
    }

    private string TruncateExtension(string extension, int maxLength)
    {
        return extension.Length <= maxLength ? extension : extension[..maxLength];
    }

    private string BuildFileName(string baseName, string extension, bool hasExtension)
    {
        return hasExtension && !string.IsNullOrEmpty(extension) 
            ? $"{baseName}.{extension}" 
            : baseName;
    }

    private string GenerateUniqueFileName(
        string baseName,
        string extension,
        FileNameRules rules,
        HashSet<string> existingNames)
    {
        var maxBaseLength = rules.HasExtension ? rules.MaxBaseName : rules.MaxBaseName;
        
        for (int i = 1; i <= 999; i++)
        {
            var numberSuffix = i.ToString("D3");
            var truncatedBase = baseName.Length + numberSuffix.Length > maxBaseLength
                ? baseName[..(maxBaseLength - numberSuffix.Length)]
                : baseName;
            
            var uniqueBaseName = truncatedBase + numberSuffix;
            var uniqueFileName = BuildFileName(uniqueBaseName, extension, rules.HasExtension);
            
            if (!existingNames.Contains(uniqueFileName))
            {
                return uniqueFileName;
            }
        }

        throw new FileNameConversionException(
            BuildFileName(baseName, extension, rules.HasExtension),
            "Unable to generate unique file name after 999 attempts");
    }

    private bool IsValidCharacters(string fileName, FileSystemType fileSystemType)
    {
        var invalidCharsPattern = fileSystemType switch
        {
            FileSystemType.HuBasic => @"[^\w\.\-]",
            FileSystemType.N88Basic => @"[^\w\-]",
            FileSystemType.Fat12 => @"[^\w\.\-\s]",
            FileSystemType.MsxDos => @"[^\w\.\-\s]",
            FileSystemType.Cpm => @"[^\w\.\-]",
            FileSystemType.Cdos => @"[^\w\.\-]",
            _ => @"[^\w\.\-]"
        };

        return !Regex.IsMatch(fileName, invalidCharsPattern);
    }

    private class FileNameRules
    {
        public int MaxBaseName { get; init; }
        public int MaxExtension { get; init; }
        public bool HasExtension { get; init; }
    }
}