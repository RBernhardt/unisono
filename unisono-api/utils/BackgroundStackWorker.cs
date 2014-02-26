using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace com.newsarea.search.utils {
    
    public class BackgroundStackWorker {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private List<Worker> _workers = new List<Worker>();

        private class StackItem {
            public BackgroundWorkerItem bWItem = null;
            public Object[] parameters = null;
            public TimeSpan timeout = TimeSpan.MaxValue;
            public ApartmentState apartmentState = ApartmentState.Unknown;
            public ThreadPriority priority = ThreadPriority.Normal;
        }

        List<StackItem> _methodStack = new List<StackItem>();

        public void append(BackgroundWorkerItem item, Object[] parameters, TimeSpan timeout, ApartmentState apartmentState, ThreadPriority priority) {
            StackItem sItem = new StackItem();
            sItem.bWItem = item;
            sItem.parameters = parameters;
            sItem.timeout = timeout;
            sItem.apartmentState = apartmentState;
            sItem.priority = priority;
            //
            this._methodStack.Add(sItem);
        }

        public void append(BackgroundWorkerItem item, Object[] parameters, TimeSpan timeout) {
            this.append(item, parameters, timeout, ApartmentState.Unknown, ThreadPriority.Normal);
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

        public void cancelASync() {
            //log.Debug("cancel - init");
            foreach (Worker worker in this._workers) {
                worker.cancelASync();
            }
            //log.Debug("cancel - completed");
        }

        #region "Worker"

        private class Worker {

            public event EventHandler Completed;

            private StackItem _stackItem = null;
            BackgroundWorker _bWorker = null;

            public Worker(StackItem stackItem) {
                this._stackItem = stackItem;
            }

            public void run() {
                bool correctException = false;
                //
                this._bWorker = new BackgroundWorker();
                this._bWorker.ApartmentState = this._stackItem.apartmentState;
                this._bWorker.ThreadPriority = this._stackItem.priority;
                try {
                    correctException = this._bWorker.run(this._stackItem.bWItem, this._stackItem.parameters, this._stackItem.timeout);
                } catch (TimeoutException) {
                    log.Debug("timeout - " + this._stackItem.bWItem.ToString());
                }
                //
                this._bWorker = null;
                //
                if (this.Completed != null) {
                    this.Completed(this, new EventArgs());
                }
            }

            public void cancelASync() {
                if (this._bWorker != null) {
                    this._bWorker.cancelASync();
                }
            }

        }

        #endregion
    
    }

}
