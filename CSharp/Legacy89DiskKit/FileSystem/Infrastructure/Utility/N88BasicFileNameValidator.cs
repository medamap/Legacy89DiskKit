using System.Text.RegularExpressions;

namespace Legacy89DiskKit.FileSystem.Infrastructure.Utility;

/// <summary>
/// PC-8801 N88-BASIC ファイル名バリデーター
/// </summary>
public static class N88BasicFileNameValidator
{
    // N88-BASICファイル名制限
    private const int MaxFileNameLength = 6;
    private const int MaxExtensionLength = 3;
    
    // 使用可能文字パターン (ASCII文字・数字・一部記号)
    private static readonly Regex ValidFileNamePattern = new(@"^[A-Z0-9_\-\$]{1,6}$", RegexOptions.Compiled);
    private static readonly Regex ValidExtensionPattern = new(@"^[A-Z0-9_\-\$]{1,3}$", RegexOptions.Compiled);
    
    // 予約語リスト (N88-BASIC内部コマンドと競合する名前)
    private static readonly HashSet<string> ReservedNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "CON", "PRN", "AUX", "NUL",
        "COM1", "COM2", "LPT1", "LPT2",
        "DISK", "CRT", "SCRN", "KYBD"
    };
    
    /// <summary>
    /// フルファイル名の妥当性を検証
    /// </summary>
    /// <param name="fileName">検証するファイル名 (拡張子含む可能)</param>
    /// <returns>検証結果</returns>
    public static ValidationResult ValidateFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return ValidationResult.Error("ファイル名が空です");
        }
        
        // 大文字に正規化
        fileName = fileName.ToUpperInvariant();
        
        // ファイル名と拡張子を分離
        var parts = fileName.Split('.');
        string baseName;
        string? extension = null;
        
        if (parts.Length == 1)
        {
            baseName = parts[0];
        }
        else if (parts.Length == 2)
        {
            baseName = parts[0];
            extension = parts[1];
        }
        else
        {
            return ValidationResult.Error("ファイル名に複数のドット(.)を含めることはできません");
        }
        
        // ベース名の検証
        var baseValidation = ValidateBaseName(baseName);
        if (!baseValidation.IsValid)
        {
            return baseValidation;
        }
        
        // 拡張子の検証 (存在する場合)
        if (!string.IsNullOrEmpty(extension))
        {
            var extensionValidation = ValidateExtension(extension);
            if (!extensionValidation.IsValid)
            {
                return extensionValidation;
            }
        }
        
        // 予約語チェック
        if (ReservedNames.Contains(baseName))
        {
            return ValidationResult.Error($"'{baseName}' は予約語のため使用できません");
        }
        
        return ValidationResult.Success(baseName, extension ?? string.Empty);
    }
    
    /// <summary>
    /// ベースファイル名の妥当性を検証
    /// </summary>
    /// <param name="baseName">ベースファイル名 (拡張子なし)</param>
    /// <returns>検証結果</returns>
    private static ValidationResult ValidateBaseName(string baseName)
    {
        if (string.IsNullOrWhiteSpace(baseName))
        {
            return ValidationResult.Error("ファイル名が空です");
        }
        
        if (baseName.Length > MaxFileNameLength)
        {
            return ValidationResult.Error($"ファイル名は{MaxFileNameLength}文字以内である必要があります (現在: {baseName.Length}文字)");
        }
        
        if (!ValidFileNamePattern.IsMatch(baseName))
        {
            return ValidationResult.Error("ファイル名に使用できない文字が含まれています (使用可能: A-Z, 0-9, _, -, $)");
        }
        
        // 先頭文字チェック (数字で始まることは可能だが警告)
        if (char.IsDigit(baseName[0]))
        {
            return ValidationResult.Warning(baseName, string.Empty, "ファイル名が数字で始まっています");
        }
        
        return ValidationResult.Success(baseName, string.Empty);
    }
    
    /// <summary>
    /// 拡張子の妥当性を検証
    /// </summary>
    /// <param name="extension">拡張子</param>
    /// <returns>検証結果</returns>
    private static ValidationResult ValidateExtension(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
        {
            return ValidationResult.Success(string.Empty, string.Empty);
        }
        
        if (extension.Length > MaxExtensionLength)
        {
            return ValidationResult.Error($"拡張子は{MaxExtensionLength}文字以内である必要があります (現在: {extension.Length}文字)");
        }
        
        if (!ValidExtensionPattern.IsMatch(extension))
        {
            return ValidationResult.Error("拡張子に使用できない文字が含まれています (使用可能: A-Z, 0-9, _, -, $)");
        }
        
        return ValidationResult.Success(string.Empty, extension);
    }
    
    /// <summary>
    /// ファイル名を正規化 (大文字化、長さ調整)
    /// </summary>
    /// <param name="fileName">元のファイル名</param>
    /// <returns>正規化されたファイル名</returns>
    public static string NormalizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return string.Empty;
        }
        
        var validation = ValidateFileName(fileName);
        if (!validation.IsValid)
        {
            throw new ArgumentException($"不正なファイル名: {validation.ErrorMessage}");
        }
        
        return $"{validation.BaseName}" + 
               (string.IsNullOrEmpty(validation.Extension) ? "" : $".{validation.Extension}");
    }
    
    /// <summary>
    /// 不正なファイル名に対する修正候補を生成
    /// </summary>
    /// <param name="fileName">元のファイル名</param>
    /// <returns>修正候補リスト</returns>
    public static List<string> GenerateAlternatives(string fileName)
    {
        var alternatives = new List<string>();
        
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return alternatives;
        }
        
        var upperFileName = fileName.ToUpperInvariant();
        var parts = upperFileName.Split('.');
        var baseName = parts[0];
        var extension = parts.Length > 1 ? parts[1] : string.Empty;
        
        // 長すぎる場合の切り詰め
        if (baseName.Length > MaxFileNameLength)
        {
            var truncated = baseName.Substring(0, MaxFileNameLength);
            alternatives.Add(string.IsNullOrEmpty(extension) ? truncated : $"{truncated}.{extension}");
        }
        
        if (!string.IsNullOrEmpty(extension) && extension.Length > MaxExtensionLength)
        {
            var truncatedExt = extension.Substring(0, MaxExtensionLength);
            alternatives.Add($"{baseName}.{truncatedExt}");
        }
        
        // 不正文字の置換
        var cleanBaseName = Regex.Replace(baseName, @"[^A-Z0-9_\-\$]", "_");
        var cleanExtension = string.IsNullOrEmpty(extension) ? "" : 
                           Regex.Replace(extension, @"[^A-Z0-9_\-\$]", "_");
        
        if (cleanBaseName != baseName || cleanExtension != extension)
        {
            if (cleanBaseName.Length > MaxFileNameLength)
                cleanBaseName = cleanBaseName.Substring(0, MaxFileNameLength);
            if (!string.IsNullOrEmpty(cleanExtension) && cleanExtension.Length > MaxExtensionLength)
                cleanExtension = cleanExtension.Substring(0, MaxExtensionLength);
            
            var cleanName = string.IsNullOrEmpty(cleanExtension) ? cleanBaseName : $"{cleanBaseName}.{cleanExtension}";
            alternatives.Add(cleanName);
        }
        
        // 予約語の場合
        if (ReservedNames.Contains(baseName))
        {
            alternatives.Add($"{baseName}_");
            alternatives.Add($"MY_{baseName}");
        }
        
        return alternatives.Distinct().ToList();
    }
    
    /// <summary>
    /// ファイル名が N88-BASIC の規則に準拠しているかを簡易チェック
    /// </summary>
    /// <param name="fileName">チェックするファイル名</param>
    /// <returns>準拠している場合 true</returns>
    public static bool IsValidFileName(string fileName)
    {
        return ValidateFileName(fileName).IsValid;
    }
}

