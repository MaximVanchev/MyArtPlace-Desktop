using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace MyArtPlace.ViewModels.Reusable;

/// <summary>
/// Represents a single dynamic filter for a property of a given type.
/// Requirement 4: Reusable filter control generation.
/// </summary>
public class FilterField
{
    public string PropertyName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public Type PropertyType { get; set; } = typeof(string);
    public object? Value { get; set; }
    public bool IsEnum { get; set; }
    public string[]? EnumValues { get; set; }
}

/// <summary>
/// Builds filter fields dynamically from a class type and specified property names.
/// For each property, determines the appropriate control type based on the property type.
/// Requirement 4a: Appropriate control per type.
/// Requirement 4b: Appropriate label/description per control.
/// Requirement 4c: Values used to form a query.
/// Requirement 4d: Demonstrated for two different tables.
/// </summary>
public static class FilterBuilder
{
    public static List<FilterField> BuildFilters<T>(params string[] propertyNames)
    {
        return BuildFilters(typeof(T), propertyNames);
    }

    public static List<FilterField> BuildFilters(Type entityType, params string[] propertyNames)
    {
        var filters = new List<FilterField>();

        foreach (var propName in propertyNames)
        {
            var prop = entityType.GetProperty(propName);
            if (prop is null) continue;

            var displayAttr = prop.GetCustomAttribute<DisplayAttribute>();
            var displayName = displayAttr?.Name ?? AddSpacesToPascalCase(prop.Name);

            var realType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
            var isEnum = realType.IsEnum;

            filters.Add(new FilterField
            {
                PropertyName = prop.Name,
                DisplayName = displayName,
                PropertyType = realType,
                IsEnum = isEnum,
                EnumValues = isEnum ? Enum.GetNames(realType) : null
            });
        }

        return filters;
    }

    /// <summary>
    /// Filters a list of entities by applying all non-null filter values.
    /// String: contains (case‐insensitive).
    /// Numeric: equals.
    /// Enum: equals (by name).
    /// </summary>
    public static List<T> ApplyFilters<T>(IEnumerable<T> source, List<FilterField> filters)
    {
        var result = source.AsEnumerable();

        foreach (var filter in filters)
        {
            if (filter.Value is null) continue;
            var valStr = filter.Value.ToString();
            if (string.IsNullOrWhiteSpace(valStr)) continue;

            var prop = typeof(T).GetProperty(filter.PropertyName);
            if (prop is null) continue;

            if (filter.IsEnum)
            {
                result = result.Where(item =>
                {
                    var v = prop.GetValue(item);
                    return v is not null && v.ToString() == valStr;
                });
            }
            else if (filter.PropertyType == typeof(string))
            {
                result = result.Where(item =>
                {
                    var v = prop.GetValue(item)?.ToString();
                    return v is not null && v.Contains(valStr, StringComparison.OrdinalIgnoreCase);
                });
            }
            else if (filter.PropertyType == typeof(int) || filter.PropertyType == typeof(decimal) || filter.PropertyType == typeof(double))
            {
                result = result.Where(item =>
                {
                    var v = prop.GetValue(item);
                    return v is not null && v.ToString() == valStr;
                });
            }
        }

        return result.ToList();
    }

    private static string AddSpacesToPascalCase(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        var result = new System.Text.StringBuilder();
        result.Append(text[0]);
        for (int i = 1; i < text.Length; i++)
        {
            if (char.IsUpper(text[i])) result.Append(' ');
            result.Append(text[i]);
        }
        return result.ToString();
    }
}
