using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace com.newsarea.search.utils {
    
    public class MethodStackHandler {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private List<Worker> _workers = new List<Worker>();

        private class StackItem {
            public Delegate method = null;
            public Object[] parameters = null;
            public TimeSpan timeout = TimeSpan.MaxValue;
            public ApartmentState apartmentState = ApartmentState.Unknown;
            public ThreadPriority priority = ThreadPriority.Normal;
        }

        List<StackItem> _methodStack = new List<StackItem>();

        public void append(Delegate method, Object[] parameters, TimeSpan timeout, ApartmentState apartmentState, ThreadPriority priority) {
            StackItem sItem = new StackItem();
            sItem.method = method;
            sItem.parameters = parameters;
            sItem.timeout = timeout;
            sItem.apartmentState = apartmentState;
            sItem.priority = priority;
            //
            this._methodStack.Add(sItem);
        }

        public void append(Delegate method, Object[] parameters, TimeSpan timeout) {
            this.append(method, parameters, timeout, ApartmentState.Unknown, ThreadPriority.Normal);
        }

        public bool run() {
            AutoResetEvent arEvent = new AutoResetEvent(true);
            arEvent.Reset();
            //
            int counter = this._methodStack.Count;
            //
            foreach (StackItem item in this._methodStack) {
                Worker wrk = new Worker(item);
                wrk.Completed += delegate(Object sender, EventArgs e) {
                    counter--;
                    if (counter == 0) {
                        arEvent.Set();
                    }
                };
                //
                Thread t = new Thread(new ThreadStart(wrk.run));                
                t.Start();
                //
                _workers.Add(wrk);
            }
            //
            return arEvent.WaitOne();
        }

        public void abort() {
            foreach (Worker worker in this._workers) {
                worker.abort();
            }
        }

        #region "Worker"

        private class Worker {

            public event EventHandler Completed;

            private StackItem _stackItem = null;
            MethodTimeoutHandler mtHdl = null;

            public Worker(StackItem stackItem) {
                this._stackItem = stackItem;
            }

            public void run() {
                bool correctException = false;
                //
                mtHdl = new MethodTimeoutHandler();
                mtHdl.ApartmentState = this._stackItem.apartmentState;
                mtHdl.ThreadPriority = this._stackItem.priority;
                try {
                   correctException = mtHdl.run(this._stackItem.method, this._stackItem.parameters, this._stackItem.timeout);                
                } catch (TimeoutException) {
                    log.Debug("timeout - " + this._stackItem.method.ToString());
                }
                //
                mtHdl = null;
                //
                if (this.Completed != null) {
                    this.Completed(this, new EventArgs());
                }
            }

            public void abort() {
                if (mtHdl != null) {
                    mtHdl.abort();
                }
            }

        }

        #endregion

    }

}
