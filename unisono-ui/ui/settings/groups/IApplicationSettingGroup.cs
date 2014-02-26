using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.newsarea.search.settings;

namespace com.newsarea.search.ui.settings.groups {

    public interface IApplicationSettingGroup : ISettingGroup {

        Settings ApplicationSettings {
            get;
            set;
        }

    }

}
