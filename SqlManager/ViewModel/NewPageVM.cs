using GenerateToolbox.Models;
using Newtonsoft.Json;
using SqlManager.Models;
using SqlManager.Views;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using Xu.Common;

namespace SqlManager.ViewModel
{
    public class NewPageVM : ValidationBase
    {
        public NewPage.NewPage plugin;
        List<DataTable> tables = new List<DataTable>();
        public NewPageVM(NewPage.NewPage Iplugin)
        {
            plugin = Iplugin;
            queryCount = new List<KeyValue<int, string>>
            {
                new KeyValue<int, string>(100, "100"),
                new KeyValue<int, string>(500, "500"),
                new KeyValue<int, string>(1000, "1000"),
                new KeyValue<int, string>(Int32.MaxValue, "Full"),
            };
            _selectedQueryCount = queryCount.FirstOrDefault();

            BindedWords = new Dictionary<string, string>();
        }

        

        private string _Text;
        public string Text
        {
            get
            {
                return _Text;
            }
            set
            {
                //if(string.Join("", value.Except(_Text == null ? "" : _Text).ToList()) != "\r\n")
                //{
                plugin.ShowAutoComplete(value);
                //}
                _Text = value;
                NotifyPropertyChanged("Text");
                //autoComplete.Visibility = Visibility.Collapsed;
                //autoComplete.Visibility = Visibility.Visible ;
                //plugin.pop.IsOpen = false;
                //plugin.pop.IsOpen = true;
                //plugin.tb.Focus();

                //ChangeColor(Color.FromRgb(122, 11, 1), plugin.tb, "from");
                //foreach (var ds in Text)
                //{
                //    TextPointer tp1 = plugin.tb.CaretPosition;
                //    TextPointer tp2 = plugin.tb.Document.ContentEnd;
                //    plugin.tb.SelectionBrush = Brushes.Red;
                //    //plugin.tb.Selection.Select(tp1, tp2);
                //    //plugin.tb.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Blue);
                //}
            }
        }


        private List<KeyValue<int, string>> _queryCount;
        public List<KeyValue<int, string>> queryCount
        {
            get => _queryCount;
            set
            {
                _queryCount = value;
                NotifyPropertyChanged(nameof(queryCount));
            }
        }

        private KeyValue<int, string> _selectedQueryCount;
        public KeyValue<int, string> selectedQueryCount
        {
            get => _selectedQueryCount;
            set
            {
                _selectedQueryCount = value;
                NotifyPropertyChanged(nameof(selectedQueryCount));
                Excute(value);
            }
        }


        /// <summary>
        /// 当前选中的关键词
        /// </summary>
        private string selectedKeyword;
        public string SelectedKeyword
        {
            get
            {
                return selectedKeyword;
            }
            set
            {
                selectedKeyword = value;
                NotifyPropertyChanged(nameof(SelectedKeyword));
            }
        }

        private string _Time;
        public string Time
        {
            get
            {
                return _Time;
            }
            set
            {
                _Time = value;
                NotifyPropertyChanged("Time");
            }
        }

        public Dictionary<string, string> BindedWords;


        #region command

        public SimpleCommand CmdConvertToDeclare => new SimpleCommand()
        {
            ExecuteDelegate = o =>
            {
                try
                {
                    if (string.IsNullOrEmpty(plugin.tb.SelectedText)) return;
                    var item = GenerateToolbox.Models.TableConstruct.InitTabItem(plugin.tb.SelectedText, plugin.CurrentDatabase, plugin.plugin);
                    var tabcontol = new TabControl();
                    item.ForEach(c => tabcontol.Items.Add(c));
                    var page = new MahApps.Metro.Controls.MetroWindow();
                    page.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    page.Title = plugin.tb.SelectedText?.ToUpper();
                    page.Width = 600;
                    page.Content = tabcontol;
                    page.ShowDialog();
                }
                catch(Exception ex)
                {
                    plugin.plugin.vm.Console(ex.Message);
                }
            },
            CanExecuteDelegate = x => true
        };

        public SimpleCommand CmdExcute => new SimpleCommand()
        {
            ExecuteDelegate = o =>
            {
                Excute(selectedQueryCount);
            },
            CanExecuteDelegate = x => true
        };


        public SimpleCommand CopyToInsert => new SimpleCommand()
        {
            ExecuteDelegate = x =>
            {
                try
                {
                    var items = plugin.tab.SelectedItem as TabItem;
                    var dg = items.Content as DataGrid;
                    var selectedItem = (DataRowView)dg.SelectedItem;
                    var sql1 = $"INSERT INTO \"\"(";
                    var sql2 = $" VALUES(";
                    for (int i = 1; i < selectedItem.Row.Table.Columns.Count; i++)
                    {
                        var name = selectedItem.Row.Table.Columns[i];
                        var value = selectedItem?.Row.ItemArray[i];
                        sql1 += name.ColumnName.Replace("__", "_");
                        sql2 += $"\'{value}\'";
                        if (i != selectedItem.Row.Table.Columns.Count - 1)
                        {
                            sql1 += ", ";
                            sql2 += ", ";
                        }
                    }
                    Clipboard.SetText(sql1 + ")" + sql2 + ")");
                }
                catch (Exception ex)
                {
                    Console(ex.Message);
                }
            },
            CanExecuteDelegate = o =>
            {
                return true;
            }
        };


