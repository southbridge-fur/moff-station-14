using System.Linq;
using Content.Server._Moffstation.GameTicking.Rules.Components;
using Content.Server.Clothing.Systems;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.KillTracking;
using Content.Server.Mind;
using Content.Server.RoundEnd;
using Content.Server.Station.Systems;
using Content.Shared.GameTicking;
using Content.Shared.GameTicking.Components;
using Content.Shared.Hands.EntitySystems;
using Robust.Server.Player;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server._Moffstation.GameTicking.Rules;

/// <summary>
/// The system for the gungame gamemode rule.
/// This is mostly a copy of <see cref="Content.Server.GameTicking.Rules.DeathMatchRuleSystem"/> with some
/// key changes for gun game.
/// </summary>
public sealed class GunGameRuleSystem : GameRuleSystem<GunGameRuleComponent>
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly OutfitSystem _outfitSystem = default!;
    [Dependency] private readonly RespawnRuleSystem _respawn = default!;
    [Dependency] private readonly RoundEndSystem _roundEnd = default!;
    [Dependency] private readonly StationSpawningSystem _stationSpawning = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerBeforeSpawnEvent>(OnBeforeSpawn);
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnSpawnComplete);
        SubscribeLocalEvent<KillReportedEvent>(OnKillReported);
    }

    private void OnBeforeSpawn(PlayerBeforeSpawnEvent ev)
    {
        var query = EntityQueryEnumerator<GunGameRuleComponent, RespawnTrackerComponent, GameRuleComponent>();
        while (query.MoveNext(out var uid, out var gunGame, out var tracker, out var rule))
        {
            if (!GameTicker.IsGameRuleActive(uid, rule))
                continue;

            var newMind = _mind.CreateMind(ev.Player.UserId, ev.Profile.Name);
            _mind.SetUserId(newMind, ev.Player.UserId);

            var mobMaybe = _stationSpawning.SpawnPlayerCharacterOnStation(ev.Station, null, ev.Profile);
            DebugTools.AssertNotNull(mobMaybe);
            var mob = mobMaybe!.Value;

            _mind.TransferTo(newMind, mob);
            _outfitSystem.SetOutfit(mob, gunGame.Gear);
            EnsureComp<KillTrackerComponent>(mob);
            EnsureComp<GunGameRewardTrackerComponent>(mob);
            SpawnCurrentWeapon(ev.Player.UserId, gunGame);
            _respawn.AddToTracker(ev.Player.UserId, (uid, tracker));

            ev.Handled = true;
            break;
        }
    }

    private void OnSpawnComplete(PlayerSpawnCompleteEvent ev)
    {
        EnsureComp<KillTrackerComponent>(ev.Mob);
        EnsureComp<GunGameRewardTrackerComponent>(ev.Mob);
        var query = EntityQueryEnumerator<GunGameRuleComponent, RespawnTrackerComponent, GameRuleComponent>();
        while (query.MoveNext(out var uid, out _, out var tracker, out var rule))
        {
            if (!GameTicker.IsGameRuleActive(uid, rule))
                continue;
            _respawn.AddToTracker((ev.Mob, null), (uid, tracker));
        }
    }

    /// <summary>
    /// Activates when a kill is reported, ev.Entity corresponds to the person who was killed.
    /// </summary>
    private void OnKillReported(ref KillReportedEvent ev)
    {
        // Don't want other players picking up somebody's gun
        DeleteCurrentWeapons(EnsureComp<GunGameRewardTrackerComponent>(ev.Entity));

        var query = EntityQueryEnumerator<GunGameRuleComponent, GameRuleComponent>();
        while (query.MoveNext(out var uid, out var gungame, out var rule))
        {
            if (!GameTicker.IsGameRuleActive(uid, rule))
                continue;

            // died to something other than a player
            if (ev.Primary is not KillPlayerSource player)
            {
                continue;
            }

            ProgressPlayerReward(player.PlayerId, gungame);
            SpawnCurrentWeapon(player.PlayerId, gungame);
        }
    }

    /// <summary>
    /// Appends round end text at the end of the round.
    /// </summary>
    protected override void AppendRoundEndText(EntityUid uid, GunGameRuleComponent component, GameRuleComponent gameRule, ref RoundEndTextAppendEvent args)
    {
        if (component.Victor != null && _player.TryGetPlayerData(component.Victor.Value, out var data))
        {
            args.AddLine(Loc.GetString("gun-game-scoreboard-winner", ("player", data.UserName)));
            args.AddLine("");
        }
        args.AddLine(Loc.GetString("gun-game-scoreboard-header"));
        args.AddLine(GetScoreboard(uid, component).ToMarkup());
    }

    /// <summary>
    /// Deletes a GunGameRewardTrackerComponent's gear.
    /// </summary>
    /// <param name="gear">The component with the gear.</param>
    /// <remarks>
    /// Right now this is mostly just a foreach loop which deletes all the entities in the list.
    /// This does trigger an exception in the client if a gun is deleting while it's bullets are still active in the world.
    /// </remarks>
    private void DeleteCurrentWeapons(GunGameRewardTrackerComponent gear)
    {
        foreach (var entity in gear.CurrentRewards)
        {
            QueueDel(entity);
        }
    }

    /// <summary>
    /// Progresses the specified user's reward queue.
    /// If their queue is empty then they've won so we trigger the round end.
    /// </summary>
    /// <param name="userId">The player's user ID</param>
    /// <param name="rule">The GunGameRuleComponent for this gamerule</param>
    private void ProgressPlayerReward(NetUserId userId, GunGameRuleComponent rule)
    {
        if (!rule.PlayerRewards.TryGetValue(userId, out var rewardQueue))
            return;


        if (rewardQueue.Count > 1)
        {
            rewardQueue.Dequeue();
            return;
        }

        rule.Victor = userId;
        _roundEnd.EndRound(rule.RestartDelay);
    }

    /// <summary>
    /// Spawns the player's current weapon in their queue, intended to be used on spawn and when they upgrade weapons.
    /// </summary>
    /// <param name="userId">The player's user ID</param>
    /// <param name="rule">The GunGameRuleComponent for this gamerule</param>
    private void SpawnCurrentWeapon(NetUserId userId, GunGameRuleComponent rule)
    {
        if (!_mind.TryGetMind(userId, out var _, out var mind))
            return;

        if (mind.OwnedEntity is not { } playerEntity)
            return;

        var gear = EnsureComp<GunGameRewardTrackerComponent>(playerEntity);
        DeleteCurrentWeapons(gear);

        if (!rule.PlayerRewards.TryGetValue(userId, out var gearQueue))
        {
            gearQueue = new Queue<EntProtoId>(rule.RewardSpawnsQueue);
            rule.PlayerRewards.Add(userId, gearQueue);
        }

        if (gearQueue.Count == 0) // If we somehow try to spawn somebody's weapon after they win
            return;

        // Peek the player's queue, and spawn the item there
        var newWeapon = Spawn(gearQueue.Peek());

        // Put it in their hands
        _hands.TryForcePickupAnyHand(playerEntity, newWeapon);
        gear.CurrentRewards.Add(newWeapon);
    }

    /// <summary>
    /// Formats the scoreboard for the end of round screen.
    /// </summary>
    /// <param name="uid">The gamemode uid</param>
    /// <param name="component">The GunGameRuleComponent for this gamemode</param>
    /// <returns></returns>
    private FormattedMessage GetScoreboard(EntityUid uid, GunGameRuleComponent? component = null)
    {
        var msg = new FormattedMessage();

        if (!Resolve(uid, ref component))
            return msg;

        var rewardsList = component.PlayerRewards.ToList(); // LINQ my beloved
        var orderedPlayers = rewardsList.OrderBy(p => p.Value.Count).ToList();
        var place = 1;
        foreach (var (id, playerQueue) in orderedPlayers)
        {
            if (!_player.TryGetPlayerData(id, out var data))
                continue;

            msg.AddMarkupOrThrow(Loc.GetString("gun-game-scoreboard-list-entry",
                ("place", place++),
                ("name", data.UserName),
                ("weaponsLeft", playerQueue.Count -1),
                ("weapon", playerQueue.Count == 0 || !_proto.TryIndex(playerQueue.Peek(), out var proto)
                    ? "None"
                    : proto.Name)));
            msg.PushNewline();
        }

        return msg;
    }
}
