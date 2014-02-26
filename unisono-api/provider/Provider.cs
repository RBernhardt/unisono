using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.newsarea.search.utils;
using System.Threading;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Web;
using com.newsarea.search.settings;
using System.IO;
using System.Security.Cryptography;
using System.Diagnostics;
using Microsoft.Win32;
using com.newsarea.search.settings.source;

namespace com.newsarea.search.provider {
    
    public abstract class Provider : BackgroundWorkerItem, IProvider {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public event Providers.SearchItemHandler ItemFound;
        public event Providers.SearchItemHandler ItemChanged;

        private Settings _applicationSettings = null;
        public Settings ApplicationSettings {
            get { return this._applicationSettings; }
            set { this._applicationSettings = value; }
        }

        private ISettings _settings = null;
        public ISettings Settings {
            get { return this._settings; }
            set { this._settings = value; }
        }

        public abstract String Name {
            get;
        }
       
        public abstract bool IsAvailable {
            get;
        }

        public abstract void init();

        public abstract void search(String searchValue);
        
        public abstract void handleInput(List<SearchResultItem> items, string input);

        public abstract void handleSelection(SearchResultItem item);

        public abstract System.Drawing.Image getImage();

        public abstract List<Uri> getSettingGroups();

        protected void OnItemFound(Object sender, SearchResultItem item) {
            base.OnAlive(this, new EventArgs());
            if (ItemFound != null) {
                ItemFound(sender, item);
            }
        }

        protected void OnItemChanged(Object sender, SearchResultItem item) {
            base.OnAlive(this, new EventArgs());
            if (ItemChanged != null) {
                ItemChanged(sender, item);
            }
        }

        #region "Helper"

        public override void startWorker(object[] parameters) {
            this.search((String)parameters[0]);
        }
              
        protected bool containsValue(String searchString, String compareValue) {
            String[] searchArgs = searchString.Split(' ');
            foreach (String search in searchArgs) {
                if (compareValue.ToLower().IndexOf(search.ToLower()) < 0) { return false; }
            }
            //
            return true;
        }

        protected String removeHTMLElements(String input) {
            String result = Regex.Replace(input, "<.*?>", string.Empty);
            return HttpUtility.HtmlDecode(result);
        }
        
        private FileInfo getTempFileInfo(DirectoryInfo dInfo, String filename) {
            if (!dInfo.Exists) {
                dInfo.Create();
            }
            //
            return new FileInfo(dInfo.FullName + "\\" + this.getMD5Hash(filename));
        }

        protected FileInfo getTempFileInfo(String filename) {
            return this.getTempFileInfo(this.ApplicationSettings.TempDirectory, filename);
        }

        protected FileInfo getTempFileInfo() {
            return this.getTempFileInfo(Guid.NewGuid().ToString());
        }

        private String getMD5Hash(string TextToHash) {
            //Prüfen ob Daten übergeben wurden.
            if ((TextToHash == null) || (TextToHash.Length == 0)) {
                return string.Empty;
            }

            //MD5 Hash aus dem String berechnen. Dazu muss der string in ein Byte[]
            //zerlegt werden. Danach muss das Resultat wieder zurück in ein string.
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] textToHash = Encoding.Default.GetBytes(TextToHash);
            byte[] result = md5.ComputeHash(textToHash);

            return System.BitConverter.ToString(result);
        }

        protected void openUrl(String url) {
            try {
                FileInfo browserFile = this.ApplicationSettings.BrowserFileInfo;
                if (browserFile.Exists) {
                    Process.Start(browserFile.FullName, url);
                    return;
                }
                //
                Process.Start(url);
            } catch (Exception ex) {
                log.Error(ex);
            }
        }

        #endregion

    }

}
