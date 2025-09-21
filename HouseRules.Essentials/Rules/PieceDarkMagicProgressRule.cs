namespace HouseRules.Essentials.Rules
{
    using Boardgame;
    using Boardgame.BoardEntities;
    using Boardgame.SerializableEvents;
    using DataKeys;
    using HarmonyLib;
    using HouseRules.Core.Types;

    public sealed class PieceDarkMagicProgressRule : Rule, IConfigWritable<bool>, IPatchable, IMultiplayerSafe
    {
        public override string Description => "DarkMagic's Hero progression levels are enabled";

        private static Context _context;
        private static bool _isActivated;

        public PieceDarkMagicProgressRule(bool value)
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
                    typeof(PieceDarkMagicProgressRule),
                    nameof(CreatePiece_Progression_Postfix)));

            harmony.Patch(
                original: AccessTools.Method(typeof(SerializableEventQueue), "RespondToRequest"),
                prefix: new HarmonyMethod(
                    typeof(PieceDarkMagicProgressRule),
                    nameof(SerializableEventQueue_RespondToRequest_Prefix)));

            harmony.Patch(
                original: AccessTools.Method(typeof(Inventory), "RestoreReplenishables"),
                prefix: new HarmonyMethod(
                    typeof(PieceDarkMagicProgressRule),
                    nameof(Inventory_RestoreReplenishables_Prefix)));
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
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.DownedCounter, piece.GetStat(Stats.Type.DownedCounter) - 1);
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.DownedTimer, piece.GetStat(Stats.Type.DownedTimer) + 1);
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
                else if (nextLevel == 3)
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
                    else if (piece.boardPieceId == BoardPieceId.HeroGuardian)
                    {
                        var abilityPromise = _context.AbilityFactory.LoadAbility(AbilityKey.ReplenishArmor);
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

                        var abilityPromise = _context.AbilityFactory.LoadAbility(AbilityKey.EnemyArrowSnipe);
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
                    else if (piece.boardPieceId == BoardPieceId.HeroWarlock)
                    {
                        var abilityPromise = _context.AbilityFactory.LoadAbility(AbilityKey.MinionCharge);
                        abilityPromise.OnLoaded(ability =>
                        {
                            ability.costActionPoint = false;
                        });
                    }
                }
                else if (nextLevel == 4)
                {
                    if (piece.boardPieceId == BoardPieceId.HeroBarbarian)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.BeaconOfSmite,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 6));
                        piece.AddGold(0);
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroBard)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.MonsterBait,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 6));
                        piece.AddGold(0);
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroGuardian)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.ProximityMine,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 6));
                        piece.AddGold(0);
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroRogue)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.GrapplingTotem,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 6));
                        piece.AddGold(0);
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroHunter)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.SwordOfAvalon,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 6));
                        piece.AddGold(0);
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroSorcerer)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.GuidingLight,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 6));
                        piece.AddGold(0);
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroWarlock)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.BeaconOfHealing,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 6));
                        piece.AddGold(0);
                    }
                }
                else if (nextLevel == 5)
                {
                    piece.effectSink.TrySetStatMaxValue(Stats.Type.Health, piece.GetMaxHealth() + 2);
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.Health, piece.GetHealth() + 2);
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.Speed, piece.GetStat(Stats.Type.Speed) + 1);
                    piece.effectSink.TrySetStatMaxValue(Stats.Type.Speed, piece.GetStatMax(Stats.Type.Speed) + 1);
                }
                else if (nextLevel == 6)
                {
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.DownedCounter, piece.GetStat(Stats.Type.DownedCounter) - 1);
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.DownedTimer, piece.GetStat(Stats.Type.DownedTimer) + 1);
                    if (piece.boardPieceId == BoardPieceId.HeroBarbarian)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.EarthShatter,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 7));
                        piece.AddGold(0);
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroBard)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.Petrify,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 7));
                        piece.AddGold(0);
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroGuardian)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.ElvenKingMeleeWhip,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 7));
                        piece.AddGold(0);
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroRogue)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.FretsOfFire,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 7));
                        piece.AddGold(0);
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroHunter)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.RatKingRatBomb,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 7));
                        piece.AddGold(0);
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroSorcerer)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.ImplosionExplosionRain,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 7));
                        piece.AddGold(0);
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroWarlock)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.SpawnElvenSummonerDefenders,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 7));
                        piece.AddGold(0);
                    }
                }
                else if (nextLevel == 7)
                {
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.Speed, piece.GetStat(Stats.Type.Speed) - 1);
                    piece.effectSink.TrySetStatMaxValue(Stats.Type.Speed, piece.GetStatMax(Stats.Type.Speed) - 1);
                    if (piece.boardPieceId == BoardPieceId.HeroSorcerer || piece.boardPieceId == BoardPieceId.HeroWarlock)
                    {
                        piece.effectSink.TrySetStatBaseValue(Stats.Type.MagicBonus, piece.GetStat(Stats.Type.MagicBonus) - 1);
                        piece.effectSink.TrySetStatMaxValue(Stats.Type.MagicBonus, piece.GetStatMax(Stats.Type.MagicBonus) - 1);
                    }
                    else
                    {
                        piece.effectSink.TrySetStatBaseValue(Stats.Type.Strength, piece.GetStat(Stats.Type.Strength) - 1);
                        piece.effectSink.TrySetStatMaxValue(Stats.Type.Strength, piece.GetStatMax(Stats.Type.Strength) - 1);
                    }
                }
                else if (nextLevel == 8)
                {
                    if (piece.boardPieceId == BoardPieceId.HeroBarbarian)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.ExplosiveOrb,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 3));
                        piece.AddGold(0);
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroBard)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.EnemyInvulnerability,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 3));
                        piece.AddGold(0);
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroGuardian)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.LastCrusade,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 3));
                        piece.AddGold(0);
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroRogue)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.SecondWind,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 3));
                        piece.AddGold(0);
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroHunter)
                    {
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.TurretHighDamageProjectile,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 3));
                        piece.AddGold(0);
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroSorcerer)
                    {
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.Electricity,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 3));
                        piece.AddGold(0);
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroWarlock)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.DeathFlurry,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 3));
                        piece.AddGold(0);
                    }
                }
                else if (nextLevel == 9)
                {
                    piece.effectSink.TrySetStatMaxValue(Stats.Type.Health, piece.GetMaxHealth() + 3);
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.Health, piece.GetHealth() + 3);
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.Speed, piece.GetStat(Stats.Type.Speed) + 2);
                    piece.effectSink.TrySetStatMaxValue(Stats.Type.Speed, piece.GetStatMax(Stats.Type.Speed) + 2);
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
                }
                else if (nextLevel == 10)
                {
                    piece.effectSink.TryGetStat(Stats.Type.ActionPoints, out int currentAP);
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.ActionPoints, currentAP + 1);
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

            // Extra Actions, and Action Point cost changes per character class
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

            if (level < 3)
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

                    var abilityPromise2 = _context.AbilityFactory.LoadAbility(AbilityKey.EnemyArrowSnipe);
                    abilityPromise2.OnLoaded(ability =>
                    {
                        ability.costActionPoint = true;
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
                if (piece.boardPieceId == BoardPieceId.HeroGuardian)
                {
                    var abilityPromise = _context.AbilityFactory.LoadAbility(AbilityKey.ReplenishArmor);
                    abilityPromise.OnLoaded(ability =>
                    {
                        ability.costActionPoint = false;
                    });
                }
                else if (piece.boardPieceId == BoardPieceId.HeroBarbarian)
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
    }
}
