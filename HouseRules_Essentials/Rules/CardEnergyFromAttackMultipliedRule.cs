﻿namespace HouseRules.Essentials.Rules
{
    using Boardgame;
    using Data.GameData;
    using HarmonyLib;
    using HouseRules.Types;

    public sealed class CardEnergyFromAttackMultipliedRule : Rule, IConfigWritable<float>, IMultiplayerSafe
    {
        public override string Description => "CardConfig energy from attack is multiplied";

        private readonly float _multiplier;
        private readonly float _originalValue;

        public CardEnergyFromAttackMultipliedRule(float multiplier)
        {
            _multiplier = multiplier;
            _originalValue = AIDirectorConfig.CardEnergy_EnergyToGetFromDealingDamage;
        }

        public float GetConfigObject() => _multiplier;

        protected override void OnPostGameCreated(GameContext gameContext)
        {
            Traverse.Create(typeof(AIDirectorConfig))
                .Field<float>("CardEnergy_EnergyToGetFromDealingDamage").Value = _originalValue * _multiplier;
        }

        protected override void OnDeactivate(GameContext gameContext)
        {
            Traverse.Create(typeof(AIDirectorConfig))
                .Field<float>("CardEnergy_EnergyToGetFromDealingDamage").Value = _originalValue;
        }
    }
}
