using System.Text.RegularExpressions;

namespace Legacy89DiskKit.FileSystem.Infrastructure.Utility;

public static class HuBasicFileNameValidator
{
    // Hu-BASICで使用できない文字
    private static readonly char[] InvalidChars = { 
        '/', '\\', ':', '*', '?', '"', '<', '>', '|', 
        '\0', '\t', '\n', '\r', ' ' // 制御文字とスペース
    };
    
    // Hu-BASICで使用できない文字の正規表現
    private static readonly Regex InvalidCharsRegex = new Regex(@"[\x00-\x1F\x7F/\\:*?""<>| ]", RegexOptions.Compiled);
    
    public static bool IsValidFileName(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return false;
            
        // 長さチェック（最大13文字）
        if (fileName.Length > 13)
            return false;
            
        // 無効文字チェック
        if (InvalidCharsRegex.IsMatch(fileName))
            return false;
            
        // 先頭・末尾のドットチェック
        if (fileName.StartsWith('.') || fileName.EndsWith('.'))
            return false;
            
        // 予約名チェック
        if (IsReservedName(fileName))
            return false;
            
        return true;
    }
    
    public static bool IsValidExtension(string extension)
    {
        if (string.IsNullOrEmpty(extension))
            return true; // 拡張子は省略可能
            
        // 長さチェック（最大3文字）
        if (extension.Length > 3)
            return false;
            
        // 無効文字チェック
        if (InvalidCharsRegex.IsMatch(extension))
            return false;
            
        return true;
    }
    
    public static (string fileName, string extension) SplitFileName(string fullName)
    {
        if (string.IsNullOrEmpty(fullName))
            return ("", "");
            
        var lastDotIndex = fullName.LastIndexOf('.');
        if (lastDotIndex == -1 || lastDotIndex == 0 || lastDotIndex == fullName.Length - 1)
        {
            // ドットがない、先頭にある、末尾にある場合は拡張子なし
            return (fullName, "");
        }
        
        var fileName = fullName.Substring(0, lastDotIndex);
        var extension = fullName.Substring(lastDotIndex + 1);
        
        return (fileName, extension);
    }
    
    public static string ValidateAndNormalize(string fileName)
    {
        if (!IsValidFileName(fileName))
            throw new ArgumentException($"Invalid Hu-BASIC file name: '{fileName}'");
            
        // Hu-BASICでは大文字小文字を区別するため、正規化は行わない
        return fileName;
    }
    
    public static string CreateValidFileName(string input, string defaultName = "FILE")
    {
        if (string.IsNullOrWhiteSpace(input))
            return defaultName;
            
        // 無効文字を'_'に置換
        var result = InvalidCharsRegex.Replace(input, "_");
        
        // 長さ制限
        if (result.Length > 13)
            result = result.Substring(0, 13);
            
        // 先頭・末尾のドット除去
        result = result.Trim('.');
        
        // 空文字になった場合はデフォルト名
        if (string.IsNullOrEmpty(result))
            result = defaultName;
            
        // 予約名チェック
        if (IsReservedName(result))
            result = defaultName;
            
        return result;
    }
    
    private static bool IsReservedName(string name)
    {
        // Hu-BASICの予約ファイル名
        var reservedNames = new[] { 
            "CON", "PRN", "AUX", "NUL",
            "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
            "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
        };
        
        return reservedNames.Contains(name.ToUpperInvariant());
    }
    
    public static void ValidateFileSize(int size)
    {
        if (size < 0)
            throw new ArgumentException("File size cannot be negative");
            
        if (size > 65535)
            throw new ArgumentException($"File size too large: {size} bytes (max: 65535)");
    }
    
    public static void ValidateAddress(ushort address, string parameterName)
    {
        // X1の一般的なアドレス範囲をチェック
        if (address != 0 && address < 0x8000)
            throw new ArgumentException($"{parameterName} address {address:X4} is below recommended range (0x8000-0xFFFF)");
    }
    
    public static void ValidateLoadExecuteAddresses(ushort loadAddress, ushort executeAddress)
    {
        ValidateAddress(loadAddress, "Load");
        ValidateAddress(executeAddress, "Execute");
        
        // 実行アドレスがロードアドレス範囲内にあるかチェック（ゼロでない場合）
        if (loadAddress != 0 && executeAddress != 0 && executeAddress < loadAddress)
            throw new ArgumentException($"Execute address {executeAddress:X4} is before load address {loadAddress:X4}");
    }
}