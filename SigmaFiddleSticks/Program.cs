using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using BuffLib;

namespace SigmaFiddleSticks
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
        public static bool isChannel;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            LeagueSharp.Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }

        static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.Name == Player.Name && args.SData.Name == "Drain")
            {
                isChannel = true;
            }
        }


        static void Drawing_OnDraw(EventArgs args)
        {
            foreach (var spell in SpellList)
            {
                var menuItem = Config.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active)
                {
                    Drawing.DrawCircle(Player.Position, spell.Range, menuItem.Color);
                }
            }
        }

        static void Game_OnGameUpdate(EventArgs args)
        {
            if (isChannel)
            {
                Orbwalker.SetMovement(false);
                Orbwalker.SetAttacks(false);
            }
            var foundBuff = false;
            if (!Player.IsChanneling)
            {
                foreach (var buff in Player.Buffs)
                {
                    if (buff.Name == "fearmonger_marker")
                    {
                        foundBuff = true;
                    }
                }
                if (!foundBuff)
                {
                    isChannel = false;
                    Orbwalker.SetMovement(true);
                    Orbwalker.SetAttacks(true);
                }
            }
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
            if (Config.Item("JungleActive").GetValue<KeyBind>().Active)
            {
                jungle();
            }
        }

        static void combo()
        {
            var useQ = Config.Item("UseQCombo").GetValue<bool>();
            var useW = true;
            var useE = Config.Item("UseECombo").GetValue<bool>(); 
            var Target = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);
            if (Target != null)
            {
                if (!isChannel)
                {
                    if (Player.Distance(Target) < Q.Range && useQ && Q.IsReady())
                    {
                        Q.CastOnUnit(Target);
                        return;
                    }
                    if (Player.Distance(Target) < 575 && useW && W.IsReady())
                    {
                        W.CastOnUnit(Target, true);
                        Orbwalker.SetMovement(false);
                        Orbwalker.SetAttacks(false);
                        return;
                    }
                    if (Player.Distance(Target) < E.Range && useE && E.IsReady())
                    {
                        E.CastOnUnit(Target);
                        return;
                    }
                }
            }
        }
        static void harass()
        {
            var useQ = Config.Item("UseQHarass").GetValue<bool>();
            var useW = Config.Item("UseWHarass").GetValue<bool>();
            var useE = Config.Item("UseEHarass").GetValue<bool>();
            var Target = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);
            if (Target != null)
            {
                if (!isChannel)
                {
                    if (Player.Distance(Target) < Q.Range && useQ && Q.IsReady())
                    {
                        Q.CastOnUnit(Target);
                        return;
                    }
                    if (Player.Distance(Target) < W.Range && useW && W.IsReady())
                    {
                        isChannel = true;
                        W.CastOnUnit(Target);
                        return;
                    }
                    if (Player.Distance(Target) < E.Range && useE && E.IsReady())
                    {
                        E.CastOnUnit(Target);
                        return;
                    }
                }
            }
        }
        static void waveClear()
        {
            var useW = Config.Item("useWFarm").GetValue<StringList>().SelectedIndex == 1 || Config.Item("useWFarm").GetValue<StringList>().SelectedIndex == 2;
            var useE = Config.Item("useEFarm").GetValue<StringList>().SelectedIndex == 1 || Config.Item("useEFarm").GetValue<StringList>().SelectedIndex == 2;
            var jungleMinions = MinionManager.GetMinions(ObjectManager.Player.Position, E.Range, MinionTypes.All);
            if (!isChannel)
            {
                if (jungleMinions.Count > 0)
                {
                    foreach (var minion in jungleMinions)
                    {
                        if (E.IsReady() && useE)
                        {
                            E.CastOnUnit(minion);
                            return;
                        }
                        if (W.IsReady() && useW)
                        {
                            Orbwalker.SetMovement(false);
                            Orbwalker.SetAttacks(false);
                            isChannel = true;
                            W.CastOnUnit(minion);
                            return;
                        }
                    }
                }
            }
        }
        static void freeze()
        {
            var useW = Config.Item("useWFarm").GetValue<StringList>().SelectedIndex == 0 || Config.Item("useWFarm").GetValue<StringList>().SelectedIndex == 2;
            var useE = Config.Item("useEFarm").GetValue<StringList>().SelectedIndex == 0 || Config.Item("useEFarm").GetValue<StringList>().SelectedIndex == 2;
            var jungleMinions = MinionManager.GetMinions(ObjectManager.Player.Position, E.Range, MinionTypes.All);
            if (!isChannel)
            {
                if (jungleMinions.Count > 0)
                {
                    foreach (var minion in jungleMinions)
                    {
                        if (E.IsReady() && useE)
                        {
                            E.CastOnUnit(minion);
                            return;
                        }
                        if (W.IsReady() && useW)
                        {
                            isChannel = true;
                            W.CastOnUnit(minion);
                            return;
                        }
                    }
                }
            }
        }
        static void jungle()
        {
            var useQ = Config.Item("UseQJung").GetValue<bool>();
            var useW = Config.Item("UseWJung").GetValue<bool>();
            var useE = Config.Item("UseEJung").GetValue<bool>();
            var jungleMinions = MinionManager.GetMinions(ObjectManager.Player.Position, E.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            if (Player.IsChanneling == false)
            { 
                if (jungleMinions.Count > 0)
                {
                    foreach (var minion in jungleMinions)
                    {
                        if (Q.IsReady() && useQ)
                        {
                            Q.CastOnUnit(minion);
                            return;
                        }
                        if (E.IsReady() && useE)
                        {
                            E.CastOnUnit(minion);
                            return;
                        }
                        if (W.IsReady() && useW)
                        {
                            Orbwalker.SetMovement(false);
                            Orbwalker.SetAttacks(false);
                            isChannel = true;
                            W.CastOnUnit(minion);
                            return;
                        }
                       
                    }
                }
            }
        }
        static void Game_OnGameLoad(EventArgs args)
        {

            Config = new Menu("SigmaFiddle", "SigmaFiddle", true);

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            SimpleTs.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            Config.AddSubMenu(new Menu("Combo", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Harass", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(false));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "Use W").SetValue(false));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "Use E").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("HarassActive", "Harass!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Farm", "Farm"));
            Config.SubMenu("Farm").AddItem(new MenuItem("FreezeActive", "Freeze!").SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("Farm").AddItem(new MenuItem("LaneClearActive", "LaneClear!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("Farm").AddItem(new MenuItem("useWFarm", "W").SetValue(new StringList(new[] { "Freeze", "WaveClear", "Both", "None" }, 3)));
            Config.SubMenu("Farm").AddItem(new MenuItem("useEFarm", "E").SetValue(new StringList(new[] { "Freeze", "WaveClear", "Both", "None" }, 1)));
            Config.SubMenu("Farm").AddItem(new MenuItem("JungleActive", "Jungle Clear!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("Farm").AddItem(new MenuItem("UseQJung", "Use Q").SetValue(false));
            Config.SubMenu("Farm").AddItem(new MenuItem("UseWJung", "Use W").SetValue(true));
            Config.SubMenu("Farm").AddItem(new MenuItem("UseEJung", "Use E").SetValue(true));

            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q range").SetValue(new Circle(true, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("WRange", "W range").SetValue(new Circle(true, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("ERange", "E range").SetValue(new Circle(false, System.Drawing.Color.FromArgb(255, 255, 255, 255))));

            Config.AddToMainMenu();

            Player = ObjectManager.Player;
            Q = new Spell(SpellSlot.Q, 575);
            W = new Spell(SpellSlot.W, 575);
            E = new Spell(SpellSlot.E, 750);

            SpellList.Add(Q);
            SpellList.Add(E);
            SpellList.Add(W);

            Game.PrintChat("Sigma FiddleSticks Loaded.");
        }
    }
}
