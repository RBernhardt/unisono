using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using com.newsarea.search.provider;
using com.newsarea.search.settings;
using com.newsarea.search.settings.source;
using com.newsarea.search.ui.settings;
using com.newsarea.search.ui.settings.groups;
using com.newsarea.search.utils;
using System.Diagnostics;

namespace com.newsarea.search.ui {
    /// <summary>
    /// Interaktionslogik für UISearch.xaml
    /// </summary>
    public partial class UISearch : Window {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Properties

        private String _searchString = String.Empty;

        delegate void providers_ProviderAvailabilityChangedCallback(Object sender, ProviderEventArgs e);

        private ISettingSource _defaultSettings = null;
        private ISettingSource DefaultSettings {
            get {
                FileInfo defaultSettingFileInfo = new FileInfo("./default.xml");
                defaultSettingFileInfo.Delete();
                //
                FileStream fStream = new FileStream(defaultSettingFileInfo.FullName, FileMode.OpenOrCreate, FileAccess.Write);
                Stream settingStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("com.newsarea.search.data.settings.xml");
                byte[] data = new byte[settingStream.Length];
                settingStream.Read(data, 0, data.Length);
                fStream.Write(data, 0, data.Length);
                fStream.Close();
                fStream = null;
                //
                if (this._defaultSettings == null) {
                    this._defaultSettings = new XMLSettingSource(defaultSettingFileInfo);
                    this._defaultSettings.load();
                }
                return this._defaultSettings;
            }
        }

