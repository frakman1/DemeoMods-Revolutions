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

    public sealed class PartyDamageOverriddenRule : Rule, IConfigWritable<bool>, IPatchable,
        IMultiplayerSafe
    {
        public override string Description => "Some player attacks that would hurt other players... won't";

        private static bool _isActivated;
        private readonly bool _adjustments;
        private static bool _electricOnly;
        private static Piece? _targetPiece;

        public PartyDamageOverriddenRule(bool adjustments)
        {
            _adjustments = adjustments;
            _electricOnly = _adjustments;
        }

        public bool GetConfigObject() => _adjustments;

        protected override void OnActivate(Context context) => _isActivated = true;

        protected override void OnDeactivate(Context context) => _isActivated = false;

        private static void Patch(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(Damage), "DealDamage", parameters: new[] { typeof(Target), typeof(Damage), typeof(IntPoint2D), typeof(Target), typeof(PieceAndTurnController), typeof(BoardModel), typeof(OverkillController), typeof(bool), typeof(bool), typeof(bool) }),
                prefix: new HarmonyMethod(
                    typeof(PartyDamageOverriddenRule),
                    nameof(Damage_DealDamage_Prefix)));

            harmony.Patch(
                original: AccessTools.Method(typeof(Piece), "EnableEffectState"),
                postfix: new HarmonyMethod(
                    typeof(PartyDamageOverriddenRule),
                    nameof(Piece_EnableEffectState_Postfix)));
        }

        private static void Piece_EnableEffectState_Postfix()
        {
            if (!_isActivated)
            {
                return;
            }

            if (_targetPiece != null)
            {
                if (!_targetPiece.IsImmuneToStatusEffect(EffectStateType.Stunned))
                {
                    _targetPiece.DisableEffectState(EffectStateType.Stunned);
                    _targetPiece.effectSink.SubtractHealth(0);
                }
            }

            _targetPiece = null;
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
            bool revolutions = false;
            foreach (var rule in HR.SelectedRuleset.Rules)
            {
                if (rule.ToString().Contains("PieceProgressRule") || rule.ToString().Contains("RevolutionsRule"))
                {
                    revolutions = true;
                }
            }

            // value is true so only prevent player caused electrical effects versus other players and pets
            if (_electricOnly)
            {
                if (attackerPiece != null)
                {
                    if (attackerPiece.IsPlayer() && (targetPiece.IsPlayer() || targetPiece.IsBot() || targetPiece.IsWarlockMinion() || targetPiece.HasEffectState(EffectStateType.ConfusedPermanentVisualOnly)) && damage.HasTag(DamageTag.Electricity))
                    {
                        if (damage.AbilityKey == AbilityKey.Zap)
                        {
                            _targetPiece = targetPiece;
                        }

                        return false;
                    }

                    return true;
                }

                return true;
            }

            if (revolutions)
            {
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
            }

            // value is false so players can't hurt or give negative effects to other players/pets intentionally
            if (attackerPiece != null)
            {
                if (attackerPiece.IsPlayer() && (targetPiece.IsPlayer() || targetPiece.IsBot() || targetPiece.IsWarlockMinion() || targetPiece.HasEffectState(EffectStateType.ConfusedPermanentVisualOnly)))
                {
                    if (damage.AbilityKey == AbilityKey.GodsFury || damage.AbilityKey == AbilityKey.Whirlwind || damage.AbilityKey == AbilityKey.PiercingSpear || damage.AbilityKey == AbilityKey.PlayerLeap || damage.AbilityKey == AbilityKey.Exterminate || damage.AbilityKey == AbilityKey.Implosion || damage.AbilityKey == AbilityKey.ScrollTsunami || damage.AbilityKey == AbilityKey.DeathBeam)
                    {
                        return false;
                    }
                    else if (damage.HasTag(DamageTag.Electricity))
                    {
                        if (damage.AbilityKey == AbilityKey.Zap)
                        {
                            _targetPiece = targetPiece;
                        }

                        return false;
                    }
                    else if (damage.HasTag(DamageTag.Fire))
                    {
                        targetPiece.effectSink.SubtractHealth(0);
                        return false;
                    }
                    else if (damage.HasTag(DamageTag.Ice) && !targetPiece.HasEffectState(EffectStateType.IceImmunity))
                    {
                        targetPiece.effectSink.SubtractHealth(0);
                        if (!targetPiece.IsImmuneToStatusEffect(EffectStateType.Frozen) && !targetPiece.HasEffectState(EffectStateType.Invulnerable3))
                        {
                            targetPiece.EnableEffectState(EffectStateType.IceImmunity, 1);
                        }

                        return false;
                    }
                    else if (damage.HasTag(DamageTag.Poison) && !targetPiece.HasEffectState(EffectStateType.Antidote))
                    {
                        targetPiece.effectSink.SubtractHealth(0);
                        if (!targetPiece.IsImmuneToStatusEffect(EffectStateType.Diseased) && !targetPiece.HasEffectState(EffectStateType.Antidote))
                        {
                            targetPiece.EnableEffectState(EffectStateType.Antidote, 2);
                        }

                        return false;
                    }
                    else if (attackerPiece.boardPieceId == BoardPieceId.HeroRogue && damage.HasTag(DamageTag.PhysicalMelee))
                    {
                        targetPiece.effectSink.SubtractHealth(0);
                        return false;
                    }
                    else if (damage.AbilityKey == AbilityKey.HunterArrow)
                    {
                        targetPiece.effectSink.SubtractHealth(0);
                        return false;
                    }

                    return true;
                }
                else if ((attackerPiece.boardPieceId == BoardPieceId.Tornado || attackerPiece.boardPieceId == BoardPieceId.SmiteWard || attackerPiece.boardPieceId == BoardPieceId.SwordOfAvalon || attackerPiece.boardPieceId == BoardPieceId.Verochka) && (targetPiece.IsPlayer() || targetPiece.IsBot() || targetPiece.IsWarlockMinion()))
                {
                    targetPiece.effectSink.SubtractHealth(0);
                    return false;
                }

                if (revolutions)
                {
                    if (attackerPiece.boardPieceId == BoardPieceId.GrapplingTotem && damage.AbilityKey == AbilityKey.GrapplingTotemHook)
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
            }

            if (targetPiece.IsPlayer() && revolutions)
            {
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
            }

            return true;
        }
    }
}
