using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace com.newsarea.search.settings.source {

    public interface ISettingSource : ISettings {

        void load();
        void save();
        void reset();

    }
}
