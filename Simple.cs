﻿using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace Benchmark {
    public class SimpleAsyncResult : IAsyncResult {
        object _state;


        public bool IsCompleted { get; set; }


        public WaitHandle AsyncWaitHandle { get; internal set; }


        public object AsyncState {
            get {
                if (Exception != null) {
                    throw Exception;
                }
                return _state;
            }
            internal set {
                _state = value;
            }
        }


        public bool CompletedSynchronously { get { return IsCompleted; } }


        internal Exception Exception { get; set; }
    }

    public class SimpleSyncObject : ISynchronizeInvoke {
        private readonly object _sync;


        public SimpleSyncObject() {
            _sync = new object();
        }


        public IAsyncResult BeginInvoke(Delegate method, object[] args) {
            var result = new SimpleAsyncResult();


            ThreadPool.QueueUserWorkItem(delegate {
                result.AsyncWaitHandle = new ManualResetEvent(false);
                try {
                    result.AsyncState = Invoke(method, args);
                } catch (Exception exception) {
                    result.Exception = exception;
                }
                result.IsCompleted = true;
            });


            return result;
        }


        public object EndInvoke(IAsyncResult result) {
            if (!result.IsCompleted) {
                result.AsyncWaitHandle.WaitOne();
            }


            return result.AsyncState;
        }


        public object Invoke(Delegate method, object[] args) {
            lock (_sync) {
                return method.DynamicInvoke(args);
            }
        }


        public bool InvokeRequired {
            get { return true; }
        }
    }
}
