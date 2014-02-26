using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace com.newsarea.search.provider {
    
    public class ProviderEventArgs {

        private String _name;
        public String Name {
            get { return this._name; }
        }

        private Image _image;
        public Image Image {
            get { return this._image; }
        }

        private bool _isAvailable;
        public bool IsAvailable {
            get { return this._isAvailable; }
        }

        public ProviderEventArgs(String name, Image image, bool isAvailable) {
            this._name = name;
            this._image = image;
            this._isAvailable = isAvailable;
        }

    }

}
