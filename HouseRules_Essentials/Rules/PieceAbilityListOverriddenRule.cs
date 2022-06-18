﻿namespace HouseRules.Essentials.Rules
{
    using System.Collections.Generic;
    using System.Linq;
    using Boardgame;
    using Data.GameData;
    using DataKeys;
    using HarmonyLib;
    using HouseRules.Types;

    public sealed class PieceAbilityListOverriddenRule : Rule,
        IConfigWritable<Dictionary<BoardPieceId, List<AbilityKey>>>, IMultiplayerSafe
    {
        public override string Description => "Piece abilities are adjusted";

        protected override SyncableTrigger ModifiedSyncables => SyncableTrigger.NewPieceModified;

        private readonly Dictionary<BoardPieceId, List<AbilityKey>> _adjustments;
        private Dictionary<BoardPieceId, List<AbilityKey>> _originals;

        /// <summary>
        /// Initializes a new instance of the <see cref="PieceAbilityListOverriddenRule"/> class.
        /// </summary>
        /// <param name="adjustments">Dict of piece name and Lists of abilityKeys
        /// Replaces original settings with new list.</param>
        public PieceAbilityListOverriddenRule(Dictionary<BoardPieceId, List<AbilityKey>> adjustments)
        {
            _adjustments = adjustments;
            _originals = new Dictionary<BoardPieceId, List<AbilityKey>>();
        }

        public Dictionary<BoardPieceId, List<AbilityKey>> GetConfigObject() => _adjustments;

        protected override void OnPreGameCreated(GameContext gameContext)
        {
            _originals = ReplaceExistingProperties(_adjustments);
        }

        protected override void OnDeactivate(GameContext gameContext)
        {
            ReplaceExistingProperties(_originals);
        }

        private static Dictionary<BoardPieceId, List<AbilityKey>> ReplaceExistingProperties(
            Dictionary<BoardPieceId, List<AbilityKey>> pieceConfigChanges)
        {
            var gameConfigPieceConfigs = Traverse.Create(typeof(GameDataAPI))
                .Field<Dictionary<GameConfigType, Dictionary<BoardPieceId, PieceConfigDTO>>>("PieceConfigDTOdict")
                .Value;
            var previousProperties = new Dictionary<BoardPieceId, List<AbilityKey>>();

            foreach (var item in pieceConfigChanges)
            {
                var pieceConfigDto = gameConfigPieceConfigs[MotherbrainGlobalVars.CurrentConfig][item.Key];
                previousProperties[item.Key] = pieceConfigDto.Abilities.ToList();
                pieceConfigDto.Abilities = item.Value.ToArray();
                gameConfigPieceConfigs[MotherbrainGlobalVars.CurrentConfig][item.Key] = pieceConfigDto;
            }

            return previousProperties;
        }
    }
}
