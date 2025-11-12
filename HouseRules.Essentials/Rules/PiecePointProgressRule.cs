namespace HouseRules.Essentials.Rules
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Boardgame;
    using Boardgame.BoardEntities;
    using Boardgame.BoardEntities.Abilities;
    using Boardgame.BoardEntities.AI;
    using Boardgame.BoardgameActions;
    using Boardgame.Data;
    using Boardgame.TurnOrder;
    using DataKeys;
    using HarmonyLib;
    using HouseRules.Core.Types;

    public sealed class PiecePointProgressRule : Rule, IConfigWritable<PiecePointProgressRule.Points>, IPatchable, IMultiplayerSafe, IDisableOnReconnect
    {
        public override string Description => "Each Hero gains experience and levels up based on their actions";

        internal static Points _globalConfig;

        private static List<Piece> _playerPieces;
        private static Context _context;
        private static bool _isActivated;
        private static bool _dropchest;
        private static Piece? tempPiece = null;
        private static float percentage;

        internal static int Player1 { get; private set; }

        internal static int Player2 { get; private set; }

        internal static int Player3 { get; private set; }

        internal static int Player4 { get; private set; }

        private readonly Points _config;

        public struct Points
        {
            public int KillEnemy;
            public int HurtEnemy;
            public int KillPlayer; // set to positive for PVP, or negative for Co-op.
            public int HurtPlayer; // set to positive for PVP, or negative for Co-op.
            public int BuffPlayer; // set to positive for PVP and Co-op.
            public int Keyholder;
            public int UnlockDoor;
            public int HurtBoss;
            public int KillBoss;
            public int HurtSelf;
            public int KillSelf;
            public int LootGold;
            public int LootChest;
            public int LootStand;
            public int OpenDoor;
            public int UseFountain;
            public int RevivePlayer;
            public bool PVPisOn; // set for PVP = true, or Co-op = false.
            public int Points4Minions; // 0 for none. 1 for Cana. 2 for Arly. 3 for Cana and Arly. 4 for ALL
            public float LevelPercentage;
        }

        public enum PointsKeys
        {
            MinionsNone = 0,
            MinionsOnlyCana = 1,
            MinionsOnlyArly = 2,
            MinionsBoth = 3,
            MinionsALL = 4,
        }

        public PiecePointProgressRule(Points points)
        {
            _config = points;
        }

        public Points GetConfigObject() => _config;

        protected override void OnActivate(Context context)
        {
            _globalConfig = _config;
            percentage = _globalConfig.LevelPercentage;
            _context = context;
            _isActivated = true;
        }

        protected override void OnDeactivate(Context context)
        {
            _isActivated = false;
        }

        private static void Patch(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(Piece), "CreatePiece"),
                postfix: new HarmonyMethod(
                    typeof(PiecePointProgressRule),
                    nameof(Piece_CreatePiece_Postfix)));

            harmony.Patch(
                original: AccessTools.Method(typeof(Ability), "GenerateAttackDamage"),
                postfix: new HarmonyMethod(
                    typeof(PiecePointProgressRule),
                    nameof(Ability_GenerateAttackDamage_Postfix)));

            harmony.Patch(
                original: AccessTools.Method(typeof(MotherTracker), "TrackUnitDefeated"),
                prefix: new HarmonyMethod(
                    typeof(PiecePointProgressRule),
                    nameof(MotherTracker_TrackUnitDefeated_Prefix)));

            harmony.Patch(
                original: AccessTools.Method(typeof(Interactable), "OnInteraction", new[] { typeof(int), typeof(IntPoint2D), typeof(GameContext), typeof(int) }),
                prefix: new HarmonyMethod(
                    typeof(PiecePointProgressRule),
                    nameof(Interactable_OnInteraction_Prefix)));

            harmony.Patch(
                original: AccessTools.Method(typeof(MotherTracker), "TrackRevive"),
                prefix: new HarmonyMethod(
                    typeof(PiecePointProgressRule),
                    nameof(MotherTracker_TrackRevive_Prefix)));

            harmony.Patch(
                original: AccessTools.Constructor(typeof(BoardgameActionPiecePickup), new[] { typeof(GameContext), typeof(int), typeof(IntPoint2D), typeof(int), typeof(int) }),
                prefix: new HarmonyMethod(
                    typeof(PiecePointProgressRule),
                    nameof(BoardgameActionPiecePickup_Prefix)));

            harmony.Patch(
                original: AccessTools.Method(typeof(Inventory), "RestoreReplenishables"),
                prefix: new HarmonyMethod(
                    typeof(PiecePointProgressRule),
                    nameof(Inventory_RestoreReplenishables_Prefix)));

            harmony.Patch(
                original: AccessTools.Constructor(typeof(RearrangePlayerTurnOrder), new[] { typeof(TurnQueue) }),
                postfix: new HarmonyMethod(
                    typeof(PiecePointProgressRule),
                    nameof(RearrangePlayerTurnOrder_Constructor_Postfix)));
        }

        private static void RearrangePlayerTurnOrder_Constructor_Postfix(
            RearrangePlayerTurnOrder __instance,
            TurnQueue turnQueue)
        {
            if (!_isActivated)
            {
                return;
            }

            _playerPieces = turnQueue.GetPlayerPieces();
        }

        public static class RandomProvider
        {
            private static int seed = Environment.TickCount;

            private static ThreadLocal<Random> randomWrapper = new ThreadLocal<Random>(
                () => new Random(Interlocked.Increment(ref seed)));

            public static Random GetThreadRandom()
            {
                return randomWrapper.Value;
            }
        }

        private static void Piece_LevelUp(Piece piece, float pointCount)
        {
            if (!_isActivated)
            {
                return;
            }

            if (!piece.IsPlayer())
            {
                return;
            }

            if (pointCount > 999)
            {
                pointCount -= 1000;
            }

            bool levelUp = false;
            int pointCount2 = (int)Math.Round(pointCount);
            if (pointCount2 > 99)
            {
                levelUp = true;
                pointCount2 -= 100;
            }

            piece.effectSink.SetStatusEffectDuration(EffectStateType.StrengthInNumbers, pointCount2);
            if (!levelUp)
            {
                return;
            }

            Inventory.Item value;
            piece.effectSink.Heal(2);
            int nextLevel = piece.GetStatMax(Stats.Type.CritChance);
            if (piece.HasEffectState(EffectStateType.Downed))
            {
                piece.effectSink.RemoveStatusEffect(EffectStateType.Downed);
                piece.effectSink.RemoveStatusEffect(EffectStateType.Stunned);
                piece.effectSink.RemoveStatusEffect(EffectStateType.Frozen);
            }

            if (!piece.HasEffectState(EffectStateType.ExtraEnergy))
            {
                piece.EnableEffectState(EffectStateType.ExtraEnergy, 1);
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
                var charType = piece.boardPieceId;
                string textName = "Player";
                switch (charType)
                {
                    case BoardPieceId.HeroGuardian:
                        textName = "Guardian";
                        break;
                    case BoardPieceId.HeroHunter:
                        textName = "Hunter";
                        break;
                    case BoardPieceId.HeroRogue:
                        textName = "Assassin";
                        break;
                    case BoardPieceId.HeroSorcerer:
                        textName = "Sorcerer";
                        break;
                    case BoardPieceId.HeroBard:
                        textName = "Bard";
                        break;
                    case BoardPieceId.HeroWarlock:
                        textName = "Warlock";
                        break;
                    case BoardPieceId.HeroBarbarian:
                        textName = "Barbarian";
                        break;
                }

                GameUI.ShowCameraMessage($"<color=#F0F312>The </color><b>{textName}</b> <color=#F0F312>has</color> <color=#00FF00>LEVELED UP</color><color=#F0F312>!</color>", 8);
                if (nextLevel == 3)
                {
                    piece.effectSink.TrySetStatMaxValue(Stats.Type.Health, piece.GetMaxHealth() + 1);
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.Health, piece.GetHealth() + 1);
                }
                else if (nextLevel == 6)
                {
                    piece.effectSink.TrySetStatMaxValue(Stats.Type.Health, piece.GetMaxHealth() + 2);
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.Health, piece.GetHealth() + 2);
                }
                else if (nextLevel == 4)
                {
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.DownedCounter, piece.GetStat(Stats.Type.DownedCounter) - 1);
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.DownedTimer, piece.GetStat(Stats.Type.DownedTimer) + 1);
                }
                else if (nextLevel == 8)
                {
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.DownedCounter, piece.GetStat(Stats.Type.DownedCounter) - 1);
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.DownedTimer, piece.GetStat(Stats.Type.DownedTimer) + 1);
                    int randAbil = RandomProvider.GetThreadRandom().Next(5);
                    if (randAbil == 4 && _dropchest)
                    {
                        randAbil = RandomProvider.GetThreadRandom().Next(4);
                    }

                    if (randAbil == 0)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.Petrify,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 7));
                    }
                    else if (randAbil == 1)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.AcidSpit,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 7));
                    }
                    else if (randAbil == 2)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.WaterExplosion,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 7));
                    }
                    else if (randAbil == 3)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.Shockwave,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 7));
                    }
                    else if (randAbil == 4)
                    {
                        _dropchest = true;
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.DropChest,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 7));
                    }

                    piece.AddGold(0);
                }
                else if (nextLevel == 2)
                {
                    if (piece.boardPieceId == BoardPieceId.HeroBarbarian)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.Net,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 3));
                        piece.AddGold(0);

                        var abilityPromise = _context.AbilityFactory.LoadAbility(AbilityKey.Grapple);
                        abilityPromise.OnLoaded(ability =>
                        {
                            ability.costActionPoint = false;
                        });
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroBard)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.EnemyFlashbang,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 3));
                        piece.AddGold(0);

                        var abilityPromise = _context.AbilityFactory.LoadAbility(AbilityKey.StrengthenCourage);
                        abilityPromise.OnLoaded(ability =>
                        {
                            ability.costActionPoint = false;
                        });
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroGuardian)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.Grab,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 1));
                        piece.AddGold(0);
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroRogue)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.DiseasedBite,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 3));
                        piece.AddGold(0);

                        var abilityPromise = _context.AbilityFactory.LoadAbility(AbilityKey.Stealth);
                        abilityPromise.OnLoaded(ability =>
                        {
                            ability.costActionPoint = false;
                        });
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

                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.EnemyFireball,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 1));
                        piece.AddGold(0);

                        var abilityPromise = _context.AbilityFactory.LoadAbility(AbilityKey.HunterArrow);
                        abilityPromise.OnLoaded(ability =>
                        {
                            ability.costActionPoint = false;
                        });
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroSorcerer)
                    {
                        int overcharge = 0;
                        if (piece.HasEffectState(EffectStateType.Overcharge))
                        {
                            overcharge = piece.effectSink.GetEffectStateDurationTurnsLeft(EffectStateType.Overcharge);
                            piece.effectSink.RemoveStatusEffect(EffectStateType.Overcharge);
                        }

                        for (var i = 0; i < piece.inventory.Items.Count; i++)
                        {
                            value = piece.inventory.Items[i];
                            if (value.AbilityKey == AbilityKey.Overcharge)
                            {
                                if (value.IsReplenishing)
                                {
                                    value.flags &= (Inventory.ItemFlag)(-3);
                                    piece.inventory.Items[i] = value;
                                }

                                piece.inventory.Items.Remove(value);
                                break;
                            }
                        }

                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.Electricity,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 1));

                        if (overcharge > 0)
                        {
                            piece.effectSink.AddStatusEffect(EffectStateType.Overcharge);
                            piece.effectSink.SetStatusEffectDuration(EffectStateType.Overcharge, overcharge);
                        }

                        piece.AddGold(0);

                        var abilityPromise = _context.AbilityFactory.LoadAbility(AbilityKey.Zap);
                        abilityPromise.OnLoaded(ability =>
                        {
                            ability.costActionPoint = false;
                        });

                        var abilityPromise2 = _context.AbilityFactory.LoadAbility(AbilityKey.LightningBolt);
                        abilityPromise2.OnLoaded(ability =>
                        {
                            ability.costActionPoint = false;
                        });
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroWarlock)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.MinionCharge,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 1));
                        piece.AddGold(0);

                        var abilityPromise = _context.AbilityFactory.LoadAbility(AbilityKey.MinionCharge);
                        abilityPromise.OnLoaded(ability =>
                        {
                            ability.costActionPoint = false;
                        });
                    }
                }
                else if (nextLevel == 5)
                {
                    if (piece.boardPieceId == BoardPieceId.HeroSorcerer || piece.boardPieceId == BoardPieceId.HeroWarlock)
                    {
                        piece.effectSink.TrySetStatBaseValue(Stats.Type.MagicBonus, piece.GetStat(Stats.Type.MagicBonus) + 1);
                        piece.effectSink.TrySetStatMaxValue(Stats.Type.MagicBonus, piece.GetStatMax(Stats.Type.MagicBonus) + 1);
                    }
                    else
                    {
                        piece.effectSink.TrySetStatBaseValue(Stats.Type.Strength, piece.GetStat(Stats.Type.Strength) + 1);
                        piece.effectSink.TrySetStatMaxValue(Stats.Type.Strength, piece.GetStatMax(Stats.Type.Strength) + 1);
                    }
                }
                else if (nextLevel == 10)
                {
                    if (piece.boardPieceId == BoardPieceId.HeroSorcerer || piece.boardPieceId == BoardPieceId.HeroWarlock)
                    {
                        piece.effectSink.TrySetStatBaseValue(Stats.Type.MagicBonus, piece.GetStat(Stats.Type.MagicBonus) + 2);
                        piece.effectSink.TrySetStatMaxValue(Stats.Type.MagicBonus, piece.GetStatMax(Stats.Type.MagicBonus) + 2);
                    }
                    else
                    {
                        piece.effectSink.TrySetStatBaseValue(Stats.Type.Strength, piece.GetStat(Stats.Type.Strength) + 2);
                        piece.effectSink.TrySetStatMaxValue(Stats.Type.Strength, piece.GetStatMax(Stats.Type.Strength) + 2);
                    }

                    piece.effectSink.TryGetStat(Stats.Type.ActionPoints, out int currentAP);
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.ActionPoints, currentAP + 1);
                }
                else if (nextLevel == 7)
                {
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.Speed, piece.GetStat(Stats.Type.Speed) + 2);
                    piece.effectSink.TrySetStatMaxValue(Stats.Type.Speed, piece.GetStatMax(Stats.Type.Speed) + 2);
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

            // Health Regeneration, Extra Actions, and Action Point cost changes per character class
            if (piece.HasEffectState(EffectStateType.ConfusedPermanentVisualOnly))
            {
                piece.DisableEffectState(EffectStateType.ConfusedPermanentVisualOnly);
                piece.DisableEffectState(EffectStateType.Corruption);
            }

            int level = piece.GetStatMax(Stats.Type.CritChance);
            if (level > 9)
            {
                piece.effectSink.TryGetStat(Stats.Type.ActionPoints, out int currentAP);
                piece.effectSink.TrySetStatBaseValue(Stats.Type.ActionPoints, currentAP + 1);
            }

            if (level > 8 && piece.GetHealth() > 0 && piece.GetHealth() < piece.GetMaxHealth())
            {
                if (!piece.HasEffectState(EffectStateType.Downed) && !piece.HasEffectState(EffectStateType.Diseased) && !piece.HasEffectState(EffectStateType.Petrified))
                {
                    piece.effectSink.Heal(1);
                    piece.DisableEffectState(EffectStateType.Heal);
                    piece.EnableEffectState(EffectStateType.Heal, 1);
                }
            }

            if (level < 2)
            {
                if (piece.boardPieceId == BoardPieceId.HeroBarbarian)
                {
                    var abilityPromise = _context.AbilityFactory.LoadAbility(AbilityKey.Grapple);
                    abilityPromise.OnLoaded(ability =>
                    {
                        ability.costActionPoint = true;
                    });
                }
                else if (piece.boardPieceId == BoardPieceId.HeroBard)
                {
                    var abilityPromise = _context.AbilityFactory.LoadAbility(AbilityKey.StrengthenCourage);
                    abilityPromise.OnLoaded(ability =>
                    {
                        ability.costActionPoint = true;
                    });
                }
                else if (piece.boardPieceId == BoardPieceId.HeroRogue)
                {
                    var abilityPromise = _context.AbilityFactory.LoadAbility(AbilityKey.Stealth);
                    abilityPromise.OnLoaded(ability =>
                    {
                        ability.costActionPoint = true;
                    });
                }
                else if (piece.boardPieceId == BoardPieceId.HeroSorcerer)
                {
                    var abilityPromise = _context.AbilityFactory.LoadAbility(AbilityKey.Zap);
                    abilityPromise.OnLoaded(ability =>
                    {
                        ability.costActionPoint = true;
                    });

                    var abilityPromise2 = _context.AbilityFactory.LoadAbility(AbilityKey.LightningBolt);
                    abilityPromise2.OnLoaded(ability =>
                    {
                        ability.costActionPoint = true;
                    });
                }
                else if (piece.boardPieceId == BoardPieceId.HeroHunter)
                {
                    var abilityPromise = _context.AbilityFactory.LoadAbility(AbilityKey.HunterArrow);
                    abilityPromise.OnLoaded(ability =>
                    {
                        ability.costActionPoint = true;
                    });

                    var abilityPromise2 = _context.AbilityFactory.LoadAbility(AbilityKey.LightningBolt);
                    abilityPromise2.OnLoaded(ability =>
                    {
                        ability.costActionPoint = false;
                    });
                }
                else if (piece.boardPieceId == BoardPieceId.HeroWarlock)
                {
                    var abilityPromise = _context.AbilityFactory.LoadAbility(AbilityKey.MinionCharge);
                    abilityPromise.OnLoaded(ability =>
                    {
                        ability.costActionPoint = true;
                    });
                }
            }
            else
            {
                if (piece.boardPieceId == BoardPieceId.HeroBarbarian)
                {
                    var abilityPromise = _context.AbilityFactory.LoadAbility(AbilityKey.Grapple);
                    abilityPromise.OnLoaded(ability =>
                    {
                        ability.costActionPoint = false;
                    });
                }
                else if (piece.boardPieceId == BoardPieceId.HeroBard)
                {
                    var abilityPromise = _context.AbilityFactory.LoadAbility(AbilityKey.StrengthenCourage);
                    abilityPromise.OnLoaded(ability =>
                    {
                        ability.costActionPoint = false;
                    });
                }
                else if (piece.boardPieceId == BoardPieceId.HeroRogue)
                {
                    var abilityPromise = _context.AbilityFactory.LoadAbility(AbilityKey.Stealth);
                    abilityPromise.OnLoaded(ability =>
                    {
                        ability.costActionPoint = false;
                    });
                }
                else if (piece.boardPieceId == BoardPieceId.HeroSorcerer)
                {
                    var abilityPromise = _context.AbilityFactory.LoadAbility(AbilityKey.Zap);
                    abilityPromise.OnLoaded(ability =>
                    {
                        ability.costActionPoint = false;
                    });

                    var abilityPromise2 = _context.AbilityFactory.LoadAbility(AbilityKey.LightningBolt);
                    abilityPromise2.OnLoaded(ability =>
                    {
                        ability.costActionPoint = false;
                    });
                }
                else if (piece.boardPieceId == BoardPieceId.HeroHunter)
                {
                    var abilityPromise = _context.AbilityFactory.LoadAbility(AbilityKey.HunterArrow);
                    abilityPromise.OnLoaded(ability =>
                    {
                        ability.costActionPoint = false;
                    });

                    var abilityPromise2 = _context.AbilityFactory.LoadAbility(AbilityKey.LightningBolt);
                    abilityPromise2.OnLoaded(ability =>
                    {
                        ability.costActionPoint = false;
                    });
                }
                else if (piece.boardPieceId == BoardPieceId.HeroWarlock)
                {
                    var abilityPromise = _context.AbilityFactory.LoadAbility(AbilityKey.MinionCharge);
                    abilityPromise.OnLoaded(ability =>
                    {
                        ability.costActionPoint = false;
                    });
                }
            }

            return false;
        }

        private static void BoardgameActionPiecePickup_Prefix(GameContext gameContext, int pieceId)
        {
            if (!_isActivated)
            {
                return;
            }

            if (!gameContext.pieceAndTurnController.TryGetPiece(pieceId, out var piece))
            {
                return;
            }

            if (!piece.IsPlayer())
            {
                Piece piece2;
                if (piece.boardPieceId == BoardPieceId.WarlockMinion)
                {
                    PieceAI pieceAI = piece.pieceAI;
                    if (pieceAI == null)
                    {
                        return;
                    }
                    else if (pieceAI.memory.TryGetAssociatedPiece(gameContext.pieceAndTurnController, out piece2))
                    {
                        piece = piece2;
                    }
                }
                else if (piece.boardPieceId == BoardPieceId.SellswordArbalestierActive)
                {
                    PieceAI pieceAI = piece.pieceAI;
                    if (pieceAI == null)
                    {
                        return;
                    }
                    else if (pieceAI.memory.TryGetAssociatedPiece(gameContext.pieceAndTurnController, out piece2))
                    {
                        piece = piece2;
                    }
                }
            }

            if (!piece.IsPlayer())
            {
                return;
            }

            var pointCount = piece.effectSink.GetEffectStateDurationTurnsLeft(EffectStateType.StrengthInNumbers);
            var addPoints = 0;
            if (pointCount > 998)
            {
                pointCount = 0;
            }

            addPoints += _globalConfig.LootGold;
            if (piece.HasEffectState(EffectStateType.Key))
            {
                addPoints += _globalConfig.Keyholder;
            }

            if (addPoints < 0)
            {
                addPoints = 0;
            }

            if (addPoints != 0)
            {
                Piece_LevelUp(piece, pointCount + (addPoints * percentage));
            }
        }

        private static void MotherTracker_TrackRevive_Prefix(Piece revivedPiece, Piece sourcePiece)
        {
            if (!_isActivated)
            {
                return;
            }

            if (revivedPiece == sourcePiece)
            {
                return;
            }

            var pointCount = sourcePiece.effectSink.GetEffectStateDurationTurnsLeft(EffectStateType.StrengthInNumbers);
            var addPoints = 0;
            if (pointCount > 998)
            {
                pointCount = 0;
            }

            addPoints += _globalConfig.RevivePlayer;

            if (addPoints < 0)
            {
                addPoints = 0;
            }

            if (addPoints != 0)
            {
                Piece_LevelUp(sourcePiece, pointCount + (addPoints * percentage));
            }

            pointCount = revivedPiece.effectSink.GetEffectStateDurationTurnsLeft(EffectStateType.StrengthInNumbers);
            addPoints = 0;
            if (pointCount > 998)
            {
                pointCount = 0;
            }

            // only "steal" points in PVP.
            if (_globalConfig.PVPisOn)
            {
                addPoints -= _globalConfig.RevivePlayer;
                if (addPoints < 0)
                {
                    addPoints = 0;
                }

                if (addPoints != 0)
                {
                    Piece_LevelUp(revivedPiece, pointCount + (addPoints * percentage));
                }
            }
        }

        private static void Interactable_OnInteraction_Prefix(
            int pieceId,
            GameContext gameContext,
            IntPoint2D targetTile)
        {
            if (!_isActivated)
            {
                return;
            }

            if (!gameContext.pieceAndTurnController.GetInteractableAtPosition(targetTile))
            {
                return;
            }

            var interactable = gameContext.pieceAndTurnController.GetInteractableAtPosition(targetTile);
            if (!gameContext.pieceAndTurnController.TryGetPiece(pieceId, out var piece))
            {
                return;
            }

            if (!piece.IsPlayer())
            {
                return;
            }

            var pointCount = piece.effectSink.GetEffectStateDurationTurnsLeft(EffectStateType.StrengthInNumbers);
            var addPoints = 0;
            if (pointCount > 998)
            {
                pointCount = 0;
            }

            if (interactable.type == Interactable.Type.LevelExit)
            {
                if (!piece.HasEffectState(EffectStateType.Key))
                {
                    return;
                }

                if (piece.HasEffectState(EffectStateType.Locked))
                {
                    addPoints += _globalConfig.UnlockDoor;
                }
            }
            else if (interactable.type == Interactable.Type.Chest)
            {
                addPoints += _globalConfig.LootChest;
            }
            else if (interactable.type == Interactable.Type.PotionStand)
            {
                addPoints += _globalConfig.LootStand;
            }
            else if (interactable.type == Interactable.Type.Door)
            {
                addPoints += _globalConfig.OpenDoor;
            }
            else if (interactable.type == Interactable.Type.AltarOfBlessing)
            {
                addPoints += _globalConfig.UseFountain;
            }

            if (piece.HasEffectState(EffectStateType.Key) && interactable.type != Interactable.Type.LevelExit)
            {
                addPoints += _globalConfig.Keyholder;
            }

            if (addPoints < 0)
            {
                addPoints = 0;
            }

            if (addPoints != 0)
            {
                Piece_LevelUp(piece, pointCount + (addPoints * percentage));
            }
        }

        private static void MotherTracker_TrackUnitDefeated_Prefix(Piece defeatedUnit, Piece attackerUnit)
        {
            if (!_isActivated)
            {
                return;
            }

            if (tempPiece != null)
            {
                if (attackerUnit != null && !attackerUnit.IsPlayer())
                {
                    attackerUnit = tempPiece;
                }
                else
                {
                    tempPiece = null;
                }
            }

            if (attackerUnit == null || defeatedUnit.HasEffectState(EffectStateType.WizardDoppelganger))
            {
                return;
            }

            if (attackerUnit.IsPlayer() && defeatedUnit.HasPieceType(PieceType.Prop) && defeatedUnit.ToString().Contains("Lamp"))
            {
                tempPiece = attackerUnit;
                return;
            }

            if (!attackerUnit.IsPlayer())
            {
                Piece piece2;
                PieceAI pieceAI = attackerUnit.pieceAI;
                var gameContext = Traverse.Create(typeof(GameHub)).Field<GameContext>("gameContext").Value;
                if (attackerUnit.boardPieceId == BoardPieceId.WarlockMinion && (_globalConfig.Points4Minions == (int)PointsKeys.MinionsOnlyCana || _globalConfig.Points4Minions > 2))
                {
                    if (pieceAI == null)
                    {
                        return;
                    }
                    else if (pieceAI.memory.TryGetAssociatedPiece(gameContext.pieceAndTurnController, out piece2))
                    {
                        attackerUnit = piece2;
                    }
                }
                else if (attackerUnit.boardPieceId == BoardPieceId.SellswordArbalestierActive && (_globalConfig.Points4Minions == (int)PointsKeys.MinionsOnlyArly || _globalConfig.Points4Minions > 2))
                {
                    if (pieceAI == null)
                    {
                        return;
                    }
                    else if (pieceAI.memory.TryGetAssociatedPiece(gameContext.pieceAndTurnController, out piece2))
                    {
                        attackerUnit = piece2;
                    }
                }
                else if (_globalConfig.Points4Minions > 3)
                {
                    if (attackerUnit.boardPieceId == BoardPieceId.Verochka && !attackerUnit.HasEffectState(EffectStateType.ConfusedPermanentVisualOnly))
                    {
                        foreach (var piece in _playerPieces)
                        {
                            if (piece.boardPieceId == BoardPieceId.HeroHunter)
                            {
                                attackerUnit = piece;
                                break;
                            }
                        }
                    }
                    else if (attackerUnit.boardPieceId == BoardPieceId.Tornado)
                    {
                        foreach (var piece in _playerPieces)
                        {
                            if (piece.boardPieceId == BoardPieceId.HeroBard)
                            {
                                attackerUnit = piece;
                                break;
                            }
                        }
                    }
                    else if (attackerUnit.boardPieceId == BoardPieceId.GrapplingTotem)
                    {
                        foreach (var piece in _playerPieces)
                        {
                            if (piece.boardPieceId == BoardPieceId.HeroBarbarian)
                            {
                                attackerUnit = piece;
                                break;
                            }
                        }
                    }
                    else if (attackerUnit.boardPieceId == BoardPieceId.SwordOfAvalon)
                    {
                        foreach (var piece in _playerPieces)
                        {
                            if (piece.boardPieceId == BoardPieceId.HeroRogue)
                            {
                                attackerUnit = piece;
                                break;
                            }
                        }
                    }
                    else if (attackerUnit.boardPieceId == BoardPieceId.SmiteWard)
                    {
                        foreach (var piece in _playerPieces)
                        {
                            if (piece.boardPieceId == BoardPieceId.HeroGuardian)
                            {
                                attackerUnit = piece;
                                break;
                            }
                        }
                    }
                    else if (attackerUnit.HasEffectState(EffectStateType.ConfusedPermanentVisualOnly) && (attackerUnit.boardPieceId == BoardPieceId.IceElemental || attackerUnit.boardPieceId == BoardPieceId.FireElemental))
                    {
                        foreach (var piece in _playerPieces)
                        {
                            if (piece.boardPieceId == BoardPieceId.HeroSorcerer)
                            {
                                attackerUnit = piece;
                                break;
                            }
                        }
                    }
                }
            }

            if (!attackerUnit.IsPlayer())
            {
                return;
            }

            var pointCount = attackerUnit.effectSink.GetEffectStateDurationTurnsLeft(EffectStateType.StrengthInNumbers);
            var addPoints = 0;
            if (pointCount > 998)
            {
                pointCount = 0;
            }

            if (!defeatedUnit.IsPlayer() && attackerUnit.IsPlayer())
            {
                addPoints += _globalConfig.KillEnemy;
                if (defeatedUnit.HasPieceType(PieceType.Boss))
                {
                    addPoints += _globalConfig.KillBoss;
                }
            }
            else if (defeatedUnit != attackerUnit && attackerUnit.IsPlayer())
            {
                addPoints += _globalConfig.KillPlayer;
            }
            else if (defeatedUnit == attackerUnit)
            {
                addPoints += _globalConfig.KillSelf;
            }

            if (attackerUnit.HasEffectState(EffectStateType.Key))
            {
                addPoints += _globalConfig.Keyholder;
            }

            if (addPoints < 0)
            {
                addPoints = 0;
            }

            if (addPoints != 0)
            {
                Piece_LevelUp(attackerUnit, pointCount + (addPoints * percentage));
            }
        }

        private static void Ability_GenerateAttackDamage_Postfix(Piece source, Piece mainTarget, Dice.Outcome diceResult, Piece[] targets)
        {
            if (!_isActivated)
            {
                return;
            }

            if (tempPiece != null)
            {
                if (source != null && !source.IsPlayer())
                {
                    source = tempPiece;
                }
                else
                {
                    tempPiece = null;
                }
            }

            if (source == null || (mainTarget != null && mainTarget.HasEffectState(EffectStateType.WizardDoppelganger)))
            {
                return;
            }

            if (source.IsPlayer())
            {
                if (mainTarget != null)
                {
                    if (mainTarget.HasPieceType(PieceType.Prop) && mainTarget.ToString().Contains("Lamp"))
                    {
                        tempPiece = source;
                        return;
                    }
                }
                else if (targets.Length != 0)
                {
                    for (int i = 0; i < targets.Length; i++)
                    {
                        if (targets[i].HasPieceType(PieceType.Prop) && targets[i].ToString().Contains("Lamp"))
                        {
                            tempPiece = source;
                            return;
                        }
                    }
                }
            }

            if (!source.IsPlayer())
            {
                Piece piece2;
                var gameContext = Traverse.Create(typeof(GameHub)).Field<GameContext>("gameContext").Value;
                if (source.boardPieceId == BoardPieceId.WarlockMinion && source.GetHealth() > 0 && (_globalConfig.Points4Minions == (int)PointsKeys.MinionsOnlyCana || _globalConfig.Points4Minions > 2))
                {
                    PieceAI pieceAI = source.pieceAI;
                    if (pieceAI == null)
                    {
                        return;
                    }
                    else if (pieceAI.memory.TryGetAssociatedPiece(gameContext.pieceAndTurnController, out piece2))
                    {
                        source = piece2;
                    }
                    else
                    {
                        return;
                    }
                }
                else if (source.boardPieceId == BoardPieceId.SellswordArbalestierActive && (_globalConfig.Points4Minions == (int)PointsKeys.MinionsOnlyArly || _globalConfig.Points4Minions > 2))
                {
                    PieceAI pieceAI = source.pieceAI;
                    if (pieceAI == null)
                    {
                        return;
                    }
                    else if (pieceAI.memory.TryGetAssociatedPiece(gameContext.pieceAndTurnController, out piece2))
                    {
                        source = piece2;
                    }
                    else
                    {
                        return;
                    }
                }
                else if (_globalConfig.Points4Minions > 3)
                {
                    if (source.boardPieceId == BoardPieceId.Verochka && !source.HasEffectState(EffectStateType.ConfusedPermanentVisualOnly))
                    {
                        foreach (var piece in _playerPieces)
                        {
                            if (piece.boardPieceId == BoardPieceId.HeroHunter)
                            {
                                source = piece;
                                break;
                            }
                        }
                    }
                    else if (source.boardPieceId == BoardPieceId.Tornado)
                    {
                        foreach (var piece in _playerPieces)
                        {
                            if (piece.boardPieceId == BoardPieceId.HeroBard)
                            {
                                source = piece;
                                break;
                            }
                        }
                    }
                    else if (source.boardPieceId == BoardPieceId.GrapplingTotem)
                    {
                        foreach (var piece in _playerPieces)
                        {
                            if (piece.boardPieceId == BoardPieceId.HeroBarbarian)
                            {
                                source = piece;
                                break;
                            }
                        }
                    }
                    else if (source.boardPieceId == BoardPieceId.SwordOfAvalon)
                    {
                        foreach (var piece in _playerPieces)
                        {
                            if (piece.boardPieceId == BoardPieceId.HeroRogue)
                            {
                                source = piece;
                                break;
                            }
                        }
                    }
                    else if (source.boardPieceId == BoardPieceId.SmiteWard)
                    {
                        foreach (var piece in _playerPieces)
                        {
                            if (piece.boardPieceId == BoardPieceId.HeroGuardian)
                            {
                                source = piece;
                                break;
                            }
                        }
                    }
                    else if (source.HasEffectState(EffectStateType.ConfusedPermanentVisualOnly) && (source.boardPieceId == BoardPieceId.IceElemental || source.boardPieceId == BoardPieceId.FireElemental))
                    {
                        foreach (var piece in _playerPieces)
                        {
                            if (piece.boardPieceId == BoardPieceId.HeroSorcerer)
                            {
                                source = piece;
                                break;
                            }
                        }
                    }
                }
            }

            if (!source.IsPlayer())
            {
                return;
            }

            var pointCount = source.effectSink.GetEffectStateDurationTurnsLeft(EffectStateType.StrengthInNumbers);
            var addPoints = 0;
            if (pointCount > 998)
            {
                pointCount = 0;
            }

            if (targets.Length != 0)
            {
                for (int i = 0; i < targets.Length; i++)
                {
                    if (targets[i].boardPieceId == BoardPieceId.GoldPile || targets[i].HasEffectState(EffectStateType.WizardDoppelganger))
                    {
                        continue;
                    }
                    else if (targets[i] != source && !targets[i].IsDowned() && !targets[i].IsImmuneToDamage())
                    {
                        if (targets[i].IsPlayer() && (diceResult == Dice.Outcome.Hit || diceResult == Dice.Outcome.Crit)) // hit.
                        {
                            addPoints += _globalConfig.HurtPlayer; // negative for co-op. positive for pvp.

                            if (source.HasEffectState(EffectStateType.Key))
                            {
                                addPoints += _globalConfig.Keyholder;
                            }
                        }
                        else if ((targets[i].IsPlayer() || targets[i].IsBot()) && diceResult == Dice.Outcome.None) // pvp and buff.
                        {
                            addPoints += _globalConfig.BuffPlayer; // test pvp

                            if (source.HasEffectState(EffectStateType.Key))
                            {
                                addPoints += _globalConfig.Keyholder;
                            }
                        }
                        else if (!targets[i].IsPlayer() && targets[i].HasPieceType(PieceType.Boss))
                        {
                            addPoints += _globalConfig.HurtBoss;
                        }
                        else if (!targets[i].IsPlayer() && !targets[i].IsBot() && !targets[i].IsProp())
                        {
                            // More points for attacking bigger enemies.
                            if ((int)targets[i].GetMaxHealth() > 20)
                            {
                                addPoints += 1;
                            }

                            addPoints += _globalConfig.HurtEnemy;
                            if (source.HasEffectState(EffectStateType.Key))
                            {
                                addPoints += _globalConfig.Keyholder;
                            }
                        }
                    }
                    else if (targets[i] == source && !source.IsDowned() && diceResult != Dice.Outcome.None)
                    {
                        addPoints += _globalConfig.HurtSelf;
                    }
                }
            }

            if (addPoints < 0)
            {
                addPoints = 0;
            }

            if (addPoints != 0)
            {
                Piece_LevelUp(source, pointCount + (addPoints * percentage));
            }
        }

        private static void Piece_CreatePiece_Postfix(ref Piece __result)
        {
            if (!_isActivated)
            {
                return;
            }

            if (!__result.IsPlayer())
            {
                return;
            }

            __result.effectSink.AddStatusEffect(EffectStateType.StrengthInNumbers, 0);
            __result.effectSink.TrySetStatMaxValue(Stats.Type.CritChance, 1);
            __result.EnableEffectState(EffectStateType.Flying);
            __result.effectSink.SetStatusEffectDuration(EffectStateType.Flying, 1);
        }
    }
}
