namespace HouseRules.Essentials.Rules
{
    using System.Collections.Generic;
    using Boardgame;
    using Boardgame.BoardEntities;
    using DataKeys;
    using HarmonyLib;
    using HouseRules.Core;
    using HouseRules.Core.Types;

    public sealed class EnergyPotionRule : Rule, IConfigWritable<Dictionary<BoardPieceId, AbilityKey>>, IPatchable, IMultiplayerSafe
    {
        public override string Description => "Using the Energy Potion can give players a new ability with limited uses";

        private static bool _isActivated;
        private static Dictionary<BoardPieceId, AbilityKey> _globalAdjustments;
        private readonly Dictionary<BoardPieceId, AbilityKey> _adjustments;

        public EnergyPotionRule(Dictionary<BoardPieceId, AbilityKey> adjustments)
        {
            _adjustments = adjustments;
        }

        public Dictionary<BoardPieceId, AbilityKey> GetConfigObject() => _adjustments;

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
                original: AccessTools.Method(typeof(StatusEffect), "Tick"),
                prefix: new HarmonyMethod(
                    typeof(EnergyPotionRule),
                    nameof(StatsusEffect_Tick_Prefix)));

            harmony.Patch(
                original: AccessTools.Method(typeof(Inventory), "RestoreReplenishables"),
                prefix: new HarmonyMethod(
                    typeof(EnergyPotionRule),
                    nameof(Inventory_RestoreReplenishables_Prefix)));
        }

        private static void StatsusEffect_Tick_Prefix(ref StatusEffect __instance)
        {
            if (!_isActivated)
            {
                return;
            }

            var pieceAndTurnController = Traverse.Create(__instance).Field<PieceAndTurnController>("pieceAndTurnController").Value;
            var playerId = pieceAndTurnController.GetCurrentPlayer();
            Piece piece = pieceAndTurnController.GetActivePieceForPlayer(playerId);
            if (piece == null)
            {
                return;
            }

            if (!piece.IsPlayer())
            {
                return;
            }

            if (!_globalAdjustments.TryGetValue(piece.boardPieceId, out var abilityKey))
            {
                return;
            }

            if (__instance.effectStateType == EffectStateType.ExtraEnergy)
            {
                Inventory.Item value;
                int howMany = piece.effectSink.GetEffectStateDurationTurnsLeft(EffectStateType.ExtraEnergy);
                bool hasChanged = false;

                // Energy Potion tick/prevention and card removal per class
                if (piece.HasEffectState(EffectStateType.ExtraEnergy))
                {
                    for (int i = 0; i < piece.inventory.Items.Count; i++)
                    {
                        value = piece.inventory.Items[i];
                        if (value.AbilityKey == abilityKey)
                        {
                            if (value.IsReplenishing)
                            {
                                hasChanged = true;
                                howMany -= 1;
                                if (howMany < 1)
                                {
                                    Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value -= 1;
                                    piece.DisableEffectState(EffectStateType.ExtraEnergy);
                                    piece.inventory.Items.Remove(value);
                                }
                            }

                            break;
                        }
                    }

                    if (howMany > 0 && !hasChanged)
                    {
                        Traverse.Create(__instance).Field<int>("durationTurnsLeft").Value = howMany + 1;
                        piece.effectSink.SetStatusEffectDuration(EffectStateType.ExtraEnergy, howMany);
                    }
                }
            }
        }

        private static bool Inventory_RestoreReplenishables_Prefix(Piece piece)
        {
            if (!_isActivated)
            {
                return true;
            }

            if (!piece.IsPlayer())
            {
                return true;
            }

            Inventory.Item value;

            // Energy Potion cards added per class
            if (!piece.HasEffectState(EffectStateType.ExtraEnergy))
            {
                return true;
            }
            else if (_globalAdjustments.TryGetValue(piece.boardPieceId, out var abilityKey))
            {
                bool hasPower = false;
                for (var i = 0; i < piece.inventory.Items.Count; i++)
                {
                    value = piece.inventory.Items[i];
                    if (value.AbilityKey == abilityKey)
                    {
                        hasPower = true;
                        break;
                    }
                }

                if (!hasPower)
                {
                    Traverse.Create(piece.inventory).Field<int>("numberOfReplenishableCards").Value += 1;
                    piece.inventory.Items.Add(new Inventory.Item(
                        abilityKey,
                        flags: (Inventory.ItemFlag)1,
                        originalOwner: -1,
                        replenishCooldown: 1));

                    HR.ScheduleBoardSync();
                    piece.AddGold(0);
                }
            }

            return true;
        }
    }
}
