using Content.Server._Moffstation.GameTicking.Rules.Components;
using Content.Server._Moffstation.Roles;
using Content.Server.GameTicking.Rules;
using Content.Server.Roles;

namespace Content.Server._Moffstation.GameTicking.Rules;

public sealed class VampireRuleSystem : GameRuleSystem<VampireRuleComponent>
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<VampireRoleComponent, GetBriefingEvent>(OnGetBriefing);
    }

    private void OnGetBriefing(Entity<VampireRoleComponent> role, ref GetBriefingEvent args)
    {
        args.Append(Loc.GetString("vampire-briefing"));
    }



}
