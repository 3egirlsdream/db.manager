using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlManager.Models
{
    class Init
    {
        /// <summary>
        /// 初始化关键词
        /// </summary>
        /// <returns></returns>
        public static List<string> KeywordsInit()
        {
            var keys = Strings.LoadJson("Models/keywords.json");
            var words = JsonConvert.DeserializeObject<List<string>>(keys);
            return words.Distinct().ToList();
        }

        /// <summary>
        /// 初始化左边列表
        /// </summary>
        /// <returns></returns>
        public static ObservableCollection<TreeList> TreeInit()
        {
            var treeList = Strings.LoadJson("Models/dbconfig.json");

            return JsonConvert.DeserializeObject<ObservableCollection<TreeList>>(treeList);

        }


        public static List<UncloseFileModel> InitUncloseFile()
        {
            var files = Strings.LoadJson("Models/unclosefileconfig.json");

            return JsonConvert.DeserializeObject<List<UncloseFileModel>>(files);
        }
    }

    public class UncloseFileModel
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string FileText { get; set; }
    }
}
