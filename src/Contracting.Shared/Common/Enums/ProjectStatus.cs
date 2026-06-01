namespace Contracting.Shared.Common.Enums;

public enum ProjectStatus
{
    /// <summary>Project is actively being constructed/worked on.</summary>
    Active = 0,

    /// <summary>Project is in the pre-construction planning phase.</summary>
    Planning = 1,

    /// <summary>Project work has been temporarily paused.</summary>
    OnHold = 2,

    /// <summary>Project is running behind the agreed schedule.</summary>
    Delayed = 3,

    /// <summary>All work has been successfully delivered.</summary>
    Completed = 4,

    /// <summary>Project was terminated before completion.</summary>
    Cancelled = 5
}