        private Settings _settings = null;
        private Settings Settings {
            get {                
                if (this._settings == null) {
                    FileInfo userFile = new FileInfo(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/NEWSAREA/usettings_search_manager.xml");                    
                    this._settings = new Settings(new XMLSettingSource(userFile, this.DefaultSettings));
                    this._settings.load();
                }
                return this._settings;
            }
        }

        private Providers _providers = null;
        public Providers Providers {
            get {
                if (this._providers == null) {
                    this._providers = new Providers(this.Settings);
                }
                return this._providers; 
            }
        }

        #endregion

        public UISearch() {
            try {
                InitializeComponent();
                //
                log.Info("/****************************************************************/");
                log.Info("/*                        APP START                             */");
                log.Info("/****************************************************************/");
                //if (this.UpdateManager != null) {
                //    log.Info("Version: " + this.UpdateManager.CurrentVersion);
                //    log.Info("Last Version: " + this.UpdateManager.LastVersion);
                //}
                ////
                //if (this.Settings.CheckForUpdates && this.UpdateManager.IsUpdateAvailable) {
                //    DialogResult dResult = System.Windows.Forms.MessageBox.Show("There is an update available, would you like to get it now?", "Update Available", MessageBoxButtons.YesNo);
                //    if (String.Compare(dResult.ToString(), "Yes") == 0) {
                //        Process.Start(this.UpdateManager.getWebsite());
                //    }
                //}
                //
                Thread.CurrentThread.CurrentUICulture = this.Settings.Language;
                //
                bool isVisible = !this.Settings.StartMinimized;
                this.Visibility = isVisible ? Visibility.Visible : Visibility.Hidden;
                //
                txtSearch.Focusable = false;
                txtSearch.Background = System.Windows.Media.Brushes.LightGray;
                //
                // INIT TRAY ICON
                NotifyIcon trayIcon = new NotifyIcon();
                trayIcon.Icon = global::com.newsarea.search.Properties.Resources.trayIcon;
                System.Windows.Forms.ContextMenu trayIconContextMenu = new System.Windows.Forms.ContextMenu();
                System.Windows.Forms.MenuItem mItemSettings = new System.Windows.Forms.MenuItem(Properties.Resources.Settings, new EventHandler(mItem_Settings));
                mItemSettings.Enabled = false;
                trayIconContextMenu.MenuItems.Add(mItemSettings);
                trayIconContextMenu.MenuItems.Add(new System.Windows.Forms.MenuItem(Properties.Resources.Exit, new EventHandler(mItem_Exit)));
                trayIcon.ContextMenu = trayIconContextMenu;
                trayIcon.Visible = true;
                //
                // KEY HOOK EVENTS
                this.initKeyHook();
                //
                // INIT PROVIDER LOCATING
                this.Providers.ProviderAvailabilityChanged += new Providers.ProviderEventHandler(providers_ProviderAvailabilityChanged);
                this.Providers.init();
                //
                // PROVIDER LOCATÌNG COMPLETED
                this.Providers.ProviderLocatingCompleted += delegate(object sender, EventArgs e) {
                    mItemSettings.Enabled = true;
                };
                //
                // INIT SEARCH
                this.initSearch();
                //
                // INIT LAYOUT EVENTS
                ucSearchResult.LayoutUpdated += delegate(object sender, EventArgs e) {
                    this.resizeWindow();
                };
                lMessage.LayoutUpdated += delegate(object sender, EventArgs e) {
                    this.resizeWindow();
                };
                pbMain.ValueChanged += delegate(object sender, RoutedPropertyChangedEventArgs<double> e) {
                    this.resizeWindow();
                };
                //
                this.IsVisibleChanged += delegate(object sender, DependencyPropertyChangedEventArgs e) {
                    if (this.Visibility != Visibility.Visible) { return; }
                    //
                    this.Focus();
                    txtSearch.Focus();
                };
                //
                iLogo.MouseUp += delegate(Object sender, MouseButtonEventArgs e) {
                    this.toggleFullscreen();
                };
            } catch (Exception ex) {
                log.Error(ex.Message, ex);
            }
        }

        private void providers_ProviderAvailabilityChanged(Object sender, ProviderEventArgs e) {
            try {
                if (this.Dispatcher.Thread != System.Threading.Thread.CurrentThread) {
                    providers_ProviderAvailabilityChangedCallback bR = new providers_ProviderAvailabilityChangedCallback(providers_ProviderAvailabilityChanged);
                    this.Dispatcher.Invoke(bR, new object[] { sender, e });
                    return;
                }
                //
                StackPanel newPanel = e.IsAvailable ? stackIcons : stackInactiveIcons;
                StackPanel oldPanel = e.IsAvailable ? stackInactiveIcons : stackIcons;
                //
                List<UIElement> itemListToRemove = new List<UIElement>();
                // append to remove list
                foreach (System.Windows.Controls.Image currentBox in oldPanel.Children) {
                    //log.Debug("old panel " + e.Name + " " + currentBox.Tag + " " + this.Dispatcher.Thread.ManagedThreadId);
                    if (String.Compare(currentBox.Tag.ToString(), e.Name, true) == 0) {
                        itemListToRemove.Add(currentBox);
                    }
                }
                // append to remove list
                foreach (System.Windows.Controls.Image currentBox in newPanel.Children) {
                    //log.Debug("old panel " + e.Name + " " + currentBox.Tag + " " + this.Dispatcher.Thread.ManagedThreadId);
                    if (String.Compare(currentBox.Tag.ToString(), e.Name, true) == 0) {
                        itemListToRemove.Add(currentBox);
                    }
                }
                // remove ui elements from stackpanel
                foreach (UIElement uiElem in itemListToRemove) {
                    oldPanel.Children.Remove(uiElem);
                    newPanel.Children.Remove(uiElem);
                }
                //
                System.Drawing.Image img = e.Image;
                if (!e.IsAvailable) {
                    img = this.getGrayscale(new Bitmap(img));
                }
                //
                System.Windows.Controls.Image imgControl = new System.Windows.Controls.Image();
                imgControl.Tag = e.Name;
                imgControl.Width = 15;
                imgControl.Height = 15;
                imgControl.Margin = new Thickness(0, 0, 10, 0);
                imgControl.Source = this.getBitmapSource(img);
                //
                newPanel.Children.Add(imgControl);
                //
                txtSearch.Focusable = newPanel.Children.Count > 0;
                if (txtSearch.Focusable) {
                    txtSearch.Focus();
                }
                txtSearch.Background = txtSearch.Focusable ? System.Windows.Media.Brushes.White : System.Windows.Media.Brushes.LightGray;                
            } catch (Exception ex) {
                log.Error(ex, ex);
            }
        }

        private void mItem_Settings(Object sender, EventArgs e) {
            this.Visibility = Visibility.Collapsed;
            UISettings settings = new UISettings();
            //
            settings.SettingGroups.Add(new String[] { "general" }, new UIGeneral());
            settings.SettingGroups.Add(new String[] { "proxy" }, new UIProxy());
            settings.SettingGroups.Add(new String[] { "providers" }, new UIProviders(this.Providers.AvailableProviders));
            //
            foreach (IProvider provider in this.Providers.AvailableProviders) {
                if (provider.getSettingGroups() == null) { continue; }
                foreach (Uri xamlUri in provider.getSettingGroups()) {
                    ISettingGroup sGroup = (ISettingGroup)System.Windows.Application.LoadComponent(xamlUri);
                    settings.SettingGroups.Add(new String[] { "providersettings", provider.GetType().ToString() }, sGroup);
                }
            }
            //
            settings.Settings = this.Settings;            
            settings.Show();
        }

        private void mItem_Exit(Object sender, EventArgs e) {
            this.Providers.dispose();
            this.Close();
        }

        private void resetApplication() {
            txtSearch.Text = String.Empty;
            ucSearchResult.clear();
            imgCurrentProvider.Source = this.getBitmapSource(global::com.newsarea.search.Properties.Resources.search);
            this.WindowState = WindowState.Normal;
        }

        private void handleSelection(SearchResultItem item) {
            lMessage.Content = "ausgewählter Eintrag wird aufgerufen";
            this.Providers.handleSelection(item, txtSearch.Text);            
            this.Visibility = Visibility.Hidden;
            lMessage.Content = String.Empty;
        }

        #region Search

        private delegate void searchInitialized_Callback(ProviderEventArgs e);
        private delegate void ucSearchResult_addItem_Callback(SearchResultItem item);
        private delegate void ucSearchResult_updateItem_Callback(SearchResultItem item);
        private delegate void progressChanged_Callback(int value, String message);        

        private System.Windows.Forms.Timer _searchTimer = null;

        private void initSearch() {
            // init search timer
            this._searchTimer = new System.Windows.Forms.Timer();
            this._searchTimer.Enabled = false;
            this._searchTimer.Interval = 1000;
            this._searchTimer.Tick += delegate(Object dsender, EventArgs de) {
                this._searchString = txtSearch.Text;
                this._searchTimer.Enabled = !this.Providers.search(txtSearch.Text);
                if (!this._searchTimer.Enabled) {
                    lMessage.Content = String.Empty;
                    pbMain.Value = 0;
                    ucSearchResult.clear();
                }
            };
            //
            this.Providers.SearchInitialized += delegate(Object dsender, ProviderEventArgs de) {
                this.searchInitialized(de);
            };
            //
            this.Providers.SearchItemFound += delegate(Object dsender, SearchResultItem item) {
                this.ucSearchResult_addItem(item);
            };
            //
            this.Providers.SearchItemChanged += delegate(Object dsender, SearchResultItem item) {
                this.ucSearchResult_updateItem(item);
            };
            //
            this.Providers.SearchProgressChanged += delegate(object sender, System.ComponentModel.ProgressChangedEventArgs e) {
                String message = null;    
                if(e.UserState != null) {
                    message = (String)e.UserState;
                }
                progressChanged(e.ProgressPercentage, message);
            };
            //
            this.Providers.SearchTimeout += delegate(Object dsender, EventArgs de) {
                this.searchTimeOut();
            };
            //
            this.Providers.SearchCompleted += delegate(Object dsender, ProviderEventArgs de) {
                this.searchCompleted();
            };
            //
            txtSearch.KeyUp += new System.Windows.Input.KeyEventHandler(txtSearch_KeyUp); 
            //
            ucSearchResult.ItemSelected += delegate(Object sender, EventArgs e) {
                this.handleSelection(ucSearchResult.SelectedItem);
            };
        }

        private void txtSearch_KeyUp(object sender, System.Windows.Input.KeyEventArgs e) {
            this._searchTimer.Enabled = false;
            //
            if (e.Key == Key.Enter) {
                this.Providers.handleInput(new List<SearchResultItem>(), txtSearch.Text);
                this.Visibility = Visibility.Hidden;
                //
                return;
            }
            //
            if (String.Compare(txtSearch.Text, this._searchString) == 0) { return; }
            //
            this.Providers.cancelSearchASync();
            this._searchTimer.Enabled = true;            
        }
               
        private void searchInitialized(ProviderEventArgs e) {
            if (this.Dispatcher.Thread != System.Threading.Thread.CurrentThread) {
                searchInitialized_Callback bR = new searchInitialized_Callback(searchInitialized);
                this.Dispatcher.Invoke(bR, new Object[] { e });
                return;
            }
            //
            imgCurrentProvider.Source = this.getBitmapSource(e.Image);
            //
            lMessage.Content = "Suche wird durchgeführt";
        }

        private void searchTimeOut() {
            if (this.Dispatcher.Thread != System.Threading.Thread.CurrentThread) {
                this.Dispatcher.Invoke(new MethodInvoker(searchTimeOut));
                return;
            }
            //
            lMessage.Content = "Die Anfrage konnte aufgrund eine Zeitüberschreitung nicht korrekt abgeschlossen werden.";
            pbMain.Value = 0;
        }

        private void progressChanged(int value, String message) {
            if (this.Dispatcher.Thread != System.Threading.Thread.CurrentThread) {
                progressChanged_Callback bR = new progressChanged_Callback(progressChanged);
                this.Dispatcher.Invoke(bR, new Object[] { value, message });
                return;
            }
            //
            if (message != null && message != String.Empty) {
                lMessage.Content = message;
            }
            pbMain.Value = value;
        }

        private void ucSearchResult_addItem(SearchResultItem item) {
            if (this.Dispatcher.Thread != System.Threading.Thread.CurrentThread) {
                ucSearchResult_addItem_Callback bR = new ucSearchResult_addItem_Callback(ucSearchResult_addItem);
                this.Dispatcher.Invoke(bR, new Object[] { item });
                return;
            }
            //
            ucSearchResult.addItem(item);
        }

        private void ucSearchResult_updateItem(SearchResultItem item) {
            if (this.Dispatcher.Thread != System.Threading.Thread.CurrentThread) {
                ucSearchResult_updateItem_Callback bR = new ucSearchResult_updateItem_Callback(ucSearchResult_updateItem);
                this.Dispatcher.Invoke(bR, new Object[] { item });
                return;
            }
            //
            ucSearchResult.updateItem(item);
        }

        private void searchCompleted() {
            if (this.Dispatcher.Thread != System.Threading.Thread.CurrentThread) {
                this.Dispatcher.Invoke(new MethodInvoker(searchCompleted));
                return;
            }
            //
            lMessage.Content = String.Empty;
            if (ucSearchResult.ItemsCount == 0) {
                lMessage.Content = "Es wurden keine übereinstimmenden Elemente gefunden";
            }
            //
            pbMain.Value = 0;
        }
        
        #endregion

        //#region SearchResult Selections

        //public void initSearchResultSelections() {
        //    //this.KeyUp += new System.Windows.Input.KeyEventHandler(initSearchResult_UISearch_KeyUp);
        //    //
        //    /* txtSearch.MouseLeftButtonUp += delegate(object sender, MouseButtonEventArgs e) {
        //        UISearchResultItem selectedItem = this.getSelectedItem(stackResults);
        //        if (selectedItem != null) {
        //            selectedItem.Selected = false;
        //        }
        //    };*/
        //}

        ////private void initSearchResult_UISearch_KeyUp(object sender, System.Windows.Input.KeyEventArgs e) {
        ////    if (!(e.Key == Key.Down || e.Key == Key.Up)) { return; }
        ////    //
        ////    UISearchResultItem selectedItem = this.getSelectedItem(stackResults);           
        ////    //
        ////    int selectedItemIdx = -1;
        ////    if (selectedItem != null) {
        ////        selectedItemIdx = stackResults.Children.IndexOf(selectedItem);                
        ////    }
        ////    //
        ////    if (e.Key == Key.Down && stackResults.Children.Count > selectedItemIdx + 1) {
        ////        ((UISearchResultItem)stackResults.Children[selectedItemIdx + 1]).Selected = true;                
        ////        if (selectedItem != null) {
        ////            selectedItem.Selected = false;
        ////        }
        ////    }
        ////    //
        ////    if (e.Key == Key.Up && selectedItemIdx > 0) {
        ////        ((UISearchResultItem)stackResults.Children[selectedItemIdx - 1]).Selected = true;                
        ////        selectedItem.Selected = false;
        ////    }
        ////}

        //#endregion

        #region Key Hook

        private KeyboardManager _keyHook = new KeyboardManager();
        private long _lastLControlKeyUpTime = 0;

        private void initKeyHook() {
            this._keyHook.Start();
            KeyboardManager.KeyUp += new System.Windows.Forms.KeyEventHandler(keyHook_KeyUp);
            this.Closed += delegate(object sender, EventArgs e) {
                this._keyHook.Stop();
            };
        }
                
        private void keyHook_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e) {
            log.Debug(e.KeyCode);
            switch(e.KeyCode) {
                case Keys.LControlKey:
                    long currentTime = DateTime.Now.Ticks;
                    long timeLControlDiff = currentTime - this._lastLControlKeyUpTime;
                    //
                    this._lastLControlKeyUpTime = currentTime;
                    //
                    if (timeLControlDiff > 10000000) { return; }
                    //
                    this.toggleWindowVisibility();
                    if (this.Visibility != Visibility.Visible) {
                        this.resetApplication();
                    }
                    this._lastLControlKeyUpTime = 0;
                    break;
                case Keys.Tab:
                    if (this.Visibility == Visibility.Visible) {
                        this.toggleFullscreen();
                    }
                    break;
                case Keys.Escape:
                    if (this.Visibility == Visibility.Visible) {
                        this.toggleWindowVisibility();
                    }
                    break;
                default:
                    this._lastLControlKeyUpTime = 0;
                    break;
            }
        }

