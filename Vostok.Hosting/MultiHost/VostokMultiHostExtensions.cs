﻿using System.Collections.Generic;
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
        /// Runs provided application.
        /// <see cref="VostokMultiHost"/> should be started to perform this operation.
        /// </summary>
        public static Task<VostokApplicationRunResult> RunApp(this VostokMultiHost host, VostokMultiHostApplicationSettings settings)
        {
            return host.AddApp(settings).RunAsync();
        }

        /// <summary>
        /// Starts provided application.
        /// <see cref="VostokMultiHost"/> should be started to perform this operation.
        /// </summary>
        public static Task StartApp(this VostokMultiHost host, VostokMultiHostApplicationSettings settings)
        {
            return host.AddApp(settings).StartAsync();
        }

        /// <summary>
        /// Stops provided application if it was added before.
        /// <see cref="VostokMultiHost"/> should be started to perform this operation.
        /// </summary>
        public static Task<VostokApplicationRunResult> StopApp(this VostokMultiHost host, string appName)
        {
            return host.GetApp(appName).StopAsync();
        }

        /// <summary>
        /// Starts provided applications one by one.
        /// <see cref="VostokMultiHost"/> should be started to perform this operation.
        /// </summary>
        public static Task StartSequentially(this VostokMultiHost host, IEnumerable<VostokMultiHostApplicationSettings> apps)
        {
            foreach (var app in apps)
                host.StartApp(app).GetAwaiter().GetResult();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Starts provided applications one by one.
        /// <see cref="VostokMultiHost"/> should be started to perform this operation.
        /// </summary>
        public static Task StartSequentially(this VostokMultiHost host, params VostokMultiHostApplicationSettings[] apps)
        {
            return StartSequentially(host, (IEnumerable<VostokMultiHostApplicationSettings>)apps);
        }

        /// <summary>
        /// Starts provided applications in parallel.
        /// <see cref="VostokMultiHost"/> should be started to perform this operation.
        /// </summary>
        public static Task StartInParallel(this VostokMultiHost host, IEnumerable<VostokMultiHostApplicationSettings> apps)
        {
            return Task.WhenAll(apps.Select(app => StartApp(host, app)));
        }

        /// <summary>
        /// Starts provided applications in parallel.
        /// <see cref="VostokMultiHost"/> should be started to perform this operation.
        /// </summary>
        public static Task StartInParallel(this VostokMultiHost host, params VostokMultiHostApplicationSettings[] apps)
        {
            return StartInParallel(host, (IEnumerable<VostokMultiHostApplicationSettings>)apps);
        }

        /// <summary>
        /// Runs provided applications one by one.
        /// <see cref="VostokMultiHost"/> should be started to perform this operation.
        /// </summary>
        public static Task RunSequentially(this VostokMultiHost host, IEnumerable<VostokMultiHostApplicationSettings> apps)
        {
            foreach (var app in apps)
                host.StartApp(app).GetAwaiter().GetResult();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Runs provided applications one by one.
        /// <see cref="VostokMultiHost"/> should be started to perform this operation.
        /// </summary>
        public static Task RunSequentially(this VostokMultiHost host, params VostokMultiHostApplicationSettings[] apps)
        {
            return RunSequentially(host, (IEnumerable<VostokMultiHostApplicationSettings>)apps);
        }

        /// <summary>
        /// Runs provided applications in parallel.
        /// <see cref="VostokMultiHost"/> should be started to perform this operation.
        /// </summary>
        public static Task RunInParallel(this VostokMultiHost host, IEnumerable<VostokMultiHostApplicationSettings> apps)
        {
            return Task.WhenAll(apps.Select(app => RunApp(host, app)));
        }

        /// <summary>
        /// Runs provided applications in parallel.
        /// <see cref="VostokMultiHost"/> should be started to perform this operation.
        /// </summary>
        public static Task RunInParallel(this VostokMultiHost host, params VostokMultiHostApplicationSettings[] apps)
        {
            return RunInParallel(host, (IEnumerable<VostokMultiHostApplicationSettings>)apps);
        }
    }
}