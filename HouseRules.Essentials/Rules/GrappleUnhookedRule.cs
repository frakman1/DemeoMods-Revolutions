namespace HouseRules.Essentials.Rules
{
    using Boardgame.BoardEntities.Abilities;
    using DataKeys;
    using HouseRules.Core.Types;

    public sealed class GrappleUnhookedRule : Rule, IConfigWritable<bool>, IMultiplayerSafe
    {
        public override string Description => "Grapple & throwing lamps can be used in the same turn";

        private static Context _context;
        private static bool _isActivated;

        public GrappleUnhookedRule(bool value)
        {
        }

        public bool GetConfigObject() => true;

        protected override void OnActivate(Context context)
        {
            _context = context;
            _isActivated = true;
            GrappleUnhooked();
        }

        protected override void OnDeactivate(Context context)
        {
            _isActivated = false;
            GrappleRehooked();
        }

        private static void GrappleUnhooked()
        {
            if (!_isActivated)
            {
                return;
            }

            _context.AbilityFactory.TryGetAbility(AbilityKey.Grapple, out var grapple);
            grapple.effectAppliedToSelf = EffectStateType.It;
            _context.AbilityFactory.TryGetAbility(AbilityKey.ExplodingIceLamp, out var launchIce);
            launchIce.effectAppliedToSelf = EffectStateType.It;
            _context.AbilityFactory.TryGetAbility(AbilityKey.ExplodingOilLamp, out var launchFire);
            launchFire.effectAppliedToSelf = EffectStateType.It;
            _context.AbilityFactory.TryGetAbility(AbilityKey.ExplodingVortexLamp, out var launchImplosion);
            launchImplosion.effectAppliedToSelf = EffectStateType.It;
            _context.AbilityFactory.TryGetAbility(AbilityKey.ExplodingGasLamp, out var launchGas);
            launchGas.effectAppliedToSelf = EffectStateType.It;
            _context.AbilityFactory.TryGetAbility(AbilityKey.ExplodingWaterLamp, out var launchWater);
            launchWater.effectAppliedToSelf = EffectStateType.It;
        }

        private static void GrappleRehooked()
        {
            _context.AbilityFactory.TryGetAbility(AbilityKey.Grapple, out var grapple);
            grapple.effectAppliedToSelf = EffectStateType.UsedHookThisTurn;
            _context.AbilityFactory.TryGetAbility(AbilityKey.ExplodingIceLamp, out var launchIce);
            launchIce.effectAppliedToSelf = EffectStateType.UsedHookThisTurn;
            _context.AbilityFactory.TryGetAbility(AbilityKey.ExplodingOilLamp, out var launchFire);
            launchFire.effectAppliedToSelf = EffectStateType.UsedHookThisTurn;
            _context.AbilityFactory.TryGetAbility(AbilityKey.ExplodingVortexLamp, out var launchImplosion);
            launchImplosion.effectAppliedToSelf = EffectStateType.UsedHookThisTurn;
            _context.AbilityFactory.TryGetAbility(AbilityKey.ExplodingGasLamp, out var launchGas);
            launchGas.effectAppliedToSelf = EffectStateType.UsedHookThisTurn;
            _context.AbilityFactory.TryGetAbility(AbilityKey.ExplodingWaterLamp, out var launchWater);
            launchWater.effectAppliedToSelf = EffectStateType.UsedHookThisTurn;
        }
    }
}
