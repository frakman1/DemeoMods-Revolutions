namespace DieDebate
{
    using System;
    using Boardgame;
    using Boardgame.PrefabRegistries;
    using Boardgame.SerializableEvents;
    using HarmonyLib;

    internal static class ModPatcher
    {
        private static int _rollCount;
        private static int _missCount;
        private static int _hitCount;
        private static int _critCount;

        internal static void Patch(Harmony harmony)
        {
            DieDebateMod.Logger.Msg("Patching...");
            harmony.Patch(
                original: typeof(DiceRollingController).GetMethod("OnTick"),
                postfix: new HarmonyMethod(typeof(ModPatcher), nameof(DiceRollingController_OnTick_Postfix)));

            harmony.Patch(
                original: typeof(SerializableEventQueue).GetMethod("OnDiceResolved"),
                postfix: new HarmonyMethod(typeof(ModPatcher), nameof(SerializableEventQueue_OnDiceResolved_Postfix)));
        }

        private static void DiceRollingController_OnTick_Postfix(DiceRollingController __instance)
        {
            DieDebateMod.Logger.Msg("Inside tick.");

            // var diceInstance = Traverse.Create(__instance).Field<Dice>("diceInstance").Value;
            // var diceData = Traverse.Create(__instance).Field("diceData").GetValue();
            // if (diceInstance.HasResolved)
            // {
            //     DieDebateMod.Logger.Msg("Has resolved.");
            //     var playerID = getPlayerID(diceData);
            //     var abilityEvent = getDicedAbility(diceData);
            //     var diceType = getDiceType(diceData);
            //
            //     DieDebateMod.Logger.Msg($"PLAYERID: {playerID}");
            //     DieDebateMod.Logger.Msg($"ABILITY: {abilityEvent.abilityKey}");
            //     DieDebateMod.Logger.Msg($"DICETYPE: {diceType}");
            //     DieDebateMod.Logger.Msg($"OUTCOME: {diceInstance.GetOutcome()}");
            // }
        }

        private static void SerializableEventQueue_OnDiceResolved_Postfix(Dice.Outcome result, SerializableEventUseAbility dicedAbility)
        {
            // DieDebateMod.Logger.Msg("Inside SerializableEventQueue_OnDiceResolved_Postfix.");

            // var diceInstance = Traverse.Create(__instance).Field<Dice>("diceInstance").Value;
            // var diceData = Traverse.Create(__instance).Field("diceData").GetValue();
            // if (diceInstance.HasResolved)
            // {
                // DieDebateMod.Logger.Msg("Has resolved.");

                // DieDebateMod.Logger.Msg($"PLAYERID: {playerID}");
                DieDebateMod.Logger.Msg($"ABILITY: {dicedAbility.abilityKey}");
                // DieDebateMod.Logger.Msg($"DICETYPE: {diceType}");
                DieDebateMod.Logger.Msg($"RESULT: {result}");

                _rollCount++;

                if (result == Dice.Outcome.Miss)
                {
                    _missCount++;
                }

                if (result == Dice.Outcome.Hit)
                {
                    _hitCount++;
                }

                if (result == Dice.Outcome.Crit)
                {
                    _critCount++;
                }

                DieDebateMod.Logger.Msg($"Total rolls: {_rollCount}");

                var floatRollCount = Convert.ToSingle(_rollCount);

                DieDebateMod.Logger.Msg($"(So far) Miss percentage: {_missCount / floatRollCount}");
                DieDebateMod.Logger.Msg($"(So far) Crit percentage: {_critCount / floatRollCount}");
            // }
        }

        private static int getPlayerID(object diceData)
        {
            return Traverse.Create(diceData).Field<int>("playerId").Value;
        }

        private static SerializableEventUseAbility getDicedAbility(object diceData)
        {
            return Traverse.Create(diceData).Field<SerializableEventUseAbility>("dicedAbility").Value;
        }

        private static DiceType getDiceType(object diceData)
        {
            return Traverse.Create(diceData).Field<DiceType>("diceType").Value;
        }
    }
}
