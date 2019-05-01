using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using WorkItem = System.Collections.Generic.KeyValuePair<System.Threading.SendOrPostCallback, object>;

namespace ServiceSeekiosUnitTest.Helper
{
    public static class GeneralThreadAffineContextHelper
    {
        class WorkQueue
        {
            private readonly BlockingCollection<WorkItem> m_queue = new BlockingCollection<WorkItem>();

            internal void Enqueue(WorkItem item)
            {
                try { m_queue.Add(item); }
                catch (InvalidOperationException) { }
            }

            internal void Shutdown() { m_queue.CompleteAdding(); }

            internal void ExecuteWorkQueueLoop()
            {
                foreach (var currentItem in m_queue.GetConsumingEnumerable())
                {
                    currentItem.Key.Invoke(currentItem.Value);
                }
            }
        }

        class Context : SynchronizationContext
        {
            internal readonly WorkQueue WorkQueue;

            internal Context() : this(new WorkQueue()) { }

            protected Context(WorkQueue queue)
            {
                this.WorkQueue = queue;
            }

            public override void Post(SendOrPostCallback callback, object state)
            {
                WorkQueue.Enqueue(new WorkItem(callback, state));
            }

            public override SynchronizationContext CreateCopy()
            {
                return new Context(WorkQueue);
            }

            public override void Send(SendOrPostCallback d, object state)
            {
                throw new NotImplementedException();
            }
        }

        public static Task<TResult> Run<TResult>(Func<Task<TResult>> asyncMethod)
        {
            return (Task<TResult>)Run((Func<Task>)asyncMethod);
        }

        public static Task Run(Func<Task> asyncMethod)
        {
            using (SynchronizationContextSwitcher.Capture())
            {
                var customContext = new Context();
                SynchronizationContext.SetSynchronizationContext(customContext);

                var task = asyncMethod.Invoke();

                if (task != null)
                {
                    // register a continuation with the task, which will shut down the loop when the task completes.
                    task.ContinueWith(_ => customContext.WorkQueue.Shutdown(), TaskContinuationOptions.ExecuteSynchronously);
                }
                else
                {
                    // the delegate returned a null Task (VB/C# compilers never do this for async methods)
                    // we don't have anything to register continuations with in this case, so just return immediately
                    return task;
                }

                customContext.WorkQueue.ExecuteWorkQueueLoop();

                task.RethrowForCompletedTasks();

                return task;
            }
        }

        /// <summary>
        /// Runs the action inside a message loop and continues looping work items
        /// as long as any asynchronous operations have been registered
        /// </summary>
        public static void Run(Action asyncAction)
        {
            using (SynchronizationContextSwitcher.Capture())
            {
                var customContext = new VoidContext();
                SynchronizationContext.SetSynchronizationContext(customContext);

                // Do an explicit increment/decrement.
                // Our sync context does a check on decrement, to see if there are any
                // outstanding asynchronous operations (async void methods register this correctly).
                // If there aren't any registerd operations, then it will exit the loop
                customContext.OperationStarted();
                try
                {
                    asyncAction.Invoke();
                }
                finally
                {
                    customContext.OperationCompleted();
                }

                customContext.WorkQueue.ExecuteWorkQueueLoop();
                // ExecuteWorkQueueLoop() has returned. This must indicate that
                // the operation count has fallen back to zero.
            }
        }

        class VoidContext : Context
        {
            int operationCount;

            /// <summary>Constructor for creating a new AsyncVoidSyncContext. Creates a new shared operation counter.</summary>
            internal VoidContext() { }

            VoidContext(WorkQueue queue) : base(queue) { }

            public override SynchronizationContext CreateCopy()
            {
                return new VoidContext(this.WorkQueue);
            }

            public override void OperationStarted()
            {
                Interlocked.Increment(ref this.operationCount);
            }

            public override void OperationCompleted()
            {
                if (Interlocked.Decrement(ref this.operationCount) == 0)
                {
                    WorkQueue.Shutdown();
                }
            }
        }
    }

    internal struct SynchronizationContextSwitcher : IDisposable
    {
        private SynchronizationContext originalSyncContext;

        public static SynchronizationContextSwitcher Capture()
        {
            // save the value to restore
            return new SynchronizationContextSwitcher { originalSyncContext = SynchronizationContext.Current };
        }

        public void Dispose()
        {
            // restore the sync context
            SynchronizationContext.SetSynchronizationContext(originalSyncContext);
        }
    }

    public static class AsyncTestUtilities
    {
        /// <summary>
        /// 'await' has semantics so that if you await a task, it will
        /// throw if the task did not run to completion normally (similar to synchronous code).
        /// 
        /// This extension method allows for you to invoke this for completed tasks (a task in
        /// the RanToCompletion, Cancelled, or Faulted state).
        /// 
        /// Thus, if the task was cancelled or encountered an exception during
        /// execution, this will now trigger that exception to be propogated.
        /// </summary>
        public static void RethrowForCompletedTasks(this Task task)
        {
            // Here we do a bit of trickery. We explicitly call the methods that are underlying
            // the await pattern so that we can take advantage of the same Task exception
            // packaging/unpackaging mechanisms.
            //
            // This doesn't actually do a true 'await', but emulates the await pattern.
            // For tasks that haven't completed, it throws an exception rather than
            // signs up the rest of the method as a continuation.
            //
            // The pattern contract is that GetResult() can be called synchronously if IsCompleted returned true.

            task.GetAwaiter().GetResult();
        }
    }
}
