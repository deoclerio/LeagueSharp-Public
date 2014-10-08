using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SigmaSeries.Plugins
{
    public class Hecarim : PluginBase
    {
        public Hecarim()
            : base(new Version(0, 1, 1))
        {
            Q = new Spell(SpellSlot.Q, 350);
            W = new Spell(SpellSlot.W, 525);
            E = new Spell(SpellSlot.E, 0);
            R = new Spell(SpellSlot.R, 1350);

            R.SetSkillshot(0.5f, 200f, 1200f, false, SkillshotType.SkillshotLine);
        }

        public static bool packetCast;


        public override void ComboMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            config.AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            config.AddItem(new MenuItem("UseRCombo", "Use R!").SetValue(false));
        }

        public override void HarassMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(false));
            config.AddItem(new MenuItem("UseWHarass", "Use W").SetValue(false));
        }

        public override void FarmMenu(Menu config)
        {
            config.AddItem(new MenuItem("useQFarm", "Q").SetValue(new StringList(new[] { "Freeze", "WaveClear", "Both", "None" }, 1)));
            config.AddItem(new MenuItem("useWFarm", "W").SetValue(new StringList(new[] { "Freeze", "WaveClear", "Both", "None" }, 3)));
            config.AddItem(new MenuItem("JungleActive", "Jungle Clear!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            config.AddItem(new MenuItem("UseQJung", "Use Q").SetValue(false));
            config.AddItem(new MenuItem("UseWJung", "Use W").SetValue(true));
        }

        public override void BonusMenu(Menu config)
        {
            config.AddItem(new MenuItem("packetCast", "Packet Cast").SetValue(true));
        }

        public override void OnUpdate(EventArgs args)
        {
            if (ComboActive)
            {
                combo();
            }
            if (HarassActive)
            {
                harass();
            }
            if (WaveClearActive)
            {
                waveClear();
            }
            if (FreezeActive)
            {
                freeze();
            }
            if (Config.Item("JungleActive").GetValue<KeyBind>().Active)
            {
                jungle();
            }
        }

        private void combo()
        {
            var useQ = Config.Item("UseQCombo").GetValue<bool>();
            var useW = Config.Item("UseWCombo").GetValue<bool>();
            var useR = Config.Item("UseRCombo").GetValue<bool>();
            var Target = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Magical);
            if (Target != null)
            {
                if (Target.IsValidTarget(Q.Range) && useQ && Q.IsReady())
                {
                    Q.Cast(Target, packetCast);
                    return;
                }
                castItems(Target);
                if (Target.IsValidTarget(R.Range) && useR && R.IsReady())
                {
                    R.Cast(Target, packetCast);
                }
                if (Target.IsValidTarget(W.Range) && useW && W.IsReady())
                {
                    W.Cast(Game.CursorPos, packetCast);
                    return;
                }
            }
        }
        private void harass()
        {
            var useQ = Config.Item("UseQHarass").GetValue<bool>();
            var useW = Config.Item("UseWHarass").GetValue<bool>();
            var Target = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Magical);

            if (Target != null)
            {
                if (Target != null)
                {
                    if (Target.IsValidTarget(Q.Range) && useQ && Q.IsReady())
                    {
                        Q.Cast(Target, packetCast);
                        return;
                    }
                    if (Target.IsValidTarget(W.Range) && useW && W.IsReady())
                    {
                        W.Cast(Game.CursorPos, packetCast);
                        return;
                    }
                }
            }
        }

        private void waveClear()
        {
            var useQ = Config.Item("useQFarm").GetValue<StringList>().SelectedIndex == 1 || Config.Item("useQFarm").GetValue<StringList>().SelectedIndex == 2;
            var useW = Config.Item("useWFarm").GetValue<StringList>().SelectedIndex == 1 || Config.Item("useWFarm").GetValue<StringList>().SelectedIndex == 2;
            var jungleMinions = MinionManager.GetMinions(ObjectManager.Player.Position, 800, MinionTypes.All);

            if (jungleMinions.Count > 0)
            {
                foreach (var minion in jungleMinions)
                {
                    if (minion.IsValidTarget(Q.Range) && useQ && Q.IsReady() && Q.GetDamage(minion) > minion.Health)
                    {
                        Q.CastOnUnit(minion, packetCast);
                    }
                    if (minion.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)) && useW && W.IsReady())
                    {
                        W.Cast(Game.CursorPos, packetCast);
                    }
                }

            }
        }
        private void freeze()
        {
            var useQ = Config.Item("useQFarm").GetValue<StringList>().SelectedIndex == 0 || Config.Item("useQFarm").GetValue<StringList>().SelectedIndex == 2;
            var useW = Config.Item("useWFarm").GetValue<StringList>().SelectedIndex == 0 || Config.Item("useWFarm").GetValue<StringList>().SelectedIndex == 2;
            var jungleMinions = MinionManager.GetMinions(ObjectManager.Player.Position, 800, MinionTypes.All);

            if (jungleMinions.Count > 0)
            {
                foreach (var minion in jungleMinions)
                {
                    if (minion.IsValidTarget(Q.Range) && useQ && Q.IsReady() && Q.GetDamage(minion) > minion.Health)
                    {
                        Q.CastOnUnit(minion, packetCast);
                    }
                    if (minion.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)) && useW && W.IsReady())
                    {
                        W.Cast(Game.CursorPos, packetCast);
                    }
                }
            }
        }
        private void jungle()
        {
            var useQ = Config.Item("UseQJung").GetValue<bool>();
            var useW = Config.Item("UseWJung").GetValue<bool>();
            var jungleMinions = MinionManager.GetMinions(ObjectManager.Player.Position, 800, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (jungleMinions.Count > 0)
            {
                foreach (var minion in jungleMinions)
                {
                    if (minion.IsValidTarget(Q.Range) && useQ && Q.IsReady())
                    {
                        Q.CastOnUnit(minion, packetCast);
                    }
                    if (minion.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)) && useW && W.IsReady())
                    {
                        W.Cast(Game.CursorPos, packetCast);
                    }
                }
            }
        }
    }
}
