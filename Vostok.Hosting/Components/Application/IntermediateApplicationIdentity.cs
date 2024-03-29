﻿using System;
using JetBrains.Annotations;
using Vostok.Hosting.Abstractions;

namespace Vostok.Hosting.Components.Application
{
    internal class IntermediateApplicationIdentity : IVostokApplicationIdentity
    {
        private readonly string project;
        private readonly string environment;
        private readonly string application;
        private readonly string instance;

        public IntermediateApplicationIdentity(
            [CanBeNull] string project,
            [CanBeNull] string subproject,
            [CanBeNull] string environment,
            [CanBeNull] string application,
            [CanBeNull] string instance)
        {
            this.project = project;
            this.environment = environment;
            this.application = application;
            this.instance = instance;

            Subproject = subproject;
        }

        public string Project => project ?? throw CreateException(nameof(Project));

        public string Subproject { get; }

        public string Environment => environment ?? throw CreateException(nameof(Environment));

        public string Application => application ?? throw CreateException(nameof(Application));

        public string Instance => instance ?? throw CreateException(nameof(instance));

        public override string ToString()
            => $"Project = {project ?? "N/A"}; Subproject = {Subproject ?? "N/A"}; Environment = {environment ?? "N/A"}; Application = {application ?? "N/A"}; Instance = {instance ?? "N/A"}";

        private Exception CreateException(string field)
            => new InvalidOperationException($"Application identity field '{field}' can't be accessed at this time as it hasn't been configured yet. " +
                                             "This may indicate a cyclic depedency arising from an attempt to use configuration-based identity members to set up configuration.");
    }
}