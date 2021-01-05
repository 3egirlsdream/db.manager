using SqlManager;
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

        public static TabItemClose Init(string NodeName, MainWindow plugin)
        {
            using (var db = SugarContext.OracleContext)
            {
                var ls = NodeName.Split('.');
                if (ls.Length == 2) NodeName = ls[1];
                var result = db.Ado.GetDataTable("select t.column_name, T.data_length, T.data_type, T.nullable, jt.comments FROM user_tab_columns t INNER JOIN user_col_comments jt ON t.table_name = jt.table_name AND t.column_name = jt.column_name where t.Table_Name='" + NodeName + "' order by t.COLUMN_ID");
                //dg.ItemsSource = result;

                var index = db.Ado.GetDataTable($"SELECT wm_concat(t.column_name) as columns,  t.index_name,  t.table_name, t.descend, i.index_type, i.uniqueness FROM user_ind_columns t, user_indexes i WHERE t.index_name = i.index_name AND t.table_name = upper('{NodeName}') GROUP BY  t.index_name,  t.table_name,  t.descend, i.index_type, i.uniqueness");
                //tindex.ItemsSource = index;

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

        public static List<TabItem> InitTabItem(string NodeName, MainWindow plugin)
        {
            using (var db = SugarContext.OracleContext)
            {
                var ls = NodeName.Split('.');
                if (ls.Length == 2) NodeName = ls[1];
                var result = db.Ado.GetDataTable("select t.column_name, T.data_length, T.data_type, T.nullable, jt.comments FROM user_tab_columns t INNER JOIN user_col_comments jt ON t.table_name = jt.table_name AND t.column_name = jt.column_name where t.Table_Name=upper('" + NodeName + "') order by t.COLUMN_ID");
                //dg.ItemsSource = result;

                var index = db.Ado.GetDataTable($"SELECT wm_concat(t.column_name) as columns,  t.index_name,  t.table_name, t.descend, i.index_type, i.uniqueness FROM user_ind_columns t, user_indexes i WHERE t.index_name = i.index_name AND t.table_name = upper('{NodeName}') GROUP BY  t.index_name,  t.table_name,  t.descend, i.index_type, i.uniqueness");
                //tindex.ItemsSource = index;

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
