namespace SWIMS.Models
{
    public class form_FieldAttributes
    {
        public string type { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string label { get; set; } = string.Empty;
        public string data { get; set; } = string.Empty;
    }

    public class FormTableViewModel
    {
        public string FormName { get; set; } = string.Empty;
        public List<ColumnMap> Columns { get; set; } = new();
        public List<Dictionary<string, string>> Rows { get; set; } = new(); // key=column, value=data
    }

    public class ColumnMap
    {
        public string ColumnName { get; set; } = string.Empty; // FormData01, FormData02
        public string Label { get; set; } = string.Empty;      // Text Field, City, etc.
    }
}
