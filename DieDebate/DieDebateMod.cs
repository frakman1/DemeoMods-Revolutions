namespace DieDebate
{
    using MelonLoader;

    internal class DieDebateMod : MelonMod
    {
        internal static readonly MelonLogger.Instance Logger = new MelonLogger.Instance("DieDebate");

        public override void OnApplicationStart()
        {
            DieDebateMod.Logger.Msg("Loading...");
            var harmony = new HarmonyLib.Harmony("com.orendain.demeomods.diedebate");
            ModPatcher.Patch(harmony);
            DieDebateMod.Logger.Msg("Loaded...");
        }
    }
}
