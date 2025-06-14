namespace HouseRules.Essentials.Rules
{
    using Boardgame;
    using Boardgame.BoardEntities;
    using Boardgame.BoardEntities.Abilities;
    using Boardgame.Data;
    using Boardgame.GameplayEffects;
    using DataKeys;
    using HarmonyLib;
    using HouseRules.Core;
    using HouseRules.Core.Types;

    public sealed class PieceExtraImmunitiesRule : Rule, IConfigWritable<bool>, IPatchable,
        IMultiplayerSafe
    {
        public override string Description => "Most abilities that would hurt other players... won't";

        private static bool _isActivated;

        public PieceExtraImmunitiesRule(bool value)
        {
        }

        public bool GetConfigObject() => true;

        protected override void OnActivate(Context context) => _isActivated = true;

        protected override void OnDeactivate(Context context) => _isActivated = false;

        private static void Patch(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(Damage), "DealDamage", parameters: new[] { typeof(Target), typeof(Damage), typeof(IntPoint2D), typeof(Target), typeof(PieceAndTurnController), typeof(BoardModel), typeof(OverkillController), typeof(bool), typeof(bool), typeof(bool) }),
                prefix: new HarmonyMethod(
                    typeof(PieceExtraImmunitiesRule),
                    nameof(Damage_DealDamage_Prefix)));
        }

        private static bool Damage_DealDamage_Prefix(Target target, Damage damage, Target attacker)
        {
            if (!_isActivated)
            {
                return true;
            }

            Piece targetPiece = target.piece;
            if (targetPiece.IsImmuneToDamage())
            {
                return true;
            }

            Piece attackerPiece = attacker.piece;
            if (attackerPiece.GetStat(Stats.Type.InnateCounterDamageExtraDamage) != 69 && !HR.SelectedRuleset.Name.Contains("Revolutions"))
            {
                if (attackerPiece.boardPieceId == BoardPieceId.HeroSorcerer)
                {
                    if (damage.HasTag(DamageTag.Electricity))
                    {
                        targetPiece.effectSink.SubtractHealth(0);
                        if (!targetPiece.HasEffectState(EffectStateType.Invulnerable3) && !targetPiece.HasEffectState(EffectStateType.Frozen) && damage.AbilityKey == AbilityKey.Zap)
                        {
                            targetPiece.EnableEffectState(EffectStateType.Invulnerable1);
                        }

                        return false;
                    }
                    else if (damage.HasTag(DamageTag.Fire))
                    {
                        targetPiece.effectSink.SubtractHealth(0);
                        return false;
                    }
                }

                return true;
            }

            if (targetPiece.IsWarlockMinion() && (attackerPiece == null || !attackerPiece.HasPieceType(PieceType.Boss)) && damage.HasTag(DamageTag.Undefined))
            {
                targetPiece.DisableEffectState(EffectStateType.CorruptedRage);
                targetPiece.effectSink.SubtractHealth(0);
                return false;
            }
            else if (targetPiece.boardPieceId == BoardPieceId.HeroWarlock && (attackerPiece == null || !attackerPiece.HasPieceType(PieceType.Boss)) && damage.HasTag(DamageTag.Undefined))
            {
                targetPiece.DisableEffectState(EffectStateType.CorruptedRage);
                targetPiece.effectSink.TrySetStatBaseValue(Stats.Type.CorruptionAP, 0);

                // if (targetPiece.GetActionPoints() > -1)
                // {
                targetPiece.effectSink.TryAddActionPoints(1);

                // }
                targetPiece.effectSink.SubtractHealth(0);
                return false;
            }

            if (targetPiece.boardPieceId == BoardPieceId.Verochka && damage.HasTag(DamageTag.Ice) && (attackerPiece == null || !attackerPiece.HasPieceType(PieceType.Boss)))
            {
                targetPiece.effectSink.SubtractHealth(0);
                return false;
            }

            if (attackerPiece != null)
            {
                if (attackerPiece.boardPieceId == BoardPieceId.HeroGuardian && (damage.AbilityKey == AbilityKey.Whirlwind || damage.AbilityKey == AbilityKey.PiercingSpear))
                {
                    BoardPieceId targetId = targetPiece.boardPieceId;
                    bool canBeHit = true;
                    if (targetId == BoardPieceId.RootVine || targetId == BoardPieceId.ProximityMine || targetId == BoardPieceId.EnemyTurret || targetId == BoardPieceId.SporeFungus || targetId.ToString().Contains("SandPile") || targetPiece.HasPieceType(PieceType.ExplodingLamp))
                    {
                        canBeHit = false;
                    }

                    if (targetPiece.IsPlayer() || targetPiece.IsBot() || (targetPiece.IsProp() && canBeHit))
                    {
                        return false;
                    }
                }
                else if (attackerPiece.boardPieceId == BoardPieceId.HeroSorcerer)
                {
                    if (damage.HasTag(DamageTag.Electricity))
                    {
                        targetPiece.effectSink.SubtractHealth(0);
                        if (!targetPiece.HasEffectState(EffectStateType.Invulnerable3) && !targetPiece.HasEffectState(EffectStateType.Frozen) && damage.AbilityKey == AbilityKey.Zap)
                        {
                            targetPiece.EnableEffectState(EffectStateType.Invulnerable1);
                        }

                        return false;
                    }
                    else if (damage.HasTag(DamageTag.Fire))
                    {
                        targetPiece.effectSink.SubtractHealth(0);
                        return false;
                    }
                }
                else if (attackerPiece.boardPieceId == BoardPieceId.HeroHunter && (damage.AbilityKey == AbilityKey.Exterminate || damage.AbilityKey == AbilityKey.HunterArrow || damage.AbilityKey == AbilityKey.PoisonedTip))
                {
                    if (!targetPiece.HasEffectState(EffectStateType.Antidote) && !targetPiece.HasEffectState(EffectStateType.Diseased) && damage.AbilityKey == AbilityKey.PoisonedTip)
                    {
                        targetPiece.EnableEffectState(EffectStateType.Antidote, 1);
                    }

                    targetPiece.effectSink.SubtractHealth(0);
                    return false;
                }
                else if (attackerPiece.boardPieceId == BoardPieceId.HeroRogue)
                {
                    if (!targetPiece.HasEffectState(EffectStateType.Antidote) && !targetPiece.HasEffectState(EffectStateType.Diseased) && damage.HasTag(DamageTag.Poison))
                    {
                        targetPiece.effectSink.SubtractHealth(0);
                        targetPiece.EnableEffectState(EffectStateType.Antidote, 1);
                        return false;
                    }
                    else if (damage.HasTag(DamageTag.PhysicalMelee))
                    {
                        targetPiece.effectSink.SubtractHealth(0);
                        return false;
                    }
                }
                else if (attackerPiece.boardPieceId == BoardPieceId.HeroRogue)
                {
                    if (!targetPiece.HasEffectState(EffectStateType.Antidote) && !targetPiece.HasEffectState(EffectStateType.Diseased) && damage.HasTag(DamageTag.Poison))
                    {
                        targetPiece.effectSink.SubtractHealth(0);
                        targetPiece.EnableEffectState(EffectStateType.Antidote, 1);
                        return false;
                    }
                }
                else if (attackerPiece.boardPieceId == BoardPieceId.Tornado)
                {
                    targetPiece.effectSink.SubtractHealth(0);
                    return false;
                }
                else if (attackerPiece.boardPieceId == BoardPieceId.GrapplingTotem && damage.AbilityKey == AbilityKey.GrapplingTotemHook)
                {
                    targetPiece.effectSink.AddStatusEffect(EffectStateType.Tangled);
                }
                else if (attackerPiece.IsWarlockMinion())
                {
                    // Cana gets Frenzy if at or below half health
                    if (attackerPiece.GetHealth() <= attackerPiece.GetMaxHealth() / 2)
                    {
                        attackerPiece.EnableEffectState(EffectStateType.Frenzy);
                        attackerPiece.effectSink.SetStatusEffectDuration(EffectStateType.Frenzy, 1);
                    }
                    else if (attackerPiece.HasEffectState(EffectStateType.Frenzy))
                    {
                        attackerPiece.DisableEffectState(EffectStateType.Frenzy);
                    }
                }
            }

            if (!targetPiece.IsPlayer())
            {
                return true;
            }

            if (targetPiece.boardPieceId == BoardPieceId.HeroBarbarian)
            {
                if ((attackerPiece == null || !attackerPiece.HasPieceType(PieceType.Boss)) && (damage.HasTag(DamageTag.Acid) || damage.AbilityKey == AbilityKey.Petrify))
                {
                    targetPiece.effectSink.SubtractHealth(0);
                    return false;
                }
            }

            if (attackerPiece == null)
            {
                return true;
            }

            if (targetPiece.boardPieceId == BoardPieceId.HeroHunter && !attackerPiece.HasPieceType(PieceType.Boss) && damage.HasTag(DamageTag.Ice))
            {
                targetPiece.effectSink.SubtractHealth(0);
                return false;
            }
            else if (targetPiece.boardPieceId == BoardPieceId.HeroGuardian && !attackerPiece.HasPieceType(PieceType.Boss) && damage.HasTag(DamageTag.Fire))
            {
                targetPiece.effectSink.SubtractHealth(0);
                return false;
            }
            else if (targetPiece.boardPieceId == BoardPieceId.HeroSorcerer && !attackerPiece.HasPieceType(PieceType.Boss) && damage.HasTag(DamageTag.Electricity))
            {
                targetPiece.effectSink.SubtractHealth(0);
                return false;
            }

            return true;
        }
    }
}
