using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
/*
 * 软工五班 曾志昊 2017302580214
 */
namespace 解释器构造实践
{
    delegate void deleexecute(MyNode node);
    class SemanticParse
    {
        private Form1 form;
        private TreeView treeview;
        private TreeNode rootnode;
        private MyNode mainnode;
        public const String err = "_ERR";
        private int errorcount = 0;
        private int tablestop = -1;
 
        private List<SignTable> tables = new List<SignTable>();
        private List<Function> functionlist = new List<Function>();
        private SignTable toptable;
        private List<int> temporary;
        public bool run = false;//是否运行，运行则需要执行输入输出，以及返回最终结果
                                //不运行则只编译，针对函数内部进行分析
        private String not = "not";
        private String jump = "jump";
        private String add = "add";
        private String emp = "";
        public String backtemp;//返回值
        public int loopnumber = 0;
        private int texterrorcount = 0;


        public SemanticParse() { }
        public SemanticParse(MyNode mainnode, Form1 form)
        {
            this.form = form;
            this.mainnode = mainnode;
            //构造scanf和print的node**************************************
            //functionlist.Add(new Function("print", null, "int"));
            //functionlist.Add(new Function("scanf", null, "int"));
            //for (int i = 1; i <= 20; i++)
            //{
            //    temporary.Add(i);
            //}
        }

        private SignTable addTable()
        {
            SignTable table = new SignTable();
            tables.Add(table);
            tablestop++;
            toptable = table;
            return table;
        }

        private void paintAllBelowToken(MyNode node)
        {
            texterrorcount++;
            if(texterrorcount==1)
            {
                paintAllBelowToken2(node);
            }
        }
        private void paintAllBelowToken2(MyNode node)
        {
            foreach (MyNode subnode in node.nodes)
            {
                paintAllBelowToken2(subnode);
            }
            if (node.leaf)
            {
                form.highlightError(node.stringindex, node.gettoken().Length, Color.MediumPurple,true);
            }
        }


        public void insert(String operate, String first, String second, String result)
        {
            form.insertIntoListview2(operate, first, second, result);
        }
        public void insert(String address)
        {
            form.insertIntoListview2(address, emp, emp, emp);
        }
        public void insert(String operate, String first, String second, String result, int index)
        {
            ListViewItem temp = new ListViewItem(operate.ToString());
            temp.SubItems.Add(first);
            temp.SubItems.Add(second);
            temp.SubItems.Add(result);
            form.listView2.Items.Insert(index, temp);
        }



        private SignTable popTable()
        {
            SignTable table = tables[tablestop];
            tables.RemoveAt(tablestop);
            tablestop--;
            toptable = tables[tablestop];
            return table;
        }

        

        private int BREAK = 1;
        private int CONTINUE = 2;
        private int RETURN = 3;
        private bool paint = true;

        String no = "";
        public void print(String operation,String o1,String operatorsign,String o2,String equal,String result)
        {
            // form.richTextBox2.Text += str + '\n';
            ListViewItem temp = new ListViewItem(operation);
            temp.SubItems.Add(o1);
            temp.SubItems.Add(operatorsign);
            temp.SubItems.Add(o2);
            temp.SubItems.Add(equal);
            temp.SubItems.Add(result);
            form.listView2.Items.Add(temp);
        }

        private bool throwErr(String error, MyNode node,bool all)
        {
            
            paintAllBelowToken(node);
            //所有子树都高亮
            if (all)
            {
                foreach(MyNode mynode in node.nodes)
                {
                    throwErr("", mynode, true);
                    mynode.treenode.BackColor = Color.MediumPurple;
                }
            }
            if(node.leaf)
                form.highlightTree(node.stringindex,
                     node.gettoken().Length,
                     Color.MediumPurple, node);
            else
                form.highlightTree( Color.MediumPurple, node);
            if(error != "")
                form.richTextBox3.Text += "语义错误:::::" + error + '\n';
            errorcount++;
            return false;
        }

        private bool throwErr(String error, MyNode node)
        {
            paintAllBelowToken(node);
            String errorinfo;
            if (node.leaf)
            {
                errorinfo = "语义错误:::::token:" + node.gettoken() + error;
                //form.highlightError(node.stringindex,
                //    node.gettoken().Length,
                //    Color.MediumPurple);
                form.highlightTree(Color.MediumPurple, node);

            }
            else if(node.nodes.Count == 1&&node.nodes[0].leaf)
            {
                form.highlightTree(Color.MediumPurple, node.nodes[0]);

                //form.highlightTree(node.nodes[0].stringindex,
                //    node.nodes[0].gettoken().Length,
                //    Color.MediumPurple,node.nodes[0]);
                form.highlightTree(Color.MediumPurple,node);
                errorinfo = "语义错误:::::" + error;
                errorinfo = "语义错误:::::token:" + node.nodes[0].gettoken();
            }
            else
            { 
                form.highlightTree(Color.MediumPurple, node);
                errorinfo = "语义错误:::::" + error;
            }
            node.treenode.BackColor = Color.MediumPurple;
            if (error != "") form.richTextBox3.Text += errorinfo+'\n' ;
            errorcount++;
            return false;
        }



        public bool startSemantic()
        {
            print("开始语义分析执行",no,no,no,no,no);
            errorcount = 0;
            paint = true;
            program();
            return true;
        }

        public bool program()
        {
            MyNode node = mainnode;
            if(node.nodes[0].nodes[1].gettoken()!="main")
            {
                return throwErr("程序无main函数", node.nodes[0].nodes[1]);
            }
            if (!FunctionDefinition(node.nodes[0]))
                return throwErr("函数定义语义错误，", node);
            return true;

        }


        public bool FunctionDefinition(MyNode node)
        {
            if (node.info != "函数定义") return false;
            addTable();
            int temp = 0;
            if (!CompoundSentence(node.nodes[node.nodes.Count-1], ref temp))
                return throwErr("函数定义中语句列表语义错误", node);
            return true;
        }
        String equal = "=";

        public bool Marker(MyNode node, ref Sign lastsign)
        {//标志符
            if (node.info != "标志符") return false;

            Sign marker = toptable.findSign(node.nodes[0].gettoken());
            if(marker != null)
            {//标志符已定义
                if(marker.hasvalue)
                {//标志符有初值
                    lastsign.type = marker.type;
                    lastsign.value = marker.value;
                    lastsign.name = marker.name;
                    lastsign.hasvalue = true;
                    print("取数" , marker.type, marker.name ,no,no,marker.value);
                }
                //else
                //    return throwErr("标志符未赋初值", node, true);
            }
            else
                return throwErr("标志符未定义",node,true);
            return true;
        }
        public bool Constant(MyNode node,ref Sign lastsign)
        {//常数
            if (node.info != "常数") return false;
            lastsign.type = "int";
            lastsign.setIntValue(Convert.ToInt32(node.nodes[0].gettoken()));
            lastsign.hasvalue = true;
            print("常数", "int", no, no, equal, lastsign.value);
            return true;
        }

