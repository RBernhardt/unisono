using System;
using System.Collections.Generic;
using System.Text;
using com.newsarea.search.utils;
using System.ComponentModel;
using com.newsarea.search.ui.settings.groups;

namespace com.newsarea.search.provider {
    
    public class IntelligentProvider : Provider {

        public override bool IsAvailable {
            get { return true; }
        }

        public override string Name {
            get { return "Intelligent"; }
        }

        public override void init() { /* */ }

        public override void search(string searchValue) {
            if (String.Compare(searchValue, "tv") == 0) {
                this.readRSSFeed("http://www.tvmovie.de/rss/tvjetzt.xml");
            }
        }

        public override void handleInput(List<SearchResultItem> items, string input) {
            
        }

        public override void handleSelection(SearchResultItem item) {
            
        }

        public override System.Drawing.Image getImage() {
            return global::com.newsarea.search.provider.Properties.Resources.icon_intelligent;
        }

        public override List<Uri> getSettingGroups() {
            return null;
        }

    }
}
