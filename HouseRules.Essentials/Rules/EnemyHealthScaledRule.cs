namespace HouseRules.Essentials.Rules
{
    using Boardgame;
    using Boardgame.BoardEntities;
    using DataKeys;
    using HarmonyLib;
    using HouseRules.Core;
    using HouseRules.Core.Types;
    using UnityEngine;

    public sealed class EnemyHealthScaledRule : Rule, IConfigWritable<float>, IPatchable, IMultiplayerSafe
    {
        public override string Description => "Enemy health is adjusted";

        protected override SyncableTrigger ModifiedSyncables => SyncableTrigger.NewPieceModified;

        private static float _globalMultiplier;
        private static bool _isActivated;
        private static bool _wizardBoss;
        private static bool revolutions;
        private static int _wizardHealth;
        private readonly float _multiplier;

        public EnemyHealthScaledRule(float multiplier)
        {
            _multiplier = multiplier;
        }

        public float GetConfigObject() => _multiplier;

        protected override void OnActivate(Context context)
        {
            _globalMultiplier = _multiplier;
            _isActivated = true;
            foreach (var rule in HR.SelectedRuleset.Rules)
            {
                if (rule.ToString().Contains("Revolutions"))
                {
                    revolutions = true;
                    break;
                }
            }
        }

        protected override void OnDeactivate(Context context)
        {
            _wizardBoss = false;
            _wizardHealth = 0;
            _isActivated = false;
        }

        private static void Patch(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(Piece), "CreatePiece"),
                postfix: new HarmonyMethod(
                    typeof(EnemyHealthScaledRule),
                    nameof(CreatePiece_StartHealth_Postfix)));
        }

        private static void CreatePiece_StartHealth_Postfix(ref Piece __result)
        {
            if (!_isActivated)
            {
                return;
            }

            if (__result.IsPlayer() || __result.IsBot() || __result.IsProp() || __result.boardPieceId == BoardPieceId.MonsterBait || !__result.IsCreature())
            {
                return;
            }

            float range = 1f;
            if (revolutions)
            {
                var ruleSet = HR.SelectedRuleset.Name;
                if (ruleSet.Contains("(LEGENDARY"))
                {
                    range = Random.Range(1.334f, 1.667f);
                }
                else if (ruleSet.Contains("(HARD"))
                {
                    range = Random.Range(1.25f, 1.5f);
                }
                else if (ruleSet.Contains("(EASY"))
                {
                    range = Random.Range(0.75f, 1f);
                }
                else if (ruleSet.Contains("PROGRESSIVE") || HR.SelectedRuleset.Name.Equals("SURVIVE!"))
                {
                    var gameContext = Traverse.Create(typeof(GameHub)).Field<GameContext>("gameContext").Value;
                    var level = gameContext.levelLoaderAndInitializer.GetLevelSequence().CurrentLevelIndex;
                    if (level == 1)
                    {
                        range = Random.Range(1.0f, 1.3f);
                    }
                    else if (level == 3)
                    {
                        range = Random.Range(1.3f, 1.6f);
                    }
                    else if (level == 5)
                    {
                        if (__result.HasPieceType(PieceType.Boss))
                        {
                            range = Random.Range(1.9f, 2.25f);
                        }
                        else
                        {
                            range = Random.Range(1.6f, 2f);
                        }
                    }
                }
                else
                {
                    if (__result.GetMaxHealth() < 5)
                    {
                        return;
                    }

                    range = Random.Range(0.85f, 1.2f);
                }
            }

            if (__result.boardPieceId == BoardPieceId.WizardBoss && _wizardBoss)
            {
                __result.effectSink.TrySetStatMaxValue(Stats.Type.Health, _wizardHealth);
                __result.effectSink.TrySetStatBaseValue(Stats.Type.Health, _wizardHealth);
                return;
            }

            int newStartHealth = (int)(__result.GetMaxHealth() * _globalMultiplier * range);
            __result.effectSink.TrySetStatMaxValue(Stats.Type.Health, newStartHealth);
            __result.effectSink.TrySetStatBaseValue(Stats.Type.Health, newStartHealth);

            if (__result.boardPieceId == BoardPieceId.WizardBoss && !_wizardBoss)
            {
                _wizardBoss = true;
                _wizardHealth = newStartHealth;
            }
        }
    }
}