        private void toggleWindowVisibility() {
            this.Visibility = this.Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;            
        }

        private void toggleFullscreen() {
            WindowState newState = this.WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
            ucSearchResult.setFullScreen(newState == WindowState.Maximized);
            //
            this.WindowState = newState;
            //    
            if (this.Visibility == Visibility.Visible) {
                this.Activate();
                txtSearch.TabIndex = 0;                
                txtSearch.Focus();
            }
        }

        #endregion

        #region Win Resize Animation

        private const int MAX_PREVIEW_ITEMS = 10;
        private const int DEFAULT_HEIGHT = 200;

        private static AutoResetEvent mrEvtProviderAvailabilityExit = new AutoResetEvent(true);

        private void resizeWindow() {
            int defaultWindowHeight = DEFAULT_HEIGHT;            
            //
            int itemCount = ucSearchResult.ItemsCount < MAX_PREVIEW_ITEMS ? ucSearchResult.ItemsCount : MAX_PREVIEW_ITEMS;
            //      
            bool emptyMessage = lMessage.Content == null || lMessage.Content.ToString() == String.Empty;
            lMessage.Visibility = emptyMessage ? Visibility.Collapsed : Visibility.Visible;
            if (lMessage.Visibility != Visibility.Visible) {
                defaultWindowHeight -= 20;
            }
            //
            pbMain.Visibility = pbMain.Value == 0 ? Visibility.Collapsed : Visibility.Visible;
            if (pbMain.Visibility != Visibility.Visible) {
                defaultWindowHeight -= 20;
            }
            //
            int marginBottom = 20;
            marginBottom += lMessage.Visibility == Visibility.Visible ? 20 : 0;
            marginBottom += pbMain.Visibility == Visibility.Visible ? 20 : 0;
            //
            Thickness cTNess = ucSearchResult.Margin;
            ucSearchResult.Margin = new Thickness(cTNess.Left, cTNess.Top, cTNess.Right, marginBottom);
            //
            if (itemCount == 0) { defaultWindowHeight -= 50; }
            //
            double currentHeight = gridMain.Height;
            resizeWindow(this.Width, defaultWindowHeight + (itemCount * 25));
        }

