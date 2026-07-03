#nullable enable

namespace NamespaceReplaceMe
{
    /// <summary>
    /// Base class for async PowerShell cmdlets.
    /// User cmdlets should inherit from this class and override the async lifecycle methods.
    /// Use AsyncPSCmdlet<T> for cmdlets that need to output objects, this class is for cmdlets
    /// that do not output objects.
    /// </summary>
    public abstract class AsyncPSCmdlet : global::System.IDisposable
    {
        // So we can keep an empty ctor for derived classes we pretend
        // _asyncHelper is not null. AsyncPSCmdletBase calls
        // InternalSetAsyncHelper to set the actual helper instance.
        private global::NamespaceReplaceMe.IAsyncHelper _asyncHelper = default!;
        internal void InternalSetAsyncHelper(
            global::NamespaceReplaceMe.IAsyncHelper asyncHelper,
            out global::System.Action beforeBegin,
            out global::System.Func<global::System.Threading.CancellationToken, global::System.Threading.Tasks.Task> beginAsync,
            out global::System.Action afterBegin,
            out global::System.Action beforeProcess,
            out global::System.Func<global::System.Threading.CancellationToken, global::System.Threading.Tasks.Task> processAsync,
            out global::System.Action afterProcess,
            out global::System.Action beforeEnd,
            out global::System.Func<global::System.Threading.CancellationToken, global::System.Threading.Tasks.Task> endAsync,
            out global::System.Action afterEnd)
        {
            _asyncHelper = asyncHelper;
            beforeBegin = BeforeBegin;
            beginAsync = BeginAsync;
            afterBegin = AfterBegin;
            beforeProcess = BeforeProcess;
            processAsync = ProcessAsync;
            afterProcess = AfterProcess;
            beforeEnd = BeforeEnd;
            endAsync = EndAsync;
            afterEnd = AfterEnd;
        }

        /// <summary>
        /// Disposes the cmdlet resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            global::System.GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Gets the underlying PSCmdlet instance. This can safely be used in
        /// the Before* and After* block methods as that runs on the same
        /// thread as the PSCmdlet. It can still be used in the *Async block
        /// methods but any members that are not thread-safe should be avoided.
        /// Use the InvokeInPipelineThreadAsync method to safely access the
        /// PSCmdlet on the pipeline thread in those cases.
        /// </summary>
        /// <returns>The underlying PSCmdlet instance.</returns>
        protected global::System.Management.Automation.PSCmdlet DangerousGetCmdlet()
        {
            global::System.Diagnostics.Debug.Assert(
                _asyncHelper != null,
                "Underlying async helper is not set.");
            return _asyncHelper.Cmdlet;
        }

