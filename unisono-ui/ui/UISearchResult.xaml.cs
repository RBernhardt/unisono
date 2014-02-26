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
using System.Windows.Controls.Primitives;
using System.Threading;

namespace com.newsarea.search.ui {
    
    /// <summary>
    /// Interaktionslogik für UISearchResult.xaml
    /// </summary>
    public partial class UISearchResult : UserControl {

        public event EventHandler ItemSelected;

        public enum ViewModeENum {
            NORMAL,
            DETAILED
        }

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private SearchResultItem _selectedItem = null;
        public SearchResultItem SelectedItem {
            get { return this._selectedItem; }
        }

        private bool IsShowToolBar {
            get { return tbarViewMode.Visibility == Visibility.Visible; }
            set { tbarViewMode.Visibility = value ? Visibility.Visible : Visibility.Collapsed; }
        }

        //private bool IsShowCoverFlow {
        //    get { return ucCoverFlow.Visibility == Visibility.Visible; }
        //    set { 
        //        ucCoverFlow.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
        //        tbtnViewCoverFlow.IsChecked = value;
        //    }
        //}

        public int ItemsCount {
            get { return stackResults.Children.Count; }
        }

        public UISearchResult() {
            InitializeComponent();
            //
            tbarViewMode.AddHandler(Button.ClickEvent, new RoutedEventHandler(tbarViewMode_Click));
            //
            this.LayoutUpdated += new EventHandler(UISearchResult_LayoutUpdated);
            //
            this.setFullScreen(false);
            //
            tbtnViewCoverFlow.Visibility = Visibility.Collapsed;
        }

        void UISearchResult_LayoutUpdated(object sender, EventArgs e) {
            this.updateView();
        }

        public void addItem(SearchResultItem item) {
            UISearchResultItem itemUI = new UISearchResultItem();
            itemUI.Opened = this.IsShowToolBar;
            itemUI.Selected = false;
            itemUI.DataContext = item;
            itemUI.MouseDown += delegate(object sender, MouseButtonEventArgs e) {

            };
            itemUI.MouseDoubleClick += delegate(object sender, MouseButtonEventArgs e) {
                this._selectedItem = item;
                if (this.ItemSelected != null) {
                    this.ItemSelected(this, new EventArgs());
                }
                this._selectedItem = null;
            };
            itemUI.Databind();            
            stackResults.Children.Add(itemUI);
        }

        public void updateItem(SearchResultItem item) {            
            UISearchResultItem itemUI = this.getItemUI(item);
            if (itemUI == null) { return; }
            //
            itemUI.DataContext = item;
            itemUI.Databind();
            //
            //if (item.ImageUri != null) {
            //    ucCoverFlow.Items.Add(new BitmapImage(item.ImageUri));
            //}
        }

        public void setViewMode(ViewModeENum viewMode) {
            bool opened = false;
            //
            tbtnViewNormal.IsChecked = false;
            tbtnViewDetailed.IsChecked = false;            
            //
            switch (viewMode) {
                case ViewModeENum.NORMAL:
                    opened = false;
                    tbtnViewNormal.IsChecked = true;
                    break;

                case ViewModeENum.DETAILED:
                    opened = true;
                    tbtnViewDetailed.IsChecked = true;                    
                    break;

            }
            //
            foreach (UISearchResultItem itemUI in stackResults.Children) {
                itemUI.Opened = opened;
            }
        }

        public void setFullScreen(bool fullscreen) {
            this.IsShowToolBar = fullscreen;            
            this.setViewMode(fullscreen ? ViewModeENum.DETAILED : ViewModeENum.NORMAL);
            //
            //if (!fullscreen) {
            //    this.IsShowCoverFlow = false;
            //}
        }

        public void clear() {
            stackResults.Children.Clear();
            //ucCoverFlow.Items.Clear();
        }

        private void updateView() {
            double height = 0;
            height += this.IsShowToolBar ? 40 : 0;
            //height += this.IsShowCoverFlow ? 310 : 0;
            rdHeader.Height = new GridLength(height);
        }

        private void tbarViewMode_Click(object sender, RoutedEventArgs e) {
            Control btnViewMode = (Control)e.OriginalSource;
            //
            if (String.Compare(btnViewMode.Name, tbtnViewNormal.Name) == 0) {
                this.setViewMode(ViewModeENum.NORMAL);                
            } else if (String.Compare(btnViewMode.Name, tbtnViewDetailed.Name) == 0) {
                this.setViewMode(ViewModeENum.DETAILED);
            } else if (String.Compare(btnViewMode.Name, tbtnViewCoverFlow.Name) == 0) {
                //this.IsShowCoverFlow = !this.IsShowCoverFlow;                
            }
        }

        private UISearchResultItem getItemUI(SearchResultItem item) {
            foreach (UIElement uiElem in stackResults.Children) {
                UISearchResultItem itemUI = (UISearchResultItem)uiElem;
                if (itemUI.DataContext.Equals(item)) {
                    return itemUI;
                }
            }
            return null;
        }
        

    }

}
