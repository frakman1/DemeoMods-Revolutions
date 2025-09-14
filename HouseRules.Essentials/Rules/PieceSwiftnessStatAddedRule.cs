namespace HouseRules.Essentials.Rules
{
    using System.Collections.Generic;
    using Boardgame.BoardEntities;
    using DataKeys;
    using HarmonyLib;
    using HouseRules.Core;
    using HouseRules.Core.Types;

    public sealed class PieceSwiftnessStatAddedRule : Rule, IConfigWritable<Dictionary<BoardPieceId, int>>, IPatchable, IMultiplayerSafe
    {
        public override string Description => "Some Heroes start with a swiftness bonus";

        protected override SyncableTrigger ModifiedSyncables => SyncableTrigger.NewPieceModified;

        private static Dictionary<BoardPieceId, int> _globalAdjustments;
        private static bool _isActivated;

        public PieceSwiftnessStatAddedRule(Dictionary<BoardPieceId, int> adjustments)
        {
            _adjustments = adjustments;
        }

        private readonly Dictionary<BoardPieceId, int> _adjustments;

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
                original: AccessTools.Method(typeof(Piece), "CreatePiece"),
                postfix: new HarmonyMethod(
                    typeof(PieceSwiftnessStatAddedRule),
                    nameof(CreatePiece_Effects_Postfix)));
        }

        private static void CreatePiece_Effects_Postfix(ref Piece __result)
        {
            if (!_isActivated)
            {
                return;
            }

            if (!_globalAdjustments.ContainsKey(__result.boardPieceId))
            {
                return;
            }

            foreach (var replacement in _globalAdjustments)
            {
                if (replacement.Key == __result.boardPieceId)
                {
                    __result.effectSink.TryGetStatMax(Stats.Type.Speed, out int myMaxSwiftness);
                    __result.effectSink.TrySetStatMaxValue(Stats.Type.Speed, myMaxSwiftness + replacement.Value);
                    __result.effectSink.TrySetStatBaseValue(Stats.Type.Speed, replacement.Value);
                    return;
                }
            }
        }
    }
}
