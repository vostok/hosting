using System;
using JetBrains.Annotations;
using Vostok.Hosting.Abstractions;

namespace Vostok.Hosting.Components.Application
{
    internal class ApplicationIdentity : IVostokApplicationIdentity
    {
        public ApplicationIdentity(
            [NotNull] string project, 
            [CanBeNull] string subproject, 
            [NotNull] string environment, 
            [NotNull] string application, 
            [NotNull] string instance)
        {
            if (string.IsNullOrWhiteSpace(project))
                throw new ArgumentNullException(nameof(project), "Project should be specified.");
            if (string.IsNullOrWhiteSpace(subproject))
                subproject = null;
            if (string.IsNullOrWhiteSpace(environment))
                throw new ArgumentNullException(nameof(environment), "Environment should be specified.");
            if (string.IsNullOrWhiteSpace(application))
                throw new ArgumentNullException(nameof(application), "Application should be specified.");
            if (string.IsNullOrWhiteSpace(instance))
                throw new ArgumentNullException(nameof(instance), "Instance should be specified.");

            Project = project;
            Subproject = subproject;
            Environment = environment;
            Application = application;
            Instance = instance;
        }

        public string Project { get; set; }
        public string Subproject { get; }
        public string Environment { get; set; }
        public string Application { get; set; }
        public string Instance { get; set; }
    }
}