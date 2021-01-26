using Microsoft.Win32;
using NPOI.XSSF.UserModel;
using SqlManager.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using Model.Helper;
using System.Windows;
using Xu.Common;
using System.Threading.Tasks;
using ControlzEx.Native;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using GenerateToolbox.Models;

namespace SqlManager.ViewModel
{
    public class MainView : ValidationBase
    {
        #region construct
        MainWindow plugin;
        public MainView(MainWindow IPlugin)
        {
            plugin = IPlugin;
            TreeSource = Init.TreeInit();
            InitDatabase(TreeSource, cb_connection_itemsource);
            cb_selected_connection_itemsource = cb_connection_itemsource.FirstOrDefault();
        }

        #region init
        private void InitDatabase(ObservableCollection<TreeList> trees, List<dbcfg> keyvalues)
        {
            if (keyvalues == null) keyvalues = new List<dbcfg>();
            foreach (var item in trees)
            {

                if (item.LEVEL == TreeLevel.Database && !string.IsNullOrEmpty(item.CONN_STRING))
                {
                    keyvalues.Add(new dbcfg { db_name = item.NODE_NAME, connection_string = item.CONN_STRING, dbtype = item.Type.Value });
                }
                if (item.Children != null && item.Children.Count != 0)
                {
                    InitDatabase(item.Children, keyvalues);
                }
            }
            cb_connection_itemsource = keyvalues;
        }
        #endregion

        public MainView()
        {
        }
        #endregion

        #region Model Part Words

        /// <summary>
        /// 当前打开的数据库
        /// </summary>
        public ObservableCollection<TreeList> SelectedChildrenList { get; set; } = new ObservableCollection<TreeList>();

        private ObservableCollection<TreeList> treeSource;
        public ObservableCollection<TreeList> TreeSource
        {
            get
            {
                return treeSource;
            }
            set
            {
                treeSource = value;
                NotifyPropertyChanged(nameof(TreeSource));
            }
        }

        private TreeList _SelectedNode;
        public TreeList SelectedNode
        {
            get
            {
                return _SelectedNode;
            }
            set
            {
                _SelectedNode = value;
                NotifyPropertyChanged("SelectedNode");
                //SelectedChildrenList = JsonConvert.DeserializeObject<ObservableCollection<TreeList>>(JsonConvert.SerializeObject(SelectedNode.Children));
            }
        }


        private List<dbcfg> _cb_connection_itemsource;
        public List<dbcfg> cb_connection_itemsource
        {
            get => _cb_connection_itemsource;
            set
            {
                _cb_connection_itemsource = value;
                NotifyPropertyChanged(nameof(cb_connection_itemsource));
            }
        }

        public bool IsChange { get; set; } = true;
        private dbcfg _cb_selected_connection_itemsource;
        public dbcfg cb_selected_connection_itemsource
        {
            get => _cb_selected_connection_itemsource;
            set
            {
                _cb_selected_connection_itemsource = value;
                NotifyPropertyChanged(nameof(cb_selected_connection_itemsource));
                if (value != null && IsChange)
                {
                    var page = GenerateToolbox.Models.FindNewPage.GetPage(MainWindow.CurrentTabItem);
                    if (page != null) page.CurrentDatabase = value;
                }
            }
        }


        private Key_Value _Filter;
        public Key_Value Filter
        {
            get
            {
                return _Filter;
            }
            set
            {
                _Filter = value;
                NotifyPropertyChanged("Filter");
            }
        }

        private Key_Value _Constraint;
        public Key_Value Constraint
        {
            get
            {
                return _Constraint;
            }
            set
            {
                _Constraint = value;
                NotifyPropertyChanged("Constraint");
            }
        }

        /// <summary>
        /// 是否生成sugar模型
        /// </summary>
        private bool _IsSugar;
        public bool IsSugar
        {
            get
            {
                return _IsSugar;
            }
            set
            {
                _IsSugar = value;
                NotifyPropertyChanged("IsSugar");
            }
        }

        private string _SqlText;
        public string SqlText
        {
            get
            {
                return _SqlText;
            }
            set
            {
                _SqlText = value;
                NotifyPropertyChanged("SqlText");
            }
        }

        private string _DBName;
        public string DBName
        {
            get
            {
                return _DBName;
            }
            set
            {
                _DBName = value;
                Common.SetConfig("DBName", value);
                NotifyPropertyChanged("DBName");
            }
        }




        #endregion

        #region Model Command

