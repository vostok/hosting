using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Vostok.Commons.Helpers.Extensions;
using Vostok.Hosting.Abstractions;
using Vostok.Logging.Abstractions;

// ReSharper disable SuspiciousTypeConversion.Global

namespace Vostok.Hosting.Helpers
{
    internal class ApplicationDisposable : IDisposable
    {
        private readonly IVostokApplication application;
        private readonly IVostokHostingEnvironment environment;
        private readonly ILog log;

        public ApplicationDisposable(IVostokApplication application, IVostokHostingEnvironment environment, ILog log)
        {
            this.application = application;
            this.environment = environment;
            this.log = log;
        }

        public void Dispose()
        {
            var disposableApplication = application as IDisposable;
            if (disposableApplication == null)
                return;

            var disposeTask = Task.Run(
                () =>
                {
                    var watch = Stopwatch.StartNew();

                    log.Info("Disposing of the application..");

                    try
                    {
                        disposableApplication.Dispose();

                        log.Info("Disposed of the application in {ApplicationDisposeTime}.", watch.Elapsed);
                    }
                    catch (Exception error)
                    {
                        log.Error(error, "Failed to dispose of the application.");
                    }
                });

            var disposedInTime = disposeTask.WaitAsync(environment.ShutdownTimeout).GetAwaiter().GetResult();

            if (!disposedInTime)
                log.Warn("Failed to dispose of the application in time to meet shutdown budget.");
        }
    }
}
