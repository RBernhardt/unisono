using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Drawing;
using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;
using com.newsarea.search.utils;
using System.ComponentModel;
using System.Collections.Specialized;
using com.newsarea.search.settings;

namespace com.newsarea.search.provider {
    
    public class Providers {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        //

        #region Events

        public delegate void ProviderEventHandler(Object sender, ProviderEventArgs e);        
        public event ProviderEventHandler ProviderAvailabilityChanged;
        public event EventHandler ProviderLocatingCompleted;

        public delegate void SearchItemHandler(Object sender, SearchResultItem item);
        public event ProviderEventHandler SearchInitialized;
        public event SearchItemHandler SearchItemFound;
        public event SearchItemHandler SearchItemChanged;
        public event ProgressChangedEventHandler SearchProgressChanged;
        public event EventHandler SearchTimeout;
        public event ProviderEventHandler SearchCompleted;

        #endregion

        #region Properties

        private ProviderLocatorAndAvailabilityWorker _providerLocatorAndAvailabilityWorker = null;
        private SearchWorker _searchWorker = null;
        private Thread _searchWorkerThread = null;

        private List<IProvider> _availableProviders = new List<IProvider>();
        public List<IProvider> AvailableProviders {
            get { return this._availableProviders; }
        }

        private Settings _applicationSettings = null;
        private Settings ApplicationSettings {
            get { return this._applicationSettings; }
        }

        #endregion

        public Providers(Settings applicationSettings) {
            this._applicationSettings = applicationSettings;
        }
        
        public void init() {         
            /* ----------------------------
             *      LOCATE PROVIDER
             * ------------------------- */
            _providerLocatorAndAvailabilityWorker = new ProviderLocatorAndAvailabilityWorker(this.ApplicationSettings.ProviderDirectories);
            // append provider and forward provider located event
            _providerLocatorAndAvailabilityWorker.Located += delegate(Object sender, IProvider provider) {
                log.Debug("provider located - " + provider.ToString());
                this.appendProvider(provider);
                //                
                if (this.ProviderAvailabilityChanged != null) {
                    this.ProviderAvailabilityChanged(sender, new ProviderEventArgs(provider.ToString(), provider.getImage(), provider.IsAvailable));
                }
            };
            // forward provider location completed event
            _providerLocatorAndAvailabilityWorker.LocatingCompleted += delegate(Object sender, EventArgs e) {
                log.Debug("provider locating completed");
                if (this.ProviderLocatingCompleted != null) {
                    this.ProviderLocatingCompleted(sender, e);
                }
                //
            };
            // forward availability changed
            _providerLocatorAndAvailabilityWorker.AvailabilityChanged += delegate(Object sender, ProviderEventArgs e) {
                log.Debug("provider availability changed - " + e.Name);
                if (this.ProviderAvailabilityChanged != null) {
                    this.ProviderAvailabilityChanged(sender, e);
                }
            };
            //
            Thread pInitThread = new Thread(new ThreadStart(_providerLocatorAndAvailabilityWorker.start));
            pInitThread.Priority = ThreadPriority.Lowest;
            pInitThread.Start();            
            /* ----------------------------
             *      SEARCH WORKER
             * ------------------------- */
            this._searchWorker = new SearchWorker();
            //
            this._searchWorker.Initialized += delegate(object sender, ProviderEventArgs e) {
                if (this.SearchInitialized != null) {
                    this.SearchInitialized(sender, e);
                }
            };
            //
            this._searchWorker.ItemFound += delegate(object sender, SearchResultItem item) {
                if (this.SearchItemFound != null) {
                    this.SearchItemFound(sender, item);
                }
            };
            //
            this._searchWorker.ItemChanged += delegate(object sender, SearchResultItem item) {
                if (this.SearchItemChanged != null) {
                    this.SearchItemChanged(sender, item);
                }
            };
            //
            this._searchWorker.ProgressChanged += delegate(object sender, ProgressChangedEventArgs e) {
                if (this.SearchProgressChanged != null) {
                    this.SearchProgressChanged(sender, e);
                }
            };
            //
            this._searchWorker.Timeout += delegate(object sender, EventArgs e) {
                if (this.SearchTimeout != null) {
                    this.SearchTimeout(sender, e);
                }
            };
            // 
            this._searchWorker.Completed += delegate(object sender, ProviderEventArgs e) {
                if (this.SearchCompleted != null) {
                    this.SearchCompleted(sender, e);
                }
            };
            // start search thread
            this._searchWorkerThread = new Thread(new ThreadStart(this._searchWorker.start));
            this._searchWorkerThread.Start();
        }

