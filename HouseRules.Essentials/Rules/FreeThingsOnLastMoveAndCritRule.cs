namespace HouseRules.Essentials.Rules
{
    using System.Collections.Generic;
    using Boardgame.BoardEntities;
    using Boardgame.BoardEntities.Abilities;
    using DataKeys;
    using HarmonyLib;
    using HouseRules.Core.Types;

    public sealed class FreeThingsOnLastMoveAndCritRule : Rule, IConfigWritable<List<BoardPieceId>>, IPatchable, IMultiplayerSafe
    {
        public override string Description => "Some Heroes gain effects by getting critical hits on their last move";

        private static Context _context;
        private static List<BoardPieceId> _globalAdjustments;
        private static bool _isActivated;

        private readonly List<BoardPieceId> _adjustments;

        public FreeThingsOnLastMoveAndCritRule(List<BoardPieceId> adjustments)
        {
            _adjustments = adjustments;
        }

        public List<BoardPieceId> GetConfigObject() => _adjustments;

        protected override void OnActivate(Context context)
        {
            _context = context;
            _globalAdjustments = _adjustments;
            _isActivated = true;
        }

        protected override void OnDeactivate(Context context) => _isActivated = false;

        private static void Patch(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(Ability), "GenerateAttackDamage"),
                postfix: new HarmonyMethod(
                    typeof(FreeThingsOnLastMoveAndCritRule),
                    nameof(Ability_GenerateAttackDamage_Postfix)));
        }

        private static void Ability_GenerateAttackDamage_Postfix(Piece source, Dice.Outcome diceResult)
        {
            if (!_isActivated)
            {
                return;
            }

            if (!source.IsPlayer())
            {
                return;
            }

            if (diceResult != Dice.Outcome.Crit)
            {
                return;
            }

            if (!_globalAdjustments.Contains(source.boardPieceId))
            {
                return;
            }

            Inventory.Item value;
            source.effectSink.TryGetStat(Stats.Type.ActionPoints, out int currentAP);
            if (currentAP > 0)
            {
                return;
            }

            // At level 1
            if (source.boardPieceId == BoardPieceId.HeroRogue)
            {
                var buffed = source.effectSink.GetEffectStateDurationTurnsLeft(EffectStateType.ExtraAction);
                if (buffed > 0)
                {
                    source.effectSink.SetStatusEffectDuration(EffectStateType.ExtraAction, buffed + 1);
                }
                else
                {
                    source.EnableEffectState(EffectStateType.ExtraAction, 2);
                }
            }
            else if (source.boardPieceId == BoardPieceId.HeroGuardian)
            {
                source.effectSink.TryGetStat(Stats.Type.Armor, out int armor);
                if (armor < 4)
                {
                    source.effectSink.TrySetStatBaseValue(Stats.Type.Armor, armor + 2);
                }
                else
                {
                    source.effectSink.TrySetStatBaseValue(Stats.Type.Armor, 5);
                }
            }
            else if (source.boardPieceId == BoardPieceId.HeroSorcerer)
            {
                source.effectSink.RemoveStatusEffect(EffectStateType.Wet);
                var overCharge = source.effectSink.GetEffectStateDurationTurnsLeft(EffectStateType.Overcharge);
                if (overCharge > 0)
                {
                    source.effectSink.SetStatusEffectDuration(EffectStateType.Overcharge, overCharge + 1);
                }
                else
                {
                    source.EnableEffectState(EffectStateType.Overcharge, 2);
                }
            }
            else if (source.boardPieceId == BoardPieceId.HeroGuardian)
            {
                source.effectSink.TryGetStat(Stats.Type.Armor, out int armor);
                if (armor < 4)
                {
                    source.effectSink.TrySetStatBaseValue(Stats.Type.Armor, armor + 2);
                }
                else
                {
                    source.effectSink.TrySetStatBaseValue(Stats.Type.Armor, 5);
                }
            }
            else if (source.boardPieceId == BoardPieceId.HeroBarbarian)
            {
                var magicShield = source.effectSink.GetEffectStateDurationTurnsLeft(EffectStateType.MagicShield1);
                if (magicShield > 0)
                {
                    source.effectSink.SetStatusEffectDuration(EffectStateType.MagicShield1, magicShield + 1);
                }
                else
                {
                    source.EnableEffectState(EffectStateType.MagicShield1, 2);
                }
            }
            else if (source.boardPieceId == BoardPieceId.HeroBard)
            {
                source.effectSink.TryGetStat(Stats.Type.MagicArmor, out int myArmor);
                if (myArmor < 6)
                {
                    source.effectSink.TrySetStatBaseValue(Stats.Type.MagicArmor, myArmor + 5);
                }
                else
                {
                    source.effectSink.TrySetStatBaseValue(Stats.Type.MagicArmor, 10);
                }
            }
            else if (source.boardPieceId == BoardPieceId.HeroHunter)
            {
                // Todo
                // var inventory = new Inventory(_context.AbilityFactory);
                source.inventory.Items.Add(new Inventory.Item(
                    AbilityKey.SpawnSpiderlings,
                    flags: 0,
                    originalOwner: -1,
                    replenishCooldown: 0));

                source.AddGold(0);
            }
            else if (source.boardPieceId == BoardPieceId.HeroWarlock)
            {
                // Todo
                var deflect = source.effectSink.GetEffectStateDurationTurnsLeft(EffectStateType.Deflect);
                if (deflect > 0)
                {
                    source.effectSink.SetStatusEffectDuration(EffectStateType.Deflect, deflect + 2);
                }
                else
                {
                    source.EnableEffectState(EffectStateType.Deflect, 3);
                }
            }

            // End at level 1
            // Start at level 3
            if (source.GetStatMax(Stats.Type.CritChance) > 3)
            {
                source.effectSink.TrySetStatBaseValue(Stats.Type.ActionPoints, currentAP + 1);
            }

            // End at level 3
            // Start at level 7
            if (source.GetStatMax(Stats.Type.CritChance) > 7)
            {
                source.effectSink.Heal(1);
                source.DisableEffectState(EffectStateType.Heal);
                source.EnableEffectState(EffectStateType.Heal, 1);
                if (source.boardPieceId == BoardPieceId.HeroRogue)
                {
                    for (int i = 0; i < source.inventory.Items.Count; i++)
                    {
                        value = source.inventory.Items[i];
                        if (value.AbilityKey == AbilityKey.Tornado)
                        {
                            if (value.IsReplenishing)
                            {
                                if (value.replenishCooldown < 0)
                                {
                                    value.replenishCooldown = 3;
                                    source.inventory.Items[i] = value;
                                }

                                value.replenishCooldown -= 1;
                                if (value.replenishCooldown < 1)
                                {
                                    value.flags &= (Inventory.ItemFlag)(-3);
                                }

                                source.inventory.Items[i] = value;
                                source.AddGold(0);
                            }

                            break;
                        }
                    }
                }
                else if (source.boardPieceId == BoardPieceId.HeroBarbarian)
                {
                    for (int i = 0; i < source.inventory.Items.Count; i++)
                    {
                        value = source.inventory.Items[i];
                        if (value.AbilityKey == AbilityKey.Exterminate)
                        {
                            if (value.IsReplenishing)
                            {
                                value.flags &= (Inventory.ItemFlag)(-3);
                                source.inventory.Items[i] = value;
                                source.AddGold(0);
                            }
                        }
                    }
                }
                else if (source.boardPieceId == BoardPieceId.HeroBard)
                {
                    for (int i = 0; i < source.inventory.Items.Count; i++)
                    {
                        value = source.inventory.Items[i];
                        if (value.AbilityKey == AbilityKey.MissileSwarm)
                        {
                            if (value.IsReplenishing)
                            {
                                value.flags &= (Inventory.ItemFlag)(-3);
                                source.inventory.Items[i] = value;
                                source.AddGold(0);
                            }
                        }
                    }
                }
                else if (source.boardPieceId == BoardPieceId.HeroHunter)
                {
                    for (int i = 0; i < source.inventory.Items.Count; i++)
                    {
                        value = source.inventory.Items[i];
                        if (value.AbilityKey == AbilityKey.Electricity)
                        {
                            if (value.IsReplenishing)
                            {
                                value.flags &= (Inventory.ItemFlag)(-3);
                                source.inventory.Items[i] = value;
                                source.AddGold(0);
                            }
                        }
                    }
                }
                else if (source.boardPieceId == BoardPieceId.HeroSorcerer)
                {
                    for (int i = 0; i < source.inventory.Items.Count; i++)
                    {
                        value = source.inventory.Items[i];
                        if (value.AbilityKey == AbilityKey.Implode)
                        {
                            if (value.IsReplenishing)
                            {
                                value.flags &= (Inventory.ItemFlag)(-3);
                                source.inventory.Items[i] = value;
                                source.AddGold(0);
                            }
                        }
                    }
                }
                else if (source.boardPieceId == BoardPieceId.HeroGuardian)
                {
                    for (int i = 0; i < source.inventory.Items.Count; i++)
                    {
                        value = source.inventory.Items[i];
                        if (value.AbilityKey == AbilityKey.MarkOfVerga)
                        {
                            if (value.IsReplenishing)
                            {
                                value.flags &= (Inventory.ItemFlag)(-3);
                                source.inventory.Items[i] = value;
                                source.AddGold(0);
                            }
                        }
                    }
                }
                else if (source.boardPieceId == BoardPieceId.HeroWarlock)
                {
                    for (int i = 0; i < source.inventory.Items.Count; i++)
                    {
                        value = source.inventory.Items[i];
                        if (value.AbilityKey == AbilityKey.Flashbang)
                        {
                            if (value.IsReplenishing)
                            {
                                value.flags &= (Inventory.ItemFlag)(-3);
                                source.inventory.Items[i] = value;
                                source.AddGold(0);
                            }
                        }
                    }
                }
            }
        }
    }
}
