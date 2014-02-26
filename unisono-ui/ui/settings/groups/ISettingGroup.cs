using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.newsarea.search.settings;
using com.newsarea.search.settings.source;

namespace com.newsarea.search.ui.settings.groups {

    public interface ISettingGroup {

        String Title {
            get;
        }

        ISettings Settings {
            get;
            set;
        }

        void databind();
        void save();

    }

}