        public void dispose() {
            /* DISPOSE PROVIDER WORKER */
            this._providerLocatorAndAvailabilityWorker.exit();
            /* DISPOSE SEARCH WORKER */
            this._searchWorker.exit();
            //
            this.AvailableProviders.Clear();
        }

        public bool search(String searchString) {
            SearchContext sContext = this.getSearchContext(searchString);
            if (sContext.provider == null) { return true; }
            //
            return this._searchWorker.search(searchString, sContext.value, sContext.provider);
        }

        public void cancelSearchASync() {
            this._searchWorker.cancelASync();
        }

        public void handleInput(List<SearchResultItem> items, String searchValue) {
            log.Debug("handleInput " + searchValue);
            SearchContext sContext = this.getSearchContext(searchValue);
            if (sContext.provider == null) { return; }
            //
            sContext.provider.handleInput(items, sContext.value);
        }

        public void handleSelection(SearchResultItem item, String searchValue) {            
            SearchContext sContext = this.getSearchContext(searchValue);
            if (sContext.provider == null) { return; }
            log.Debug("handleSelection - " + searchValue + " with " + sContext.provider.ToString());
            sContext.provider.handleSelection(item);
        }

        #region Thread Classes

        private class ProviderLocatorAndAvailabilityWorker {

            public delegate void LocatedEventHandler(Object sender, IProvider provider);
            /* */
            public event LocatedEventHandler Located;
            public event EventHandler LocatingCompleted;            
            public event ProviderEventHandler AvailabilityChanged;
            public event EventHandler Exit;
            /* */
            private List<DirectoryInfo> _providerDirectories = null;
            /* */
            private Dictionary<IProvider, bool> _lastStates = new Dictionary<IProvider, bool>();
            private bool _exit = false;

            public ProviderLocatorAndAvailabilityWorker(List<DirectoryInfo> providerDirectories) {
                this._providerDirectories = providerDirectories;
            }

            public void start() {
                List<IProvider> providers = new List<IProvider>();
                /* ---------------------------
                 *    LOCATE PROVIDERS
                 * ------------------------ */
                foreach (DirectoryInfo dInfo in this._providerDirectories) {
                    if (!dInfo.Exists) { continue; }
                    foreach (IProvider provider in this.getProviders(dInfo, dInfo)) {
                        providers.Add(provider);
                        if (this.Located != null) {
                            this.Located(this, provider);
                        }
                    }
                }
                //
                if (this.LocatingCompleted != null) {
                    this.LocatingCompleted(this, new EventArgs());
                }
                /* ---------------------------
                 *    CHECK AVAILABILITY
                 * ------------------------ */
                log.Debug("start loop - check availability");
                while (!this._exit) {
                    foreach (IProvider provider in providers.ToList()) {
                        bool currentState = provider.IsAvailable;
                        //
                        if (!this._lastStates.ContainsKey(provider)) {
                            this._lastStates.Add(provider, !currentState);
                        } else {
                            if (!currentState) {
                                try {
                                    log.Debug("try to init provider - " + provider);
                                    provider.init();
                                    currentState = provider.IsAvailable;
                                } catch (ArgumentException aEx) {
                                    currentState = false;
                                    log.Debug(aEx.Message, aEx);
                                } catch (Exception ex) {
                                    currentState = false;
                                    log.Error(ex.Message, ex);
                                }
                            }
                        }
                        //
                        bool lastState = this._lastStates[provider];
                        // check changes
                        if (lastState == currentState) { continue; }
                        //
                        Image img = provider.getImage();
                        if (this.AvailabilityChanged != null) {
                            this.AvailabilityChanged(this, new ProviderEventArgs(provider.ToString(), img, currentState));
                        }
                        // set new state
                        this._lastStates[provider] = currentState;
                    }
                    //
                    Thread.Sleep(5000);
                }
                // fire exit event
                if (this.Exit != null) {
                    this.Exit(this, new EventArgs());
                }
            }

            private Assembly getAssembly(String name, DirectoryInfo dInfo) {
                FileInfo[] files = dInfo.GetFiles(name);
                if (files.Length == 1) {
                    return Assembly.LoadFile(files[0].FullName);
                }
                //
                foreach (DirectoryInfo childDInfo in dInfo.GetDirectories()) {
                    Assembly asm = this.getAssembly(name, childDInfo);
                    if (asm != null) { return asm; }
                }
                //
                return null;
            }

