using SqlManager;
using SqlManager.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Xu.Common;

namespace GenerateToolbox.Models
{
    public class TableConstruct
    {
        public TableConstruct()
        {

        }

        public static TabItemClose Init(TreeList SelectedNode, MainWindow plugin)
        {
            using (var db = SugarContext.GetContext(SelectedNode.ParentNode.ParentNode.CONN_STRING, SelectedNode.ParentNode.ParentNode.Type.Value))
            {
                var NodeName = SelectedNode.NODE_NAME;
                var ls = NodeName.Split('.');
                if (ls.Length == 2) NodeName = ls[1];
                var sql1 = "select t.column_name, T.data_length, T.data_type, T.nullable, jt.comments FROM user_tab_columns t INNER JOIN user_col_comments jt ON t.table_name = jt.table_name AND t.column_name = jt.column_name where t.Table_Name='" + NodeName + "' order by t.COLUMN_ID";
                
                //dg.ItemsSource = result;
                var sql2 = $"SELECT wm_concat(t.column_name) as columns,  t.index_name,  t.table_name, t.descend, i.index_type, i.uniqueness FROM user_ind_columns t, user_indexes i WHERE t.index_name = i.index_name AND t.table_name = upper('{NodeName}') GROUP BY  t.index_name,  t.table_name,  t.descend, i.index_type, i.uniqueness";
                
                //tindex.ItemsSource = index;
                if(SelectedNode.ParentNode.ParentNode.Type.Value == SqlSugar.DbType.SqlServer)
                {
                    sql1 = $@"SELECT
A.name AS table_name,
B.name AS column_name,
b.max_length as data_length,
d.data_type,
d.is_nullable as nullable,
C.value AS comments
FROM sys.tables A
INNER JOIN sys.columns B ON B.object_id = A.object_id
LEFT JOIN sys.extended_properties C ON C.major_id = B.object_id AND C.minor_id = B.column_id
inner join information_schema.columns d on d.table_name = a.name and d.column_name = b.name
WHERE A.name = upper('{NodeName}')";
                    //dg.ItemsSource = result;

                    sql2 = $@"SELECT a.name as index_name FROM sys.sysindexes a
inner join sys.tables b on a.id = b.object_id
where a.name = upper('{NodeName}')";

                }
                var result = db.Ado.GetDataTable(sql1);
                var index = db.Ado.GetDataTable(sql2);
                var p_tab = new TabControl();

                var data1 = new DataGrid
                {
                    ItemsSource = result.DefaultView,
                    GridLinesVisibility = DataGridGridLinesVisibility.All,
                    CanUserAddRows = false
                };
                var item1 = new TabItem
                {
                    Content = data1,
                    Height = 30,
                    MinWidth = 100,
                    Header = "列",
                    Style = (Style)plugin.FindResource("TabItemNormal")
                };

                var data_index = new DataGrid
                {
                    ItemsSource = index.DefaultView,
                    GridLinesVisibility = DataGridGridLinesVisibility.All,
                    CanUserAddRows = false
                };

                var item_index = new TabItem
                {
                    Content = data_index,
                    Height = 30,
                    MinWidth = 100,
                    Header = "索引",
                    Style = (Style)plugin.FindResource("TabItemNormal")
                };


                p_tab.Items.Add(item1);
                p_tab.Items.Add(item_index);

                var p_item = new TabItemClose();
                p_item.Content = p_tab;
                return p_item;
            }
                
        }

        public static List<TabItem> InitTabItem(string NodeName, dbcfg cfg, MainWindow plugin)
        {
            using (var db = SugarContext.GetContext(cfg.connection_string, cfg.dbtype))
            {
                var ls = NodeName.Split('.');
                if (ls.Length == 2) NodeName = ls[1];
                var sql1 = "select t.column_name, T.data_length, T.data_type, T.nullable, jt.comments FROM user_tab_columns t INNER JOIN user_col_comments jt ON t.table_name = jt.table_name AND t.column_name = jt.column_name where t.Table_Name='" + NodeName + "' order by t.COLUMN_ID";

                //dg.ItemsSource = result;
                var sql2 = $"SELECT wm_concat(t.column_name) as columns,  t.index_name,  t.table_name, t.descend, i.index_type, i.uniqueness FROM user_ind_columns t, user_indexes i WHERE t.index_name = i.index_name AND t.table_name = upper('{NodeName}') GROUP BY  t.index_name,  t.table_name,  t.descend, i.index_type, i.uniqueness";


                if (cfg.dbtype == SqlSugar.DbType.SqlServer)
                {
                    sql1 = $@"SELECT
A.name AS table_name,
B.name AS column_name,
b.max_length as data_length,
d.data_type,
d.is_nullable as nullable,
C.value AS comments
FROM sys.tables A
INNER JOIN sys.columns B ON B.object_id = A.object_id
LEFT JOIN sys.extended_properties C ON C.major_id = B.object_id AND C.minor_id = B.column_id
inner join information_schema.columns d on d.table_name = a.name and d.column_name = b.name
WHERE A.name = upper('{NodeName}')";
                    //dg.ItemsSource = result;

                    sql2 = $@"SELECT a.name as index_name FROM sys.sysindexes a
inner join sys.tables b on a.id = b.object_id
where a.name = upper('{NodeName}')";

                }
                var result = db.Ado.GetDataTable(sql1);
                var index = db.Ado.GetDataTable(sql2);
                var p_item = new List<TabItem>();

                var data1 = new DataGrid
                {
                    ItemsSource = result.DefaultView,
                    GridLinesVisibility = DataGridGridLinesVisibility.All,
                    CanUserAddRows = false
                };
                var item1 = new TabItem
                {
                    Content = data1,
                    Height = 30,
                    MinWidth = 100,
                    Header = "列",
                    Style = (Style)plugin.FindResource("TabItemNormal")
                };

                var data_index = new DataGrid
                {
                    ItemsSource = index.DefaultView,
                    GridLinesVisibility = DataGridGridLinesVisibility.All,
                    CanUserAddRows = false
                };

                var item_index = new TabItem
                {
                    Content = data_index,
                    Height = 30,
                    MinWidth = 100,
                    Header = "索引",
                    Style = (Style)plugin.FindResource("TabItemNormal")
                };


                p_item.Add(item1);
                p_item.Add(item_index);
                return p_item;
            }

        }

    }



    class TABLE_CONSTRUCT
    {
        public string COMMENTS { get; set; }
        public string NULLABLE { get; set; }
        public string DATA_LENGTH { get; set; }
        public string DATA_TYPE { get; set; }
        public string COLUMN_NAME { get; set; }
    }

    class INDEX_TABLE
    {
        public string COLUMNS { get; set; }
        public string UNIQUENESS { get; set; }
        public string INDEX_NAME { get; set; }
        public string TABLE_NAME { get; set; }
        public string COLUMN_NAME { get; set; }
        public string COLUMN_POSITION { get; set; }
        public string COLUMN_LENGTH { get; set; }
        public string CHAR_LENGTH { get; set; }
        public string DESCEND { get; set; }
        public string INDEX_TYPE { get; set; }
    }

    public class Tables
    {
        public string OWNER { get; set; }
        public string TABLE_NAME { get; set; }
        public List<string> tables { get; set; }
    }

}
