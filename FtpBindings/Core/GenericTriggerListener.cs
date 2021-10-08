using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FtpBindings
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TOptions">configuration type</typeparam>
    /// <typeparam name="TMessage">input message type</typeparam>
    /// <typeparam name="TResult">function result type</typeparam>
    internal class GenericTriggerListener<TOptions, TMessage, TResult> : IListener /*, IScaleMonitorProvider */
        where TMessage : class
        where TOptions : IGenericOptions
    {
        private const int TimeToWaitForRunningProcessToEnd = 10 * 1000;
        private SemaphoreSlim? _subscriberFinished = null;
        private int isClosed = 0;
        private bool disposed = false;
        private readonly string _functionId;
        private readonly CancellationTokenSource _listenerStoppingTokenSource;
        private readonly ITriggeredFunctionExecutor _executor;
        private readonly int _MaxRetryOnAFFailure;
        private readonly int _MaxRetryDelayOnAFFailure;
        private readonly IValueProvider _resultValueProvider;

        protected readonly TOptions _options;
        protected readonly ILogger _logger;

        public GenericTriggerListener(ITriggeredFunctionExecutor executor, string functionId, ILogger logger, TOptions options, IValueProvider resultValueProvider)
        {
            this._executor = executor;
            this._functionId = functionId;
            this._logger = logger;
            this._options = options;
            this._listenerStoppingTokenSource = new CancellationTokenSource();
            _MaxRetryOnAFFailure = _options.MaxRetryOnFailure; //should be configurable
            _MaxRetryDelayOnAFFailure = _options.MaxRetryDelayOnFailureMs;
            _resultValueProvider = resultValueProvider;
        }

        public void Cancel()
        {
            SafeStopAsync().GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                this._logger.LogInformation($"Disposing FTP Listener for '{_functionId}'");

                if (disposing)
                {
                    this.SafeStopAsync().GetAwaiter().GetResult();
                }

                disposed = true;
            }
        }

        private async Task SafeStopAsync()
        {
            try
            {
                //to avoid multipe calls
                if (Interlocked.Exchange(ref isClosed, 1) == 1)
                {
                    return;
                }
                _listenerStoppingTokenSource.Cancel();

                // Wait for subscriber thread to end                
                if (this._subscriberFinished != null)
                {
                    await this._subscriberFinished.WaitAsync(TimeToWaitForRunningProcessToEnd);
                }

                this._subscriberFinished?.Dispose();
                _listenerStoppingTokenSource.Dispose();
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, $"Failed to close FTP listener for '{_functionId}'");
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var thread = new Thread(ListenThread)
            {
                IsBackground = true
            };
            thread.Start(_listenerStoppingTokenSource.Token);

            return Task.CompletedTask;
        }

        private void ListenThread(object parameter)
        {
            ListenThreadAsync(parameter).GetAwaiter().GetResult();
        }
        private async Task ListenThreadAsync(object parameter)
        {
            this._subscriberFinished = new SemaphoreSlim(0, 1);
            var cancellationToken = (CancellationToken)parameter;
            try
            {
                var lastRead = DateTime.MinValue;
                while (!cancellationToken.IsCancellationRequested)
                {
                    //in order to simulate polling frequency
                    await PreviousReadMessages(DateTime.UtcNow - lastRead, cancellationToken);
                    lastRead = DateTime.UtcNow;

                    await foreach (var message in ReadMessages().WithCancellation(cancellationToken))
                    {
                        int retry = 0;
                        while (retry < _MaxRetryOnAFFailure && !cancellationToken.IsCancellationRequested)
                        {
                            LogInformation("Before TryExecuteAsync");
                            var fnResult = await _executor.TryExecuteAsync(new TriggeredFunctionData()
                            {
                                TriggerValue = message
                            }, cancellationToken);
                            if (fnResult.Succeeded)
                            {
                                retry = _MaxRetryOnAFFailure;
                                var fnResultValue = (TResult)await _resultValueProvider.GetValueAsync();
                                await ProceedWithSuccessAsync(message, fnResultValue);
                            }
                            else
                            {
                                
                                retry++;
                                //wait before retry, to ensure failure is not caused by the message content
                                if (retry < _MaxRetryOnAFFailure)
                                {
                                    LogInformation("Before Retry");
                                    //wait for cancellationToken or our retry timeout
                                    cancellationToken.WaitHandle.WaitOne(_MaxRetryDelayOnAFFailure * retry);
                                }
                                else
                                {
                                    LogInformation("MaxRetry is reached");
                                    //max retry is reached
                                    //Move to error folder
                                    //TODO: manage fnResult.Exception                                
                                    await ProceedWithFailureAsync(message);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Trigger failed while consuming : {0}", ex);
                throw;
            }
            finally
            {
                this._subscriberFinished.Release();
            }
        }

        protected virtual Task PreviousReadMessages(TimeSpan peviousReadElapsed, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected virtual Task ProceedWithSuccessAsync(TMessage? message, TResult fnResult)
        {
            return Task.CompletedTask;
        }

        protected virtual Task ProceedWithFailureAsync(TMessage? message)
        {
            return Task.CompletedTask;
        }

        protected virtual async IAsyncEnumerable<TMessage> ReadMessages()
        {
            await Task.Delay(1);
            yield break;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await SafeStopAsync();
        }

        private void LogInformation(string message)
        {
            _logger.LogInformation($"{this.GetHashCode()}:{message}");
        }
    }
}