        public SimpleCommand CmdOpen => new SimpleCommand()
        {
            ExecuteDelegate = o =>
            {
                switch (Filter.value)
                {
                    case 0: SqlText = OpenFile(); break;
                    case 1: OpenNotifyFile(); break;
                    case 2: Common.Export(Common.SetConfig("DbName").ToUpper() + ".cs", SqlText); break;
                    case 4: FormatJson(); break;
                    case 6: GenerateModel();break;
                    case 7: SqlText = Guid.NewGuid().ToString("N").ToUpper();break;
                    default: break;
                }
            }
        };




        #endregion

        #region Model部分方法

    

        private string OpenFile()
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Excel (*.XLSX)|*.xlsx|all file|*.*";
            open.ShowDialog();

            try
            {
                XSSFWorkbook xss;
                using (FileStream fs = new FileStream(open.FileName, FileMode.Open, FileAccess.Read))
                {
                    xss = new XSSFWorkbook(fs);
                }

                List<excel> sFCs = new List<excel>();
                XSSFSheet sheet = (XSSFSheet)xss.GetSheetAt(0);

                
                string[] url = open.FileName.Split('\\');
                string comment = "";
                string pk = "";
                string[] name = url.Last().Split(' ');
                string str = "CREATE TABLE " + name[0].Replace(".xlsx", "") + "(\r\n";
                int cot = sheet.LastRowNum;
                if (Constraint.label == "Oracle")
                {
                    str += Excel2Sql.SQL().Excel2Oracle(sheet, name[0].Replace(".xlsx", ""), cot, ref comment, ref pk);
                }
                else
                {
                    str += Excel2Sql.SQL().Excel2MysqlMssql(sheet, cot, Constraint.value);
                }
                str += "\r\n) TABLESPACE MESPROD;\r\n";
                str += comment;
                str += $"\r\nALTER TABLE {name[0].Replace(".xlsx", "")} ADD CONSTRAINT {pk} PRIMARY KEY(ID) USING INDEX TABLESPACE MESPROX;\r\n";
                str += $"GRANT SELECT ON {name[0].Replace(".xlsx", "")} TO MESPROREAD;";
                return str.Replace("  ", " ").Replace(" ,", ",");
            }
            catch (Exception ex)
            {
                Warning warning = new Warning(ex.Message);
                warning.ShowDialog();
            }

            return "";
        }

        string SorName;
        private void OpenNotifyFile()
        {
            try
            {
                OpenFileDialog open = new OpenFileDialog();
                open.Filter = "all file|*.*";
                open.ShowDialog();
                StreamReader file, f;
                string str;
                ArrayList use;
                using (FileStream fs = new FileStream(open.FileName, FileMode.Open, FileAccess.Read))
                {
                    SorName = open.FileName;
                    use = new ArrayList();
                    file = new StreamReader(open.FileName, Encoding.Default);
                    f = new StreamReader(fs, Encoding.Default);
                    str = file.ReadToEnd();

                    string line;
                    while ((line = f.ReadLine()) != null)
                    {
                        if (line.Contains("using")) use.Add(line);
                        if (line.Contains("namespace")) use.Add(line + "{");
                        if (line.Contains("class"))
                        {
                            use.Add(line);
                            break;
                        }
                    }
                }
                ArrayList arr = new ArrayList(str.Split(' '));
                ArrayList marxs = new ArrayList();


                //get using 

                for (int i = 0; i < arr.Count; i++)
                {
                    if (Common.IsType(arr[i].ToString()))
                    {
                        marxs.Add(arr[i + 1]);
                    }
                }

                string FileContent = "";
                foreach (string s in use)
                {
                    FileContent += s;
                }
                if (FileContent.Length != 0) FileContent += "{";
                foreach (var t in marxs)
                {
                    string pri = "private string _" + t.ToString() + ";";
                    string pub = "public string " + t.ToString() + "{ get{return _" + t.ToString() + ";} set{_" + t.ToString() + " = value; NotifyPropertyChanged(\"" + t.ToString() + "\");}}";
                    FileContent += pri + pub;
                }

                FileContent = Common.format(FileContent);
                SqlText = FileContent;
            }
            catch (Exception ex)
            {
                Warning warning = new Warning(ex.Message);
                warning.ShowDialog();
            }

        }

        
        private void FormatJson()
        {
            if (String.IsNullOrEmpty(SqlText))
                return;
            string source = SqlText;
            ArrayList arr = new ArrayList();
            Stack<char> stack = new Stack<char>();
            foreach (var c in source)
                stack.Push(c);

            while (stack.Count > 0)
            {
                if (stack.Peek() == ':')
                {
                    stack.Pop();
                    if (stack.Peek() == '"')
                    {
                        stack.Pop();
                        string str = string.Empty;
                        while (stack.Peek() != '"')
                        {
                            str += stack.Pop();
                        }
                        stack.Pop();
                        var marx = str.ToCharArray();
                        Array.Reverse(marx);
                        str = "";
                        foreach (var t in marx)
                        {
                            str += t;
                        }
                        if (!arr.Contains(str))
                        {
                            if (Constraint.value == 1)
                            {
                                if (Common.HasLower(str))
                                    arr.Add(str);
                            }
                            else
                            {
                                arr.Add(str);
                            }
                        }
                    }
                }
                else stack.Pop();
            }
            //arr.Sort(new SortByLength());

            ArrayList models = new ArrayList();
            SqlText = "";
            SqlText = "namespace json\n{\n\tpublic class models\n\t{\n";
            foreach (var marx in arr)
            {
                string model = "";
                model += "\t\tpublic string " + marx + " {get;set;}";
                SqlText += model + "\n";
                models.Add(model);
            }
            SqlText += "\t}\n}";
        }