        public bool SentenceList(MyNode node,ref int breakcontinuereturn)
        {//语句列表
            if (node.info != "语句列表") return false;
            if (!Sentence(node.nodes[0],ref breakcontinuereturn))
            {
                //抛出错误 语句语义错误
                return throwErr("语句列表中语义错误", node);
            }
            //break 或者continue 或者 return 直接返回,到了循环中判断
            if (breakcontinuereturn > 0 && loopnumber > 0)
            {
               // loopnumber--;
                return true;
            }
            //判断后继的语句列表
            if (node.nodes.Count > 1)
            {
                if (!SentenceList(node.nodes[1], ref breakcontinuereturn))
                    return throwErr("语句列表中后继语句列表语义错误", node);
            }
            return true;
        }

        public bool Sentence(MyNode node,ref int breakcontinuereturn)
        {
            if (node.info != "语句") return false;
            switch (node.nodes[0].info)
            {
                case "复合语句":
                    if (!CompoundSentence(node.nodes[0], ref breakcontinuereturn))
                        return throwErr("语句语义错误", node);
                    break;
                case "表达式语句":
                    if (!ExpressionSentence(node.nodes[0], ref breakcontinuereturn))
                        return throwErr("语句语义错误", node);
                    break;
                case "条件语句":
                    if (!ConditionSentence(node.nodes[0], ref breakcontinuereturn))
                        return throwErr("语句语义错误", node);
                    break;
                case "循环语句":
                    if (!LoopSentence(node.nodes[0], ref breakcontinuereturn))
                        return throwErr("语句语义错误", node);
                    break;
                default:
                    //语法分析中应该保证了不会进入这个分支
                    //throwErr
                    return false;
            }
            return true;


        }

        public bool CompoundSentence(MyNode node, ref int breakcontinuereturn)
        {//复合语句
            if (node.info != "复合语句") return false;
            //保存符号表栈顶,以供回退
            print("保存符号表栈顶", no, no, no, no, no);
            int regionsigntop = toptable.signtop;
            if(node.nodes.Count == 3&&!SentenceList(node.nodes[1],ref breakcontinuereturn))
            {
                return throwErr("复合语句中语句列表语义错误", node);
            }
            //此处不用处理breakcontinuereturn，返回的函数已经忽略了后面的语句
            //回退到旧符号表栈顶
            print("回退符号表栈顶", no, no, no, no, no);
            toptable.signtop = regionsigntop;
            toptable.table.RemoveRange(regionsigntop + 1, toptable.table.Count - 1 - regionsigntop);
            return true;
        }

        public bool ExpressionSentence(MyNode node,ref int breakcontinuereturn)
        {//表达式语句
            if (node.info != "表达式语句") return false;
            if (Expression(node.nodes[0],ref breakcontinuereturn))
                return true;
            else if (node.nodes.Count == 1)//当该表达式语句只有一个;时
                return true;
            else
                return throwErr("表达式语句语义错误", node);
        }

        

        public bool ConditionSentence(MyNode node,ref int beakconreturn)
        {//<条件语句>		->if (<逻辑表达式>) <语句> <elseif>
         //< elseif >        -> else < 语句 > | 空
            if (node.info != "条件语句") return false;
            bool result = true;
            bool condition = false;
            Sign resultsigh = new Sign();
            if(!LogicExpression(node.nodes[2], ref resultsigh))
            {
                return throwErr("条件语句中逻辑表达式语义错误", node);
            }
            condition = resultsigh.type == "bool" ? 
                resultsigh.getBoolValue() : (resultsigh.type == "int" ? 
                (resultsigh.getIntValue() > 0 ? true : false) : 
                (resultsigh.getFloatValue() > 0 ? true : false));
            if (condition)
            {
                print("if", no, no, no, no, "true");
                if (!Sentence(node.nodes[4], ref beakconreturn))
                    return throwErr("条件语句中语句语义错误", node);
                if (beakconreturn>0)
                    return true;
            }
            else if (node.nodes.Count == 6)
            {
                if (!ElseIf(node.nodes[5], ref beakconreturn))
                    return throwErr("条件语句中else语义错误", node);
                if (beakconreturn > 0)
                    return true;
            }
            return true;
        }

        public bool LoopSentence(MyNode node, ref int breakcontinuereturn)
        {//<循环语句>		->while(<逻辑表达式>)<语句>
            if (node.info != "循环语句") return false;
            //|for(<声明语句>|<赋值语句>;<逻辑表达式>;<自增减>|<赋值表达式>)<语句>
            int regionsigntop = toptable.signtop;
            bool result = true;
            loopnumber++;
            Sign nextsign = new Sign(),resultsign = new Sign();
            switch (node.nodes[0].info)
            {
                case "while":
                    print("while", no, no, no, no, no);
                    if (!LogicExpression(node.nodes[2], ref resultsign))
                        return throwErr("while中逻辑表达式语义错误", node);
                    while(resultsign.getBoolValue())
                    { //执行循环语句
                        print("进入循环", no, no, no, no, no);
                        breakcontinuereturn = 0;
                        if (!Sentence(node.nodes[4], ref breakcontinuereturn))
                            return throwErr("while中语句语义错误", node);
                        if (breakcontinuereturn == BREAK) break;
                        if (breakcontinuereturn == RETURN) return true;
                       // if (breakcontinuereturn == CONTINUE) loopnumber++;
                        LogicExpression(node.nodes[2], ref resultsign);
                    }
                    loopnumber--;

                    return true;
                case "for":
                    print("for", no, no, no, no, no);
                    int i = -1;
                    if (node.nodes.Count == 9)
                    {
                        i = 0;
                        if ((!DeclarationSentence(node.nodes[2]))
                            && (!AssignmentSentence(node.nodes[2])))
                            return throwErr("for中声明语句或赋值语句语义错误", node.nodes[2]);
                    }
                    if (!LogicExpression(node.nodes[4+i],ref resultsign))
                        return throwErr("for中逻辑表达式语义错误", node);
                    while(resultsign.getBoolValue())
                    {//执行循环语句
                        print("进入循环", no, no, no, no, no);
                        breakcontinuereturn = 0;
                        if (!Sentence(node.nodes[8+i], ref breakcontinuereturn))
                            return throwErr("for中语句语义错误", node);
                        if (breakcontinuereturn == BREAK) break;
                        if (breakcontinuereturn == RETURN) return true;
                       // if (breakcontinuereturn == CONTINUE) loopnumber++;
                        if ((!PlusMinus(node.nodes[6+i], ref emptysign))&&(!AssignmentSentence(node.nodes[6+i])))
                            return throwErr("for中自增减或赋值表达式语义错误", node.nodes[6 + i]);
                        LogicExpression(node.nodes[4 + i], ref resultsign);
                        
                    }
                    loopnumber--;
                    toptable.signtop = regionsigntop;
                    toptable.table.RemoveRange(regionsigntop + 1, toptable.table.Count - 1 - regionsigntop);
                    return true;
            }
            return true;
        }

