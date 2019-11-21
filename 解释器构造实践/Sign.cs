using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 解释器构造实践
{
    class Sign
    {
        public String name;
        public String type;
        public String value;
        public bool hasvalue = false;
        public ArrayList array;
        public int arraycount;
        public int arraytop;
        //布尔值true为1，false为0
        //区分浮点数 整数 布尔值

        //public static getSign(MyNode node)
        //{
        //    Sign sign = new Sign();
            
        //}

        public Sign(String name,String type,String value)
        {
            this.name = name;
            this.type = type;
            this.value = value;
            hasvalue = true;
        }
        public Sign(String name,String type)
        {
            this.name = name;
            this.type = type;
        }
        public Sign()
        { }

        private bool pm(int p)
        {
            if (type == "bool")
            {
                return false;
            }
            else if(type == "int")
            {
                this.value = (this.getIntValue() + p).ToString();
                return true;
            }
            else if(type == "float")
            {
                this.value = (this.getFloatValue() + p).ToString();
                return true;
            }
            return false;

        }

        public bool pp()
        {
            return pm(1);
        }
        public bool mm()
        {
            return pm(-1);
        }
        
        public int setValue(Sign rightsign)
        {
            if(this.type != rightsign.type)
            {//类型不同
                return 3;
            }
            if(!rightsign.hasvalue)
            {//右部未赋值
                return 2;
            }
            setValue(rightsign.value);
            return 1;
        }

        public int forcedSetArrayValue(Sign rightsign, int index)
        {
            try
            {
                if (!rightsign.hasvalue)
                {//右部未赋值
                    return 2;
                }
                if (this.type == "int")
                    this.array[index] = rightsign.getIntValue();
                else if (this.type == "float")
                    this.array[index] = rightsign.getFloatValue();
                else if (this.type == "bool")
                    this.array[index] = rightsign.getBoolValue();

            }
            catch (Exception e) { }
            return 1;
        }



        public int setValue(Sign rightsign, int index)
        {
            try
            {
                if (this.type != rightsign.type)
                {//类型不同
                    return 3;
                }
                if (!rightsign.hasvalue)
                {//右部未赋值
                    return 2;
                }
                if (this.type == "int")
                    this.array[index] = rightsign.getIntValue();
                else if (this.type == "float")
                    this.array[index] = rightsign.getFloatValue();
                else if (this.type == "bool")
                    this.array[index] = rightsign.getBoolValue();

            } catch (Exception e) { }
                return 1;

        }
        public void setValue(String value)
        {
            this.value = value;
            this.hasvalue = true;
        }
        public int getIntValue(ref bool right)
        {
            if(type != "int")
            {
                right = false;
                return 0;
            }
            right = true;
            return Convert.ToInt32(value);
        }
        public int getIntValue()
        {
            return Convert.ToInt32(value);
        }
        public float getFloatValue()
        {
            return Convert.ToSingle(value);
        }
        public bool getBoolValue()
        {
            return Convert.ToBoolean(value);
        }
        public bool getBoolValue(ref bool right)
        {
            if (type != "bool")
            {
                right = false;
                return false;
            }
            right = true;
            return Convert.ToBoolean(value);
        }
        public float getFloatValue(ref bool right)
        {
            if(type!="float")
            {
                right = false;
                return 0.0f;
            }
            right = true;
            return Convert.ToSingle(value);

        }
        public bool setIntValue(int newvalue)
        {
            if (this.type != "int")
                return false;
            this.value = Convert.ToString(newvalue);
            this.hasvalue = true;
            return true;
        }
        public bool setBoolValue(bool newvalue)
        {
            if (this.type != "bool")
                return false;
            this.value = Convert.ToString(newvalue);
            this.hasvalue = true;

            return true;
        }
        public bool setFloatValue(float newvalue)
        {
            if (this.type != "float")
                return false;
            this.value = Convert.ToString(newvalue);
            this.hasvalue = true;
            return true;
        }
        public bool addIntoArray(object value)
        {
            array.Add(value);
            arraytop++;
            arraycount++;
            return true;
        }
    }
}
