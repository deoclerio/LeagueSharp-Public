using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Igniter;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;


namespace SigmaFizz
{
    class Program
    {
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell E2;
        public static Spell R;
        public static Menu Config;
        public static Orbwalking.Orbwalker Orbwalker;
        public static List<Spell> SpellList = new List<Spell>();
        public static Obj_AI_Hero Player;
        public static int jumpState;
        public static float time;
        public static bool called;


        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnLoad;
            Game.OnGameUpdate += OnTick;
            Drawing.OnDraw += OnDraw;
            LeagueSharp.Obj_AI_Base.OnProcessSpellCast += OnProcSpell;
            Orbwalking.BeforeAttack += BefAtt;
        }

        private static void BefAtt(Orbwalking.BeforeAttackEventArgs args)
        {
            if (args.Target.Type == GameObjectType.obj_AI_Hero)
            {
                if (Config.Item("UseWCombo").GetValue<bool>() || Config.Item("UseWHarass").GetValue<bool>())
                {
                    W.Cast();
                }
            }
        }

        private static void OnProcSpell(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.Name == Player.Name) 
            {
                if (args.SData.Name == "FizzJump")
                {
                    jumpState = 1;
                    time = Game.Time;
                    called = false;
                }

            }
        }

        private static void OnDraw(EventArgs args)
        {
            foreach (var spell in SpellList)
            {
                var menuItem = Config.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active)
                {
                    Utility.DrawCircle(ObjectManager.Player.ServerPosition, spell.Range, menuItem.Color);
                }
            }
        }

        private static void OnTick(EventArgs args)
        {

            if (time + 1f < Game.Time && !called)
            {
                called = true;
                jumpState = 0;
            }

            Player = ObjectManager.Player;
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

        private static void waveClear()
        {
            var minions = MinionManager.GetMinions(Player.ServerPosition, 800);
            var rangedMinionsE = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range + E.Width + 30, MinionTypes.All);
            var useQ = Config.Item("UseQFarm").GetValue<StringList>().SelectedIndex;
            var useE = Config.Item("UseEFarm").GetValue<StringList>().SelectedIndex;

            foreach (var min in minions)
            {
                if (useQ == 1 || useQ == 2)
                {
                    if (Q.IsReady() && Q.GetDamage(min) >= min.Health)
                    {
                        Q.CastOnUnit(min);
                    }
                }
                if (useE == 1 || useE == 2)
                {
                    if (E.IsReady() && jumpState == 0)
                    {
                        var ePos = E.GetCircularFarmLocation(rangedMinionsE);
                        E.Cast(ePos.Position, true);
                    }
                }
                
            }
        }

        private static void freeze()
        {
            var minions = MinionManager.GetMinions(Player.ServerPosition, 800);
            var useQ = Config.Item("UseQFarm").GetValue<StringList>().SelectedIndex;

            foreach (var min in minions)
            {
                if (useQ == 0 || useQ == 2)
                {
                    if (Q.IsReady() && Q.GetDamage(min) >= min.Health)
                    {
                        Q.CastOnUnit(min);
                    }
                }

            }
        }
            


        private static void harass()
        {
            var qTarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            var eTarget = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);

            if (qTarget != null && Config.Item("UseQHarass").GetValue<bool>())
            {
                if (qTarget.IsValidTarget(Q.Range) && Q.IsReady())
                {
                    Q.CastOnUnit(qTarget);
                }
            }

            if (eTarget != null && Config.Item("UseEHarass").GetValue<bool>())
            {
                if (eTarget.IsValidTarget(E.Range * 2) && E.IsReady())
                {
                    if (Vector3.Distance(Player.ServerPosition, eTarget.ServerPosition) < E.Range && jumpState != 1)
                    {
                        if (E.GetPrediction(eTarget).Hitchance >= HitChance.High)
                        {
                            E.Cast(eTarget, true);
                        }
                    }

                    if (Vector3.Distance(Player.ServerPosition, eTarget.ServerPosition) < E.Range && Vector3.Distance(Player.ServerPosition, eTarget.ServerPosition) > 300 && jumpState == 1)
                    {
                        if (E2.GetPrediction(eTarget).Hitchance >= HitChance.High)
                        {

                            E2.Cast(eTarget, true);
                        }
                    }
                }
            }
        }

        private static void combo()
        {
            var qTarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            var wTarget = SimpleTs.GetTarget(Orbwalking.GetRealAutoAttackRange(Player), SimpleTs.DamageType.Magical);
            var eTarget = SimpleTs.GetTarget(800, SimpleTs.DamageType.Magical);
            var rTarget = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Magical);

            if (qTarget != null && Config.Item("UseQCombo").GetValue<bool>())
            {
                if (qTarget.IsValidTarget(Q.Range) && Q.IsReady())
                {

                    Q.CastOnUnit(qTarget);
                }
            }
            if (eTarget != null && Config.Item("UseECombo").GetValue<bool>())
            {
                if (eTarget.IsValidTarget(800) && E.IsReady())
                {
                    if (Vector3.Distance(Player.ServerPosition, eTarget.ServerPosition) < 800 && jumpState != 1)
                    {
                        if (E.GetPrediction(eTarget).Hitchance >= HitChance.High)
                        {
                            E.Cast(eTarget, true);
                        }
                    }

                    if (Vector3.Distance(Player.ServerPosition, eTarget.ServerPosition) < E.Range && Vector3.Distance(Player.ServerPosition, eTarget.ServerPosition) > 300 && jumpState == 1)
                    {
                        if (E2.GetPrediction(eTarget).Hitchance >= HitChance.High)
                        {

                            E2.Cast(eTarget, true);
                        }
                    }
                }
            }
            if (rTarget != null && Config.Item("UseRCombo").GetValue<bool>())
            {
                if (rTarget.IsValidTarget(R.Range) && R.IsReady())
                {
                    if (R.GetDamage(rTarget) > rTarget.Health || R.GetDamage(rTarget) + W.GetDamage(rTarget) + E.GetDamage(rTarget) + Q.GetDamage(rTarget) > rTarget.Health)
                    {
                        if (R.GetPrediction(rTarget).Hitchance >= HitChance.High)
                        {
                            R.Cast(rTarget, true);
                        }
                    }
                }
            }
        }

        private static void OnLoad(EventArgs args)
        {


            Q = new Spell(SpellSlot.Q, 550);
            W = new Spell(SpellSlot.W, 0);
            E = new Spell(SpellSlot.E, 400);
            E2 = new Spell(SpellSlot.E, 400);
            R = new Spell(SpellSlot.R, 1275);
           

            E.SetSkillshot(0.5f, 120, 1300, false, SkillshotType.SkillshotCircle);
            E2.SetSkillshot(0.5f, 400, 1300, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.5f, 250f, 1200f, false, SkillshotType.SkillshotLine);
            
            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            Config = new Menu("SigmaFizz", "SigmaFizz", true);

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
            Config.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Harass", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "Use W").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "Use E").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("HarassActive", "Harass!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Farm", "Farm"));
            Config.SubMenu("Farm").AddItem(new MenuItem("UseQFarm", "Use Q").SetValue(new StringList(new[] { "Freeze", "LaneClear", "Both", "No" }, 0)));
            Config.SubMenu("Farm").AddItem(new MenuItem("UseEFarm", "Use E").SetValue(new StringList(new[] { "Freeze", "LaneClear", "Both", "No" }, 1)));
            Config.SubMenu("Farm").AddItem( new MenuItem("FreezeActive", "Freeze!").SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("Farm").AddItem( new MenuItem("LaneClearActive", "LaneClear!").SetValue(new KeyBind("C".ToCharArray()[0],KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q range").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("WRange", "W range").SetValue(new Circle(false, Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("ERange", "E range").SetValue(new Circle(false, Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("RRange", "R range").SetValue(new Circle(false, Color.FromArgb(255, 255, 255, 255))));

            Config.AddToMainMenu();

        }
    }
}
