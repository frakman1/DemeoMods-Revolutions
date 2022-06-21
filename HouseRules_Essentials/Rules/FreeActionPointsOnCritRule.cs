namespace HouseRules.Essentials.Rules
{
    using System.Collections.Generic;
    using Boardgame;
    using Boardgame.BoardEntities;
    using Boardgame.BoardEntities.Abilities;
    using DataKeys;
    using HarmonyLib;
    using HouseRules.Types;

    public sealed class FreeActionPointsOnCritRule : Rule, IConfigWritable<List<BoardPieceId>>, IPatchable, IMultiplayerSafe
    {
        public override string Description => "Critical Hit restores action points.";

        private static List<BoardPieceId> _globalAdjustments;
        private static bool _isActivated;

        private readonly List<BoardPieceId> _adjustments;

        public FreeActionPointsOnCritRule(List<BoardPieceId> adjustments)
        {
            _adjustments = adjustments;
        }

        public List<BoardPieceId> GetConfigObject() => _adjustments;

        protected override void OnActivate(GameContext gameContext)
        {
            _globalAdjustments = _adjustments;
            _isActivated = true;
        }

        protected override void OnDeactivate(GameContext gameContext) => _isActivated = false;

        private static void Patch(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(Ability), "GenerateAttackDamage"),
                prefix: new HarmonyMethod(
                    typeof(FreeActionPointsOnCritRule),
                    nameof(Ability_GenerateAttackDamage_Prefix)));
        }

        private static bool Ability_GenerateAttackDamage_Prefix(Piece source, Dice.Outcome diceResult)
        {
            if (!_isActivated)
            {
                return true;
            }

            MelonLoader.MelonLogger.Msg("Free AP called");
            if (diceResult == Dice.Outcome.Crit)
            {
                if (source.IsPlayer() && _globalAdjustments.Contains(source.boardPieceId))
                {
                    source.effectSink.TryGetStat(Stats.Type.ActionPoints, out int currentAP);
                    if (source.boardPieceId == BoardPieceId.HeroGuardian)
                    {
                        if (currentAP < 1)
                        {
                            source.effectSink.TrySetStatBaseValue(Stats.Type.ActionPoints, currentAP + 2);
                        }
                        else
                        {
                            source.effectSink.TrySetStatBaseValue(Stats.Type.ActionPoints, currentAP + 1);
                        }

                        source.EnableEffectState(EffectStateType.Frenzy);
                        source.effectSink.SetStatusEffectDuration(EffectStateType.Frenzy, 1);
                    }
                    else if (currentAP < 1)
                    {
                        source.effectSink.TrySetStatBaseValue(Stats.Type.ActionPoints, currentAP + 1);
                    }

                    HR.ScheduleBoardSync();
                }
            }

            return true; // Allow the regular GenerateAttackDamage function to run afterwards.
        }
    }
}
