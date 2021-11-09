using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Vostok.Commons.Helpers.Extensions;
using Vostok.Commons.Time;
using Vostok.Hosting.Abstractions;
using Vostok.Logging.Abstractions;

// ReSharper disable SuspiciousTypeConversion.Global

namespace Vostok.Hosting.Helpers
{
    internal class ApplicationDisposable : IDisposable
    {
        private const string TheApplication = "the application";

        private readonly IVostokApplication application;
        private readonly IVostokHostingEnvironment environment;
        private readonly ILog log;

        public ApplicationDisposable(IVostokApplication application, IVostokHostingEnvironment environment, ILog log)
        {
            this.application = application;
            this.environment = environment;
            this.log = log;
        }

        public void Dispose() =>
            DisposeComponent(application as IDisposable, TheApplication, environment.ShutdownTimeout, log);

        public static void DisposeComponent(IDisposable disposable, string componentName, TimeSpan timeout, ILog log, bool shouldLog = true)
        {
            if (disposable == null)
                return;

            var disposeTask = Task.Run(
                () =>
                {
                    var watch = Stopwatch.StartNew();

                    if (shouldLog)
                        LogDisposing(log, componentName);

                    try
                    {
                        disposable.Dispose();

                        if (shouldLog && componentName == TheApplication)
                            log.Info("Disposed of the application in {ApplicationDisposeTime}.", watch.Elapsed.ToPrettyString());
                    }
                    catch (Exception error)
                    {
                        log.Error(error, "Failed to dispose of {ComponentName}.", componentName);
                    }
                });

            var disposedInTime = disposeTask.WaitAsync(timeout).GetAwaiter().GetResult();

            if (!disposedInTime)
                log.Warn("Failed to dispose of {ComponentName} within {ComponentShutdownTimeout} shutdown budget.", componentName, timeout);
        }

        public static void LogDisposing(ILog log, string componentName) =>
            log.Info("Disposing of {ComponentName}..", componentName);
    }
}