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
using com.newsarea.search.settings.source;

namespace com.newsarea.search.ui.settings.groups {
    /// <summary>
    /// Interaktionslogik für UserControl1.xaml
    /// </summary>
    public partial class UIProxy : System.Windows.Controls.UserControl, IApplicationSettingGroup {

        public String Title {
            get { return "General\\Proxy"; }
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

        public UIProxy() {
            InitializeComponent();
        }

        public void databind() {
            txtHTTPProxy.Text = this.ApplicationSettings.HttpProxy;
        }

        public void save() {
            this.ApplicationSettings.HttpProxy = txtHTTPProxy.Text;
        }
    }
}
