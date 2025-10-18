namespace HouseRules.Essentials.Rules
{
    using System.Collections.Generic;
    using Boardgame.BoardEntities;
    using Boardgame.BoardEntities.Abilities;
    using DataKeys;
    using HarmonyLib;
    using HouseRules.Core;
    using HouseRules.Core.Types;

    public sealed class FreeAbilityOnCritRule : Rule, IConfigWritable<Dictionary<BoardPieceId, AbilityKey>>, IPatchable,
        IMultiplayerSafe
    {
        public override string Description => "Some Heroes can get a free card by getting critical hits";

        private static Context _context;
        private static Dictionary<BoardPieceId, AbilityKey> _globalAdjustments;
        private static bool _isActivated;

        private readonly Dictionary<BoardPieceId, AbilityKey> _adjustments;

        public FreeAbilityOnCritRule(Dictionary<BoardPieceId, AbilityKey> adjustments)
        {
            _adjustments = adjustments;
        }

        public Dictionary<BoardPieceId, AbilityKey> GetConfigObject() => _adjustments;

        protected override void OnActivate(Context context)
        {
            _context = context;
            _globalAdjustments = _adjustments;
            _isActivated = true;
        }

        protected override void OnDeactivate(Context context) => _isActivated = false;

        private static void Patch(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(Ability), "GenerateAttackDamage"),
                prefix: new HarmonyMethod(
                    typeof(FreeAbilityOnCritRule),
                    nameof(Ability_GenerateAttackDamage_Prefix)));
        }

        private static void Ability_GenerateAttackDamage_Prefix(Piece source, Dice.Outcome diceResult)
        {
            if (!_isActivated)
            {
                return;
            }

            if (diceResult != Dice.Outcome.Crit)
            {
                return;
            }

            if (!_globalAdjustments.TryGetValue(source.boardPieceId, out var abilityKey))
            {
                return;
            }

            if (!source.IsPlayer())
            {
                return;
            }

            var abilityPromise = _context.AbilityFactory.LoadAbility(_globalAdjustments[source.boardPieceId]);
            abilityPromise.OnLoaded(ability =>
            {
                source.TryAddAbilityToInventory(ability, showTooltip: true, isReplenishable: false);
                HR.ScheduleBoardSync();
            });
        }
    }
}
