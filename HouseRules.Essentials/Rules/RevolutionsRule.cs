namespace HouseRules.Essentials.Rules
{
    using Boardgame;
    using Boardgame.BoardEntities;
    using DataKeys;
    using HarmonyLib;
    using HouseRules.Core;
    using HouseRules.Core.Types;

    public sealed class RevolutionsRule : Rule, IConfigWritable<int>, IPatchable, IMultiplayerSafe
    {
        public override string Description => "A Reloaded/Revolutions style game is enabled";

        private static bool _isActivated;
        private static int _globalGameType;
        private static bool _isReconnect;
        private readonly int _gameType;

        public RevolutionsRule(int gameType)
        {
            _gameType = gameType;
        }

        public int GetConfigObject() => _gameType;

        protected override void OnActivate(Context context)
        {
            _globalGameType = _gameType;
            _isActivated = true;
        }

        protected override void OnDeactivate(Context context)
        {
            _isActivated = false;
            _isReconnect = false;
        }

        private static void Patch(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(Piece), "CreatePiece"),
                postfix: new HarmonyMethod(
                    typeof(RevolutionsRule),
                    nameof(CreatePiece_Revolutions_Postfix)));

            harmony.Patch(
                original: AccessTools.Method(typeof(Inventory), "RestoreReplenishables"),
                prefix: new HarmonyMethod(
                    typeof(RevolutionsRule),
                    nameof(Inventory_RestoreReplenishables_Prefix)));
            harmony.Patch(
                original: AccessTools.Method(typeof(StatusEffect), "Tick"),
                prefix: new HarmonyMethod(
                    typeof(RevolutionsRule),
                    nameof(StatsusEffect_Tick_Prefix)));
        }

        private static void StatsusEffect_Tick_Prefix(ref StatusEffect __instance)
        {
            if (!_isActivated)
            {
                return;
            }

            var pieceId = Traverse.Create(__instance).Field<int>("sourcePieceId").Value;
            var pieceAndTurnController = Traverse.Create(__instance).Field<PieceAndTurnController>("pieceAndTurnController").Value;
            Piece piece = pieceAndTurnController.GetPiece(pieceId);
            if (piece == null)
            {
                return;
            }

            if (!piece.IsPlayer())
            {
                return;
            }

            // mode: Reloaded = 1, Rev_Easy = 2, Rev = 3, Rev_Hard = 4, Rev_Leg = 5, Prog_Small = 6, Prog = 7, Prog_Leg = 8, PointsProg = 9
            int mode = piece.GetStat(Stats.Type.InnateCounterDamageExtraDamage);

            if (mode > 1 && mode < 6 && __instance.effectStateType == EffectStateType.ExtraEnergy)
            {
                Inventory.Item value;
                int howMany = piece.effectSink.GetEffectStateDurationTurnsLeft(EffectStateType.ExtraEnergy);
                bool hasChanged = false;

                // Energy Potion tick/prevention and card removal per class
                if (piece.HasEffectState(EffectStateType.ExtraEnergy))
                {
                    if (piece.boardPieceId == BoardPieceId.HeroBarbarian)
                    {
                        for (int i = 0; i < piece.inventory.Items.Count; i++)
                        {
                            value = piece.inventory.Items[i];
                            if (value.AbilityKey == AbilityKey.ImplosionExplosionRain)
                            {
                                if (value.IsReplenishing)
                                {
                                    hasChanged = true;
                                    howMany -= 1;
                                    if (howMany < 1)
                                    {
                                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value -= 1;
                                        piece.DisableEffectState(EffectStateType.ExtraEnergy);
                                        piece.inventory.Items.Remove(value);
                                    }
                                }

                                break;
                            }
                        }
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroGuardian)
                    {
                        for (int i = 0; i < piece.inventory.Items.Count; i++)
                        {
                            value = piece.inventory.Items[i];
                            if (value.AbilityKey == AbilityKey.LeapHeavy)
                            {
                                if (value.IsReplenishing)
                                {
                                    hasChanged = true;
                                    howMany -= 1;
                                    if (howMany < 1)
                                    {
                                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value -= 1;
                                        piece.DisableEffectState(EffectStateType.ExtraEnergy);
                                        piece.inventory.Items.Remove(value);
                                    }
                                }

                                break;
                            }
                        }
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroHunter)
                    {
                        for (int i = 0; i < piece.inventory.Items.Count; i++)
                        {
                            value = piece.inventory.Items[i];
                            if (value.AbilityKey == AbilityKey.PVPMissileSwarm)
                            {
                                if (value.IsReplenishing)
                                {
                                    hasChanged = true;
                                    howMany -= 1;
                                    if (howMany < 1)
                                    {
                                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value -= 1;
                                        piece.DisableEffectState(EffectStateType.ExtraEnergy);
                                        piece.inventory.Items.Remove(value);
                                    }
                                }

                                break;
                            }
                        }
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroBard)
                    {
                        for (int i = 0; i < piece.inventory.Items.Count; i++)
                        {
                            value = piece.inventory.Items[i];
                            if (value.AbilityKey == AbilityKey.PVPBlink)
                            {
                                if (value.IsReplenishing)
                                {
                                    hasChanged = true;
                                    howMany -= 1;
                                    if (howMany < 1)
                                    {
                                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value -= 1;
                                        piece.DisableEffectState(EffectStateType.ExtraEnergy);
                                        piece.inventory.Items.Remove(value);
                                    }
                                }

                                break;
                            }
                        }
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroSorcerer)
                    {
                        for (int i = 0; i < piece.inventory.Items.Count; i++)
                        {
                            value = piece.inventory.Items[i];
                            if (value.AbilityKey == AbilityKey.DeathBeam)
                            {
                                if (value.IsReplenishing)
                                {
                                    hasChanged = true;
                                    howMany -= 1;
                                    if (howMany < 1)
                                    {
                                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value -= 1;
                                        piece.DisableEffectState(EffectStateType.ExtraEnergy);
                                        piece.inventory.Items.Remove(value);
                                    }
                                }

                                break;
                            }
                        }
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroRogue)
                    {
                        for (int i = 0; i < piece.inventory.Items.Count; i++)
                        {
                            value = piece.inventory.Items[i];
                            if (value.AbilityKey == AbilityKey.PVPFireball)
                            {
                                if (value.IsReplenishing)
                                {
                                    hasChanged = true;
                                    howMany -= 1;
                                    if (howMany < 1)
                                    {
                                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value -= 1;
                                        piece.DisableEffectState(EffectStateType.ExtraEnergy);
                                        piece.inventory.Items.Remove(value);
                                    }
                                }

                                break;
                            }
                        }
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroWarlock)
                    {
                        for (int i = 0; i < piece.inventory.Items.Count; i++)
                        {
                            value = piece.inventory.Items[i];
                            if (value.AbilityKey == AbilityKey.WeakeningShout)
                            {
                                if (value.IsReplenishing)
                                {
                                    hasChanged = true;
                                    howMany -= 1;
                                    if (howMany < 1)
                                    {
                                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value -= 1;
                                        piece.DisableEffectState(EffectStateType.ExtraEnergy);
                                        piece.inventory.Items.Remove(value);
                                    }
                                }

                                break;
                            }
                        }
                    }

                    if (howMany > 0 && !hasChanged)
                    {
                        Traverse.Create(__instance).Field<int>("durationTurnsLeft").Value = howMany + 1;
                        piece.effectSink.SetStatusEffectDuration(EffectStateType.ExtraEnergy, howMany);
                        piece.effectSink.AddStatusEffect(EffectStateType.It, 1);
                    }
                }
            }
            else if (__instance.effectStateType == EffectStateType.PlayerBerserk)
            {
                if (piece.boardPieceId == BoardPieceId.HeroGuardian)
                {
                    piece.effectSink.TryGetStat(Stats.Type.MoveRange, out int myMoveRange);
                    piece.effectSink.TryGetStatMax(Stats.Type.MoveRange, out int myMaxMove);
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.MoveRange, myMoveRange - 3);
                    piece.effectSink.TrySetStatMaxValue(Stats.Type.MoveRange, myMaxMove - 3);
                }
            }
            else if (__instance.effectStateType == EffectStateType.SpawnBuildUp)
            {
                if (piece.boardPieceId == BoardPieceId.HeroWarlock)
                {
                    if (piece.effectSink.GetEffectStateDurationTurnsLeft(EffectStateType.SpawnBuildUp) == 1)
                    {
                        piece.effectSink.TryGetStat(Stats.Type.MagicBonus, out int myMagic);
                        piece.effectSink.TryGetStatMax(Stats.Type.MagicBonus, out int myMaxMagic);
                        piece.effectSink.TrySetStatBaseValue(Stats.Type.MagicBonus, myMagic - 3);
                        piece.effectSink.TrySetStatMaxValue(Stats.Type.MagicBonus, myMaxMagic - 3);
                    }
                }
            }
            else if (__instance.effectStateType == EffectStateType.DeflectionBarrier)
            {
                if (piece.boardPieceId == BoardPieceId.HeroBard)
                {
                    if (piece.effectSink.GetEffectStateDurationTurnsLeft(EffectStateType.DeflectionBarrier) == 1)
                    {
                        piece.effectSink.TryGetStat(Stats.Type.MoveRange, out int myMoveRange);
                        piece.effectSink.TryGetStatMax(Stats.Type.MoveRange, out int myMaxMove);
                        piece.effectSink.TrySetStatBaseValue(Stats.Type.MoveRange, myMoveRange - 3);
                        piece.effectSink.TrySetStatMaxValue(Stats.Type.MoveRange, myMaxMove - 3);
                    }
                }
            }
        }

        private static bool Inventory_RestoreReplenishables_Prefix(Piece piece)
        {
            if (!_isActivated)
            {
                return true;
            }

            if (!piece.IsPlayer())
            {
                return true;
            }

            Inventory.Item value;

            // mode: Reloaded = 1, Rev_Easy = 2, Rev = 3, Rev_Hard = 4, Rev_Leg = 5, Prog_Small = 6, Prog = 7, Prog_Leg = 8, PointsProg = 9
            int mode = piece.GetStat(Stats.Type.InnateCounterDamageExtraDamage);

            // Determine if Host is returning to Revolutions/Reloaded game as Client
            if (mode == 0)
            {
                HouseRulesEssentialsBase.LogWarning($"Reconnect true for piece {piece.boardPieceId}");
                _isReconnect = true;
            }

            // Handle fixing character stats and cards for Revolutions/Reloaded game when the Host reconnects and becomes the Master Client again
            if (_isReconnect && GameStateMachine.IsMasterClient && !piece.IsDead() && mode == 0)
            {
                HouseRulesEssentialsBase.LogWarning($"Regained HOST so fixing player stats/cards for {piece.boardPieceId}...");

                mode = _globalGameType;
                bool rev_progr = false;
                bool reloaded = false;
                if (mode == 1)
                {
                    reloaded = true;
                }
                else if (mode > 5 && mode < 10)
                {
                    rev_progr = true;
                }

                int mage = 0;
                int runner = 0;
                int diff = 0;
                if (mode == 2)
                {
                    diff = 2;
                }
                else if (mode == 3)
                {
                    diff = 1;
                }

                if (piece.boardPieceId == BoardPieceId.HeroSorcerer)
                {
                    mage = 1;
                    if (mode == 1 || mode == 9)
                    {
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.SummonElemental,
                            flags: 0,
                            originalOwner: -1,
                            replenishCooldown: 0));

                        if (mode == 9)
                        {
                           piece.inventory.Items.Add(new Inventory.Item(
                           AbilityKey.Banish,
                           flags: 0,
                           originalOwner: -1,
                           replenishCooldown: 0));
                        }
                    }
                    else
                    {
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.Implosion,
                            flags: 0,
                            originalOwner: -1,
                            replenishCooldown: 0));

                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.Banish,
                            flags: 0,
                            originalOwner: -1,
                            replenishCooldown: 0));
                    }

                    if (!rev_progr)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.Electricity,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 1));

                        if (piece.inventory.HasAbility(AbilityKey.Overcharge))
                        {
                            for (var i = 0; i < piece.inventory.Items.Count; i++)
                            {
                                value = piece.inventory.Items[i];
                                if (value.AbilityKey == AbilityKey.Overcharge)
                                {
                                    piece.inventory.Items.Remove(value);
                                    break;
                                }
                            }
                        }
                    }

                    piece.effectSink.TrySetStatBaseValue(Stats.Type.AttackDamage, 1);
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.CritDamage, 3);
                    if (reloaded)
                    {
                        piece.effectSink.TrySetStatMaxValue(Stats.Type.Health, 11);
                        piece.effectSink.TrySetStatBaseValue(Stats.Type.Health, 11);
                    }
                    else
                    {
                        piece.effectSink.TrySetStatBaseValue(Stats.Type.MagicBonus, 1);
                        piece.effectSink.TrySetStatMaxValue(Stats.Type.Health, 6 + diff);
                        piece.effectSink.TrySetStatBaseValue(Stats.Type.Health, 6 + diff);
                    }
                }
                else if (piece.boardPieceId == BoardPieceId.HeroWarlock)
                {
                    if (piece.inventory.HasAbility(AbilityKey.MinionCharge))
                    {
                        for (var i = 0; i < piece.inventory.Items.Count; i++)
                        {
                            value = piece.inventory.Items[i];
                            if (value.AbilityKey == AbilityKey.MinionCharge)
                            {
                                piece.inventory.Items.Remove(value);
                                break;
                            }
                        }
                    }

                    piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.Deflect,
                            flags: 0,
                            originalOwner: -1,
                            replenishCooldown: 0));

                    if (!reloaded)
                    {
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.Implode,
                            flags: 0,
                            originalOwner: -1,
                            replenishCooldown: 0));

                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.GuidingLight,
                            flags: 0,
                            originalOwner: -1,
                            replenishCooldown: 0));
                    }

                    if (!rev_progr)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.MinionCharge,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 1));
                    }

                    piece.effectSink.TrySetStatBaseValue(Stats.Type.AttackDamage, 2);
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.CritDamage, 5);
                    if (reloaded)
                    {
                        piece.effectSink.TrySetStatMaxValue(Stats.Type.Health, 11);
                        piece.effectSink.TrySetStatBaseValue(Stats.Type.Health, 11);
                    }
                    else
                    {
                        piece.effectSink.TrySetStatMaxValue(Stats.Type.Health, 6 + diff);
                        piece.effectSink.TrySetStatBaseValue(Stats.Type.Health, 6 + diff);
                    }
                }
                else if (piece.boardPieceId == BoardPieceId.HeroHunter)
                {
                    if (!reloaded)
                    {
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.MonsterBait,
                            flags: 0,
                            originalOwner: -1,
                            replenishCooldown: 0));
                    }

                    piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.CallCompanion,
                            flags: 0,
                            originalOwner: -1,
                            replenishCooldown: 0));

                    if (!rev_progr)
                    {
                        for (var i = 0; i < piece.inventory.Items.Count; i++)
                        {
                            value = piece.inventory.Items[i];
                            if (value.AbilityKey == AbilityKey.HunterArrow)
                            {
                                piece.inventory.Items.Remove(value);
                                break;
                            }
                        }

                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.EnemyFireball,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 1));
                    }

                    runner = 1;
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.MoveRange, 5);
                    if (reloaded)
                    {
                        piece.effectSink.TrySetStatMaxValue(Stats.Type.Health, 14);
                        piece.effectSink.TrySetStatBaseValue(Stats.Type.Health, 14);
                    }
                    else
                    {
                        piece.effectSink.TrySetStatBaseValue(Stats.Type.AttackDamage, 2);
                        piece.effectSink.TrySetStatBaseValue(Stats.Type.CritDamage, 5);
                        piece.effectSink.TrySetStatMaxValue(Stats.Type.Health, 7 + diff);
                        piece.effectSink.TrySetStatBaseValue(Stats.Type.Health, 7 + diff);
                    }
                }
                else if (piece.boardPieceId == BoardPieceId.HeroBard)
                {
                    piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.ShatteringVoice,
                            flags: 0,
                            originalOwner: -1,
                            replenishCooldown: 0));

                    if (!reloaded)
                    {
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.NotesOfConfusion,
                            flags: 0,
                            originalOwner: -1,
                            replenishCooldown: 0));
                    }

                    if (!rev_progr)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.Flashbang,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 1));
                    }

                    piece.effectSink.TrySetStatBaseValue(Stats.Type.AttackDamage, 2);
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.CritDamage, 5);
                    if (reloaded)
                    {
                        piece.effectSink.TrySetStatMaxValue(Stats.Type.Health, 12);
                        piece.effectSink.TrySetStatBaseValue(Stats.Type.Health, 12);
                    }
                    else
                    {
                        piece.effectSink.TrySetStatMaxValue(Stats.Type.Health, 7 + diff);
                        piece.effectSink.TrySetStatBaseValue(Stats.Type.Health, 7 + diff);
                    }
                }
                else if (piece.boardPieceId == BoardPieceId.HeroRogue)
                {
                    piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.CursedDagger,
                            flags: 0,
                            originalOwner: -1,
                            replenishCooldown: 0));

                    if (!reloaded)
                    {
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.Flashbang,
                            flags: 0,
                            originalOwner: -1,
                            replenishCooldown: 0));
                    }

                    if (!rev_progr)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.DiseasedBite,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 1));
                    }

                    runner = 1;
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.MoveRange, 5);
                    if (reloaded)
                    {
                        piece.effectSink.TrySetStatBaseValue(Stats.Type.CritDamage, 9);
                        piece.effectSink.TrySetStatMaxValue(Stats.Type.Health, 13);
                        piece.effectSink.TrySetStatBaseValue(Stats.Type.Health, 13);
                    }
                    else
                    {
                        piece.effectSink.TrySetStatBaseValue(Stats.Type.CritDamage, 8);
                        piece.effectSink.TrySetStatMaxValue(Stats.Type.Health, 7 + diff);
                        piece.effectSink.TrySetStatBaseValue(Stats.Type.Health, 7 + diff);
                    }
                }
                else if (piece.boardPieceId == BoardPieceId.HeroBarbarian)
                {
                    if (reloaded)
                    {
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.GrapplingSmash,
                            flags: 0,
                            originalOwner: -1,
                            replenishCooldown: 0));
                    }
                    else
                    {
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.GrapplingTotem,
                            flags: 0,
                            originalOwner: -1,
                            replenishCooldown: 0));

                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.GrapplingPush,
                            flags: 0,
                            originalOwner: -1,
                            replenishCooldown: 0));
                    }

                    if (!rev_progr)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.Net,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 1));
                    }

                    if (reloaded)
                    {
                        piece.effectSink.TrySetStatBaseValue(Stats.Type.AttackDamage, 5);
                        piece.effectSink.TrySetStatBaseValue(Stats.Type.CritDamage, 13);
                        piece.effectSink.TrySetStatMaxValue(Stats.Type.Health, 15);
                        piece.effectSink.TrySetStatBaseValue(Stats.Type.Health, 15);
                    }
                    else
                    {
                        piece.effectSink.TrySetStatBaseValue(Stats.Type.CritDamage, 9);
                        piece.effectSink.TrySetStatMaxValue(Stats.Type.Health, 7 + diff);
                        piece.effectSink.TrySetStatBaseValue(Stats.Type.Health, 7 + diff);
                    }
                }
                else if (piece.boardPieceId == BoardPieceId.HeroGuardian)
                {
                    piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.Charge,
                            flags: 0,
                            originalOwner: -1,
                            replenishCooldown: 0));

                    if (!reloaded)
                    {
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.WarCry,
                            flags: 0,
                            originalOwner: -1,
                            replenishCooldown: 0));
                    }

                    if (!rev_progr)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.Grab,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 1));
                    }

                    if (reloaded)
                    {
                        piece.effectSink.TrySetStatBaseValue(Stats.Type.AttackDamage, 4);
                        piece.effectSink.TrySetStatBaseValue(Stats.Type.CritDamage, 9);
                        piece.effectSink.TrySetStatMaxValue(Stats.Type.Health, 16);
                        piece.effectSink.TrySetStatBaseValue(Stats.Type.Health, 16);
                    }
                    else
                    {
                        piece.effectSink.TrySetStatBaseValue(Stats.Type.CritDamage, 7);
                        piece.effectSink.TrySetStatMaxValue(Stats.Type.Health, 8 + diff);
                        piece.effectSink.TrySetStatBaseValue(Stats.Type.Health, 8 + diff);
                        piece.effectSink.TrySetStatBaseValue(Stats.Type.InnateCounterDamage, 1);
                        piece.effectSink.TrySetStatBaseValue(Stats.Type.InnateCounterDirections, 255);
                    }
                }

                if (!reloaded)
                {
                    piece.effectSink.TrySetStatMaxValue(Stats.Type.MagicBonus, 5 + mage);
                    piece.effectSink.TrySetStatMaxValue(Stats.Type.Strength, 5);
                    piece.effectSink.TrySetStatMaxValue(Stats.Type.Speed, 5 + runner);
                }

                if (rev_progr)
                {
                    if (mode == 5 || mode == 8)
                    {
                        piece.effectSink.TrySetStatBaseValue(Stats.Type.DownedCounter, 3);
                    }
                    else if (mode == 9)
                    {
                        piece.effectSink.TrySetStatBaseValue(Stats.Type.DownedCounter, 2);
                    }
                    else
                    {
                        piece.effectSink.TrySetStatBaseValue(Stats.Type.DownedCounter, 1);
                    }

                    piece.effectSink.TrySetStatMaxValue(Stats.Type.CritChance, 1);
                    piece.EnableEffectState(EffectStateType.Flying);
                    piece.effectSink.SetStatusEffectDuration(EffectStateType.Flying, 1);
                }

                piece.effectSink.TrySetStatBaseValue(Stats.Type.InnateCounterDamageExtraDamage, _globalGameType);
                piece.AddGold(0);
            }

            // Remove One-Time replenishables if used
            if (piece.boardPieceId == BoardPieceId.HeroHunter)
            {
                if (piece.inventory.HasAbility(AbilityKey.EnemyFrostball) || piece.inventory.HasAbility(AbilityKey.Bone))
                {
                    for (var i = 0; i < piece.inventory.Items.Count; i++)
                    {
                        value = piece.inventory.Items[i];
                        if (value.AbilityKey == AbilityKey.EnemyFrostball || value.AbilityKey == AbilityKey.Bone)
                        {
                            if (value.IsReplenishing)
                            {
                                Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value -= 1;
                                piece.inventory.Items.Remove(value);
                                piece.AddGold(0);
                            }
                        }
                    }
                }
            }
            else if (piece.boardPieceId == BoardPieceId.HeroSorcerer)
            {
                if (piece.inventory.HasAbility(AbilityKey.WaterBottle))
                {
                    for (var i = 0; i < piece.inventory.Items.Count; i++)
                    {
                        value = piece.inventory.Items[i];
                        if (value.AbilityKey == AbilityKey.WaterBottle)
                        {
                            if (value.IsReplenishing)
                            {
                                Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value -= 1;
                                piece.inventory.Items.Remove(value);
                                piece.AddGold(0);
                            }

                            break;
                        }
                    }
                }
            }
            else if (piece.boardPieceId == BoardPieceId.HeroWarlock)
            {
                if (piece.inventory.HasAbility(AbilityKey.SpellPower))
                {
                    for (var i = 0; i < piece.inventory.Items.Count; i++)
                    {
                        value = piece.inventory.Items[i];
                        if (value.AbilityKey == AbilityKey.SpellPower)
                        {
                            if (value.IsReplenishing)
                            {
                                Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value -= 1;
                                piece.inventory.Items.Remove(value);
                                piece.AddGold(0);
                            }

                            break;
                        }
                    }
                }
            }
            else if (piece.boardPieceId == BoardPieceId.HeroBard)
            {
                if (piece.inventory.HasAbility(AbilityKey.ScarePowder))
                {
                    for (var i = 0; i < piece.inventory.Items.Count; i++)
                    {
                        value = piece.inventory.Items[i];
                        if (value.AbilityKey == AbilityKey.ScarePowder)
                        {
                            if (value.IsReplenishing)
                            {
                                Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value -= 1;
                                piece.inventory.Items.Remove(value);
                                piece.AddGold(0);
                            }

                            break;
                        }
                    }
                }
            }
            else if (piece.boardPieceId == BoardPieceId.HeroBarbarian)
            {
                if (piece.inventory.HasAbility(AbilityKey.SpawnRandomLamp))
                {
                    for (var i = 0; i < piece.inventory.Items.Count; i++)
                    {
                        value = piece.inventory.Items[i];
                        if (value.AbilityKey == AbilityKey.SpawnRandomLamp)
                        {
                            if (value.IsReplenishing)
                            {
                                Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value -= 1;
                                piece.inventory.Items.Remove(value);
                                piece.AddGold(0);
                            }

                            break;
                        }
                    }
                }
            }

            // Energy Potion cards added per class for all non Points games
            if (mode < 9 && piece.HasEffectState(EffectStateType.ExtraEnergy))
            {
                bool hasPower = false;
                if (piece.boardPieceId == BoardPieceId.HeroBarbarian)
                {
                    for (var i = 0; i < piece.inventory.Items.Count; i++)
                    {
                        value = piece.inventory.Items[i];
                        if (value.AbilityKey == AbilityKey.ImplosionExplosionRain)
                        {
                            hasPower = true;
                            break;
                        }
                    }

                    if (!hasPower)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.ImplosionExplosionRain,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 1));

                        HR.ScheduleBoardSync();
                        piece.AddGold(0);
                    }
                }
                else if (piece.boardPieceId == BoardPieceId.HeroGuardian)
                {
                    for (var i = 0; i < piece.inventory.Items.Count; i++)
                    {
                        value = piece.inventory.Items[i];
                        if (value.AbilityKey == AbilityKey.LeapHeavy)
                        {
                            hasPower = true;
                            break;
                        }
                    }

                    if (!hasPower)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.LeapHeavy,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 1));

                        HR.ScheduleBoardSync();
                        piece.AddGold(0);
                    }
                }
                else if (piece.boardPieceId == BoardPieceId.HeroHunter)
                {
                    for (var i = 0; i < piece.inventory.Items.Count; i++)
                    {
                        value = piece.inventory.Items[i];
                        if (value.AbilityKey == AbilityKey.PVPMissileSwarm)
                        {
                            hasPower = true;
                            break;
                        }

                        if (value.AbilityKey == AbilityKey.Zap)
                        {
                            piece.inventory.Items.Remove(value);
                            Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value -= 1;
                            break;
                        }
                    }

                    if (!hasPower)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.PVPMissileSwarm,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 1));

                        HR.ScheduleBoardSync();
                        piece.AddGold(0);
                    }
                }
                else if (piece.boardPieceId == BoardPieceId.HeroBard)
                {
                    for (var i = 0; i < piece.inventory.Items.Count; i++)
                    {
                        value = piece.inventory.Items[i];
                        if (value.AbilityKey == AbilityKey.PVPBlink)
                        {
                            hasPower = true;
                            break;
                        }
                    }

                    if (!hasPower)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.PVPBlink,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 1));

                        HR.ScheduleBoardSync();
                        piece.AddGold(0);
                    }
                }
                else if (piece.boardPieceId == BoardPieceId.HeroSorcerer)
                {
                    for (var i = 0; i < piece.inventory.Items.Count; i++)
                    {
                        value = piece.inventory.Items[i];
                        if (value.AbilityKey == AbilityKey.DeathBeam)
                        {
                            hasPower = true;
                            break;
                        }
                    }

                    if (!hasPower)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.DeathBeam,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 1));

                        HR.ScheduleBoardSync();
                        piece.AddGold(0);
                    }
                }
                else if (piece.boardPieceId == BoardPieceId.HeroRogue)
                {
                    for (var i = 0; i < piece.inventory.Items.Count; i++)
                    {
                        value = piece.inventory.Items[i];
                        if (value.AbilityKey == AbilityKey.PVPFireball)
                        {
                            hasPower = true;
                            break;
                        }
                    }

                    if (!hasPower)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.PVPFireball,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 1));

                        HR.ScheduleBoardSync();
                        piece.AddGold(0);
                    }
                }
                else if (piece.boardPieceId == BoardPieceId.HeroWarlock)
                {
                    for (var i = 0; i < piece.inventory.Items.Count; i++)
                    {
                        value = piece.inventory.Items[i];
                        if (value.AbilityKey == AbilityKey.WeakeningShout)
                        {
                            hasPower = true;
                            break;
                        }
                    }

                    if (!hasPower)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.WeakeningShout,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 1));

                        HR.ScheduleBoardSync();
                        piece.AddGold(0);
                    }
                }
            }

            return false;
        }

        private static void CreatePiece_Revolutions_Postfix(ref Piece __result)
        {
            if (!_isActivated)
            {
                return;
            }

            int mode = _globalGameType;
            if (!__result.IsPlayer())
            {
                bool rev_progr = false;
                if (mode > 5 && mode < 10)
                {
                    rev_progr = true;
                }

                if (__result.boardPieceId == BoardPieceId.FireElemental || __result.boardPieceId == BoardPieceId.ServantOfAlfaragh)
                {
                    __result.effectSink.AddStatusEffect(EffectStateType.FireImmunity, 99);
                }
                else if (__result.boardPieceId == BoardPieceId.Tornado || __result.boardPieceId == BoardPieceId.GasLamp)
                {
                    __result.effectSink.AddStatusEffect(EffectStateType.Overcharge, 99);
                }
                else if (__result.boardPieceId == BoardPieceId.IceElemental)
                {
                    __result.effectSink.AddStatusEffect(EffectStateType.IceImmunity, 99);
                }
                else if (__result.boardPieceId.ToString().Contains("SummoningRift"))
                {
                    __result.effectSink.AddStatusEffect(EffectStateType.Corruption, 99);
                }
                else if (rev_progr || mode == 5)
                {
                    var gameContext = Traverse.Create(typeof(GameHub)).Field<GameContext>("gameContext").Value;
                    if (gameContext.levelLoaderAndInitializer.GetLevelSequence().CurrentLevelIndex == 3)
                    {
                        if (__result.boardPieceId == BoardPieceId.ReptileMutantWizard || __result.boardPieceId == BoardPieceId.TheUnseen)
                        {
                            __result.effectSink.AddStatusEffect(EffectStateType.MagicShield, 99);
                        }
                        else if (__result.boardPieceId.ToString().Contains("Goblin") || __result.boardPieceId.ToString().Contains("Elven"))
                        {
                            __result.effectSink.AddStatusEffect(EffectStateType.Courageous, 99);
                        }
                    }
                    else if (gameContext.levelLoaderAndInitializer.GetLevelSequence().CurrentLevelIndex == 5)
                    {
                        if (__result.boardPieceId == BoardPieceId.ReptileMutantWizard || __result.boardPieceId == BoardPieceId.TheUnseen)
                        {
                            __result.effectSink.AddStatusEffect(EffectStateType.MagicShield, 99);
                        }
                        else if (__result.boardPieceId.ToString().Contains("The"))
                        {
                            __result.effectSink.AddStatusEffect(EffectStateType.Courageous, 99);
                        }
                        else if (__result.boardPieceId.ToString().Contains("Goblin") || (__result.boardPieceId != BoardPieceId.ElvenQueen && __result.boardPieceId.ToString().Contains("Elven")))
                        {
                            __result.effectSink.AddStatusEffect(EffectStateType.Heroic, 99);
                        }
                        else if (__result.boardPieceId.ToString().Contains("Druid"))
                        {
                            __result.effectSink.AddStatusEffect(EffectStateType.Recovery, 99);
                        }
                    }
                }

                return;
            }

            if (__result.GetStat(Stats.Type.InnateCounterDamageExtraDamage) == 0)
            {
                __result.effectSink.TrySetStatBaseValue(Stats.Type.InnateCounterDamageExtraDamage, _globalGameType);
            }
        }
    }
}
