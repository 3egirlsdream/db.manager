using MahApps.Metro.Controls;
using SqlManager.Models;
using SqlManager.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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

namespace SqlManager
{
    /// <summary>
    /// AddConnection.xaml 的交互逻辑
    /// </summary>
    public partial class AddConnection : MetroWindow
    {
        MainView vm;
        TreeList CurNode = new TreeList();
        public AddConnection(ValidationBase v)
        {
            vm = v as MainView;
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Loaded += AddConnection_Loaded;
            this.ResizeMode = ResizeMode.NoResize; 
        }

        private void AddConnection_Loaded(object sender, RoutedEventArgs e)
        {
            ///文件目录层级
            levels.Items.Add(new KeyValue<string, string>("1", "File"));
            levels.Items.Add(new KeyValue<string, string>("2", "Database"));
            levels.Items.Add(new KeyValue<string, string>("3", "Table"));

            ///dbType
            ///暂时只有Oracle
            dbtype.Items.Add(new KeyValue<string, string>("Oracle", "Oracle"));
            dbtype.Items.Add(new KeyValue<string, string>("MsSql", "MsSql"));
            dbtype.SelectedIndex = vm.SelectedNode.Type == SqlSugar.DbType.Oracle ? 0 : 1;
            if(vm.SelectedNode == null)///说明是根节点
            {
                levels.SelectedIndex = 0;
                connstr.IsEnabled = false;
                levels.IsEnabled = false;
            }
            else
            {
                if (vm.IsEdit == false)
                {
                    levels.SelectedIndex = 1;
                    levels.IsEnabled = false;
                }
                else//编辑
                {
                    Title = "编辑连接";
                    levels.SelectedIndex = vm.SelectedNode.LEVEL == TreeLevel.File ? 0 : (vm.SelectedNode.LEVEL == TreeLevel.Database ? 1 : 2);
                    if(vm.SelectedNode.LEVEL == TreeLevel.File || vm.SelectedNode.LEVEL == TreeLevel.Table)
                    {
                        connstr.IsEnabled = false;
                        levels.IsEnabled = false;
                    }
                    
                    connstr.Text = vm.SelectedNode.CONN_STRING;
                    name.Text = vm.SelectedNode.NODE_NAME;
                }
            }
        }

        private void commit_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(name.Text))
            {
                Dialog.ShowMsg("内容不能为空");
                return;
            }

            TreeLevel lv = TreeLevel.File;
            switch (levels.SelectedIndex)
            {
                case 0:lv = TreeLevel.File;break;
                case 1:lv = TreeLevel.Database;break;
                case 2:lv = TreeLevel.Table; break;
            }
                
            KeyValue<string, string>  key = dbtype.SelectedItem as KeyValue<string, string>;
            CurNode = new TreeList
            {
                CONN_STRING = connstr.Text,
                LEVEL = (!vm.IsEdit && vm.SelectedNode == null) ? TreeLevel.File : lv,
                NODE_NAME = name.Text,
                SqlType = string.IsNullOrEmpty(connstr.Text) ? null : key.Key
            };

            if (vm.SelectedNode != null)
            {
                if (!vm.IsEdit)
                    Insert(vm.TreeSource, vm.SelectedNode, CurNode);
                else
                    Update(vm.TreeSource, vm.SelectedNode, CurNode);
            }
            else
            {
                vm.TreeSource.Add(CurNode);
            }
            Close();
        }

        private void Insert(ObservableCollection<TreeList> trees, TreeList aimPos, TreeList curTree)
        {
            if (trees == null || trees.Count == 0 || curTree == null)
                return;
            for(int i = 0; i < trees.Count; i++)
            {
                if(trees[i] == aimPos)
                {
                    trees[i].Children.Add(curTree);
                    return;
                }
                Insert(trees[i].Children, aimPos, curTree);
            }
        }

        private void Update(ObservableCollection<TreeList> trees, TreeList aimPos, TreeList curTree)
        {
            if (trees == null || trees.Count == 0 || curTree == null)
                return;
            for (int i = 0; i < trees.Count; i++)
            {
                if (trees[i] == aimPos)
                {
                    curTree.Children = trees[i].Children;
                    trees[i] = curTree;
                    return;
                }
                Update(trees[i].Children, aimPos, curTree);
            }
        }
    }
}
