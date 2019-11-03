using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 解释器构造实践
{
    public class TokenInfo
    {
        public TokenInfo() { }
        public TokenInfo(String token,int type)
        {
            this.token = token;
            this.type = type;
        }
        public TokenInfo(String token, int type,String information,int stringindex)
        {
            this.token = token;
            this.type = type;
            this.information = information;
            this.stringindex = stringindex;
        }
        public String token;
        public int type;
        public String information;
        public int stringindex;//在字符串中的index
    }
}
