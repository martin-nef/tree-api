using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using tree_api.Interfaces;

namespace tree_api.Models {
    public class Node {
        public Node () { }
        public Node (string name, params Node[] nodes) : this () {
            Name = name;
            if (nodes != null && nodes.Length > 0) {
                Children = nodes.ToList<Node> ();
            }
        }
        public string Name { get; set; }
        public string Value { get; set; }
        public int Height { get; set; }

        public bool IsLeaf => Children.Any () == false;
        public bool IsRoot => Parent == null;

        [JsonIgnore]
        public Node Root => IsRoot ? this : ((Node) Parent).Root;

        [JsonIgnore]
        public Node Parent { get; set; }
        public IList<Node> Children { get; set; } = new List<Node> ();

        /// <summary>
        /// If looking for current node, return self.
        /// Otherwise, look through each of own children in depth-first order until the needed node is found.
        /// </summary>
        /// <param name="name">Node.Name to look for</param>
        /// <returns>Node</returns>
        public Node GetNodeByName (string name) {
            var node = _GetNodeByName (name);
            if (node != null) {
                return node;
            } else {
                throw new KeyNotFoundException ($"Could not find a node named '{name}'");
            }
        }

        /// Worst case time complexity here is O(n) where 'n' is the number of nodes in the tree.
        public Node _GetNodeByName (string name) {
            if (this.Name == name) {
                return this;
            } else {
                for (int i = 0; i < Children.Count; i++) {
                    var child = Children[i];
                    var nodeFound = child.GetNodeByName (name);
                    if (nodeFound != null) {
                        return nodeFound;
                    }
                }
                return null;
            }
        }

        public void AddChild (Node node) {
            // Does Name have to be unique? If so, need to check
            // The check makes the insertion time complexity O(n), the same as GetNodeByName's (complexity is O(1) - without the check)
            if (Root.GetNodeByName (node.Name) != null) {
                throw new ArgumentException ($"Failed to add a child node named {node?.Name}, as another node with that name is in the tree");
            }
            Children.Add (node);
        }

        // Same time complexity as GetNodeByName
        public void DeleteNodeByName (string name) {
            var node = GetNodeByName (name: name);
            if (node.IsRoot) {
                // Can make another node root instead, as an option. Need to decide which child becomes the new root first though.
                throw new NotSupportedException ("Cannot delete the root node");
            }
            // Linear in number of children, so worst case O(n);
            ((Node) node).Parent.Children.Remove (node);
        }

        public void UpdateWith (Node node) {
            var _value = Value;
            Value = node.Value;
            var _children = Children;
            Children = node.Children;
            // If names need to be unique
            if (!CheckNamesUnique ()) {
                Value = _value;
                Children = _children;
                throw new Exception ($"Failed to update node named '{node?.Name}', duplicate names detected in the tree");
            }
        }

        public bool CheckNamesUnique () {
            if (!IsRoot) {
                return ((Node) Root).CheckNamesUnique ();
            } else {
                var names = new Dictionary<string, bool> ();
                return _CheckNamesUnique (ref names);
            }
        }

        internal bool _CheckNamesUnique (ref Dictionary<string, bool> names) {
            for (int i = 0; i < Children.Count; i++) {
                var child = Children[i];
                if (names.ContainsKey (child.Name)) {
                    return false;
                } else if (!((Node) child)._CheckNamesUnique (ref names)) {
                    return false;
                } else {
                    names[child.Name] = true;
                }
            }
            return true;
        }
    }
}