        private void resizeWindow(double width, double height) {
            if (this.WindowState == WindowState.Maximized) { return; }
            if (this.Width == width && this.Height == height) { return; }
            //
            System.Windows.Size newSize = new System.Windows.Size(width, height);
            this.Height = newSize.Height;
            this.Width = newSize.Width;
            //
            //this.Top = (Screen.PrimaryScreen.Bounds.Height / 2) - (this.Height / 2);
            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            ////
            ////bool waitOne = mrEvtProviderAvailabilityExit.WaitOne();
            ////log.Debug("waitOne - " + waitOne);
            ////if (!waitOne) {
            ////    return;
            ////}
            ////
            ////
            //Storyboard resizeAnimator = PrepareStoryboard(new System.Windows.Size(thisthis.Win.Width, this.Height), newSize);
            //resizeAnimator.Completed += delegate(object sender, EventArgs e) {
            //    //log.Debug("resizeWindow - Completed - (" + (int)newSize.Width + "x" + (int)newSize.Height + ") - (" + (int)this.Width + "x" + (int)this.Height + ")");
            //    //if (!(((int)this.Width) == (int)newSize.Width && ((int)this.Height) == (int)newSize.Height)) { return; }                
            //    //mrEvtProviderAvailabilityExit.Set();
            //};
            ////
            //if ((int)this.Width == (int)newSize.Width && (int)this.Height == (int)newSize.Height) {
            //    //log.Debug("resizeWindow - Exit - (" + (int)newSize.Width + "x" + (int)newSize.Height + ") - (" + (int)this.Width + "x" + (int)this.Height + ")");
            //    return;
            //}
            //// Create Storyboard
            ////log.Debug("resizeWindow - Begin - (" + (int)newSize.Width + "x" + (int)newSize.Height + ") - (" + (int)this.Width + "x" + (int)this.Height + ")");
            //resizeAnimator.Begin(this, false); 
        }

