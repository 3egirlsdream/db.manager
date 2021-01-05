using SqlManager;
using SqlManager.NewPage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Xu.Common;

namespace GenerateToolbox.Models
{
    public class FindNewPage
    {
        public static NewPage GetPage(TabItemClose tabItem)
        {
            try
            {
                var grid = tabItem?.Content as Grid;
                var frame = grid?.Children[0] as Frame;
                var page = frame?.Content as NewPage;
                return page;
            }
            catch
            {
                return null;
            }
        }

        public static NewPage GetPage(object tabItem)
        {
            try
            {
                var item = tabItem as TabItemClose;
                var grid = item?.Content as Grid;
                var frame = grid?.Children[0] as Frame;
                var page = frame?.Content as NewPage;
                return page;
            }
            catch
            {
                return null;
            }
        }

        public static void OpenNewPage(string text, string name, MainWindow plugin)
        {
            var item = new TabItemClose
            {
                Header = name,
                Height = 30,
                MinWidth = 100
            };
            var content = new Grid();
            content.Margin = new Thickness(-3);
            var npage = new NewPage(item, plugin);
            npage.vm.Text = text;
            var f = new Frame { Content = npage };
            content.Children.Add(f);
            item.Content = content;
            item.GotFocus += plugin.Item_GotFocus;
            plugin.tabcontol.Items.Add(item);
            item.Focus();
            MainWindow.CurrentTabItem = item;
            //return item;
        }
    }
}
