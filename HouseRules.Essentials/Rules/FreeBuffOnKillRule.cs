namespace HouseRules.Essentials.Rules
{
    using System.Collections.Generic;
    using Boardgame;
    using Boardgame.BoardEntities;
    using Boardgame.BoardEntities.AI;
    using Boardgame.TurnOrder;
    using DataKeys;
    using HarmonyLib;
    using HouseRules.Core.Types;

    public sealed class FreeBuffOnKillRule : Rule, IConfigWritable<Dictionary<BoardPieceId, EffectStateType>>,
        IPatchable, IMultiplayerSafe, IDisableOnReconnect
    {
        public override string Description => "Heroes each gain or increase a specific buff per kill";

        private readonly Dictionary<BoardPieceId, EffectStateType> _adjustments;
        private static Dictionary<BoardPieceId, EffectStateType> _globalAdjustments;
        private static bool _isActivated;
        private static List<Piece> _playerPieces;
        private static Piece? tempPiece;

        public FreeBuffOnKillRule(Dictionary<BoardPieceId, EffectStateType> adjustments)
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
                original: AccessTools.Method(typeof(MotherTracker), "TrackUnitDefeated"),
                prefix: new HarmonyMethod(
                    typeof(FreeBuffOnKillRule),
                    nameof(MotherTracker_TrackUnitDefeated_Prefix)));

            harmony.Patch(
                original: AccessTools.Constructor(typeof(RearrangePlayerTurnOrder), new[] { typeof(TurnQueue) }),
                postfix: new HarmonyMethod(
                    typeof(FreeBuffOnKillRule),
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
                bool isCana = false;
                Piece piece2;
                PieceAI pieceAI = attackerUnit.pieceAI;
                var gameContext = Traverse.Create(typeof(GameHub)).Field<GameContext>("gameContext").Value;
                if (attackerUnit.boardPieceId == BoardPieceId.WarlockMinion && attackerUnit.GetHealth() > 0)
                {
                    foreach (var replacement in _globalAdjustments)
                    {
                        if (replacement.Key == BoardPieceId.WarlockMinion)
                        {
                            isCana = true;
                            break;
                        }
                    }

                    if (!isCana)
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
                else if (attackerUnit.boardPieceId == BoardPieceId.Verochka && attackerUnit.HasEffectState(EffectStateType.ConfusedPermanentVisualOnly))
                {
                    foreach (var piece in _playerPieces)
                    {
                        if (piece.boardPieceId == BoardPieceId.HeroHunter)
                        {
                            attackerUnit = piece;
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
                        }
                    }
                }
                else
                {
                    return;
                }

                if (!attackerUnit.IsPlayer() && !isCana)
                {
                    return;
                }
            }

            foreach (var replacement in _globalAdjustments)
            {
                if (attackerUnit.boardPieceId == replacement.Key)
                {
                    var effect = replacement.Value;
                    var buffed = attackerUnit.effectSink.GetEffectStateDurationTurnsLeft(effect);
                    if (effect == EffectStateType.Luck || effect == EffectStateType.SpellPower)
                    {
                        if (buffed > 0)
                        {
                            return;
                        }

                        attackerUnit.EnableEffectState(effect);
                    }
                    else if (effect == EffectStateType.Petrified || effect == EffectStateType.Tangled || effect == EffectStateType.Weaken1Turn || effect == EffectStateType.PlayerBerserk)
                    {
                        if (buffed > 0)
                        {
                            return;
                        }

                        attackerUnit.EnableEffectState(effect, 1);
                    }
                    else if (effect == EffectStateType.Invulnerable3)
                    {
                        if (buffed > 0)
                        {
                            attackerUnit.effectSink.SetStatusEffectDuration(effect, buffed + 1);
                            return;
                        }

                        attackerUnit.EnableEffectState(effect, 1);
                    }
                    else if (effect == EffectStateType.Resilience)
                    {
                        attackerUnit.effectSink.TryGetStat(Stats.Type.MagicArmor, out int currentArmor);
                        if (currentArmor < 10)
                        {
                            attackerUnit.effectSink.TrySetStatBaseValue(Stats.Type.MagicArmor, currentArmor + 1);
                        }
                    }
                    else if (effect == EffectStateType.ExtraAction)
                    {
                        if (buffed > 0)
                        {
                            attackerUnit.effectSink.SetStatusEffectDuration(effect, buffed + 1);
                        }
                        else
                        {
                            attackerUnit.EnableEffectState(effect, 1);
                        }

                        attackerUnit.effectSink.TryGetStat(Stats.Type.ActionPoints, out int currentAP);
                        attackerUnit.effectSink.TrySetStatBaseValue(Stats.Type.ActionPoints, currentAP + 1);
                    }
                    else if (effect == EffectStateType.Fearless)
                    {
                        if (attackerUnit.HasEffectState(EffectStateType.Heroic))
                        {
                            buffed = attackerUnit.effectSink.GetEffectStateDurationTurnsLeft(EffectStateType.Heroic);
                            attackerUnit.DisableEffectState(EffectStateType.Heroic);
                        }
                        else if (attackerUnit.HasEffectState(EffectStateType.Courageous))
                        {
                            buffed = attackerUnit.effectSink.GetEffectStateDurationTurnsLeft(EffectStateType.Courageous);
                            attackerUnit.DisableEffectState(EffectStateType.Courageous);
                        }

                        if (buffed > 0)
                        {
                            attackerUnit.effectSink.SetStatusEffectDuration(effect, buffed + 1);
                            return;
                        }

                        attackerUnit.EnableEffectState(effect, 1);
                    }
                    else if (effect == EffectStateType.Heroic)
                    {
                        if (attackerUnit.HasEffectState(EffectStateType.Fearless))
                        {
                            return;
                        }

                        if (attackerUnit.HasEffectState(EffectStateType.Courageous))
                        {
                            buffed = attackerUnit.effectSink.GetEffectStateDurationTurnsLeft(EffectStateType.Courageous);
                            attackerUnit.DisableEffectState(EffectStateType.Courageous);
                        }

                        if (buffed > 0)
                        {
                            attackerUnit.effectSink.SetStatusEffectDuration(effect, buffed + 1);
                            return;
                        }

                        attackerUnit.EnableEffectState(effect, 1);
                    }
                    else if (effect == EffectStateType.Courageous)
                    {
                        if (attackerUnit.HasEffectState(EffectStateType.Fearless) || attackerUnit.HasEffectState(EffectStateType.Heroic))
                        {
                            return;
                        }
                        else if (buffed > 0)
                        {
                            attackerUnit.effectSink.SetStatusEffectDuration(effect, buffed + 1);
                            return;
                        }

                        attackerUnit.EnableEffectState(effect, 1);
                    }
                    else if (buffed > 0)
                    {
                        attackerUnit.effectSink.SetStatusEffectDuration(effect, buffed + 1);
                    }
                    else if (replacement.Key == BoardPieceId.WarlockMinion)
                    {
                        attackerUnit.EnableEffectState(effect, 2);
                    }
                    else
                    {
                        attackerUnit.EnableEffectState(effect, 1);
                    }
                }
            }
        }
    }
}