        /// <summary>
        /// Invokes an action on the pipeline thread asynchronously.
        /// This is useful for executing code that must run on the PowerShell pipeline thread.
        /// </summary>
        /// <param name="action">The action to execute on the pipeline thread.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the async operation.</returns>
        protected global::System.Threading.Tasks.Task InvokeInPipelineThreadAsync(
            global::System.Action action,
            global::System.Threading.CancellationToken cancellationToken)
        {
            var pipeline = GetAsyncPipeline();

            global::System.Threading.Tasks.TaskCompletionSource tcs = new();
            using var _ = cancellationToken.Register(() => tcs.TrySetCanceled());

            pipeline.Add(() =>
            {
                try
                {
                    action();
                    tcs.TrySetResult();
                }
                catch (global::System.Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });

            return tcs.Task;
        }

        /// <summary>
        /// Invokes a function on the pipeline thread and returns the result asynchronously.
        /// This is useful for executing code that must run on the PowerShell pipeline thread.
        /// </summary>
        /// <typeparam name="T">The return type of the function.</typeparam>
        /// <param name="func">The function to execute on the pipeline thread.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the async operation with the function result.</returns>
        protected global::System.Threading.Tasks.Task<T> InvokeInPipelineThreadAsync<T>(
            global::System.Func<T> func,
            global::System.Threading.CancellationToken cancellationToken)
        {
            var pipeline = GetAsyncPipeline();

            global::System.Threading.Tasks.TaskCompletionSource<T> tcs = new();
            using var _ = cancellationToken.Register(() => tcs.TrySetCanceled());

            pipeline.Add(() =>
            {
                try
                {
                    T result = func();
                    tcs.TrySetResult(result);
                }
                catch (global::System.Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });

            return tcs.Task;
        }

        /// <summary>
        /// Requests confirmation from the user before performing an action.
        /// </summary>
        /// <param name="target">The resource being acted upon.</param>
        /// <param name="action">The action being performed.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if the action should proceed, false otherwise.</returns>
        protected global::System.Threading.Tasks.Task<bool> ShouldProcessAsync(
            string target,
            string action,
            global::System.Threading.CancellationToken cancellationToken)
            => InvokeInPipelineThreadAsync(() => DangerousGetCmdlet().ShouldProcess(target, action), cancellationToken);

        /// <summary>
        /// Requests confirmation from the user to continue with the operation.
        /// </summary>
        /// <param name="query">The confirmation question to present to the user.</param>
        /// <param name="caption">The caption to display in the confirmation prompt.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if the operation should continue, false otherwise.</returns>
        protected global::System.Threading.Tasks.Task<bool> ShouldContinueAsync(
            string query,
            string caption,
            global::System.Threading.CancellationToken cancellationToken)
            => InvokeInPipelineThreadAsync(() => DangerousGetCmdlet().ShouldContinue(query, caption), cancellationToken);

        /// <summary>
        /// Requests confirmation from the user to continue with the operation,
        /// with options to apply the decision to all subsequent items.
        /// </summary>
        /// <param name="query">The confirmation question to present to the user.</param>
        /// <param name="caption">The caption to display in the confirmation prompt.</param>
        /// <param name="hasSecurityImpact">True if the operation has security implications.</param>
        /// <param name="yesToAll">The current value of "Yes to All".</param>
        /// <param name="noToAll">The current value of "No to All".</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A ShouldContinueResult containing the decision and updated yesToAll/noToAll values.</returns>
        protected global::System.Threading.Tasks.Task<ShouldContinueResult> ShouldContinueAsync(
            string query,
            string caption,
            bool hasSecurityImpact,
            bool yesToAll,
            bool noToAll,
            global::System.Threading.CancellationToken cancellationToken)
        {
            return InvokeInPipelineThreadAsync(() =>
            {
                bool tempYesToAll = yesToAll;
                bool tempNoToAll = noToAll;

                bool shouldContinue = DangerousGetCmdlet().ShouldContinue(
                    query,
                    caption,
                    hasSecurityImpact,
                    ref tempYesToAll,
                    ref tempNoToAll);

                return new ShouldContinueResult
                {
                    ShouldContinue = shouldContinue,
                    YesToAll = tempYesToAll,
                    NoToAll = tempNoToAll
                };
            }, cancellationToken);
        }

        /// <summary>
        /// Requests confirmation from the user to continue with the operation,
        /// with options to apply the decision to all subsequent items.
        /// </summary>
        /// <param name="query">The confirmation question to present to the user.</param>
        /// <param name="caption">The caption to display in the confirmation prompt.</param>
        /// <param name="yesToAll">The current value of "Yes to All".</param>
        /// <param name="noToAll">The current value of "No to All".</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A ShouldContinueResult containing the decision and updated yesToAll/noToAll values.</returns>
        protected global::System.Threading.Tasks.Task<ShouldContinueResult> ShouldContinueAsync(
            string query,
            string caption,
            bool yesToAll,
            bool noToAll,
            global::System.Threading.CancellationToken cancellationToken)
        {
            return InvokeInPipelineThreadAsync(() =>
            {
                bool tempYesToAll = yesToAll;
                bool tempNoToAll = noToAll;

                bool shouldContinue = DangerousGetCmdlet().ShouldContinue(
                    query,
                    caption,
                    ref tempYesToAll,
                    ref tempNoToAll);

                return new ShouldContinueResult
                {
                    ShouldContinue = shouldContinue,
                    YesToAll = tempYesToAll,
                    NoToAll = tempNoToAll
                };
            }, cancellationToken);
        }

        /// <summary>
        /// Result from ShouldContinueAsync containing the decision and updated flags.
        /// </summary>
        public class ShouldContinueResult
        {
            /// <summary>
            /// True if the operation should continue, false otherwise.
            /// </summary>
            public bool ShouldContinue { get; init; }

            /// <summary>
            /// True if the user selected "Yes to All".
            /// </summary>
            public bool YesToAll { get; init; }

            /// <summary>
            /// True if the user selected "No to All".
            /// </summary>
            public bool NoToAll { get; init; }
        }

        /// <summary>
        /// Terminates the currently running cmdlet with a terminating error.
        /// The returned task will throw a PipelineStoppedException when awaited.
        /// </summary>
        /// <param name="errorRecord">The error record that describes the terminating error.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task that will throw PipelineStoppedException when awaited.</returns>
        protected global::System.Threading.Tasks.Task ThrowTerminatingErrorAsync(
            global::System.Management.Automation.ErrorRecord errorRecord,
            global::System.Threading.CancellationToken cancellationToken)
        {
            return InvokeInPipelineThreadAsync(() =>
            {
                // ThrowTerminatingError never returns - it throws PipelineStoppedException
                DangerousGetCmdlet().ThrowTerminatingError(errorRecord);
            }, cancellationToken);
        }

        /// <summary>
        /// Writes an error record to the error pipeline.
        /// </summary>
        /// <param name="errorRecord">The error record to write.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the async operation.</returns>
        protected global::System.Threading.Tasks.Task WriteErrorAsync(
            global::System.Management.Automation.ErrorRecord errorRecord,
            global::System.Threading.CancellationToken cancellationToken)
            => InvokeInPipelineThreadAsync(() => DangerousGetCmdlet().WriteError(errorRecord), cancellationToken);

        /// <summary>
        /// Writes a warning message to the warning pipeline.
        /// </summary>
        /// <param name="message">The warning message.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the async operation.</returns>
        protected global::System.Threading.Tasks.Task WriteWarningAsync(
            string message,
            global::System.Threading.CancellationToken cancellationToken)
            => InvokeInPipelineThreadAsync(() => DangerousGetCmdlet().WriteWarning(message), cancellationToken);

        /// <summary>
        /// Writes a verbose message to the verbose pipeline.
        /// </summary>
        /// <param name="message">The verbose message.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the async operation.</returns>
        protected global::System.Threading.Tasks.Task WriteVerboseAsync(
            string message,
            global::System.Threading.CancellationToken cancellationToken)
            => InvokeInPipelineThreadAsync(() => DangerousGetCmdlet().WriteVerbose(message), cancellationToken);

        /// <summary>
        /// Writes a debug message to the debug pipeline.
        /// </summary>
        /// <param name="message">The debug message.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the async operation.</returns>
        protected global::System.Threading.Tasks.Task WriteDebugAsync(
            string message,
            global::System.Threading.CancellationToken cancellationToken)
            => InvokeInPipelineThreadAsync(() => DangerousGetCmdlet().WriteDebug(message), cancellationToken);

        /// <summary>
        /// Writes an information record to the information pipeline.
        /// </summary>
        /// <param name="informationRecord">The information record to write.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the async operation.</returns>
        protected global::System.Threading.Tasks.Task WriteInformationAsync(
            global::System.Management.Automation.InformationRecord informationRecord,
            global::System.Threading.CancellationToken cancellationToken)
            => InvokeInPipelineThreadAsync(() => DangerousGetCmdlet().WriteInformation(informationRecord), cancellationToken);

        /// <summary>
        /// Writes an information message with tags to the information pipeline.
        /// </summary>
        /// <param name="messageData">The message data.</param>
        /// <param name="tags">Tags to associate with the message.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the async operation.</returns>
        protected global::System.Threading.Tasks.Task WriteInformationAsync(
            object messageData,
            string[] tags,
            global::System.Threading.CancellationToken cancellationToken)
        {
            return InvokeInPipelineThreadAsync(() =>
            {
                global::System.Management.Automation.PSCmdlet cmdlet = DangerousGetCmdlet();
                string? source = cmdlet.MyInvocation.PSCommandPath;
                if (string.IsNullOrEmpty(source))
                {
                    source = cmdlet.MyInvocation.MyCommand.Name;
                }

                global::System.Management.Automation.InformationRecord infoRecord = new(
                    messageData,
                    source);
                infoRecord.Tags.AddRange(tags);
                cmdlet.WriteInformation(infoRecord);
            }, cancellationToken);
        }

        /// <summary>
        /// Writes a message to the host (console).
        /// </summary>
        /// <param name="message">The message to write.</param>
        /// <param name="noNewLine">If true, does not append a newline.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the async operation.</returns>
        protected global::System.Threading.Tasks.Task WriteHostAsync(
            string message,
            bool noNewLine,
            global::System.Threading.CancellationToken cancellationToken)
        {
            global::System.Management.Automation.HostInformationMessage msg = new()
            {
                Message = message,
                NoNewLine = noNewLine,
            };
            return WriteInformationAsync(msg, ["PSHOST"], cancellationToken);
        }

        /// <summary>
        /// Writes a progress record to the progress pipeline.
        /// </summary>
        /// <param name="progressRecord">The progress record to write.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the async operation.</returns>
        protected global::System.Threading.Tasks.Task WriteProgressAsync(
            global::System.Management.Automation.ProgressRecord progressRecord,
            global::System.Threading.CancellationToken cancellationToken)
            => InvokeInPipelineThreadAsync(() => DangerousGetCmdlet().WriteProgress(progressRecord), cancellationToken);

        /// <summary>
        /// Called before BeginAsync. Use for synchronous initialization.
        /// </summary>
        protected virtual void BeforeBegin()
        {
        }

        /// <summary>
        /// Called once at the beginning of cmdlet execution. Override to perform async initialization.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token that signals when the cmdlet is stopping.</param>
        /// <returns>A task representing the async operation.</returns>
        protected virtual global::System.Threading.Tasks.Task BeginAsync(global::System.Threading.CancellationToken cancellationToken)
        {
            return global::System.Threading.Tasks.Task.CompletedTask;
        }

        /// <summary>
        /// Called after BeginAsync completes. Use for synchronous cleanup.
        /// </summary>
        protected virtual void AfterBegin()
        {
        }

        /// <summary>
        /// Called before ProcessAsync. Use for synchronous per-record setup.
        /// </summary>
        protected virtual void BeforeProcess()
        {
        }

        /// <summary>
        /// Called once for each input record. Override to perform async processing of pipeline input.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token that signals when the cmdlet is stopping.</param>
        /// <returns>A task representing the async operation.</returns>
        protected virtual global::System.Threading.Tasks.Task ProcessAsync(global::System.Threading.CancellationToken cancellationToken)
        {
            return global::System.Threading.Tasks.Task.CompletedTask;
        }

        /// <summary>
        /// Called after ProcessAsync completes. Use for synchronous per-record cleanup.
        /// </summary>
        protected virtual void AfterProcess()
        {
        }

        /// <summary>
        /// Called before EndAsync. Use for synchronous finalization setup.
        /// </summary>
        protected virtual void BeforeEnd()
        {
        }

        /// <summary>
        /// Called once at the end of cmdlet execution. Override to perform async finalization.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token that signals when the cmdlet is stopping.</param>
        /// <returns>A task representing the async operation.</returns>
        protected virtual global::System.Threading.Tasks.Task EndAsync(global::System.Threading.CancellationToken cancellationToken)
        {
            return global::System.Threading.Tasks.Task.CompletedTask;
        }

        /// <summary>
        /// Called after EndAsync completes. Use for synchronous final cleanup.
        /// </summary>
        protected virtual void AfterEnd()
        {
        }

        /// <summary>
        /// Releases resources used by the cmdlet.
        /// </summary>
        /// <param name="disposing">True if disposing managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
        }

        /// <summary>
        /// Gets the async pipeline for executing actions on the PowerShell pipeline thread.
        /// This should only be called when in the BeginAsync, ProcessAsync, or EndAsync methods.
        /// </summary>
        /// <returns>The async pipeline.</returns>
        private global::System.Collections.Concurrent.BlockingCollection<global::System.Action?> GetAsyncPipeline()
        {
            global::System.Diagnostics.Debug.Assert(
                _asyncHelper != null,
                "Underlying async helper is not set.");

            if (!_asyncHelper.InAsyncBlock)
            {
                throw new global::System.InvalidOperationException(
                    "Cannot execute actions on the pipeline thread. This operation can only be called from within the BeginAsync, ProcessAsync, or EndAsync methods.");
            }

            // No need to check IsStopping - if the pipeline is stopped via CompleteAdding(),
            // Pipeline.Add() will throw InvalidOperationException naturally, which is the
            // expected behavior when trying to add to a completed collection.
            return _asyncHelper.Pipeline;
        }
    }

