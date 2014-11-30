using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp.Common;
using LeagueSharp;
using SharpDX;
using Color = System.Drawing.Color;

namespace TS
{



    public static class CustomTS
    {
        private static bool DrawText;
        private static Menu Menu;
        private static String text = "Target Selector Mode is now: ";

        public static void addTSToMenu(this Menu MainMenu)
        {
            var menu = MainMenu.AddSubMenu(new Menu("Target Selector", "Target Selector"));
            menu.AddItem(new MenuItem("Draw Target", "Draw Target")).SetValue(new Circle(true, Color.DodgerBlue));
            menu.AddItem(new MenuItem("Selected Mode", "Selected Mode"))
                .SetValue(
                    new StringList(new[]
                    {
                        "Auto", "Closest", "Less Attack", "Less Cast", "Low Hp", "Highest AD", "Highest Ap", "Near Mouse",
                        "Priority"
                    }));
            var priorMenu = menu.AddSubMenu(new Menu("Priority", "Priority"));
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(a => !a.IsAlly))
            {
                priorMenu.AddItem(new MenuItem(enemy.ChampionName, enemy.ChampionName)).SetValue(new Slider(1, 1, 5));
            }
            priorMenu.AddItem(new MenuItem("Lowest no. is Highest", "Lowest is Highest"));
            Game.OnGameUpdate += a => UpdateTSMode(MainMenu);
            Drawing.OnDraw += Drawing_OnDraw;

        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Target() != null && Menu.Item("Draw Target").GetValue<Circle>().Active)
            {
                Utility.DrawCircle(Target().Position, 120, Menu.Item("Draw Target").GetValue<Circle>().Color);
            }
        }

        private static int fatness(this Obj_AI_Hero t)
        {
            return (int) (t.ChampionsKilled*1 + t.Assists*0.375 + t.MinionsKilled*0.067);
        }

        public static void UpdateTSMode(Menu Config)
        {
            Menu = Config;
            if (DrawText)
            {
                Drawing.DrawText(100, 100, Color.White, text);
            }

            bool Priority = false;
            TargetSelector.TargetingMode mode = TargetSelector.GetTargetingMode();
            switch (Config.Item("Selected Mode").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    mode = TargetSelector.TargetingMode.AutoPriority;
                    break;
                case 1:
                    mode = TargetSelector.TargetingMode.Closest;
                    break;
                case 2:
                    mode = TargetSelector.TargetingMode.LessAttack;
                    break;
                case 3:
                    mode = TargetSelector.TargetingMode.LessCast;
                    break;
                case 4:
                    mode = TargetSelector.TargetingMode.LowHP;
                    break;
                case 5:
                    mode = TargetSelector.TargetingMode.MostAD;
                    break;
                case 6:
                    mode = TargetSelector.TargetingMode.MostAP;
                    break;
                case 7:
                    mode = TargetSelector.TargetingMode.NearMouse;
                    break;
                case 8:
                    Priority = true;
                    break;
            }
            if (TargetSelector.GetTargetingMode() != mode && Priority == false)
            {
                TargetSelector.SetTargetingMode(mode);
                text = ("Target Selector Mode is now: " + mode);
                DrawText = true;
                Utility.DelayAction.Add(2000, () => { DrawText = false; });
            }
        }

        public static Obj_AI_Hero Target()
        {
            var priorty = 5;
            Obj_AI_Hero target = null;
            if (Menu.Item("Selected Mode").GetValue<StringList>().SelectedIndex == 8)
            {

                foreach (
                    var enemy in
                        ObjectManager.Get<Obj_AI_Hero>()
                            .Where(a => !a.IsAlly && a.IsValidTarget(TargetSelector.GetRange()))
                            .OrderBy(a => a.Health))
                {
                    if (Menu.Item(enemy.ChampionName).GetValue<Slider>().Value < priorty)
                    {
                        priorty = Menu.Item(enemy.ChampionName).GetValue<Slider>().Value;
                        target = enemy;
                    }
                }
            }
            else
            {
                target = TargetSelector.Target;
            }
            return target;
        }

        public static TargetSelector TargetSelector { set; get; }
    }
}
