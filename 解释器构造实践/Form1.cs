using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Collections;
/*
注意出错时：
token+=text[index];
index++;
type = -1;
error = "1231231321";
return type;

*/

namespace 解释器构造实践
{
    public partial class Form1 : Form
    {
        public bool clicked = false;
        private String token;           //词素
        private String text;            //代码文本字符串
        private int index = 0;          //当前读取位置
        private float number = 0;       //读入数字时的中间变量
        public int signAddress = 16;
        private String error = "";      //当前遇到的错误信息
        private String outputPath = @"E:\词法.txt";   //输入结果文件路径
        private StreamWriter stream;        //流
        public bool wordparseok = true;
        public List<TokenInfo> tokeninfos;
        private bool pastspace = false;
        public TreeView treeview;
        public ShowTree showtree;
        public bool semanticparseok = true;
        //关键字表
        private String[] table = {      //种类码表
            "#","bool", "char", "int", "float", "main", "if",
            "else", "while", "for", "break", "return","continue",
            "true","false","print123321" ,"scan123321","(",")",
            "[","]","{","}","+","-","*","/","<",">","=","<=",
            ">=","==","!=",",",";","常数","标志符","<>","||",
            "&&","++","--" ,"浮点数"};


        private bool isNumber(char character)
        {
            //判断当前字符是否为数字
            return character <= '9' && character >= '0';
        }

        private bool isLetter(char character)
        {
            //判断当前字符是否为字母
            return (character >= 'a' && character <= 'z')
                || (character >= 'A' && character <= 'Z');
        }

        private bool isBlank(char character)
        {
            return (character == ' ' 
                || character == '\n' 
                || character == '\t' 
                || character == '\r');
        }


        //一趟词法分析
        private void wordParse()
        {
            //输出流
            stream = File.CreateText(outputPath);
            text = richTextBox1.Text;
            index = 0;
            char character ;
            int type = 999;
            while (type != 0)
            {
                try
                {
                    token = "";
                    character = text[index];
                    type = getToken();
                    
                }
                catch (Exception e)
                {
                    type = -1;
                    error = "程序未以#终止";
                    text = text + '#';
                }
                switch (type)
                {
                    case 0://"#"
                        insertIntoListview1(0, "#", "#", Color.Blue);//number.ToString()
                        break;
                    case 36://常数
                        insertIntoListview1(type, token, "常数", Color.Black);//number.ToString()
                        pastspace = false;
                        break;
                    case 43://浮点数
                        insertIntoListview1(type, token, "浮点数", Color.Black);//number.ToString()
                        pastspace = false;
                        break;
                    case -1://出错
                        insertIntoListview1(type, token, error, Color.Red);
                        break;
                    case -2://注释
                        //不对注释进行处理
                        break;
                    case 37://标志符
                        String temp = "";
                        if (tokeninfos.Count>0)
                            temp = tokeninfos[tokeninfos.Count-1].token;
                        if (pastspace && (temp == "++" || temp == "--"))
                        {//标识符之前出现了++--但是有空格
                            pastspace = false;
                            insertIntoListview1(-1, token, "符号非法使用", Color.Red);
                            break;
                        }
                        pastspace = false;
                        insertIntoListview1(type, token, table[type], Color.Black);
                        break;
                    default://运算符
                        if (token == "++" || token == "--")
                        {
                            if(pastspace&&tokeninfos[tokeninfos.Count-1].type==37)
                            {//当++--前出现了标识符但是有空格则表示错误
                                pastspace = false;
                                insertIntoListview1(-1, token, "符号非法使用", Color.Red);
                                break;
                            }
                        }
                        pastspace = false;
                        insertIntoListview1(type, token, table[type],Color.Black);
                        break;
                }
            }
            //遇到#，程序结束
            stream.Close();
        }