        public SimpleCommand ExportToInsert => new SimpleCommand()
        {
            ExecuteDelegate = x =>
            {
                try
                {
                    var items = plugin.tab.SelectedItem as TabItem;
                    var dg = items.Content as DataGrid;
                    var dv = dg.ItemsSource as DataView;
                    var dt = dv.Table.Copy();
                    dt.Columns.RemoveAt(0);
                    List<string> sqls = new List<string>();
                    
                    foreach (DataRow row in dt.Rows)
                    {
                        var sql1 = $"INSERT INTO \"{((string)items.Header).Replace("__", "_")}\"(";
                        var sql2 = $" VALUES(";
                        for (int i = 0; i < dt.Columns.Count; i++)
                        {
                            var name = dt.Columns[i];
                            var value = row.ItemArray[i];
                            sql1 += name.ColumnName.Replace("__", "_");
                            sql2 += $"\'{value}\'";
                            if (i != dt.Columns.Count - 1)
                            {
                                sql1 += ", ";
                                sql2 += ", ";
                            }
                        }
                        var rsSql = sql1 + ")" + sql2 + ");";
                        sqls.Add(rsSql);
                    }
                    FindNewPage.OpenNewPage(string.Join("\r\n", sqls), ((string)items.Header).Replace("__", "_"), plugin.plugin);
                }
                catch (Exception ex)
                {
                    Console(ex.Message);
                }
            },
            CanExecuteDelegate = o =>
            {
                return true;
            }
        };

        public SimpleCommand CopyToJson => new SimpleCommand()
        {
            ExecuteDelegate = x =>
            {
                try
                {
                    var items = plugin.tab.SelectedItem as TabItem;
                    var dg = items.Content as DataGrid;
                    var dv = dg.ItemsSource as DataView;
                    var dt = dv.Table.Copy();
                    dt.Columns.RemoveAt(0);

                    var json = new JsonSerializer();
                    StringWriter textWriter = new StringWriter();
                    JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
                    {
                        Formatting = Newtonsoft.Json.Formatting.Indented,
                        Indentation = 4,
                        IndentChar = ' '
                    };
                    json.Serialize(jsonWriter, dt);

                    var js = textWriter.ToString();
                    FindNewPage.OpenNewPage(js.Replace("__", "_"), ((string)items.Header).Replace("__", "_"), plugin.plugin);
                }
                catch (Exception ex)
                {
                    Console(ex.Message);
                }
            },
            CanExecuteDelegate = o =>
            {
                return true;
            }
        };



        public SimpleCommand CmdFormatFullSql => new SimpleCommand()
        {
            ExecuteDelegate = x => {
                Xu.Common.SqlHelper.Formatter formatter = new Xu.Common.SqlHelper.Formatter();
                plugin.tb.Text = formatter.Format(plugin.tb.Text);
            },
            CanExecuteDelegate = o => {
                return true;
            }
        };
        public SimpleCommand CmdFormatSelectionSql => new SimpleCommand()
        {
            ExecuteDelegate = x => {
                Xu.Common.SqlHelper.Formatter formatter = new Xu.Common.SqlHelper.Formatter();
                plugin.tb.SelectedText = formatter.Format(plugin.tb.SelectedText);
            },
            CanExecuteDelegate = o => {
                return true;
            }
        };


        #endregion


        #region function

        private void Excute(KeyValue<int, string> query)
        {
            if (plugin?.CurrentDatabase is null || string.IsNullOrEmpty(plugin.CurrentDatabase.connection_string))
            {
                Warning.ShowMsg("未设置数据库！");
                return;
            }
            if (string.IsNullOrEmpty(Text)) return;
            var tx = Text;
            if (tx.Contains(":"))
            {
                var sp = new SqlParams(tx, plugin);
                sp.ShowDialog();
                if (sp.Data is List<EditPamas> ps)
                {
                    foreach (var ds in ps)
                    {
                        tx = tx.Replace(ds.Name, ds.Value);
                    }
                }
            }
            using (var db = SugarContext.GetContext(plugin.CurrentDatabase.connection_string, plugin.CurrentDatabase.dbtype))
            {
                if (tx.ToUpper().Contains("SELECT"))
                {
                    Select(tx, query);
                }
                else
                {
                    Update(db, tx);
                }
            }
        }

        private void Update(SqlSugarClient db, string sql)
        {
            var list = sql.Split(';');
            db.Ado.BeginTran();
            try
            {
                foreach (var ds in list)
                {
                    var num = db.Ado.ExecuteCommand(ds);
                    Console(num + "");
                }
                db.Ado.CommitTran();
            }
            catch (Exception ex)
            {
                db.Ado.RollbackTran();
            }

        }


