using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 解释器构造实践
{
    public class MyNode
    {
        public bool leaf = false;
        public String info;
        public List<MyNode> nodes;
        public int type;
        private String token;
        public int stringindex;

        private bool cut = false;
        //deleexecute action;
        public MyNode()
        {
            nodes = new List<MyNode>();
            treenode = new TreeNode();
        }
        public MyNode(String info)
        {
            this.info = info;
            nodes = new List<MyNode>();
            treenode = new TreeNode(info);
        }

        public void settoken(String token, bool change)
        {
            this.token = token;
            settoken(token);
        }
        public void settoken(String token)
        {
            this.token = token;
            treenode.Text = token;
        }
        public String gettoken()
        {
            return token;
        }
        public TreeNode treenode;
        public void AddNode(MyNode node)
        {
            if(cut)
            {
                if (node.nodes.Count == 1)
                {
                    this.AddNode(node.nodes[0]);
                    return;
                }
                this.OrdinaryAddNode(node);

            }
            else
            {
                this.OrdinaryAddNode(node);
            }
        }


        //优化子树
        public void AddNode(MyNode node, bool shortcut)
        {
            if (shortcut)
            {
                if (node.nodes.Count == 1)
                {
                    this.AddNode(node.nodes[0]);
                    return;
                }
            }

            this.OrdinaryAddNode(node);
        }

        //正常将所有的过程结点都加到树中
        public void OrdinaryAddNode(MyNode node)
        {
            nodes.Add(node);
            treenode.Nodes.Add(node.treenode);
            if (node.leaf)
            {
                node.treenode.BackColor = Color.LightPink;
            }
        }
    }
}
