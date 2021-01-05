using SqlManager.Models;
using Model.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xu.Common;

namespace SqlManager.Models
{
    public class Strings
    {
        public static string LoadJson(string path)
        {
            try
            {
                FileStream file = new FileStream(path, FileMode.Open);
                StreamReader sr = new StreamReader(file, Encoding.Default);
                string json = sr.ReadToEnd();
                //MessageBox.Show(json);
                file.Close();
                return json;
            }
            catch (IOException)
            {
                return null;
                //Console.WriteLine(e.ToString());
            }
        }

        public static void Write(string str, string path)
        {
            FileStream fs = new FileStream(path, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs, Encoding.Unicode);
            //开始写入
            sw.Write(str);
            //清空缓冲区
            sw.Flush();
            //关闭流
            sw.Close();
            fs.Close();
        }


        public static string formatString(string str, List<string> param)
        {
            var s = string.Format(str, string.Join(", ", param));

            return s.Replace("{{", "{").Replace("}}", "}");
        }
    }
}
