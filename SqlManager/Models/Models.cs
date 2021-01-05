using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlManager.Models
{
    public class excel
    {
        public string name { get; set; }
        public string desc { get; set; }
        public string type { get; set; }
        public string isNull { get; set; }
        public string defaultContext { get; set; }
        public int length { get; set; }
    }

    public class Excel
    {
        public bool IsApi { get; set; }
        public string GRID_CODE { get; set; }
        public string GRID_NAME { get; set; }
        public string CONTROL_CODE { get; set; }
        public string SEARCH_CODE { get; set; }
        public string SEARCH_NAME { get; set; }
        public string BUTTON { get; set; }
    }

    



    public class Key_Value
    {
        //public string Key { get; set; }
        public string Value { get; set; }
        //兼容旧的
        public string label { get; set; }
        public int value { get; set; }
        public override string ToString()
        {
            return label.ToString();
        }
    }

   
  


}
