﻿using JetBrains.Annotations;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public delegate void LogSetup([NotNull] IHostingLogBuilder builder);
}