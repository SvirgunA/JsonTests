using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;

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

    public static class FieldsExtensions
    {
        public static ResultFieldsNode ToTree(this List<ResultField> fieldsList)
        {
            var tree = new ResultFieldsNode();
            foreach (var field in fieldsList)
            {
                var path = field.FieldPath.Split('.');
                var node = tree;
                foreach (var p in path)
                {
                    var type = p.Contains("[]") ? NodeType.Array : NodeType.Field;
                    node = node.CreateChild(p.Replace("[]", string.Empty), type);
                }
            }

            return tree;
        }
    }

    public static class JObjectExtensions
    {
        public static List<dynamic> TransformObject(this JToken obj, ResultFieldsNode transformationTree)
        {
            dynamic levelObj = new object();
            List<dynamic> levelObjs = new List<dynamic>();

            foreach (var name in transformationTree.ChildrenNames)
            {
                if (!transformationTree[name].HasKids)
                {
                    var value = obj.SelectToken(name, false);
                    SetObjectProperty(name, value, levelObj);
                }
                else
                {
                    var list = transformationTree[name].Type == NodeType.Field
                        ? TransformObject(obj.SelectToken(name), transformationTree[name])
                        : obj.SelectToken(name)?
                            .Children()
                            .Select(childObj =>
                                TransformObject(childObj, transformationTree[name]))
                            .SelectMany(l => l)
                            .ToList();
                    if (!levelObjs!.Any())
                    {
                        levelObjs = list;
                    }
                    else
                    {
                        levelObjs = list!
                            .Select(el => levelObjs
                                .Select(lo => Merge(lo, el)))
                            .SelectMany(el => el)
                            .ToList();
                    }
                }
            }

            if (levelObjs!.Any())
            {
                return levelObjs.Select(lo => Merge(lo, levelObj)).ToList();
            }

            levelObjs.Add(levelObj);
            return levelObjs;
        }

        private static dynamic Merge(object item1, object item2)
        {
            IDictionary<string, object> result = new ExpandoObject();

            foreach (var property in item1.GetType().GetProperties())
            {
                if (property.CanRead)
                    result[property.Name] = property.GetValue(item1);
            }

            foreach (var property in item2.GetType().GetProperties())
            {
                if (property.CanRead)
                    result[property.Name] = property.GetValue(item2);
            }

            return result;
        }

        private static void SetObjectProperty(string propertyName, object value, object obj)
        {
            PropertyInfo propertyInfo = obj.GetType().GetProperty(propertyName);
            // make sure object has the property we are after
            if (propertyInfo != null)
            {
                propertyInfo.SetValue(obj, value, null);
            }
        }
    }
}