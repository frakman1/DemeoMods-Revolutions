namespace HouseRules.Essentials.Rulesets
{
    using HouseRules.Core.Types;
    using HouseRules.Essentials.Rules;

    internal static class QoL_OnlyRuleset
    {
        internal static Ruleset Create()
        {
            const string name = "QoL Only";
            const string displayname = "<b><color=#FF0000>Q<color=#FFFF00>o<color=#00FFFF>L <color=#FFFFFF>Only</b>";
            const string description = "Quality of life rules for playing a better base game.";
            const string longdesc = "Reviving a fallen hero removes stun and frozen effects.\nHero's abilites can't affect/hurt other heroes or pets.\nBarbarian can hook a lamp and throw it in the same turn.\n";

            var reviveRemovesEffects = new ReviveRemovesEffectsRule(true);
            var grappleUhooked = new GrappleUnhookedRule(true);
            var partyDamageRule = new PartyDamageOverriddenRule(false);

            return Ruleset.NewInstance(
                name,
                displayname,
                description,
                longdesc,
                reviveRemovesEffects,
                grappleUhooked,
                partyDamageRule);
        }
    }
}
