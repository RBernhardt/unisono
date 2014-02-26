using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace com.newsarea.search.provider {
    
    public class CMDProvider : Provider {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        List<SearchResultItem> _items = new List<SearchResultItem>();
        private bool _initialized = false;
        
        public override bool IsAvailable {
            get { return this._initialized; }
        }

        public override string Name {
            get { return "CMD"; }
        }

        public override void init() {
            // append user startmenu
            DirectoryInfo userStartMenuDirectory = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu));
            this.appendFiles(userStartMenuDirectory);
            // append all user startmenu
            DirectoryInfo allUserStartMenuDirectory = new DirectoryInfo(Environment.GetEnvironmentVariable("ALLUSERSPROFILE") + "\\" + userStartMenuDirectory.FullName.Substring(userStartMenuDirectory.FullName.LastIndexOf("\\") + 1));
            this.appendFiles(allUserStartMenuDirectory);  
            //
            this._initialized = true;
        }

        private void appendFiles(DirectoryInfo dInfo) {
            foreach (FileInfo fInfo in dInfo.GetFiles()) {
                SearchResultItem item = new SearchResultItem(fInfo.Name.Replace(".lnk", ""), fInfo.FullName, fInfo);
                this._items.Add(item);
            }
            //
            foreach (DirectoryInfo childDInfo in dInfo.GetDirectories()) {
                this.appendFiles(childDInfo);
            }
        }

        public override void search(String searchValue) {
            foreach (SearchResultItem item in this._items) {
                if (this.containsValue(searchValue, item.Description)) {
                    this.OnItemFound(this, item);
                }
            }
        }

        public override void handleInput(List<SearchResultItem> items, String input) {
            try {
                System.Diagnostics.Process.Start(input);
            } catch { }
        }

        public override void handleSelection(SearchResultItem item) {
            try {
                FileInfo fInfo = (FileInfo)item.Value;
                System.Diagnostics.Process.Start(fInfo.FullName);
            } catch { }
        }

        public override System.Drawing.Image getImage() {
            return global::com.newsarea.search.provider.Properties.Resources.icon_cmd;
        }

        public override List<Uri> getSettingGroups() {
            return null;
        }
    }

}
