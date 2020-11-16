using System.Collections.Generic;
using System.Linq;

namespace JsonTest
{
    public class ResultField
    {
        public string FieldName { get; set; }

        public string FieldPath { get; set; }

        public NodeType Type => FieldPath.EndsWith("[]") ? NodeType.Array : NodeType.Field;
    }

    public class ResultFieldsNode
    {
        private string _name { get; set; }

        private NodeType _type { get; set; }

        private ResultFieldsNode _parent { get; set; }

        private Dictionary<string, ResultFieldsNode> _children { get; }

        public ResultFieldsNode()
        {
            _name = "root";
            _children = new Dictionary<string, ResultFieldsNode>();
        }

        public string Name => _name;

        public NodeType Type => _type;

        public string[] ChildrenNames => _children.Keys.ToArray();

        public bool HasKids => _children.Any();
        
        public ResultFieldsNode this[string name] => _children.ContainsKey(name) ? _children[name] : null;

        public ResultFieldsNode CreateChild(string name, NodeType type)
        {
            if (!_children.ContainsKey(name))
            {
                _children.Add(name, new ResultFieldsNode
                {
                    _name = name,
                    _type = type,
                    _parent = this,
                });
            }

            return _children[name];
        }
    }
}