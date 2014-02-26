using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using com.newsarea.search.settings;
using com.newsarea.search.settings.source;

namespace com.newsarea.search.provider {
    
    public interface IProvider {

        event Providers.SearchItemHandler ItemFound;
        event Providers.SearchItemHandler ItemChanged;

        Settings ApplicationSettings {
            get;
            set;
        }

        ISettings Settings {
            get;
            set;
        }

        String Name {
            get;
        }

        bool IsAvailable {
            get;
        }

        void init();

        void search(String searchValue);

        void handleInput(List<SearchResultItem> items, string input);

        void handleSelection(SearchResultItem item);

        System.Drawing.Image getImage();

        List<Uri> getSettingGroups();
    
    }

}