        public bool DeclarationSentence(MyNode node)
        {//<声明语句>		-><类型><声明语句2>
            if (node.info != "声明语句") return false;
            if (node.nodes.Count == 3)
                return false;
            String signtype = node.nodes[0].nodes[0].gettoken();
            if (!DeclarationSentence2(node.nodes[1], signtype))
                return throwErr("声明语句中声明语句2语义错误", node);
            return true;
        }
      

        public bool DeclarationSentence2(MyNode node,String signtype)
        {//<声明语句2>		-><声明语句3>,<声明语句2>|<声明语句3>
            if (node.info != "声明语句2") return false;
            if (!DeclarationSentence3(node.nodes[0], signtype))
                return throwErr("声明语句2中声明语句3语义错误", node);
            if(node.nodes.Count==3)
                if (!DeclarationSentence2(node.nodes[2],signtype))
                    return throwErr("声明语句3中后继声明语句2语义错误", node);
            return true;
        }
        public bool DeclarationSentence3(MyNode node, String signtype)
        {//<声明语句3>		-><数组>=<数组赋值>|<数组>|<赋值语句>|<标志符>
            if (node.info != "声明语句3") return false;
            MyNode mynode = node.nodes[0];
            String name;
            Sign sign;
            switch(mynode.info)
            {
                case "数组":
                    //先将数组加入到符号表中
                    name = mynode.nodes[0].nodes[0].gettoken();
                    sign = new Sign(name, signtype);
                    toptable.addSign(sign);
                    //object array = new object();
                    //分配数组空间
                    //switch (signtype)
                    //{
                    //    case "int":
                    //        array = new ArrayList(Convert.ToInt32(node.nodes[0].nodes[2].gettoken()));
                    //        break;
                    //    case "float":
                    //        array = new List<float>(Convert.ToInt32(node.nodes[0].nodes[2].gettoken()));
                    //        break;
                    //    case "bool":
                    //        array = new List<bool>(Convert.ToInt32(node.nodes[0].nodes[2].gettoken()));
                    //        break;
                    //}
                    ArrayList array = new ArrayList(Convert.ToInt32(node.nodes[0].nodes[2].gettoken()));
                    sign.array = array;
                    sign.arraytop = -1;
                    Sign arraycountsign = new Sign();
                    if (!MathematicExpression(mynode.nodes[2], ref arraycountsign))
                        return throwErr("数组下标语义错误",node);
                    if (arraycountsign.type != "int")
                        return throwErr("数组下标只可以为整型", node);
                    sign.arraycount = arraycountsign.getIntValue();
                    for(int i =0;i<arraycountsign.getIntValue();i++)
                    {
                        sign.array.Add(0);
                    }

                    if (node.nodes.Count == 3)
                    {//有数组赋值
                        if (!ArrayAssignment(node.nodes[2], sign,signtype))
                            return throwErr("数组赋值语义错误", node);
                    }
                    break;
                case "赋值语句":
                    //先将标志符加入到符号表中，再进入赋值语句
                    /////赋值语句.标志符/数组.token
                    if(mynode.nodes[0].info=="数组")
                    {//语法分析将这种情况排除在外了
                    }
                    else
                    {//标志符
                        name = mynode.nodes[0].nodes[0].gettoken();
                        sign = new Sign(name, signtype);//
                        if(!toptable.addSign(sign))
                        {
                            //存在同名的标志符
                            return throwErr("声明语句中标志符重名",node);
                        }
                    }
                    if(!AssignmentSentence(mynode))
                        return throwErr("声明语句中赋值语句语义错误", node);
                    return true;
                case "标志符":
                    name = mynode.nodes[0].gettoken();
                    sign = new Sign(name,signtype);
                    if (!toptable.addSign(sign))
                    {
                        //存在同名的标志符
                        return throwErr("声明语句中标志符重名", node);
                    }
                    break;
            }
            return true;
        }

        public bool ArrayAssignment(MyNode  node, Sign sign,String type)
        {//<数组赋值>		->{<数组赋值2>}
            if (node.info != "数组赋值") return false;
            if (!ArrayAssignment2(node.nodes[1], sign, 0,type))
                return throwErr("数组赋值中数组赋值2语义错误", node);            
            return true;
        }
        int nowindex = 0;
        public bool ArrayAssignment2(MyNode node, Sign sign, int index,String type)
        {//< 数组赋值2 > ->< 算术表达式 >|< 算术表达式 >,< 数组赋值2 >|<逻辑表达式>|<逻辑表达式>,<数组赋值2>
            if (node.info != "数组赋值2") return false;
            Sign tempsign = new Sign();
            if (node.nodes.Count == 0)
                return true;
            if(type == "bool")
            {
                Sign resultsign = new Sign();
                if (node.nodes[0].info=="逻辑表达式")
                {//类型判断
                    resultsign.value = false.ToString();
                    if (!LogicExpression(node.nodes[0], ref resultsign))
                        return throwErr("数组赋值中逻辑表达式语义错误", node);
                    sign.addIntoArray(resultsign.getBoolValue());
                }
                else
                    return throwErr("数组赋值中类型不符", node);
            }
            else
            {
                if(node.nodes[0].info == "算术表达式")
                {
                    //float 和 int
                    if (!MathematicExpression(node.nodes[0], ref tempsign))
                        return throwErr("数组赋值中算术表达式语义错误", node);
                    //if (tempsign.type != type)
                    //    return throwErr("数组赋值中算术表达式类型不符", node);
                    if(tempsign.type == "bool")
                        return throwErr("数组赋值中算术表达式类型不符", node);
                    if (tempsign.type == "int")
                    {
                        sign.array[nowindex] = tempsign.getIntValue();
                        nowindex++;
                    }
                    else
                    {
                        sign.array[nowindex] = tempsign.getFloatValue();
                        nowindex++;
                    }
                }
                else
                    return throwErr("数组赋值中类型不符", node);
            }
            if (node.nodes.Count == 3)
            {
                if (!ArrayAssignment2(node.nodes[2], sign, index + 1,type))
                    return throwErr("数组赋值2中后继语句语义错误", node);
            }
            nowindex--;
            return true;
        }


        public bool Expression(MyNode node,ref int breakcontinuereturn)
        {
            if (node.info != "表达式") return false;
            Sign sign = new Sign();
            //当break和continue要更新两个布尔值
            switch (node.nodes[0].info)
            {
                case "声明语句":
                    return DeclarationSentence(node.nodes[0]);
                case "赋值语句":
                    return AssignmentSentence(node.nodes[0]);
                case "自增减":
                    return PlusMinus(node.nodes[0],ref sign);
                case "返回语句":
                    if (!Return(node.nodes[0]))
                        return throwErr("表达式中返回语句语义错误",node, true);
                    print("RETURN", no, no, no, no, no);//返回类型返回值判断**********************************
                    //保存当前栈顶函数的类型，保存返回值
                    breakcontinuereturn = 3;
                    break;
                case "BREAKCONTINUE":
                    if(!BREAKCONTINUE(node.nodes[0], ref breakcontinuereturn))
                        return throwErr("当前没有对应的循环", node, true);
                    
                    break;
                case "函数调用":
                    if (!FunctionCall(node.nodes[0], ref sign))
                        return throwErr("函数调用错误", node);
                    //***************************
                    break;
            }
            return true;
        }