/// <summary>
/// ファイル名検証結果
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; private set; }
    public bool HasWarning { get; private set; }
    public string BaseName { get; private set; } = string.Empty;
    public string Extension { get; private set; } = string.Empty;
    public string ErrorMessage { get; private set; } = string.Empty;
    public string WarningMessage { get; private set; } = string.Empty;
    
    private ValidationResult() { }
    
    public static ValidationResult Success(string baseName, string extension)
    {
        return new ValidationResult
        {
            IsValid = true,
            BaseName = baseName,
            Extension = extension
        };
    }
    
    public static ValidationResult Warning(string baseName, string extension, string warning)
    {
        return new ValidationResult
        {
            IsValid = true,
            HasWarning = true,
            BaseName = baseName,
            Extension = extension,
            WarningMessage = warning
        };
    }
    
    public static ValidationResult Error(string errorMessage)
    {
        return new ValidationResult
        {
            IsValid = false,
            ErrorMessage = errorMessage
        };
    }
    
    public override string ToString()
    {
        if (!IsValid)
            return $"エラー: {ErrorMessage}";
        
        var result = $"{BaseName}" + (string.IsNullOrEmpty(Extension) ? "" : $".{Extension}");
        if (HasWarning)
            result += $" (警告: {WarningMessage})";
        
        return result;
    }
}