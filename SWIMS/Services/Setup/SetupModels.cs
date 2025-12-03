using System;
using System.Collections.Generic;

namespace SWIMS.Services.Setup
{
    public enum SetupCheckStatus
    {
        Ok,
        Warning,
        Error
    }

    public sealed class SetupCheck
    {
        public string Key { get; init; } = default!;
        public string Name { get; init; } = default!;
        public string? Description { get; init; }
        public SetupCheckStatus Status { get; init; }
        public string? Details { get; init; }
    }

    public sealed class SetupSummary
    {
        /// <summary>
        /// True when the app is considered fully configured (DB reachable + no pending migrations + no hard errors).
        /// </summary>
        public bool IsConfigured { get; init; }

        public string EnvironmentName { get; init; } = default!;

        public IReadOnlyList<SetupCheck> Checks { get; init; } = Array.Empty<SetupCheck>();
    }
}
