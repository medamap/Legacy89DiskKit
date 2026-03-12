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

        // Initial shortening
        string currentBase = ShortenByBytes(basePart, encoder, maxBase);
        string candidate = BuildCandidate(currentBase, extPart);

        if (existingNames == null || !existingNames.Contains(candidate.ToUpperInvariant()))
        {
            return candidate;
        }

        // Collision Handling (Tilde approach)
        for (int i = 1; i < 100; i++)
        {
            string suffix = $"~{i}";
            int limit = maxBase - suffix.Length;
            if (limit < 1) limit = 1;

            string shortenedBase = ShortenByBytes(basePart, encoder, limit);
            candidate = BuildCandidate(shortenedBase + suffix, extPart);

            if (!existingNames.Contains(candidate.ToUpperInvariant()))
            {
                return candidate;
            }
        }

        return candidate;
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