        public bool LogicExpression(MyNode node, ref Sign lastresult)
        {
            if (node.info != "逻辑表达式") return false;
            bool nextresult1 = false, nextresult2 = false;
            lastresult.type = "bool";
            if (!AND(node.nodes[0],ref nextresult1))
                return throwErr("逻辑表达式中AND语句语义错误", node);
            if (nextresult1)
            {
                lastresult.setValue("true");
                print("逻辑表达式", nextresult1.ToString(), "||", nextresult2.ToString(), equal, lastresult.value);
                return true;
            }
             
            if (node.nodes.Count == 3)
            {
                Sign subsign = new Sign();
                if (!LogicExpression(node.nodes[2], ref subsign))
                    return throwErr("逻辑表达式中后继逻辑表达式语义错误", node);
                else
                    lastresult.setValue(subsign.getBoolValue().ToString());
                print("逻辑表达式", nextresult1.ToString(), "||", nextresult2.ToString(), equal, lastresult.value);
            }
            else
            {
                lastresult.setValue(nextresult1.ToString());
                print("逻辑表达式", nextresult1.ToString(), no, no, equal, nextresult1.ToString());
            }
            return true;
        }


        public bool AND(MyNode node,ref bool lastresult)
        {
            if (node.info != "AND") return false;
            bool nextresult1 = false, nextresult2 = false;
            if (!FACTOR(node.nodes[0], ref nextresult1))
                return throwErr("AND语句中FACTOR语义错误", node);
            if(!nextresult1)
            {
                lastresult = false;
                print("AND", nextresult1.ToString(), "&&", nextresult2.ToString(), equal, lastresult.ToString());
                return true;

            }
            if (node.nodes.Count == 3)
            {
                if (!AND(node.nodes[2], ref nextresult2))
                    return throwErr("AND语句中后继AND语义错误", node);
                lastresult =  nextresult2;
                print("AND", nextresult1.ToString(), "&&", nextresult2.ToString(), equal, lastresult.ToString());
            }
            else
            {
                lastresult = nextresult1;
                print("AND", nextresult1.ToString(), no, no, equal, nextresult1.ToString());
            }
            return true;
        }

        public bool FACTOR(MyNode node, ref bool lastresult)
        {//< FACTOR > ->true|false|<算术表达式>==<算术表达式> |<算术表达式>!=<算术表达式> | <算术表达式>>=<算术表达式>|<算术表达式><=<算术表达式>|(<逻辑表达式>)|<算术表达式>
            if (node.info != "FACTOR") return false;
            Sign factor = new Sign();
            factor.type = "bool";
            if (node.nodes[0].leaf)
            {
                if(node.nodes[0].gettoken() == "true")
                {
                    factor.value = "true";
                    lastresult = true;
                }
                else if (node.nodes[0].gettoken() == "false")
                {
                    factor.value = "false";
                    lastresult = false;
                }
                else
                {//(
                    if (!LogicExpression(node.nodes[1], ref factor))
                        return throwErr("FACTOR括号内逻辑表达式语义错误", node);
                    lastresult = factor.getBoolValue();
                }
                print("FACTOR", lastresult.ToString(), no, no, equal, lastresult.ToString());
            }
            else
            {
                Sign leftsign = new Sign(), rightsign = new Sign();
                if (!MathematicExpression(node.nodes[0], ref leftsign))
                    return throwErr("FACTOR中左部算术表达式语义错误",node);
                if(node.nodes.Count == 3)
                {
                    if (!MathematicExpression(node.nodes[2], ref rightsign))
                        return throwErr("FACTOR中右部算术表达式语义错误", node);
                    if (leftsign.type != rightsign.type)
                    {
                        if (leftsign.type != "bool" && rightsign.type != "bool")
                        {
                            leftsign.type = rightsign.type = "float";
                        }
                        else
                        {
                            return throwErr("左右类型不可比较", node);
                        }
                    }
                        lastresult = compare(leftsign.value, rightsign.value, node.nodes[1].gettoken(), leftsign.type);
                    
                   // return throwErr("逻辑算符左右两式类型不同", node,true);
                    print("逻辑运算", leftsign.value, node.nodes[1].gettoken(), rightsign.value, equal, lastresult.ToString());
                    return true;
                }
                else
                {
                    if (leftsign.type != "bool")
                    {
                        if (leftsign.type == "int")
                        {
                            int boolint = leftsign.getIntValue();
                            if (boolint > 0)
                                lastresult = true;
                            else
                                lastresult = false;
                        }
                        else if(leftsign.type == "float")
                        {
                            float floatint = leftsign.getFloatValue();
                            if (floatint > 0)
                                lastresult = true;
                            else
                                lastresult = false;
                        }
                        else
                            return throwErr("FACTOR中单独算术表达式的类型错误", node, true);
                    }
                    else 
                        lastresult = leftsign.getBoolValue();
                    print("FACTOR", lastresult.ToString(), no, no, equal, lastresult.ToString());
                    return true;
                }
            }
            return true;
        }

        public bool compare(String a, String b, String operation, String type)
        {
            if (type == "int")
            {
                int aint = Convert.ToInt32(a), bint = Convert.ToInt32(b);
                switch (operation)
                {
                    case "<":
                        return (aint < bint);
                    case ">":
                        return (aint > bint);
                    case "==":
                        return (aint == bint);
                    case "<>":
                        return (aint == bint);
                    case "!=":
                        return (aint != bint);
                    case ">=":
                        return (aint >= bint);
                    case "<=":
                        return (aint <= bint);
                }
            }
              
            else if(type == "float")
                switch (operation)
                {
                    case "<":
                        return Convert.ToSingle(a) < Convert.ToSingle(b);
                    case ">":
                        return Convert.ToSingle(a) > Convert.ToSingle(b);
                    case "==":
                        return Convert.ToSingle(a) == Convert.ToSingle(b);
                    case "<>":
                        return Convert.ToSingle(a) != Convert.ToSingle(b);
                    case "!=":
                        return Convert.ToSingle(a) != Convert.ToSingle(b);
                    case ">=":
                        return Convert.ToSingle(a) >= Convert.ToSingle(b);
                    case "<=":
                        return Convert.ToSingle(a) <= Convert.ToSingle(b);
                }
            return false;
        }

        public bool ElseIf(MyNode node,ref int breakcontinuereturn)
        {
            print("else", no, no, no, no, "true");
            if (!Sentence(node.nodes[1], ref breakcontinuereturn))
                return throwErr("ELSEIF语句后继语句语义错误", node);
            return true;
        }

