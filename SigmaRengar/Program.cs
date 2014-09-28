using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SigmaRengar
{
    class Program
    {
        public static Menu Config;
        public static Orbwalking.Orbwalker Orbwalker;
        public static List<Spell> SpellList = new List<Spell>();
        public static Obj_AI_Hero Player;
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static Items.Item HDR;
        public static Items.Item TMT;
 

        static void Main(string[] args)
        {
            if (ObjectManager.Player.BaseSkinName == "Rengar")
            {
                LeagueSharp.Common.CustomEvents.Game.OnGameLoad += onGameLoad;
                Game.OnGameUpdate += OnTick;
                Drawing.OnDraw += OnDraw;
            }
        }

       

        private static void OnDraw(EventArgs args)
        {
            foreach (var spell in SpellList)
            {
                var menuItem = Config.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active && spell.IsReady())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, spell.Range, menuItem.Color);
                }
            }
        }

        private static void OnTick(EventArgs args)
        {
            Player = ObjectManager.Player;
            
            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                combo(false);
            }
            if (Config.Item("tripleQ").GetValue<KeyBind>().Active)
            {
                combo(true);
            }
            if (Config.Item("HarassActive").GetValue<KeyBind>().Active)
            {
                harass();
            }
            if (Config.Item("LaneClearActive").GetValue<KeyBind>().Active)
            {
                waveClear();
            }
            if (Config.Item("FreezeActive").GetValue<KeyBind>().Active)
            {
                freeze();
            }
            if (Config.Item("JungleActive").GetValue<KeyBind>().Active)
            {
                jungle();
            }
        }

        private static void jungle()
        {
            var useQ = Config.Item("qj").GetValue<bool>();
            var useW = Config.Item("wj").GetValue<bool>();
            var useE = Config.Item("ej").GetValue<bool>();
            var stack5 = Config.Item("52").GetValue<bool>();
            var enemyMinions = MinionManager.GetMinions(Player.Position, 1000, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            
                foreach (var minion in enemyMinions)
                {

                    var stackPrior = Config.Item("jungPriority").GetValue<StringList>().SelectedIndex;

                    if (Player.Mana < 5 && minion.IsValidTarget(E.Range) && useE)
                    {
                        E.Cast(minion, true);
                    }
                    if (W.IsReady() && minion.IsValidTarget(W.Range) && useW)
                    {
                        if (Player.Mana < 5)
                        {
                            W.Cast();
                        }
                        else if (Player.Mana == 5 && stackPrior == 1 && stack5)
                        {
                            W.Cast();
                        }
                    }
                    if (Vector3.Distance(minion.Position, Player.Position) < Orbwalking.GetRealAutoAttackRange(Player) && Q.IsReady() && useQ)
                    {
                        if (Player.Mana < 5)
                        {
                            Q.Cast();
                        }
                        else if (Player.Mana == 5 && stackPrior == 0 && stack5)
                        {
                            Q.Cast();
                        }
                    }
                    if (Player.Distance(minion) <= HDR.Range)
                    {
                        HDR.Cast(minion);
                    }
                    if (Player.Distance(minion) <= TMT.Range)
                    {
                        TMT.Cast(minion);
                    }
                }
            
        }

        public static void combo(bool is3Q)
        {
            var smartMode = Config.Item("smart5Stack").GetValue<bool>();
            var smartHP = Config.Item("wSlide").GetValue<Slider>().Value;
            var smartQ = Config.Item("q1").GetValue<bool>();
            var smartW = Config.Item("w1").GetValue<bool>();
            var smartE = Config.Item("e1").GetValue<bool>();
            var useW = Config.Item("UseWCombo").GetValue<bool>();
            var useE = Config.Item("UseECombo").GetValue<bool>();
            var stackPrior = Config.Item("stackPriority").GetValue<StringList>().SelectedIndex;
            var eTarget = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Physical);
            var aaTarget = SimpleTs.GetTarget(Orbwalking.GetRealAutoAttackRange(Player) + 200, SimpleTs.DamageType.Physical);
            if (eTarget != null)
            {
                if (is3Q)
                {
                    Player.IssueOrder(GameObjectOrder.AttackUnit, aaTarget);
                }
                if (smartMode && Player.Mana == 5 && !is3Q)
                {
                    if (smartW && Player.Health/Player.MaxHealth*100 < smartHP)
                    {
                        W.Cast();
                        return;
                    }
                    if (smartQ && Player.Distance(eTarget) <= Orbwalking.GetRealAutoAttackRange(Player))
                    {
                        Q.Cast();
                        return;
                    }
                    if (smartE && Player.Distance(eTarget) > Orbwalking.GetRealAutoAttackRange(Player))
                    {
                        E.CastIfHitchanceEquals(eTarget, HitChance.High, true);
                        return;
                    }
                }
                var useQ = Config.Item("UseQCombo").GetValue<bool>();
                if (Player.Distance(eTarget) < Orbwalking.GetRealAutoAttackRange(Player))
                {
                    if (Q.IsReady() && Player.Mana < 5 && useQ || Q.IsReady() && stackPrior == 0 && useQ && !smartMode || Q.IsReady() && is3Q)
                    {
                        Q.Cast();
                        return;
                    }
                }
                if (eTarget.IsValidTarget(E.Range) && E.IsReady() && useE && Player.Mana < 5 || eTarget.IsValidTarget(E.Range) && E.IsReady() && useE && stackPrior == 2 && !smartMode && !is3Q)
                {
                    E.CastIfHitchanceEquals(eTarget, HitChance.High, true);
                    return;
                }
                if (Player.Distance(eTarget) < W.Range && W.IsReady() && useW && Player.Mana < 5 || Player.Distance(eTarget) < W.Range && W.IsReady() && useW && stackPrior == 1 && !smartMode && !is3Q)
                {
                    W.Cast();
                    return;
                }

                if (Config.Item("UseItems").GetValue<bool>())
                {
                    
                    if (Items.HasItem(HDR.Id) && HDR.IsReady())
                    {
                        if (Player.Distance(eTarget) <= HDR.Range)
                        {
                            HDR.Cast(eTarget);
                        }
                    }
                    if (Items.HasItem(TMT.Id) && TMT.IsReady())
                    {
                        if (Player.Distance(eTarget) <= TMT.Range)
                        {
                            TMT.Cast(eTarget);
                        }
                    }
                }
            }

        }
        public static void harass()
        {
            var useW = Config.Item("UseWHarass").GetValue<bool>();
            var useE = Config.Item("UseEHarass").GetValue<bool>();
            var stackPrior = Config.Item("stackPriority").GetValue<StringList>().SelectedIndex;
            var eTarget = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);
            if (eTarget != null)
            {
                if (eTarget.IsValidTarget(E.Range) && E.IsReady() && useE && Player.Mana > 5 || eTarget.IsValidTarget(E.Range) && E.IsReady() && useE && stackPrior == 2 || Vector3.Distance(eTarget.Position, Player.Position) > Orbwalking.GetRealAutoAttackRange(Player) + 100 && eTarget.IsValidTarget(E.Range) && E.IsReady())
                {
                    E.CastIfHitchanceEquals(eTarget, HitChance.High, true);
                    return;
                }
                if (eTarget.IsValidTarget(W.Range) && W.IsReady() && useW && Player.Mana > 5 || eTarget.IsValidTarget(W.Range) && W.IsReady() && useW && stackPrior == 1)
                {
                    W.Cast();
                    return;
                }
            }
        }
        public static void waveClear()
        {
            var useQ = Config.Item("ql").GetValue<bool>();
            var useW = Config.Item("wl").GetValue<bool>();
            var useE = Config.Item("el").GetValue<bool>();
            var minions = MinionManager.GetMinions(ObjectManager.Player.Position, E.Range, MinionTypes.All, MinionTeam.Enemy);
            var stackPrior = Config.Item("waveClearPriority").GetValue<StringList>().SelectedIndex;
            var stack5 = Config.Item("51").GetValue<bool>();
                
            foreach (var minion in minions)
                {
                    if (E.IsReady())
                    {
                        if (Player.Mana < 5 && E.GetDamage(minion) > minion.Health && minion.IsValidTarget(E.Range) && useE)
                        {
                            E.Cast(minion, true);
                        }
                    }
                    if (W.IsReady())
                    {
                        if (minion.IsValidTarget(W.Range) && useW)
                        {
                            if (Player.Mana < 5 && W.GetDamage(minion) > minion.Health)
                            {
                                W.Cast();
                            }
                            else if (Player.Mana == 5 && stackPrior == 1 && stack5)
                            {
                                W.Cast();
                            }
                        }
                    }
                    if (Q.IsReady())
                    {
                        if (Q.GetDamage(minion) > minion.Health && Vector3.Distance(minion.Position, Player.Position) < Orbwalking.GetRealAutoAttackRange(Player) && useQ)
                        {
                            if (Player.Mana < 5)
                            {
                                Q.Cast();
                            }
                            else if (Player.Mana == 5 && stackPrior == 0 && stack5)
                            {
                                Q.Cast();
                            }
                        }
                    }
                    if (Player.Distance(minion) <= HDR.Range)
                    {
                        HDR.Cast(minion);
                    }
                    if (Player.Distance(minion) <= TMT.Range)
                    {
                        TMT.Cast(minion);
                    }
                }
            
        }
        public static void freeze()
        {
            var useQ = Config.Item("qf").GetValue<bool>();
            var useW = Config.Item("wf").GetValue<bool>();
            var useE = Config.Item("ef").GetValue<bool>();
            var minions = MinionManager.GetMinions(ObjectManager.Player.Position, E.Range, MinionTypes.All, MinionTeam.Enemy);
            
                foreach (var minion in minions)
                {
                    if (Q.IsReady())
                    {
                        if (Player.Mana < 5 && Q.GetDamage(minion) > minion.Health && minion.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)) && useQ)
                        {
                            Q.Cast();
                        }
                    }
                    if (E.IsReady())
                    {
                        if (Player.Mana < 5 && E.GetDamage(minion) > minion.Health && minion.IsValidTarget(E.Range) && useE)
                        {
                            E.Cast(minion, true);
                        }
                    }
                    if (W.IsReady())
                    {
                        if (Player.Mana < 5 && W.GetDamage(minion) > minion.Health && useW && minion.IsValidTarget(W.Range))
                        {
                            W.Cast();
                        }
                    }
                }
            
        }

        private static void onGameLoad(EventArgs args)
        {
            HDR = new Items.Item(3074, 175f);
            TMT = new Items.Item(3077, 175f);

            Q = new Spell(SpellSlot.Q, 0);
            W = new Spell(SpellSlot.W, 500);
            E = new Spell(SpellSlot.E, 1000);

            E.SetSkillshot(0.5f, 70, 1500, false, SkillshotType.SkillshotLine);

            SpellList.Add(W);
            SpellList.Add(E);

            Config = new Menu("SigmaRengar", "SigmaRengar", true);

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            SimpleTs.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            Config.AddSubMenu(new Menu("Combo", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseItems", "Use Items").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("sep", "-- Smart Stack Settings"));
            Config.SubMenu("Combo").AddItem(new MenuItem("smart5Stack", "Use Stacks Smartly").SetValue(false));
            Config.SubMenu("Combo").AddItem(new MenuItem("q1", "Q if AA Able.").SetValue(false));
            Config.SubMenu("Combo").AddItem(new MenuItem("w1", "W Below % Hp").SetValue(false));
            Config.SubMenu("Combo").AddItem(new MenuItem("wSlide", "% HP").SetValue(new Slider(15, 0, 100)));
            Config.SubMenu("Combo").AddItem(new MenuItem("e1", "E if Distance > AA Range").SetValue(false));
            Config.SubMenu("Combo").AddItem(new MenuItem("sep2", "-- No Smart Stack Settings"));
            Config.SubMenu("Combo").AddItem(new MenuItem("stackPriority", "5 Stack Priority").SetValue(new StringList(new[] { "Q", "W", "E" }, 0)));
            Config.SubMenu("Combo").AddItem(new MenuItem("tripleQ", "Triple Q!").SetValue(new KeyBind(32, KeyBindType.Press)));
            Config.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));


            Config.AddSubMenu(new Menu("Harass", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "Use W").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "Use E").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("HarassActive", "Harass!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Farm", "Farm"));
            Config.SubMenu("Farm").AddItem(new MenuItem("sep22", "-- Freeze Settings"));
            Config.SubMenu("Farm").AddItem(new MenuItem("FreezeActive", "Freeze!").SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("Farm").AddItem(new MenuItem("qf", "Q").SetValue(false));
            Config.SubMenu("Farm").AddItem(new MenuItem("wf", "W").SetValue(false));
            Config.SubMenu("Farm").AddItem(new MenuItem("ef", "E").SetValue(false));
            Config.SubMenu("Farm").AddItem(new MenuItem("sep33", "-- Lane Clear Settings"));
            Config.SubMenu("Farm").AddItem(new MenuItem("LaneClearActive", "Lane Clear!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("Farm").AddItem(new MenuItem("51", "Use 5th Passive Stack").SetValue(false));
            Config.SubMenu("Farm").AddItem(new MenuItem("waveClearPriority", "5 Stack Priority").SetValue(new StringList(new[] { "Q", "W" }, 0)));
            Config.SubMenu("Farm").AddItem(new MenuItem("ql", "Q").SetValue(false));
            Config.SubMenu("Farm").AddItem(new MenuItem("wl", "W").SetValue(false));
            Config.SubMenu("Farm").AddItem(new MenuItem("el", "E").SetValue(false));
            Config.SubMenu("Farm").AddItem(new MenuItem("sep", "-- Jungle Settings"));
            Config.SubMenu("Farm").AddItem(new MenuItem("JungleActive", "Jungle Clear!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("Farm").AddItem(new MenuItem("52", "Use 5th Passive Stack").SetValue(false));
            Config.SubMenu("Farm").AddItem(new MenuItem("jungPriority", "5 Stack Priority").SetValue(new StringList(new[] { "Q", "W" }, 0)));
            Config.SubMenu("Farm").AddItem(new MenuItem("qj", "Q").SetValue(false));
            Config.SubMenu("Farm").AddItem(new MenuItem("wj", "W").SetValue(false));
            Config.SubMenu("Farm").AddItem(new MenuItem("ej", "E").SetValue(false));

            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q range").SetValue(new Circle(true, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("WRange", "W range").SetValue(new Circle(false, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("ERange", "E range").SetValue(new Circle(false, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("RRange", "R range").SetValue(new Circle(false, System.Drawing.Color.FromArgb(255, 255, 255, 255))));

            Config.AddToMainMenu();
        }
    }
}
