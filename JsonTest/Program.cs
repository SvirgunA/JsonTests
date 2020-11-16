using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonTest
{
    class Program
    {
        //private static string[] _basis = new string[0];

        private static List<ResultField> _fields;

        static void Main(string[] args)
        {
            var fieldsString = File.ReadAllText("Fields.json");
            _fields = JsonConvert.DeserializeObject<List<ResultField>>(fieldsString);

            using var reader = new StreamReader("InboundData.json");
            using var jsonReader = new JsonTextReader(reader);
            var inputTree = JToken.Load(jsonReader);

            var results = inputTree.TransformObject(_fields.ToTree());
        }
    }
}