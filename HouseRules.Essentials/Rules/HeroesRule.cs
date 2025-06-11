namespace HouseRules.Essentials.Rules
{
    using Boardgame;
    using Boardgame.BoardEntities;
    using Boardgame.BoardEntities.Abilities;
    using Boardgame.BoardEntities.AI;
    using Data.GameData;
    using DataKeys;
    using HarmonyLib;
    using HouseRules.Core;
    using HouseRules.Core.Types;
    using UnityEngine;
    using static UnityEngine.UIElements.StylePropertyAnimationSystem;

    public sealed class HeroesRule : Rule, IConfigWritable<int>, IPatchable, IMultiplayerSafe
    {
        public override string Description => "A Heroes style game (featuring Revolutions) is enabled";

        private static Context _context;
        private static bool _isActivated;
        private static float _globalGameType;
        private static bool _isReconnect;
        private static bool _checkPlayers;
        private static int _numPlayers = 1;
        private readonly int _gameType;

        public HeroesRule(int gameType)
        {
            _gameType = gameType;
        }

        public int GetConfigObject() => _gameType;

        protected override void OnActivate(Context context)
        {
            _context = context;
            _globalGameType = _gameType;
            _isActivated = true;

            // Traverse.Create(typeof(AIDirectorConfig)).Field<float>("CardEnergy_EnergyRequiredToGetNewCard").Value = 5.0f;
            if (HR.SelectedRuleset.Name.Contains("Heroes "))
            {
                // set sorting priority in player hands for refreshing cards.
                // warlock.
                if (_context.AbilityFactory.TryGetAbility(AbilityKey.MagicMissile, out Ability ability))
                {
                    ability.hasCardHandSortingPriority = true;
                }

                if (_context.AbilityFactory.TryGetAbility(AbilityKey.EnemyArrow, out Ability ability1))
                {
                    ability1.hasCardHandSortingPriority = true;
                }

                // guardian.
                if (_context.AbilityFactory.TryGetAbility(AbilityKey.ReplenishArmor, out Ability ability2))
                {
                    ability2.hasCardHandSortingPriority = true;
                }

                if (_context.AbilityFactory.TryGetAbility(AbilityKey.Zap, out Ability ability3))
                {
                    ability3.hasCardHandSortingPriority = true;
                }

                if (_context.AbilityFactory.TryGetAbility(AbilityKey.TurretHealProjectile, out Ability ability4))
                {
                    ability4.hasCardHandSortingPriority = true;
                }

                // bard.
                if (_context.AbilityFactory.TryGetAbility(AbilityKey.StrengthenCourage, out Ability ability5))
                {
                    ability5.hasCardHandSortingPriority = true;
                }

                if (_context.AbilityFactory.TryGetAbility(AbilityKey.DrainingKiss, out Ability ability6))
                {
                    ability6.hasCardHandSortingPriority = true;
                }

                // hunter.
                if (_context.AbilityFactory.TryGetAbility(AbilityKey.LetItRain, out Ability ability7))
                {
                    ability7.hasCardHandSortingPriority = true;
                }

                if (_context.AbilityFactory.TryGetAbility(AbilityKey.Whip, out Ability ability8))
                {
                    ability8.hasCardHandSortingPriority = true;
                }

                if (_context.AbilityFactory.TryGetAbility(AbilityKey.TornadoCharge, out Ability ability9))
                {
                    ability9.hasCardHandSortingPriority = true;
                }

                // assassin.
                if (_context.AbilityFactory.TryGetAbility(AbilityKey.Stealth, out Ability ability10))
                {
                    ability10.hasCardHandSortingPriority = true;
                }

                if (_context.AbilityFactory.TryGetAbility(AbilityKey.EnemyJavelin, out Ability ability11))
                {
                    ability11.hasCardHandSortingPriority = true;
                }

                if (_context.AbilityFactory.TryGetAbility(AbilityKey.DiseasedBite, out Ability ability12))
                {
                    ability12.hasCardHandSortingPriority = true;
                }

                if (_context.AbilityFactory.TryGetAbility(AbilityKey.EnemyFireball, out Ability ability13))
                {
                    ability13.hasCardHandSortingPriority = true;
                }

                // barbarian.
                if (_context.AbilityFactory.TryGetAbility(AbilityKey.Grapple, out Ability ability14))
                {
                    ability14.hasCardHandSortingPriority = true;
                }

                if (_context.AbilityFactory.TryGetAbility(AbilityKey.EnemyJavelin, out Ability ability15))
                {
                    ability15.hasCardHandSortingPriority = true;
                }

                // sorcerer.
                if (_context.AbilityFactory.TryGetAbility(AbilityKey.SnakeBossLongRange, out Ability ability16))
                {
                    ability16.hasCardHandSortingPriority = true;
                }

                if (_context.AbilityFactory.TryGetAbility(AbilityKey.EnemyArrow, out Ability ability17))
                {
                    ability17.hasCardHandSortingPriority = true;
                }

                // extra adjustments for some abilities.
                // AbilityFactory.GetAbility(AbilityKey.Grab).maxRange = 15; // only works for the host
                // AbilityFactory.GetAbility(AbilityKey.ProximityMine).maxRange = 5; // only works for the host
                // AbilityFactory.GetAbility(AbilityKey.StrengthenCourage).mayTargetSelf = true; // only works for the host
            }
        }

        protected override void OnDeactivate(Context context)
        {
            _isActivated = false;
            _isReconnect = false;
            _checkPlayers = false;
            _numPlayers = 1;
        }

        private static void Patch(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(Piece), "CreatePiece"),
                postfix: new HarmonyMethod(
                    typeof(HeroesRule),
                    nameof(CreatePiece_Revolutions_Postfix)));

            harmony.Patch(
                original: AccessTools.Method(typeof(Inventory), "RestoreReplenishables"),
                prefix: new HarmonyMethod(
                    typeof(HeroesRule),
                    nameof(Inventory_RestoreReplenishables_Prefix)));
        }

        private static bool Inventory_RestoreReplenishables_Prefix(ref bool __result, Piece piece)
        {
            if (!_isActivated)
            {
                return true;
            }

            if (piece == null || !piece.IsPlayer())
            {
                return true;
            }

            var ruleSet = HR.SelectedRuleset.Name;

            if (ruleSet.Contains("Heroes ") && ruleSet.Contains("PROGRESSIVE"))
            {
                // decrease the counter by 1.
                piece.effectSink.TryGetStat(Stats.Type.InnateCounterDirections, out int countCurrent);
                if (countCurrent > 0)
                {
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.InnateCounterDirections, countCurrent - 1);
                }
            }

            var gameContext = Traverse.Create(typeof(GameHub)).Field<GameContext>("gameContext").Value;

            // force refresh of everything?
            // gameContext.serializableEventQueue.OnStartLoadNewLevel();
            // gameContext.serializableEventQueue.ProcessRequests();
            // gameContext.serializableEventQueue.SendResponseEvent(SerializableEvent.CreateRecovery());
            // gameContext.serializableEventQueue.Tick();

            if (_checkPlayers)
            {
                _numPlayers = gameContext.pieceAndTurnController.GetNumberOfPlayerPieces();
                _checkPlayers = false;
            }

            // Handle Host reconnect makes returning players invulnerable when becoming master client again
            if (_isReconnect)
            {
                if (piece.GetStat(Stats.Type.InnateCounterDamageExtraDamage) != 55)
                {
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.InnateCounterDamageExtraDamage, 55);
                    _isReconnect = false;
                }
                else if (_numPlayers > 1)
                {
                    piece.effectSink.AddStatusEffect(EffectStateType.Invulnerable1);
                    piece.effectSink.SetStatusEffectDuration(EffectStateType.Invulnerable1, 1);
                    _numPlayers--;
                }
            }

            // Handle fixing character stats for and cards Heroes game when the Host reconnects and becomes the Master Client again
            if (GameStateMachine.IsMasterClient && !piece.IsDead() && piece.GetStat(Stats.Type.InnateCounterDamageExtraDamage) != 55)
            {
                _isReconnect = true;
                _checkPlayers = true;

                // reset the sorcerer.
                if (piece.boardPieceId == BoardPieceId.HeroSorcerer)
                {
                    // reset the sorcerer hand.
                    piece.inventory.Items.Clear();
                    Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                    piece.inventory.Items.Add(new Inventory.Item(
                        AbilityKey.EnemyArrow,
                        flags: (Inventory.ItemFlag)1,
                        originalOwner: -1,
                        replenishCooldown: 1));
                    piece.inventory.Items.Add(new Inventory.Item(
                        AbilityKey.SnakeBossLongRange,
                        flags: 0,
                        originalOwner: -1,
                        replenishCooldown: 0));

                    piece.inventory.Items.Add(new Inventory.Item(
                        AbilityKey.Confuse,
                        flags: 0,
                        originalOwner: -1,
                        replenishCooldown: 0));

                    piece.inventory.Items.Add(new Inventory.Item(
                        AbilityKey.Heal,
                        flags: 0,
                        originalOwner: -1,
                        replenishCooldown: 0));

                    piece.effectSink.TrySetStatBaseValue(Stats.Type.CritDamage, 4);
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.MagicBonus, 1);
                    piece.effectSink.TrySetStatMaxValue(Stats.Type.Health, 5);
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.Health, 5);
                }

                // reset the warlock.
                else if (piece.boardPieceId == BoardPieceId.HeroWarlock)
                {
                    piece.inventory.Items.Clear();
                    Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 2;
                    piece.inventory.Items.Add(new Inventory.Item(
                        AbilityKey.MagicMissile,
                        flags: (Inventory.ItemFlag)1,
                        originalOwner: -1,
                        replenishCooldown: 1));

                    piece.inventory.Items.Add(new Inventory.Item(
                        AbilityKey.EnemyArrow,
                        flags: (Inventory.ItemFlag)1,
                        originalOwner: -1,
                        replenishCooldown: 1));

                    piece.inventory.Items.Add(new Inventory.Item(
                        AbilityKey.Exterminate,
                        flags: 0,
                        originalOwner: -1,
                        replenishCooldown: 0));

                    piece.inventory.Items.Add(new Inventory.Item(
                        AbilityKey.Barrage,
                        flags: 0,
                        originalOwner: -1,
                        replenishCooldown: 0));

                    piece.inventory.Items.Add(new Inventory.Item(
                        AbilityKey.Heal,
                        flags: 0,
                        originalOwner: -1,
                        replenishCooldown: 0));

                    piece.effectSink.TrySetStatBaseValue(Stats.Type.AttackDamage, 2); // warlock starting damage.
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.CritDamage, 4);
                    piece.effectSink.TrySetStatMaxValue(Stats.Type.Health, 5);
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.Health, 5);
                }

                // reset the hunter.
                else if (piece.boardPieceId == BoardPieceId.HeroHunter)
                {
                    piece.inventory.Items.Clear();
                    Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 2;
                    piece.inventory.Items.Add(new Inventory.Item(
                        AbilityKey.LetItRain,
                        flags: (Inventory.ItemFlag)1,
                        originalOwner: -1,
                        replenishCooldown: 4));

                    piece.inventory.Items.Add(new Inventory.Item(
                        AbilityKey.Whip,
                        flags: (Inventory.ItemFlag)1,
                        originalOwner: -1,
                        replenishCooldown: 1));

                    piece.inventory.Items.Add(new Inventory.Item(
                        AbilityKey.EarthShatter,
                        flags: 0,
                        originalOwner: -1,
                        replenishCooldown: 0));

                    piece.inventory.Items.Add(new Inventory.Item(
                        AbilityKey.Heal,
                        flags: 0,
                        originalOwner: -1,
                        replenishCooldown: 0));

                    piece.effectSink.TrySetStatBaseValue(Stats.Type.AttackDamage, 2); // reset hunter stats.
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.CritDamage, 4);
                    piece.effectSink.TrySetStatMaxValue(Stats.Type.Health, 5);
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.Health, 5);
                }

                // reset the bard.
                else if (piece.boardPieceId == BoardPieceId.HeroBard)
                {
                    piece.inventory.Items.Clear();
                    Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 2;
                    piece.inventory.Items.Add(new Inventory.Item(
                        AbilityKey.StrengthenCourage,
                        flags: (Inventory.ItemFlag)1,
                        originalOwner: -1,
                        replenishCooldown: 1));

                    piece.inventory.Items.Add(new Inventory.Item(
                        AbilityKey.DrainingKiss,
                        flags: (Inventory.ItemFlag)1,
                        originalOwner: -1,
                        replenishCooldown: 2));

                    piece.inventory.Items.Add(new Inventory.Item(
                        AbilityKey.Weaken,
                        flags: 0,
                        originalOwner: -1,
                        replenishCooldown: 0));

                    piece.inventory.Items.Add(new Inventory.Item(
                        AbilityKey.Heal,
                        flags: 0,
                        originalOwner: -1,
                        replenishCooldown: 0));

                    piece.effectSink.TrySetStatBaseValue(Stats.Type.AttackDamage, 2); // reset bard stats.
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.CritDamage, 3);
                    piece.effectSink.TrySetStatMaxValue(Stats.Type.Health, 5);
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.Health, 5);
                }

                // reset the assassin.
                else if (piece.boardPieceId == BoardPieceId.HeroRogue)
                {
                    piece.inventory.Items.Clear();
                    Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 3;
                    piece.inventory.Items.Add(new Inventory.Item(
                        AbilityKey.Stealth,
                        flags: (Inventory.ItemFlag)1,
                        originalOwner: -1,
                        replenishCooldown: 1));

                    piece.inventory.Items.Add(new Inventory.Item(
                        AbilityKey.ExtraActionPotion,
                        flags: (Inventory.ItemFlag)1,
                        originalOwner: -1,
                        replenishCooldown: 4));

                    piece.inventory.Items.Add(new Inventory.Item(
                        AbilityKey.EnemyJavelin,
                        flags: (Inventory.ItemFlag)1,
                        originalOwner: -1,
                        replenishCooldown: 1));

                    piece.inventory.Items.Add(new Inventory.Item(
                        AbilityKey.Whirlwind,
                        flags: 0,
                        originalOwner: -1,
                        replenishCooldown: 0));

                    piece.inventory.Items.Add(new Inventory.Item(
                        AbilityKey.Heal,
                        flags: 0,
                        originalOwner: -1,
                        replenishCooldown: 0));

                    piece.effectSink.TrySetStatBaseValue(Stats.Type.CritDamage, 5); // reset assassin stats.
                    piece.effectSink.TrySetStatMaxValue(Stats.Type.Health, 6);
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.Health, 6);
                }

                // reset barbarian.
                else if (piece.boardPieceId == BoardPieceId.HeroBarbarian)
                {
                    piece.inventory.Items.Clear();
                    Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 3;
                    piece.inventory.Items.Add(new Inventory.Item(
                        AbilityKey.Grapple,
                        flags: (Inventory.ItemFlag)1,
                        originalOwner: -1,
                        replenishCooldown: 1));

                    piece.inventory.Items.Add(new Inventory.Item(
                        AbilityKey.ExplodingLampPlaceholder,
                        flags: (Inventory.ItemFlag)1,
                        originalOwner: -1,
                        replenishCooldown: 1));

                    piece.inventory.Items.Add(new Inventory.Item(
                        AbilityKey.TauntingScream,
                        flags: (Inventory.ItemFlag)1,
                        originalOwner: -1,
                        replenishCooldown: 3));

                    piece.inventory.Items.Add(new Inventory.Item(
                        AbilityKey.Heal,
                        flags: 0,
                        originalOwner: -1,
                        replenishCooldown: 0));

                    piece.effectSink.TrySetStatBaseValue(Stats.Type.CritDamage, 5); // reset barbarian stats.
                    piece.effectSink.TrySetStatMaxValue(Stats.Type.Health, 7);
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.Health, 7);
                }

                // reset guardian.
                else if (piece.boardPieceId == BoardPieceId.HeroGuardian)
                {
                    piece.inventory.Items.Clear();
                    Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 2;
                    piece.inventory.Items.Add(new Inventory.Item(
                        AbilityKey.ReplenishArmor,
                        flags: (Inventory.ItemFlag)1,
                        originalOwner: -1,
                        replenishCooldown: 1));

                    piece.inventory.Items.Add(new Inventory.Item(
                        AbilityKey.BlindingLight,
                        flags: (Inventory.ItemFlag)1,
                        originalOwner: -1,
                        replenishCooldown: 2));

                    piece.inventory.Items.Add(new Inventory.Item(
                        AbilityKey.MagicShield,
                        flags: 0,
                        originalOwner: -1,
                        replenishCooldown: 0));

                    piece.inventory.Items.Add(new Inventory.Item(
                        AbilityKey.Heal,
                        flags: 0,
                        originalOwner: -1,
                        replenishCooldown: 0));

                    piece.effectSink.TrySetStatBaseValue(Stats.Type.CritDamage, 4); // reset guardian stats.
                    piece.effectSink.TrySetStatMaxValue(Stats.Type.Health, 6);
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.Health, 6);
                }

                if (ruleSet.Contains("INSANE"))
                {
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.DownedCounter, 3);
                }
                else if (ruleSet.Contains("EASY"))
                {
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.DownedCounter, 1);
                }
                else
                {
                    piece.effectSink.TrySetStatBaseValue(Stats.Type.DownedCounter, 2);
                }

                piece.effectSink.TrySetStatMaxValue(Stats.Type.CritChance, 1);
                piece.EnableEffectState(EffectStateType.Flying);
                piece.effectSink.SetStatusEffectDuration(EffectStateType.Flying, 1); // flying is the level tracker.
                piece.effectSink.TrySetStatBaseValue(Stats.Type.InnateCounterDamageExtraDamage, _globalGameType);
                piece.effectSink.TrySetStatBaseValue(Stats.Type.InnateCounterDirections, 0);
                piece.AddGold(0);
            }

            return false;
        }

        private static void CreatePiece_Revolutions_Postfix(ref Piece __result)
        {
            if (!_isActivated)
            {
                return;
            }

            if (__result.IsPlayer())
            {
                // Apply level 2.
                if (HR.SelectedRuleset.Name.Contains("Heroes ") && HR.SelectedRuleset.Name.Contains("EASY PROGRESSIVE"))
                {
                    Piece piece = __result;
                    GameUI.ShowCameraMessage("<color=#F0F312>The party Starts at</color> <color=#00FF00>LEVEL 3</color><color=#F0F312>!</color>", 6);

                    // extra stats and refreshables become 0AP at level 2
                    // piece.effectSink.TrySetStatMaxValue(Stats.Type.Health, piece.GetMaxHealth() + 1);
                    // piece.effectSink.TrySetStatBaseValue(Stats.Type.Health, piece.GetHealth() + 1);
                    piece.GetPieceConfig().StartHealth += 1;

                    if (piece.boardPieceId == BoardPieceId.HeroBarbarian)
                    {
                        _context.AbilityFactory.TryGetAbility(AbilityKey.TauntingScream, out var ability);
                        ability.costActionPoint = false;
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroBard)
                    {
                        _context.AbilityFactory.TryGetAbility(AbilityKey.StrengthenCourage, out var ability);
                        ability.costActionPoint = false;
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroGuardian)
                    {
                        // Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                        AbilityKey.DropChest,
                        flags: 0,
                        originalOwner: -1,
                        replenishCooldown: 0));
                        piece.AddGold(0);

                        _context.AbilityFactory.TryGetAbility(AbilityKey.BlindingLight, out var ability);
                        ability.costActionPoint = false;
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroRogue)
                    {
                        _context.AbilityFactory.TryGetAbility(AbilityKey.Stealth, out var ability);
                        ability.costActionPoint = false;
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroHunter)
                    {
                        _context.AbilityFactory.TryGetAbility(AbilityKey.Whip, out var ability);
                        ability.costActionPoint = false;
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroSorcerer)
                    {
                        _context.AbilityFactory.TryGetAbility(AbilityKey.SnakeBossLongRange, out var ability);
                        ability.costActionPoint = false;
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroWarlock)
                    {
                        _context.AbilityFactory.TryGetAbility(AbilityKey.MagicMissile, out var ability);
                        ability.costActionPoint = false;
                    }

                    // Apply level 3.
                    if (piece.boardPieceId == BoardPieceId.HeroBarbarian)
                    {
                        // increases their hand size incase their hand is full, so they can take the card.
                        // I think this is making everything refreshing. update: it was the 'flags' variable that made it refreshing.
                        // Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.SpawnRandomLamp,
                            flags: 0,
                            originalOwner: -1,
                            replenishCooldown: 0));
                        piece.AddGold(0); // this only updates their inventory, but its still helpful.

                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.ReplenishArmor,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 2));

                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.WeakeningShout,
                            flags: 0,
                            originalOwner: -1,
                            replenishCooldown: 0));
                        piece.AddGold(0);
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroBard)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.TeleportLamp,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 2));

                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.BlindingLight,
                            flags: 0,
                            originalOwner: -1,
                            replenishCooldown: 0));

                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.PoisonGasGrenade,
                            flags: 0,
                            originalOwner: -1,
                            replenishCooldown: 0));
                        piece.AddGold(0);
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroGuardian)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 2;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.Zap,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 2));

                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.TurretHealProjectile,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 3));
                        piece.AddGold(0);
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroRogue)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 2;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.EnemyFireball,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 1));

                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.DiseasedBite,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 1));

                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.ProximityMine,
                            flags: 0,
                            originalOwner: -1,
                            replenishCooldown: 0));
                        piece.AddGold(0);
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroHunter)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 2;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.TornadoCharge,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 1));

                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.WaterDive,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 3));

                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.EnemyFrostball,
                            flags: 0,
                            originalOwner: -1,
                            replenishCooldown: 0));

                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.RatWhisperer,
                            flags: 0,
                            originalOwner: -1,
                            replenishCooldown: 0));
                        piece.AddGold(0);
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroSorcerer)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.TurretDamageProjectile,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 3));

                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.Implode,
                            flags: 0,
                            originalOwner: -1,
                            replenishCooldown: 0));
                        piece.AddGold(0);
                    }
                    else if (piece.boardPieceId == BoardPieceId.HeroWarlock)
                    {
                        Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.EnemyFireball,
                            flags: (Inventory.ItemFlag)1,
                            originalOwner: -1,
                            replenishCooldown: 2));
                        piece.AddGold(0);

                        // Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                        piece.inventory.Items.Add(new Inventory.Item(
                            AbilityKey.MinionCharge,
                            flags: 0,
                            originalOwner: -1,
                            replenishCooldown: 0));
                        piece.AddGold(0);

                        _context.AbilityFactory.TryGetAbility(AbilityKey.MinionCharge, out var ability);
                        ability.costActionPoint = false;
                    }
                }
            }

            if (!__result.IsPlayer())
            {
                var ruleSet = HR.SelectedRuleset.Name;
                var heroesInsaneMultiplier = 1.0f;
                var heroesDifficultyMultiplier = 1.0f;
                if (ruleSet.Contains("Heroes ") && ruleSet.Contains("INSANE PROGRESSIVE"))
                {
                    heroesInsaneMultiplier = 1.20f;
                }
                else if (ruleSet.Contains("Heroes ") && ruleSet.Contains("EASY PROGRESSIVE"))
                {
                    heroesDifficultyMultiplier = 0.10f;
                }

                // checks the game type for Heroes, and makes sure its not giving lamps more HP.
                if (ruleSet.Contains("Heroes ") && (__result.boardPieceId != BoardPieceId.GasLamp || __result.boardPieceId != BoardPieceId.IceLamp || __result.boardPieceId != BoardPieceId.OilLamp || __result.boardPieceId != BoardPieceId.VortexLamp || __result.boardPieceId != BoardPieceId.WaterLamp))
                {
                    if (__result.boardPieceId == BoardPieceId.FireElemental)
                    {
                        __result.effectSink.AddStatusEffect(EffectStateType.FireImmunity, 99);
                    }
                    else if (__result.boardPieceId == BoardPieceId.IceElemental)
                    {
                        __result.effectSink.AddStatusEffect(EffectStateType.IceImmunity, 99);
                    }

                    if (ruleSet.Contains("Insane"))
                    {
                        var gameContext = Traverse.Create(typeof(GameHub)).Field<GameContext>("gameContext").Value;
                        if (gameContext.levelLoaderAndInitializer.GetLevelSequence().CurrentLevelIndex == 3)
                        {
                            // increase the monster health by 1/4. Should scale for bigger enemies.
                            __result.effectSink.TrySetStatMaxValue(Stats.Type.Health, __result.GetMaxHealth() * (1.25f * heroesInsaneMultiplier));
                            __result.effectSink.TrySetStatBaseValue(Stats.Type.Health, __result.GetMaxHealth() * (1.25f * heroesInsaneMultiplier));
                        }
                        else if (gameContext.levelLoaderAndInitializer.GetLevelSequence().CurrentLevelIndex == 5)
                        {
                            // check if it's a boss so they don't get too many hit points.
                            if (__result.boardPieceId != BoardPieceId.ElvenQueen && __result.boardPieceId != BoardPieceId.RootLord && __result.boardPieceId != BoardPieceId.MotherCy && __result.boardPieceId != BoardPieceId.WizardBoss && __result.boardPieceId != BoardPieceId.BossTown)
                            {
                                // increase the monster health by 1/2. Should scale for bigger enemies.
                                __result.effectSink.TrySetStatMaxValue(Stats.Type.Health, __result.GetMaxHealth() * (1.5f * heroesInsaneMultiplier));
                                __result.effectSink.TrySetStatBaseValue(Stats.Type.Health, __result.GetMaxHealth() * (1.5f * heroesInsaneMultiplier));

                                // if Insane mode, all monsters berserk at low health.
                                if (heroesInsaneMultiplier > 1.0f)
                                {
                                    if (__result.GetPieceConfig().BerserkBelowHealth < 40.0f)
                                    {
                                        __result.GetPieceConfig().BerserkBelowHealth = 40.0f;
                                    }
                                }
                            }
                            else if (__result.boardPieceId == BoardPieceId.ElvenQueen && __result.boardPieceId == BoardPieceId.RootLord && __result.boardPieceId == BoardPieceId.MotherCy && __result.boardPieceId == BoardPieceId.WizardBoss && __result.boardPieceId == BoardPieceId.BossTown)
                            {
                                // decrease Boss health for easy mode.
                                __result.effectSink.TrySetStatMaxValue(Stats.Type.Health, __result.GetMaxHealth() * heroesDifficultyMultiplier);
                                __result.effectSink.TrySetStatBaseValue(Stats.Type.Health, __result.GetMaxHealth() * heroesDifficultyMultiplier);
                            }
                        }

                        // check if they have 0 health and give them at least 1 HP.
                        if (__result.GetMaxHealth() < 1)
                        {
                            __result.effectSink.TrySetStatMaxValue(Stats.Type.Health, 1);
                            __result.effectSink.TrySetStatBaseValue(Stats.Type.Health, 1);
                        }
                    }

                    if (__result.boardPieceId == BoardPieceId.GasLamp || __result.boardPieceId == BoardPieceId.IceLamp || __result.boardPieceId == BoardPieceId.OilLamp || __result.boardPieceId == BoardPieceId.VortexLamp || __result.boardPieceId == BoardPieceId.WaterLamp)
                    {
                        // check to make sure lamps only have 1HP.
                        __result.effectSink.TrySetStatMaxValue(Stats.Type.Health, 1.0f);
                        __result.effectSink.TrySetStatBaseValue(Stats.Type.Health, 1.0f);
                    }
                }

                // Insanity! Corresponds to check in Tick. If duration is above 50, reset to another random buff.
                if (heroesInsaneMultiplier > 1.0f && (__result.boardPieceId != BoardPieceId.ElvenQueen && __result.boardPieceId != BoardPieceId.RootLord))
                {
                    if (__result.HasPieceType(PieceType.Creature) && !__result.HasPieceType(PieceType.ExplodingLamp) && !__result.HasEffectState(EffectStateType.Confused) && __result.boardPieceId != BoardPieceId.Verochka && __result.boardPieceId != BoardPieceId.WarlockMinion)
                    {
                        int nextPhase = Random.Range(1, 6);
                        switch (nextPhase)
                        {
                            case 1:
                                __result.EnableEffectState(EffectStateType.Deflect, 55);
                                // __result.effectSink.SetStatusEffectDuration(EffectStateType.Deflect, 2);
                                break;
                            case 2:
                                __result.EnableEffectState(EffectStateType.MagicShield, 55);
                                break;
                            case 3:
                                __result.EnableEffectState(EffectStateType.Frenzy, 55);
                                break;
                            case 4:
                                __result.EnableEffectState(EffectStateType.Recovery, 55);
                                break;
                            case 5:
                                __result.EnableEffectState(EffectStateType.Courageous, 55);
                                break;
                        }
                    }
                }

                return;
            }
            else
            {
                __result.effectSink.TrySetStatBaseValue(Stats.Type.InnateCounterDamageExtraDamage, _globalGameType);
                __result.effectSink.TrySetStatBaseValue(Stats.Type.InnateCounterDirections, 0);
            }
        }
    }
}
