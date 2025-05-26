using System.Text.RegularExpressions;

namespace Legacy89DiskKit.FileSystem.Infrastructure.Utility;

/// <summary>
/// MSX-DOS ファイル名検証・正規化ユーティリティ
/// </summary>
public static class MsxDosFileNameValidator
{
    /// <summary>
    /// ファイル名の最大長（拡張子除く）
    /// </summary>
    private const int MaxFileNameLength = 8;
    
    /// <summary>
    /// 拡張子の最大長
    /// </summary>
    private const int MaxExtensionLength = 3;
    
    /// <summary>
    /// 禁止文字のパターン
    /// </summary>
    private static readonly Regex InvalidCharsRegex = new Regex(@"[\x00-\x1F\x7F""*+,/:;<=>?[\\\]|]");
    
    /// <summary>
    /// DOS予約名
    /// </summary>
    private static readonly HashSet<string> ReservedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "CON", "PRN", "AUX", "NUL",
        "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
        "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
    };
    
    /// <summary>
    /// ファイル名検証結果
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; } = "";
        public string BaseName { get; set; } = "";
        public string Extension { get; set; } = "";
        public string NormalizedName { get; set; } = "";
    }
    
    /// <summary>
    /// ファイル名を検証する
    /// </summary>
    /// <param name="fileName">検証対象のファイル名</param>
    /// <returns>検証結果</returns>
    public static ValidationResult ValidateFileName(string fileName)
    {
        var result = new ValidationResult();
        
        if (string.IsNullOrWhiteSpace(fileName))
        {
            result.ErrorMessage = "ファイル名が空です";
            return result;
        }
        
        // ファイル名の正規化
        var normalizedFileName = NormalizeFileName(fileName);
        
        // 拡張子の分離
        var parts = SplitFileName(normalizedFileName);
        var baseName = parts.baseName;
        var extension = parts.extension;
        
        // 基本ファイル名の検証
        if (string.IsNullOrWhiteSpace(baseName))
        {
            result.ErrorMessage = "ファイル名が無効です";
            return result;
        }
        
        if (baseName.Length > MaxFileNameLength)
        {
            result.ErrorMessage = $"ファイル名が長すぎます（最大{MaxFileNameLength}文字）";
            return result;
        }
        
        if (extension.Length > MaxExtensionLength)
        {
            result.ErrorMessage = $"拡張子が長すぎます（最大{MaxExtensionLength}文字）";
            return result;
        }
        
        // 禁止文字チェック
        if (InvalidCharsRegex.IsMatch(baseName) || InvalidCharsRegex.IsMatch(extension))
        {
            result.ErrorMessage = "ファイル名に使用できない文字が含まれています";
            return result;
        }
        
        // スペース・ピリオドのチェック
        if (baseName.EndsWith(' ') || baseName.EndsWith('.'))
        {
            result.ErrorMessage = "ファイル名の最後にスペースまたはピリオドは使用できません";
            return result;
        }
        
        if (extension.EndsWith(' ') || extension.EndsWith('.'))
        {
            result.ErrorMessage = "拡張子の最後にスペースまたはピリオドは使用できません";
            return result;
        }
        
        // DOS予約名チェック
        if (ReservedNames.Contains(baseName))
        {
            result.ErrorMessage = $"'{baseName}'は予約語のため使用できません";
            return result;
        }
        
        // 成功
        result.IsValid = true;
        result.BaseName = baseName;
        result.Extension = extension;
        result.NormalizedName = string.IsNullOrEmpty(extension) ? baseName : $"{baseName}.{extension}";
        
        return result;
    }
    
    /// <summary>
    /// ファイル名を正規化する
    /// </summary>
    /// <param name="fileName">元のファイル名</param>
    /// <returns>正規化されたファイル名</returns>
    public static string NormalizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return "";
        
        // トリム
        fileName = fileName.Trim();
        
        // 大文字に変換（MSX-DOSは大文字小文字を区別しない）
        fileName = fileName.ToUpperInvariant();
        
        // 全角英数字を半角に変換（MSX日本語環境対応）
        fileName = ConvertFullWidthToHalfWidth(fileName);
        
        return fileName;
    }
    
    /// <summary>
    /// ファイル名と拡張子を分離する
    /// </summary>
    /// <param name="fileName">ファイル名</param>
    /// <returns>分離されたファイル名と拡張子</returns>
    public static (string baseName, string extension) SplitFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return ("", "");
        
        var lastDotIndex = fileName.LastIndexOf('.');
        
        if (lastDotIndex == -1 || lastDotIndex == 0)
        {
            // 拡張子なし、または最初がピリオド
            return (fileName, "");
        }
        
        var baseName = fileName.Substring(0, lastDotIndex);
        var extension = fileName.Substring(lastDotIndex + 1);
        
        return (baseName, extension);
    }
    
    /// <summary>
    /// 無効なファイル名の修正候補を生成する
    /// </summary>
    /// <param name="invalidFileName">無効なファイル名</param>
    /// <returns>修正候補のリスト</returns>
    public static List<string> GenerateAlternatives(string invalidFileName)
    {
        var alternatives = new List<string>();
        
        if (string.IsNullOrWhiteSpace(invalidFileName))
            return alternatives;
        
        var normalized = NormalizeFileName(invalidFileName);
        var (baseName, extension) = SplitFileName(normalized);
        
        // 基本的な修正
        baseName = SanitizeFileName(baseName);
        extension = SanitizeFileName(extension);
        
        // 長さ調整
        if (baseName.Length > MaxFileNameLength)
        {
            // 段階的な短縮候補
            for (int len = MaxFileNameLength; len >= Math.Min(3, MaxFileNameLength); len--)
            {
                var truncatedName = baseName.Substring(0, len);
                var candidate = string.IsNullOrEmpty(extension) ? truncatedName : $"{truncatedName}.{extension}";
                
                var validation = ValidateFileName(candidate);
                if (validation.IsValid)
                {
                    alternatives.Add(validation.NormalizedName);
                }
            }
        }
        
        if (extension.Length > MaxExtensionLength)
        {
            extension = extension.Substring(0, MaxExtensionLength);
        }
        
        // 予約語対応
        if (ReservedNames.Contains(baseName))
        {
            for (int i = 1; i <= 99; i++)
            {
                var newBaseName = $"{baseName}{i:D2}";
                if (newBaseName.Length <= MaxFileNameLength)
                {
                    var candidate = string.IsNullOrEmpty(extension) ? newBaseName : $"{newBaseName}.{extension}";
                    
                    var validation = ValidateFileName(candidate);
                    if (validation.IsValid)
                    {
                        alternatives.Add(validation.NormalizedName);
                        break;
                    }
                }
            }
        }
        
        // 基本候補
        if (baseName.Length <= MaxFileNameLength && extension.Length <= MaxExtensionLength)
        {
            var candidate = string.IsNullOrEmpty(extension) ? baseName : $"{baseName}.{extension}";
            
            var validation = ValidateFileName(candidate);
            if (validation.IsValid)
            {
                alternatives.Add(validation.NormalizedName);
            }
        }
        
        return alternatives.Distinct().ToList();
    }
    
    /// <summary>
    /// ファイル名から無効文字を除去・置換する
    /// </summary>
    /// <param name="fileName">ファイル名</param>
    /// <returns>サニタイズされたファイル名</returns>
    private static string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return "";
        
        // 無効文字を下線に置換
        fileName = InvalidCharsRegex.Replace(fileName, "_");
        
        // 末尾のスペース・ピリオドを除去
        fileName = fileName.TrimEnd(' ', '.');
        
        return fileName;
    }
    
    /// <summary>
    /// 全角英数字を半角に変換（日本語MSX対応）
    /// </summary>
    /// <param name="input">入力文字列</param>
    /// <returns>半角変換された文字列</returns>
    private static string ConvertFullWidthToHalfWidth(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;
        
        var result = new StringBuilder(input.Length);
        
        foreach (char c in input)
        {
            // 全角英数字を半角に変換
            if (c >= '０' && c <= '９')
            {
                result.Append((char)(c - '０' + '0'));
            }
            else if (c >= 'Ａ' && c <= 'Ｚ')
            {
                result.Append((char)(c - 'Ａ' + 'A'));
            }
            else if (c >= 'ａ' && c <= 'ｚ')
            {
                result.Append((char)(c - 'ａ' + 'a'));
            }
            else
            {
                result.Append(c);
            }
        }
        
        return result.ToString();
    }
    
    /// <summary>
    /// DOSファイル名形式（8.3形式）に変換する
    /// </summary>
    /// <param name="fileName">ファイル名</param>
    /// <returns>DOS形式ファイル名（11文字、スペースパディング）</returns>
    public static string ToDosFileName(string fileName)
    {
        var validation = ValidateFileName(fileName);
        if (!validation.IsValid)
            throw new ArgumentException($"無効なファイル名: {validation.ErrorMessage}");
        
        var baseName = validation.BaseName.PadRight(8);
        var extension = validation.Extension.PadRight(3);
        
        return baseName + extension;
    }
    
    /// <summary>
    /// DOS形式ファイル名（11文字）から通常のファイル名に変換する
    /// </summary>
    /// <param name="dosFileName">DOS形式ファイル名（11文字）</param>
    /// <returns>通常のファイル名</returns>
    public static string FromDosFileName(string dosFileName)
    {
        if (string.IsNullOrEmpty(dosFileName) || dosFileName.Length != 11)
            throw new ArgumentException("DOS形式ファイル名は11文字である必要があります");
        
        var baseName = dosFileName.Substring(0, 8).TrimEnd();
        var extension = dosFileName.Substring(8, 3).TrimEnd();
        
        if (string.IsNullOrEmpty(extension))
            return baseName;
        
        return $"{baseName}.{extension}";
    }
}