using System;
using System.Collections.Generic;
using System.Linq;
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
            var missingFields = new List<string>();

            if (string.IsNullOrWhiteSpace(project))
                missingFields.Add(nameof(project));
            if (string.IsNullOrWhiteSpace(subproject))
                subproject = null;
            if (string.IsNullOrWhiteSpace(environment))
                missingFields.Add(nameof(environment));
            if (string.IsNullOrWhiteSpace(application))
                missingFields.Add(nameof(application));
            if (string.IsNullOrWhiteSpace(instance))
                missingFields.Add(nameof(instance));

            if (missingFields.Any())
                throw new ArgumentException($"Some of the Vostok application identity required fields have not been specified: '{string.Join(", ", missingFields)}'.");

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

        public override string ToString()
            => $"Project = {Project}; Subproject = {Subproject ?? "N/A"}; Environment = {Environment}; Application = {Application}; Instance = {Instance}";
    }
}