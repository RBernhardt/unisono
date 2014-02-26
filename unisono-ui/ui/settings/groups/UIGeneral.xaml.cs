using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using com.newsarea.search.settings;
using System.IO;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using com.newsarea.search.settings.source;

namespace com.newsarea.search.ui.settings.groups {

    public partial class UIGeneral : System.Windows.Controls.UserControl, IApplicationSettingGroup {

        #region Properties

        public String Title {
            get { return "General\\Essential";  }
        }

        private Settings _applicationSettings = null;
        public Settings ApplicationSettings {
            get { return (Settings)this._applicationSettings; }
            set { this._applicationSettings = value; }
        }

        private ISettings _settings = null;
        public ISettings Settings {
            get { return (ISettings)this._settings; }
            set { this._settings = value; }
        }

        #endregion

        public UIGeneral() {
            InitializeComponent();
        }

        #region Databind

        public void databind() {
            cboxLoadOnStartup.IsChecked = this.ApplicationSettings.LoadOnStartUp;
            cboxStartMinimized.IsChecked = this.ApplicationSettings.StartMinimized;
            cboxCheckForUpdates.IsChecked = this.ApplicationSettings.CheckForUpdates;
            //
            this.cboxLanguages_databind();
            //
            txtBrowser.Text = this.ApplicationSettings.BrowserFileInfo.FullName;
            btnBrowseBrowser.Click += delegate(object sender, RoutedEventArgs e) {
                FileDialog fDialog = new OpenFileDialog();
                DialogResult dResult = fDialog.ShowDialog();
                if (dResult == DialogResult.OK) {
                    this.ApplicationSettings.BrowserFileInfo = new FileInfo(fDialog.FileName);
                    txtBrowser.Text = this.ApplicationSettings.BrowserFileInfo.FullName;
                }
            };
            //
            txtTempFolder.Text = this.ApplicationSettings.TempDirectory.FullName;
            btnBrowseTempFolder.Click += delegate(object sender, RoutedEventArgs e) {
                FolderBrowserDialog fBrowser = new FolderBrowserDialog();
                DialogResult dResult = fBrowser.ShowDialog();
                if (dResult == DialogResult.OK) {
                    this.ApplicationSettings.TempDirectory = new DirectoryInfo(fBrowser.SelectedPath);
                    txtTempFolder.Text = this.ApplicationSettings.TempDirectory.FullName;
                }
            };
        }

        private void cboxLanguages_databind() {
            //foreach (CultureInfo cinfo in System.Globalization.CultureInfo.GetCultures(CultureTypes.SpecificCultures)) {
            //    ComboBoxItem item = new ComboBoxItem();
            //    item.Content = cinfo.DisplayName;
            //    item.DataContext = cinfo.Name;
            //    cboxLanguages.Items.Add(item);
            //}
            //
            //coboxLanguages.Items.Add(new KeyValuePair<String, String>("Deutsch", "de-DE"));
            coboxLanguages.Items.Clear();
            coboxLanguages.Items.Add(new KeyValuePair<String, String>("English", "en-US"));
            //
            coboxLanguages.SelectedValue = this.ApplicationSettings.Language.Name;
        }
        
        #endregion

        public void save() {
            this.ApplicationSettings.Language = new CultureInfo(coboxLanguages.SelectedValue.ToString());
            this.ApplicationSettings.StartMinimized = (bool)cboxStartMinimized.IsChecked;
            this.ApplicationSettings.LoadOnStartUp = (bool)cboxLoadOnStartup.IsChecked;
            this.ApplicationSettings.CheckForUpdates = (bool)cboxCheckForUpdates.IsChecked;
        }
    }
}
