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
        }

        public bool GetConfigObject() => _adjustments;

        protected override void OnActivate(Context context)
        {
            _isActivated = true;
            _electricOnly = _adjustments;
        }

        protected override void OnDeactivate(Context context)
        {
            _isActivated = false;
        }

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
                if (rule.ToString().Contains("Progress") || rule.ToString().Contains("Revolutions"))
                {
                    revolutions = true;
                    break;
                }
            }

            // value is true so only prevent player caused electrical effects versus other players and pets
            if (_electricOnly == true)
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
                }
            }

            if (revolutions)
            {
                if (targetPiece.IsWarlockMinion() && (attackerPiece == null || !attackerPiece.HasPieceType(PieceType.Boss)) && damage.HasTag(DamageTag.Undefined))
                {
                    targetPiece.DisableEffectState(EffectStateType.CorruptedRage);
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
                    return false;
                }

                if (targetPiece.boardPieceId == BoardPieceId.Verochka && damage.HasTag(DamageTag.Ice) && (attackerPiece == null || !attackerPiece.HasPieceType(PieceType.Boss)))
                {
                    return false;
                }
            }

            // value is false so players can't hurt or give any negative effects to other players/pets intentionally
            if (_electricOnly == false && attackerPiece != null)
            {
                if (attackerPiece.IsPlayer() && (targetPiece.IsPlayer() || targetPiece.IsBot() || targetPiece.IsWarlockMinion() || targetPiece.HasEffectState(EffectStateType.ConfusedPermanentVisualOnly)))
                {
                    if (damage.HasTag(DamageTag.Electricity))
                    {
                        if (damage.AbilityKey == AbilityKey.Zap || damage.AbilityKey == AbilityKey.LightningBolt || damage.AbilityKey == AbilityKey.Overload)
                        {
                            _targetPiece = targetPiece;
                        }
                    }
                    else if (damage.HasTag(DamageTag.Ice) && !targetPiece.HasEffectState(EffectStateType.IceImmunity))
                    {
                        if (!targetPiece.IsImmuneToStatusEffect(EffectStateType.Frozen) && !targetPiece.HasEffectState(EffectStateType.Invulnerable1) && !targetPiece.HasEffectState(EffectStateType.Invulnerable3))
                        {
                            targetPiece.EnableEffectState(EffectStateType.IceImmunity, 1);
                        }
                    }
                    else if (damage.HasTag(DamageTag.Poison) && !targetPiece.HasEffectState(EffectStateType.Antidote))
                    {
                        if (!targetPiece.IsImmuneToStatusEffect(EffectStateType.Diseased) && !targetPiece.HasEffectState(EffectStateType.Antidote))
                        {
                            targetPiece.EnableEffectState(EffectStateType.Antidote, 2);
                        }
                    }

                    return false;
                }
                else if (targetPiece.IsPlayer() || targetPiece.IsBot())
                {
                    if (damage.HasTag(DamageTag.Electricity) && !targetPiece.HasEffectState(EffectStateType.Stunned))
                    {
                        return false;
                    }
                    else if (damage.HasTag(DamageTag.Ice) && !targetPiece.HasEffectState(EffectStateType.IceImmunity))
                    {
                        return false;
                    }
                }
                else if ((attackerPiece.boardPieceId == BoardPieceId.Tornado || attackerPiece.boardPieceId == BoardPieceId.SmiteWard || attackerPiece.boardPieceId == BoardPieceId.SwordOfAvalon || attackerPiece.boardPieceId == BoardPieceId.Verochka || attackerPiece.HasEffectState(EffectStateType.ConfusedPermanentVisualOnly)) && (targetPiece.IsPlayer() || targetPiece.IsBot() || targetPiece.IsWarlockMinion() || targetPiece.HasEffectState(EffectStateType.ConfusedPermanentVisualOnly)))
                {
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
                        return false;
                    }
                }

                if (attackerPiece == null)
                {
                    return true;
                }

                if (targetPiece.boardPieceId == BoardPieceId.HeroHunter && !attackerPiece.HasPieceType(PieceType.Boss) && damage.HasTag(DamageTag.Ice))
                {
                    return false;
                }
                else if (targetPiece.boardPieceId == BoardPieceId.HeroGuardian && !attackerPiece.HasPieceType(PieceType.Boss) && damage.HasTag(DamageTag.Fire))
                {
                    return false;
                }
                else if (targetPiece.boardPieceId == BoardPieceId.HeroSorcerer && !attackerPiece.HasPieceType(PieceType.Boss) && damage.HasTag(DamageTag.Electricity))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
