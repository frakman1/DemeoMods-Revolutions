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
    using UnityEngine;

    public sealed class FreeRandomBuffOnKillRule : Rule, IConfigWritable<List<BoardPieceId>>,
        IPatchable, IMultiplayerSafe, IDisableOnReconnect
    {
        public override string Description => "Players gain a random buff per kill";

        private readonly List<BoardPieceId> _adjustments;
        private static List<BoardPieceId> _globalAdjustments;
        private static List<EffectStateType> _effectStates = new List<EffectStateType> { EffectStateType.FireImmunity, EffectStateType.Antidote, EffectStateType.PlayerBerserk, EffectStateType.Courageous, EffectStateType.Deflect, EffectStateType.IceImmunity, EffectStateType.ExtraAction, EffectStateType.Fearless, EffectStateType.Recovery, EffectStateType.Heroic, EffectStateType.IceImmunity, EffectStateType.Invisibility, EffectStateType.Invulnerable1, EffectStateType.Luck, EffectStateType.MagicShield, EffectStateType.Petrified, EffectStateType.Resilience, EffectStateType.SpellPower, EffectStateType.Stealthed, EffectStateType.Wet, EffectStateType.Blinded, EffectStateType.CorruptedRage, EffectStateType.MarkOfVerga, EffectStateType.Overcharge, EffectStateType.Netted, EffectStateType.Tangled, EffectStateType.TorchPlayer, EffectStateType.Weaken1Turn };
        private static bool _isActivated;
        private static List<Piece> _playerPieces;
        private static Piece tempPiece;

        public FreeRandomBuffOnKillRule(List<BoardPieceId> adjustments)
        {
            _adjustments = adjustments;
        }

        public List<BoardPieceId> GetConfigObject() => _adjustments;

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
                    typeof(FreeRandomBuffOnKillRule),
                    nameof(MotherTracker_TrackUnitDefeated_Prefix)));

            harmony.Patch(
                original: AccessTools.Constructor(typeof(RearrangePlayerTurnOrder), new[] { typeof(TurnQueue) }),
                postfix: new HarmonyMethod(
                    typeof(FreeRandomBuffOnKillRule),
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
                else if (attackerUnit.boardPieceId == BoardPieceId.Verochka)
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

                if (!attackerUnit.IsPlayer())
                {
                    return;
                }
            }

            // Add random effect to player here
            int buff = Random.Range(1, _effectStates.Count + 1);
            {
                var effect = _effectStates[buff];
                var buffed = attackerUnit.effectSink.GetEffectStateDurationTurnsLeft(effect);
                attackerUnit.effectSink.RemoveStatusEffect(effect);
                attackerUnit.effectSink.AddStatusEffect(effect, buffed + 1);
                if (effect == EffectStateType.ExtraAction)
                {
                    attackerUnit.effectSink.TryGetStat(Stats.Type.ActionPoints, out int currentAP);
                    attackerUnit.effectSink.TrySetStatBaseValue(Stats.Type.ActionPoints, currentAP + 1);
                }

                if (effect == EffectStateType.Resilience)
                {
                    attackerUnit.effectSink.TryGetStat(Stats.Type.MagicArmor, out int currentArmor);
                    attackerUnit.effectSink.TrySetStatBaseValue(Stats.Type.MagicArmor, currentArmor + 1);
                }
            }
        }
    }
}