        public bool AssignmentSentence(MyNode node)
        {
            if (node.info != "赋值语句") return false;
            Sign leftsign,rightsign = new Sign();
            if(node.nodes[0].info == "数组")
            {
                leftsign = toptable.findSign(node.nodes[0].nodes[0].nodes[0].gettoken());
                if (leftsign == null)
                    return throwErr("不可对未定义变量赋值", node);
                else
                {
                    print("取数", leftsign.type + "[]", leftsign.name, no, no, no);
                    int index = 0;
                    Sign arrayindex = new Sign();
                    if (!MathematicExpression(node.nodes[0].nodes[2], ref arrayindex))
                        return throwErr("数组下标语义错误", node);
                    if (arrayindex.type != "int")
                        return throwErr("数组下标只可以是整型", node);
                    index = arrayindex.getIntValue();
                    print("数组下标", no, no, no, equal, arrayindex.value);
                    //if(leftsign.type == "bool")
                    //{
                    //    if(!LogicExpression(node.nodes[2], ref rightsign))
                    //        return throwErr("赋值表达式右部算式表达式错误", node);
                    //}
                    //else if (!MathematicExpression(node.nodes[2], ref rightsign))
                    //    return throwErr("赋值表达式右部算式表达式错误", node);
                    try
                    {
                        if (!MathematicExpression(node.nodes[2], ref rightsign))
                            if (!LogicExpression(node.nodes[2], ref rightsign))
                                return throwErr("赋值表达式右部算式表达式错误", node);
                    }
                    catch (Exception e)
                    {
                        return throwErr("赋值表达式右部算式表达式错误", node);
                    }

                    print("等号右部", no, no, no, equal, rightsign.value);
                    switch (leftsign.setValue(rightsign,index))
                    {
                        case 1:
                            print("赋值", node.nodes[0].nodes[0].nodes[0].gettoken() + "[" + arrayindex.value + "]"
                                , no, no, equal, rightsign.value);
                            return true;
                        case 2:
                            throwErr("", node.nodes[2], true);
                            return throwErr("赋值表达式右部无值", node);
                        case 3:
                            if (leftsign.type == "int")
                            {
                                if (rightsign.type == "float")
                                {
                                    //rightsign.value = Convert.ToString((int)rightsign.getFloatValue()); 
                                    //rightsign.type = "int";
                                    //leftsign.setValue(rightsign, index);
                                    leftsign.array[index] = (int)rightsign.getFloatValue();
                                    return true;
                                }
                                else if(rightsign.type =="bool")
                                {
                                    leftsign.array[index] = rightsign.getBoolValue() ? 1 : 0;
                                    return true;
                                }
                                return throwErr("赋值表达式左右类型不同", node, true);
                            }
                            else if (leftsign.type == "float")
                            {
                                if (rightsign.type == "int")
                                {
                                    //leftsign.value = Convert.ToString((float)rightsign.getIntValue());
                                    //rightsign.type = "float";
                                    //leftsign.setValue(rightsign, index);
                                    leftsign.array[index] = (float)rightsign.getIntValue();

                                    return true;
                                }
                                else if (rightsign.type == "bool")
                                {
                                    leftsign.array[index] = rightsign.getBoolValue() ? 1 : 0;
                                    return true;
                                }
                                return throwErr("赋值表达式左右类型不同", node, true);
                            }
                            else
                                return throwErr("赋值表达式左右类型不同", node, true);
                    }

                }
                //数组运算！！！！！！！！！！！！！！！
            }
            else
            {//标志符
                leftsign = toptable.findSign(node.nodes[0].nodes[0].gettoken());
                if (leftsign == null)
                    return throwErr("不可对未定义变量赋值", node,true);
                else
                {
                    print("取数", leftsign.type, leftsign.name, no, no, leftsign.value);

                    //if (leftsign.type == "bool")
                    //{
                    //    try
                    //    {
                    //        if (!LogicExpression(node.nodes[2], ref rightsign))
                    //            return throwErr("赋值表达式右部算式表达式错误", node);
                    //    }
                    //    catch(Exception e)
                    //    {
                    //        return throwErr("赋值表达式右部算式表达式错误", node);
                    //    }
                    //}
                    //else if (!MathematicExpression(node.nodes[2], ref rightsign))
                    //    return throwErr("赋值表达式右部算式表达式错误", node);

                    //if(leftsign.type == "bool")
                    //{
                    //    if(!LogicExpression(node.nodes[2], ref rightsign))
                    //        return throwErr("赋值表达式右部算式表达式错误", node);
                    //}
                    //else if (!MathematicExpression(node.nodes[2], ref rightsign))
                    //    return throwErr("赋值表达式右部算式表达式错误", node);

                    try
                    {
                        if (!MathematicExpression(node.nodes[2], ref rightsign))
                        if (!LogicExpression(node.nodes[2], ref rightsign))
                            return throwErr("赋值表达式右部算式表达式错误", node);
                    }
                    catch (Exception e)
                    {
                        return throwErr("赋值表达式右部算式表达式错误", node);
                    }

                    print("等号右部", no, no, no, equal, rightsign.value);

                    switch (leftsign.setValue(rightsign))
                    {
                        case 1:
                            print("赋值",leftsign.name, no, no, equal, rightsign.value);
                            return true;
                        case 2:
                            throwErr("",node.nodes[2], true);
                            return throwErr("赋值表达式右部无值", node);
                        case 3:
                            if (leftsign.type == "int")
                            {
                                if (rightsign.type == "float")
                                {
                                    //rightsign.value = Convert.ToString((int)rightsign.getFloatValue());
                                    //rightsign.type = "int";
                                    //leftsign.setValue(rightsign);
                                    leftsign.value = Convert.ToString((int)rightsign.getFloatValue());
                                    leftsign.hasvalue = true;
                                    return true;
                                }
                                else if (rightsign.type == "bool")
                                {
                                    leftsign.value = Convert.ToString(rightsign.getBoolValue() ? 1 : 0);
                                    leftsign.hasvalue = true;
                                    return true;
                                }
                                return throwErr("赋值表达式左右类型不同", node, true);
                            }
                            else if (leftsign.type == "float")
                            {
                                if (rightsign.type == "int")
                                {
                                    //leftsign.value = Convert.ToString((float)rightsign.getIntValue());
                                    //rightsign.type = "float";
                                    //leftsign.setValue(rightsign);
                                    leftsign.value = Convert.ToString((int)rightsign.getFloatValue());
                                    leftsign.hasvalue = true;
                                    return true;
                                }
                                else if (rightsign.type == "bool")
                                {
                                    leftsign.value = Convert.ToString(rightsign.getBoolValue() ? 1 : 0);
                                    leftsign.hasvalue = true;
                                    return true;
                                }
                                return throwErr("赋值表达式左右类型不同", node, true);
                            }
                            else
                                return throwErr("赋值表达式左右类型不同", node, true);
                    }
                }
            }
            return true;
        }
        Sign emptysign = new Sign();

