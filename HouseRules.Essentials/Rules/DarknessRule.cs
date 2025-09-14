namespace HouseRules.Essentials.Rules
{
    using System.Collections.Generic;
    using Boardgame;
    using Boardgame.Board;
    using Boardgame.BoardEntities;
    using Boardgame.BoardEntities.AI;
    using Boardgame.LevelLoading;
    using Boardgame.TurnOrder;
    using DataKeys;
    using HarmonyLib;
    using HouseRules.Core.Types;

    public sealed class DarknessRule : Rule, IConfigWritable<Dictionary<BoardPieceId, EffectStateType>>,
        IPatchable, IMultiplayerSafe, IDisableOnReconnect
    {
        public override string Description => "Players gain vision range via torch per kill";

        private readonly Dictionary<BoardPieceId, EffectStateType> _adjustments;
        private static Dictionary<BoardPieceId, EffectStateType> _globalAdjustments;
        private static bool _isActivated;
        private static List<Piece> _playerPieces;
        private static Piece? tempPiece;

        public DarknessRule(Dictionary<BoardPieceId, EffectStateType> adjustments)
        {
            _adjustments = adjustments;
        }

        public Dictionary<BoardPieceId, EffectStateType> GetConfigObject() => _adjustments;

        protected override void OnActivate(Context context)
        {
            _globalAdjustments = _adjustments;
            _isActivated = true;
        }

        protected override void OnDeactivate(Context context)
        {
            _isActivated = false;
        }

        private static void Patch(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(LevelLoaderAndInitializer), "GetFloorTileEffects"),
                postfix: new HarmonyMethod(
                    typeof(DarknessRule),
                    nameof(LevelLoaderAndInitializer_GetFloorTileEffects_Postfix)));

            harmony.Patch(
                original: AccessTools.Method(typeof(MotherTracker), "TrackUnitDefeated"),
                prefix: new HarmonyMethod(
                    typeof(DarknessRule),
                    nameof(MotherTracker_TrackUnitDefeated_Prefix)));

            harmony.Patch(
                original: AccessTools.Constructor(typeof(RearrangePlayerTurnOrder), new[] { typeof(TurnQueue) }),
                postfix: new HarmonyMethod(
                    typeof(DarknessRule),
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

        private static void LevelLoaderAndInitializer_GetFloorTileEffects_Postfix(out float prob, out List<TileEffect> list)
        {
            if (!_isActivated)
            {
                if (MotherbrainGlobalVars.CurrentConfig == GameConfigType.Forest)
                {
                    prob = 0.1f;
                    list = new List<TileEffect> { TileEffect.Water };
                }
                else
                {
                    prob = 0f;
                    list = new List<TileEffect> { };
                }
            }
            else
            {
                prob = 0.2f;
                list = new List<TileEffect> { TileEffect.Acid };
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
                if (attackerUnit.HasPieceType(PieceType.Prop))
                {
                    attackerUnit = tempPiece;
                }
                else
                {
                    tempPiece = null;
                }
            }

            if (defeatedUnit.HasPieceType(PieceType.Prop))
            {
                tempPiece = attackerUnit;
                return;
            }

            if (!defeatedUnit.IsCreature())
            {
                return;
            }

            if (!attackerUnit.IsPlayer())
            {
                Piece piece2;
                PieceAI pieceAI = attackerUnit.pieceAI;
                var gameContext = Traverse.Create(typeof(GameHub)).Field<GameContext>("gameContext").Value;
                if (attackerUnit.boardPieceId == BoardPieceId.WarlockMinion && attackerUnit.GetHealth() > 0)
                {
                    if (pieceAI == null)
                    {
                        return;
                    }
                    else if (pieceAI.memory.TryGetAssociatedPiece(gameContext.pieceAndTurnController, out piece2))
                    {
                        attackerUnit = piece2;
                    }
                    else
                    {
                        return;
                    }
                }
                else if (attackerUnit.boardPieceId == BoardPieceId.SellswordArbalestierActive)
                {
                    if (pieceAI == null)
                    {
                        return;
                    }
                    else if (pieceAI.memory.TryGetAssociatedPiece(gameContext.pieceAndTurnController, out piece2))
                    {
                        attackerUnit = piece2;
                    }
                    else
                    {
                        return;
                    }
                }
                else if (attackerUnit.boardPieceId == BoardPieceId.Verochka && !attackerUnit.HasEffectState(EffectStateType.ConfusedPermanentVisualOnly))
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
                else
                {
                    return;
                }

                if (!attackerUnit.IsPlayer())
                {
                    return;
                }
            }

            foreach (var replacement in _globalAdjustments)
            {
                if (attackerUnit.boardPieceId == replacement.Key)
                {
                    var buffed = attackerUnit.effectSink.GetEffectStateDurationTurnsLeft(replacement.Value);
                    attackerUnit.effectSink.RemoveStatusEffect(replacement.Value);
                    attackerUnit.effectSink.AddStatusEffect(replacement.Value, buffed + 2);
                }
            }
        }
    }
}
