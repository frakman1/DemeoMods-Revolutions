namespace HouseRules.Essentials.Rulesets
{
    using System.Collections.Generic;
    using Boardgame.Board;
    using DataKeys;
    using global::Types;
    using HouseRules.Core.Types;
    using HouseRules.Essentials.Rules;

    internal static class SURVIVE
    {
        internal static Ruleset Create()
        {
            const string name = "SURVIVE!";
            const string displayname = "<b><color=#FF0000>S<color=#E80000>U<color=#CF0000>R<color=#B80000>V<color=#9F0000>I<color=#880000>V<color=#6F0000>E<color=#580000>!</b>";
            const string description = "Finish the cursed dungeon with only 1 life starting with 1 health!";
            const string longdesc = "\n<color=#770077>* * </color><color=#FF0000>Finish a CURSED DUNGEON With only 1 life!</color> <color=#770077>* *</color>\n<u><b>HIGHLY RECOMMENDED:</u></b> Don't play multiple of the same class!\n\n<color=#003300>ALL bosses and keyholders are immune to Hunter's Mark and Net\nHeroes can't directly hurt each other with any AoE abilities\nGain 1 life and 1 max life by defeating enemies\n50% chance to gain random effects by defeating enemies\nStealth, Leap, Zap, Arrow, Grapple, Feral Charge, and Courage Shanty are 0 action cost\nCritical hits replenish the above abilities OR instead give 1 more action to Assassin and Guardian</color>\nHero turn order is randomized after each round\n\n<color=#FFFFFF><b><u>Other ways to gain life and buffs:</u></b></color>\n<color=#090900>Break a lamp that kills something\nAssassin - Ballista kills\nBarbarian - Leviathan kills\nBard - Tornado kills\nGuardian - Behemoth kills\nHunter - Non-Boned Verochka kills\nSorcerer - Summoned Elemental kills\nWarlock - Cana kills\nWhoever hires him - Arly Owl kills</color>";

            var piecesAdjustedRule = new PieceConfigAdjustedRule(new List<PieceConfigAdjustedRule.PieceProperty>
            {
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.EarthElemental, Property = "PreciseAttack", Value = 1 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.EarthElemental, Property = "AttackDamage", Value = 3 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.SilentSentinel, Property = "PreciseHealth", Value = 1 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.SilentSentinel, Property = "PreciseAttack", Value = 1 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.SilentSentinel, Property = "StartHealth", Value = 50 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.SilentSentinel, Property = "AttackDamage", Value = 6 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.SilentSentinel, Property = "BerserkBelowHealth", Value = 0.5f },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.Wyvern, Property = "PreciseHealth", Value = 1 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.Wyvern, Property = "PreciseAttack", Value = 1 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.Wyvern, Property = "StartHealth", Value = 72 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.Wyvern, Property = "AttackDamage", Value = 8 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.Wyvern, Property = "BerserkBelowHealth", Value = 0.66f },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.Wyvern, Property = "MoveRange", Value = 4 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.HeroBarbarian, Property = "StartHealth", Value = 1 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.HeroBard, Property = "StartHealth", Value = 1 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.HeroGuardian, Property = "StartHealth", Value = 1 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.HeroGuardian, Property = "StartArmor", Value = 1 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.HeroHunter, Property = "StartHealth", Value = 1 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.HeroRogue, Property = "StartHealth", Value = 1 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.HeroSorcerer, Property = "StartHealth", Value = 1 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.HeroWarlock, Property = "StartHealth", Value = 1 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.Verochka, Property = "StartHealth", Value = 4 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.WarlockMinion, Property = "StartHealth", Value = 3 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.WarlockMinion, Property = "AttackDamage", Value = 2 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.MonsterBait, Property = "StartHealth", Value = 8 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.Tornado, Property = "ActionPoint", Value = 2 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.SellswordArbalestierActive, Property = "StartHealth", Value = 5 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.ElvenQueen, Property = "StartHealth", Value = 90 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.BossTown, Property = "StartHealth", Value = 155 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.RootLord, Property = "StartHealth", Value = 95 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.MotherCy, Property = "StartHealth", Value = 70 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.RatKing, Property = "StartHealth", Value = 135 },
                new PieceConfigAdjustedRule.PieceProperty { Piece = BoardPieceId.WizardBoss, Property = "StartHealth", Value = 125 },
            });

            var pieceImmunityRule = new PieceImmunityListAdjustedRule(new Dictionary<BoardPieceId, List<EffectStateType>>
            {
                { BoardPieceId.Wyvern, new List<EffectStateType> { EffectStateType.Stunned, EffectStateType.Frozen, EffectStateType.Tangled, EffectStateType.Blinded, EffectStateType.MarkOfAvalon, EffectStateType.Confused, EffectStateType.Disoriented, EffectStateType.AbilityBlocked, EffectStateType.Netted } },
                { BoardPieceId.ElvenQueen, new List<EffectStateType> { EffectStateType.Stunned, EffectStateType.Frozen, EffectStateType.Tangled, EffectStateType.Panic, EffectStateType.Blinded, EffectStateType.Confused, EffectStateType.Disoriented, EffectStateType.AbilityBlocked, EffectStateType.Netted, EffectStateType.MarkOfAvalon } },
                { BoardPieceId.RatKing, new List<EffectStateType> { EffectStateType.Stunned, EffectStateType.Frozen, EffectStateType.Panic, EffectStateType.Blinded, EffectStateType.Confused, EffectStateType.Diseased, EffectStateType.Disoriented, EffectStateType.AbilityBlocked, EffectStateType.Netted, EffectStateType.MarkOfAvalon } },
                { BoardPieceId.RootLord, new List<EffectStateType> { EffectStateType.Stunned, EffectStateType.Frozen, EffectStateType.Tangled, EffectStateType.Panic, EffectStateType.Blinded, EffectStateType.MarkOfAvalon, EffectStateType.Confused, EffectStateType.Disoriented, EffectStateType.AbilityBlocked, EffectStateType.Netted } },
                { BoardPieceId.MotherCy, new List<EffectStateType> { EffectStateType.Stunned, EffectStateType.Frozen, EffectStateType.Tangled, EffectStateType.Panic, EffectStateType.Blinded, EffectStateType.MarkOfAvalon, EffectStateType.Confused, EffectStateType.Disoriented, EffectStateType.AbilityBlocked, EffectStateType.Netted } },
                { BoardPieceId.WizardBoss, new List<EffectStateType> { EffectStateType.Stunned, EffectStateType.Frozen, EffectStateType.Tangled, EffectStateType.Panic, EffectStateType.Blinded, EffectStateType.MarkOfAvalon, EffectStateType.Diseased, EffectStateType.CorruptedRage, EffectStateType.Corruption, EffectStateType.Confused, EffectStateType.Disoriented, EffectStateType.AbilityBlocked, EffectStateType.Netted } },
                { BoardPieceId.BossTown, new List<EffectStateType> { EffectStateType.Stunned, EffectStateType.Frozen, EffectStateType.Tangled, EffectStateType.Panic, EffectStateType.Blinded, EffectStateType.MarkOfAvalon, EffectStateType.Confused, EffectStateType.Weaken1Turn, EffectStateType.ConfusedPermanentVisualOnly, EffectStateType.Petrified, EffectStateType.Disoriented, EffectStateType.AbilityBlocked, EffectStateType.Netted } },
            });

            var myEntranceDeckFloor1 = new Dictionary<BoardPieceId, int>
            {
                { BoardPieceId.ElvenSpearman, 1 },
                { BoardPieceId.ReptileArcher, 1 },
                { BoardPieceId.ReptileMutantWizard, 1 },
                { BoardPieceId.LargeCorruption, 2 },
                { BoardPieceId.GeneralRonthian, 1 },
                { BoardPieceId.ElvenArcher, 1 },
                { BoardPieceId.ElvenHound, 2 },
                { BoardPieceId.RootHound, 2 },
                { BoardPieceId.TheUnspoken, 2 },
                { BoardPieceId.Bandit, 2 },
                { BoardPieceId.DruidArcher, 2 },
                { BoardPieceId.DruidHoundMaster, 1 },
                { BoardPieceId.GoblinChieftan, 1 },
                { BoardPieceId.GoblinMadUn, 1 },
                { BoardPieceId.RootBeast, 2 },
                { BoardPieceId.ScabRat, 1 },
                { BoardPieceId.Spider, 2 },
                { BoardPieceId.Rat, 1 },
                { BoardPieceId.TheUnheard, 2 },
                { BoardPieceId.Slimeling, 3 },
                { BoardPieceId.Thug, 1 },
                { BoardPieceId.ElvenMystic, 2 },
                { BoardPieceId.ElvenSkirmisher, 1 },
                { BoardPieceId.GoblinFighter, 2 },
                { BoardPieceId.GoblinRanger, 3 },
                { BoardPieceId.ChestGoblin, 1 },
                { BoardPieceId.EarthElemental, 1 },
                { BoardPieceId.GiantSlime, 1 },
                { BoardPieceId.ElvenMarauder, 1 },
                { BoardPieceId.IceElemental, 2 },
                { BoardPieceId.GiantSpider, 1 },
                { BoardPieceId.ServantOfAlfaragh, 1 },
            };
            var myExitDeckFloor1 = new Dictionary<BoardPieceId, int>
            {
                { BoardPieceId.ElvenSpearman, 2 },
                { BoardPieceId.ReptileArcher, 1 },
                { BoardPieceId.ReptileMutantWizard, 1 },
                { BoardPieceId.LargeCorruption, 2 },
                { BoardPieceId.GeneralRonthian, 1 },
                { BoardPieceId.ElvenArcher, 1 },
                { BoardPieceId.ElvenHound, 1 },
                { BoardPieceId.RootHound, 2 },
                { BoardPieceId.TheUnspoken, 2 },
                { BoardPieceId.Bandit, 1 },
                { BoardPieceId.DruidArcher, 1 },
                { BoardPieceId.DruidHoundMaster, 1 },
                { BoardPieceId.GoblinChieftan, 1 },
                { BoardPieceId.GoblinMadUn, 1 },
                { BoardPieceId.RootBeast, 1 },
                { BoardPieceId.ScabRat, 1 },
                { BoardPieceId.Spider, 1 },
                { BoardPieceId.Rat, 1 },
                { BoardPieceId.TheUnheard, 2 },
                { BoardPieceId.Slimeling, 1 },
                { BoardPieceId.Thug, 1 },
                { BoardPieceId.ElvenMystic, 1 },
                { BoardPieceId.ElvenPriest, 1 },
                { BoardPieceId.ElvenSkirmisher, 1 },
                { BoardPieceId.GoblinFighter, 1 },
                { BoardPieceId.GoblinRanger, 1 },
                { BoardPieceId.EarthElemental, 1 },
                { BoardPieceId.Cavetroll, 1 },
                { BoardPieceId.ElvenMarauder, 1 },
                { BoardPieceId.Sigataur, 1 },
                { BoardPieceId.GiantSlime, 1 },
                { BoardPieceId.ServantOfAlfaragh, 1 },
            };
            var myEntranceDeckFloor2 = new Dictionary<BoardPieceId, int>
            {
                { BoardPieceId.ElvenSpearman, 2 },
                { BoardPieceId.LargeCorruption, 2 },
                { BoardPieceId.ReptileArcher, 1 },
                { BoardPieceId.ReptileMutantWizard, 1 },
                { BoardPieceId.SmallCorruption, 2 },
                { BoardPieceId.GeneralRonthian, 1 },
                { BoardPieceId.ElvenArcher, 1 },
                { BoardPieceId.ElvenHound, 1 },
                { BoardPieceId.RootHound, 2 },
                { BoardPieceId.TheUnspoken, 2 },
                { BoardPieceId.Bandit, 1 },
                { BoardPieceId.DruidArcher, 1 },
                { BoardPieceId.DruidHoundMaster, 1 },
                { BoardPieceId.GoblinChieftan, 1 },
                { BoardPieceId.GoblinMadUn, 1 },
                { BoardPieceId.RootBeast, 1 },
                { BoardPieceId.ScabRat, 2 },
                { BoardPieceId.Spider, 1 },
                { BoardPieceId.Rat, 1 },
                { BoardPieceId.TheUnheard, 2 },
                { BoardPieceId.Slimeling, 1 },
                { BoardPieceId.Thug, 1 },
                { BoardPieceId.SporeFungus, 2 },
                { BoardPieceId.ElvenMystic, 2 },
                { BoardPieceId.ElvenPriest, 1 },
                { BoardPieceId.ElvenSkirmisher, 1 },
                { BoardPieceId.GoblinFighter, 1 },
                { BoardPieceId.GoblinRanger, 2 },
                { BoardPieceId.Sigataur, 1 },
                { BoardPieceId.GiantSlime, 1 },
                { BoardPieceId.BigBoiMutant, 1 },
                { BoardPieceId.IceElemental, 2 },
                { BoardPieceId.ElvenMarauder, 2 },
                { BoardPieceId.GiantSpider, 2 },
                { BoardPieceId.ServantOfAlfaragh, 1 },
            };
            var myExitDeckFloor2 = new Dictionary<BoardPieceId, int>
            {
                { BoardPieceId.ElvenSpearman, 2 },
                { BoardPieceId.ReptileArcher, 1 },
                { BoardPieceId.ReptileMutantWizard, 1 },
                { BoardPieceId.LargeCorruption, 2 },
                { BoardPieceId.GeneralRonthian, 1 },
                { BoardPieceId.ElvenArcher, 1 },
                { BoardPieceId.ElvenHound, 1 },
                { BoardPieceId.RootHound, 2 },
                { BoardPieceId.TheUnspoken, 1 },
                { BoardPieceId.Bandit, 1 },
                { BoardPieceId.DruidArcher, 1 },
                { BoardPieceId.DruidHoundMaster, 1 },
                { BoardPieceId.GoblinChieftan, 1 },
                { BoardPieceId.GoblinMadUn, 1 },
                { BoardPieceId.RootBeast, 1 },
                { BoardPieceId.ScabRat, 2 },
                { BoardPieceId.Spider, 1 },
                { BoardPieceId.Rat, 1 },
                { BoardPieceId.TheUnheard, 2 },
                { BoardPieceId.Slimeling, 1 },
                { BoardPieceId.Thug, 1 },
                { BoardPieceId.ElvenMystic, 1 },
                { BoardPieceId.ElvenPriest, 1 },
                { BoardPieceId.ElvenSkirmisher, 1 },
                { BoardPieceId.GoblinFighter, 1 },
                { BoardPieceId.GoblinRanger, 1 },
                { BoardPieceId.SporeFungus, 2 },
                { BoardPieceId.ChestGoblin, 1 },
                { BoardPieceId.EarthElemental, 1 },
                { BoardPieceId.BigBoiMutant, 1 },
                { BoardPieceId.Sigataur, 1 },
                { BoardPieceId.GiantSlime, 1 },
                { BoardPieceId.ElvenMarauder, 1 },
                { BoardPieceId.IceElemental, 1 },
                { BoardPieceId.GiantSpider, 1 },
                { BoardPieceId.Cavetroll, 1 },
                { BoardPieceId.RootGolem, 1 },
                { BoardPieceId.Brookmare, 1 },
            };
            var myBossDeck = new Dictionary<BoardPieceId, int>
            {
                { BoardPieceId.ReptileArcher, 1 },
                { BoardPieceId.ReptileMutantWizard, 1 },
                { BoardPieceId.LargeCorruption, 1 },
                { BoardPieceId.SmallCorruption, 1 },
                { BoardPieceId.GeneralRonthian, 1 },
                { BoardPieceId.ElvenArcher, 1 },
                { BoardPieceId.ElvenHound, 1 },
                { BoardPieceId.RootHound, 1 },
                { BoardPieceId.TheUnspoken, 1 },
                { BoardPieceId.DruidArcher, 1 },
                { BoardPieceId.DruidHoundMaster, 1 },
                { BoardPieceId.GoblinChieftan, 1 },
                { BoardPieceId.GoblinMadUn, 1 },
                { BoardPieceId.RootBeast, 1 },
                { BoardPieceId.ScabRat, 2 },
                { BoardPieceId.Spider, 1 },
                { BoardPieceId.Rat, 2 },
                { BoardPieceId.TheUnheard, 2 },
                { BoardPieceId.Slimeling, 1 },
                { BoardPieceId.Thug, 1 },
                { BoardPieceId.ElvenMystic, 1 },
                { BoardPieceId.ElvenPriest, 1 },
                { BoardPieceId.ElvenSkirmisher, 1 },
                { BoardPieceId.EarthElemental, 1 },
                { BoardPieceId.Sigataur, 1 },
                { BoardPieceId.GiantSlime, 1 },
                { BoardPieceId.ElvenMarauder, 2 },
                { BoardPieceId.IceElemental, 2 },
                { BoardPieceId.GiantSpider, 2 },
                { BoardPieceId.Cavetroll, 1 },
                { BoardPieceId.ServantOfAlfaragh, 1 },
            };
            var myMonsterDeckConfig = new MyMonsterDeckOverriddenRule.MyDeckConfig
            {
                EntranceDeckFloor1 = myEntranceDeckFloor1,
                ExitDeckFloor1 = myExitDeckFloor1,
                EntranceDeckFloor2 = myEntranceDeckFloor2,
                ExitDeckFloor2 = myExitDeckFloor2,
                BossDeck = myBossDeck,
                KeyHolderFloor1 = BoardPieceId.SilentSentinel,
                KeyHolderFloor2 = BoardPieceId.Wyvern,
            };
            var myMonsterDeckRule = new MyMonsterDeckOverriddenRule(myMonsterDeckConfig);

            var barbarianCards = new List<StartCardsModifiedRule.CardConfig>
            {
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.Grapple, ReplenishFrequency = 1 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.Net, ReplenishFrequency = 1 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.Heal, ReplenishFrequency = 0 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.GrapplingPush, ReplenishFrequency = 0 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.GrapplingSmash, ReplenishFrequency = 0 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.GrapplingTotem, ReplenishFrequency = 0 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.PlayerLeap, ReplenishFrequency = 0 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.ExplodingLampPlaceholder, ReplenishFrequency = 2 },
            };
            var warlockCards = new List<StartCardsModifiedRule.CardConfig>
            {
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.MinionCharge, ReplenishFrequency = 1 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.MagicMissile, ReplenishFrequency = 1 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.MagicMissile, ReplenishFrequency = 1 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.Portal, ReplenishFrequency = 0 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.Heal, ReplenishFrequency = 0 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.Implode, ReplenishFrequency = 0 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.MissileSwarm, ReplenishFrequency = 0 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.Deflect, ReplenishFrequency = 0 },
            };
            var bardCards = new List<StartCardsModifiedRule.CardConfig>
            {
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.StrengthenCourage, ReplenishFrequency = 1 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.EnemyFlashbang, ReplenishFrequency = 2 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.Heal, ReplenishFrequency = 0 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.Tornado, ReplenishFrequency = 0 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.SongOfRecovery, ReplenishFrequency = 0 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.ShatteringVoice, ReplenishFrequency = 0 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.NotesOfConfusion, ReplenishFrequency = 0 },
            };
            var guardianCards = new List<StartCardsModifiedRule.CardConfig>
            {
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.LeapHeavy, ReplenishFrequency = 1 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.ReplenishArmor, ReplenishFrequency = 1 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.Heal, ReplenishFrequency = 0 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.Whirlwind, ReplenishFrequency = 0 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.PiercingSpear, ReplenishFrequency = 0 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.BeaconOfSmite, ReplenishFrequency = 0 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.WarCry, ReplenishFrequency = 0 },
            };
            var hunterCards = new List<StartCardsModifiedRule.CardConfig>
            {
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.HunterArrow, ReplenishFrequency = 1 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.EnemyFireball, ReplenishFrequency = 1 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.Heal, ReplenishFrequency = 0 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.Exterminate, ReplenishFrequency = 0 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.PoisonedTip, ReplenishFrequency = 0 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.CallCompanion, ReplenishFrequency = 0 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.MonsterBait, ReplenishFrequency = 0 },
            };
            var assassinCards = new List<StartCardsModifiedRule.CardConfig>
            {
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.Stealth, ReplenishFrequency = 1 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.DiseasedBite, ReplenishFrequency = 2 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.Heal, ReplenishFrequency = 0 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.Blink, ReplenishFrequency = 0 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.SwordOfAvalon, ReplenishFrequency = 0 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.CursedDagger, ReplenishFrequency = 0 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.Flashbang, ReplenishFrequency = 0 },
            };
            var sorcererCards = new List<StartCardsModifiedRule.CardConfig>
            {
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.Zap, ReplenishFrequency = 1 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.Electricity, ReplenishFrequency = 1 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.Heal, ReplenishFrequency = 0 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.Fireball, ReplenishFrequency = 0 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.Freeze, ReplenishFrequency = 0 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.Implosion, ReplenishFrequency = 0 },
                new StartCardsModifiedRule.CardConfig { Card = AbilityKey.SummonElemental, ReplenishFrequency = 0 },
            };
            var startingCardsRule = new StartCardsModifiedRule(new Dictionary<BoardPieceId, List<StartCardsModifiedRule.CardConfig>>
            {
                { BoardPieceId.HeroWarlock, warlockCards },
                { BoardPieceId.HeroBard, bardCards },
                { BoardPieceId.HeroGuardian, guardianCards },
                { BoardPieceId.HeroHunter, hunterCards },
                { BoardPieceId.HeroRogue, assassinCards },
                { BoardPieceId.HeroSorcerer, sorcererCards },
                { BoardPieceId.HeroBarbarian, barbarianCards },
            });

            var allowedChestCardsRule = new CardChestAdditionOverriddenRule(new Dictionary<BoardPieceId, List<AbilityKey>>
            {
                {
                    BoardPieceId.HeroBarbarian, new List<AbilityKey>
                    {
                        AbilityKey.EyeOfAvalon,
                        AbilityKey.Heal,
                        AbilityKey.Teleport,
                        AbilityKey.Strength,
                        AbilityKey.Speed,
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
                        AbilityKey.Strength,
                        AbilityKey.Speed,
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
                        AbilityKey.Strength,
                        AbilityKey.Speed,
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
                        AbilityKey.Strength,
                        AbilityKey.Speed,
                        AbilityKey.ExtraActionPotion,
                        AbilityKey.GodsFury,
                        AbilityKey.Confuse,
                        AbilityKey.CallCompanion,
                        AbilityKey.PoisonedTip,
                        AbilityKey.MarkOfAvalon,
                        AbilityKey.MonsterBait,
                        AbilityKey.Confuse,
                        AbilityKey.Exterminate,
                        AbilityKey.PoisonedTip,
                        AbilityKey.Exterminate,
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
                        AbilityKey.Strength,
                        AbilityKey.Speed,
                        AbilityKey.ExtraActionPotion,
                        AbilityKey.GodsFury,
                        AbilityKey.Blink,
                        AbilityKey.PoisonGasGrenade,
                        AbilityKey.CoinFlip,
                        AbilityKey.CursedDagger,
                        AbilityKey.ProximityMine,
                        AbilityKey.Flashbang,
                        AbilityKey.SwordOfAvalon,
                        AbilityKey.Blink,
                        AbilityKey.PoisonGasGrenade,
                        AbilityKey.CursedDagger,
                        AbilityKey.Flashbang,
                        AbilityKey.Blink,
                        AbilityKey.PoisonGasGrenade,
                        AbilityKey.CursedDagger,
                        AbilityKey.ProximityMine,
                        AbilityKey.Flashbang,
                        AbilityKey.SwordOfAvalon,
                    }
                },
                {
                    BoardPieceId.HeroSorcerer, new List<AbilityKey>
                    {
                        AbilityKey.EyeOfAvalon,
                        AbilityKey.Heal,
                        AbilityKey.Teleport,
                        AbilityKey.Speed,
                        AbilityKey.ExtraActionPotion,
                        AbilityKey.GodsFury,
                        AbilityKey.MagicPotion,
                        AbilityKey.Banish,
                        AbilityKey.Fireball,
                        AbilityKey.Freeze,
                        AbilityKey.Implosion,
                        AbilityKey.SummonElemental,
                        AbilityKey.Banish,
                        AbilityKey.Fireball,
                        AbilityKey.Freeze,
                        AbilityKey.MagicShield,
                        AbilityKey.MagicWall,
                        AbilityKey.Implosion,
                        AbilityKey.Banish,
                        AbilityKey.Fireball,
                        AbilityKey.Freeze,
                        AbilityKey.MagicWall,
                        AbilityKey.Implosion,
                        AbilityKey.SummonElemental,
                    }
                },
                {
                    BoardPieceId.HeroWarlock, new List<AbilityKey>
                    {
                        AbilityKey.EyeOfAvalon,
                        AbilityKey.Heal,
                        AbilityKey.Teleport,
                        AbilityKey.Speed,
                        AbilityKey.ExtraActionPotion,
                        AbilityKey.GodsFury,
                        AbilityKey.MagicPotion,
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
                        AbilityKey.MinionCharge,
                    }
                },
            });

            var allowedEnergyCardsRule = new CardEnergyAdditionOverriddenRule(new Dictionary<BoardPieceId, List<AbilityKey>>
            {
                {
                    BoardPieceId.HeroBarbarian, new List<AbilityKey>
                    {
                        AbilityKey.Bone,
                        AbilityKey.WebBomb,
                        AbilityKey.SodiumHydroxide,
                        AbilityKey.FreeAP,
                        AbilityKey.GrapplingTotem,
                        AbilityKey.ScarePowder,
                        AbilityKey.SodiumHydroxide,
                        AbilityKey.VialOfIceImmunity,
                        AbilityKey.VialOfFireImmunity,
                        AbilityKey.ScrollTsunami,
                        AbilityKey.Regroup,
                        AbilityKey.WaterBottle,
                        AbilityKey.LuckPotion,
                        AbilityKey.ScrollElectricity,
                        AbilityKey.MarkOfVerga,
                    }
                },
                {
                    BoardPieceId.HeroGuardian, new List<AbilityKey>
                    {
                        AbilityKey.Bone,
                        AbilityKey.WebBomb,
                        AbilityKey.SodiumHydroxide,
                        AbilityKey.FreeAP,
                        AbilityKey.BeaconOfSmite,
                        AbilityKey.ScarePowder,
                        AbilityKey.SodiumHydroxide,
                        AbilityKey.VialOfIceImmunity,
                        AbilityKey.ScrollTsunami,
                        AbilityKey.Regroup,
                        AbilityKey.WaterBottle,
                        AbilityKey.LuckPotion,
                        AbilityKey.ScrollElectricity,
                        AbilityKey.Whirlwind,
                    }
                },
                {
                    BoardPieceId.HeroBard, new List<AbilityKey>
                    {
                        AbilityKey.Bone,
                        AbilityKey.WebBomb,
                        AbilityKey.SodiumHydroxide,
                        AbilityKey.Tornado,
                        AbilityKey.FreeAP,
                        AbilityKey.SodiumHydroxide,
                        AbilityKey.VialOfIceImmunity,
                        AbilityKey.VialOfFireImmunity,
                        AbilityKey.ScrollTsunami,
                        AbilityKey.Regroup,
                        AbilityKey.WaterBottle,
                        AbilityKey.LuckPotion,
                        AbilityKey.ScrollElectricity,
                        AbilityKey.Confuse,
                    }
                },
                {
                    BoardPieceId.HeroHunter, new List<AbilityKey>
                    {
                        AbilityKey.WebBomb,
                        AbilityKey.SodiumHydroxide,
                        AbilityKey.FreeAP,
                        AbilityKey.ScarePowder,
                        AbilityKey.SodiumHydroxide,
                        AbilityKey.VialOfFireImmunity,
                        AbilityKey.ScrollTsunami,
                        AbilityKey.Regroup,
                        AbilityKey.Exterminate,
                        AbilityKey.WaterBottle,
                        AbilityKey.LuckPotion,
                        AbilityKey.ScrollElectricity,
                        AbilityKey.MarkOfAvalon,
                    }
                },
                {
                    BoardPieceId.HeroRogue, new List<AbilityKey>
                    {
                        AbilityKey.Bone,
                        AbilityKey.WebBomb,
                        AbilityKey.SodiumHydroxide,
                        AbilityKey.SwordOfAvalon,
                        AbilityKey.FreeAP,
                        AbilityKey.ScarePowder,
                        AbilityKey.SodiumHydroxide,
                        AbilityKey.VialOfIceImmunity,
                        AbilityKey.VialOfFireImmunity,
                        AbilityKey.ScrollTsunami,
                        AbilityKey.Regroup,
                        AbilityKey.WaterBottle,
                        AbilityKey.LuckPotion,
                        AbilityKey.ScrollElectricity,
                        AbilityKey.Blink,
                    }
                },
                {
                    BoardPieceId.HeroSorcerer, new List<AbilityKey>
                    {
                        AbilityKey.Bone,
                        AbilityKey.WebBomb,
                        AbilityKey.SodiumHydroxide,
                        AbilityKey.SummonElemental,
                        AbilityKey.FreeAP,
                        AbilityKey.ScarePowder,
                        AbilityKey.SodiumHydroxide,
                        AbilityKey.VialOfIceImmunity,
                        AbilityKey.VialOfFireImmunity,
                        AbilityKey.ScrollTsunami,
                        AbilityKey.Regroup,
                        AbilityKey.LuckPotion,
                        AbilityKey.ScrollElectricity,
                        AbilityKey.Fireball,
                    }
                },
                {
                    BoardPieceId.HeroWarlock, new List<AbilityKey>
                    {
                        AbilityKey.Bone,
                        AbilityKey.WebBomb,
                        AbilityKey.SodiumHydroxide,
                        AbilityKey.FreeAP,
                        AbilityKey.ScarePowder,
                        AbilityKey.SodiumHydroxide,
                        AbilityKey.VialOfIceImmunity,
                        AbilityKey.VialOfFireImmunity,
                        AbilityKey.ScrollTsunami,
                        AbilityKey.Regroup,
                        AbilityKey.WaterBottle,
                        AbilityKey.LuckPotion,
                        AbilityKey.ScrollElectricity,
                        AbilityKey.Portal,
                    }
                },
            });

            var allowedPotionsRule = new PotionAdditionOverriddenRule(new Dictionary<BoardPieceId, List<AbilityKey>>
            {
                {
                    BoardPieceId.HeroBarbarian, new List<AbilityKey>
                    {
                        AbilityKey.Rejuvenation,
                        AbilityKey.DamageResistPotion,
                        AbilityKey.InvisibilityPotion,
                        AbilityKey.DamageResistPotion,
                        AbilityKey.LuckPotion,
                        AbilityKey.ExtraActionPotion,
                        AbilityKey.Strength,
                        AbilityKey.Speed,
                        AbilityKey.VigorPotion,
                    }
                },
                {
                    BoardPieceId.HeroBard, new List<AbilityKey>
                    {
                        AbilityKey.Rejuvenation,
                        AbilityKey.DamageResistPotion,
                        AbilityKey.InvisibilityPotion,
                        AbilityKey.DamageResistPotion,
                        AbilityKey.LuckPotion,
                        AbilityKey.ExtraActionPotion,
                        AbilityKey.Strength,
                        AbilityKey.Speed,
                        AbilityKey.VigorPotion,
                    }
                },
                {
                    BoardPieceId.HeroGuardian, new List<AbilityKey>
                    {
                        AbilityKey.Rejuvenation,
                        AbilityKey.DamageResistPotion,
                        AbilityKey.InvisibilityPotion,
                        AbilityKey.DamageResistPotion,
                        AbilityKey.LuckPotion,
                        AbilityKey.ExtraActionPotion,
                        AbilityKey.Strength,
                        AbilityKey.Speed,
                        AbilityKey.VigorPotion,
                    }
                },
                {
                    BoardPieceId.HeroHunter, new List<AbilityKey>
                    {
                        AbilityKey.Rejuvenation,
                        AbilityKey.DamageResistPotion,
                        AbilityKey.InvisibilityPotion,
                        AbilityKey.DamageResistPotion,
                        AbilityKey.LuckPotion,
                        AbilityKey.ExtraActionPotion,
                        AbilityKey.Strength,
                        AbilityKey.Speed,
                        AbilityKey.VigorPotion,
                    }
                },
                {
                    BoardPieceId.HeroRogue, new List<AbilityKey>
                    {
                        AbilityKey.Rejuvenation,
                        AbilityKey.DamageResistPotion,
                        AbilityKey.InvisibilityPotion,
                        AbilityKey.DamageResistPotion,
                        AbilityKey.LuckPotion,
                        AbilityKey.ExtraActionPotion,
                        AbilityKey.Strength,
                        AbilityKey.Speed,
                        AbilityKey.VigorPotion,
                    }
                },
                {
                    BoardPieceId.HeroSorcerer, new List<AbilityKey>
                    {
                        AbilityKey.Rejuvenation,
                        AbilityKey.DamageResistPotion,
                        AbilityKey.InvisibilityPotion,
                        AbilityKey.DamageResistPotion,
                        AbilityKey.LuckPotion,
                        AbilityKey.ExtraActionPotion,
                        AbilityKey.MagicPotion,
                        AbilityKey.Speed,
                        AbilityKey.VigorPotion,
                    }
                },
                {
                    BoardPieceId.HeroWarlock, new List<AbilityKey>
                    {
                        AbilityKey.Rejuvenation,
                        AbilityKey.DamageResistPotion,
                        AbilityKey.InvisibilityPotion,
                        AbilityKey.DamageResistPotion,
                        AbilityKey.LuckPotion,
                        AbilityKey.ExtraActionPotion,
                        AbilityKey.MagicPotion,
                        AbilityKey.Speed,
                        AbilityKey.VigorPotion,
                    }
                },
            });

            var pieceAbilityRule = new PieceAbilityListOverriddenRule(new Dictionary<BoardPieceId, List<AbilityKey>>
            {
                { BoardPieceId.EarthElemental, new List<AbilityKey> { AbilityKey.EnemyMelee, AbilityKey.EnemyKnockbackMelee, AbilityKey.EarthShatter, AbilityKey.Grapple } },
                { BoardPieceId.Wyvern, new List<AbilityKey> { AbilityKey.EnemyMelee, AbilityKey.AcidSpit, AbilityKey.Grapple, AbilityKey.PlayerLeap, AbilityKey.EnemyFrostball, AbilityKey.LightningBolt } },
                { BoardPieceId.ChestGoblin, new List<AbilityKey> { AbilityKey.EnemyMelee, AbilityKey.EnemyStealGold, AbilityKey.Net } },
                { BoardPieceId.SilentSentinel, new List<AbilityKey> { AbilityKey.EnemyMelee, AbilityKey.Leap, AbilityKey.Grapple, AbilityKey.Petrify, AbilityKey.Zap } },
                { BoardPieceId.ElvenArcher, new List<AbilityKey> { AbilityKey.EnemyMelee, AbilityKey.EnemyArrowSnipe, AbilityKey.EnemyFrostball } },
                { BoardPieceId.ElvenQueen, new List<AbilityKey> { AbilityKey.SpawnBossMinions, AbilityKey.LightningBolt, AbilityKey.Shockwave, AbilityKey.EnemyFrostball } },
                { BoardPieceId.GoblinFighter, new List<AbilityKey> { AbilityKey.EnemyMelee, AbilityKey.EnemyFlashbang } },
                { BoardPieceId.TheUnseen, new List<AbilityKey> { AbilityKey.EnemyMelee, AbilityKey.Zap } },
                { BoardPieceId.Rat, new List<AbilityKey> { AbilityKey.EnemyMelee, AbilityKey.DiseasedBite } },
                { BoardPieceId.Spider, new List<AbilityKey> { AbilityKey.EnemyMelee, AbilityKey.DiseasedBite } },
                { BoardPieceId.GiantSpider, new List<AbilityKey> { AbilityKey.EnemyMelee, AbilityKey.SpiderWebshot } },
                { BoardPieceId.Cavetroll, new List<AbilityKey> { AbilityKey.EnemyMelee, AbilityKey.ElvenKingMeleeWhip } },
                { BoardPieceId.ScabRat, new List<AbilityKey> { AbilityKey.EnemyMelee, AbilityKey.DiseasedBite } },
            });

            var pieceBehaviourListRule = new PieceBehavioursListOverriddenRule(new Dictionary<BoardPieceId, List<Behaviour>>
            {
                { BoardPieceId.EarthElemental, new List<Behaviour> { Behaviour.Patrol, Behaviour.AttackPlayer, Behaviour.EarthShatter, Behaviour.RangedAttackHighPrio } },
                { BoardPieceId.Wyvern, new List<Behaviour> { Behaviour.Patrol, Behaviour.AttackPlayer, Behaviour.RangedAttackHighPrio } },
                { BoardPieceId.ChestGoblin, new List<Behaviour> { Behaviour.Patrol, Behaviour.FollowPlayerMeleeAttacker, Behaviour.AttackAndRetreat, Behaviour.RangedAttackHighPrio } },
                { BoardPieceId.SilentSentinel, new List<Behaviour> { Behaviour.Patrol, Behaviour.AttackPlayer, Behaviour.RangedSpellCaster } },
                { BoardPieceId.ElvenArcher, new List<Behaviour> { Behaviour.Patrol, Behaviour.RangedSpellCaster, Behaviour.FollowPlayerRangedAttacker } },
                { BoardPieceId.GoblinFighter, new List<Behaviour> { Behaviour.Patrol, Behaviour.AttackPlayer, Behaviour.RangedAttackHighPrio } },
                { BoardPieceId.TheUnseen, new List<Behaviour> { Behaviour.Patrol, Behaviour.AttackPlayer, Behaviour.RangedAttackHighPrio } },
                { BoardPieceId.GiantSpider, new List<Behaviour> { Behaviour.Patrol, Behaviour.AttackPlayer, Behaviour.RangedAttackHighPrio } },
                { BoardPieceId.Cavetroll, new List<Behaviour> { Behaviour.Patrol, Behaviour.AttackPlayer, Behaviour.RangedAttackHighPrio } },
                { BoardPieceId.ScabRat, new List<Behaviour> { Behaviour.Patrol, Behaviour.Swarm, Behaviour.AttackPlayer } },
                { BoardPieceId.JeweledScarab, new List<Behaviour> { Behaviour.Patrol, Behaviour.FleeToFOW } },
            });

            var abilityDamageAllRule = new AbilityDamageAllOverriddenRule(new Dictionary<AbilityKey, List<int>>
            {
                { AbilityKey.GrapplingTotemHook, new List<int> { 2, 2, 2, 2 } },
            });

            var pieceUseWhenKilledRule = new PieceUseWhenKilledOverriddenRule(new Dictionary<BoardPieceId, List<AbilityKey>>
            {
                { BoardPieceId.ChestGoblin, new List<AbilityKey> { AbilityKey.EnemyDropStolenGoods, AbilityKey.DropChest } },
                { BoardPieceId.EarthElemental, new List<AbilityKey> { AbilityKey.Explosion } },
            });

            var statusEffectRule = new StatusEffectConfigRule(new List<StatusEffectData>
            {
                new StatusEffectData
                {
                    effectStateType = EffectStateType.Torch,
                    durationTurns = 6,
                    damagePerTurn = 0,
                    clearOnNewLevel = false,
                    tickWhen = StatusEffectsConfig.TickWhen.StartTurn,
                },
                new StatusEffectData
                {
                    effectStateType = EffectStateType.TorchPlayer,
                    durationTurns = 10,
                    damagePerTurn = 0,
                    clearOnNewLevel = false,
                    tickWhen = StatusEffectsConfig.TickWhen.StartTurn,
                },
                new StatusEffectData
                {
                    effectStateType = EffectStateType.Netted,
                    durationTurns = 1,
                    damagePerTurn = 0,
                    clearOnNewLevel = false,
                    tickWhen = StatusEffectsConfig.TickWhen.StartTurn,
                },
                new StatusEffectData
                {
                    effectStateType = EffectStateType.Netted,
                    durationTurns = 1,
                    damagePerTurn = 0,
                    clearOnNewLevel = false,
                    tickWhen = StatusEffectsConfig.TickWhen.EndTurn,
                },
                new StatusEffectData
                {
                    effectStateType = EffectStateType.PlayerBerserk,
                    durationTurns = 1,
                    damagePerTurn = 0,
                    clearOnNewLevel = false,
                    tickWhen = StatusEffectsConfig.TickWhen.EndTurn,
                },
            });

            var aoeAdjustedRule = new AbilityAoeAdjustedRule(new Dictionary<AbilityKey, int>
            {
                { AbilityKey.BlindingLight, 2 },
                { AbilityKey.LeapHeavy, 1 },
                { AbilityKey.Leap, 1 },
                { AbilityKey.Net, 0 },
            });

            var enemyCooldownRule = new EnemyCooldownOverriddenRule(new Dictionary<AbilityKey, int>
            {
                { AbilityKey.Zap, 2 },
                { AbilityKey.LightningBolt, 3 },
                { AbilityKey.LeapHeavy, 2 },
                { AbilityKey.EnemyFrostball, 2 },
                { AbilityKey.Shockwave, 3 },
                { AbilityKey.EnemyFireball, 2 },
                { AbilityKey.EnemyFlashbang, 2 },
                { AbilityKey.Petrify, 2 },
                { AbilityKey.Net, 2 },
                { AbilityKey.Grapple, 2 },
                { AbilityKey.ElvenSummonerDeflect, 3 },
                { AbilityKey.PlayerLeap, 2 },
            });

            var targetEffectRule = new AbilityTargetEffectsRule(new Dictionary<AbilityKey, List<EffectStateType>>
            {
                { AbilityKey.Javelin, new List<EffectStateType> { EffectStateType.Weaken1Turn } },
            });

            var freeReplenishablesOnCritRule = new FreeReplenishablesOnCritRule(new List<BoardPieceId>
            {
                { BoardPieceId.HeroBarbarian },
                { BoardPieceId.HeroBard },
                { BoardPieceId.HeroGuardian },
                { BoardPieceId.HeroRogue },
                { BoardPieceId.HeroSorcerer },
                { BoardPieceId.HeroHunter },
                { BoardPieceId.HeroWarlock },
            });

            var freeMaxHealthOnKillRule = new FreeMaxHealthOnKillRule(new Dictionary<BoardPieceId, int>
            {
                { BoardPieceId.HeroRogue, 1 },
                { BoardPieceId.HeroWarlock, 1 },
                { BoardPieceId.HeroBard, 1 },
                { BoardPieceId.HeroBarbarian, 1 },
                { BoardPieceId.HeroGuardian, 1 },
                { BoardPieceId.HeroSorcerer, 1 },
                { BoardPieceId.HeroHunter, 1 },
            });

            var freeActionPointsOnCritRule = new FreeActionPointsOnCritRule(new List<BoardPieceId>
            {
                { BoardPieceId.HeroGuardian },
                { BoardPieceId.HeroRogue },
            });

            var freeRandomBuffOnKillRule = new FreeRandomBuffOnKillRule(new Dictionary<BoardPieceId, float>
            {
                { BoardPieceId.HeroBarbarian, .5f },
                { BoardPieceId.HeroBard, .5f },
                { BoardPieceId.HeroGuardian, .5f },
                { BoardPieceId.HeroRogue, .5f },
                { BoardPieceId.HeroSorcerer, .5f },
                { BoardPieceId.HeroHunter, .5f },
                { BoardPieceId.HeroWarlock, .5f },
            });

            var abilityActionCostRule = new AbilityActionCostAdjustedRule(new Dictionary<AbilityKey, bool>
            {
                { AbilityKey.Zap, false },
                { AbilityKey.LightningBolt, false },
                { AbilityKey.Stealth, false },
                { AbilityKey.HunterArrow, false },
                { AbilityKey.StrengthenCourage, false },
                { AbilityKey.MinionCharge, false },
                { AbilityKey.SpellPower, false },
                { AbilityKey.PVPBlink, false },
                { AbilityKey.PVPMissileSwarm, false },
                { AbilityKey.PVPFireball, false },
                { AbilityKey.WeakeningShout, false },
                { AbilityKey.LeapHeavy, false },
                { AbilityKey.SpawnRandomLamp, false },
                { AbilityKey.DeathBeam, false },
                { AbilityKey.Grapple, false },
                { AbilityKey.Net, true },
                { AbilityKey.ImplosionExplosionRain, false },
            });

            var statModifiersRule = new StatModifiersOverriddenRule(new Dictionary<AbilityKey, int>
            {
                { AbilityKey.ReplenishArmor, 1 },
            });

            var pieceDownedCountRule = new PieceDownedCountAdjustedRule(new Dictionary<BoardPieceId, int>
            {
                { BoardPieceId.HeroGuardian, 0 },
                { BoardPieceId.HeroHunter, 0 },
                { BoardPieceId.HeroBard, 0 },
                { BoardPieceId.HeroBarbarian, 0 },
                { BoardPieceId.HeroRogue, 0 },
                { BoardPieceId.HeroWarlock, 0 },
                { BoardPieceId.HeroSorcerer, 0 },
            });

            var pieceDamageResistRule = new PieceDamageResistRule(new List<BoardPieceId>
            {
                { BoardPieceId.ElvenQueen },
                { BoardPieceId.WizardBoss },
                { BoardPieceId.BossTown },
                { BoardPieceId.MotherCy },
                { BoardPieceId.RootLord },
                { BoardPieceId.RatKing },
                { BoardPieceId.SilentSentinel },
                { BoardPieceId.Wyvern },
            });

            var backstabConfigRule = new BackstabConfigOverriddenRule(new List<BoardPieceId>
            {
                { BoardPieceId.HeroBard },
                { BoardPieceId.HeroRogue },
            });

            var abilityBackstabRule = new AbilityBackstabAdjustedRule(new Dictionary<AbilityKey, bool>
            {
                { AbilityKey.NotesOfConfusion, true },
                { AbilityKey.ShatteringVoice, true },
                { AbilityKey.DiseasedBite, true },
            });

            var breaksStealthRule = new AbilityBreaksStealthAdjustedRule(new Dictionary<AbilityKey, bool>
            {
                { AbilityKey.PoisonGasGrenade, false },
                { AbilityKey.DiseasedBite, false },
            });

            var queenBuffs = new ElvenQueenBuffsRule(true);
            var queenSuperBuffs = new ElvenQueenSuperBuffRule(true);
            var enemyHealthScaledRule = new EnemyHealthScaledRule(1.0f);
            var enemyAttackScaledRule = new EnemyAttackScaledRule(1.0f);
            var enableDoorsRule = new EnemyDoorOpeningEnabledRule(true);
            var pieceKeyholderRule = new PieceKeyholderRule(true);
            var enemyRespawnDisabledRule = new EnemyRespawnDisabledRule(true);
            var grappleUnhookedRule = new GrappleUnhookedRule(true);
            var goldPickupRule = new GoldPickedUpMultipliedRule(1);
            var reviveEffectsRule = new ReviveRemovesEffectsRule(true);
            var courageShantyRule = new StrengthenCourageAddsHpRule(1);
            var partyDamageRule = new PartyDamageOverriddenRule(true);
            var turnOrderRandomRule = new TurnOrderRandomizedRule(true);

            var smallLevelSequenceRule = new SmallLevelSequenceOverriddenRule(new List<string>
            {
                "TownsFloor05",
                "SewersFloor08",
                "SewersFloor11",
                "ForestFloor02",
                "ForestFloor01",
                "ElvenFloor14",
            });

            var levelPropertiesRule = new LevelPropertiesModifiedRule(new Dictionary<string, int>
            {
                { "FloorOnePotionStand", 1 },
                { "FloorOneMerchant", 0 },
                { "FloorOneLootChests", 6 },
                { "FloorOneSellswords", 1 },
                { "FloorTwoPotionStand", 1 },
                { "FloorTwoVillagers", 1 },
                { "FloorTwoLootChests", 8 },
                { "FloorTwoSellswords", 1 },
                { "FloorThreeBeggars", 1 },
                { "FloorThreeHealingFountains", 1 },
                { "FloorThreeLootChests", 4 },
            });

            return Ruleset.NewInstance(
                name,
                displayname,
                description,
                longdesc,
                piecesAdjustedRule,
                pieceImmunityRule,
                myMonsterDeckRule,
                startingCardsRule,
                allowedChestCardsRule,
                allowedEnergyCardsRule,
                allowedPotionsRule,
                pieceAbilityRule,
                pieceBehaviourListRule,
                abilityDamageAllRule,
                pieceUseWhenKilledRule,
                statusEffectRule,
                aoeAdjustedRule,
                targetEffectRule,
                enemyCooldownRule,
                freeReplenishablesOnCritRule,
                freeActionPointsOnCritRule,
                freeMaxHealthOnKillRule,
                freeRandomBuffOnKillRule,
                abilityActionCostRule,
                statModifiersRule,
                pieceDownedCountRule,
                pieceDamageResistRule,
                backstabConfigRule,
                abilityBackstabRule,
                breaksStealthRule,
                queenBuffs,
                queenSuperBuffs,
                enemyHealthScaledRule,
                enemyAttackScaledRule,
                enableDoorsRule,
                pieceKeyholderRule,
                enemyRespawnDisabledRule,
                grappleUnhookedRule,
                goldPickupRule,
                reviveEffectsRule,
                courageShantyRule,
                partyDamageRule,
                turnOrderRandomRule,
                smallLevelSequenceRule,
                levelPropertiesRule);
        }
    }
}
