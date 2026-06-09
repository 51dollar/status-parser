using StatusParser.Console.Configuration;

namespace StatusParser.Console.Services;

public static class DuplicateRemover
{
    public static List<object[]> RemoveDuplicateRows(List<object[]> rows, int articleColIndex, ILogger logger)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var result = new List<object[]>(rows.Count);

        foreach (var row in rows)
        {
            var value = row[articleColIndex]?.ToString()?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(value) && !seen.Add(value))
                continue;

            result.Add(row);
        }

        var removed = rows.Count - result.Count;
        if (removed > 0)
            logger.Info($"  Removed {removed} duplicate rows (by Артикул)");

        return result;
    }

    public static int FindColumnIndexByTargetName(ColumnMappingRule[] rules, string targetName)
    {
        for (var i = 0; i < rules.Length; i++)
            if (string.Equals(rules[i].TargetColumnName, targetName, StringComparison.OrdinalIgnoreCase))
                return i;
        return -1;
    }
}
