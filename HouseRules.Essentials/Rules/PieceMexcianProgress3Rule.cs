namespace HouseRules.Essentials.Rules
{
    using System;
    using System.Threading;
    using Boardgame;
    using Boardgame.BoardEntities;
    using Boardgame.SerializableEvents;
    using DataKeys;
    using HarmonyLib;
    using HouseRules.Core.Types;

    public sealed class PieceMexicanProgress3Rule : Rule, IConfigWritable<bool>, IPatchable, IMultiplayerSafe
    {
        public override string Description => "Heroes collectively level up after filling the card energy pool";

        private static Context _context;
        private static bool _isActivated;
        private static bool _dropchest;

        public PieceMexicanProgress3Rule(bool value)
        {
        }

        public bool GetConfigObject() => true;

        protected override void OnActivate(Context context)
        {
            _context = context;
            _isActivated = true;
        }

        protected override void OnDeactivate(Context context) => _isActivated = false;

        private static void Patch(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(Piece), "CreatePiece"),
                postfix: new HarmonyMethod(
                    typeof(PieceMexicanProgress3Rule),
                    nameof(CreatePiece_Progression_Postfix)));

            harmony.Patch(
                original: AccessTools.Method(typeof(SerializableEventQueue), "RespondToRequest"),
                prefix: new HarmonyMethod(
                    typeof(PieceMexicanProgress3Rule),
                    nameof(SerializableEventQueue_RespondToRequest_Prefix)));

            harmony.Patch(
                original: AccessTools.Method(typeof(Inventory), "RestoreReplenishables"),
                prefix: new HarmonyMethod(
                    typeof(PieceMexicanProgress3Rule),
                    nameof(Inventory_RestoreReplenishables_Prefix)));

            harmony.Patch(
                original: AccessTools.Method(typeof(StatusEffect), "Tick"),
                prefix: new HarmonyMethod(
                    typeof(PieceMexicanProgress3Rule),
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
                        if (value.AbilityKey == AbilityKey.DropChest)
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
                        if (value.AbilityKey == AbilityKey.MagicShield)
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
                        if (value.AbilityKey == AbilityKey.Invulnerability)
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
                        if (value.AbilityKey == AbilityKey.SongOfRecovery)
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
                        if (value.AbilityKey == AbilityKey.SongOfResilience)
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
                        if (value.AbilityKey == AbilityKey.Deflect)
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
                        if (value.AbilityKey == AbilityKey.Regroup)
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

        private static void CreatePiece_Progression_Postfix(ref Piece __result)
        {
            if (!_isActivated || !__result.IsPlayer())
            {
                return;
            }

            __result.effectSink.TrySetStatMaxValue(Stats.Type.CritChance, 1);
            __result.EnableEffectState(EffectStateType.Flying);
            __result.effectSink.SetStatusEffectDuration(EffectStateType.Flying, 1);
        }

        private static void SerializableEventQueue_RespondToRequest_Prefix(
            SerializableEventQueue __instance,
            ref SerializableEvent request)
        {
            if (!_isActivated)
            {
                return;
            }

            if (request.type != SerializableEvent.Type.AddCardToPiece)
            {
                return;
            }

            var addCardToPieceEvent = (SerializableEventAddCardToPiece)request;
            var gameContext = Traverse.Create(__instance).Property<GameContext>("gameContext").Value;
            var pieceId = Traverse.Create(addCardToPieceEvent).Field<int>("pieceId").Value;
            var cardSource = Traverse.Create(addCardToPieceEvent).Field<int>("cardSource").Value;

            if (cardSource != (int)MotherTracker.Context.Energy)
            {
                return;
            }

            if (!gameContext.pieceAndTurnController.TryGetPiece(pieceId, out var piece))
            {
                return;
            }

            if (!piece.IsPlayer())
            {
                return;
            }

            Inventory.Item value;
            int nextLevel = piece.GetStatMax(Stats.Type.CritChance);
            piece.effectSink.Heal(2);
            if (piece.HasEffectState(EffectStateType.Downed))
            {
                piece.effectSink.RemoveStatusEffect(EffectStateType.Downed);
                piece.effectSink.RemoveStatusEffect(EffectStateType.Stunned);
                piece.effectSink.RemoveStatusEffect(EffectStateType.Frozen);
            }

            if (piece.GetHealth() < piece.GetMaxHealth())
            {
                piece.DisableEffectState(EffectStateType.Heal);
                piece.EnableEffectState(EffectStateType.Heal, 1);
            }

            if (nextLevel < 10)
            {
                piece.effectSink.TrySetStatMaxValue(Stats.Type.CritChance, nextLevel + 1);
                nextLevel++;
                piece.effectSink.SetStatusEffectDuration(EffectStateType.Flying, nextLevel);

                GameUI.ShowCameraMessage("<color=#F0F312>The party has</color> <color=#00FF00>LEVELED UP</color><color=#F0F312>!</color>", 8);
                if (nextLevel == 2)
                {
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.Speed, piece.GetStat(Stats.Type.Speed) + 1);
                    piece.effectSink.TrySetStatMaxValue(Stats.Type.Speed, piece.GetStatMax(Stats.Type.Speed) + 1);
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.MagicBonus, piece.GetStat(Stats.Type.MagicBonus) + 1);
                    piece.effectSink.TrySetStatMaxValue(Stats.Type.MagicBonus, piece.GetStatMax(Stats.Type.MagicBonus) + 1);
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.Strength, piece.GetStat(Stats.Type.Strength) + 1);
                    piece.effectSink.TrySetStatMaxValue(Stats.Type.Strength, piece.GetStatMax(Stats.Type.Strength) + 1);
                }
                else if (nextLevel == 3)
                {
                    piece.effectSink.TrySetStatMaxValue(Stats.Type.Health, piece.GetMaxHealth() + 1);
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.Health, piece.GetHealth() + 1);

                    if (piece.boardPieceId == BoardPieceId.HeroBarbarian)
                    {
                        var abilityPromise = _context.AbilityFactory.LoadAbility(AbilityKey.LeapHeavy);
                        abilityPromise.OnLoaded(ability =>
                        {
                            ability.costActionPoint = false;
                        });
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroBard)
                    {
                        var abilityPromise = _context.AbilityFactory.LoadAbility(AbilityKey.Regroup);
                        abilityPromise.OnLoaded(ability =>
                        {
                            ability.costActionPoint = false;
                        });
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroGuardian)
                    {
                        var abilityPromise = _context.AbilityFactory.LoadAbility(AbilityKey.Charge);
                        abilityPromise.OnLoaded(ability =>
                        {
                            ability.costActionPoint = false;
                        });
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroRogue)
                    {
                        var abilityPromise = _context.AbilityFactory.LoadAbility(AbilityKey.CursedDagger);
                        abilityPromise.OnLoaded(ability =>
                        {
                            ability.costActionPoint = false;
                        });
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroHunter)
                    {
                        var abilityPromise = _context.AbilityFactory.LoadAbility(AbilityKey.ProximityMine);
                        abilityPromise.OnLoaded(ability =>
                        {
                            ability.costActionPoint = false;
                        });
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroSorcerer)
                    {
                        var abilityPromise = _context.AbilityFactory.LoadAbility(AbilityKey.BeaconOfHealing);
                        abilityPromise.OnLoaded(ability =>
                        {
                            ability.costActionPoint = false;
                        });
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroWarlock)
                    {
                        var abilityPromise = _context.AbilityFactory.LoadAbility(AbilityKey.WallDestroy);
                        abilityPromise.OnLoaded(ability =>
                        {
                            ability.costActionPoint = false;
                        });
                    }
                }
                else if (nextLevel == 4)
                {
                    piece.effectSink.TrySetStatMaxValue(Stats.Type.Health, piece.GetMaxHealth() + 3);
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.Health, piece.GetHealth() + 3);
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.Speed, piece.GetStat(Stats.Type.Speed) + 1);
                    piece.effectSink.TrySetStatMaxValue(Stats.Type.Speed, piece.GetStatMax(Stats.Type.Speed) + 1);
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.MagicBonus, piece.GetStat(Stats.Type.MagicBonus) + 1);
                    piece.effectSink.TrySetStatMaxValue(Stats.Type.MagicBonus, piece.GetStatMax(Stats.Type.MagicBonus) + 1);
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.Strength, piece.GetStat(Stats.Type.Strength) + 1);
                    piece.effectSink.TrySetStatMaxValue(Stats.Type.Strength, piece.GetStatMax(Stats.Type.Strength) + 1);
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.DownedCounter, piece.GetStat(Stats.Type.DownedCounter) - 1);
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.DownedTimer, piece.GetStat(Stats.Type.DownedTimer) + 1);

                    if (piece.boardPieceId == BoardPieceId.HeroBarbarian)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.Grapple,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 9));
                        piece.AddGold(0);
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroBard)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.StrengthenCourage,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 9));
                        piece.AddGold(0);
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroGuardian)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.ReplenishArmor,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 9));
                        piece.AddGold(0);
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroRogue)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.Stealth,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 9));
                        piece.AddGold(0);
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroHunter)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 2;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.HunterArrow,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 9));

                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.HunterArrow,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 9));
                        piece.AddGold(0);
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroSorcerer)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.Zap,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 9));

                        piece.AddGold(0);
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroWarlock)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.MinionCharge,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 9));
                        piece.AddGold(0);
                    }
                }
                else if (nextLevel == 5)
                {
                    piece.effectSink.TrySetStatMaxValue(Stats.Type.Health, piece.GetMaxHealth() - 2);
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.MagicBonus, piece.GetStat(Stats.Type.MagicBonus) - 1);
                    piece.effectSink.TrySetStatMaxValue(Stats.Type.MagicBonus, piece.GetStatMax(Stats.Type.MagicBonus) - 1);
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.Strength, piece.GetStat(Stats.Type.Strength) - 1);
                    piece.effectSink.TrySetStatMaxValue(Stats.Type.Strength, piece.GetStatMax(Stats.Type.Strength) - 1);

                    if (piece.GetHealth() > piece.GetMaxHealth())
                    {
                        piece.effectSink.TrySetStatBaseValue(Stats.Type.Health, piece.GetMaxHealth());
                    }

                    if (piece.boardPieceId == BoardPieceId.HeroBarbarian)
                    {
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.MarkOfVerga,
                            flags: 0,
                            originalOwner: -1,
                            replenishCooldown: 1));
                        piece.AddGold(0);
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroBard)
                    {
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.Tornado,
                            flags: 0,
                            originalOwner: -1,
                            replenishCooldown: 1));
                        piece.AddGold(0);
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroGuardian)
                    {
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.Whirlwind,
                            flags: 0,
                            originalOwner: -1,
                            replenishCooldown: 1));
                        piece.AddGold(0);
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroRogue)
                    {
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.Blink,
                            flags: 0,
                            originalOwner: -1,
                            replenishCooldown: 1));
                        piece.AddGold(0);
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroHunter)
                    {
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.MarkOfAvalon,
                            flags: 0,
                            originalOwner: -1,
                            replenishCooldown: 1));
                        piece.AddGold(0);
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroSorcerer)
                    {
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.Rejuvenation,
                            flags: 0,
                            originalOwner: -1,
                            replenishCooldown: 1));

                        piece.AddGold(0);
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroWarlock)
                    {
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.MissileSwarm,
                            flags: 0,
                            originalOwner: -1,
                            replenishCooldown: 1));
                        piece.AddGold(0);
                    }
                }
                else if (nextLevel == 6)
                {
                    piece.effectSink.TrySetStatMaxValue(Stats.Type.Health, piece.GetMaxHealth() + 4);
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.Health, piece.GetHealth() + 4);
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.MagicBonus, piece.GetStat(Stats.Type.MagicBonus) + 2);
                    piece.effectSink.TrySetStatMaxValue(Stats.Type.MagicBonus, piece.GetStatMax(Stats.Type.MagicBonus) + 2);
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.Strength, piece.GetStat(Stats.Type.Strength) + 2);
                    piece.effectSink.TrySetStatMaxValue(Stats.Type.Strength, piece.GetStatMax(Stats.Type.Strength) + 2);

                    piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.ExtraActionPotion,
                            flags: 0,
                            originalOwner: -1,
                            replenishCooldown: 1));
                    piece.AddGold(0);
                }
                else if (nextLevel == 7)
                {
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.DownedCounter, piece.GetStat(Stats.Type.DownedCounter) - 1);
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.DownedTimer, piece.GetStat(Stats.Type.DownedTimer) + 1);

                    if (!piece.HasEffectState(EffectStateType.ExtraEnergy))
                    {
                        piece.EnableEffectState(EffectStateType.ExtraEnergy, 3);
                    }
                }
                else if (nextLevel == 8)
                {
                    piece.effectSink.TryGetStat(Stats.Type.ActionPoints, out int currentAP);
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.ActionPoints, currentAP + 1);
                }
                else if (nextLevel == 9)
                {
                    piece.effectSink.TrySetStatMaxValue(Stats.Type.Health, piece.GetMaxHealth() - 2);
                    if (piece.GetHealth() > piece.GetMaxHealth())
                    {
                        piece.effectSink.TrySetStatBaseValue(Stats.Type.Health, piece.GetMaxHealth());
                    }

                    piece.effectSink.TrySetStatBaseValue(Stats.Type.Speed, piece.GetStat(Stats.Type.Speed) + 1);
                    piece.effectSink.TrySetStatMaxValue(Stats.Type.Speed, piece.GetStatMax(Stats.Type.Speed) + 1);
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.DownedCounter, piece.GetStat(Stats.Type.DownedCounter) - 1);
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.DownedTimer, piece.GetStat(Stats.Type.DownedTimer) + 1);
                }
                else if (nextLevel == 10)
                {
                    piece.effectSink.TrySetStatMaxValue(Stats.Type.Health, piece.GetMaxHealth() + 1);
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.Health, piece.GetHealth() + 1);
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.Speed, piece.GetStat(Stats.Type.Speed) + 1);
                    piece.effectSink.TrySetStatMaxValue(Stats.Type.Speed, piece.GetStatMax(Stats.Type.Speed) + 1);
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.MagicBonus, piece.GetStat(Stats.Type.MagicBonus) + 1);
                    piece.effectSink.TrySetStatMaxValue(Stats.Type.MagicBonus, piece.GetStatMax(Stats.Type.MagicBonus) + 1);
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.Strength, piece.GetStat(Stats.Type.Strength) + 1);
                    piece.effectSink.TrySetStatMaxValue(Stats.Type.Strength, piece.GetStatMax(Stats.Type.Strength) + 1);

                    if (piece.boardPieceId == BoardPieceId.HeroBarbarian)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.GrapplingSmash,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 2));
                        piece.AddGold(0);
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroBard)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.HymnOfBattle,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 2));
                        piece.AddGold(0);
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroGuardian)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.PiercingSpear,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 2));
                        piece.AddGold(0);
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroRogue)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.DiseasedBite,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 2));
                        piece.AddGold(0);
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroHunter)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.Exterminate,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 2));
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroSorcerer)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.Portal,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 2));

                        piece.AddGold(0);
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroWarlock)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.SpawnElvenSummonerDefenders,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 2));
                        piece.AddGold(0);
                    }
                }
            }

            // Energy Potion cards added per class
            if (piece.HasEffectState(EffectStateType.ExtraEnergy))
            {
                bool hasPower = false;
                if (piece.boardPieceId == BoardPieceId.HeroBarbarian)
                {
                    for (var i = 0; i < piece.inventory.Items.Count; i++)
                    {
                        value = piece.inventory.Items[i];
                        if (value.AbilityKey == AbilityKey.DropChest)
                        {
                            hasPower = true;
                            break;
                        }
                    }

                    if (!hasPower)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.DropChest,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 1));
                        piece.AddGold(0);
                    }
                }
                else if (piece.boardPieceId == BoardPieceId.HeroGuardian)
                {
                    for (var i = 0; i < piece.inventory.Items.Count; i++)
                    {
                        value = piece.inventory.Items[i];
                        if (value.AbilityKey == AbilityKey.MagicShield)
                        {
                            hasPower = true;
                            break;
                        }
                    }

                    if (!hasPower)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.MagicShield,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 1));
                        piece.AddGold(0);
                    }
                }
                else if (piece.boardPieceId == BoardPieceId.HeroHunter)
                {
                    for (var i = 0; i < piece.inventory.Items.Count; i++)
                    {
                        value = piece.inventory.Items[i];
                        if (value.AbilityKey == AbilityKey.Invulnerability)
                        {
                            hasPower = true;
                            break;
                        }
                    }

                    if (!hasPower)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.Invulnerability,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 1));
                        piece.AddGold(0);
                    }
                }
                else if (piece.boardPieceId == BoardPieceId.HeroBard)
                {
                    for (var i = 0; i < piece.inventory.Items.Count; i++)
                    {
                        value = piece.inventory.Items[i];
                        if (value.AbilityKey == AbilityKey.SongOfRecovery)
                        {
                            hasPower = true;
                            break;
                        }
                    }

                    if (!hasPower)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.SongOfRecovery,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 1));
                        piece.AddGold(0);
                    }
                }
                else if (piece.boardPieceId == BoardPieceId.HeroSorcerer)
                {
                    for (var i = 0; i < piece.inventory.Items.Count; i++)
                    {
                        value = piece.inventory.Items[i];
                        if (value.AbilityKey == AbilityKey.SongOfResilience)
                        {
                            hasPower = true;
                            break;
                        }
                    }

                    if (!hasPower)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.SongOfResilience,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 1));
                        piece.AddGold(0);
                    }
                }
                else if (piece.boardPieceId == BoardPieceId.HeroRogue)
                {
                    for (var i = 0; i < piece.inventory.Items.Count; i++)
                    {
                        value = piece.inventory.Items[i];
                        if (value.AbilityKey == AbilityKey.Deflect)
                        {
                            hasPower = true;
                            break;
                        }
                    }

                    if (!hasPower)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.Deflect,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 1));
                        piece.AddGold(0);
                    }
                }
                else if (piece.boardPieceId == BoardPieceId.HeroWarlock)
                {
                    for (var i = 0; i < piece.inventory.Items.Count; i++)
                    {
                        value = piece.inventory.Items[i];
                        if (value.AbilityKey == AbilityKey.Regroup)
                        {
                            hasPower = true;
                            break;
                        }
                    }

                    if (!hasPower)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.Regroup,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 1));
                        piece.AddGold(0);
                    }
                }
            }
        }

        private static bool Inventory_RestoreReplenishables_Prefix(ref bool __result, Piece piece)
        {
            if (!_isActivated)
            {
                return true;
            }

            if (!piece.IsPlayer())
            {
                return true;
            }

            int level = piece.GetStatMax(Stats.Type.CritChance);

            // Removal of level special cards with long cooldowns from player start cards
            Inventory.Item value;
            if (level < 2)
            {
                if (piece.boardPieceId == BoardPieceId.HeroBarbarian)
                {
                    for (var i = 0; i < piece.inventory.Items.Count; i++)
                    {
                        value = piece.inventory.Items[i];
                        if (value.AbilityKey == AbilityKey.Grapple)
                        {
                            piece.inventory.Items.Remove(value);
                            break;
                        }
                    }

                    for (var i = 0; i < piece.inventory.Items.Count; i++)
                    {
                        value = piece.inventory.Items[i];
                        if (value.AbilityKey == AbilityKey.GrapplingSmash)
                        {
                            piece.inventory.Items.Remove(value);
                            break;
                        }
                    }
                }
                else if (piece.boardPieceId == BoardPieceId.HeroBard)
                {
                    for (var i = 0; i < piece.inventory.Items.Count; i++)
                    {
                        value = piece.inventory.Items[i];
                        if (value.AbilityKey == AbilityKey.StrengthenCourage)
                        {
                            piece.inventory.Items.Remove(value);
                            break;
                        }
                    }

                    for (var i = 0; i < piece.inventory.Items.Count; i++)
                    {
                        value = piece.inventory.Items[i];
                        if (value.AbilityKey == AbilityKey.HymnOfBattle)
                        {
                            piece.inventory.Items.Remove(value);
                            break;
                        }
                    }
                }
                else if (piece.boardPieceId == BoardPieceId.HeroGuardian)
                {
                    for (var i = 0; i < piece.inventory.Items.Count; i++)
                    {
                        value = piece.inventory.Items[i];
                        if (value.AbilityKey == AbilityKey.ReplenishArmor)
                        {
                            piece.inventory.Items.Remove(value);
                            break;
                        }
                    }

                    for (var i = 0; i < piece.inventory.Items.Count; i++)
                    {
                        value = piece.inventory.Items[i];
                        if (value.AbilityKey == AbilityKey.PiercingSpear)
                        {
                            piece.inventory.Items.Remove(value);
                            break;
                        }
                    }
                }
                else if (piece.boardPieceId == BoardPieceId.HeroRogue)
                {
                    for (var i = 0; i < piece.inventory.Items.Count; i++)
                    {
                        value = piece.inventory.Items[i];
                        if (value.AbilityKey == AbilityKey.Stealth)
                        {
                            piece.inventory.Items.Remove(value);
                            break;
                        }
                    }

                    for (var i = 0; i < piece.inventory.Items.Count; i++)
                    {
                        value = piece.inventory.Items[i];
                        if (value.AbilityKey == AbilityKey.DiseasedBite)
                        {
                            piece.inventory.Items.Remove(value);
                            break;
                        }
                    }
                }
                else if (piece.boardPieceId == BoardPieceId.HeroHunter)
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

                    for (var i = 0; i < piece.inventory.Items.Count; i++)
                    {
                        value = piece.inventory.Items[i];
                        if (value.AbilityKey == AbilityKey.Exterminate)
                        {
                            piece.inventory.Items.Remove(value);
                            break;
                        }
                    }
                }
                else if (piece.boardPieceId == BoardPieceId.HeroSorcerer)
                {
                    for (var i = 0; i < piece.inventory.Items.Count; i++)
                    {
                        value = piece.inventory.Items[i];
                        if (value.AbilityKey == AbilityKey.Zap)
                        {
                            piece.inventory.Items.Remove(value);
                            break;
                        }
                    }

                    for (var i = 0; i < piece.inventory.Items.Count; i++)
                    {
                        value = piece.inventory.Items[i];
                        if (value.AbilityKey == AbilityKey.Portal)
                        {
                            piece.inventory.Items.Remove(value);
                            break;
                        }
                    }
                }
                else if (piece.boardPieceId == BoardPieceId.HeroWarlock)
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

                    for (var i = 0; i < piece.inventory.Items.Count; i++)
                    {
                        value = piece.inventory.Items[i];
                        if (value.AbilityKey == AbilityKey.SpawnElvenSummonerDefenders)
                        {
                            piece.inventory.Items.Remove(value);
                            break;
                        }
                    }
                }

                piece.AddGold(0);
            }

            // Extra Actions added when level 8 or 9 only.
            if (level > 7 && level < 10)
            {
                piece.effectSink.TryGetStat(Stats.Type.ActionPoints, out int currentAP);
                piece.effectSink.TrySetStatBaseValue(Stats.Type.ActionPoints, currentAP + 1);
            }

            return false;
        }
    }
}
