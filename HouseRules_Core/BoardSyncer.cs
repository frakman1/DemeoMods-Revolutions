﻿namespace HouseRules
{
    using System;
    using Boardgame;
    using Boardgame.BoardEntities;
    using Boardgame.SerializableEvents;
    using DataKeys;
    using HarmonyLib;

    [Flags]
    public enum SyncableTrigger
    {
        /// <summary>Nothing that needs special syncing is modified.</summary>
        None = 0,

        /// <summary>New pieces are changed or have their attributes modified.</summary>
        NewPieceModified = 1,

        /// <summary>Status effect immunities are modified.</summary>
        StatusEffectImmunityModified = 2,

        /// <summary>Status effect data (e.g., values, duration, etc.) are modified.</summary>
        StatusEffectDataModified = 4,
    }

    internal static class BoardSyncer
    {
        private static GameContext _gameContext;
        private static bool _isSyncScheduled;
        private static bool _isMove;
        private static bool _isGrab;
        private static bool _isStateChange;

        /// <summary>
        /// Schedules a sync to be triggered at the next available opportunity.
        /// </summary>
        internal static void ScheduleSync()
        {
            if (!HR.IsRulesetActive)
            {
                throw new InvalidOperationException("Can not schedule sync without an active ruleset.");
            }

            _isSyncScheduled = true;
        }

        internal static void Patch(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(GameStartup), "InitializeGame"),
                postfix: new HarmonyMethod(typeof(BoardSyncer), nameof(GameStartup_InitializeGame_Postfix)));

            harmony.Patch(
                original: AccessTools.Method(typeof(SerializableEventQueue), "SendResponseEvent"),
                postfix: new HarmonyMethod(
                    typeof(BoardSyncer),
                    nameof(SerializableEventQueue_SendResponseEvent_Postfix)));

            harmony.Patch(
                original: AccessTools.Method(typeof(Piece), "IsImmuneToStatusEffect"),
                postfix: new HarmonyMethod(
                    typeof(BoardSyncer),
                    nameof(Piece_IsImmuneToStatusEffect_Postfix)));

            harmony.Patch(
                original: AccessTools.Method(typeof(EffectSink), "AddStatusEffect"),
                postfix: new HarmonyMethod(
                    typeof(BoardSyncer),
                    nameof(EffectSink_AddStatusEffect_Postfix)));
        }

        private static void GameStartup_InitializeGame_Postfix(GameStartup __instance)
        {
            var gameContext = Traverse.Create(__instance).Field<GameContext>("gameContext").Value;
            _gameContext = gameContext;
        }

        private static void SerializableEventQueue_SendResponseEvent_Postfix(SerializableEvent serializableEvent)
        {
            if (!HR.IsRulesetActive)
            {
                return;
            }

            var isNewPieceCheckRequired = (HR.SelectedRuleset.ModifiedSyncables & SyncableTrigger.NewPieceModified) > 0;
            if (!_isSyncScheduled && isNewPieceCheckRequired && CanRepresentNewSpawn(serializableEvent))
            {
                _isSyncScheduled = true;
            }

            if (_isSyncScheduled && IsSyncOpportunity(serializableEvent))
            {
                SyncBoard();
            }
        }

        private static void Piece_IsImmuneToStatusEffect_Postfix()
        {
            var isEffectImmunityCheckRequired = (HR.SelectedRuleset.ModifiedSyncables & SyncableTrigger.StatusEffectImmunityModified) > 0;
            if (isEffectImmunityCheckRequired)
            {
                _isStateChange = true;
                _isSyncScheduled = true;
            }
        }

        private static void EffectSink_AddStatusEffect_Postfix()
        {
            var isEffectDataCheckRequired = (HR.SelectedRuleset.ModifiedSyncables & SyncableTrigger.StatusEffectDataModified) > 0;
            if (isEffectDataCheckRequired)
            {
                _isStateChange = true;
                _isSyncScheduled = true;
            }
        }

        private static bool CanRepresentNewSpawn(SerializableEvent serializableEvent)
        {
            string whatUp = serializableEvent.ToString();
            switch (serializableEvent.type)
            {
                case SerializableEvent.Type.UpdateGameHub:
                    // CoreMod.Logger.Msg($"<<>> {whatUp}");
                    _gameContext.serializableEventQueue.SendResponseEvent(new SerializableEventUpdateFog());
                    return true;
                case SerializableEvent.Type.Move:
                case SerializableEvent.Type.Interact:
                    if (_gameContext.pieceAndTurnController.IsPlayersTurn())
                    {
                        // CoreMod.Logger.Msg($"<<>> {whatUp}");
                        _isMove = true;
                        return true;
                    }

                    // CoreMod.Logger.Msg($"---- {whatUp}");
                    return false;
                case SerializableEvent.Type.SpawnPiece:
                case SerializableEvent.Type.SetBoardPieceID:
                case SerializableEvent.Type.SlimeFusion:
                    // CoreMod.Logger.Msg($"<<>> {whatUp}");
                    return true;
                case SerializableEvent.Type.OnAbilityUsed:
                    return CanRepresentNewSpawn((SerializableEventOnAbilityUsed)serializableEvent);
                case SerializableEvent.Type.PieceDied:
                    return CanRepresentNewSpawn((SerializableEventPieceDied)serializableEvent);
                default:
                    // CoreMod.Logger.Msg($"---- {whatUp}");
                    return false;
            }
        }

        private static bool CanRepresentNewSpawn(SerializableEventOnAbilityUsed onAbilityUsedEvent)
        {
            var abilityKey = Traverse.Create(onAbilityUsedEvent).Field<AbilityKey>("abilityKey").Value;
            switch (abilityKey)
            {
                case AbilityKey.Grab:
                    _isGrab = true;
                    return true;
                case AbilityKey.DetectEnemies:
                    _isStateChange = true;
                    return true;
                case AbilityKey.RevealPath:
                case AbilityKey.Leap:
                case AbilityKey.LeapHeavy:
                    _isMove = true;
                    return true;
                case AbilityKey.BeastWhisperer:
                case AbilityKey.HurricaneAnthem:
                case AbilityKey.Lure:
                case AbilityKey.BoobyTrap:
                case AbilityKey.RepeatingBallista:
                case AbilityKey.TheBehemoth:
                case AbilityKey.HealingWard:
                case AbilityKey.RaiseRoots:
                case AbilityKey.CallCompanion:
                case AbilityKey.DigRatsNest:
                case AbilityKey.Barricade:
                case AbilityKey.MagicBarrier:
                    return true;
            }

            var abilityName = abilityKey.ToString();
            var isSpawnAbility = abilityName.Contains("Spawn");
            var isLampAbility = abilityName.Contains("Lamp");
            var isSummonAbility = abilityName.Contains("Summon");

            return isSpawnAbility || isLampAbility || isSummonAbility;
        }

        private static bool CanRepresentNewSpawn(SerializableEventPieceDied pieceDiedEvent)
        {
            foreach (var pieceId in pieceDiedEvent.deadPieces)
            {
                if (!_gameContext.pieceAndTurnController.TryGetPiece(pieceId, out var piece))
                {
                    continue;
                }

                if (piece.boardPieceId == BoardPieceId.SpiderEgg || piece.boardPieceId == BoardPieceId.ScorpionSandPile)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsSyncOpportunity(SerializableEvent serializableEvent)
        {
            if (_isGrab)
            {
                if (serializableEvent.type != SerializableEvent.Type.OnMoved)
                {
                    return false;
                }
                else
                {
                    // CoreMod.Logger.Msg("FOG: Grab (OnMoved)");
                    _isGrab = false;
                    _isSyncScheduled = false;
                    _gameContext.serializableEventQueue.SendResponseEvent(new SerializableEventUpdateFog());
                    return false;
                }
            }
            else if (_isMove)
            {
                if (serializableEvent.type != SerializableEvent.Type.EndAction)
                {
                    return false;
                }
                else
                {
                    // CoreMod.Logger.Msg("FOG: EndAction (Move)");
                    _isMove = false;
                    _isSyncScheduled = false;
                    _gameContext.serializableEventQueue.SendResponseEvent(new SerializableEventUpdateFog());
                    return false;
                }
            }
            else if (_isStateChange)
            {
                if (serializableEvent.type != SerializableEvent.Type.EndAction)
                {
                    return false;
                }
                else
                {
                    // CoreMod.Logger.Msg("SYNC: EndAction (StateChange)");
                    _gameContext.serializableEventQueue.SendResponseEvent(new SerializableEventUpdateFog());
                    return true;
                }
            }
            else if (_gameContext.pieceAndTurnController.GetCurrentIndexFromTurnQueue() >= 0 && !_gameContext.pieceAndTurnController.IsPlayersTurn())
            {
                if (serializableEvent.type == SerializableEvent.Type.EndTurn)
                {
                    // CoreMod.Logger.Msg("SYNC: Enemies EndTurn");
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                // CoreMod.Logger.Msg($">>> Normal Sync <<<");
                return true;
            }
        }

        private static void SyncBoard()
        {
            // CoreMod.Logger.Msg("<<< !!RECOVERY!! >>>");
            _isStateChange = false;
            _isSyncScheduled = false;
            _gameContext.serializableEventQueue.SendResponseEvent(SerializableEvent.CreateRecovery());
        }
    }
}
