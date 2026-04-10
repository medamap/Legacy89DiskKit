using System.Reflection;

namespace Legacy89DiskKit.Cli;

public static class VersionDisplay
{
    public static string GetDisplayVersion()
    {
        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        var baseVersion = ResolveBaseVersion(assembly);
        var buildMoment = ResolveBuildMoment(assembly);
        return $"{baseVersion} build-{buildMoment:yyyyMMddHHmmss}-{buildMoment:fffff}";
    }

    public static string GetNormalizedVersion()
    {
        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        return ResolveBaseVersion(assembly);
    }

    private static string ResolveBaseVersion(Assembly assembly)
    {
        var informationalVersion = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;

        if (!string.IsNullOrWhiteSpace(informationalVersion))
        {
            return NormalizeVersion(informationalVersion);
        }

        var assemblyVersion = assembly.GetName().Version?.ToString();
        return string.IsNullOrWhiteSpace(assemblyVersion) ? "0.0.0" : NormalizeVersion(assemblyVersion);
    }

    private static DateTime ResolveBuildMoment(Assembly assembly)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(assembly.Location) && File.Exists(assembly.Location))
            {
                return File.GetLastWriteTime(assembly.Location);
            }
        }
        catch
        {
        }

        return DateTime.Now;
    }

    private static string NormalizeVersion(string value)
    {
        var normalized = value.Trim();
        if (normalized.StartsWith("v", StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized[1..];
        }

        var separatorIndex = normalized.IndexOfAny(['-', '+']);
        if (separatorIndex >= 0)
        {
            normalized = normalized[..separatorIndex];
        }

        return normalized;
    }
}
