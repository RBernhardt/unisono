using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace com.newsarea.search.utils {
    
    public class BackgroundWorker {

        private const int INTERVAL = 10;

        public event EventHandler IntervalTick;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private AutoResetEvent _evnt = new AutoResetEvent(false);
        //
        private Thread _thread = null;
        private bool _abort = false;
        private bool _cancel = false;

        private BackgroundWorkerItem _item = null;

        private ApartmentState _apartmentState = ApartmentState.Unknown;
        public ApartmentState ApartmentState {
            get { return _apartmentState; }
            set { this._apartmentState = value; }
        }

        private ThreadPriority _tPriority = ThreadPriority.Normal;
        public ThreadPriority ThreadPriority {
            get { return this._tPriority; }
            set { this._tPriority = value; }
        }

        public bool IsRunning {
            get {
                if (this._thread != null) {
                    return this._thread.IsAlive;
                }
                return false;
            }            
        }

        /// <summary>
        /// Führt die Methode mittels Delegate und übergebenen Parametern, die in der festgesetzen Zeit ausgeführt wurde.
        /// </summary>
        /// <param name="d">Auszuführendes Delegate</param>
        /// <param name="parameters">Zu übergebende Paramter für das Delegate</param>
        /// <param name="timeout">Zu erwartende Höchstzeit, bevor die Ausführung des Delegates abgebrochen wird</param>
        /// <returns>True, wenn die Ausführung des Delegates vor dem Timeout zu Ende gegangen ist. False wenn das Timeout überschritten wurde.</returns>
        public bool run(BackgroundWorkerItem item, object[] parameters, TimeSpan timeout) {
            this._item = item;
            //
            bool resetTimeout = false;
            //
            Worker w = new Worker(item, parameters, this._evnt);
            w.Alive += delegate(Object sender, EventArgs e) {                
                resetTimeout = true;
            };
            this._thread = new Thread(new ThreadStart(w.run));

            // init
            this._abort = false;
            this._cancel = false;            

            this._evnt.Reset();
            
            this._thread.Priority = this.ThreadPriority;
            this._thread.SetApartmentState(this.ApartmentState);
            this._thread.Start();
            //
            for (int i = 0; true; i++) {
                bool waitOne = this._evnt.WaitOne(INTERVAL, false);
                if (waitOne) {
                    this._thread = null;
                    //
                    if (this._cancel) {
                        log.Debug("run - cancel completed");
                        return false;
                    }
                    //
                    if (this._abort) {
                        log.Debug("run - abort completed");
                        return false;
                    }
                    //
                    return true; 
                }
                //
                // fire interval tick event
                if (this.IntervalTick != null) {
                    this.IntervalTick(this, new EventArgs());
                }
                // check for reset timeout
                if (resetTimeout) {
                    resetTimeout = false;
                    i = -1;
                }
                // check abort or timeout
                if ((timeout.TotalMilliseconds / INTERVAL) == i) {
                    this._thread.Abort();
                    this._thread = null;
                    //
                    throw new TimeoutException();
                }
            }
        }

        public void cancelASync() {
            if (this._thread == null) { return; }
            //            
            this._cancel = true;
            this._item.cancelASync();
        }

        public void cancel() {
            if (this._thread == null) { return; }
            //
            log.Debug("cancel - init - " + this._item.ToString());
            this._cancel = true;
            try {
                this._item.cancel();
            } catch (TimeoutException) {
                log.Debug("cancel - timeout - " + this._item.ToString());
                return;
            }
            log.Debug("cancel - completed - " + this._item.ToString());
        }

        public void abort() {
            if (this._thread == null) { return; }
            //
            this._abort = true;
            this._thread.Abort();
            this._evnt.Set();
            //
            log.Debug("abort - completed");
        }

        #region Worker Klasse

        private class Worker {

            public event EventHandler Alive;

            private AutoResetEvent _evnt;
            public BackgroundWorkerItem _item;
            public Object[] _parameters;

            public Worker(BackgroundWorkerItem item, object[] parameters, AutoResetEvent evnt) {
                this._item = item;
                this._parameters = parameters;
                this._evnt = evnt;
            }

            public void run() {
                EventHandler aliveEvntHdl = delegate(Object sender, EventArgs e) {
                    if (this.Alive != null) {
                        this.Alive(sender, e);
                    }
                };
                this._item.Alive += aliveEvntHdl;
                this._item.run(this._parameters);
                this._item.Alive -= aliveEvntHdl;
                this._evnt.Set();
            }

        }

        #endregion

    }
}
