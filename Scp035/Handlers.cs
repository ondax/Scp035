using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vigilance.EventHandlers;
using Vigilance.Events;
using Vigilance.API;
using Dissonance;
using Assets._Scripts.Dissonance;
using MEC;
using Mirror;

namespace Scp035
{
    public class Handlers : RoundStartHandler, PickupItemHandler, RoundEndHandler, PlayerDieEventHandler, PlayerHurtHandler, PlayerLeaveHandler, RemoteAdminEventHandler
    {
        public static Pickup Scp035Pickup;
        public static Player Scp035Player;
        public static CoroutineHandle Hurt035Coroutine;
        public static DissonanceUserSetup DUS;
        public static ItemType SCP035ItemType;
        public void OnPickupItem(PickupItemEvent ev)
        {
            if(ev.Item==Scp035Pickup)
            {
                MakePlayer035(ev.Player);
                ev.Allow = false;
                ev.Item.Delete();
                Scp035Pickup = null;
            }
        }

        public void OnPlayerDie(PlayerDieEvent ev)
        {
            if(ev.Attacker==Scp035Player)
            {
                ev.Target.ShowHint("Byl jsi zabit SCP-035", 5);
                
            }
            if(ev.Target==Scp035Player)
            {
                Scp035Player = null;
                Pickup pickup = Map.SpawnItem(SCP035ItemType, ev.Target.Position);
                Timing.KillCoroutines(Hurt035Coroutine);
                Scp035Pickup = pickup;
            }
            if(ev.Target!=Scp035Player)
            {
                CheckOnlySCPs();
            }
        }
        public void CheckOnlySCPs()
        {
            bool OnlySCPS = true;
            foreach (Player player in Server.Players)
            {
                if (player.IsAlive && !(player.IsAnySCP || player == Scp035Player || player.Role == RoleType.ChaosInsurgency) && !player.IsHost)
                {
                    OnlySCPS = false;
                }
            }
            if (OnlySCPS)
            {
                Round.End();
            }
        }

        public void OnRoundEnd(RoundEndEvent ev)
        {
            Scp035Pickup = null;
            Scp035Player = null;
        }

        public void OnRoundStart(RoundStartEvent ev)
        {
            int spawnTries = 0;
            SelectPickup();
            while((Scp035Pickup.ItemId == ItemType.Ammo556 || Scp035Pickup.ItemId == ItemType.Ammo9mm || Scp035Pickup.ItemId == ItemType.Ammo762)&&spawnTries<5)
            {
                Scp035Pickup = null;
                SelectPickup();
            }
        }
        public static void SelectPickup()
        {
            System.Random random = new System.Random();
            int randomInt = random.Next(0, Map.Pickups.Count);
            Scp035Pickup = Map.Pickups[randomInt];
            if (Scp035Pickup == null)
            {
                Scp035.Singleton.AddLog("Couldn't select 035 pickup");
            }
            else
            {
                Scp035.Singleton.AddLog("Spawned 035 pickup");
                SCP035ItemType = Scp035Pickup.ItemId;
            }
        }
        public static void MakePlayer035(Player player)
        {
            Scp035Player = player;
            Scp035Player.AddItem(ItemType.KeycardZoneManager);
            Scp035Player.ShowHint("You are now SCP-035", 5);
            Scp035Player.SetInfo(PlayerInfoArea.Role, "SCP-035");
            Scp035Player.DisplayNick = "[SCP-035] " + Scp035Player.Nick;
            foreach (NetworkBehaviour behaviour in Scp035Player.Hub.networkIdentity.NetworkBehaviours)
            {
                if(behaviour.GetType()==typeof(DissonanceUserSetup))
                {
                    DUS = (DissonanceUserSetup)behaviour;
                  //  Scp035.Singleton.AddLog("Found DissonanceUserSetup of player");
                }
            }
            DissonanceUserSetup dus = DUS;
            dus.SCPChat = true;
            dus.MimicAs939 = true;
            dus.SpeakerAs079 = true;
            Hurt035Coroutine = Timing.RunCoroutine(Damage035());
            Scp035.Singleton.AddLog("Player " + Scp035Player.Nick + " is now Scp035");
        }

        public void OnHurt(PlayerHurtEvent ev)
        {
            if(ev.Attacker.IsAnySCP&&ev.Player==Scp035Player)
            {
                ev.Allow = false;
                ev.Attacker.ShowHint("Don't attack SCP-035!");
            }
            if (ev.Attacker==Scp035Player&&ev.Player.IsAnySCP)
            {
                ev.Allow = false;
                ev.Attacker.ShowHint("Don't attack SCPs!");
            }
        }
        public static IEnumerator<float> Damage035()
        {
            while(Scp035Player!=null)
            {
                yield return Timing.WaitForSeconds(30);
                Scp035Player.Damage(1, DamageTypes.Decont);
            }
        }

        public void OnLeave(PlayerLeaveEvent ev)
        {
            if(ev.Player==Scp035Player)
            {
                Scp035Player = null;
                Pickup pickup = Map.SpawnItem(SCP035ItemType, ev.Player.Position);
                Timing.KillCoroutines(Hurt035Coroutine);
                Scp035Pickup = pickup;
            }
            CheckOnlySCPs();
        }

        public void OnRemoteAdminCommand(RemoteAdminCommandEvent ev)
        {
            if (ev.Command.ToUpper().StartsWith("FORCECLASS"))
            {
                CheckOnlySCPs();
            }
        }
    }
}
