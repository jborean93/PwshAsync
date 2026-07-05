#nullable enable

namespace NamespaceReplaceMe
{
    public abstract partial class PSAsyncCmdletBase<TAsyncCmdlet> : global::System.Management.Automation.PSCmdlet, global::System.IDisposable
        where TAsyncCmdlet : global::NamespaceReplaceMe.PSAsyncCmdlet, new()
    {
        private class AsyncHelper : global::NamespaceReplaceMe.IAsyncHelper, global::System.IDisposable
        {
            public global::System.Management.Automation.PSCmdlet Cmdlet { get; init; }

            public global::System.Collections.Concurrent.BlockingCollection<global::System.Action?> Pipeline { get; } = new();

            public bool InAsyncBlock { get; set; }

            public AsyncHelper(global::System.Management.Automation.PSCmdlet cmdlet)
            {
                Cmdlet = cmdlet;
            }

            public void Dispose()
            {
                Pipeline.Dispose();
            }
        }

        private readonly AsyncHelper _asyncHelper;
        private readonly global::System.Threading.CancellationTokenRegistration _pipelineStopTokenRegistration;
        private readonly global::System.Action _beforeBegin;
        private readonly global::System.Func<global::System.Threading.CancellationToken, global::System.Threading.Tasks.Task> _beginAsync;
        private readonly global::System.Action _afterBegin;
        private readonly global::System.Action _beforeProcess;
        private readonly global::System.Func<global::System.Threading.CancellationToken, global::System.Threading.Tasks.Task> _processAsync;
        private readonly global::System.Action _afterProcess;
        private readonly global::System.Action _beforeEnd;
        private readonly global::System.Func<global::System.Threading.CancellationToken, global::System.Threading.Tasks.Task> _endAsync;
        private readonly global::System.Action _afterEnd;

        // protected is needed so derived classes can access the parameter
        // properties in SyncInitialProperties and SyncPipelineProperties.
        private protected readonly TAsyncCmdlet _asyncCmdlet;

        protected PSAsyncCmdletBase()
        {
            _asyncHelper = new(this);
            _pipelineStopTokenRegistration = PipelineStopToken.Register(() =>
            {
                // Signal pipeline to stop accepting new actions.
                // This is thread-safe and will cause GetConsumingEnumerable() to exit
                // and Pipeline.Add() to throw InvalidOperationException.
                _asyncHelper.Pipeline.CompleteAdding();
            });

            _asyncCmdlet = new TAsyncCmdlet();
            _asyncCmdlet.InternalSetAsyncHelper(
                _asyncHelper,
                out _beforeBegin,
                out _beginAsync,
                out _afterBegin,
                out _beforeProcess,
                out _processAsync,
                out _afterProcess,
                out _beforeEnd,
                out _endAsync,
                out _afterEnd);
        }

        // Sync all properties - called once in BeginProcessing
        protected abstract void SyncInitialProperties();

        // Sync ValueFromPipeline properties - called in each ProcessRecord
        protected abstract void SyncPipelineProperties();

        protected override void BeginProcessing()
        {
            SyncInitialProperties();
            RunBlockInAsync(_beforeBegin, _beginAsync, _afterBegin);
        }

        protected override void ProcessRecord()
        {
            SyncPipelineProperties();
            RunBlockInAsync(_beforeProcess, _processAsync, _afterProcess);
        }

        protected override void EndProcessing()
        {
            RunBlockInAsync(_beforeEnd, _endAsync, _afterEnd);
        }

        private void RunBlockInAsync(
            global::System.Action beforeBlock,
            global::System.Func<global::System.Threading.CancellationToken, global::System.Threading.Tasks.Task> taskBlock,
            global::System.Action afterBlock)
        {
            beforeBlock();

            try
            {
                _asyncHelper.InAsyncBlock = true;

                // Kick off the async task.
                global::System.Threading.Tasks.Task blockTask = global::System.Threading.Tasks.Task.Run(async () =>
                {
                    try
                    {
                        await taskBlock(PipelineStopToken);
                    }
                    finally
                    {
                        // Add null sentinel to signal the block is complete.
                        // Use TryAdd because CompleteAdding() might have been called
                        // by the PipelineStopToken cancellation callback.
                        _asyncHelper.Pipeline.TryAdd(null);
                    }
                });

                // Consume the actions and execute them on the PowerShell pipeline thread.
                // The loop exits when the async task's finally block adds the sentinel null.
                // We don't pass PipelineStopToken here because we want to wait for the
                // task to complete naturally and signal via the sentinel.
                foreach (global::System.Action? action in _asyncHelper.Pipeline.GetConsumingEnumerable())
                {
                    if (action is null)
                    {
                        // Sentinel received - task has completed its finally block
                        break;
                    }

                    action();
                }

                // Wait for the async task to fully complete.
                // At this point it should be done (we received the sentinel), but we
                // wait to ensure it's fully exited and to propagate any exceptions.
                try
                {
                    blockTask.GetAwaiter().GetResult();
                }
                catch (global::System.OperationCanceledException)
                {
                    // Expected when PipelineStopToken is cancelled
                }
            }
            finally
            {
                _asyncHelper.InAsyncBlock = false;
            }

            afterBlock();
        }

        public void Dispose()
        {
            _asyncCmdlet.Dispose();
            _asyncHelper.Dispose();
            _pipelineStopTokenRegistration.Dispose();
            DisposeInternal();
            global::System.GC.SuppressFinalize(this);
        }

        // Implemented in the generated partial class
        partial void DisposeInternal();
    }
}
