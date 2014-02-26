using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.newsarea.search.settings.source {

    public interface ISettings : IDictionary<String, StringValue> {

        void Add(String key, String value);

    }

}
