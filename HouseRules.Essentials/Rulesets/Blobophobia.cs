namespace HouseRules.Essentials.Rulesets
{
    using System.Collections.Generic;
    using DataKeys;
    using global::Types;
    using HouseRules.Core.Types;
    using HouseRules.Essentials.Rules;

    internal static class Blobophobia
    {
        internal static Ruleset Create()
        {
            const string name = "Blobophobia";
            const string description = "Money Slimes everywhere. On my face and in my hair.";
            const string longdesc = "";

            var blobophobiaRule = new PieceBlobophobiaRule(true);

            var abilityDamageAllRule = new AbilityDamageAllOverriddenRule(new Dictionary<AbilityKey, List<int>>
            {
                { AbilityKey.Zap, new List<int> { 2, 4, 2, 4 } },
            });

            var barbarianCards = new List<StartCardsModifiedRule.CardConfig>
            {
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.Heal, ReplenishFrequency = 0 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.Grapple, ReplenishFrequency = 1 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.OilLamp, ReplenishFrequency = 1 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.GrapplingPush, ReplenishFrequency = 0 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.GrapplingSmash, ReplenishFrequency = 0 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.ExplodingLampPlaceholder, ReplenishFrequency = 1 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.Stealth, ReplenishFrequency = 0 },
            };
            var bardCards = new List<StartCardsModifiedRule.CardConfig>
            {
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.Heal, ReplenishFrequency = 0 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.StrengthenCourage, ReplenishFrequency = 1 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.IceLamp, ReplenishFrequency = 1 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.SongOfRecovery, ReplenishFrequency = 0 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.Stealth, ReplenishFrequency = 0 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.Tornado, ReplenishFrequency = 0 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.ShatteringVoice, ReplenishFrequency = 0 },
            };
            var guardianCards = new List<StartCardsModifiedRule.CardConfig>
            {
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.Heal, ReplenishFrequency = 0 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.ReplenishArmor, ReplenishFrequency = 1 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.LeapHeavy, ReplenishFrequency = 2 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.Bone, ReplenishFrequency = 1 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.Whirlwind, ReplenishFrequency = 0 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.PiercingSpear, ReplenishFrequency = 0 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.Stealth, ReplenishFrequency = 0 },
            };
            var hunterCards = new List<StartCardsModifiedRule.CardConfig>
            {
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.Heal, ReplenishFrequency = 0 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.HunterArrow, ReplenishFrequency = 1 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.EnemyFireball, ReplenishFrequency = 1 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.PoisonedTip, ReplenishFrequency = 1 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.SplittingArrow, ReplenishFrequency = 0 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.CallCompanion, ReplenishFrequency = 0 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.Stealth, ReplenishFrequency = 0 },
            };
            var assassinCards = new List<StartCardsModifiedRule.CardConfig>
            {
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.Heal, ReplenishFrequency = 0 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.Stealth, ReplenishFrequency = 1 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.OilLamp, ReplenishFrequency = 1 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.DiseasedBite, ReplenishFrequency = 2 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.ProximityMine, ReplenishFrequency = 0 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.PoisonGasGrenade, ReplenishFrequency = 0 },
            };
            var sorcererCards = new List<StartCardsModifiedRule.CardConfig>
            {
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.Heal, ReplenishFrequency = 0 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.Zap, ReplenishFrequency = 1 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.WaterLamp, ReplenishFrequency = 1 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.GodsFury, ReplenishFrequency = 0 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.EyeOfAvalon, ReplenishFrequency = 0 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.Fireball, ReplenishFrequency = 0 },
            };
            var warlockCards = new List<StartCardsModifiedRule.CardConfig>
            {
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.Heal, ReplenishFrequency = 0 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.MagicMissile, ReplenishFrequency = 1 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.MagicMissile, ReplenishFrequency = 1 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.MinionCharge, ReplenishFrequency = 1 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.MissileSwarm, ReplenishFrequency = 0 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.GodsFury, ReplenishFrequency = 0 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.EyeOfAvalon, ReplenishFrequency = 0 },
            };
            var startingCardsRule = new StartCardsModifiedRule(new Dictionary<BoardPieceId, List<StartCardsModifiedRule.CardConfig>>
            {
                { BoardPieceId.HeroBarbarian, barbarianCards },
                { BoardPieceId.HeroBard, bardCards },
                { BoardPieceId.HeroGuardian, guardianCards },
                { BoardPieceId.HeroHunter, hunterCards },
                { BoardPieceId.HeroRogue, assassinCards },
                { BoardPieceId.HeroSorcerer, sorcererCards },
                { BoardPieceId.HeroWarlock, warlockCards },
            });

            var abilityAoeRule = new AbilityAoeAdjustedRule(new Dictionary<AbilityKey, int>
            {
                { AbilityKey.StrengthenCourage, 1 },
                { AbilityKey.Strength, 1 },
                { AbilityKey.Speed, 1 },
                { AbilityKey.MagicPotion, 1 },
                { AbilityKey.VigorPotion, 1 },
                { AbilityKey.DamageResistPotion, 1 },
                { AbilityKey.Antidote, 1 },
                { AbilityKey.Heal, 1 },
            });

            var enemyCooldownRule = new EnemyCooldownOverriddenRule(new Dictionary<AbilityKey, int>
            {
                { AbilityKey.AcidSpit, 2 },
            });

            var abilityMaxedRule = new RegainAbilityIfMaxxedOutOverriddenRule(new Dictionary<AbilityKey, bool>
            {
                { AbilityKey.Strength, false },
                { AbilityKey.Speed, false },
                { AbilityKey.MagicPotion, false },
                { AbilityKey.DamageResistPotion, false },
            });

            var piecesAdjustedRule = new PieceConfigAdjustedRule(new List<PieceConfigAdjustedRule.PieceProperty>
            {
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.Sigataur, Property = "PowerIndex", Value = 6 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.GiantSlime, Property = "PowerIndex", Value = 6 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.GiantSlime, Property = "StartHealth", Value = 50 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.HeroBarbarian, Property = "StartHealth", Value = 15 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.HeroBard, Property = "StartHealth", Value = 15 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.HeroGuardian, Property = "StartHealth", Value = 18 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.HeroHunter, Property = "StartHealth", Value = 15 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.HeroRogue, Property = "StartHealth", Value = 15 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.HeroRogue, Property = "MoveRange", Value = 5 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.HeroSorcerer, Property = "StartHealth", Value = 15 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.HeroWarlock, Property = "StartHealth", Value = 15 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.WarlockMinion, Property = "StartHealth", Value = 7 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.HeroBard, Property = "MoveRange", Value = 5 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.HeroHunter, Property = "MoveRange", Value = 5 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.MonsterBait, Property = "StartHealth", Value = 30 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.SmiteWard, Property = "StartHealth", Value = 20 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.SmiteWard, Property = "ActionPoint", Value = 2 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.SwordOfAvalon, Property = "StartHealth", Value = 20 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.SwordOfAvalon, Property = "ActionPoint", Value = 3 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.Verochka, Property = "ActionPoint", Value = 3 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.Verochka, Property = "MoveRange", Value = 6 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.Verochka, Property = "VisionRange", Value = 40 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.Slimeling, Property = "StartHealth", Value = 3 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.Slimeling, Property = "ActionPoint", Value = 3 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.Slimeling, Property = "MoveRange", Value = 2 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.Slimeling, Property = "PowerIndex", Value = 2 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.RatKing, Property = "StartHealth", Value = 120 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.ElvenQueen, Property = "StartHealth", Value = 90 },
            });

            var abilityActionCostRule = new AbilityActionCostAdjustedRule(new Dictionary<AbilityKey, bool>
            {
                { AbilityKey.Zap, false },
                { AbilityKey.MinionCharge, false },
                { AbilityKey.StrengthenCourage, false },
                { AbilityKey.Stealth, false },
                { AbilityKey.Grapple, false },
                { AbilityKey.HunterArrow, false },
                { AbilityKey.LeapHeavy, false },
            });

            var levelPropertiesRule = new LevelPropertiesModifiedRule(new Dictionary<string, int>
            {
                { "FloorOneGoldMaxAmount", 900 },
                { "FloorOneHealingFountains", 1 },
                { "FloorOneLootChests", 12 },
                { "FloorOnePotionStand", 2 },
                { "FloorOneElvenSummoners", 0 },
                { "FloorTwoHealingFountains", 2 },
                { "FloorTwoLootChests", 7 },
                { "FloorTwoPotionStand", 1 },
                { "FloorTwoElvenSummoners", 0 },
                { "FloorTwoGoldMaxAmount", 1100 },
                { "FloorThreeHealingFountains", 0 },
                { "FloorThreeLootChests", 0 },
                { "FloorThreeElvenSummoners", 0 },
            });

            var pieceUseWhenKilledRule = new PieceUseWhenKilledOverriddenRule(new Dictionary<BoardPieceId, List<AbilityKey>>
            {
                { BoardPieceId.Slimeling, new List<AbilityKey> { AbilityKey.EnemyDropStolenGoods } },
            });

            var entranceDeckFloor1 = new Dictionary<BoardPieceId, int>
            {
                { BoardPieceId.Slimeling, 0 }, // Unlimited slimes.
                { BoardPieceId.GiantSlime, 2 },
                { BoardPieceId.ElvenArcher, 1 },
            };
            var exitDeckFloor1 = new Dictionary<BoardPieceId, int>
            {
                { BoardPieceId.Slimeling, 0 }, // Unlimited slimes.
                { BoardPieceId.GiantSlime, 3 },
                { BoardPieceId.Sigataur, 1 },
            };
            var entranceDeckFloor2 = new Dictionary<BoardPieceId, int>
            {
                { BoardPieceId.Slimeling, 0 }, // Unlimited slimes.
                { BoardPieceId.GiantSlime, 4 },
                { BoardPieceId.DruidArcher, 3 },
            };
            var exitDeckFloor2 = new Dictionary<BoardPieceId, int>
            {
                { BoardPieceId.Slimeling, 0 }, // Unlimited slimes.
                { BoardPieceId.GiantSlime, 5 },
                { BoardPieceId.Sigataur, 1 },
            };
            var bossDeck = new Dictionary<BoardPieceId, int>
            {
                { BoardPieceId.Slimeling, 0 },
                { BoardPieceId.GiantSlime, 5 },
                { BoardPieceId.TheUnseen, 0 },
                { BoardPieceId.TheUnheard, 0 },
                { BoardPieceId.TheUnspoken, 0 },
                { BoardPieceId.DruidArcher, 3 },
                { BoardPieceId.ElvenPriest, 3 },
                { BoardPieceId.ElvenMystic, 3 },
                { BoardPieceId.Sigataur, 2 },
            };
            var myMonsterDeckConfig = new MyMonsterDeckOverriddenRule.MyDeckConfig
            {
                EntranceDeckFloor1 = entranceDeckFloor1,
                ExitDeckFloor1 = exitDeckFloor1,
                EntranceDeckFloor2 = entranceDeckFloor2,
                ExitDeckFloor2 = exitDeckFloor2,
                BossDeck = bossDeck,
                KeyHolderFloor1 = BoardPieceId.ElvenQueen,
                KeyHolderFloor2 = BoardPieceId.RatKing,
            };
            var myMonsterDeckRule = new MyMonsterDeckOverriddenRule(myMonsterDeckConfig);

            var pieceAbilityRule = new PieceAbilityListOverriddenRule(new Dictionary<BoardPieceId, List<AbilityKey>>
            {
                { BoardPieceId.Slimeling, new List<AbilityKey> { AbilityKey.AcidSpit, AbilityKey.EnemyStealCard, AbilityKey.EnemyStealGold } },
                { BoardPieceId.ElvenArcher, new List<AbilityKey> { AbilityKey.EnemyFrostball, AbilityKey.EnemyMelee, AbilityKey.EnemyArrowSnipe } },
            });

            var pieceBehaviorsRule = new PieceBehavioursListOverriddenRule(new Dictionary<BoardPieceId, List<Behaviour>>
            {
                { BoardPieceId.Slimeling, new List<Behaviour> { Behaviour.AttackAndRetreat, Behaviour.Patrol, Behaviour.RangedSpellCaster, Behaviour.FleeToFOW, Behaviour.SlimeFusion } },
                { BoardPieceId.ElvenArcher, new List<Behaviour> { Behaviour.Patrol, Behaviour.RangedSpellCaster, Behaviour.FollowPlayerRangedAttacker } },
            });

            var allowedChestCardsRule = new CardChestAdditionOverriddenRule(new Dictionary<BoardPieceId, List<AbilityKey>>
            {
                {
                    BoardPieceId.HeroBarbarian, new List<AbilityKey>
                    {
                        AbilityKey.EyeOfAvalon,
                        AbilityKey.Heal,
                        AbilityKey.Teleport,
                        AbilityKey.ExtraActionPotion,
                        AbilityKey.GodsFury,
                        AbilityKey.GrapplingTotem,
                        AbilityKey.PlayerLeap,
                        AbilityKey.MarkOfVerga,
                        AbilityKey.GrapplingSmash,
                        AbilityKey.GrapplingTotem,
                        AbilityKey.TauntingScream,
                        AbilityKey.PlayerLeap,
                        AbilityKey.MarkOfVerga,
                        AbilityKey.GrapplingPush,
                        AbilityKey.GrapplingSmash,
                        AbilityKey.TauntingScream,
                        AbilityKey.PlayerLeap,
                        AbilityKey.MarkOfVerga,
                        AbilityKey.GrapplingPush,
                        AbilityKey.GrapplingSmash,
                    }
                },
                {
                    BoardPieceId.HeroGuardian, new List<AbilityKey>
                    {
                        AbilityKey.EyeOfAvalon,
                        AbilityKey.Heal,
                        AbilityKey.Teleport,
                        AbilityKey.ExtraActionPotion,
                        AbilityKey.GodsFury,
                        AbilityKey.BeaconOfHealing,
                        AbilityKey.Whirlwind,
                        AbilityKey.WarCry,
                        AbilityKey.PiercingSpear,
                        AbilityKey.Charge,
                        AbilityKey.BeaconOfHealing,
                        AbilityKey.Whirlwind,
                        AbilityKey.WarCry,
                        AbilityKey.BeaconOfSmite,
                        AbilityKey.PiercingSpear,
                        AbilityKey.Charge,
                        AbilityKey.Whirlwind,
                        AbilityKey.WarCry,
                        AbilityKey.BeaconOfSmite,
                        AbilityKey.PiercingSpear,
                    }
                },
                {
                    BoardPieceId.HeroBard, new List<AbilityKey>
                    {
                        AbilityKey.EyeOfAvalon,
                        AbilityKey.Heal,
                        AbilityKey.Teleport,
                        AbilityKey.ExtraActionPotion,
                        AbilityKey.GodsFury,
                        AbilityKey.SongOfRecovery,
                        AbilityKey.SongOfResilience,
                        AbilityKey.BlockAbilities,
                        AbilityKey.NotesOfConfusion,
                        AbilityKey.ShatteringVoice,
                        AbilityKey.SongOfRecovery,
                        AbilityKey.Confuse,
                        AbilityKey.SongOfResilience,
                        AbilityKey.ShatteringVoice,
                        AbilityKey.Tornado,
                        AbilityKey.Confuse,
                        AbilityKey.SongOfRecovery,
                        AbilityKey.SongOfResilience,
                        AbilityKey.ShatteringVoice,
                        AbilityKey.Tornado,
                    }
                },
                {
                    BoardPieceId.HeroHunter, new List<AbilityKey>
                    {
                        AbilityKey.EyeOfAvalon,
                        AbilityKey.Heal,
                        AbilityKey.Teleport,
                        AbilityKey.ExtraActionPotion,
                        AbilityKey.GodsFury,
                        AbilityKey.Confuse,
                        AbilityKey.CallCompanion,
                        AbilityKey.PoisonedTip,
                        AbilityKey.MarkOfAvalon,
                        AbilityKey.MonsterBait,
                        AbilityKey.Confuse,
                        AbilityKey.SplittingArrow,
                        AbilityKey.CallCompanion,
                        AbilityKey.PoisonedTip,
                        AbilityKey.MarkOfAvalon,
                        AbilityKey.SplittingArrow,
                        AbilityKey.CallCompanion,
                        AbilityKey.MonsterBait,
                    }
                },
                {
                    BoardPieceId.HeroRogue, new List<AbilityKey>
                    {
                        AbilityKey.EyeOfAvalon,
                        AbilityKey.Heal,
                        AbilityKey.Teleport,
                        AbilityKey.ExtraActionPotion,
                        AbilityKey.GodsFury,
                        AbilityKey.Blink,
                        AbilityKey.PoisonGasGrenade,
                        AbilityKey.CoinFlip,
                        AbilityKey.CursedDagger,
                        AbilityKey.ProximityMine,
                        AbilityKey.Flashbang,
                        AbilityKey.Blink,
                        AbilityKey.PoisonGasGrenade,
                        AbilityKey.CursedDagger,
                        AbilityKey.Flashbang,
                        AbilityKey.Blink,
                        AbilityKey.PoisonGasGrenade,
                        AbilityKey.CursedDagger,
                        AbilityKey.ProximityMine,
                        AbilityKey.Flashbang,
                    }
                },
                {
                    BoardPieceId.HeroSorcerer, new List<AbilityKey>
                    {
                        AbilityKey.EyeOfAvalon,
                        AbilityKey.Heal,
                        AbilityKey.Teleport,
                        AbilityKey.ExtraActionPotion,
                        AbilityKey.GodsFury,
                        AbilityKey.Banish,
                        AbilityKey.Fireball,
                        AbilityKey.Freeze,
                        AbilityKey.VortexDust,
                        AbilityKey.Banish,
                        AbilityKey.Fireball,
                        AbilityKey.Freeze,
                        AbilityKey.MagicShield,
                        AbilityKey.MagicWall,
                        AbilityKey.VortexDust,
                        AbilityKey.Banish,
                        AbilityKey.Fireball,
                        AbilityKey.Freeze,
                        AbilityKey.MagicWall,
                        AbilityKey.VortexDust,
                    }
                },
                {
                    BoardPieceId.HeroWarlock, new List<AbilityKey>
                    {
                        AbilityKey.EyeOfAvalon,
                        AbilityKey.Heal,
                        AbilityKey.Teleport,
                        AbilityKey.ExtraActionPotion,
                        AbilityKey.GodsFury,
                        AbilityKey.Deflect,
                        AbilityKey.GuidingLight,
                        AbilityKey.Implode,
                        AbilityKey.MissileSwarm,
                        AbilityKey.Portal,
                        AbilityKey.Deflect,
                        AbilityKey.GuidingLight,
                        AbilityKey.Implode,
                        AbilityKey.MissileSwarm,
                        AbilityKey.Portal,
                        AbilityKey.Deflect,
                        AbilityKey.GuidingLight,
                        AbilityKey.Implode,
                        AbilityKey.MissileSwarm,
                        AbilityKey.Portal,
                    }
                },
            });

            var electricityDamageRule = new PartyElectricityDamageOverriddenRule(true);
            var enemyRespawnRule = new EnemyRespawnDisabledRule(true);
            var grappleUnhookedRule = new GrappleUnhookedRule(true);
            var backstabConfigRule = new BackstabConfigOverriddenRule(new List<BoardPieceId> { BoardPieceId.HeroBard, BoardPieceId.HeroRogue });

            var statusEffectRule = new StatusEffectConfigRule(new List<StatusEffectData>
            {
                new StatusEffectData
                {
                    effectStateType = EffectStateType.Invulnerable1,
                    durationTurns = 2,
                    clearOnNewLevel = false,
                    tickWhen = StatusEffectsConfig.TickWhen.EndTurn,
                },
            });

            return Ruleset.NewInstance(
                name,
                description,
                longdesc,
                blobophobiaRule,
                abilityDamageAllRule,
                startingCardsRule,
                abilityAoeRule,
                enemyCooldownRule,
                abilityMaxedRule,
                piecesAdjustedRule,
                abilityActionCostRule,
                levelPropertiesRule,
                myMonsterDeckRule,
                pieceAbilityRule,
                pieceBehaviorsRule,
                allowedChestCardsRule,
                electricityDamageRule,
                enemyRespawnRule,
                grappleUnhookedRule,
                backstabConfigRule,
                pieceUseWhenKilledRule,
                statusEffectRule);
        }
    }
}
