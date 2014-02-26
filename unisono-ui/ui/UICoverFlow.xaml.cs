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
using System.Collections.ObjectModel;

namespace com.newsarea.search.ui {
    /// <summary>
    /// Interaktionslogik für UICoverFlow.xaml
    /// </summary>
    public partial class UICoverFlow : UserControl {

        ObservableCollection<BitmapSource> _items = new ObservableCollection<BitmapSource>();
        public ObservableCollection<BitmapSource> Items {
            get { return this._items; }
            set { this._items = value; }
        }

        public UICoverFlow() {
            InitializeComponent();
            //
            lboxElementFlow.ItemsSource = this.Items;
        }

    }
}
