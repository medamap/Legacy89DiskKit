using System.Text.RegularExpressions;
using Legacy89DiskKit.Domain.CharacterEncoding.Interface;
using Legacy89DiskKit.Domain.CharacterEncoding.Interface.Registry;

namespace Legacy89DiskKit.Application.Services;

public class FileNameNormalizationService
{
    private readonly IEncoderRegistry _encoderRegistry;

    public FileNameNormalizationService(IEncoderRegistry encoderRegistry)
    {
        _encoderRegistry = encoderRegistry;
    }

    public string Normalize(string input, string platformId, int maxBase, int maxExt = 0, HashSet<string>? existingNames = null)
    {
        var encoder = _encoderRegistry.GetEncoder(platformId);
        if (encoder == null) throw new Exception($"Encoder for platform {platformId} not found.");

        string basePart = "";
        string extPart = "";

        if (maxExt > 0)
        {
            // Handle splitting (e.g. 8.3 or 6.3)
            int lastDot = input.LastIndexOf('.');
            if (lastDot > 0)
            {
                basePart = input.Substring(0, lastDot);
                extPart = input.Substring(lastDot + 1);
            }
            else
            {
                basePart = input;
            }
            
            basePart = Sanitize(basePart);
            extPart = Sanitize(extPart);
            
            extPart = ShortenByBytes(extPart, encoder, maxExt);
        }
        else
        {
            // Unified (e.g. 13 chars)
            basePart = Sanitize(input);
        }

        string originalBase = basePart;
        string currentBase = ShortenByBytes(basePart, encoder, maxBase);
        string candidate = BuildCandidate(currentBase, extPart);

        if (originalBase == currentBase && (existingNames == null || !existingNames.Contains(candidate.ToUpperInvariant())))
        {
            return candidate;
        }

        int prefixLength = Math.Max(1, maxBase - 3);
        string prefix = ShortenByBytes(basePart, encoder, prefixLength);

        for (int i = 1; i <= 999; i++)
        {
            string suffix = i.ToString("D3");
            string shortenedBase = prefix + suffix;
            candidate = BuildCandidate(shortenedBase, extPart);

            if (existingNames == null || !existingNames.Contains(candidate.ToUpperInvariant()))
            {
                return candidate;
            }
        }

        throw new Exception($"Failed to generate unique file name for '{input}' after 999 attempts.");
    }

    private string Sanitize(string input)
    {
        // Replace illegal characters with underscores
        return Regex.Replace(input, @"[<>:""/\\|?* .]", "_");
    }

    private string ShortenByBytes(string text, ICharacterEncoder encoder, int maxBytes)
    {
        if (string.IsNullOrEmpty(text)) return "";
        var bytes = encoder.EncodeText(text);
        if (bytes.Length <= maxBytes) return text;

        string result = text;
        while (encoder.EncodeText(result).Length > maxBytes && result.Length > 0)
        {
            result = result.Substring(0, result.Length - 1);
        }
        return result;
    }

    private string BuildCandidate(string baseName, string extension)
    {
        if (!string.IsNullOrEmpty(extension))
        {
            return $"{baseName}.{extension}";
        }
        return baseName;
    }
}