        //词法分析获取token
        private int getToken()
        {
            int type = 999;
            //清空token,error
            error = "";
            token = "";
            //忽略空格，换行符，缩进等
            while (isBlank(text[index]))
            {
                index++;
                pastspace = true;
            }
            //if(pastspace)
            //{
            //    //pastspace = false;
            //    token = " ";
            //    type = -2;
            //    return type;
            //}
            if (isNumber(text[index]))
            {
                //当前字符为数字时，开始循环读入字符
                //查找string转换为int和float的机制
                int state = 4;//状态4为暂未接收到'.'直接收到数字的状态
                while (true)
                {
                    token += text[index];
                    index++;
                    if (isNumber(text[index]))
                    {
                        if (state == 5)
                        {
                            //在读入'.'之后读入数字进入6状态
                            state = 6;
                        }
                        //state = 6 不进行状态转换
                    }
                    else if (isLetter(text[index]))
                    {
                        if (state == 5)
                        {
                            token += text[index];
                            index++;
                            error = "常数不可以'.'结尾";
                            type = -1;
                            return type;
                        }
                        else
                        {
                            //数字之后出现字母,默认为错误标志符命名
                            //此时暂未考虑1.0f等数字表达法
                            type = -1;
                            token += text[index];
                            index++;
                            error = "不可以数字作为标志符开头";
                            return type;
                        }
                    }
                    else if (text[index] == '.')
                    {
                        if (state == 4)
                        {
                            state = 5;
                        }
                        else//state = 5 或 6
                        {
                            token += text[index];
                            index++;
                            //连续出现..
                            type = -1;
                            error = ".重复出现";
                            return type;
                        }
                    }
                    else
                    {
                        //当读入一个.后不接数字，则出错
                        if (state == 5)
                        {
                            token += text[index];
                            index++;
                            error = "常数不可以'.'结尾";
                            type = -1;
                            return type;
                        }
                        if (state == 4 )
                        {
                            type = 20+signAddress;
                            return type;
                        }
                        if(state == 6)
                        {//浮点数
                            type = 27 + signAddress;
                            return type;
                        }

                    }
                }
            }
            else if (isLetter(text[index]))
            {
                //当当前字符为字母时
                //循环读入存入token，返回type，注意记录错误
                bool indicator = true;
                int state = 0;//状态位，接收到字母和数字进入0状态，接收到下划线进入1状态
                //标志符不可以1状态结束，即不可以下划线结束
                while (indicator)
                {
                    token += text[index];
                    index++;
                    if (isLetter(text[index]) || isNumber(text[index]))
                    {
                        //进入下一次循环，存入token，读取下一个字符
                        indicator = true;
                        state = 0;//接收字母或数字，进入0状态
                    }
                    else if (text[index] == '_')
                    {
                        indicator = true;
                        state = 1;//接收下划线，进入1状态
                    }
                    else
                    {
                        indicator = false;
                        type = 21+signAddress;//标志符
                        if (state == 1)
                        {
                            //标志符以下划线结束
                            type = -1;//-1标记错误
                            token += text[index];
                            index++;
                            error = "标志符以'_'结束";
                            return type;
                        }
                        //此处判断是否为保留字

                        for (int j = 1; j <= signAddress; j++)
                        {
                            if (token == table[j])
                            {
                                type = j;//保留字
                                break;
                            }
                        }
                        return type;
                    }
                }
            }
            else
            {
                //当当前字符是运算符时
                switch (text[index])
                {
                    case '(':
                        type = 1+signAddress;
                        token += text[index];
                        index++;
                        return type;
                    case ')':
                        type = 2 + signAddress;
                        token += text[index];
                        index++;
                        return type;
                    case '[':
                        type = 3+ signAddress;
                        token += text[index];
                        index++;
                        return type;
                    case ']':
                        type = 4+ signAddress;
                        token += text[index];
                        index++;
                        return type;
                    case '{':
                        type = 5 + signAddress;
                        token += text[index];
                        index++;
                        return type;
                    case '}':
                        type = 6 + signAddress;
                        token += text[index];
                        index++;
                        return type;
                    case '+':
                        type = 7 + signAddress;
                        token += text[index++];
                        if(text[index]=='+')
                        {
                            token += '+';
                            index++;
                            type = 25 + signAddress;
                            return type;
                        }

                        return type;
                    case '-':
                        type = 8 + signAddress;
                        token += text[index++];
                        if(text[index]=='-')
                        {
                            token += '-';
                            index++;
                            type = 26 + signAddress;
                            return type;
                        }
                        return type;
                    case '*':
                        type = 9 + signAddress;
                        token += text[index];
                        index++;
                        return type;
                    case '/':
                        //此处可以是注释!!!!!!!!!!!!!!
                        token += text[index];
                        index++;
                        if (text[index] == '/')
                        {
                            //该行之后换行符之前所有的都为注释
                            //清空token
                            token = "";
                            while (text[index] != '\n')
                            {//忽略直到第一个换行符出现之前的所有内容
                                index++;
                            }
                            type = -2;//注释特殊码
                            //index++;
                        }
                        else if (text[index] == '*')
                        {
                            //在下一个*/出现之前都为注释
                            int state = 2;//状态2表示等待下一个*出现的状态
                            token = "";//清空token
                            while (true)
                            {
                                index++;
                                if (text[index] == '*')
                                    state = 3;//状态三表示接收到*
                                else if (text[index] == '/')
                                {
                                    if (state == 3)
                                    {//只有在状态三下接收到/才表示跳出注释
                                        index++;
                                        type = -2;//注释特殊码
                                        break;
                                    }
                                }
                                else
                                    state = 2;
                            }
                        }
                        else
                        {
                            //不是注释符号，为普通除法运算符
                            //token += text[index];
                            //index++;
                            type = 10 + signAddress; 
                            return type;
                        }
                        return type;
                    case '<':
                        token += text[index];
                        index++;
                        if (text[index] == '=')
                        {//当前符号为<=
                            token += text[index];
                            index++;
                            type = 14 + signAddress; 
                            return type;
                        }
                        else if(text[index]=='>')
                        {//<>
                            token += text[index];
                            index++;
                            type = 22 + signAddress;
                            return type;
                        }
                        //不是<=或是<>
                        type = 11 + signAddress; 
                        return type;
                    case '>':
                        //下一个可能是等于号!!!!!!!!!!!!!!
                        token += text[index];
                        index++;
                        if (text[index] == '=')
                        {//当前符号为>=
                            token += text[index];
                            index++;
                            type = 15 + signAddress;
                            return type;
                        }
                        //不是>=
                        type = 12 + signAddress;
                        return type;
                    case '=':
                        //下一个可能是等于号!!!!!!!!!!!!!!
                        token += text[index];
                        index++;
                        if (text[index] == '=')
                        {//当前符号为==
                            token += text[index];
                            index++;
                            type = 16 + signAddress;
                            return type;
                        }
                        //不是==
                        type = 13 + signAddress;
                        return type;
                    case '!':
                        //下一个可能是等于号!!!!!!!!!!!!!!
                        token += text[index];
                        index++;
                        if (text[index] == '=')
                        {//当前符号为!=
                            token += text[index];
                            index++;
                            type = 17 + signAddress;
                            return type;
                        }
                        //不是!=
                        //暂时不支持！运算符，此时看为非法运算符
                        token += text[index];
                        index++;
                        error = "除!=以外，!为非法运算符";
                        type = -1;
                        return type;
                    case ',':
                        type = 18 + signAddress;
                        token += text[index];
                        index++;
                        return type;
                    case ';':
                        type = 19 + signAddress;
                        token += text[index];
                        index++;
                        return type;
                    case '#':
                        type = 0;
                        return type;
                    case '|':
                        token += text[index];
                        index++;
                        if (text[index] == '|')
                        {
                            token += text[index];
                            index++;
                            type = 23 + signAddress;
                            return type;
                        }
                        else
                        {
                            type = -1;//出错
                            error = "单独的|为非法字符";
                            return type;
                        }
                    case '&':
                        token += text[index];
                        index++;
                        if(text[index]=='&')
                        {
                            token += text[index];
                            index++;
                            type = 24 + signAddress;
                            return type;
                        }
                        else
                        {
                            type = -1;//出错
                            error = "单独的&为非法字符";
                            return type;
                        }
                    
                    default:
                        token += text[index];
                        index++;
                        type = -1;
                        error = "非法符号";
                        break;

                }
            }
            return type;
        }
        

