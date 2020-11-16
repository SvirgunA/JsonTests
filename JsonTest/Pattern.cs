using System.Linq;

namespace JsonTest
{
    public class Pattern
    {
        public PatternElement[] Root { get; set; }
    }

    public class PatternElement
    {
        public string ObjectName { get; set; }
        public bool IsBase { get; set; }

        public string FieldPath { get; set; }

        public string FieldName => FieldPath.Split('.').Last().Replace("[]", "");

        public int Level => FieldPath.Split('.').Length + FieldPath.Count(p => p == '[');

        public NodeType Type => FieldPath.EndsWith("[]") ? NodeType.Array : NodeType.Field;

        public string ParentName
        {
            get
            {
                var a = FieldPath.Split('.');
                return a.Length > 1 ? a[^2].Replace("[]", "") : "root";
            }
        }
    }
}
