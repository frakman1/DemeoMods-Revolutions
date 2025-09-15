namespace HouseRules.Essentials.Rules
{
    using System.Collections.Generic;
    using System.Linq;
    using Boardgame;
    using Boardgame.SerializableEvents;
    using Boardgame.SerializableEvents.CustomEventHandlers;
    using HarmonyLib;
    using HouseRules.Core.Types;
    using UnityEngine;

    public sealed class MyRandomLevelSequenceOverriddenRule : Rule, IConfigWritable<List<string>>, IPatchable, IMultiplayerSafe, IDisableOnReconnect
    {
        public override string Description => "The adventure's map order is adjusted randomly";

        private static bool isFastForward;
        private static bool isSkipLevel1;
        private static List<string> _globalAdjustments;
        private static List<string> _randomMaps = new List<string>
                    { string.Empty, string.Empty, string.Empty, string.Empty, string.Empty };

        private static bool _isActivated;
        private readonly List<string> _adjustments;

        /// <summary>
        /// Initializes a new instance of the <see cref="MyRandomLevelSequenceOverriddenRule"/> class.
        /// </summary>
        /// <param name="adjustments">List of strings of LevelNames.</param>
        public MyRandomLevelSequenceOverriddenRule(List<string> adjustments)
        {
            _adjustments = adjustments;
        }

        public List<string> GetConfigObject() => _adjustments;

        protected override void OnActivate(Context context)
        {
            _globalAdjustments = _adjustments;
            _isActivated = true;
        }

        protected override void OnPreGameCreated(Context context)
        {
            ReplaceExistingProperties(_globalAdjustments, context.GameContext);
        }

        private static void Patch(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(
                    typeof(LevelSequenceConfiguration), "GetSequenceDefinition"),
                prefix: new HarmonyMethod(
                    typeof(LevelSequenceOverriddenRule),
                    nameof(LevelSequenceConfiguration_GetSequenceDefinition_Prefix)));

            harmony.Patch(
                original: AccessTools.Method(
                    typeof(PlayAgainEventHandler), "AfterResponse"),
                prefix: new HarmonyMethod(
                    typeof(LevelSequenceOverriddenRule),
                    nameof(PlayAgainEventHandler_AfterResponse_Prefix)));
        }

        /// <remarks>
        /// Sets a safe sequence definition even if the active game type does not have one that extends to the current level.
        /// </remarks>
        private static bool LevelSequenceConfiguration_GetSequenceDefinition_Prefix(
            ref SequenceDefinition __result,
            int index,
            LevelSequence.GameType gameType)
        {
            if (!_isActivated)
            {
                return true;
            }

            var gameContext = Traverse.Create(typeof(GameHub)).Field<GameContext>("gameContext").Value;
            var sequenceDefinitions =
                gameContext.levelSequenceConfiguration.sequenceDefinitions.GetSequenceFromId(gameType, out _);

            if (index >= 0 && index < sequenceDefinitions.Length)
            {
                return true;
            }

            __result = gameContext.levelLoaderAndInitializer.GetLevelSequence().CurrentLevelIsLastLevel
                ? sequenceDefinitions[sequenceDefinitions.Length - 1]
                : sequenceDefinitions[sequenceDefinitions.Length - 3];

            return false;
        }

        /// <remarks>
        /// Overrides the level sequence used for a restarted game with the fresh copy of the current overriden one.
        /// </remarks>
        private static bool PlayAgainEventHandler_AfterResponse_Prefix(
            PlayAgainEventHandler __instance,
            SerializableEventQueue eventQueue)
        {
            if (!_isActivated)
            {
                return true;
            }

            var gameContext = Traverse.Create(typeof(GameHub)).Field<GameContext>("gameContext").Value;
            var newGameType =
                Traverse.Create(__instance).Field<PostGameControllerBase>("postGameController").Value.gameType;

            var gsmLevelSequence = gameContext.levelSequenceConfiguration.GetNewLevelSequence(-1, newGameType, LevelSequence.ControlType.OneHero);
            var originalSequence = Traverse.Create(gsmLevelSequence).Field<string[]>("levels").Value;

            if (newGameType == LevelSequence.GameType.Desert)
            {
                _randomMaps[4] = "DesertBossFloor01";
            }
            else if (_randomMaps[4] == "DesertBossFloor01")
            {
                _randomMaps[4] = "DesertFloor10";
            }
            else if (newGameType == LevelSequence.GameType.Town)
            {
                _randomMaps[4] = "TownsBossFloor01";
            }
            else if (_randomMaps[4] == "TownsBossFloor01")
            {
                _randomMaps[4] = "TownsFloor02";
            }

            Traverse.Create(gsmLevelSequence).Field<string[]>("levels").Value =
                _randomMaps.Prepend(originalSequence[0]).ToArray();

            var gameState = gameContext.gameStateMachine.GetCurrentGameState();
            eventQueue.SendEventRequest(new SerializableEventStartNewGame(gsmLevelSequence, gameState));
            return false;
        }

        /// <summary>
        /// Replaces LevelSequence levels with predefined list.
        /// </summary>
        /// <returns>List of previous LevelSequence levels that are now replaced.</returns>
        private static List<string> ReplaceExistingProperties(List<string> replacements, GameContext gameContext)
        {
            var gsmLevelSequence =
                Traverse.Create(gameContext.gameStateMachine).Field<LevelSequence>("levelSequence").Value;
            var originalSequence = Traverse.Create(gsmLevelSequence).Field<string[]>("levels").Value;

            if (replacements[0].Contains("fastforward"))
            {
                isFastForward = true;
            }
            else if (replacements[0].Contains("skiplevel1"))
            {
                isSkipLevel1 = true;
            }

            int newMap1;
            int newMap2;
            int newMap3;
            int startMap = 0;
            if (isFastForward || isSkipLevel1)
            {
                startMap = 1;
            }

            newMap1 = Random.Range(startMap, replacements.Count);
            _randomMaps[0] = replacements[newMap1];
            newMap2 = newMap1;
            while (newMap2 == newMap1)
            {
                newMap2 = Random.Range(startMap, replacements.Count);
            }

            _randomMaps[2] = replacements[newMap2];
            newMap3 = newMap2;
            while (newMap3 == newMap1 || newMap3 == newMap2)
            {
                newMap3 = Random.Range(startMap, replacements.Count);
            }

            _randomMaps[4] = replacements[newMap3];

            if (gsmLevelSequence.gameType == LevelSequence.GameType.Desert)
            {
                _randomMaps[4] = "DesertBossFloor01";
            }
            else if (gsmLevelSequence.gameType == LevelSequence.GameType.Town)
            {
                _randomMaps[4] = "TownsBossFloor01";
            }

            if (isFastForward)
            {
                HouseRulesEssentialsBase.LogWarning("Fast Forward mode detected");
                if (gsmLevelSequence.gameType == LevelSequence.GameType.Forest)
                {
                    _randomMaps[0] = "CryptEntrance";
                    _randomMaps[2] = "ElvenFloor17";
                }
                else if (gsmLevelSequence.gameType == LevelSequence.GameType.ElvenQueen)
                {
                    _randomMaps[0] = "DesertEntranceFloor";
                    _randomMaps[2] = "ForestEntrance";
                }
                else if (gsmLevelSequence.gameType == LevelSequence.GameType.Town)
                {
                    _randomMaps[0] = "SewersEntranceFloor";
                    _randomMaps[2] = "DesertEntranceFloor";
                }
                else if (gsmLevelSequence.gameType == LevelSequence.GameType.RatKing)
                {
                    _randomMaps[0] = "ForestEntrance";
                    _randomMaps[2] = "CryptEntrance";
                }
                else if (gsmLevelSequence.gameType == LevelSequence.GameType.Desert)
                {
                    _randomMaps[0] = "TownsEntrance";
                    _randomMaps[2] = "CryptEntrance";
                }
            }
            else if (isSkipLevel1)
            {
                HouseRulesEssentialsBase.LogWarning("Skip Level 1 mode detected");
                if (gsmLevelSequence.gameType == LevelSequence.GameType.Forest)
                {
                    _randomMaps[0] = "CryptEntrance";
                }
                else if (gsmLevelSequence.gameType == LevelSequence.GameType.ElvenQueen)
                {
                    _randomMaps[0] = "DesertEntranceFloor";
                }
                else if (gsmLevelSequence.gameType == LevelSequence.GameType.Town)
                {
                    _randomMaps[0] = "SewersEntranceFloor";
                }
                else if (gsmLevelSequence.gameType == LevelSequence.GameType.RatKing)
                {
                    _randomMaps[0] = "ForestEntrance";
                }
                else if (gsmLevelSequence.gameType == LevelSequence.GameType.Desert)
                {
                    _randomMaps[0] = "TownsEntrance";
                }
            }

            switch (_randomMaps[2].Substring(0, 4))
            {
                case "Elve":
                    _randomMaps[1] = "ShopFloor02";
                    break;

                case "Fore":
                    _randomMaps[1] = "ForestShopFloor";
                    break;

                case "Sewe":
                    _randomMaps[1] = "SewersShopFloor";
                    break;

                case "Dese":
                    _randomMaps[1] = "DesertShopFloor";
                    break;
                case "Town":
                    _randomMaps[1] = "TownsShopFloor";
                    break;
            }

            switch (_randomMaps[4].Substring(0, 4))
            {
                case "Elve":
                    _randomMaps[3] = "ShopFloor02";
                    break;

                case "Fore":
                    _randomMaps[3] = "ForestShopFloor";
                    break;

                case "Sewe":
                    _randomMaps[3] = "SewersShopFloor";
                    break;

                case "Dese":
                    _randomMaps[3] = "DesertShopFloor";
                    break;
                case "Town":
                    _randomMaps[3] = "TownsShopFloor";
                    break;
            }

            HouseRulesEssentialsBase.LogWarning("Randomly generated level sequence loaded");
            HouseRulesEssentialsBase.LogWarning($"Map1: {_randomMaps[0]} Shop1: {_randomMaps[1]} Map2: {_randomMaps[2]} Shop2: {_randomMaps[3]} Map3: {_randomMaps[4]}");
            Traverse.Create(gsmLevelSequence).Field<string[]>("levels").Value =
                _randomMaps.Prepend(originalSequence[0]).ToArray();
            return originalSequence.ToList();
        }
    }
}