        private void insertIntoListview1(int inserttype, String inserttoken, String insertinfo,Color color)
        {
            ListViewItem temp = new ListViewItem(inserttype.ToString());
            temp.SubItems.Add(inserttoken);
            temp.SubItems.Add(insertinfo);
            listView1.Items.Add(temp);
            temp.ForeColor = color;
            stream.WriteLine("<" + inserttype + " , " + inserttoken + ">");
            tokeninfos.Add(new TokenInfo(inserttoken, inserttype, insertinfo,index-token.Length));
            //出错划红线
            if (inserttype == -1)
            {
                //出错则不进行语法分析
                wordparseok = false;
                highlightError(index - token.Length, token.Length, Color.Red);
            }
        }

        public void insertIntoListview2(String operate, String first, String second, String result)
        {
            ListViewItem temp = new ListViewItem(operate.ToString());
            temp.SubItems.Add(first);
            temp.SubItems.Add(second);
            temp.SubItems.Add(result);
            listView2.Items.Add(temp);
            
        }

        public void highlightError(int start, int length,Color color)
        {
            richTextBox1.SelectionStart = start;
            richTextBox1.SelectionLength = length;
            richTextBox1.SelectionColor = color;
        }
        public void highlightTree(Color color,MyNode node)
        {
            node.treenode.BackColor = color;
        }
        public void highlightTree(int start, int length, Color color, MyNode node)
        {
            richTextBox1.SelectionStart = start;
            richTextBox1.SelectionLength = length;
            richTextBox1.SelectionBackColor = color;
            node.treenode.BackColor = color;
        }
        public void highlightError(int start, int length, Color color,bool background)
        {
            richTextBox1.SelectionStart = start;
            richTextBox1.SelectionLength = length;
            if(background)
                richTextBox1.SelectionBackColor = color;
            else
                richTextBox1.SelectionColor = color;
        }



