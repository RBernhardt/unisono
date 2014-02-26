using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

namespace com.newsarea.search {
    
    public class SearchResultItem {

        #region Properties

        private String _name = null;
        public String Name {
            get { return this._name; }
        }

        private Dictionary<String, String> _preDescription = new Dictionary<String, String>();
        public Dictionary<String, String> PreDescription {
            get { return this._preDescription; }
        }

        private String _description = null;
        public String Description {
            get { return this._description; }
        }

        private Dictionary<String, String> _postDescription = new Dictionary<String, String>();
        public Dictionary<String, String> PostDescription {
            get { return this._postDescription; }
        }

        private Object _value = null;
        public Object Value {
            get { return this._value; }
        }

        private Image _icon = null;
        public Image Icon {
            get { return this._icon; }
            set { this._icon = value; }
        }

        private Uri _imageUri = null;
        public Uri ImageUri {
            get { return this._imageUri; }
            set { this._imageUri = value; }
        }

        #endregion

        public SearchResultItem(String name, String description, Object value, Image icon) {
            this._name = name;
            this._description = description;
            this._value = value;
            this._icon = icon;
        }

        public SearchResultItem(String name, String description, Object value)
            : this(name, description, value, null) {
        }
    }

}
