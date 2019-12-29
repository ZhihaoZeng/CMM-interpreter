using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/*
 * 软工五班 曾志昊 2017302580214
 */

namespace 解释器构造实践
{
    class SignTable
    { //为每个局部块定义的符号表
        public int signtop = -1;
        public List<Sign> table;
        public SignTable()
        {
            table = new List<Sign>();
            addSign(new Sign("false", "bool", "false"));
            addSign(new Sign("true", "bool", "true"));
        }
        public bool addSign(Sign sign)
        {
            if (table.Exists(s =>(s.name == sign.name)/*&&(s.type==sign.type)*/))
            {//当有同名同类的标志符时，返回添加错误
                return false;
            }
            else
            {
                table.Add(sign);
                signtop++;
                return true;
            }
        }
        public Sign findSign(String name)
        {
            if (table.Exists( s =>(s.name == name)/*&& (s.type == type)*/))
            {//当有同名同类的标志符时，返回正确
                Sign sign = table.Find(s => (s.name == name) /*&& (s.type == type)*/);
                return sign;
            }
            return null;
        }

        public bool changeValue(String value)
        {
            return true;
        }

        public int top = 0;
        //层级0为函数名
        //层级1为函数内变量
        //层级...为嵌套语句内变量
        public Sign find(String name)
        {
            return table.Find(s => s.name == name);
        }
        //public Symbol find()
        //{ }
    }
}
