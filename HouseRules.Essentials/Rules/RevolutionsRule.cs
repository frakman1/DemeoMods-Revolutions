namespace HouseRules.Essentials.Rules
{
    using Boardgame;
    using Boardgame.BoardEntities;
    using DataKeys;
    using HarmonyLib;
    using HouseRules.Core;
    using HouseRules.Core.Types;

    public sealed class RevolutionsRule : Rule, IConfigWritable<int>, IPatchable, IMultiplayerSafe, IDisableOnReconnect
    {
        public override string Description => "A Reloaded/Revolutions style game is enabled";

        private static bool _isActivated;
        private static float _globalGameType;
        private static bool _isReconnect;
        private static bool _isFirst;
        private static bool _checkPlayers;
        private static int _numPlayers = 1;
        private static int _invPlayers = 1;
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
            _isFirst = false;
            _checkPlayers = false;
            _numPlayers = 1;
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

            /*harmony.Patch(
                original: AccessTools.Method(typeof(GameStateMachine), "OnRoomJoined"),
                postfix: new HarmonyMethod(typeof(RevolutionsRule), nameof(GameStateMachine_OnRoomJoined_Postfix)));*/
        }

        /*private static void GameStateMachine_OnRoomJoined_Postfix()
        {
            if (!_isActivated)
            {
                return;
            }

            // HouseRulesEssentialsBase.LogWarning("Room Joined so _isReconnect = true!");
            _isReconnect = true;
            _isFirst = true;
            _checkPlayers = false;
        }*/

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
            /*var ruleSet = HR.SelectedRuleset.Name;
            bool revolutions = false;
            bool rev_progr = false;
            foreach (var rule in HR.SelectedRuleset.Rules)
            {
                if (rule.ToString().Contains("PieceProgressRule"))
                {
                    rev_progr = true;
                }
                else if (rule.ToString().Contains("RevolutionsRule"))
                {
                    revolutions = true;
                }
            }

            bool reloaded = false;
            if (ruleSet.Equals("Demeo Reloaded"))
            {
                reloaded = true;
            }

            var gameContext = Traverse.Create(typeof(GameHub)).Field<GameContext>("gameContext").Value;
            if (_checkPlayers)
            {
                _numPlayers = gameContext.pieceAndTurnController.GetNumberOfPlayerPieces();
                _invPlayers = _numPlayers;
                HouseRulesEssentialsBase.LogWarning($"Number of players set to {_numPlayers}");
            }*/

            // Handle Host reconnect makes returning players invulnerable when becoming master client again
            /*if (_isReconnect)
            {
                HouseRulesEssentialsBase.LogWarning("_IsReconnect check...");
                if (_invPlayers > 0)
                {
                    HouseRulesEssentialsBase.LogWarning($"RECONNECT with {_numPlayers} players...");
                    if (reloaded)
                    {
                        piece.effectSink.TrySetStatBaseValue(Stats.Type.InnateCounterDamageExtraDamage, 42);
                    }
                    else if (revolutions)
                    {
                        piece.effectSink.TrySetStatBaseValue(Stats.Type.InnateCounterDamageExtraDamage, 69);
                    }

                    if (_invPlayers != _numPlayers)
                    {
                        piece.effectSink.AddStatusEffect(EffectStateType.Invulnerable1);
                        piece.effectSink.SetStatusEffectDuration(EffectStateType.Invulnerable1, 1);
                        _invPlayers--;
                        HouseRulesEssentialsBase.LogWarning("Invulnerable set!");
                    }
                    else
                    {
                        _invPlayers--;
                    }
                }
                else
                {
                    HouseRulesEssentialsBase.LogWarning("_numPlayers < 2 so _IsReconnect = false!");
                    _checkPlayers = true;
                    _isReconnect = false;
                }
            }*/

            // Handle fixing character stats and cards for Revolutions/Reloaded game when the Host reconnects and becomes the Master Client again
            /*if (_isReconnect && GameStateMachine.IsMasterClient && !piece.IsDead())
            {
                HouseRulesEssentialsBase.LogWarning($"Regained HOST so fixing player stats/cards for {piece.boardPieceId}...");
                if (piece.GetStat(Stats.Type.InnateCounterDamageExtraDamage) == 42 || piece.GetStat(Stats.Type.InnateCounterDamageExtraDamage) == 69)
                {
                    int mage = 0;
                    int runner = 0;
                    int diff = 0;
                    if (ruleSet.Contains("(EASY"))
                    {
                        diff = 2;
                    }
                    else if (ruleSet.Equals("Demeo Revolutions"))
                    {
                        diff = 1;
                    }

                    if (piece.boardPieceId == BoardPieceId.HeroSorcerer)
                    {
                        mage = 1;
                        if (reloaded)
                        {
                            piece.inventory.Items.Add(new Inventory.Item(
                                AbilityKey.SummonElemental,
                                flags: 0,
                                originalOwner: -1,
                                replenishCooldown: 0));
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
                        if (ruleSet.Contains("(LEGENDARY"))
                        {
                            piece.effectSink.TrySetStatBaseValue(Stats.Type.DownedCounter, 3);
                        }
                        else
                        {
                            piece.effectSink.TrySetStatBaseValue(Stats.Type.DownedCounter, 2);
                        }

                        piece.effectSink.TrySetStatMaxValue(Stats.Type.CritChance, 1);
                        piece.EnableEffectState(EffectStateType.Flying);
                        piece.effectSink.SetStatusEffectDuration(EffectStateType.Flying, 1);
                    }
                }

                piece.effectSink.TrySetStatBaseValue(Stats.Type.InnateCounterDamageExtraDamage, _globalGameType);
                piece.AddGold(0);
            }*/

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

            // Energy Potion cards added per class
            if (piece.HasEffectState(EffectStateType.ExtraEnergy))
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

            if (!__result.IsPlayer())
            {
                var ruleSet = HR.SelectedRuleset.Name;

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
                else if (ruleSet.Contains("PROGRESSIVE") || ruleSet.Contains("(LEGENDARY"))
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
            else
            {
                // HouseRulesEssentialsBase.LogWarning($"{__result.boardPieceId} value set to {_globalGameType}");
                __result.effectSink.TrySetStatBaseValue(Stats.Type.InnateCounterDamageExtraDamage, _globalGameType);
            }
        }
    }
}
