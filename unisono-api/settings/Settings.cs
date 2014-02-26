using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using com.newsarea.search.settings.source;
using com.newsarea.search.provider;
using System.Net;

namespace com.newsarea.search.settings {
    
    public class Settings {

        private ISettingSource _source = null;
        public ISettings Source {
            get { return this._source; }
        }

        List<DirectoryInfo> _providerDirectories = null;
        public List<DirectoryInfo> ProviderDirectories {
            get {
                if(this._providerDirectories == null) {
                    this._providerDirectories = new List<DirectoryInfo>();
                    ISettings settings = this.getNode(new String[] { "providers", "directories" });
                    if (settings.Values != null) {
                        foreach (StringValue value in settings.Values.ToList()) {
                            this._providerDirectories.Add(new DirectoryInfo(value.Value));
                        }
                    }
                }
                //
                return this._providerDirectories;
            }
        }

        private Dictionary<String, String> _identCodes = null;
        public Dictionary<String, String> IdentCodes {
            get {
                if (this._identCodes == null) {
                    this._identCodes = new Dictionary<String, String>();
                    ISettings settings = this.getNode(new String[] { "providers", "identCodes" });
                    if (settings.Values != null) {
                        foreach (StringValue value in settings.Values.ToList()) {
                            this._identCodes.Add(value.Value, value.Name);
                        }
                    }
                }
                //
                return this._identCodes;
            }
        }

        public CultureInfo Language {
            get { return new CultureInfo(this.Source["general"]["language"].Value); }
            set { this.Source["general"]["language"].Value = value.Name; }
        }

        public DirectoryInfo TempDirectory {
            get { return new DirectoryInfo(this.Source["general"]["tempDirectory"].Value); }
            set { this.Source["general"]["tempDirectory"].Value = value.FullName; }
        }

        public FileInfo BrowserFileInfo {
            get { return new FileInfo(this.Source["general"]["browserFileInfo"].Value); }
            set { this.Source["general"]["browserFileInfo"].Value = value.FullName; }
        }

        public bool StartMinimized {
            get { return Boolean.Parse(this.Source["general"]["startMinimized"].Value); }
            set { this.Source["general"]["startMinimized"].Value = value.ToString(); }
        }

        public bool LoadOnStartUp {
            get { return Boolean.Parse(this.Source["general"]["loadOnStartup"].Value); }
            set { this.Source["general"]["loadOnStartup"].Value = value.ToString(); }
        }

        public bool CheckForUpdates {
            get { return Boolean.Parse(this.Source["general"]["checkForUpdates"].Value); }
            set { this.Source["general"]["checkForUpdates"].Value = value.ToString(); }
        }

        public String HttpProxy {
            get { 
                String value = this.Source["proxy"]["http"].Value;
                if (value == "") {
                    value = null;
                }
                return value;
            }
            set { this.Source["proxy"]["http"].Value = value; }
        }

        public Settings(ISettingSource source) {
            this._source = source;
        }

        public void load() {
            this._source.load();
        }

        public void save() {
            ISettings providerDirectorySettings = this.getNode(new String[] { "providers", "directories" });
            providerDirectorySettings.Clear();
            foreach (DirectoryInfo providerDirectory in this.ProviderDirectories) {
                int idx = this.ProviderDirectories.IndexOf(providerDirectory);
                providerDirectorySettings.Add(idx.ToString(), providerDirectory.FullName);
            }
            //
            ISettings identCodesSettings = this.getNode(new String[] { "providers", "identCodes" });
            identCodesSettings.Clear();
            foreach (KeyValuePair<String, String> kvIdentCodes in this.IdentCodes) {
                identCodesSettings.Add(kvIdentCodes.Value, kvIdentCodes.Key);
            }
            //
            this._source.save();
        }

        public void reset() {
            this._source.reset();
        }

        private ISettings getNode(String[] path, ISettings settings) {
            if (!settings.ContainsKey(path[0])) {
                settings.Add(path[0], new StringValue(path[0], null));
            }
            //
            ISettings resultSettings = settings[path[0]];
            //
            if (path.Length > 1) {
                String[] newPath = new String[path.Length - 1];
                for (int i = 1; i < path.Length; i++) {
                    newPath[i - 1] = path[i];
                }
                return this.getNode(newPath, resultSettings);
            }
            //
            return resultSettings;
        }

        public ISettings getNode(String[] path) {
            return this.getNode(path, this.Source);
        }

        public override String ToString() {
            return this.Source.ToString();
        }

    }

}
