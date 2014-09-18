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
        public static List<Obj_AI_Base> minions;
        public static Items.Item HDR;
        public static Items.Item TMT;
 

        static void Main(string[] args)
        {
            LeagueSharp.Common.CustomEvents.Game.OnGameLoad += onGameLoad;
            Game.OnGameUpdate += OnTick;
            Drawing.OnDraw += OnDraw;
            Orbwalking.OnAttack += Orbwalking_OnAttack;
        }

        static void Orbwalking_OnAttack(Obj_AI_Base unit, Obj_AI_Base target)
        {
            var stackPrior = Config.Item("stackPriority").GetValue<StringList>().SelectedIndex;
            var useQ = Config.Item("UseQCombo").GetValue<bool>();
            Orbwalker.SetAttacks(true);
            Orbwalker.SetMovement(true);
            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                if (target.IsEnemy && target.Type == GameObjectType.obj_AI_Hero && Q.IsReady() && Player.Mana > 5 && useQ || target.IsEnemy && target.Type == GameObjectType.obj_AI_Hero && Q.IsReady() && stackPrior == 0 && useQ)
                {
                    Q.Cast();
                }
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
            minions = MinionManager.GetMinions(ObjectManager.Player.Position, E.Range, MinionTypes.All);
            
            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                combo();
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
        }

        public static void combo()
        {
            var useW = Config.Item("UseWCombo").GetValue<bool>();
            var useE = Config.Item("UseECombo").GetValue<bool>();
            var stackPrior = Config.Item("stackPriority").GetValue<StringList>().SelectedIndex;
            var eTarget = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);
            if (eTarget != null)
            {
                var useQ = Config.Item("UseQCombo").GetValue<bool>();
                if (Player.Distance(eTarget) < Orbwalking.GetRealAutoAttackRange(Player))
                {
                    if (eTarget.IsEnemy && eTarget.Type == GameObjectType.obj_AI_Hero && Q.IsReady() && Player.Mana < 5 && useQ || eTarget.IsEnemy && eTarget.Type == GameObjectType.obj_AI_Hero && Q.IsReady() && stackPrior == 0 && useQ)
                    {
                        Q.Cast();
                    }
                }
                if (eTarget.IsValidTarget(E.Range) && E.IsReady() && useE && Player.Mana < 5 || eTarget.IsValidTarget(E.Range) && E.IsReady() && useE && stackPrior == 2 || Vector3.Distance(eTarget.Position, Player.Position) > Orbwalking.GetRealAutoAttackRange(Player) + 100 && eTarget.IsValidTarget(E.Range) && E.IsReady())
                {
                    E.CastIfHitchanceEquals(eTarget, HitChance.High, true);
                }
                if (Player.Distance(eTarget) < W.Range && W.IsReady() && useW && Player.Mana < 5 || Player.Distance(eTarget) < W.Range && W.IsReady() && useW && stackPrior == 1)
                {
                    W.Cast();
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
            var useW = Config.Item("UseWCombo").GetValue<bool>();
            var useE = Config.Item("UseECombo").GetValue<bool>();
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
            if (minions.Count > 1)
            {
                foreach (var minion in minions)
                {
                    var stackPrior = Config.Item("waveClearPriority").GetValue<StringList>().SelectedIndex;
                   
                    if (Player.Mana < 5 && E.GetDamage(minion) > minion.Health && E.IsReady() && minion.IsValidTarget(E.Range))
                    {
                        E.Cast(minion, true);
                    }
                    if ( W.IsReady() && minion.IsValidTarget(W.Range))
                    {
                        if (Player.Mana < 5 && W.GetDamage(minion) > minion.Health)
                        {
                            W.Cast();
                        }
                        else if (Player.Mana == 5 && stackPrior == 1)
                        {
                            W.Cast();
                        }
                    }
                    if (Q.GetDamage(minion) > minion.Health && Vector3.Distance(minion.Position, Player.Position) < Orbwalking.GetRealAutoAttackRange(Player) && Q.IsReady())
                    {
                        if (Player.Mana < 5)
                        {
                            Orbwalker.SetAttacks(false);
                            Orbwalker.SetMovement(false);
                            Q.Cast();
                            Player.IssueOrder(GameObjectOrder.AttackUnit, minion);
                            Orbwalker.SetAttacks(true);
                            Orbwalker.SetMovement(true);
                        }
                        else if (Player.Mana == 5 && stackPrior == 0)
                        {
                            Orbwalker.SetAttacks(false);
                            Orbwalker.SetMovement(false);
                            Q.Cast();
                            Player.IssueOrder(GameObjectOrder.AttackUnit, minion);
                            Orbwalker.SetAttacks(true);
                            Orbwalker.SetMovement(true);
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
        }
        public static void freeze()
        {
            if (minions.Count > 1)
            {
                foreach (var minion in minions)
                {
                    if (Player.Mana < 5 && E.GetDamage(minion) > minion.Health && E.IsReady() && minion.IsValidTarget(E.Range))
                    {
                        E.Cast(minion, true);
                    }
                    if (Player.Mana < 5 && Q.GetDamage(minion) > minion.Health && Vector3.Distance(minion.Position, Player.Position) < Orbwalking.GetRealAutoAttackRange(Player) && Q.IsReady())
                    {
                        Orbwalker.SetAttacks(false);
                        Orbwalker.SetMovement(false);
                        Q.Cast();
                        Player.IssueOrder(GameObjectOrder.AttackUnit, minion);
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
            Config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Use R").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseItems", "Use Items").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("stackPriority", "5 Stack Priority").SetValue(new StringList(new[] { "Q", "W", "E" }, 0)));
            Config.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));


            Config.AddSubMenu(new Menu("Harass", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "Use W").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "Use E").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("HarassActive", "Harass!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Farm", "Farm"));
            Config.SubMenu("Farm").AddItem(new MenuItem("FreezeActive", "Freeze!").SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("Farm").AddItem(new MenuItem("LaneClearActive", "LaneClear!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("Farm").AddItem(new MenuItem("waveClearPriority", "5 Stack Priority").SetValue(new StringList(new[] { "Q", "W" }, 0)));

            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q range").SetValue(new Circle(true, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("WRange", "W range").SetValue(new Circle(false, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("ERange", "E range").SetValue(new Circle(false, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("RRange", "R range").SetValue(new Circle(false, System.Drawing.Color.FromArgb(255, 255, 255, 255))));

            Config.AddToMainMenu();
        }
    }
}
