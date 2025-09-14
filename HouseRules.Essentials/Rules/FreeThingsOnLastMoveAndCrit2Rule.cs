namespace HouseRules.Essentials.Rules
{
    using System.Collections.Generic;
    using Boardgame.BoardEntities;
    using Boardgame.BoardEntities.Abilities;
    using DataKeys;
    using HarmonyLib;
    using HouseRules.Core.Types;

    public sealed class FreeThingsOnLastMoveAndCrit2Rule : Rule, IConfigWritable<List<BoardPieceId>>, IPatchable, IMultiplayerSafe
    {
        public override string Description => "Some Heroes gain effects by getting critical hits on their last move";

        private static Context _context;
        private static List<BoardPieceId> _globalAdjustments;
        private static bool _isActivated;

        private readonly List<BoardPieceId> _adjustments;

        public FreeThingsOnLastMoveAndCrit2Rule(List<BoardPieceId> adjustments)
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
                    typeof(FreeThingsOnLastMoveAndCrit2Rule),
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

            var level = source.GetStatMax(Stats.Type.CritChance);

            // Start at level 4
            if (level > 3)
            {
                source.effectSink.TrySetStatBaseValue(Stats.Type.ActionPoints, currentAP + 1);
            }

            // End at level 4
            // Start at level 8
            if (level > 7)
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
                        if (value.AbilityKey == AbilityKey.DeathBeam)
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