            private List<IProvider> getProviders(DirectoryInfo dInfo, DirectoryInfo asmSearchDirectory) {
                List<IProvider> providers = new List<IProvider>();
                foreach (FileInfo fInfo in dInfo.GetFiles("*.dll")) {
                    log.Debug("found " + fInfo.Name);
                    List<IProvider> cproviders = this.getProviders(fInfo, asmSearchDirectory);
                    foreach (IProvider cprovider in cproviders) {
                        providers.Add(cprovider);
                    }
                }
                //
                foreach (DirectoryInfo childDInfo in dInfo.GetDirectories()) {
                    foreach (IProvider childHandler in this.getProviders(childDInfo, asmSearchDirectory)) {
                        providers.Add(childHandler);
                    }
                }
                //
                return providers;
            }

            private List<IProvider> getProviders(FileInfo fInfo, DirectoryInfo asmSearchDirectory) {
                List<IProvider> providers = new List<IProvider>();
                //
                AppDomain.CurrentDomain.AssemblyResolve += delegate(object sender, ResolveEventArgs args) {
                    int sIdx = args.Name.IndexOf(",");
                    String filename = args.Name.Substring(0, sIdx) + ".dll";
                    log.Debug("try to load required asm - " + filename);
                    Assembly asm = this.getAssembly(filename, asmSearchDirectory);
                    if (asm == null) {
                        log.Error("can't load " + asm.ToString());
                    }
                    return asm;
                };
                //
                log.Debug("try to load asm from dll - " + fInfo.Name);
                //
                try {
                    Assembly asm = Assembly.LoadFile(fInfo.FullName);
                    log.Debug("asm loaded from file - " + asm.FullName);
                    foreach (Type type in asm.GetTypes()) {
                        log.Debug("asm contains type - " + type.ToString());
                        foreach (Type iType in type.GetInterfaces()) {
                            if (iType == typeof(IProvider)) {
                                IProvider provider = (IProvider)asm.CreateInstance(type.ToString());
                                log.Debug("found IProvider implementation - " + provider);
                                providers.Add(provider);
                            }
                        }
                    }
                } catch (Exception ex) {
                    log.Error("unable to load asm from dll - " + fInfo.FullName, ex);

                }
                //
                return providers;
            }

            public void exit() {
                this._exit = true;
            }

        }

        private class SearchWorker {

            public event ProviderEventHandler Initialized;
            public event SearchItemHandler ItemFound;
            public event SearchItemHandler ItemChanged;
            public event ProgressChangedEventHandler ProgressChanged;
            public event EventHandler Timeout;
            public event ProviderEventHandler Completed;
            public event EventHandler Exit;

            utils.BackgroundWorker _bWorker = new utils.BackgroundWorker();

            private static Mutex searchMTex = new Mutex();
            private AutoResetEvent arEvent = new AutoResetEvent(true);

            /* */

            private IProvider _provider = null;
            public IProvider Provider {
                get { return this._provider; }
            }
            //
            private String _searchString = null;
            public String SearchString {
                get { return this._searchString; }
            }

            private String _searchValue = null;
            public String SearchValue {
                get { return this._searchValue; }
            }

            private bool _exit = false;

            public SearchWorker() {
                arEvent.Reset();
            }

