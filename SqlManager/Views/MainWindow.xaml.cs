using GenerateToolbox.Models;
using Newtonsoft.Json;
using SqlManager.Models;
using SqlManager.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xu.Common;

namespace SqlManager
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        static TabItemClose MakeDataItem = new TabItemClose();
        List<string> TabItemName = new List<string>();
        public static TabItemClose CurrentTabItem { get; set; }

        RightPage page = new RightPage();
        //NewPage.NewPage newPage = new NewPage.NewPage();
        //SqlManager.MakeData.MakeData makeData = new SqlManager.MakeData.MakeData();
        MainView vm;
        public MainWindow()
        {
            InitializeComponent();
            //WindowState = WindowState.Maximized;
            MakeDataItem = MakeData_item;

            foreach(TabItemClose item in tabcontol.Items)
            {
                item.Visibility = Visibility.Collapsed;
            }

            #region 激活校验
            /*
             * 不启用激活校验
             */
            //if (Common.Key() != Common.SetConfig("Password"))
            //{
            //    JiHuo ji = new JiHuo();
            //    ji.ShowDialog();
            //    if (Common.Key() != Common.SetConfig("Password"))
            //        this.Close();
            //}

            //if (Common.SetConfig("Date") == "0")
            //{
            //    this.Close();
            //}
            //else
            //{
            //    Common.SetConfig("Date", (Convert.ToInt32(Common.SetConfig("Date")) - 1).ToString());
            //}
            #endregion

            page.ParentWindow = this;
            //newPage.ParentWindow = this;

            LoadMode();
            right.Content = new Frame() { Content = page };
            right.Visibility = Visibility.Visible;
            //MakeData.Content = new Frame() { Content = makeData };
            //打开更新日志界面
            if (Common.SetConfig("Update") == "0")
            {
                UpdateDesc u = new UpdateDesc();
                u.ShowDialog();
                if (u.IsChecked())
                    Common.SetConfig("Update", "1");
            }
            vm = new MainView(this);
            this.DataContext = vm;
            InitUncloseFile();
            //RunNotifyBox();
        }

        protected override void OnClosed(EventArgs e)
        {
            var files = new List<UncloseFileModel>();
            foreach (TabItemClose item in tabcontol.Items)
            {
                var npage = FindNewPage.GetPage(item);
                if (npage is null) continue;
                var m = new UncloseFileModel
                {
                    FileName = (string)item.Header,
                    FilePath = npage.FilePath,
                    FileText = npage.tb.Text
                };
                files.Add(m);
            }
            Strings.Write(JsonConvert.SerializeObject(files), "Models/unclosefileconfig.json");
            OracleHelper.CloseAllTable(vm.TreeSource);
            Strings.Write(JsonConvert.SerializeObject(vm.TreeSource), "Models/dbconfig.json");

            //Strings.Write(JsonConvert.SerializeObject(keywords), "Models/keywords.json");
        }

        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                WindowState = WindowState.Normal;
                DragMove();
            }
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
            System.Environment.Exit(0);
        }



        private void MinWindow(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaxWindow(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
                this.WindowState = WindowState.Maximized;
            else
                this.WindowState = WindowState.Normal;

        }


        public static SolidColorBrush GetColor(string str)
        {
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(str));
        }



        private void bodyDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //Transfer();
        }

        private void Transfer()
        {
            bd1.Visibility = bd1.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
            tabcontol.Visibility = tabcontol.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
            bd2.Height = (bd2.Height - 30) <= 10 ? 70 : 35;
            setting.Visibility = setting.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
            if (setting.Visibility == Visibility.Collapsed)
            {
                zz.Visibility = Visibility.Visible;
                //zz.Content = new Frame() { Content = newPage };
            }
            else
            {
                zz.Visibility = Visibility.Collapsed;
                //Preview.Content = new Frame() { Content = newPage };
            }
        }

        private void LoadMode()
        {
            if (Common.SetConfig("Mode") == "1")
            {
                bd1.Visibility = Visibility.Collapsed;
                tabcontol.Visibility = Visibility.Collapsed;
                bd2.Height = 70;
                setting.Visibility = Visibility.Collapsed;
                //zz.Content = new Frame() { Content = newPage };
            }
        }



        private void NewFileClick(object sender, RoutedEventArgs e)
        {
            OpenNewSqlFile();
        }

        private void OpenNewSqlFile()
        {
            var item = new TabItemClose
            {
                Header = $"{GetName(1)}.sql*",
                Height = 30,
                MinWidth = 100
            };
            var content = new Grid();
            content.Margin = new Thickness(-3);
            var npage = new NewPage.NewPage(item, this);
            var f = new Frame { Content = npage };
            content.Children.Add(f);
            item.Content = content;
            item.GotFocus += Item_GotFocus;
            tabcontol.Items.Add(item);
            item.Focus();
            CurrentTabItem = item;
        }

        private void InitUncloseFile()
        {
            var files = Init.InitUncloseFile();
            foreach(var file in files)
            {
                TabItemName.Add(file.FileName);
                var item = new TabItemClose
                {
                    Header = $"{file.FileName}",
                    Height = 30,
                    MinWidth = 100
                };
                var content = new Grid();
                content.Margin = new Thickness(-3);
                var npage = new NewPage.NewPage(item, this);
                npage.FilePath = file.FilePath;
                npage.vm.Text = file.FileText;
                //npage.tb.Text = file.FileText;
                var f = new Frame { Content = npage };
                content.Children.Add(f);
                item.Content = content;
                item.GotFocus += Item_GotFocus;
                tabcontol.Items.Add(item);
                item.Focus();
                CurrentTabItem = item;
            }
        }

        public void Item_GotFocus(object sender, RoutedEventArgs e)
        {
            CurrentTabItem = sender as TabItemClose;
        }

        public string GetName(int i)
        {
            if (TabItemName.Count(x => x.Replace("*", "").Replace(".sql", "") == $"SQL{i}") == 0)
            {
                TabItemName.Add($"SQL{i}");
                return $"SQL{i}";
            }
            else return GetName(i + 1);
        }

       

        private void DataClick(object sender, RoutedEventArgs e)
        {
            if (MakeDataItem.Parent == null) tabcontol.Items.Add(MakeDataItem);
            MakeDataItem.Visibility = Visibility;
            MakeDataItem.Focus();
        }


        private void settingClick(object sender, RoutedEventArgs e)
        {
            page.Visibility = page.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void tree_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var treeViewItem = VisualUpwardSearch<TreeViewItem>(e.OriginalSource as DependencyObject) as TreeViewItem;
            if (treeViewItem != null)
            {
                treeViewItem.Focus();
                e.Handled = true;
            }
        }
        static DependencyObject VisualUpwardSearch<T>(DependencyObject source)
        {
            while (source != null && source.GetType() != typeof(T))
                source = VisualTreeHelper.GetParent(source);
            return source;
        }

        private void HierarchicalDataTemplate_Collapsed(object sender, RoutedEventArgs e)
        {

        }

        private void tree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            vm.SelectedNode = e.NewValue as TreeList;
        }

        private void tree_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void tabcontol_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tab = sender as TabControl;
            var page = GenerateToolbox.Models.FindNewPage.GetPage(tab.SelectedItem);
            if (page != null)
            {
                vm.IsChange = false;
                vm.cb_selected_connection_itemsource = vm.cb_connection_itemsource.FirstOrDefault(x => x.label == page?.CurrentDatabase?.Key);
                vm.IsChange = true;
            }
        }

       
        private void Output_Click(object sender, RoutedEventArgs e)
        {
            outputtab.Height = 25;
        }

        private void outputtab_GotFocus(object sender, RoutedEventArgs e)
        {
            outputtab.Height = 200;
        }

        private void console_TextChanged(object sender, TextChangedEventArgs e)
        {
            outputtab.Height = 200;
        }

    }
}
