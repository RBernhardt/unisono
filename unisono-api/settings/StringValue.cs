using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.newsarea.search.settings {

    public class StringValue : Values {

        private String _name = null;
        public String Name {
            get { return this._name; }
        }

        private String _value = null;
        public String Value {
            get { return this._value; }
            set { this._value = value; }
        }

        public StringValue(String name, String value) {
            this._name = name;
            this._value = value;
        }

        private String ToString(String spacer) {
            StringBuilder strBld = new StringBuilder();
            strBld.Append(spacer);
            strBld.Append(this.Name);
            strBld.Append(" - ");
            strBld.Append(this.Value);
            strBld.Append(Environment.NewLine);
            foreach (KeyValuePair<String, StringValue> kvPair in this) {
                strBld.Append(kvPair.Value.ToString(spacer + " "));
            }
            return strBld.ToString();
        }

        public override String ToString() {
            return this.ToString(String.Empty);
        }

    }

}