        private async void ModelRun()
        {
            await Task.Run(() =>
            {
                if (!string.IsNullOrEmpty(DBName))
                {
                    ModelHelper model = new ModelHelper();
                    SqlSugar.DbType ty = SqlSugar.DbType.SqlServer;
                    switch (Constraint.value)
                    {
                        case 3:
                            ty = SqlSugar.DbType.MySql;
                            break;
                        case 4:
                            ty = SqlSugar.DbType.Oracle;
                            break;
                    }
                    string json = model.GetTableJson(DBName, ty);
                    if (IsSugar)
                        SqlText = Common.format(model.ModelCreate(json, "MeiCloud.DataAccess", "Sugar"));
                    else
                        SqlText = Common.format(model.ModelCreate(json, "MeiCloud.DataAccess"));

                    SqlText = SqlText.Replace("\nusing Creative.ODA;", "");
                    Loading.Loading.Framework.HideLoading();
                }
            }).ConfigureAwait(true);
        }


        private void GenerateModel()
        {
            
            try
            {
                Loading.Loading.Framework.ShowLoading();
                ModelRun();
            }
            catch (Exception ex)
            {
                Warning.ShowMsg(ex.Message);
            }
        }



        public void Console(string content)
        {
            plugin.console.Text += $"1>{DateTime.Now}：{content}\r\n";
            plugin.console.ScrollToEnd();
        }

        #endregion

        #region menu
        public SimpleCommand CmdOpenFolder => new SimpleCommand()
        {
            ExecuteDelegate = x =>
            {
                OpenFileDialog dialog = new OpenFileDialog
                {
                    Multiselect = true,
                    Filter = "SQL (*.sql)|*.sql|txt (*.txt)|*.txt|all (*.*)|*.*"
                };
                var flag = dialog.ShowDialog();
                if (flag == true)
                {
                    foreach (var name in dialog.FileNames)
                    {
                        using (var fs = new FileStream(name, FileMode.Open, FileAccess.Read))
                        {
                            var rs = new StreamReader(fs);
                            var rte = rs.ReadToEnd();
                            FindNewPage.OpenNewPage(rte, $"{dialog.FileName.Split('\\').Last().Split(' ')[0]}", plugin);
                        }
                    }
                } 
                
            },
            CanExecuteDelegate = o => true
        };


        public SimpleCommand OpenTableConstruct => new SimpleCommand()
        {
            ExecuteDelegate = x => {
                try
                {
                    var item = GenerateToolbox.Models.TableConstruct.Init(SelectedNode, plugin);
                    item.Header = SelectedNode.NODE_NAME;
                    item.MinWidth = 100;
                    item.Height = 30;
                    plugin.tabcontol.Items.Add(item);
                    item.Focus();
                }
                catch (Exception ex)
                {
                    Console(ex.Message);
                }
            },
            CanExecuteDelegate = o => {
                return SelectedNode != null && SelectedNode.LEVEL == TreeLevel.Table;
            }
        };


