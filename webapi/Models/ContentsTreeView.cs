using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Windows.Forms;

namespace webapi.Models
{
    public class ContentsTreeView<T> : TreeView
    {
        private T _contentObject;

        public T ContentObject
        {
            get { return _contentObject; }
            set
            {
                _contentObject = value;
                RefreshTree();
            }
        }

        public void RefreshTree()
        {
            Nodes.Clear();
            if (ContentObject != null)
            {
                var root = new HolderTreeNode("Root", ContentObject);
                Nodes.Add(root);
                Populate(root);
                ExpandAll();
            }
        }


        public Dictionary<string, ContextMenuStrip> NodesContextMenus { get; set; }

        public object SelectedContent { get { return ((HolderTreeNode)(SelectedNode)).Content; } }

        protected override void OnNodeMouseClick(TreeNodeMouseClickEventArgs e)
        {
            if (e.Node != null)
            {
                if (e.Button == MouseButtons.Right)
                {
                    if (NodesContextMenus != null)
                    {
                        SelectedNode = e.Node;
                        var pair =
                            NodesContextMenus.SingleOrDefault(p => p.Key == SelectedContent.GetType().Name);
                        if (pair.Key != null)
                            pair.Value.Show(this, e.Location);
                    }
                }
                //propertyGrid1.SelectedObject = ((HolderTreeNode)e.Node).Content;
            }
            base.OnNodeMouseClick(e);
        }

        private static string RestrictionNamespace { get { return typeof(T).Namespace; } }

        private static void Populate(HolderTreeNode node)
        {
            if (node.Content != null)
            {
                if (node.Content.GetType().Namespace == RestrictionNamespace)
                    foreach (var p in node.Content.GetType().GetProperties())
                    {
                        object val = p.GetValue(node.Content);
                        if (val != null)
                        {
                            Type t = val.GetType();
                            if ((p.Name == "Items") || ((t.Namespace == RestrictionNamespace) && t.IsClass))
                                if (t.IsArray)
                                {
                                    var i = 0;
                                    foreach (var item in (IEnumerable)val)
                                    {
                                        var n = new HolderTreeNode(p.Name + "[" + i + "]", item);
                                        node.Nodes.Add(n);
                                        Populate(n);
                                        i++;
                                    }
                                }
                                else
                                {
                                    var n = new HolderTreeNode(p.Name, val);
                                    node.Nodes.Add(n);
                                    Populate(n);
                                }
                        }
                    }
            }
        }
    }
    public class HolderTreeNode : TreeNode
    {
        public HolderTreeNode(string label, object c)
        {
            Label = label;
            Content = c;
        }
        private object _content;
        private string _label = "";

        public string Label
        {
            get { return _label; }
            set
            {
                _label = value;
                RefreshLabel();
            }
        }

        public object Content
        {
            get { return _content; }
            set
            {
                _content = value;
                RefreshLabel();
            }
        }

        private void RefreshLabel()
        {
            Text = _label;
            if (_content != null)
                Text += @" (" + _content.GetType().Name + @")";
        }
    }
}