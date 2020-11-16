using System;
using System.Collections.Generic;
using System.Linq;

namespace JsonTest
{
    public class ParsingFactory
    {
        public bool GetAction(
            NodeType type,
            TreeNode<string> treeEl, 
            PatternElement patternEl, 
            out object data)
        {
            switch (type)
            {
                case NodeType.Field:
                    return GetElement(treeEl, patternEl, out data);
                case NodeType.Array:
                    return GetInnerArray(treeEl, patternEl, out data);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
        
        public static string GetElementData(TreeNode<string> el)
        {
            if (el.Children.Count == 1 && el.Children.First().IsLeaf)
            {
                return el.Children.First().Data;
            }

            return null;
        }
        
        private bool GetInnerArray(TreeNode<string> treeEl, PatternElement patternEl, out object data)
        {
            data = null;
            var result = new List<string>();
            var innerElements = treeEl.Parent.FindTreeNodes(p => 
                p.Data == patternEl.FieldName 
                && p.Level == patternEl.Level-1 
                && p.Parent.NodeName == patternEl.ParentName);
            foreach (var element in innerElements)
            {
                var elData = GetElementData(element);
                result.Add(elData);
            }

            data = result.ToArray();
            return true;
        }
        
        private bool GetElement(TreeNode<string> treeEl, PatternElement patternEl, out object data)
        {
            data = string.Empty;
            if (treeEl.IsRoot) return false;
            if (treeEl.Level <= patternEl.Level)
            {
                var requiredElement = treeEl.Parent.FindTreeNode(p => 
                    p.Data == patternEl.FieldName 
                    && p.Level == patternEl.Level 
                    && p.Parent.NodeName == patternEl.ParentName);
                data = requiredElement != null ? GetElementData(requiredElement) : null;
                return data != null;
            } 
            
            if (treeEl.Level > patternEl.Level)
            {
                var upperNode = LevelUp(treeEl, treeEl.Level - patternEl.Level);
                
                var requiredElement =
                    upperNode.Parent.FindTreeNode(p => 
                        p.Data == patternEl.FieldName 
                        && p.Level == patternEl.Level 
                        && p.Parent.NodeName == patternEl.ParentName);
                data = requiredElement != null ? GetElementData(requiredElement) : null;
                return data != null;
            }
            return true;
        }
        
        private static TreeNode<string> LevelUp(TreeNode<string> node, int steps)
        {
            for (int i = 0; i < steps; i++)
            {
                if (node.IsRoot) return node;
                node = node.Parent;
            }

            return node;
        }
        
        
    }
}