    /// <summary>
    /// Base class for async PowerShell cmdlets with a typed output.
    /// Provides WriteAsync method for writing strongly-typed objects to the pipeline.
    /// </summary>
    /// <typeparam name="T">The output type of the cmdlet.</typeparam>
    public abstract class AsyncPSCmdlet<T> : global::NamespaceReplaceMe.AsyncPSCmdlet
    {
        /// <summary>
        /// Writes an object to the output pipeline asynchronously.
        /// </summary>
        /// <param name="sendToPipeline">The object to write to the pipeline.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the async operation.</returns>
        protected global::System.Threading.Tasks.Task WriteAsync(
            T sendToPipeline,
            global::System.Threading.CancellationToken cancellationToken)
            => WriteAsync(sendToPipeline, false, cancellationToken);

        /// <summary>
        /// Writes an object to the output pipeline asynchronously.
        /// </summary>
        /// <param name="sendToPipeline">The object to write to the pipeline.</param>
        /// <param name="enumerateCollection">If true and sendToPipeline is a collection, writes each element separately.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the async operation.</returns>
        protected global::System.Threading.Tasks.Task WriteAsync(
            T sendToPipeline,
            bool enumerateCollection,
            global::System.Threading.CancellationToken cancellationToken)
            => InvokeInPipelineThreadAsync(() => DangerousGetCmdlet().WriteObject(sendToPipeline, enumerateCollection), cancellationToken);
    }
}
