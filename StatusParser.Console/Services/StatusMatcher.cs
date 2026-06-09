using StatusParser.Console.Configuration;

namespace StatusParser.Console.Services;

public static class StatusMatcher
{
    public static Dictionary<string, string> BuildPatternLookup(NewStatusMappingRule[] rules)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var rule in rules)
            foreach (var pattern in rule.SourceValuePatterns)
                map.TryAdd(pattern.Trim(), rule.TargetNewStatusValue);
        return map;
    }

    public static string? MatchStatus(string? sourceValue, NewStatusMappingRule[] rules)
    {
        if (string.IsNullOrWhiteSpace(sourceValue))
            return null;

        foreach (var rule in rules)
            foreach (var pattern in rule.SourceValuePatterns)
                if (string.Equals(sourceValue.Trim(), pattern.Trim(), StringComparison.OrdinalIgnoreCase))
                    return rule.TargetNewStatusValue;

        return null;
    }
}
