using System;
using System.Collections.Generic;
using Vostok.Commons.Helpers;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Components.ZooKeeper;
using Vostok.Hosting.Setup;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.HostExtensions
{
    internal class HostExtensionsBuilder : IVostokHostExtensionsBuilder
    {
        public readonly HostExtensions HostExtensions;

        private readonly Customization<HostExtensionsBuilder> builderCustomization;
        private readonly List<object> disposables;
        private volatile IVostokHostingEnvironment environment;

        public HostExtensionsBuilder()
        {
            HostExtensions = new HostExtensions();
            builderCustomization = new Customization<HostExtensionsBuilder>();
            disposables = new List<object>();
        }

        public void AddCustomization(Action<IVostokHostExtensionsBuilder> setup)
            => builderCustomization.AddCustomization(setup);

        public void AddCustomization(Action<IVostokHostExtensionsBuilder, IVostokHostingEnvironment> setup)
            => builderCustomization.AddCustomization(b => setup(b, environment));

        public void Build(BuildContext context, IVostokHostingEnvironment environment)
        {
            this.environment = environment;

            if (context.ZooKeeperClient != null && !(context.ZooKeeperClient is DevNullZooKeeperClient))
                HostExtensions.Add(context.ZooKeeperClient);

            HostExtensions.Add<IVostokApplicationDiagnostics>(context.DiagnosticsHub);
            HostExtensions.Add(context.Logs.LogEventLevelCounterFactory);

            builderCustomization.Customize(this);

            context.HostExtensions = HostExtensions;

            // note (kungurtsev, 02.11.2021): user components should be disposed right after application in reverse order
            disposables.Reverse();
            context.Disposables.InsertRange(0, disposables);
        }

        public IVostokHostExtensionsBuilder Add(Type type, object extension) =>
            AddInternal(type, extension, false);

        public IVostokHostExtensionsBuilder Add(Type type, string key, object extension) =>
            AddInternal(type, key, extension, false);

        public IVostokHostExtensionsBuilder Add<TExtension>(TExtension extension) =>
            AddInternal(typeof(TExtension), extension, false);

        public IVostokHostExtensionsBuilder Add<TExtension>(string key, TExtension extension) =>
            AddInternal(typeof(TExtension), key, extension, false);

        public IVostokHostExtensionsBuilder AddDisposable<TExtension>(TExtension extension) =>
            AddInternal(typeof(TExtension), extension, true);

        public IVostokHostExtensionsBuilder AddDisposable<TExtension>(string key, TExtension extension) =>
            AddInternal(typeof(TExtension), key, extension, true);

        public IVostokHostExtensionsBuilder AddDisposable(Type type, object extension) =>
            AddInternal(type, extension, true);

        public IVostokHostExtensionsBuilder AddDisposable(Type type, string key, object extension) =>
            AddInternal(type, key, extension, true);

        private IVostokHostExtensionsBuilder AddInternal(Type type, object extension, bool disposable)
        {
            if (extension == null)
                throw new ArgumentNullException(nameof(extension));

            HostExtensions.Add(type ?? throw new ArgumentNullException(nameof(type)), extension);

            if (disposable)
                disposables.Add(extension);

            return this;
        }

        private IVostokHostExtensionsBuilder AddInternal(Type type, string key, object extension, bool disposable)
        {
            if (extension == null)
                throw new ArgumentNullException(nameof(extension));

            HostExtensions.Add(type ?? throw new ArgumentNullException(nameof(type)), key ?? throw new ArgumentNullException(nameof(key)), extension);

            if (disposable)
                disposables.Add(extension);

            return this;
        }
    }
}