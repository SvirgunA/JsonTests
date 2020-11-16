using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace JsonTest
{
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
                    node = node.CreateChild(p.Replace("[]", string.Empty), type, field.FieldName);
                }
            }

            return tree;
        }
    }

    public static class JObjectExtensions
    {
        public static List<JObject> TransformObject(this JToken obj, ResultFieldsNode transformationTree)
        {
            var levelObj = new JObject();
            var levelObjs = new List<JObject>();

            foreach (var name in transformationTree.ChildrenNames)
            {
                if (!transformationTree[name].HasKids)
                {
                    var value = obj.SelectToken(name, false);
                    levelObj[transformationTree[name].FieldName] = value;
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

        private static JObject Merge(JObject a, JObject b)
        {
            var tmp = (JObject)a.DeepClone();
            tmp.Merge(b);
            return tmp;
        }
    }
}