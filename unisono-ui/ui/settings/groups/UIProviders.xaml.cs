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
using System.Collections.ObjectModel;
using System.IO;
using com.newsarea.search.provider;
using System.Windows.Forms;
using com.newsarea.search.settings.source;

namespace com.newsarea.search.ui.settings.groups {
    /// <summary>
    /// Interaktionslogik für UIProviders.xaml
    /// </summary>
    public partial class UIProviders : System.Windows.Controls.UserControl, IApplicationSettingGroup {

        #region Properties

        public String Title {
            get { return "General\\Providers"; }
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

        private List<IProvider> _providers = null;
        private List<IProvider> Providers {
            get { return this._providers; }
        }

        #endregion

        public UIProviders(List<IProvider> providers) {
            InitializeComponent();
            //
            this._providers = providers;
            /**************************
            *      DIRECTORIES
            **************************/
            btnInsertDirectory.Click += delegate(object sender, RoutedEventArgs e) {
                FolderBrowserDialog fbDialog = new FolderBrowserDialog();
                DialogResult result = fbDialog.ShowDialog();
                if (result == DialogResult.OK) {
                    ObservableCollection<DirectoryInfo> list = (ObservableCollection<DirectoryInfo>)lboxDirectories.ItemsSource;
                    list.Add(new DirectoryInfo(fbDialog.SelectedPath));
                }
            };
            //
            btnRemoveDirectory.Click += delegate(object sender, RoutedEventArgs e) {
                if (lboxDirectories.SelectedItem == null) { return; }
                //
                ObservableCollection<DirectoryInfo> list = (ObservableCollection<DirectoryInfo>)lboxDirectories.ItemsSource;
                list.Remove((DirectoryInfo)lboxDirectories.SelectedItem);
            };
            /**************************
            *      PROVIDERS
            **************************/
            lboxProviders.SelectionChanged += delegate(object sender, SelectionChangedEventArgs e) {
                if (lboxProviders.SelectedValue == null) { return; }
                txtIdentCode.Text = this.getIdentCode((IProvider)lboxProviders.SelectedValue);
            };
            //
            btnAssign.Click += delegate(object sender, RoutedEventArgs e) {
                if (lboxProviders.SelectedValue == null) { return; }
                IProvider provider = (IProvider)lboxProviders.SelectedValue;
                String identCode = this.getIdentCode(provider);
                if(identCode != null) {
                    this.ApplicationSettings.IdentCodes.Remove(identCode);
                }
                //
                try {
                    this.ApplicationSettings.IdentCodes.Add(txtIdentCode.Text, provider.GetType().ToString());
                } catch (Exception) { }
                //
                txtIdentCode.Text = String.Empty;
                this.lboxProviders_databind();
            };
        }

        public void databind() {
            this.lboxDirectories_databind();
            this.lboxProviders_databind();
        }

        private void lboxDirectories_databind() {
            ObservableCollection<DirectoryInfo> list = new ObservableCollection<DirectoryInfo>();
            foreach (DirectoryInfo dInfo in this.ApplicationSettings.ProviderDirectories) {
                list.Add(dInfo);
            }
            //
            lboxDirectories.ItemsSource = list;
        }

        private void lboxProviders_databind() {
            Dictionary<String, IProvider> list = new Dictionary<String, IProvider>();
            foreach (IProvider provider in this.Providers) {
                StringBuilder name = new StringBuilder();                
                String identCode = this.getIdentCode(provider);
                if (identCode != null) {                    
                    name.Append(identCode);
                    name.Append(" => ");
                }
                name.Append(provider.Name);
                //
                list.Add(name.ToString(), provider);
            }
            //
            lboxProviders.ItemsSource = list;
        }

        public void save() {
            this.ApplicationSettings.ProviderDirectories.Clear();
            ObservableCollection<DirectoryInfo> list = (ObservableCollection<DirectoryInfo>)lboxDirectories.ItemsSource;
            foreach (DirectoryInfo dInfo in list) {
                this.ApplicationSettings.ProviderDirectories.Add(dInfo);
            }
        }

        private String getIdentCode(IProvider provider) {
            foreach (KeyValuePair<String, String> kvPair in this.ApplicationSettings.IdentCodes) {
                if (String.Compare(kvPair.Value, provider.GetType().ToString(), true) == 0) {
                    return kvPair.Key;
                }
            }
            return null;
        }

    }
}
