namespace HouseRules.Essentials.Rules
{
    using System.Collections.Generic;
    using DataKeys;
    using HouseRules.Core.Types;

    public sealed class AbilityBreaksStealthAdjustedRule : Rule, IConfigWritable<Dictionary<AbilityKey, bool>>, IMultiplayerSafe
    {
        public override string Description => "Some additional Hero abilities won't break stealth";

        private readonly Dictionary<AbilityKey, bool> _adjustments;
        private Dictionary<AbilityKey, bool> _originals;

        /// <summary>
        /// Initializes a new instance of the <see cref="AbilityBreaksStealthAdjustedRule"/> class.
        /// </summary>
        /// <param name="adjustments">Key-value pairs of abilityKey and whether the ability breaks stealth.</param>
        public AbilityBreaksStealthAdjustedRule(Dictionary<AbilityKey, bool> adjustments)
        {
            _adjustments = adjustments;
            _originals = new Dictionary<AbilityKey, bool>();
        }

        public Dictionary<AbilityKey, bool> GetConfigObject() => _adjustments;

        protected override void OnPreGameCreated(Context context)
        {
            _originals = ReplaceAbilities(context, _adjustments);
        }

        protected override void OnDeactivate(Context context)
        {
            ReplaceAbilities(context, _originals);
        }

        private static Dictionary<AbilityKey, bool> ReplaceAbilities(Context context, Dictionary<AbilityKey, bool> replacements)
        {
            var originals = new Dictionary<AbilityKey, bool>();

            foreach (var replacement in replacements)
            {
                var abilityPromise = context.AbilityFactory.LoadAbility(replacement.Key);
                abilityPromise.OnLoaded(ability =>
                {
                    originals[replacement.Key] = ability.breaksStealth;
                    ability.breaksStealth = replacement.Value;
                });
        }

            return originals;
        }
    }
}
