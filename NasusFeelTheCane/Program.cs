using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using LX_Orbwalker;
using SharpDX;

namespace NasusFeelTheCane
{
    class Program
    {
        public static Menu Config;
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static Obj_AI_Hero Player;
        public static Int32 Sheen = 3057, Iceborn = 3025;

        public static List<NewBuff> buffList =  new List<NewBuff>
        {
            
            new NewBuff()
            {
                DisplayName = "PantheonPassiveShield", Name = "pantheonpassiveshield"
            },
            new NewBuff()
            {
                DisplayName = "FioraRiposte", Name = "FioraRiposte"
            },
            new NewBuff()
            {
                DisplayName = "JaxEvasion", Name = "JaxCounterStrike"
            },
        };
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Game.OnGameUpdate += Game_OnGameUpdate;
            
        }

        static void Game_OnGameUpdate(EventArgs args)
        {
            Obj_AI_Hero target = SimpleTs.GetTarget(800, SimpleTs.DamageType.Physical);
            if ((Player.Health/Player.MaxHealth*100) <= Config.Item("minRHP").GetValue<Slider>().Value)
            {
                if ((Config.Item("minRChamps").GetValue<Slider>().Value == 0) ||
                    (Config.Item("minRChamps").GetValue<Slider>().Value > 0) &&
                    Utility.CountEnemysInRange(800) >= Config.Item("minRChamps").GetValue<Slider>().Value)
                {
                    R.Cast(true);
                }
            }
            if (LXOrbwalker.CurrentMode == LXOrbwalker.Mode.Combo && target != null)
            {
                if (target.IsValidTarget(W.Range) && paramBool("ComboW")) W.CastOnUnit(target);
                if (target.IsValidTarget(E.Range) && paramBool("ComboE")) E.Cast(target, Config.Item("packets").GetValue<bool>());
                if (hasAntiAA(target)) return;
                if (target.IsValidTarget(LXOrbwalker.GetAutoAttackRange(Player) + 100) && paramBool("ComboQ"))
                {
                    Q.Cast(Config.Item("packets").GetValue<bool>());
                }

            }
            if (isFarmMode())
            {
                var jungleMinions = MinionManager.GetMinions(Player.Position, E.Range, MinionTypes.All,
                    MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
                var laneMinions = MinionManager.GetMinions(Player.Position, E.Range, MinionTypes.All, MinionTeam.Enemy,
                    MinionOrderTypes.MaxHealth);
                if((LXOrbwalker.CurrentMode == LXOrbwalker.Mode.LaneClear))
                {
                    if (jungleMinions.Count > 0)
                    {
                        if (Q.IsReady() && Q.IsReady() && paramBool("WaveClearQ"))
                        {
                            Q.Cast(Config.Item("packets").GetValue<bool>());
                        }
                        if (!E.IsReady() && paramBool("WaveClearE"))
                        {
                            List<Vector2> minionerinos2 =
                                (from minions in jungleMinions select minions.Position.To2D()).ToList();
                            var ePos2 =
                                MinionManager.GetBestCircularFarmLocation(minionerinos2, E.Width, E.Range).Position;
                            if (ePos2.Distance(Player.Position.To2D()) < E.Range)
                            {
                                E.Cast(ePos2, Config.Item("packets").GetValue<bool>());
                            }
                        }
                    }

                    if (jungleMinions.Count > 0) return;
                    foreach (var minion in laneMinions)
                    {
                        if (GetBonusDmg(minion) > minion.Health &&
                            minion.Distance(Player) < LXOrbwalker.GetAutoAttackRange(Player) + 50 && Q.IsReady() && paramBool("JungleQ"))
                        {
                            LXOrbwalker.SetAttack(false);
                            Player.IssueOrder(GameObjectOrder.AttackUnit, minion);
                            LXOrbwalker.SetAttack(true);
                            break;
                        }
                    }
                    if (!E.IsReady() && paramBool("JungleE"))
                    {
                        List<Vector2> minionerinos =
                            (from minions in laneMinions select minions.Position.To2D()).ToList();
                        var ePos2 =
                            MinionManager.GetBestCircularFarmLocation(minionerinos, E.Width, E.Range).Position;
                        if (ePos2.Distance(Player.Position.To2D()) < E.Range)
                        {
                            E.Cast(ePos2, Config.Item("packets").GetValue<bool>());
                        }
                    }
                }
                if ((LXOrbwalker.CurrentMode == LXOrbwalker.Mode.LaneFreeze || LXOrbwalker.CurrentMode == LXOrbwalker.Mode.Lasthit) && paramBool("LastHitQ"))
                {
                    if (jungleMinions.Count > 0) return;
                    foreach (var minion in laneMinions)
                    {
                        if (GetBonusDmg(minion) > minion.Health &&
                            minion.Distance(Player) < LXOrbwalker.GetAutoAttackRange(Player) + 50 && Q.IsReady())
                        {
                            LXOrbwalker.SetAttack(false);
                            Player.IssueOrder(GameObjectOrder.AttackUnit, minion);
                            LXOrbwalker.SetAttack(true);
                            break;
                        }
                    }
                }
            }
        }

        public static bool hasAntiAA(Obj_AI_Hero target)
        {
            foreach (var buff in buffList)
            {
                if (target.HasBuff(buff.DisplayName) || target.HasBuff(buff.Name) ||
                    Player.HasBuffOfType(BuffType.Blind)) return true;
            }
            return false;
        } 

        public static bool isFarmMode()
        {  
            return LXOrbwalker.CurrentMode == LXOrbwalker.Mode.LaneClear ||
                   LXOrbwalker.CurrentMode == LXOrbwalker.Mode.Lasthit ||
                   LXOrbwalker.CurrentMode == LXOrbwalker.Mode.LaneFreeze;
        }

        static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!args.SData.Name.ToLower().Contains("attack") || !sender.IsMe) return;
            var unit = ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(args.Target.NetworkId);
            if ((GetBonusDmg(unit) > unit.Health) && isFarmMode())
            {
                Q.Cast(Config.Item("packets").GetValue<bool>());
            }
        }

        // From Master of Nasus + modified by me
        private static double GetBonusDmg(Obj_AI_Base target)
        {
            double DmgItem = 0;
            if (Items.HasItem(Sheen) && (Items.CanUseItem(Sheen) || Player.HasBuff("sheen", true)) && Player.BaseAttackDamage > DmgItem) DmgItem = Damage.GetAutoAttackDamage(Player, target);
            if (Items.HasItem(Iceborn) && (Items.CanUseItem(Iceborn) || Player.HasBuff("itemfrozenfist", true)) && Player.BaseAttackDamage * 1.25 > DmgItem) DmgItem = Damage.GetAutoAttackDamage(Player, target) * 1.25;
            return Q.GetDamage(target) + Damage.GetAutoAttackDamage(Player, target) + DmgItem;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;
            Q = new Spell(SpellSlot.Q, LXOrbwalker.GetAutoAttackRange(Player));
            W = new Spell(SpellSlot.W, 600);
            E = new Spell(SpellSlot.E, 650);
            R = new Spell(SpellSlot.R, 0);
            E.SetSkillshot(E.Instance.SData.SpellCastTime, E.Instance.SData.LineWidth, E.Instance.SData.MissileSpeed, false, SkillshotType.SkillshotCircle);

            Config = new Menu("Nasus - Feel The Cane", "nftc", true);

            var OWMenu = Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            LXOrbwalker.AddToMenu(OWMenu);
            var TSMenu = Config.AddSubMenu(new Menu("Target Selector", "Target Selector"));
            SimpleTs.AddToMenu(TSMenu);
            var ComboMenu = Config.AddSubMenu(new Menu("Combo", "Combo"));
            ComboMenu.AddItem(new MenuItem("ComboQ", "Combo with Q").SetValue(true));
            ComboMenu.AddItem(new MenuItem("ComboW", "Combo with W").SetValue(true));
            ComboMenu.AddItem(new MenuItem("ComboE", "Combo with E").SetValue(true));
            ComboMenu.AddItem(new MenuItem("ndskafjk", "-- R Settings"));
            ComboMenu.AddItem(new MenuItem("ComboR", "Combo with R").SetValue(true));
            ComboMenu.AddItem(new MenuItem("minRHP", "Min HP For R").SetValue(new Slider(1, 1)));
            ComboMenu.AddItem(new MenuItem("minRChamps", "Min Champs For R").SetValue(new Slider(0, 0, 5)));
            ComboMenu.AddItem(new MenuItem("fsffs", "Set to 0 to disable"));

            var FarmMenu = Config.AddSubMenu(new Menu("Farm", "Farm"));
            FarmMenu.AddItem(new MenuItem("pratum", "-- Last Hit"));
            FarmMenu.AddItem(new MenuItem("LastHitQ", "LastHit with Q").SetValue(true));
            FarmMenu.AddItem(new MenuItem("pratum2", "-- WaveClear"));
            FarmMenu.AddItem(new MenuItem("WaveClearQ", "WaveClear with Q").SetValue(true));
            FarmMenu.AddItem(new MenuItem("WaveClearE", "WaveClear with E").SetValue(true));
            FarmMenu.AddItem(new MenuItem("pratum22", "-- Jungle"));
            FarmMenu.AddItem(new MenuItem("JungleQ", "Jungle with Q").SetValue(true));
            FarmMenu.AddItem(new MenuItem("JungleE", "Jungle with E").SetValue(true));

            Config.AddItem(new MenuItem("packets", "Packet Cast?")).SetValue(true);

            Config.AddToMainMenu();
        }

        public static bool paramBool(String menuName)
        {
            return Config.Item(menuName).GetValue<bool>();
        }
    }
}
