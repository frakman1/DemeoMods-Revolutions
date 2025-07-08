namespace HouseRules.Essentials.Rulesets
{
    using System.Collections.Generic;
    using DataKeys;
    using HouseRules.Core.Types;
    using HouseRules.Essentials.Rules;

    internal static class BetterSorcererRuleset
    {
        internal static Ruleset Create()
        {
            const string name = "Better Sorcerer";
            const string displayname = name;
            const string description = "No Cost for Zap - No electrical damage/effects to teammates.";
            const string longdesc = "";

            var abilityActionCostRule = new AbilityActionCostAdjustedRule(new Dictionary<AbilityKey, bool>
            {
                { AbilityKey.Zap, false },
            });

            var levelPropertiesRule = new LevelPropertiesModifiedRule(new Dictionary<string, int>
            {
                { "FloorOneElvenSummoners", 0 },
                { "FloorTwoElvenSummoners", 0 },
                { "FloorThreeElvenSummoners", 0 },
            });

            var zapRule = new PartyDamageOverriddenRule(true);

            return Ruleset.NewInstance(
                name,
                displayname,
                description,
                longdesc,
                zapRule,
                levelPropertiesRule,
                abilityActionCostRule);
        }
    }
}
