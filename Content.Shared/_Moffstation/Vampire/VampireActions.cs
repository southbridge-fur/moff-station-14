using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._Moffstation.Vampire;

public sealed partial class VampireEventGlare : InstantActionEvent;
public sealed partial class VampireEventRejuvenate : InstantActionEvent;

public sealed partial class VampireEventFeed : EntityTargetActionEvent;

[Serializable, NetSerializable]
public sealed partial class VampireEventFeedDoAfter : SimpleDoAfterEvent;


