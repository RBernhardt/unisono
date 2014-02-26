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
using System.Drawing;
using System.Windows.Interop;

namespace com.newsarea.search.ui {
    /// <summary>
    /// Interaktionslogik für SearchResultItem.xaml
    /// </summary>
    public partial class UISearchResultItem : UserControl {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public bool Opened {
            set { cContent.Visibility = value ? Visibility.Visible : Visibility.Collapsed; }
        }

        private bool _selected = false;
        public bool Selected {
            get { return this._selected; }
            set { 
                this._selected = value;
                lName.FontWeight = this._selected ? FontWeights.Bold : FontWeights.Normal;
                this.Background = this._selected ? System.Windows.Media.Brushes.GhostWhite : System.Windows.Media.Brushes.Transparent;
            }
        }

        public UISearchResultItem() {
            InitializeComponent();
            //
            cContent.Visibility = Visibility.Collapsed;
        }

        public void Databind() {
            SearchResultItem searchItem = (SearchResultItem)this.DataContext;
            //
            System.Drawing.Image icon = searchItem.Icon;
            if (icon == null) {
                icon = global::com.newsarea.search.Properties.Resources.standard;
            }
            //
            iIcon.Source = this.getBitmapSource(icon);
            //
            // append pre items
            preDescription.Children.Clear();
            foreach (KeyValuePair<String, String> kvPreDescription in searchItem.PreDescription) {
                this.appendItem(kvPreDescription, preDescription);
            }
            //
            // append post items
            postDescription.Children.Clear();
            foreach (KeyValuePair<String, String> kvPostDescription in searchItem.PostDescription) {
                this.appendItem(kvPostDescription, postDescription);
            }
            //     
            boIImage.Visibility = Visibility.Collapsed;
            if (searchItem.ImageUri != null) {
                BitmapSource bSource = new BitmapImage(searchItem.ImageUri);
                System.Drawing.Size iThumbSize = this.getSize(bSource, 100);
                iImage.Height = iThumbSize.Height;
                iImage.Width = iThumbSize.Width;
                boIImage.Visibility = Visibility.Visible;
                iImage.Source = bSource;
                //
                if (bSource.Height > iImage.Height) {
                    iImage.Cursor = Cursors.Help;
                    System.Windows.Controls.Image iToolTipImage = new System.Windows.Controls.Image();
                    iToolTipImage.Source = iImage.Source;
                    System.Drawing.Size iImageSize = this.getSize(bSource, 400);
                    iToolTipImage.Height = iImageSize.Height;
                    iToolTipImage.Width = iImageSize.Width;                    
                    //
                    ToolTip tip = new ToolTip();
                    tip.Content = iToolTipImage;
                    iImage.ToolTip = tip;
                }
            }
        }

        private System.Drawing.Size getSize(BitmapSource bitmapSource, int height) {
            double factor = 1.0 * bitmapSource.Height / height;
            int width = (int)Math.Round(1.0 * bitmapSource.Width / factor);
            //
            return new System.Drawing.Size(width, height);
        }

        private void appendItem(KeyValuePair<String, String> kvPair, StackPanel container) {
            StackPanel itemStack = new StackPanel();
            itemStack.Orientation = Orientation.Horizontal;
            //
            if (kvPair.Key != null && kvPair.Key != String.Empty) {
                Label lKey = new Label();
                lKey.Height = 20;
                lKey.FontWeight = FontWeights.Bold;
                lKey.Margin = new Thickness(0, 5, 10, 0);
                lKey.Padding = new Thickness(0);
                lKey.Content = kvPair.Key + ":";
                itemStack.Children.Add(lKey);
            }
            //
            Label lValue = new Label();
            lValue.Height = 20;
            lValue.Margin = new Thickness(0, 5, 0, 0);
            lValue.Padding = new Thickness(0);
            lValue.Content = kvPair.Value;
            itemStack.Children.Add(lValue);
            //
            container.Children.Add(itemStack);
        }

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

    }
}
