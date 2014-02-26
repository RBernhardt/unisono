using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ComponentModel;

namespace com.newsarea.search.utils {
    
    public abstract class BackgroundWorkerItem {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public event ProgressChangedEventHandler ProgressChanged;
        public event EventHandler Alive;
        protected event EventHandler CancellationPending;
        
        private AutoResetEvent _arEvtCanceled = new AutoResetEvent(true);

        private bool _isCancellationPending = false;
        public bool IsCancellationPending {
            get { 
                return this._isCancellationPending; 
            }
        }

        private bool _isRunning = false;
        public bool IsRunning {
            get { return this._isRunning; }
        }

        public abstract void startWorker(Object[] parameters);

        public void run(Object[] parameters) {
            this._isCancellationPending = false;
            this._isRunning = true;
            this.startWorker(parameters);
            this._isRunning = false;
            if (this.IsCancellationPending) {
                this._arEvtCanceled.Set();
            }
        }

        public void cancelASync() {
            if (!IsRunning) { return; }
            //
            this._isCancellationPending = true;
            //
            if (this.CancellationPending != null) {
                this.CancellationPending(this, new EventArgs());
            }
        }

        public void cancel(int timeout) {
            if (!IsRunning) { return; }
            //
            this._arEvtCanceled.Reset();
            //
            this.cancelASync();
            //
            if (!this._arEvtCanceled.WaitOne(timeout)) {
                throw new TimeoutException();
            }
        }

        public void cancel() {
            this.cancel(10000);
        }

        protected void OnAlive(Object sender, EventArgs e) {
            if (Alive != null) {
                Alive(sender, e);
            }
        }

        protected void OnProgressChanged(double percentage, String message) {
            if (ProgressChanged != null) {                  
                ProgressChanged(this, new ProgressChangedEventArgs((int)Math.Round(percentage), message));
            }
        }

        protected void OnProgressChanged(double percentage) {
            this.OnProgressChanged(percentage, null);
        }

    }

}