        public bool MathematicExpression(MyNode node, ref Sign lastsign)
        {//<算术表达式>	-><算术表达式2>+<算术表达式>|<算术表达式2>-<算术表达式>|<算术表达式2>
         //tempsign 保存类别，值，
         //int 和 float操作 得到float
         //int 和int得到int
         //int不可和bool操作
            if (node.info != "算术表达式") return false;
            Sign nextsign1 = new Sign(), nextsign2 = new Sign();
            //递归计算左节点
            if (!Math2(node.nodes[0], ref nextsign1))
                return throwErr("算术表达式语义错误", node);
            if(node.nodes.Count==3)
            {
                print("加减算符左部", no, no, no, equal, nextsign1.value);
                int operation = 1;
                if (node.nodes[1].gettoken() == "-")
                    operation = -1;
                //计算右节点
                if (!MathematicExpression(node.nodes[2],ref nextsign2))
                    return throwErr("算术表达式后继语句语义错误", node);

                if (!nextsign2.hasvalue || !nextsign1.hasvalue) return throwErr("标志符未赋初值", node);
                print("加减算符右部", no, no, no, equal, nextsign2.value);

                if (nextsign1.type == "int")
                {//i
                    if(nextsign2.type == "int")
                    {//ii
                        lastsign.type = "int";
                        lastsign.setValue( Convert.ToString(Convert.ToInt32(nextsign1.getIntValue() + operation * nextsign2.getIntValue())));
                    }
                    else
                    {//if
                        lastsign.type = "float";
                        lastsign.setValue(Convert.ToString(nextsign1.getIntValue() + operation * nextsign2.getFloatValue()));
                    }
                }
                else
                {//f
                    if (nextsign2.type == "int")
                    {//fi
                        lastsign.type = "float";
                        lastsign.setValue(Convert.ToString(nextsign1.getFloatValue() + operation * nextsign2.getIntValue()));                    }
                    else
                    {//ff
                        lastsign.type = "float";
                        lastsign.setValue( Convert.ToString(nextsign1.getFloatValue() + operation * nextsign2.getFloatValue()));
                    }
                }
                print("加减运算", nextsign1.value, node.nodes[1].gettoken(), nextsign2.value, equal, lastsign.value);
            }
            else
            {
                lastsign = nextsign1;
                lastsign.hasvalue = true;
                print("Math1", nextsign1.value, no, no, equal, nextsign1.value);
            }
            return true;
        }

        public bool Math2(MyNode node, ref Sign lastsign)
        {//<算术表达式2>	-><算术表达式3>*<算术表达式2>|<算术表达式3>/<算术表达式2>|<算术表达式3>
            if (node.info != "算术表达式2") return false;
            Sign nextsign1 = new Sign(), nextsign2 = new Sign();
            //递归计算左节点
            if (!Math3(node.nodes[0], ref nextsign1))
                if(!LogicExpression(node.nodes[0],ref nextsign1))
                    return throwErr("", node);
            if (node.nodes.Count == 3)
            {
                print("乘除算符左部", no, no, no, equal, nextsign2.value);
                //计算右节点
                if (!Math2(node.nodes[2], ref nextsign2))
                    if (!LogicExpression(node.nodes[0], ref nextsign1))
                        return throwErr("算术表达式后继语句语义错误", node);
                if (!nextsign2.hasvalue || !nextsign1.hasvalue) return throwErr("标志符未赋初值", node);

                if (nextsign1.type == "bool")
                {
                    nextsign1.type = "int";
                    nextsign1.value = nextsign1.value == "true" ? "1" : "0";
                }
                if (nextsign2.type == "bool")
                {
                    nextsign2.type = "int";
                    nextsign2.value = nextsign1.value == "true" ? "1" : "0";
                }

                print("乘除算符右部", no, no, no, equal, nextsign2.value);
                int operation = 1;
                if (node.nodes[1].gettoken() == "/")
                {
                    operation = -1;
                    //nextsign2.type = "float";
                    if(nextsign2.getFloatValue() == 0.0f)
                    {
                        return throwErr("除法右部不可以为0", node, true);
                    }
                }
                if (nextsign1.type == "int")
                {//i
                    if (nextsign2.type == "int")
                    {//ii
                        lastsign.type = "int";
                        lastsign.setValue(Convert.ToString(Convert.ToInt32(nextsign1.getIntValue() * Math.Pow(nextsign2.getIntValue(), operation))));
                    }
                    else
                    {//if
                        lastsign.type = "float";
                        lastsign.setValue(Convert.ToString(nextsign1.getIntValue() * Math.Pow(nextsign2.getFloatValue(), operation)));
                    }
                }
                else
                {//f
                    if (nextsign2.type == "int")
                    {//fi
                        lastsign.type = "float";
                        lastsign.setValue(Convert.ToString(nextsign1.getFloatValue() * Math.Pow(nextsign2.getIntValue(), operation)));
                    }
                    else
                    {//ff
                        lastsign.type = "float";
                        lastsign.setValue(Convert.ToString(nextsign1.getFloatValue() * Math.Pow(nextsign2.getFloatValue(), operation)));
                    }
                }
                print("乘除运算", nextsign1.value, node.nodes[1].gettoken(), nextsign2.value, equal, lastsign.value);

            }
            else
            {//单独的算术表达式3
                lastsign = nextsign1;
                lastsign.hasvalue = true;
                bool a = true;
                print("Math2", nextsign1.value, no, no, equal, nextsign1.value);

            }
            return true;
        }

        public bool Math3(MyNode node, ref Sign lastsign)
        {//<算术表达式3>	->(<算术表达式>)|<算术表达式4>
            if (node.info != "算术表达式3") return false;
            Sign nextsign = new Sign();
            if(node.nodes.Count == 3)
            {
                if (!MathematicExpression(node.nodes[1], ref nextsign))
                    return throwErr("算术表达式3括号内算术表达式语义错误", node);
                lastsign = nextsign;
            }
            else
            {
                if (!Math4(node.nodes[0], ref nextsign))
                    return throwErr("算术表达式3后继算术表达式4语义错误", node);
                lastsign = nextsign;
            }
            print("Math3", nextsign.value, no, no, equal, nextsign.value);
            return true;
        }

