using System.Linq;

namespace JsonTest
{
    public static class Extensions
    {
        public static string GetFirstEl(this string path)
        {
            var a = path.Split('.');
            return a[0].Replace("[]", "");
        }
        
        public static string GetChildrenPath(this string path)
        {
            var a = path.Split('.').Skip(1);
            return string.Join(".", a);
        }
    }
}