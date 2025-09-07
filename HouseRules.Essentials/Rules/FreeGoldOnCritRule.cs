namespace HouseRules.Essentials.Rules
{
    using System.Collections.Generic;
    using Boardgame.BoardEntities;
    using Boardgame.BoardEntities.Abilities;
    using DataKeys;
    using HarmonyLib;
    using HouseRules.Core;
    using HouseRules.Core.Types;

    public sealed class FreeGoldOnCritRule : Rule, IConfigWritable<Dictionary<BoardPieceId, int>>, IPatchable, IMultiplayerSafe
    {
        public override string Description => "Some Heroes gain gold by getting critical hits";

        private static Dictionary<BoardPieceId, int> _globalAdjustments;
        private static bool _isActivated;

        private readonly Dictionary<BoardPieceId, int> _adjustments;

        public FreeGoldOnCritRule(Dictionary<BoardPieceId, int> adjustments)
        {
            _adjustments = adjustments;
        }

        public Dictionary<BoardPieceId, int> GetConfigObject() => _adjustments;

        protected override void OnActivate(Context context)
        {
            _globalAdjustments = _adjustments;
            _isActivated = true;
        }

        protected override void OnDeactivate(Context context) => _isActivated = false;

        private static void Patch(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(Ability), "GenerateAttackDamage"),
                postfix: new HarmonyMethod(
                    typeof(FreeGoldOnCritRule),
                    nameof(Ability_GenerateAttackDamage_Postfix)));
        }

        private static void Ability_GenerateAttackDamage_Postfix(Piece source, Dice.Outcome diceResult)
        {
            if (!_isActivated)
            {
                return;
            }

            if (!source.IsPlayer())
            {
                return;
            }

            if (diceResult != Dice.Outcome.Crit)
            {
                return;
            }

            if (!_globalAdjustments.ContainsKey(source.boardPieceId))
            {
                return;
            }

            if (!HR.SelectedRuleset.Name.Contains("Revolutions"))
            {
                if (source.GetStatMax(Stats.Type.CritChance) > 1)
                {
                    source.inventory.AddGold(10);
                }
                else if (source.GetStatMax(Stats.Type.CritChance) > 0)
                {
                    source.inventory.AddGold(_globalAdjustments[source.boardPieceId]);
                }
            }
        }
    }
}