        public bool Math4(MyNode node,ref Sign lastsign)
        {//<算术表达式4>	-><负数>|<常数>|<浮点数>|<自增减>|<数组>|<函数调用>|<标志符>
            if (node.info != "算术表达式4") return false;
            Sign nextsign = new Sign();
            switch (node.nodes[0].info)
            {
                case "负数":
                    if (!Negative(node.nodes[0], ref nextsign))
                        return throwErr("算术表达式4下负数语义错误",node);
                    break;
                case "常数":
                    if (!Constant(node.nodes[0], ref nextsign))
                        return throwErr("算术表达式4下常数语义错误", node);
                    break;
                case "浮点数":
                    if (!Float(node.nodes[0], ref nextsign))
                        return throwErr("算术表达式4下浮点数语义错误", node);
                    break;
                case "自增减":
                    if (!PlusMinus(node.nodes[0], ref nextsign))
                        return throwErr("算术表达式4下自增减语义错误", node);
                    break;
                case "数组":
                    if (!Array(node.nodes[0], ref nextsign))
                        return throwErr("算术表达式4下数组语义错误", node);
                    break;
                case "函数调用":
                    if (!FunctionCall(node.nodes[0], ref nextsign))
                        return throwErr("算术表达式4下函数调用语义错误", node);
                    break;
                case "标志符":
                    if (!Marker(node.nodes[0], ref nextsign))
                        return throwErr("算术表达式4下标志符语义错误", node);
                    break;
            }
            lastsign = nextsign;
            lastsign.hasvalue = true;
            
            print("Math4", nextsign.value, no, no, equal, nextsign.value);

            return true;
        }

