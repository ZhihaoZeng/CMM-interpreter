using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 解释器构造实践
{
    class GrammarParse
    {
        private Form1 form;
        private String token = "";
        private int type = 999;
        private int index = 0;
        private String errorinfo = "";
        public MyNode mainNode;
        //词法分析结果
        private List<TokenInfo> tokeninfos;
        private TreeView tree;
        private TreeNode rootnode;
        public SemanticParse semanticParse;

        public GrammarParse(Form1 form)
        {
            this.form = form;
            tokeninfos = form.tokeninfos;
            tree = form.treeview;
        }

        private void print(MyNode node, String space, bool indicator)
        {


            String nowspace;
            if (indicator)
                nowspace = "     ";
            else
                nowspace = space;
            if (node.leaf)
                Console.WriteLine(nowspace + node.gettoken());

            if (!node.leaf)

            {
                Console.Write(nowspace + node.info);

                foreach (MyNode subnode in node.nodes)
                {
                    if (subnode == node.nodes[0])
                        indicator = true;
                    else
                        indicator = false;
                    print(subnode, space + "     ", indicator);
                    //print(subnode, space + "   ");
                }
            }
        }

        private void grammarParse()
        {

        }
        //！！！！！！！！！！！！！！！！！
        //将词法分析的结果保存到数组中，以供回退方便
        //消除文法二义性，进入一个子分支，第一个符号判断正确则表示一定是该状态，在第二个状态就可以报错！！！
        //获取下一个token

        //上层也要抛出错误，以供检查
        private String getnext()
        {
            //更新index
            //更新token
            //更新type
            if (index >= tokeninfos.Count())
            {
                Console.WriteLine("到达程序终点");
                return "!!!!!";
            }
            token = tokeninfos[index].token;
            type = tokeninfos[index].type;
            //此时指向下一个token
            index++;
            return token;
        }

        private bool windBack(int position)
        {
            index = position;
            token = "";
            type = 9999;
            return false;
        }
        int errorcount = 0;
        private bool throwErr(String error)
        {
            //错误栈
            errorinfo = "位置index：" + index + "token：" + token + " , 错误信息：" + error + '\n';
           if(errorcount==0)
                form.highlightError(
                tokeninfos[index - 1].stringindex,
                tokeninfos[index - 1].token.Length, 
                Color.LightSkyBlue,
                true);
            errorcount++;
            form.richTextBox3.Text += errorinfo;
            return false;
        }

        private bool throwWarning(int start, int length,String warning)
        {

            //错误栈
            errorinfo = 
                "警告！！！：" + "位置index：" + index 
                + "token：" + token + "警告信息："+warning + '\n';

            if (errorcount == 0) form.highlightError(start, length,Color.LightGoldenrodYellow,true);
            form.richTextBox3.Text += errorinfo;
            return false;
        }


        //常数
        private bool Constant(MyNode uppernode)
        {
            getnext();
            MyNode node = new MyNode("常数");
            MyNode leafnode = new MyNode("token");

            if (type == 20 + form.signAddress)
            {
                leafnode.leaf = true;
                leafnode.settoken(token);
                leafnode.type = type;
                node.stringindex = tokeninfos[index - 1].stringindex;
                leafnode.stringindex = tokeninfos[index - 1].stringindex;
                node.AddNode(leafnode);// node.nodes.Add(leafnode);
                uppernode.AddNode(node);//uppernode.nodes.Add(node);
                return true;
            }
            return false;
        }
        
        private bool Float(MyNode uppernode)
        {//浮点数
            getnext();
            MyNode node = new MyNode("浮点数");
            MyNode leafnode = new MyNode("token");

            if (type == 27 + form.signAddress)
            {
                leafnode.leaf = true;
                leafnode.settoken(token);
                leafnode.type = type;
                node.stringindex = tokeninfos[index - 1].stringindex;
                leafnode.stringindex = tokeninfos[index - 1].stringindex;

                node.AddNode(leafnode);// node.nodes.Add(leafnode);
                uppernode.AddNode(node);//uppernode.nodes.Add(node);
                return true;
            }
            return false;
        }

        //符号和保留字！！！！！！！！！！！
        private bool Sign(MyNode uppernode, String temp)
        {
            getnext();
            MyNode node = new MyNode(temp);

            if (token == temp)
            {
                node.leaf = true;
                node.type = type;
                node.settoken(token);
                node.stringindex = tokeninfos[index - 1].stringindex;

                uppernode.AddNode(node);//uppernode.nodes.Add(node);
                return true;
            }
            else return false;
        }


        //程序
        public bool Programe()
        {//<程序>-><函数定义> #
            //初始化最顶结点
            mainNode = new MyNode("程序");
            MyNode node = mainNode;

            rootnode = new TreeNode("程序");
            mainNode.treenode = rootnode;
            tree.Nodes.Add(rootnode);
            int position = index;
            //<函数定义>
            if (!FunctionDefinition(node)) return false;
            position = index;
            while(FunctionDefinition(node))
            {
                position = index;
            }
            windBack(position);
            if (Sign(node, "#"))
            {//遇到#终止符，程序结束
             //TODO
             //  print(mainNode, "",true);
             // tree.Nodes.Add(rootnode);
                tree.ExpandAll();

                return true;
            }
            //没遇到终止符，即有多余符号
            throwErr("函数定义外部有多余符号");
            //print(mainNode, "",true);

            //tree.Nodes.Add(rootnode);
            tree.ExpandAll();
            return false;
        }


        //函数定义
        private bool FunctionDefinition(MyNode uppernode)
        {//<函数定义>-><类型><标志符>(<形参>)<语句列表>
            //建立自己的结点
            MyNode node = new MyNode("函数定义");
            int position = index;
            if (!Type(node))
            {
                //throwErr("FunctionDefinition：函数定义缺少类别定义");
                
                return windBack(position);
            }
            if (!Marker(node))//缺少标志符
            {
                if (!Sign(node, "main"))
                {
                    throwErr("FunctionDefinition：函数定义缺少函数名");
                    return windBack(position);
                }
            }
            if (!Sign(node, "("))
            {
                //缺少括号
                throwErr("FunctionDefinition：函数定义缺少(");
                return windBack(position);

            }
            if (!FormalParameter(node))//缺少......
            {
                throwErr("FunctionDefinition：函数定义缺少形参");
                return windBack(position);
            }
            if (!Sign(node, ")"))
            {
                throwErr("FunctionDefinition：函数定义缺少)   ");
                return windBack(position);
            }
            //if (!SentenceList(node))
            if (!CompoundSentence(node))
            {
                throwErr("FunctionDefinition：函数定义缺少语句");
                return windBack(position);
            }
            //当所有的子树都成功后才将自己连上父节点
            uppernode.AddNode(node);//uppernode.nodes.Add(node);
            int a = - --position;
            return true;
        }

        //类型
        private bool Type(MyNode uppernode)
        {//类型
            MyNode node = new MyNode("类型");
            MyNode leafnode = new MyNode("token");
            leafnode.leaf = true;

            bool indicator = false;
            switch (getnext())
            {
                case "int":
                    indicator = true;
                    break;
                case "float":
                    indicator = true;
                    break;
                case "bool":
                    indicator = true;
                    break;
                default:
                    indicator = false;
                    break;
            }
            if (indicator)
            {//是类型
                leafnode.settoken(token);
                leafnode.type = type;
                leafnode.stringindex = tokeninfos[index - 1].stringindex;

                node.AddNode(leafnode);//node.nodes.Add(leafnode);
                uppernode.AddNode(node);//uppernode.nodes.Add(node);
                return true;
            }
            return false;
        }


        //标志符
        private bool Marker(MyNode uppernode)
        {//标志符
            int position = index;
            MyNode node = new MyNode("标志符");
            MyNode leafnode = new MyNode("token");
            getnext();
            if (type == 21 + form.signAddress)
            {
                leafnode.leaf = true;
                leafnode.type = type;
                leafnode.settoken(token);
                leafnode.stringindex = tokeninfos[index - 1].stringindex;

                node.AddNode(leafnode);//node.nodes.Add(leafnode);
                uppernode.AddNode(node);//uppernode.nodes.Add(node);
                return true;
            }
            else
                return windBack(position);
        }

        //形参
        private bool FormalParameter(MyNode uppernode)
        {//<形参>-><类型><标志符> 
            MyNode node = new MyNode("形参");
            int position = index;
            //<形参>-><类型><标志符>
            if (!Type(node))
            {
                //node.leaf = true;
                windBack(position);
                return true;
            }
            uppernode.AddNode(node);//uppernode.nodes.Add(node);
            //！！！！！！！！！！！！！！！！！！！！！！多形参？？？？？函数调用？？？？？？
            if (!Marker(uppernode))
            {
                //缺少括号
                throwErr("FormalParameter：形参缺少标志符");
                return windBack(position);

            }
            return true;
        }

        //private bool FormalParameter2(Node uppernode)
        //{
        //    Node node = new Node("形参")
        //}

        //语句列表
        private bool SentenceList(MyNode uppernode)
        {//< 语句列表 >->< 语句 > | < 语句  ><语句列表 >
            MyNode node = new MyNode("语句列表");
            int position = index;
            if (!Sentence(node)) return windBack(position);

            //保存当前位置以供回退使用
            position = index;
            if (!SentenceList(node))
            {//不再右递归
                //回退位置
                windBack(position);
            }
            uppernode.AddNode(node);//uppernode.nodes.Add(node);
            return true;
        }


        //语句
        private bool Sentence(MyNode uppernode)
        {//<语句>-> <复合语句> | <表达式语句> | <条件语句> | <循环语句>
            MyNode node = new MyNode("语句");
            int position = index;
            if (CompoundSentence(node))
            {
                uppernode.AddNode(node);//uppernode.nodes.Add(node);
                return true;
            }
            else windBack(position);

            if (ExpressionSentence(node))
            {
                uppernode.AddNode(node);//uppernode.nodes.Add(node);
                return true;
            }
            else windBack(position);

            if (ConditionSentence(node))
            {
                uppernode.AddNode(node);//uppernode.nodes.Add(node);
                return true;
            }
            else windBack(position);

            if (LoopSentence(node))
            {
                uppernode.AddNode(node);//uppernode.nodes.Add(node);
                return true;
            }
            else windBack(position);

            return false;
        }

        //复合语句
        private bool CompoundSentence(MyNode uppernode)
        {//<复合语句>->{<语句列表> }
            MyNode node = new MyNode("复合语句");
            int position = index;
            if (!Sign(node, "{"))//读入一个括号后就直接进入了复合语句的状态，没有其他状态可以接收括号
                return windBack(position);

            if (!SentenceList(node))
            {
                //先检查是否为空
                if (peek() == "}")
                {//说明复合语句为空
                    Sign(node, "}");
                    uppernode.AddNode(node);
                    throwWarning(tokeninfos[index - 2].stringindex, tokeninfos[index - 1].stringindex + 1 - tokeninfos[index - 2].stringindex,"警告！！！：{}中为空");
                    return true;
                }
                else
                {throwErr("CompoundSentence：复合语句缺少语句列表");
                return windBack(position);
                }
                
            }

            if (!Sign(node, "}"))
            {
                //错误处理！！！！！！！！！！！！！！！！
                throwErr("CompoundSentence：复合语句缺少}");
                return windBack(position);
            }

            uppernode.AddNode(node);//uppernode.nodes.Add(node);
            return true;
        }


        //表达式语句
        private bool ExpressionSentence(MyNode uppernode)
        {//<表达式语句>-> <表达式>;|;
            MyNode node = new MyNode("表达式语句");
            int position = index;

            if (!Expression(node))
            {
                //  throwErr("ExpressionSentence：表达式语句缺少表达式");
                //如果当前行只有一个分号，则表示为空，不返回错误
                if(peek()==";")
                {
                    Sign(node, ";");
                    uppernode.AddNode(node);
                    return true;
                }
                return windBack(position);

            }
            if (!Sign(node, ";"))
            {
                //错误处理！！！！！！！！！！！！！！！！
                throwErr("ExpressionSentence：表达式语句缺少;");
                return windBack(position);
            }
            uppernode.AddNode(node);//uppernode.nodes.Add(node);
            return true;
        }


        //条件语句
        private bool ConditionSentence(MyNode uppernode)
        {//< 条件语句 >->if (< 逻辑表达式 >) < 语句 > < elseif >
            MyNode node = new MyNode("条件语句");
            int position = index;
            if (!Sign(node, "if"))
                return windBack(position);
            if (!Sign(node, "("))
            {
                //报错
                throwErr("ConditionSentence：if语句中缺少(");
                return windBack(position);
            }
            if (!LogicExpression(node))
            {
                throwErr("ConditionSentence：if语句中缺少逻辑表达式");
                return windBack(position);

            }
            if (!Sign(node, ")"))
            {
                //报错
                throwErr("ConditionSentence：if语句中缺少)");
                return windBack(position);
            }
            if (!Sentence(node))
            {
                throwErr("ConditionSentence：if语句缺少语句");
                return windBack(position);
            }

            position = index;
            if (!ElseIf(node))
            {
                windBack(position);
            }
            uppernode.AddNode(node);//uppernode.nodes.Add(node);
            return true;
        }
        //elseif
        private bool ElseIf(MyNode uppernode)
        {//<elseif>-> else <语句> | 空
            MyNode node = new MyNode("ElseIf");
            int position = index;

            if (!Sign(node, "else"))
                return windBack(position);

            if (!Sentence(node))
            {
                //报错
                throwErr("ConsitionSentence：else分支中缺少语句");
                return windBack(position);
            }

            uppernode.AddNode(node);//uppernode.nodes.Add(node);
            return true;
        }




        //循环语句
        private bool LoopSentence(MyNode uppernode)
        {//<循环语句>->while(<逻辑表达式>)<语句>

            MyNode node = new MyNode("循环语句");
            int position = index;

            switch(peek())
            {
                case "while":
                    if (!Sign(node, "while"))
                        return windBack(position);
                    if (!Sign(node, "("))
                    {
                        throwErr("while循环中缺少(");
                        return windBack(position);
                    }
                    if (!LogicExpression(node))
                    {
                        throwErr("while循环中缺少语句");

                        return windBack(position);
                    }
                    if (!Sign(node, ")"))
                    {
                        throwErr("while循环中缺少)");
                        return windBack(position);
                    }
                    if (!Sentence(node))
                    {
                        throwErr("while循环中缺少语句");
                        return windBack(position);

                    }

                    uppernode.AddNode(node);//uppernode.nodes.Add(node);
                    return true;
                case "for":
                    if (!Sign(node, "for"))
                        return windBack(position);
                    if (!Sign(node, "("))
                    {
                        throwErr("for循环中缺少(");
                        return windBack(position);
                    }
                    int subposition = index;
                    if (!DeclarationSentence(node))
                    {
                        windBack(subposition);
                        if(!AssignmentSentence(node))
                        {
                            windBack(subposition);
                            
                        }
                    }
                    subposition = index;

                    if(!Sign(node,";"))
                    {
                        throwErr("for语句中缺少;");
                        return windBack(position);
                    }
                    subposition = index;
                    if(!LogicExpression(node))
                    {
                        throwErr("for语句缺少逻辑表达式");
                        return windBack(position);
                    }
                    subposition = index;
                    if(!Sign(node,";"))
                    {
                        throwErr("for语句缺少;");
                        return windBack(position);

                    }
                    subposition = index;
                    if(!PlusMinus(node))
                    {
                        windBack(subposition);
                        if(!AssignmentSentence(node))
                        {//第三部分可以为空
                            windBack(subposition);
                        }
                    }

                    if(!Sign(node,")"))
                    {
                        throwErr("for循环中缺少)");
                        return windBack(position);
                    }
                    if(!Sentence(node))
                    {
                        throwErr("循环控制中缺少语句");
                        return windBack(position);
                    }
                    uppernode.AddNode(node);//uppernode.nodes.Add(node);
                    return true;
                default:
                    return false;
                    
            }
            
        }


        //表达式
        private bool Expression(MyNode uppernode)
        {//< 表达式 > ->< 声明语句 >|<函数调用>|< 赋值语句 >|<自增减>
            MyNode node = new MyNode("表达式");
            int position = index;
            if (DeclarationSentence(node))
            {
                uppernode.AddNode(node);//uppernode.nodes.Add(node);
                return true;
            }
            windBack(position);
            if (PlusMinus(node))
            {
                uppernode.AddNode(node);
                return true;
            }
            windBack(position);
            if (Return(node))
            {
                uppernode.AddNode(node);
                return true;
            }
            windBack(position);
            if (BreakContinue(node))
            {
                uppernode.AddNode(node);
                return true;
            }
            // throwErr("Expression：表达式中缺少声明语句或赋值语句");

            windBack(position);
            if (FunctionCall(node))
            {
                uppernode.AddNode(node);
                return true;
            }windBack(position);
            if (AssignmentSentence(node))
            {
                uppernode.AddNode(node);//uppernode.nodes.Add(node);
                return true;
            }
            
            return windBack(position);

        }


        //声明语句
        private bool DeclarationSentence(MyNode uppernode)
        {//<声明语句>-><类型><赋值语句>
            MyNode node = new MyNode("声明语句");
            int position = index;
            if (!Type(node))
                return windBack(position);

            int subposition = index;
            if(!DeclarationSentence2(node))
            {
                throwErr("声明语句缺少右部变量");
                return windBack(position);
            }
            uppernode.AddNode(node);
            return true;
           
        }

        private bool DeclarationSentence2(MyNode uppernode)
        {//<声明语句2>		-><声明语句3>,<声明语句2>|<声明语句3>

            MyNode node = new MyNode("声明语句2");
            int position = index;
            if(!DeclarationSentence3(node))
            {
                return windBack(position);
            }
            int subposition = index;
            if(Sign(node,","))
            {
                if(!DeclarationSentence2(node))
                {

                    throwErr("声明语句,后缺少声明");
                    return windBack(position);
                }
                uppernode.AddNode(node);
                return true;
            }
            windBack(subposition);
            uppernode.AddNode(node);
            return true;
        }

        private  bool DeclarationSentence3(MyNode uppernode)
        {//<声明语句3>		-><数组>=<数组赋值>|<数组>|<赋值语句>|<标志符>

            MyNode node = new MyNode("声明语句3");
            int position = index;
            int subposition = index;
            if (Array(node))
            {
                if (Sign(node, "="))
                {
                    if (ArrayAssignment(node))
                    {
                        uppernode.AddNode(node);
                        return true;
                    }
                    throwErr("数组声明缺少右部");
                    return windBack(position);
                }
                windBack(index - 1);
                uppernode.AddNode(node);
                return true;
            }
            windBack(subposition);
            if (!AssignmentSentence(node))
            {
                //int subposition = index;

                //if(Array(node))
                //{
                //    uppernode.AddNode(node);
                //    return true;
                //}
                windBack(subposition);

                if (Marker(node))
                {
                    uppernode.AddNode(node);//uppernode.nodes.Add(node);
                    return true;
                }// throwErr("DeclarationSentence：声明语句中缺少赋值语句");
                throwErr("DeclarationSentence：声明语句中缺少变量名");
                return windBack(position);
            }

            uppernode.AddNode(node);//uppernode.nodes.Add(node);
            return true;
        }

        //赋值语句
        private bool AssignmentSentence(MyNode uppernode)
        {/*<赋值语句>-><标志符> = <赋值语句>xxxxx
                        |<标志符> =<标志符>xxxxx
                        |<标志符> = <算术表达式>
                        |<标志符> = <逻辑表达式>
                        |<标识符> = <自增减>
                        |<标志符> = <常数>xxxxx*/

            MyNode node = new MyNode("赋值语句");
            int position = index;
            int subposition;


            if(!Array(node))
            {
                windBack(position);
                if(!Marker(node))
                {
                    return windBack(position);
                }
            }
            if (!Sign(node, "="))
            {
                //赋值语句中缺少 = 
                //当当前符号是自增减符号时
                if (token == "++" || token == "--")
                    return windBack(position);
               // throwErr("赋值语句中缺少 = ");
                return windBack(position);
            }
            subposition = index;
            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!暂时取消多变量赋值
            //if (AssignmentSentence(node))
            //{
            //    uppernode.AddNode(node);//uppernode.nodes.Add(node);
            //    return true;
            //}
            //else windBack(subposition);

            //if (Marker(node))
            //{
            //    uppernode.AddNode(node);//uppernode.nodes.Add(node);
            //    return true;
            //}
            //else windBack(subposition);

            if (MathematicExpression(node, false))
            {//初始的算术表达式不用翻转加减法以及乘除号
                int temp = tokeninfos[index].type;
                if(temp == 11 + form.signAddress
                        || temp == 12 + form.signAddress
                        || (temp <= 17 + form.signAddress && temp >= 14 + form.signAddress)
                        || temp == 23 + form.signAddress
                        || temp == 24 + form.signAddress)
                {//`````
                    windBack(subposition);
                }
                //else if (token == "++" || token == "--")
                //{
                //    windBack(subposition);
                //}
                else
                {
                    uppernode.AddNode(node);//uppernode.nodes.Add(node);
                    return true;
                }
            }
            else windBack(subposition);

            if(PlusMinus(node))
            {
                uppernode.AddNode(node);//uppernode.nodes.Add(node);
                return true;
            }
            else windBack(subposition);

            if (LogicExpression(node))
            {
                uppernode.AddNode(node);//uppernode.nodes.Add(node);
                return true;
            }
            else windBack(subposition);

            //if (Constant(node))
            //{
            //    uppernode.AddNode(node);//uppernode.nodes.Add(node);
            //    return true;
            //}
            //else windBack(subposition);
            throwErr("AssignmentSentence：赋值语句缺少右部");
            return false;
        }


        //数组赋值
        private bool ArrayAssignment(MyNode uppernode)
        {
            MyNode node = new MyNode("数组赋值");
            int position = index;
            if(!Sign(node,"{"))
            {
                return windBack(position);
            }
            if(!ArrayAssignment2(node))
            {
                if (!Sign(node, "}"))
                {
                    return windBack(position);
                }//sthrowErr("数组赋值缺少内容");
                uppernode.AddNode(node);
                return true;
            }

            if (!Sign(node, "}"))
            {
                return windBack(position);
            }//sthrowErr("数组赋值缺少内容");
            uppernode.AddNode(node);
            return true;
        }


        //数组赋值2
        private bool ArrayAssignment2(MyNode uppernode)
        {
            MyNode node = new MyNode("数组赋值2");
            int position = index;
            if(MathematicExpression(node,false))
            {
                if(Sign(node,","))
                {
                    if(!ArrayAssignment2(node))
                    {
                        throwErr("数组赋值表达式出错");
                        return windBack(position);
                    }
                    uppernode.AddNode(node);
                    return true;
                }
                else
                {
                    windBack(index-1);
                    uppernode.AddNode(node);
                    return true;
                }
            }
            else if (!LogicExpression(node))
            {
                if (Sign(node, ","))
                {
                    if (!ArrayAssignment2(node))
                    {
                        throwErr("数组赋值表达式出错");
                        return windBack(position);
                    }
                    uppernode.AddNode(node);
                    return true;
                }
                else
                {
                    windBack(index - 1);
                    uppernode.AddNode(node);
                    return true;
                }
            }
            else
            {
                return windBack(position);
            }

        }


        //逻辑表达式
        private bool LogicExpression(MyNode uppernode)
        {//<逻辑表达式>	-><AND>||<逻辑表达式> |<AND>
            MyNode node = new MyNode("逻辑表达式");
            int position = index;
            if (!AND(node))
            {
                return windBack(position);
            }
            position = index;
            if (!Sign(node, "||"))
                windBack(position);
            else
            {
                if (!LogicExpression(node))
                {
                    throwErr("缺少后续逻辑表达式");
                    return windBack(position);
                }
            }
            uppernode.AddNode(node);//uppernode.nodes.Add(node);
            return true;
        }

        //AND
        private bool AND(MyNode uppernode)
        {//< AND >-><FACTOR>&&<AND> |<FACTOR>
            MyNode node = new MyNode("AND");
            int position = index;
            if (!FACTOR(node))
            {
                return windBack(position);
            }
            position = index;
            if (!Sign(node, "&&"))
                windBack(position);
            else
            {
                if (!AND(node))
                {
                    throwErr("AND：缺少后续逻辑表达式");
                    return windBack(position);
                }
            }
            uppernode.AddNode(node);//uppernode.nodes.Add(node);
            return true;
        }

        

        //FACTOR
        private bool FACTOR(MyNode uppernode)
        {/*< FACTOR > -><算术表达式>><算术表达式> 
                        |<算术表达式><<算术表达式> 
                        |<算术表达式>==<算术表达式> 
                        |<算术表达式>!=<算术表达式> 
                        |<算术表达式>>=<算术表达式>
                        |<算术表达式><=<算术表达式>
                        |(<逻辑表达式>)
                        |<算术表达式>
            */
            MyNode node = new MyNode("FACTOR");
            int position = index;


            if(Sign(node,"true"))
            {
                uppernode.AddNode(node);//uppernode.nodes.Add(node);
                return true;
            }
            windBack(position);
            if (Sign(node,"false"))
            {
                uppernode.AddNode(node);//uppernode.nodes.Add(node);
                return true;
            }
            windBack(position);
            if (!MathematicExpression(node, false))
            {//(<逻辑表达式>)
                if (!Sign(node, "("))
                {
                    return windBack(position);
                }

                if (!LogicExpression(node))
                {
                    throwErr("FACTOR：缺少逻辑表达式");
                    return windBack(position);
                }
                if (!Sign(node, ")"))
                {
                    throwErr("FACTOR：缺少)");
                    return windBack(position);
                }
            }
            else
            {//<算术表达式>
                int subposition;
                bool right = false;
                subposition = index;
                //node.type = type;
                //node.settoken(token);
                if (Sign(node, ">"))
                {
                    right = true;
                    subposition = index;
                }
                else windBack(subposition);

                if (Sign(node, "<"))
                {
                    right = true;
                    subposition = index;
                }
                else windBack(subposition);

                if (Sign(node, "=="))
                {
                    right = true;
                    subposition = index;
                }
                else windBack(subposition);

                if (Sign(node, "!="))
                {
                    right = true;
                    subposition = index;
                }
                else windBack(subposition);

                if (Sign(node, ">="))
                {
                    right = true;
                    subposition = index;
                }
                else windBack(subposition);

                if (Sign(node, "<="))
                {
                    right = true;
                    subposition = index;
                }
                else windBack(subposition);

                if (Sign(node, "<>"))
                {
                    right = true;
                    subposition = index;
                }
                else windBack(subposition);


                if(!right)
                {
                    uppernode.AddNode(node);
                    return true;
                }
                //if (!right)
                //{
                //    throwErr("FACTOR：缺少逻辑运算符");
                //    return windBack(position);
                //}

                if (!MathematicExpression(node, false))
                {
                    throwErr("FACTOR：逻辑运算符后缺少算术表达式");
                    return windBack(position);
                }
            }
            uppernode.AddNode(node);//uppernode.nodes.Add(node);
            return true;
        }



        //算术表达式
        private bool MathematicExpression(MyNode uppernode)
        {
            return MathematicExpression(uppernode, false);
        }
        private bool MathematicExpression(MyNode uppernode, bool plusreverse)
        {//<算术表达式>	-><算术表达式2>+<算术表达式>|<算术表达式2>-<算术表达式>|<算术表达式2>
            MyNode node = new MyNode("算术表达式");
            int position = index;
            if (!Math2(node, false))
            {
                return windBack(position);
            }            //node.type = type;F
            //node.settoken(token);
            int subposition = index;
            bool newreverse = false;
            if (peek() == "+")
            {
                if (plusreverse)
                {
                    ReverseSign(node, "+");//将减号加到当前的子树上
                    newreverse = false;
                }
                else
                {
                    Sign(node, "+");
                    newreverse = false;
                }
            }
            else if (peek() == "-")
            {
                if (plusreverse)
                {
                    ReverseSign(node, "-");
                    newreverse = true;
                }
                else
                {
                    Sign(node, "-");
                    newreverse = true;
                }
            }
            else
            {
                //没有检测到符号
                uppernode.AddNode(node);//uppernode.nodes.Add(node);
                return true;
            }

            if (!MathematicExpression(node, newreverse))
            {
                throwErr("MathematicExoression: +-符号后缺少算术表达式");
                return windBack(position);
            }
            uppernode.AddNode(node);
            return true;
        }

        private String peek()
        {
            return tokeninfos[index].token;
        }
        private bool ReverseSign(MyNode uppernode, String temp)
        {
            getnext();
            switch (token)
            {
                case "+":
                    token = "-";
                    type = 8 + form.signAddress;
                    break;
                case "-":
                    token = "+";
                    type = 7 + form.signAddress;
                    break;
                case "*":
                    token = "/";
                    type = 10 + form.signAddress;
                    break;
                case "/":
                    token = "*";
                    type = 9 + form.signAddress;
                    break;
            }


            MyNode node = new MyNode(temp);
            //if (token == temp)
            //{
            node.leaf = true;
            node.type = type;
            node.settoken(token);
            node.stringindex = tokeninfos[index - 1].stringindex;

            uppernode.AddNode(node);
            return true;
            //}
            //else return false;
        }

        //算术表达式2
        private bool Math2(MyNode uppernode, bool multreverse)
        {//<算术表达式>	-><算术表达式2>+<算术表达式>|<算术表达式2>-<算术表达式>|<算术表达式2>
            MyNode node = new MyNode("算术表达式2");
            int position = index;
            if (!Math3(node))
            {
                return windBack(position);
            }

            //node.type = type;
            //node.settoken(token);
            int subposition = index;

            bool newreverse = false;
            if (peek() == "*")
            {
                if (multreverse)
                {
                    ReverseSign(node, "*");//将除号加到当前的子树上
                    newreverse = false;
                }
                else
                {
                    Sign(node, "*");
                    newreverse = false;
                }
            }
            else if (peek() == "/")
            {
                if (multreverse)
                {
                    ReverseSign(node, "/");
                    newreverse = true;
                }
                else
                {
                    Sign(node, "/");
                    newreverse = true;
                }
            }
            else
            {
                //没有检测到符号

                uppernode.AddNode(node);
                return true;
            }

            if (!Math2(node, newreverse))
            {
                throwErr("Math2: */符号后缺少算术表达式");
                return windBack(position);
            }
            uppernode.AddNode(node);
            return true;
        }


        //算术表达式3
        private bool Math3(MyNode uppernode)
        {//<算术表达式3>->(<算术表达式>)|<常数>|<标志符>
            MyNode node = new MyNode("算术表达式3");
            int position = index;
            if (Sign(node, "("))
            {//算术表达式
                if (!MathematicExpression(node, false))
                {
                    throwErr("Math3：()内缺少算术表达式");
                    return windBack(position);
                }
                if (!Sign(node, ")"))
                {
                    windBack(index - 1);
                    int temp = tokeninfos[index].type;
                    if (temp == 11 + form.signAddress
                        || temp == 11 + form.signAddress
                        || (temp <= 17 + form.signAddress && temp >= 14 + form.signAddress)
                        || (temp >= 22 + form.signAddress
                        && temp <= 24 + form.signAddress))
                    {//说明此处的等号为逻辑运算符中匹配的
                        //可以直接返回错误的原因是，算术表达式返回出错并不会直接抛出错误
                        return windBack(position);
                    }
                    
                    throwErr("Math3：缺少匹配的）");
                    return windBack(position);
                }

                uppernode.AddNode(node);
                return true;
            }
            windBack(position);


            if (Math4(node))
            {
                uppernode.AddNode(node);
                return true;
            }


            //throwErr("算术表达式3什么都没有");
            return false;
        }


        private bool Math4(MyNode uppernode)
        {
            MyNode node = new MyNode("算术表达式4");
            int position = index;
            
            if(Negative(node))
            {
                uppernode.AddNode(node);
                return true;
            }
            windBack(position);

            if(Float(node))
            {
                uppernode.AddNode(node);
                return true;
            }
            windBack(position);

            if (Constant(node))
            {
                uppernode.AddNode(node);
                return true;
            }
            windBack(position);

            if(PlusMinus(node))
            {
                uppernode.AddNode(node);
                return true;
            }
            windBack(position);

            if (Array(node))
            {
                uppernode.AddNode(node);
                return true;
            }
            windBack(position);

            //if(FunctionCall(node))
            //{
            //    uppernode.AddNode(node);
            //    return true;
            //}
            //windBack(position);
            if (Marker(node))
            {
                uppernode.AddNode(node);
                return true;
            }
            
            return windBack(position);
        }


        private bool Negative(MyNode uppernode)
        {
            MyNode node = new MyNode("负数");
            int position = index;
            if (peek() == "-")
            {
                Sign(node, "-");
                int subposition = index;
                if(!Math4(node))
                {
                    windBack(subposition);
                    throwErr("错误的-号");
                    return false;
                }
                uppernode.AddNode(node);
                return true;

            }
            else
                return false;
        }
        private bool PlusMinus(MyNode uppernode)
        {
            MyNode node = new MyNode("自增减");
            int position = index;


            if (Array(node))
            {
                int subposition = position;

                if (peek() == "++" || peek() == "--")
                {
                    if (Sign(node, "++"))
                    {
                        uppernode.AddNode(node);//uppernode.nodes.Add(node);
                        return true;
                    }
                    windBack(subposition);

                    if (Sign(node, "++"))
                    {
                        uppernode.AddNode(node);//uppernode.nodes.Add(node);
                        return true;
                    }
                    windBack(subposition);
                    return false;
                }
                //纯数组
                //uppernode.AddNode(node);//uppernode.nodes.Add(node);
                //return true;
                return windBack(position);
            }
            windBack(position);

            if (Marker(node))
            {
                int subposition = index;

                if (peek() == "++" || peek() == "--")
                {
                    if (Sign(node, "++"))
                    {
                        uppernode.AddNode(node);//uppernode.nodes.Add(node);
                        return true;
                    }
                    windBack(subposition);

                    if (Sign(node, "--"))
                    {
                        uppernode.AddNode(node);//uppernode.nodes.Add(node);
                        return true;
                    }
                    windBack(subposition);
                    return false;
                }
                //纯标识符
                // uppernode.AddNode(node);//uppernode.nodes.Add(node);
                //return true;
                return windBack(position);
            }
            windBack(position);

            if (Sign(node, "++"))
            {
                int subposition = index;
                if (Array(node))
                {
                    uppernode.AddNode(node);
                    return true;
                }
                windBack(subposition);

                if (Marker(node))
                {
                    uppernode.AddNode(node);
                    return true;
                }
                windBack(subposition);
                throwErr("自增符号后无标志符或数组");
                return false;
            }
            windBack(position);
            if (Sign(node, "--"))
            {
                int subposition = index;
                if (Array(node))
                {
                    uppernode.AddNode(node);
                    return true;
                }
                windBack(subposition);

                if (Marker(node))
                {
                    uppernode.AddNode(node);
                    return true;
                }
                windBack(subposition);
                throwErr("自增符号后无标志符或数组");
                return false;
            }

            windBack(position);
            return false;
        }


        private bool Array(MyNode uppernode)
        {//<数组>-><标志符>[<算术表达式>]
            MyNode node = new MyNode("数组");
            int position = index;
            
            if(!Marker(node))
            {
                return windBack(position);
            }
            if(!Sign(node,"["))
            {
                return windBack(position);
            }
            if(!MathematicExpression(node))
            {
                throwErr("数组中缺少算术表达式");
                return windBack(position);
            }
            if(!Sign(node,"]"))
            {
                throwErr("数组缺少]");
                return windBack(position);
            }
            uppernode.AddNode(node);
            
            return true;
            
        }

        private bool FunctionCall(MyNode uppernode)
        {//<函数调用>-><标志符>(<算术表达式>)|<标志符>(<逻辑表达式>)
            MyNode node = new MyNode("函数调用");
            int position = index;
            if(!Marker(node))
            {
                return windBack(position);
            }
            if(!Sign(node,"("))
            {
               // throwErr("函数调用缺少(");
                return windBack(position);
            }
            int subposition = index;
            if(!MathematicExpression(node))
            {
                  
                if(peek()!=")")
                {
                    //参数为逻辑表达式
                    if (!LogicExpression(node))
                        return throwErr("函数形参错误");
                }
                //无参数 
            }
            if (!Sign(node,")"))
            {
                throwErr("函数调用缺少)");
                return windBack(position);
            }
            uppernode.AddNode(node);
            return true;

        }


        private bool BreakContinue(MyNode uppernode)
        {
            MyNode node = new MyNode("BREAKCONTINUE");
            int position = index;
            if(!Sign(node,"break"))
            {
                windBack(position);
                if(!Sign(node,"continue"))
                {
                    return windBack(position);
                }
                uppernode.AddNode(node);
                return true;
            }

            uppernode.AddNode(node);

            return true;
        }
        //返回语句
        private bool Return(MyNode uppernode)
        {
            MyNode node = new MyNode("返回语句");
            int position = index;
            if(!Sign(node,"return"))
            {
                return windBack(position);
            }
            int subposition = index;
            if(!MathematicExpression(node,false))
            {
                //此处如果无返回应该报错
                //但是考虑到void类型的函数可以只使用return，错误处理留到语义分析过程中
                //throwErr

                windBack(index);
                uppernode.AddNode(node);

                return true;
            }
             
            uppernode.AddNode(node);
            return true;
        }
    }
}
