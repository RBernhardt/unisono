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
using System.Windows.Shapes;
using com.newsarea.search.provider;
using com.newsarea.search.settings;
using com.newsarea.search.ui.settings.groups;

namespace com.newsarea.search.ui.settings {

    public partial class UISettings : Window {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Settings Settings {
            get { return (Settings)base.DataContext; }
            set { base.DataContext = value; }
        }

        private Dictionary<String[], ISettingGroup> _settingGroups = new Dictionary<String[], ISettingGroup>();
        public Dictionary<String[], ISettingGroup> SettingGroups {
            get { return this._settingGroups; }
        }
        
        public UISettings() {
            InitializeComponent();
            //
            this.DataContextChanged += new DependencyPropertyChangedEventHandler(UISettings_DataContextChanged);
            tvSettings.SelectedItemChanged += new RoutedPropertyChangedEventHandler<object>(tvSettings_SelectedItemChanged);
            btnSave.Click += delegate(object sender, RoutedEventArgs e) {
                foreach (ISettingGroup settingGroup in this.SettingGroups.Values.ToList()) {
                    if (settingGroup.Settings != null) {
                        settingGroup.save();
                    }
                }
                //
                this.Settings.save();
                this.Close();
            };
            btnCancel.Click += delegate(object sender, RoutedEventArgs e) {
                this.Settings.reset();
                this.Close();
            };
        }

        void UISettings_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            this.tvSettings_databind();
            //
            this.spSettings_databind(this.SettingGroups.First());
        }

        private void tvSettings_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            TreeViewItem tvItem = (TreeViewItem)e.NewValue;
            this.spSettings_databind((KeyValuePair<String[], ISettingGroup>)tvItem.DataContext);
        }

        #region Databind

        private void tvSettings_databind() {
            foreach (KeyValuePair<String[], ISettingGroup> sGroup in this.SettingGroups) {
                this.appendToTreeView(tvSettings.Items, sGroup.Value.Title.Split('\\'), sGroup);
            }
        }

        private void spSettings_databind(KeyValuePair<String[], ISettingGroup> kvSettingGroup) {
            //
            // save setting group settings
            foreach(UIElement uiElem in spSettings.Children) {
                ((ISettingGroup)uiElem).save();
            }
            //
            // clear view of setting groups
            spSettings.Children.Clear();
            spSettings.Children.Add((UIElement)kvSettingGroup.Value);
            //                        
            if (kvSettingGroup.Value is IApplicationSettingGroup) {
                ((IApplicationSettingGroup)kvSettingGroup.Value).ApplicationSettings = this.Settings;
            }
            //            
            kvSettingGroup.Value.Settings = this.Settings.getNode(kvSettingGroup.Key);
            kvSettingGroup.Value.databind();
            //
            String[] path = kvSettingGroup.Value.Title.Split('\\');
            lTitle.Content = path[path.Length - 1];
        }

        #endregion

        private void appendToTreeView(ItemCollection itemCollection, String[] path, KeyValuePair<String[], ISettingGroup> kvSettingGroup) {
            TreeViewItem tvItem = this.getTreeViewItem(itemCollection, path[0]);
            if (tvItem == null) {
                tvItem = new TreeViewItem();
                tvItem.Header = path[0];
                tvItem.DataContext = kvSettingGroup;
                tvItem.IsExpanded = true;
                itemCollection.Add(tvItem);
            }
            //
            if (path.Length > 1) {
                String[] newPath = new String[path.Length - 1];
                for (int i = 1; i < path.Length; i++) {
                    newPath[i - 1] = path[i];
                }
                this.appendToTreeView(tvItem.Items, newPath, kvSettingGroup);
            }
        }

        private TreeViewItem getTreeViewItem(ItemCollection collection, String header) {
            foreach (TreeViewItem item in collection) {
                if (String.Compare(item.Header.ToString(), header) == 0) {
                    return item;
                }
            }
            return null;
        }

    }
}