        public bool FunctionCall(MyNode node, ref Sign returnsign)
        {
            if (node.info != "函数调用") return false;
            Sign sign = new Sign();
            if (node.nodes.Count != 4)
                return throwErr("函数调用缺少参数", node);

            if (node.nodes[0].nodes[0].gettoken()=="print"|| node.nodes[0].nodes[0].gettoken() =="printf" || node.nodes[0].nodes[0].gettoken() == "write")
            {//打印函数
                if (!MathematicExpression(node.nodes[2], ref sign))
                {
                    if(!LogicExpression(node.nodes[2], ref sign))
                        if(!Array(node.nodes[2],ref sign))
                            return throwErr("函数调用中参数语义错误", node, true);
                }
                //if(sign.type == "float")
                //    sign.value = ((decimal)sign.getFloatValue()).ToString();
                form.richTextBox3.Text += sign.value+"\n";
                print("打印数据", no, no, no, no, sign.value);
                return true;
            }
            else if(node.nodes[0].nodes[0].gettoken() == "scanf"|| node.nodes[0].nodes[0].gettoken() =="scan" || node.nodes[0].nodes[0].gettoken() == "read")
            {//输入函数
                MyNode mynode = node.nodes[2];
                if (node.nodes[2].info=="算术表达式")
                {
                    if (!MathematicExpression(node.nodes[2], ref sign))
                    {
                        if(!LogicExpression(node.nodes[2],ref sign))
                            if (!Array(node.nodes[2], ref sign))
                                return throwErr("函数调用中参数语义错误", node, true);
                    }
                    Input inputform = new Input();
                    inputform.ShowDialog();
                    int start = node.nodes[2].stringindex;
                    Sign assignsign = new Sign();
                    assignsign = null;
                    String name = node.nodes[2].nodes[0].nodes[0].nodes[0].nodes[0].nodes[0].gettoken();
                    if (/*sign.name*/name != null)
                    {
                        assignsign = toptable.findSign(/*sign.name*/name);
                        String input = click.value;
                        assignsign.setValue(input);
                        try
                        {
                            switch (assignsign.type)
                            {
                                case "float":
                                    assignsign.getFloatValue();
                                    break;
                                case "int":
                                    assignsign.getIntValue();
                                    break;
                                case "bool":
                                    assignsign.getBoolValue();
                                    break;
                            }
                        }
                        catch (Exception e)
                        {
                            return throwErr("输入的数据类型错误", node, true);
                        }
                        print("读入数据", assignsign.name, no, no, no, input);
                        return true;
                    }
                    else if(node.nodes[2].nodes[0].nodes[0].nodes[0].nodes[0].info == "数组")
                    {
                        mynode = node.nodes[2].nodes[0].nodes[0].nodes[0].nodes[0];
                    }
                    else
                        return throwErr("括号中变量错误", node, true);

                }
                if (mynode.info == "数组"/*node.nodes[2].info == "数组"*/)
                {
                    Sign backsign = new Sign(), arraysign = new Sign();
                    int index = 0;
                    Sign arrayindex = new Sign();
                    arraysign = toptable.findSign(mynode.nodes[0].nodes[0].gettoken());
                    if (arraysign == null)
                        return throwErr("数组未定义", mynode);
                    print("取数", arraysign.type + "[]", arraysign.name, no, no, no);
                    if (!MathematicExpression(mynode.nodes[2], ref arrayindex))
                        return throwErr("数组下标语义错误", mynode);
                    if (arrayindex.type != "int")
                        return throwErr("数组下标只可以是整型", mynode);
                    index = arrayindex.getIntValue();
                    print("数组下标", no, no, no, equal, index.ToString());
                    if (index >= arraysign.array.Count)
                        return throwErr("数组访问越界", mynode);
                    String input = click.value;
                    print("数组赋值", no, no, no, equal, input);
                    arraysign.array[index] = input;
                    backsign.setValue(arraysign.array[index].ToString());
                    backsign.type = arraysign.type;

                    try
                    {
                        switch (backsign.type)
                        {
                            case "float":
                                backsign.getFloatValue();
                                break;
                            case "int":
                                backsign.getIntValue();
                                break;
                            case "bool":
                                backsign.getBoolValue();
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        return throwErr("输入的数据类型错误", node, true);
                    }
                    return true;
                }
            }
            else
            {//非法调用函数
                return throwErr("非法调用函数", node);
            }

            return true;
            /* Sign sign = new Sign();
             Function function;
             function = findFunction(node.nodes[0].nodes[0].gettoken());
             if (function == null)
                 return throwErr("不可调用未定义的函数", node);
             if (node.nodes.Count == 3)
             {//无参数调用
             }
             else
             {
                 //有参数调用

             }
             return true;*/
        }

        public Function findFunction(String functionname)
        {
            if (functionlist.Exists(s => (s.name == functionname)))
            {
                return functionlist.Find(s => (s.name == functionname));
            }
            else
                return null;

        }
        public bool Float(MyNode node, ref Sign lastsign)
        {
            lastsign.type = "float";
            lastsign.setFloatValue(Convert.ToSingle(node.nodes[0].gettoken()));
            lastsign.hasvalue = true;
            return true;
        }

        public bool Array(MyNode node, ref Sign lastsign)
        {
            if (node.info != "数组") return false;
            Sign backsign = new Sign(),arraysign = new Sign();
            int index = 0;
            Sign arrayindex = new Sign();
            arraysign = toptable.findSign(node.nodes[0].nodes[0].gettoken());
            if (arraysign == null)
                return throwErr("数组未定义", node);
            print("取数", arraysign.type + "[]", arraysign.name, no, no, no);
            if (!MathematicExpression(node.nodes[2], ref arrayindex))
                return throwErr("数组下标语义错误", node);
            if (arrayindex.type != "int")
                return throwErr("数组下标只可以是整型", node);
            index = arrayindex.getIntValue();
            print("数组下标", no, no, no, equal, index.ToString());
            if (index >= arraysign.array.Count)
                return throwErr("数组访问越界", node);
            backsign.setValue(arraysign.array[index].ToString());
            backsign.type = arraysign.type;
            lastsign = backsign;
            lastsign.hasvalue = true;
            print("数组取值", no, no, no, equal, backsign.value);
            return true;
        }
        public bool Negative(MyNode node, ref Sign lastsign)
        {//<负数>			->-<算术表达式4>
            if (node.info != "负数") return false;
            Sign nextsign = new Sign();
            if (!Math4(node.nodes[1], ref nextsign))
                return throwErr("负数后算术表达式4语义错误", node);
            lastsign.type = nextsign.type;
            if (nextsign.type == "int")
                lastsign.setValue( Convert.ToString(nextsign.getIntValue() * (-1)));
            else if (nextsign.type == "float")
                lastsign.setValue( Convert.ToString(nextsign.getFloatValue() * (-1)));
            print("负数", no, "-", nextsign.value, equal, lastsign.value);
            return true;
        }

        public bool PlusMinus(MyNode node, ref Sign lastsign)
        {//<自增减>		-><标志符><自增减符>|<自增减符><标志符>|<数组><自增减符>|<自增减符><数组>
            if (node.info != "自增减") return false;
            Sign sign = new Sign();
            Sign backsign = new Sign();
            Sign indexsign = new Sign();
            int arrayindex;
            switch (node.nodes[0].info)
            {
                case "标志符":
                    sign = toptable.findSign(node.nodes[0].nodes[0].gettoken());
                    if (sign == null)
                        return throwErr("标志符未定义", node);
                    if (!sign.hasvalue) return throwErr("标志符未赋初值", node);
                    print("取数", sign.type, sign.name, no, no, sign.value);
                    backsign = new Sign(sign.name, sign.type, sign.value);
                    lastsign = backsign;
                    lastsign.hasvalue = true;

                    if (node.nodes[1].gettoken() == "++")
                    {
                        if (!sign.pp())
                            return throwErr("自增操作只可用于整型或浮点数", node, true);
                    }
                    else
                    {
                        if (!sign.mm())
                            return throwErr("自减操作只可用于整型或浮点数", node, true);
                    }
                    print("自增减", backsign.value, node.nodes[1].gettoken(), no, equal, sign.value);

                    break;
                case "数组":
                    sign = toptable.findSign(node.nodes[0].nodes[0].nodes[0].gettoken());
                    if (sign == null)
                        return throwErr("数组未定义", node);
                    print("取数", sign.type + "[]", sign.name, no, no, no);

                    backsign = new Sign(sign.name, sign.type);
                    lastsign = backsign;
                    lastsign.hasvalue = true;
                    if (sign.type == "bool")
                        return throwErr("自增减操作只可用于整型", node, true);
                    indexsign = new Sign();//获取数组下标
                    if (!MathematicExpression(node.nodes[0].nodes[2], ref indexsign))
                        return throwErr("数组下标语义错误", node);
                    if (indexsign.type != "int")
                        return throwErr("数组下标应该为整型", node);
                    print("数组下标", no, no, no, equal, indexsign.value);
                    arrayindex = indexsign.getIntValue();
                    if (arrayindex >= sign.array.Count)
                        return throwErr("数组访问越界", node);
                    //将值存入backsign返回
                    backsign.setValue(sign.array[arrayindex].ToString());
                    //if(node.nodes[0].nodes[2].)
                    if (node.nodes[1].gettoken() == "++")
                        sign.array[arrayindex] = Convert.ToInt32(sign.array[arrayindex]) + 1;
                    else
                        sign.array[arrayindex] = Convert.ToInt32(sign.array[arrayindex]) - 1;
                    print("自增减", sign.name + "[" + arrayindex.ToString() + "]"
                        , node.nodes[1].gettoken(), no, equal, sign.array[arrayindex].ToString());
                    break;
                default:
                    if (node.nodes[1].info == "数组")
                    {
                        sign = toptable.findSign(node.nodes[1].nodes[0].nodes[0].gettoken());

                        if (sign == null)
                            return throwErr("数组未定义", node);
                        print("取数", sign.type + "[]", sign.name, no, no, no);

                        backsign = new Sign(sign.name, sign.type);
                        lastsign = backsign;
                        lastsign.hasvalue = true;

                        if (sign.type == "bool")
                            return throwErr("自增减操作只可用于整型", node, true);
                        indexsign = new Sign();//获取数组下标
                        if (!MathematicExpression(node.nodes[1].nodes[2], ref indexsign))
                            return throwErr("数组下标语义错误", node);
                        if (indexsign.type != "int")
                            return throwErr("数组下标应该为整型", node);
                        arrayindex = indexsign.getIntValue();
                        print("数组下标", no, no, no, equal, indexsign.value);
                        if (arrayindex >= sign.array.Count)
                            return throwErr("数组访问越界", node);
                        if (node.nodes[1].gettoken() == "++")
                            sign.array[arrayindex] = Convert.ToInt32(sign.array[arrayindex]) + 1;
                        else
                            sign.array[arrayindex] = Convert.ToInt32(sign.array[arrayindex]) - 1;

                        //将值存入backsign返回
                        backsign.setValue(sign.array[arrayindex].ToString());
                        print("自增减", no, node.nodes[1].gettoken(),  sign.name+ "[" + arrayindex.ToString() + "]"
                        , equal, sign.array[arrayindex].ToString());
                        break;
                    }
                    else
                    {
                        sign = toptable.findSign(node.nodes[1].nodes[0].gettoken());
                        if (sign == null)
                            return throwErr("标志符未定义", node);
                        if (!sign.hasvalue) return throwErr("标志符未赋初值", node);

                        print("取数", sign.type, sign.name, no, no, sign.value);

                        if (node.nodes[0].gettoken() == "++")
                        {
                            if (!sign.pp())
                                return throwErr("自增操作只可用于整型或浮点数", node, true);
                        }
                        else
                        {
                            if (!sign.mm())
                                return throwErr("自减操作只可用于整型或浮点数", node, true);
                        }
                        lastsign = sign;
                        lastsign.hasvalue = true;

                        print("自增减", backsign.value, node.nodes[1].gettoken(), no, equal, sign.value);


                        sign = toptable.findSign(node.nodes[1].nodes[0].gettoken());
                        if (sign == null)
                            return throwErr("标志符未定义", node);
                        if (!sign.hasvalue) return throwErr("标志符未赋初值", node);

                        print("取数", sign.type, sign.name, no, no, sign.value);
                    }
                    break;
            }
            return true;
        }

        public bool Return(MyNode node)
        {
            return true;
        }

        public bool BREAKCONTINUE(MyNode node,ref int breakcontinuereturn)
        {
            if (node.nodes[0].gettoken() == "break")
                breakcontinuereturn = 1;
            else if (node.nodes[0].gettoken() == "continue")
                breakcontinuereturn = 2;
            if(loopnumber <= 0)  return false;
            print(node.nodes[0].gettoken(), no, no, no, no, no);//循环控制
            return true;
        }

    }
}