        public SimpleCommand OpenLink => new SimpleCommand()
        {
            ExecuteDelegate = x =>
            {
                if (SelectedNode == null || SelectedNode.LEVEL == TreeLevel.File || SelectedNode.LEVEL == TreeLevel.Table)
                    return;
                OracleHelper.CloseAllTable(TreeSource);
                //CurDatabase = TableOperation.OpenLink(SelectedNode);

                ///打开所有表
                try
                {
                    using (var db = SugarContext.GetContext(SelectedNode.CONN_STRING, SelectedNode.Type.Value))
                    {
                        var sql = @"select * from all_tables";
                        if(SelectedNode.Type == SqlSugar.DbType.SqlServer)
                        {
                            sql = "select name as TABLE_NAME, type_desc as OWNER from sys.tables";
                        }
                        var tables = db.SqlQueryable<Tables>(sql).ToList();
                        var names = tables.GroupBy(c => c.OWNER).Select(c => new Tables
                        {
                            OWNER = c.Key,
                            tables = c.Select(z => z.TABLE_NAME).OrderBy(z=>z).ToList()
                        }).ToList();

                        SelectedNode.Children = new ObservableCollection<TreeList>();
                        SelectedNode.IS_OPEN = true;
                        SelectedChildrenList = new ObservableCollection<TreeList>();
                        foreach (var ds in names)
                        {
                            TreeList list = new TreeList();
                            list.LEVEL = TreeLevel.Folder;
                            list.NODE_NAME = ds.OWNER;
                            list.ParentNode = SelectedNode;
                            SelectedNode.Children.Add(list);

                            SelectedChildrenList.Add(list);

                            list.Children = new ObservableCollection<TreeList>();
                            foreach(var table in ds.tables)
                            {
                                TreeList tr = new TreeList();
                                tr.LEVEL = TreeLevel.Table;
                                tr.NODE_NAME = table;
                                tr.ParentNode = list;
                                list.Children.Add(tr);
                            }
                            //plugin.keywords.Add(list.NODE_NAME);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console(ex.Message);
                }
            },
            CanExecuteDelegate = o => {
                return SelectedNode != null && SelectedNode.LEVEL != TreeLevel.File && SelectedNode.LEVEL != TreeLevel.Table;
            }
        };

        public SimpleCommand CmdCreateData => new SimpleCommand()
        {
            ExecuteDelegate = x =>
            {
                MakeData.MakeData page = new MakeData.MakeData(SelectedNode.NODE_NAME);
                var item = new TabItemClose();
                var frame = new Frame { Content = page };
                frame.Margin = new Thickness(-3);
                item.Header = "生成数据";
                item.MinWidth = 100;
                item.Height = 30;
                item.Content = frame;
                plugin.tabcontol.Items.Add(item);
                item.Focus();
            },
            CanExecuteDelegate = o => SelectedNode != null && SelectedNode.LEVEL == TreeLevel.Table
        };

        public bool IsEdit { get; set; } = false;
        public SimpleCommand AddConn => new SimpleCommand()
        {
            ExecuteDelegate = x => {
                //Json = JsonConvert.SerializeObject(TreeSource);
                //这里默认增加根节点
                IsEdit = false;
                SelectedNode = null;
                AddConnection window = new AddConnection(this);
                window.ShowDialog();
            },
            CanExecuteDelegate = o => {
                return true;
            }
        };

        public SimpleCommand AddChildren => new SimpleCommand()
        {
            ExecuteDelegate = x => {
                IsEdit = false;
                AddConnection window = new AddConnection(this);
                window.ShowDialog();
                InitDatabase(TreeSource, null);
            },
            CanExecuteDelegate = o => {
                return SelectedNode != null && SelectedNode.LEVEL == TreeLevel.File;
            }
        };

        public SimpleCommand EditNode => new SimpleCommand()
        {
            ExecuteDelegate = x => {
                IsEdit = true;
                AddConnection window = new AddConnection(this);
                window.ShowDialog();
            },
            CanExecuteDelegate = o => {
                return true;
            }
        };


        public SimpleCommand CmdFormatFullSql => new SimpleCommand()
        {
            ExecuteDelegate = x => {
                var page = FindNewPage.GetPage(MainWindow.CurrentTabItem);
                if (page is null) return;
                Xu.Common.SqlHelper.Formatter formatter = new Xu.Common.SqlHelper.Formatter();
                page.tb.Text = formatter.Format(page.tb.Text);
                //page.tb.Text = SqlHelper.FormatSql(page.tb.Text);
            },
            CanExecuteDelegate = o => {
                return true;
            }
        };
        public SimpleCommand CmdFormatSelectionSql => new SimpleCommand()
        {
            ExecuteDelegate = x => {
                var page = FindNewPage.GetPage(MainWindow.CurrentTabItem);
                if (page is null) return;
                Xu.Common.SqlHelper.Formatter formatter = new Xu.Common.SqlHelper.Formatter();
                page.tb.SelectedText = formatter.Format(page.tb.SelectedText);
            },
            CanExecuteDelegate = o => {
                return true;
            }
        };


        public SimpleCommand CloseLink => new SimpleCommand()
        {
            ExecuteDelegate = x =>
            {
                SelectedNode.Children = new ObservableCollection<TreeList>();
            },
            CanExecuteDelegate = o => SelectedNode != null && SelectedNode.LEVEL == TreeLevel.Database
        };
        #endregion
    }
}
