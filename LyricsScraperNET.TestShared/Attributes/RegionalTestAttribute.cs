using System;
using System.Globalization;
using System.Linq;
using Xunit;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class RegionalTestTheoryAttribute : TheoryAttribute
{
    public RegionalTestTheoryAttribute(string[] includeRegions = null, string[] excludeRegions = null)
    {
        if (!ShouldRunTest(includeRegions, excludeRegions))
        {
            Skip = GenerateSkipMessage(includeRegions, excludeRegions);
        }
    }

    private static bool ShouldRunTest(string[] includeRegions, string[] excludeRegions)
    {
        var currentRegion = RegionInfo.CurrentRegion.Name;
        
        // If regions are specified to be included, run only if the current region is in the list.
        if (includeRegions != null && includeRegions.Length > 0)
        {
            if (!includeRegions.Any(code => string.Equals(currentRegion, code, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }
        }

        // If regions are specified to be excluded, do not run if the current region is in the list.
        if (excludeRegions != null && excludeRegions.Length > 0)
        {
            if (excludeRegions.Any(code => string.Equals(currentRegion, code, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }
        }

        return true;
    }

    private static string GenerateSkipMessage(string[] includeRegions, string[] excludeRegions)
    {
        var includeMessage = includeRegions != null && includeRegions.Length > 0
            ? $"Include: {string.Join(", ", includeRegions)}."
            : string.Empty;

        var excludeMessage = excludeRegions != null && excludeRegions.Length > 0
            ? $"Exclude: {string.Join(", ", excludeRegions)}."
            : string.Empty;

        return $"Test skipped. {includeMessage} {excludeMessage}".Trim();
    }
}