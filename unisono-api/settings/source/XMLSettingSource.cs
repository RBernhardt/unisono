using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.Specialized;
using System.Reflection;
using de.rappcollins.data.xml;

namespace com.newsarea.search.settings.source {

    public class XMLSettingSource : Values, ISettingSource   {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        //
        private FileInfo _xmlFile = null;
        private ISettingSource _defaultSettings = null;
                
        public XMLSettingSource(FileInfo xmlFile, ISettingSource defaultSettings) {
            this._xmlFile = xmlFile;
            this._defaultSettings = defaultSettings;
        }

        public XMLSettingSource(FileInfo xmlFile) : this(xmlFile, null) { }

        public void save() {
            if (!this._xmlFile.Directory.Exists) {
                this._xmlFile.Directory.Create();
            }
            this._xmlFile.Delete();
            //
            FileStream fStream = null;
            try {
                fStream = new FileStream(this._xmlFile.FullName, FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter strWrt = new StreamWriter(fStream);
                //                
                XMLItem rootItem = new XMLItem("settings");
                this.writeXMLItems(rootItem, this, (Values)this._defaultSettings);
                strWrt.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
                strWrt.Write(rootItem.ToString());
                //
                rootItem = null;
                strWrt.Close();
                strWrt = null;
            //} catch(Exception ex) {
            //    throw ex;

            } finally {
                if(fStream != null) {
                    fStream.Close();
                    fStream = null;
                }
            }
        }

        public void reset() {
            base.Clear();
            /*********************************
            *         READ XML FILE 
            *********************************/
            if (this._xmlFile.Exists) {
                FileStream fStream = null;
                try {
                    fStream = new FileStream(this._xmlFile.FullName, FileMode.Open, FileAccess.Read);
                    XMLReader xmlRdr = new XMLReader(fStream);
                    xmlRdr.Refresh();
                    //                
                    this.readXMLItems(xmlRdr, this);
                    //
                    xmlRdr.Clear();
                    xmlRdr = null;
                } catch (Exception ex) {
                    throw ex;

                } finally {
                    if (fStream != null) {
                        fStream.Close();
                        fStream = null;
                    }
                }
            }
            /*********************************
            *   ACCUMULATE WITH DEFAULTS
            *********************************/
            if (this._defaultSettings != null) {
                this.accumulateWithDefaultSettings(this, (Values)this._defaultSettings);
            }
        }

        public void load() {
            this.reset();
        }

        private void readXMLItems(XMLItem rootItem, Values parent) {            
            foreach (XMLItem item in rootItem) {
                StringValue newValue = null;
                //
                String name = item.TagName;
                if (String.Compare(name, "item", true) == 0) {
                    name = rootItem.IndexOf(item).ToString(); 
                }
                //
                if(item.Value != null) {
                    String value = item.Value.ToString();
                    newValue = new StringValue(name, value); 
                }
                //
                this.readXMLItems(item, (Values)newValue);
                //
                parent.Add(name, newValue);
            }
        }

        private void writeXMLItems(XMLItem rootItem, Values userValues, Values defaultValues) {
            foreach (KeyValuePair<String, StringValue> kvValue in userValues) {
                //
                // get the user values
                String userValue = kvValue.Value.Value;
                Values childUserValues = (Values)kvValue.Value;
                //
                // get the default values
                String defaultValue = null;
                Values childDefaultValues = null;
                if (defaultValues != null && defaultValues.ContainsKey(kvValue.Key)) {
                    defaultValue = defaultValues[kvValue.Key].Value;
                    childDefaultValues = (Values)defaultValues[kvValue.Key];
                }
                //
                // compare user and default values
                if (childUserValues.Count == 0 && String.Compare(userValue, defaultValue) == 0) { continue; }
                //
                // write the user values
                String name = kvValue.Key;
                try {
                    int.Parse(kvValue.Key);
                    name = "item";
                } catch (FormatException) { }
                //
                XMLItem xmlItem = new XMLItem(name);
                if (kvValue.Value != null && kvValue.Value.Value != null && kvValue.Value.Value != String.Empty) {
                    xmlItem.Value = new Value(kvValue.Value.Value);
                }
                rootItem.Add(xmlItem);
                //
                this.writeXMLItems(xmlItem, childUserValues, childDefaultValues);
            }
        }

        private void accumulateWithDefaultSettings(Values userValues, Values defaultValues) {
            foreach (KeyValuePair<String, StringValue> kvValue in defaultValues) {
                if (!userValues.ContainsKey(kvValue.Key)) {
                    userValues.Add(kvValue.Key, new StringValue(kvValue.Key, kvValue.Value.Value));                    
                }
                //
                StringValue userValue = userValues[kvValue.Key];
                this.accumulateWithDefaultSettings(userValue, kvValue.Value);
            }
        }

    }

}
