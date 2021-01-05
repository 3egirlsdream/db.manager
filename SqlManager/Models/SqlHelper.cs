using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlManager.Models
{
    public class SqlHelper
    {
        public static string FormatSql(string sql)
        {
            var text = sql;
            text = text.Replace("\r\n", "").Replace("\r", "").Replace("\n", "");
            var afterFormat = "";
            foreach (var ds in text)
            {
                afterFormat += ds;
                if (ds == ',' || ds == '(' || ds == ')')
                    afterFormat += "\r\n";
            }
            return afterFormat;
        }


        private List<string> Keywords = new List<string>
        {
            "LEFT JOIN",
            "INNER JOIN",
            "WHERE",
            "RIGHT JOIN",
            "JOIN",
            "AND",
            "FROM"
        };
    }
}
