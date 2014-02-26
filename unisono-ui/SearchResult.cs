using System;
using System.Linq;
using System.Text;
using com.newsarea.search.provider;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace com.newsarea.search {
    
    public class SearchResult : List<SearchResultItem> {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public delegate void ItemAddedHandler(Object sender, SearchResultItem item);
        public delegate void ItemChangedHandler(Object sender, SearchResultItem item);        

        public event ItemAddedHandler ItemAdded;
        public event ItemChangedHandler ItemChanged;

        private String _searchString = null;
        public String SearchString {
            get { return this._searchString; }
            set { this._searchString = value; }
        }

        private String _searchValue = null;
        public String SearchValue {
            get { return this._searchValue; }
            set { this._searchValue = value; }
        }

        private IProvider _provider = null;
        public IProvider Provider {
            get { return this._provider; }
            set { this._provider = value; }
        }

        public new void Add(SearchResultItem item) {
            base.Add(item);
            //
            if (this.ItemAdded != null) {
                this.ItemAdded(this, item);
            }
        }

        public void OnItemChanged(Object sender, SearchResultItem item) {
            if (this.ItemChanged != null) {
                this.ItemChanged(sender, item);
            }
        }

        public List<SearchResultItem> Clone() {
            List<SearchResultItem> result = new List<SearchResultItem>();
            foreach (SearchResultItem item in this) {
                result.Add(item);
            }
            return result;
        }

    }

}