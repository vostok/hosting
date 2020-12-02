using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Hosting.Models;

namespace Vostok.Hosting.MultiHost
{
    [PublicAPI]
    public static class VostokMultiHostExtensions
    {
        /// <summary>
        /// Adds an application and returns <see cref="IVostokMultiHostApplication.RunAsync"/> task.
        /// May throw an exception if application with this identifier has already been added.
        /// <see cref="VostokMultiHost"/> should be running to perform this operation.
        /// </summary>
        public static Task<VostokApplicationRunResult> RunApplicationAsync(this VostokMultiHost host, VostokMultiHostApplicationSettings settings)
        {
            return host.AddApplication(settings).RunAsync();
        }

        /// <summary>
        /// Adds an application and returns <see cref="IVostokMultiHostApplication.StartAsync"/> task.
        /// May throw an exception if application with this identifier has already been added.
        /// <see cref="VostokMultiHost"/> should be running to perform this operation.
        /// </summary>
        public static Task StartApplicationAsync(this VostokMultiHost host, VostokMultiHostApplicationSettings settings)
        {
            return host.AddApplication(settings).StartAsync();
        }

        /// <summary>
        /// Stops provided application if it was added before.
        /// <see cref="VostokMultiHost"/> should be running to perform this operation.
        /// </summary>
        public static Task<VostokApplicationRunResult> StopApplicationAsync(this VostokMultiHost host, VostokMultiHostApplicationIdentifier identifier)
        {
            var application = host.GetApplication(identifier);

            if (application == null)
                throw new InvalidOperationException($"{identifier} doesn't exist.");

            return application.StopAsync();
        }

        /// <summary>
        /// Adds and starts provided applications one by one.
        /// May throw an exception if application with one of the identifiers has already been added.
        /// <see cref="VostokMultiHost"/> should be running to perform this operation.
        /// </summary>
        public static async Task StartSequentiallyAsync(this VostokMultiHost host, IEnumerable<VostokMultiHostApplicationSettings> apps)
        {
            var addedApps = apps.Select(host.AddApplication).ToArray();

            foreach (var app in addedApps)
                await app.StartAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Adds and starts provided applications one by one.
        /// May throw an exception if application with one of the identifiers has already been added.
        /// <see cref="VostokMultiHost"/> should be running to perform this operation.
        /// </summary>
        public static Task StartSequentiallyAsync(this VostokMultiHost host, params VostokMultiHostApplicationSettings[] apps)
        {
            return StartSequentiallyAsync(host, (IEnumerable<VostokMultiHostApplicationSettings>)apps);
        }

        /// <summary>
        /// Adds and starts provided applications in parallel.
        /// May throw an exception if application with one of the identifiers has already been added.
        /// <see cref="VostokMultiHost"/> should be running to perform this operation.
        /// </summary>
        public static Task StartInParallelAsync(this VostokMultiHost host, IEnumerable<VostokMultiHostApplicationSettings> apps)
        {
            return Task.WhenAll(apps.Select(app => StartApplicationAsync(host, app)));
        }

        /// <summary>
        /// Adds and starts provided applications in parallel.
        /// May throw an exception if application with one of the identifiers has already been added.
        /// <see cref="VostokMultiHost"/> should be running to perform this operation.
        /// </summary>
        public static Task StartInParallelAsync(this VostokMultiHost host, params VostokMultiHostApplicationSettings[] apps)
        {
            return StartInParallelAsync(host, (IEnumerable<VostokMultiHostApplicationSettings>)apps);
        }

        /// <summary>
        /// Adds and runs provided applications one by one.
        /// May throw an exception if application with one of the identifiers has already been added.
        /// <see cref="VostokMultiHost"/> should be running to perform this operation.
        /// </summary>
        public static async Task RunSequentiallyAsync(this VostokMultiHost host, IEnumerable<VostokMultiHostApplicationSettings> apps)
        {
            var addedApps = apps.Select(host.AddApplication).ToArray();

            foreach (var app in addedApps)
                await app.RunAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Adds and runs provided applications one by one.
        /// May throw an exception if application with one of the identifiers has already been added.
        /// <see cref="VostokMultiHost"/> should be running to perform this operation.
        /// </summary>
        public static Task RunSequentiallyAsync(this VostokMultiHost host, params VostokMultiHostApplicationSettings[] apps)
        {
            return RunSequentiallyAsync(host, (IEnumerable<VostokMultiHostApplicationSettings>)apps);
        }

        /// <summary>
        /// Adds and runs provided applications in parallel.
        /// May throw an exception if application with one of the identifiers has already been added.
        /// <see cref="VostokMultiHost"/> should be running to perform this operation.
        /// </summary>
        public static Task RunInParallelAsync(this VostokMultiHost host, IEnumerable<VostokMultiHostApplicationSettings> apps)
        {
            return Task.WhenAll(apps.Select(app => RunApplicationAsync(host, app)));
        }

        /// <summary>
        /// Adds and runs provided applications in parallel.
        /// May throw an exception if application with one of the identifiers has already been added.
        /// <see cref="VostokMultiHost"/> should be running to perform this operation.
        /// </summary>
        public static Task RunInParallelAsync(this VostokMultiHost host, params VostokMultiHostApplicationSettings[] apps)
        {
            return RunInParallelAsync(host, (IEnumerable<VostokMultiHostApplicationSettings>)apps);
        }
    }
}