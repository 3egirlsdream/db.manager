using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Xu.Common;

namespace SqlManager.Views
{
    /// <summary>
    /// SqlParams.xaml 的交互逻辑
    /// </summary>
    public partial class SqlParams : MetroWindow
    {
        public string sqlParams;
        public object Data;
        SqlParamsVM vm;
        public NewPage.NewPage plugin;
        public SqlParams(string sql, NewPage.NewPage Iplugin)
        {
            plugin = Iplugin;
            sqlParams = sql;
            InitializeComponent();
            vm = new SqlParamsVM(this);
            DataContext = vm;
        }


        
    }

    class EditPamas
    {
        public string Name { get; set; }
        public string Name1
        {
            get
            {
                return Name?.Replace(":", "");
            }
            set { }
        }
        public string Value { get; set; }
    }

    class SqlParamsVM : ValidationBase
    {
        private SqlParams plugin;
        public SqlParamsVM(SqlParams Iplugin)
        {
            plugin = Iplugin;
            BindingParams(plugin.sqlParams);
        }

        private List<EditPamas> itemsSource;
        public List<EditPamas> ItemsSource
        {
            get => itemsSource;
            set
            {
                itemsSource = value;
                NotifyPropertyChanged(nameof(ItemsSource));
            }
        }

        private void BindingParams(string sql)
        {
            var parametors = new List<string>();
            var mx = Regex.Matches(sql, @"[:][A-Za\u005f]*");
            ItemsSource = new List<EditPamas>();
            foreach (Match m in mx)
            {
                parametors.Add(m.Value);
                ItemsSource.Add(new EditPamas { Name = m.Value, Value = "" });
            }
            ItemsSource.ForEach(c => c.Value = plugin.plugin.vm.BindedWords.ContainsKey(c.Name1) ? plugin.plugin.vm.BindedWords[c.Name1] : "");
        }

        public SimpleCommand CmdCommit => new SimpleCommand()
        {
            ExecuteDelegate = x =>
            {
                ItemsSource.ForEach(c => plugin.plugin.vm.BindedWords[c.Name1] = c.Value);
                plugin.Data = ItemsSource;
                plugin.Close();
            },
            CanExecuteDelegate = o => true
        };
    }
}