        private Storyboard PrepareStoryboard(System.Windows.Size size, System.Windows.Size newSize) {
            Storyboard board = new Storyboard();
            board.FillBehavior = FillBehavior.HoldEnd;

            // Width
            if (size.Width != newSize.Width) {
                DoubleAnimation wAnim = new DoubleAnimation(size.Width, newSize.Width, new Duration(TimeSpan.FromMilliseconds(300)));
                Storyboard.SetTargetProperty(wAnim, new PropertyPath("(0)", WidthProperty));
                board.Children.Add(wAnim);
            }

            // Height
            DoubleAnimation hAnim = new DoubleAnimation(size.Height, newSize.Height, new Duration(TimeSpan.FromMilliseconds(300)));
            Storyboard.SetTargetProperty(hAnim, new PropertyPath("(0)", HeightProperty));

            board.Children.Add(hAnim);
            return board;
        }

        #endregion

        #region Helper

        private BitmapSource getBitmapSource(System.Drawing.Image img) {
            Bitmap bitmap = new Bitmap(img);
            IntPtr HBitmap = bitmap.GetHbitmap();
            BitmapSource result = Imaging.CreateBitmapSourceFromHBitmap
                (HBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            bitmap.Dispose();
            return result;
        }

        private System.Drawing.Image getImage(System.Drawing.Image image, int width, int height) {
            if (width == -1) {
                double factor = 1.0 * height / image.Height;
                width = (int)Math.Round(image.Width * factor);
            }
            //
            return image.GetThumbnailImage(width, height, null, IntPtr.Zero);
        }

        private Bitmap getGrayscale(Bitmap original) {
            //create a blank bitmap the same size as original
            Bitmap newBitmap = new Bitmap(original.Width, original.Height);

            //get a graphics object from the new image
            Graphics g = Graphics.FromImage(newBitmap);

            //create the grayscale ColorMatrix
            ColorMatrix colorMatrix = new ColorMatrix(
              new float[][] {
                new float[] {.3f, .3f, .3f, 0, 0},
                new float[] {.59f, .59f, .59f, 0, 0},
                new float[] {.11f, .11f, .11f, 0, 0},
                new float[] {0, 0, 0, 1, 0},
                new float[] {0, 0, 0, 0, 1}
            });

            //create some image attributes
            ImageAttributes attributes = new ImageAttributes();

            //set the color matrix attribute
            attributes.SetColorMatrix(colorMatrix);

            //draw the original image on the new image
            //using the grayscale color matrix
            g.DrawImage(original,
               new System.Drawing.Rectangle(0, 0, original.Width, original.Height),
               0, 0, original.Width, original.Height,
               GraphicsUnit.Pixel, attributes);

            //dispose the Graphics object
            g.Dispose();
            return newBitmap;
        }

        #endregion

    }
}