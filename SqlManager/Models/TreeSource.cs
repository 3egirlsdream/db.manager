using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xu.Common;

namespace SqlManager.Models
{
    
    public class TreeList : ValidationBase
    {
        static string parentIcon = "../../img/Folder_32x32.png";
        static string folderIcon = "../../img/folder.png";
        static string childIcon = "../../img/Item_32x32.png";
        static string tableIcon = "../../img/表格.png";
        static string dbIcon = "../../img/数据库.png";
        
        private TreeLevel _LEVEL = TreeLevel.File;
        /// <summary>
        /// 树层级
        /// </summary>
        public TreeLevel LEVEL
        {
            get
            {
                return _LEVEL;
            }
            set
            {
                _LEVEL = value;
                NotifyPropertyChanged("LEVEL");
                SelectIcon();
            }
        }

        public bool IS_OPEN { get; set; } = false;

        /// <summary>
        /// 层名称
        /// </summary>
        public string NODE_NAME { get; set; }
        /// <summary>
        /// 连接字符串
        /// </summary>
        public string CONN_STRING { get; set; }


        private string _NoteIcon;
        /// <summary>
        /// 图标
        /// </summary>
        public string NoteIcon
        {
            get
            {
                return _NoteIcon;
            }
            set
            {
                _NoteIcon = value;
                NotifyPropertyChanged("NoteIcon");
            }
        }

        private string sqlType;
        public string  SqlType 
        {
            get => sqlType;
            set
            {
                sqlType = value;
                NotifyPropertyChanged(nameof(SqlType));
                Type = value == "Oracle" ? SqlSugar.DbType.Oracle : (value == "MsSql" ? SqlSugar.DbType.SqlServer : SqlSugar.DbType.Oracle);
            }
        }
        private SqlSugar.DbType? _type;
        public SqlSugar.DbType? Type 
        {
            get => _type;
            
            set
            {
                _type = value;
                NotifyPropertyChanged(nameof(Type));
            }
        }
        /// <summary>
        /// 子节点
        /// </summary>
        private ObservableCollection<TreeList> _Children;
        public ObservableCollection<TreeList> Children
        {
            get
            {
                return _Children;
            }
            set
            {
                _Children = value;
                NotifyPropertyChanged("Children");
            }
        }

        public List<string> Tables { get; set; }

        public TreeList()
        {
            SelectIcon();
            Children = new ObservableCollection<TreeList>();
        }

        private void SelectIcon()
        {
            switch (LEVEL)
            {
                case TreeLevel.Table:
                    NoteIcon = tableIcon;
                    break;
                case TreeLevel.Database:
                    NoteIcon = dbIcon;
                    break;
                case TreeLevel.File:
                    NoteIcon = parentIcon;
                    break;
                case TreeLevel.Folder:
                    NoteIcon = folderIcon;
                    break;
            }
        }
    }

    public enum TreeLevel
    {
        File = 1,
        Database = 2,
        Table = 3,
        Folder = 4
    }


    public class KeyValue<TKey, TValue>
    {
        public TKey Key { get; set; }
        public TValue Value { get; set; }
        public KeyValue(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }
        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