            public void start() {
                //
                // START WORKER WHILE
                while (!this._exit) {
                    log.Debug("SearchWorker - wait for search input");
                    //
                    arEvent.WaitOne();
                    if (this._exit) { continue; }
                    searchMTex.WaitOne();
                    //
                    try {     
                        if (this.Initialized != null) {
                            log.Debug("fire Initialized Event - " + this.SearchValue);
                            this.Initialized(this, new ProviderEventArgs(this.Provider.ToString(), this.Provider.getImage(), this.Provider.IsAvailable));
                        }
                        //
                        SearchItemHandler sItemHdlItemFound = delegate(Object sender, SearchResultItem item) {
                            if (ItemFound != null) {
                                ItemFound(sender, item);
                            }
                        };
                        this.Provider.ItemFound += sItemHdlItemFound;
                        //
                        SearchItemHandler sItemHdlItemChanged = delegate(Object sender, SearchResultItem item) {
                            if (ItemChanged != null) {
                                ItemChanged(sender, item);
                            }
                        };
                        this.Provider.ItemChanged += sItemHdlItemChanged;
                        //
                        ProgressChangedEventHandler sItemHdlProgressChanged = delegate(Object sender, ProgressChangedEventArgs item) {
                            if (ProgressChanged != null) {
                                ProgressChanged(sender, item);
                            }
                        };
                        ((BackgroundWorkerItem)this.Provider).ProgressChanged += sItemHdlProgressChanged;
                        //
                        bool normalExecution = false;
                        try {
                            log.Debug("call doSearch - " + this._searchValue);
                            normalExecution = this._bWorker.run((BackgroundWorkerItem)this.Provider, new Object[] { this.SearchValue }, new TimeSpan(0, 0, 20));
                            log.Debug("exit doSearch - " + this.SearchValue + " - " + normalExecution);
                        } catch(TimeoutException tEx) {
                            if (this.Timeout != null) {
                                log.Debug("fire Timeout Event - " + this.SearchValue + " - " + tEx.Message);
                                this.Timeout(this, new EventArgs());                                
                            }
                        }
                        //
                        this.Provider.ItemFound -= sItemHdlItemFound;
                        this.Provider.ItemChanged -= sItemHdlItemChanged;
                        ((BackgroundWorkerItem)this.Provider).ProgressChanged -= sItemHdlProgressChanged;
                        //
                        // fire completed event
                        if (normalExecution && this.Completed != null) {
                            log.Debug("fire Completed Event - " + this.SearchValue);
                            this.Completed(this, new ProviderEventArgs(this.Provider.ToString(), this.Provider.getImage(), this.Provider.IsAvailable));
                        }
                    } catch (Exception ex) {
                        log.Error(ex);
                    }
                    //
                    searchMTex.ReleaseMutex();
                    arEvent.Reset();
                }
                // fire exit event
                if (this.Exit != null) {
                    this.Exit(this, new EventArgs());
                }
            }
            
            public bool search(String searchString, String value, IProvider provider) {
                if (this._bWorker.IsRunning) {
                    return false;
                }
                //
                searchMTex.WaitOne();
                this._provider = provider;
                this._searchString = searchString;
                this._searchValue = value;                
                searchMTex.ReleaseMutex();
                //                
                arEvent.Set();
                //
                return true;
            }

            public void cancelASync() {
                log.Debug("cancelASync - try to cancel");
                if (this._bWorker.IsRunning) {
                    this._bWorker.cancelASync();
                }
            }

            public void exit() {
                this.cancelASync();
                this._exit = true;   
                //
                arEvent.Set();
            }

        }

        #endregion

        #region Helper

        private struct SearchContext {
            public IProvider provider;
            public String code;
            public String value;
        }

        private SearchContext getSearchContext(String searchValue) {
            SearchContext context = new SearchContext();
            context.provider = null;
            context.code = null;
            context.value = searchValue;
            // get searchtype and value
            Match regMatch = Regex.Match(context.value, "(?i)(?:([a-z]{1,2})\\s+)?(.*)");
            if (regMatch.Groups[1] != null) {
                context.code = regMatch.Groups[1].Value;
                context.value = regMatch.Groups[2].Value;
            }
            // get provider code
            if (this.ApplicationSettings.IdentCodes.ContainsKey(context.code)) {
                String providerFullname = this.ApplicationSettings.IdentCodes[context.code];
                context.provider = this.getProvider(providerFullname);
            }
            //
            return context;
        }

        private void appendProvider(IProvider newProvider) {
            //
            // get available provider
            IProvider currentProvider = null;
            foreach (IProvider provider in this.AvailableProviders) {
                if (String.Compare(newProvider.GetType().ToString(), provider.GetType().ToString()) == 0) {
                    currentProvider = provider;
                }
            }
            //
            // remove current provider
            if (currentProvider != null) {
                this.AvailableProviders.Remove(currentProvider);
                // check date of providers
                FileInfo currentFileInfo = new FileInfo(currentProvider.GetType().Assembly.Location);
                FileInfo newFileInfo = new FileInfo(newProvider.GetType().Assembly.Location);
                if (currentFileInfo.CreationTime.CompareTo(newFileInfo.CreationTime) > 0) {
                    newProvider = currentProvider;
                }
            }
            //
            newProvider.ApplicationSettings = this.ApplicationSettings;
            newProvider.Settings = this.ApplicationSettings.getNode(new String[] { "providersettings", newProvider.GetType().ToString() });
            //
            // append provider
            this.AvailableProviders.Add(newProvider);
        }

        private IProvider getProvider(String fullname) {
            foreach (IProvider provider in this.AvailableProviders) {
                if (String.Compare(fullname, provider.ToString(), false) == 0) {
                    return provider;
                }
            }
            return null;
        }

        #endregion

    }
}
