using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace SigmaSeries.Plugins
{
    public class Varus : PluginBase
    {
        public Varus()
            : base(new Version(0, 1, 1))
        {
            Q = new Spell(SpellSlot.Q, 1600);
            W = new Spell(SpellSlot.W, 0);
            E = new Spell(SpellSlot.E, 925);
            R = new Spell(SpellSlot.R, 1300);

            Q.SetSkillshot(0.5f, 100f, 1500f, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(1f, 235f, 1500f, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.25f, 100f, 1950f, false, SkillshotType.SkillshotLine);
            Q.SetCharged("VarusQ", "VarusQ", 1100, 1600, 1.2f);

            Orbwalking.BeforeAttack +=Orbwalking_BeforeAttack;
        }

        void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
                args.Process = !Q.IsCharging;
        }

        public static bool packetCast;


        public override void ComboMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            config.AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            config.AddItem(new MenuItem("UseRCombo", "Use R").SetValue(false));
            config.AddItem(new MenuItem("forceR", "Force R Cast").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
        }

        public override void HarassMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(false));
            config.AddItem(new MenuItem("UseEHarass", "Use E").SetValue(true));
        }

        public override void FarmMenu(Menu config)
        {
            config.AddItem(new MenuItem("useQFarm", "Q").SetValue(new StringList(new[] { "Freeze", "WaveClear", "Both", "None" }, 3)));
            config.AddItem(new MenuItem("useEFarm", "E").SetValue(new StringList(new[] { "Freeze", "WaveClear", "Both", "None" }, 3)));
            config.AddItem(new MenuItem("JungleActive", "Jungle Clear!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            config.AddItem(new MenuItem("UseQJung", "Use Q").SetValue(false));
            config.AddItem(new MenuItem("UseEJung", "Use E").SetValue(true));
        }

        public override void BonusMenu(Menu config)
        {
            config.AddItem(new MenuItem("packetCast", "Packet Cast").SetValue(true));
        }

        public override void OnUpdate(EventArgs args)
        {
            packetCast = Config.Item("packetCast").GetValue<bool>();
            var target = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Magical);
            if ((Config.Item("forceR").GetValue<KeyBind>().Active) && target != null)
            {
                R.Cast(target, true);
            }
            if (ComboActive)
            {
                Combo();
            }
            if (HarassActive)
            {
                Harass();
            }
            if (WaveClearActive)
            {
                WaveClear();
            }
            if (FreezeActive)
            {
                Freeze();
            }
            if (Config.Item("JungleActive").GetValue<KeyBind>().Active)
            {
                Jungle();
            }
        }

        private void Combo()
        {
            var useQ = Config.Item("UseQCombo").GetValue<bool>();
            var useE = Config.Item("UseECombo").GetValue<bool>();
            var useR = Config.Item("UseRCombo").GetValue<bool>();
            var Target = SimpleTs.GetTarget(1600, SimpleTs.DamageType.Magical);
            if (Target != null)
            {
                if (Q.IsReady() && useQ)
                {
                    if (Q.IsCharging)
                    {
                        Q.Cast(Target, true);
                        Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
						return;
                    }
                    if (!Q.IsCharging)
                    {
                        Q.StartCharging();
						return;
                    }
                }
				if (Q.IsCharging) return;
                if (E.IsReady() && useE && Q.IsCharging == false)
                {
                    E.Cast(Target, true);
                }
                if (R.IsReady() && useR && Q.IsCharging == false)
                {
                    R.Cast(Target, true);
                }
            }
        }

        private void Harass()
        {
            var useQ = Config.Item("UseQHarass").GetValue<bool>();
            var useE = Config.Item("UseEHarass").GetValue<bool>();
            var Target = SimpleTs.GetTarget(1600, SimpleTs.DamageType.Magical);
            if (Target != null )
            {
                if (Q.IsReady() && useQ)
                {
                    if (Q.IsCharging)
                    {
                        Q.Cast(Target, true);
                        Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                    }
                    if (!Q.IsCharging)
                    {
                        Q.StartCharging();
                    }
                }
                if (E.IsReady() && useE && Q.IsCharging == false)
                {
                    E.Cast(Target, true);
                }
            }
        }

        private void WaveClear()
        {
            var useQ = Config.Item("useQFarm").GetValue<StringList>().SelectedIndex == 1 || Config.Item("useQFarm").GetValue<StringList>().SelectedIndex == 2;
            var useE = Config.Item("useEFarm").GetValue<StringList>().SelectedIndex == 1 || Config.Item("useEFarm").GetValue<StringList>().SelectedIndex == 2;
            var jungleMinions = MinionManager.GetMinions(ObjectManager.Player.Position, E.Range, MinionTypes.All);
            if (jungleMinions.Count > 0)
            {
                foreach (var minion in jungleMinions)
                {
                    if (Q.IsReady() && useQ)
                    {
                        if (Q.IsCharging)
                        {
                            Q.Cast(minion, true);
                            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                        }
                        if (!Q.IsCharging)
                        {
                            Q.StartCharging();
                        }
                    }
                    if (E.IsReady() && useE)
                    {
                        E.Cast(minion, packetCast);
                        return;
                    }
                    
                }
            }
        }
        private void Freeze()
        {

            var useQ = Config.Item("useQFarm").GetValue<StringList>().SelectedIndex == 0 || Config.Item("useQFarm").GetValue<StringList>().SelectedIndex == 2;
            var useE = Config.Item("useEFarm").GetValue<StringList>().SelectedIndex == 0 || Config.Item("useEFarm").GetValue<StringList>().SelectedIndex == 2;
            var jungleMinions = MinionManager.GetMinions(ObjectManager.Player.Position, E.Range, MinionTypes.All);

            if (jungleMinions.Count > 0)
            {
                foreach (var minion in jungleMinions)
                {
                    if (Q.IsReady() && useQ)
                    {
                        if (Q.IsCharging)
                        {
                            Q.Cast(minion, true);
                            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                        }
                        if (!Q.IsCharging)
                        {
                            Q.StartCharging();
                        }
                    }
                    if (E.IsReady() && useE)
                    {
                        E.Cast(minion, packetCast);
                        return;
                    }
                }
            }
        }
        private void Jungle()
        {
            var useQ = Config.Item("UseQJung").GetValue<bool>();
            var useE = Config.Item("UseEJung").GetValue<bool>();
            var jungleMinions = MinionManager.GetMinions(ObjectManager.Player.Position, E.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (jungleMinions.Count > 0)
            {
                foreach (var minion in jungleMinions)
                {
                    if (Q.IsReady() && useQ)
                    {
                        if (Q.IsCharging)
                        {
                            Q.Cast(minion, true);
                            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                        }
                        if (!Q.IsCharging)
                        {
                            Q.StartCharging();
                        }
                    }
                    if (E.IsReady() && useE)
                    {
                        E.Cast(minion, packetCast);
                        return;
                    }
                }
            }
        }
    }
}
