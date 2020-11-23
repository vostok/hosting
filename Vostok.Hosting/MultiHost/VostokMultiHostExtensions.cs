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
        public static Task<VostokApplicationRunResult> RunApplication(this VostokMultiHost host, VostokMultiHostApplicationSettings settings)
        {
            return host.AddApplication(settings).RunAsync();
        }

        /// <summary>
        /// Adds an application and returns <see cref="IVostokMultiHostApplication.StartAsync"/> task.
        /// May throw an exception if application with this identifier has already been added.
        /// <see cref="VostokMultiHost"/> should be running to perform this operation.
        /// </summary>
        public static Task StartApplication(this VostokMultiHost host, VostokMultiHostApplicationSettings settings)
        {
            return host.AddApplication(settings).StartAsync();
        }

        /// <summary>
        /// Stops provided application if it was added before.
        /// <see cref="VostokMultiHost"/> should be running to perform this operation.
        /// </summary>
        public static Task<VostokApplicationRunResult> StopApplication(this VostokMultiHost host, VostokMultiHostApplicationIdentifier identifier)
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
        public static async Task StartSequentially(this VostokMultiHost host, IEnumerable<VostokMultiHostApplicationSettings> apps)
        {
            var addedApps = apps.Select(host.AddApplication).ToArray();

            foreach (var app in addedApps)
                await app.StartAsync();
        }

        /// <summary>
        /// Adds and starts provided applications one by one.
        /// May throw an exception if application with one of the identifiers has already been added.
        /// <see cref="VostokMultiHost"/> should be running to perform this operation.
        /// </summary>
        public static Task StartSequentially(this VostokMultiHost host, params VostokMultiHostApplicationSettings[] apps)
        {
            return StartSequentially(host, (IEnumerable<VostokMultiHostApplicationSettings>)apps);
        }

        /// <summary>
        /// Adds and starts provided applications in parallel.
        /// May throw an exception if application with one of the identifiers has already been added.
        /// <see cref="VostokMultiHost"/> should be running to perform this operation.
        /// </summary>
        public static Task StartInParallel(this VostokMultiHost host, IEnumerable<VostokMultiHostApplicationSettings> apps)
        {
            return Task.WhenAll(apps.Select(app => StartApplication(host, app)));
        }

        /// <summary>
        /// Adds and starts provided applications in parallel.
        /// May throw an exception if application with one of the identifiers has already been added.
        /// <see cref="VostokMultiHost"/> should be running to perform this operation.
        /// </summary>
        public static Task StartInParallel(this VostokMultiHost host, params VostokMultiHostApplicationSettings[] apps)
        {
            return StartInParallel(host, (IEnumerable<VostokMultiHostApplicationSettings>)apps);
        }

        /// <summary>
        /// Adds and runs provided applications one by one.
        /// May throw an exception if application with one of the identifiers has already been added.
        /// <see cref="VostokMultiHost"/> should be running to perform this operation.
        /// </summary>
        public static async Task RunSequentially(this VostokMultiHost host, IEnumerable<VostokMultiHostApplicationSettings> apps)
        {
            var addedApps = apps.Select(host.AddApplication).ToArray();

            foreach (var app in addedApps)
                await app.RunAsync();
        }

        /// <summary>
        /// Adds and runs provided applications one by one.
        /// May throw an exception if application with one of the identifiers has already been added.
        /// <see cref="VostokMultiHost"/> should be running to perform this operation.
        /// </summary>
        public static Task RunSequentially(this VostokMultiHost host, params VostokMultiHostApplicationSettings[] apps)
        {
            return RunSequentially(host, (IEnumerable<VostokMultiHostApplicationSettings>)apps);
        }

        /// <summary>
        /// Adds and runs provided applications in parallel.
        /// May throw an exception if application with one of the identifiers has already been added.
        /// <see cref="VostokMultiHost"/> should be running to perform this operation.
        /// </summary>
        public static Task RunInParallel(this VostokMultiHost host, IEnumerable<VostokMultiHostApplicationSettings> apps)
        {
            return Task.WhenAll(apps.Select(app => RunApplication(host, app)));
        }

        /// <summary>
        /// Adds and runs provided applications in parallel.
        /// May throw an exception if application with one of the identifiers has already been added.
        /// <see cref="VostokMultiHost"/> should be running to perform this operation.
        /// </summary>
        public static Task RunInParallel(this VostokMultiHost host, params VostokMultiHostApplicationSettings[] apps)
        {
            return RunInParallel(host, (IEnumerable<VostokMultiHostApplicationSettings>)apps);
        }
    }
}