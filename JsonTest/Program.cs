using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonTest
{
    class Program
    {
        private static TreeNode<string> tree = new TreeNode<string>("root");
        //private static string[] _basis = new string[0];

        private static Pattern Pattern;

        static void Main(string[] args)
        {
            var patternString = File.ReadAllText("Pattern.json");
            Pattern = JsonConvert.DeserializeObject<Pattern>(patternString);
            
            
            using (var reader = new StreamReader("InboundData.json"))
            using (var jsonReader = new JsonTextReader(reader))
            {
                var root = JToken.Load(jsonReader);
                GenerateTree(root, tree);
                
                var result = GenerateDepthMatrix();
                var json = JsonConvert.SerializeObject(result);
                var fileName = $"{Guid.NewGuid()}.json";
                System.IO.File.WriteAllText(@"D:\2\"+fileName, json);
                PrintTree();
            }
        }

        static List<IDictionary<string, Object>> GenerateDepthMatrix()
        {
            var baseElement = Pattern.Root.First(p => p.IsBase);

            var elements = tree.FindTreeNodes(p => 
                p.Data == baseElement.FieldName 
                && p.Level == baseElement.Level 
                && p.Parent.NodeName == baseElement.ParentName);

            var objList = new List<IDictionary<string, Object>>();
            foreach (var element in elements)
            {
                var obj = new Dictionary<string, Object>();

                foreach (var patternField in Pattern.Root)
                {
                    if (patternField.FieldName == element.Data)
                    {
                        obj.Add(patternField.ObjectName, ParsingFactory.GetElementData(element));
                    }
                    else
                    {
                        var action = new ParsingFactory().GetAction(
                            patternField.Type,
                            element,
                            patternField,
                            out var data);
                        if(action)
                            obj.Add(patternField.ObjectName, data);
                    }
                }

                objList.Add(obj);
            }

            return objList;
        }

        static void PrintTree()
        {
            foreach (TreeNode<string> node in tree)
            {
                string indent = CreateIndent(node.Level);
                Console.WriteLine(indent + (node.Data ?? "null"));
            }
        }

        static JToken[] GetNodes(JToken obj, string path)
        {
            var first = path.GetFirstEl();
            //obj.Children().Where(p => p)

            return null;
        }

        static void GenerateTree(JToken obj, TreeNode<string> node)
        {
            // iterate fields in JToken
            foreach (var child in obj.Children())
            {
                // child = key value pair
                if ((child as JProperty) != null)
                {
                    // if value is true value
                    if ((child.First as JValue) != null)
                    {
                        // add node with key 
                        node.AddChild((child as JProperty).Name)
                            // and add leaf with value.
                            .AddChild((child.First as JValue)?.Value.ToString());
                    }
                    // if value is object or array
                    else
                    {
                        GenerateTree(child, node);
                    }
                }
                // if child is array
                else
                {
                    // add node and recursion 
                    GenerateTree(child, node.AddChild(child.Path));
                }
            }
        }

        private static String CreateIndent(int depth)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < depth; i++)
            {
                sb.Append(' ');
            }

            return sb.ToString();
        }
    }
}