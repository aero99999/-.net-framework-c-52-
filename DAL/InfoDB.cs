using Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class InfoDB
    {
        
        public int AddUser(Baseinfo userInfo)
        {
            //对数据库进添加一个用户操作
            string sql = "insert into UserInfo(userName, Password)values(@userName,@Password)";
            SqlParameter[] paras = new SqlParameter[]
            {
                //new SqlParameter (@userName,userInfo.UserName ),
                //new SqlParameter (@Password,userInfo.Password )
            };
            return SqlHelper.ExcuteNonQuery(sql, paras);
        }
    }
}