        private void button2_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog openDlg = new OpenFileDialog();
            // 指定打开文本文件（后缀名为txt）
            openDlg.Filter = "文本文件|*.txt";
            if (openDlg.ShowDialog() == DialogResult.OK)
            {
                // 读出文本文件的所以行
                string[] lines = File.ReadAllLines(openDlg.FileName);
                // 先清空textBox1
                richTextBox1.Clear();
                // 在textBox1中显示
                foreach (string line in lines)
                {
                    richTextBox1.AppendText(line + Environment.NewLine);
                }
                // 显示文件路径名
                label1.Text = openDlg.FileName;
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {

            richTextBox3.Text = "";
            wordparseok = true;
            tokeninfos.Clear();
            treeview.Nodes.Clear();
            listView1.Items.Clear();
            listView2.Items.Clear();
            highlightError(0, richTextBox1.Text.Length, Color.Black);
            highlightError(0, richTextBox1.Text.Length, Color.Transparent, true);
            //text = richTextBox1.Text;
            //richTextBox1.Clear();
            //richTextBox1.Text = text;
            wordParse();
            if (wordparseok)
            {//词法分析没有出错才进行语法分析
                GrammarParse grammarParse = new GrammarParse(this);
                if (grammarParse.Programe())
                {
                    showtree.Show();
                    SemanticParse semanticParse = new SemanticParse(grammarParse.mainNode, this);
                    semanticParse.startSemantic();
                }
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }
        public Form1()
        {

            InitializeComponent();
            this.listView1.Items.Clear();
            this.listView1.Columns.Clear();
            listView1.View = View.Details;
            listView1.FullRowSelect = true;
            listView1.Columns.Add("类型", this.listView2.Width / 3, HorizontalAlignment.Center);
            listView1.Columns.Add("tokens", this.listView2.Width / 3, HorizontalAlignment.Center);
            listView1.Columns.Add("详细信息", this.listView2.Width / 3, HorizontalAlignment.Center);

            this.listView2.Items.Clear();
            this.listView2.Columns.Clear();
            listView2.View = View.Details;
            listView2.FullRowSelect = true;
            listView2.Columns.Add("操作", 2*this.listView2.Width / 8, HorizontalAlignment.Center);
            listView2.Columns.Add("操作数1", this.listView2.Width / 8, HorizontalAlignment.Center);
            listView2.Columns.Add("操作符", this.listView2.Width / 8, HorizontalAlignment.Center);
            listView2.Columns.Add("操作数2", this.listView2.Width / 8, HorizontalAlignment.Center);
            listView2.Columns.Add("=", this.listView2.Width / 8, HorizontalAlignment.Center);
            listView2.Columns.Add("结果", this.listView2.Width / 8, HorizontalAlignment.Center);

            tokeninfos = new List<TokenInfo>();
            showtree = new ShowTree();
            treeview = showtree.treeView1;
            //showtree.Show();
        }

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.clicked = true;
        }
    }
}
