using SqlManager.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlManager.Models
{
    public class OracleHelper
    {
        public static void CloseAllTable(ObservableCollection<TreeList> trees)
        {
            foreach (var ds in trees)
            {
                ds.IS_OPEN = false;
                if (ds.Children == null || ds.Children.Count == 0)
                    continue;
                if (ds.LEVEL == TreeLevel.Database)
                {
                    ds.Children = new ObservableCollection<TreeList>();
                }
                else
                {
                    CloseAllTable(ds.Children);
                }
            }
        }

        public static void GetOpenedDb(ObservableCollection<TreeList> trees, ref TreeList result)
        {
            foreach (var ds in trees)
            {
                if (ds.Children == null || ds.Children.Count == 0)
                    return;
                if (ds.LEVEL == TreeLevel.Database)
                {
                    if (ds.IS_OPEN) result = ds;
                }
                else
                {
                    GetOpenedDb(ds.Children, ref result);
                }
            }
        }


    }

    public class dbcfg
    {
        public string connection_string { get; set; }
        public SqlSugar.DbType dbtype { get; set; }
        public string db_name { get; set; }

        public override string ToString()
        {
            return db_name;
        }
    }
}
