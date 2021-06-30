using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using SharpDX.Direct3D11;

namespace CaptureCore
{
    internal interface IBackgroundConversionQueue: IDisposable
    {
        void Start();
        Task EnqueueConversionAsync(Texture2D texture);
    }

    internal class BackgroundConversionQueue<T> : IBackgroundConversionQueue
    {
        private readonly Action<T> onResult;
        private readonly Func<Texture2D, T> converter;
        private AutoResetEvent waitHandle;
        private ConcurrentQueue<T> innerQueue;
        private Thread callbackThread;
        public bool AutoDispose { get; set; } = true;


        public BackgroundConversionQueue(Action<T> onResult, Func<Texture2D, T> converter)
        {
            this.onResult = onResult;
            this.converter = converter;
        }

        public void Start()
        {
            Stop();
            innerQueue = new ConcurrentQueue<T>();
            callbackThread = new Thread(CallbackMain);
            // callbackThread.Priority = ThreadPriority.AboveNormal;
            callbackThread.Start();
        }

        public void Stop()
        {
            callbackThread?.Abort();
            callbackThread = null;
            innerQueue = null;
        }


        public void Dispose()
        {
            Stop();
        }

        public async Task EnqueueConversionAsync(Texture2D texture)
        {
            if (onResult != null)
                Enqueue(await Task.Factory.StartNew(() => converter(texture), TaskCreationOptions.LongRunning) );
        }

        public void Enqueue(T item)
        {
            innerQueue?.Enqueue(item);
        }

        private void CallbackMain()
        {
            try
            {

                while (true)
                {
                    if (innerQueue.TryDequeue(out T result))
                    {
                        try
                        {
                            onResult?.Invoke(result);
                        }
                        finally
                        {
                            if (AutoDispose)
                            {
                                var disposable = result as IDisposable;
                                disposable?.Dispose();
                            }
                        }
                    }

                }
            }
            catch (Exception exception)
            {
            }
        }
    }
}