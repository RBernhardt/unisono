using com.newsarea.search.settings.source;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.newsarea.search.settings {

    public abstract class Values : Dictionary<String, StringValue>, ISettings {

        public void Add(String key, String value) {
            base.Add(key, new StringValue(key, value));
        }

        public override String ToString() {
            StringBuilder strBld = new StringBuilder();
            foreach (KeyValuePair<String, StringValue> kvPair in this) {
                strBld.Append(kvPair.Value.ToString());
            }
            return strBld.ToString();
        }

    }

}
