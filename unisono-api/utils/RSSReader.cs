using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.ComponentModel;

namespace com.newsarea.search.utils {
    
    public class RSSReader {

        public delegate void ItemFoundHandler(Object sender, Item item);

        public class Item {
            public String title = null;
            public String description = null;
            public Uri link = null;
            public Uri imageUrl = null;
        }

        public event ProgressChangedEventHandler ProgressChanged;
        public event ItemFoundHandler ItemFound;

        private String _url = null;
        private bool _cancel = false;

        public RSSReader(String url) {
            this._url = url;
        }

        public void read() {
            WebClient wClient = new WebClient();
            byte[] resultData = wClient.DownloadData(this._url);
            String input = System.Text.Encoding.UTF8.GetString(resultData);
            MatchCollection itemCol = Regex.Matches(input, "(?i)(?s)<item>(.*?)</item>");
            //
            double itemFactor = 1.0 * 100 / itemCol.Count;
            double progressValue = 0;
            //
            foreach (Match itemMatch in itemCol) {
                if (this._cancel) { return; }
                //
                String itemString = HttpUtility.UrlDecode(itemMatch.Groups[1].Value);
                Item item = new Item();
                //
                item.title = Regex.Match(itemString, "(?i)(?s)<title>(.*?)</title>").Groups[1].Value;
                item.description = Regex.Match(itemString, "(?i)(?s)<description>(.*?)</description>").Groups[1].Value;
                item.link = new Uri(Regex.Match(itemString, "(?i)(?s)<link>(.*?)</link>").Groups[1].Value, UriKind.Absolute);
                Group imageUrlGroup = Regex.Match(itemString, "(?i)(?s)<enclosure(?:.*?)url=\"(.*?)\"(?:.*?)/>").Groups[1];
                if (imageUrlGroup != null) {
                    try {
                        item.imageUrl = new Uri(imageUrlGroup.Value, UriKind.Absolute);
                    } catch (Exception) { }
                }
                
                //
                if (ItemFound != null) {
                    ItemFound(this, item);
                }
                //
                progressValue += itemFactor;
                if (ProgressChanged != null) {                    
                    ProgressChanged(this, new ProgressChangedEventArgs((int)Math.Round(progressValue), null));
                }
            }
        }

        public void cancel() {
            this._cancel = true;
        }

    }
}