        private void Select(string text, KeyValue<int, string> query)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var t1 = run(text, query);

            try
            {
                var t3 = Task.Run(() =>
                {
                    try
                    {
                        t1.Wait();

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            stopwatch.Stop();
                            Time = $"查询时间{stopwatch.ElapsedMilliseconds}ms";
                            var list = text.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                            plugin.tab.Items.Clear();
                            foreach (var ds in tables)
                            {
                                var curTable = ds.Copy();
                                DataColumn newC = new DataColumn
                                {
                                    DataType = System.Type.GetType("System.Int32"),
                                    ColumnName = "SEQ",
                                    AutoIncrement = true,//自动增加 
                                    AutoIncrementSeed = 1,//起始为1 
                                    AutoIncrementStep = 1//步长为1
                                };
                                var dt = new DataTable();
                                if (!dt.Columns.Contains("SEQ")) dt.Columns.Add(newC);
                                dt.Merge(curTable);
                                foreach (DataRow row in dt.Rows)
                                {
                                    row["SEQ"] = dt.Rows.IndexOf(row) + 1;
                                }
                                dt.Columns[0].ReadOnly = true;
                                plugin.totalCount.Text = $"  共计{dt.Rows.Count}条数据 ";
                                LoadTab(dt.DefaultView, list[tables.IndexOf(ds)]);
                            }
                            plugin.tab.SelectedIndex = 0;
                        });
                    }
                    catch (Exception ex)
                    {
                        stopwatch.Stop();
                        Time = $"查询时间{stopwatch.ElapsedMilliseconds}ms";
                        Console(ex.Message);
                    }

                });
            }
            catch (AggregateException ex)
            {
                stopwatch.Stop();
                Time = $"查询时间{stopwatch.ElapsedMilliseconds}ms";
                Dialog.ShowMsg(ex.Message);
            }
        }

        
        
        private Task run(string text, KeyValue<int, string> query)
        {

            return Task.Run(() =>
            {
                using (var db = SugarContext.GetContext(plugin.CurrentDatabase.connection_string, plugin.CurrentDatabase.dbtype))
                {
                    db.Ado.BeginTran();
                    try
                    {
                        tables = new List<DataTable>();
                        var list = text.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var ds in list)
                        {
                            var sql = ds;
                            if(query.Value != "Full")
                            {
                                sql = $"select t.* from ({ds}) t where {(plugin.CurrentDatabase.dbtype == SqlSugar.DbType.Oracle ? "rownum" : (plugin.CurrentDatabase.dbtype == SqlSugar.DbType.SqlServer ? "@@ROWCOUNT" : ""))} <= {query.Key}";
                            }
                            var result = db.SqlQueryable<dynamic>(sql).ToDataTable();
                            foreach (DataColumn column in result.Columns)
                            {
                                column.ColumnName = column.ColumnName.Replace("_", "__");
                            }

                            tables.Add(result);
                        }
                        db.Ado.CommitTran();

                    }
                    catch (Exception ex)
                    {
                        db.Ado.RollbackTran();
                        Console(ex.Message);
                    }
                }
            });

        }

        private void LoadTab(DataView view, string sql)
        {
            MenuItem sonItem = new MenuItem
            {
                Header = "复制为INSERT语句",
                Command = CopyToInsert
            };
            MenuItem item = new MenuItem
            {
                Header = "复制"
            };
            MenuItem export = new MenuItem
            {
                Header = "导出"
            };
            export.Items.Add(new MenuItem
            {
                Header = "导出为JSON",
                Command = CopyToJson
            });
            export.Items.Add(new MenuItem
            {
                Header = "导出为INSERT语句",
                Command = ExportToInsert
            });


            item.Items.Add(sonItem);
            ContextMenu menu = new ContextMenu();
            menu.Items.Add(item);
            menu.Items.Add(export);
            DataGrid dg = new DataGrid
            {
                ContextMenu = menu,
                GridLinesVisibility = DataGridGridLinesVisibility.All,
                ItemsSource = view,
                CanUserAddRows = false
            };

            TabItem tabItem = new TabItem
            {
                Height = 30,
                MinWidth = 80,
                Header = GetTableName(sql).ToUpper().Replace("__", "_"),
                Content = dg,
                Style = (Style)plugin.FindResource("TabItemNormal")
            };
            plugin.tab.Items.Add(tabItem);
        }
        private string GetTableName(string sql)
        {
            var list = sql.Split(' ');
            for (var i = 0; i < list.Length; i++)
            {
                if (list[i].ToLower() == "from")
                    return list[i + 1];
            }
            return "NULL";
        }


        private void Console(string content)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                plugin.plugin.console.Text += $"1>{DateTime.Now}：已完成{content}\r\n";
                plugin.plugin.console.ScrollToEnd();
            });
        }

        #endregion
    }
}