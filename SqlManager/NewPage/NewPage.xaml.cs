using SqlManager;
using SqlManager.Models;
using SqlManager.ViewModel;
using SqlManager;
using SqlManager.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xu.Common;

namespace SqlManager.NewPage
{
    /// <summary>
    /// NewPage.xaml 的交互逻辑
    /// </summary>
    public partial class NewPage : Page
    {
        public NewPageVM vm;
        TabItemClose parentTab;
        public MainWindow plugin;

        public string FilePath { get; set; }
        public KeyValue<string, string> CurrentDatabase { get; set; } = new KeyValue<string, string>("", "");
        public NewPage(TabItemClose parentTabItem, MainWindow IPlugin)
        {
            parentTab = parentTabItem;
            plugin = IPlugin;
            InitializeComponent();
            keywords = Init.KeywordsInit();
            vm = new NewPageVM(this);
            this.DataContext = vm;
            FilePath = "";
        }

        public NewPage()
        {
            InitializeComponent();
            keywords = Init.KeywordsInit();
            vm = new NewPageVM(this);
            this.DataContext = vm;

        }
        public MainWindow ParentWindow { get; set; }

        private void tb_MouseDown(object sender, KeyEventArgs e)
        {
            try
            {
                var header = (string)parentTab.Header;
                if (!header.Contains("*"))
                {
                    parentTab.Header = header + "*";
                }

                ///tab键和enter键自动补全
                if ((e.Key == Key.Tab || e.Key == Key.Enter) && pop.IsOpen)
                {

                    try
                    {
                        var item = listbox.SelectedItem as ListBoxItem;
                        vm.SelectedKeyword = (string)item.Content;
                        TextReplace();
                        tb.SelectionStart = tb.Text.Length;
                        pop.IsOpen = false;
                        e.Handled = true;
                    }
                    catch (Exception ex)
                    {
                        Warning.ShowMsg(ex.Message);
                    }
                }
                ///空格时不显示提示
                if (e.Key == Key.Space)
                {
                    pop.IsOpen = false;
                }

                if (e.KeyStates == Keyboard.GetKeyStates(Key.S) && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    e.Handled = true;
                    parentTab.Header = header.Replace("*", "");
                    if (string.IsNullOrEmpty(FilePath))
                    {
                        FilePath = Common.Export(header.Replace("*", ""), tb.Text, "(*.sql)|*.sql");
                        parentTab.Header = FilePath.Remove(0, FilePath.LastIndexOf('\\') + 1);
                    }
                    else
                    {
                        Common.SaveFile(FilePath, tb.Text);
                    }
                }
            }
            catch(Exception ex)
            {
                Warning.ShowMsg(ex.Message);
            }
        }

        private void tb_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            pop.IsOpen = false;
        }

        private void tb_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //获取行数
            //var rows = tb.LineCount;
            try
            {
                var curPos = tb.SelectionStart;
                int rowIndex = -1;
                while (curPos > 0)
                {
                    var countOfLine = tb.GetLineLength(++rowIndex);
                    //var text = tb.GetLineText(rowIndex);
                    curPos -= countOfLine;
                }
                ////每行字符数
                int start = tb.GetCharacterIndexFromLineIndex(rowIndex);

                //长度
                int length = tb.GetLineText(rowIndex).Length;
                tb.Focus();
                //选择指定行内容
                tb.Select(start, length);
                tb.ScrollToLine(rowIndex);
            }
            catch (Exception ex)
            {
                Warning.ShowMsg(ex.Message);
            }
        }
        /// <summary>
        /// 当前离光标最近的提示码
        /// </summary>
        public static string SourceText { get; set; }

        /// <summary>
        /// 关键词
        /// </summary>
        public List<string> keywords { get; set; }

        /// <summary>
        /// 显示自动提示
        /// </summary>
        /// <param name="text"></param>
        public void ShowAutoComplete(string text)
        {
            var selectedText = text;
            ///分词
            var selected = selectedText.Split(' ');
            selected = selected.Reverse().ToArray();
            listbox.Items.Clear();
            SourceText = /*GetRecentString(text);// */selected.FirstOrDefault();
            ///如果为空不提示
            if (SourceText.All(c => c == ' '))
                return;
            ///显示在智能提示框的内容
            var showtext = keywords.Where(c => c.Contains(SourceText.ToUpper())).ToArray();

            foreach (var ds in showtext)
            {
                var listItem = new ListBoxItem();
                listItem.Content = ds;
                if (ds.All(x => x == ' '))
                    continue;
                ///减小提示框显示数量，避免卡顿
                if (listbox.Items.Count > 10)
                {
                    break;
                }
                listbox.Items.Add(listItem);
            }

            Rect rect = tb.GetRectFromCharacterIndex(tb.SelectionStart);
            Point point = rect.BottomLeft;
            pop.PlacementRectangle = rect;
            if (listbox.Items.Count == 0)
            {
                pop.IsOpen = false;
                return;
            }
            listbox.SelectedIndex = 0;
            pop.IsOpen = false;
            pop.IsOpen = true;
        }


        public void TextReplace()
        {
            var from_pos = tb.SelectionStart;
            var text = tb.Text.Substring(0, tb.SelectionStart);
            var lastIndex = text.LastIndexOf(' ');
            lastIndex = lastIndex < 0 ? 0 : lastIndex + 1;
            var t_r = tb.Text.Remove(lastIndex, text.Length - lastIndex);
            tb.Text = t_r.Insert(lastIndex, vm.SelectedKeyword);
            //tb.SelectionStart = from_pos + vm.SelectedKeyword.Length - (text.Length - lastIndex);
        }

        private void keywords_doubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = listbox.SelectedItem as ListBoxItem;
            vm.SelectedKeyword = (string)item.Content;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CopyToClipboard(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(tb.Text);
        }
    }
}
