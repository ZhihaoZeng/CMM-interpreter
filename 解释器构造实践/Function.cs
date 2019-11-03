using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 解释器构造实践
{
    class Function
    {
        public MyNode node;//指向结点
        public int tableindex;
        public String type;
        public String name;
        public Function(String name, MyNode node, String type)
        {
            this.name = name;
            this.node = node;
            this.type = type;
            this.name = node.nodes[1].nodes[0].gettoken();
        }
        public Function(MyNode node,String type)
        {
            this.node = node;
            this.type = type;
            this.name = node.nodes[1].nodes[0].gettoken();
        }

        //public Function(TreeNode treenode, MyNode node, int index);


    }
}
