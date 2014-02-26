using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace com.newsarea.search.utils {
    /// <summary>
    /// Die Ausführungszeit einer Methode kontrollieren.
    /// </summary>
    public class MethodTimeoutHandler {

        private const int INTERVAL = 100;

        public event EventHandler IntervalTick;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private AutoResetEvent evnt = new AutoResetEvent(false);
        private Thread _thread = null;
        private bool _resetTimeout = false;
        private bool _abort = false;

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

        public delegate void RunMethodDelegate();

        /// <summary>
        /// Führt die Methode aus, die in einer festgesetzen Zeit erfolgen soll.
        /// </summary>
        /// <param name="runMethod">Methode zum ausführen</param>
        /// <param name="timeout">Zu erwartende Höchstzeit, bevor die Ausführung der Methode abgebrochen wird</param>
        /// <returns>True, wenn die Ausführung der Methode vor dem Timeout zu Ende gegangen ist. False wenn das Timeout überschritten wurde.</returns>
        public bool run(Delegate runMethod, TimeSpan timeout) {
            return this.run(runMethod, null, timeout);
        }

        /// <summary>
        /// Führt die Methode aus, die in einer festgesetzten Zeit erfolgen soll und übergibt die für sie bestimmte Parameter.
        /// </summary>
        /// <param name="runMethod">Methode zum ausführen</param>
        /// <param name="parameters">Parametertabelle</param>
        /// <param name="timeout">Zu erwartende Höchstzeit, bevor die Ausführung der Methode abgebrochen wird</param>
        /// <returns>True, wenn die Ausführung der Methode vor dem Timeout zu Ende gegangen ist. False wenn das Timeout überschritten wurde.</returns>
        public bool run(Delegate runMethod, object[] parameters, TimeSpan timeout) {
            return this.runImp(runMethod, parameters, timeout);
        }
                
        public void resetTimeout() {
            this._resetTimeout = true;
        }

        public void abort() {
            if (this._thread != null) {
                log.Debug("try to abort");
                this._abort = true;
            }
        }

        /// <summary>
        /// Führt die Methode mittels Delegate und übergebenen Parametern, die in der festgesetzen Zeit ausgeführt wurde.
        /// </summary>
        /// <param name="d">Auszuführendes Delegate</param>
        /// <param name="parameters">Zu übergebende Paramter für das Delegate</param>
        /// <param name="timeout">Zu erwartende Höchstzeit, bevor die Ausführung des Delegates abgebrochen wird</param>
        /// <returns>True, wenn die Ausführung des Delegates vor dem Timeout zu Ende gegangen ist. False wenn das Timeout überschritten wurde.</returns>
        private bool runImp(Delegate d, object[] parameters, TimeSpan timeout) {
            Worker w = new Worker(d, parameters, this.evnt);
            this._thread = new Thread(new ThreadStart(w.Run));            
            
            // init
            this._abort = false;
            this._resetTimeout = false;

            evnt.Reset();
            //
            this._thread.Priority = this.ThreadPriority;
            this._thread.SetApartmentState(this.ApartmentState);
            this._thread.Start();               
            //
            for(int i=0; true; i++) {
                bool waitOne = evnt.WaitOne(INTERVAL, false);                
                if (waitOne) { return true; }                
                //
                // fire interval tick event
                if (this.IntervalTick != null) {
                    this.IntervalTick(this, new EventArgs());
                }                
                // check for reset timeout
                if (this._resetTimeout) {
                    this._resetTimeout = false;
                    i = -1;
                }
                // check abort or timeout
                if (this._abort || (timeout.TotalMilliseconds / INTERVAL) == i) {                    
                    this._thread.Abort();
                    //
                    if (this._abort) {
                        log.Debug("runImp - abort - " + d.ToString());
                        return false;
                    } else {
                        log.Debug("runImp - timeout - " + d.ToString());
                    }
                    //
                    throw new TimeoutException();
                }
            }
        }

        #region Worker Klasse

        private class Worker {

            private AutoResetEvent evnt;
            public Delegate method;
            public Object[] parameters;

            public Worker(Delegate method, object[] parameters, AutoResetEvent evnt) {
                this.method = method;
                this.parameters = parameters;
                this.evnt = evnt;
            }

            public void Run() {
                this.method.DynamicInvoke(parameters);
                evnt.Set();
            }
        }

        #endregion
    }

}
