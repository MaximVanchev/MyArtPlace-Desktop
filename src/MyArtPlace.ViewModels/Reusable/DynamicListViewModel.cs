using System.Collections.ObjectModel;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MyArtPlace.ViewModels.Reusable;

/// <summary>
/// Column definition for dynamic list display.
/// </summary>
public class ColumnDefinition
{
    public string PropertyName { get; set; } = string.Empty;
    public string Header { get; set; } = string.Empty;
}

/// <summary>
/// Wraps an object with its visible column values for display.
/// Requirement 5d: full object is retained regardless of displayed columns.
/// </summary>
public class DynamicRow
{
    public object FullObject { get; }
    public List<string> DisplayValues { get; }

    public DynamicRow(object fullObject, List<string> displayValues)
    {
        FullObject = fullObject;
        DisplayValues = displayValues;
    }
}

/// <summary>
/// Reusable ViewModel for displaying a dynamic list of objects with configurable columns.
/// Requirement 5a: Set list of objects. 
/// Requirement 5b: Specify which columns to display.
/// Requirement 5c: Dynamically change columns.
/// Requirement 5d: Return full object on selection.
/// Requirement 5e: Clear and set new list.
/// Requirement 5f: Demonstrated for two different tables.
/// </summary>
public partial class DynamicListViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<ColumnDefinition> _columns = new();

    [ObservableProperty]
    private ObservableCollection<DynamicRow> _rows = new();

    [ObservableProperty]
    private DynamicRow? _selectedRow;

    /// <summary>Returns the full object of the selected row (req 5d).</summary>
    public T? GetSelectedObject<T>() where T : class
    {
        return SelectedRow?.FullObject as T;
    }

    /// <summary>
    /// Sets the data source and visible columns (req 5a, 5b).
    /// </summary>
    public void SetData<T>(IEnumerable<T> items, params string[] propertyNames) where T : class
    {
        var cols = new ObservableCollection<ColumnDefinition>();
        foreach (var propName in propertyNames)
        {
            var prop = typeof(T).GetProperty(propName);
            if (prop is null) continue;
            cols.Add(new ColumnDefinition
            {
                PropertyName = propName,
                Header = AddSpacesToPascalCase(propName)
            });
        }

        Columns = cols;
        BuildRows(items);
    }

    /// <summary>
    /// Changes which columns are visible without changing the data (req 5c).
    /// </summary>
    public void ChangeColumns<T>(params string[] propertyNames) where T : class
    {
        var cols = new ObservableCollection<ColumnDefinition>();
        foreach (var propName in propertyNames)
        {
            var prop = typeof(T).GetProperty(propName);
            if (prop is null) continue;
            cols.Add(new ColumnDefinition
            {
                PropertyName = propName,
                Header = AddSpacesToPascalCase(propName)
            });
        }

        Columns = cols;

        // Rebuild rows with new columns
        var objects = Rows.Select(r => r.FullObject).ToList();
        var newRows = new ObservableCollection<DynamicRow>();
        foreach (var obj in objects)
        {
            var vals = new List<string>();
            foreach (var col in Columns)
            {
                var prop = obj.GetType().GetProperty(col.PropertyName);
                vals.Add(prop?.GetValue(obj)?.ToString() ?? "");
            }
            newRows.Add(new DynamicRow(obj, vals));
        }
        Rows = newRows;
    }

    /// <summary>
    /// Clears current data and sets a new list (req 5e).
    /// </summary>
    public void Clear()
    {
        Columns.Clear();
        Rows.Clear();
        SelectedRow = null;
    }

    private void BuildRows<T>(IEnumerable<T> items) where T : class
    {
        var newRows = new ObservableCollection<DynamicRow>();
        foreach (var item in items)
        {
            var vals = new List<string>();
            foreach (var col in Columns)
            {
                var prop = typeof(T).GetProperty(col.PropertyName);
                vals.Add(prop?.GetValue(item)?.ToString() ?? "");
            }
            newRows.Add(new DynamicRow(item, vals));
        }
        Rows = newRows;
        SelectedRow = null;
    }

    private static string AddSpacesToPascalCase(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        var sb = new System.Text.StringBuilder();
        sb.Append(text[0]);
        for (int i = 1; i < text.Length; i++)
        {
            if (char.IsUpper(text[i])) sb.Append(' ');
            sb.Append(text[i]);
        }
        return sb.ToString();
    }
}
