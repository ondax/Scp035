using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vigilance;
using Vigilance.API;
using Vigilance.Extensions;

namespace Scp035
{
    public class Scp035 : Plugin
    {
        public static Scp035 Singleton;
        public bool Enabled; 
        public override string Name => "Scp035";

        public override void Disable()
        {
            Handlers.Scp035Player = null;
            Handlers.Scp035Pickup = null;
            Enabled = false;
            AddLog("Scp035 disabled");
        }

        public override void Enable()
        {
            Singleton = this;
            Enabled = Config.GetBool("scp035_enabled", true);
            if (Enabled)
            {
                AddLog("Scp035 enabled");
                EventManager.RegisterHandler(this, new Handlers());
                AddCommand(new Scp035Command());
                AddCommand(new TpTo035Command());
            }
            else
            {
                AddLog("Scp035 loaded, but is disabled in config");
            }
        }

        public override void Reload()
        {
            Enabled = Config.GetBool("scp035_enabled", true);
            AddLog("Scp035 reloaded");
        }
    }
    public class Scp035Command : CommandHandler
    {
        public string Command => "Scp035";

        public string Usage => "Scp035 <player>";

        public string Aliases => "035";

        public string Execute(Player sender, string[] args)
        {
            if (args.Length<1)
            {
                return Usage;
            }
            else
            {
                Handlers.MakePlayer035(args[0].GetPlayer());
                return "Made "+args[0].GetPlayer().Nick+ " SCP-035";
            }
        }
    }
    public class TpTo035Command : CommandHandler
    {
        public string Command => "tpto035";
        public string Usage => "tpto035";
        public string Aliases => "tptoscp035 teleportto035";
        public string Execute(Player sender, string[] args)
        {
            if(Handlers.Scp035Pickup!=null)
            {
                sender.Position = Handlers.Scp035Pickup.position;
                return "Teleported to 035 pickup";
            }
            else if (Handlers.Scp035Player!=null)
            {
                sender.Position = Handlers.Scp035Player.Position;
                return "Teleported to 035 player";
            }
            else
            {
                return "Pickup or player not found";
            }
        }
    }
    public class Spawn035ItemCommand : CommandHandler
    {
        public string Command => "spawn035item";

        public string Usage => "spawn035item <player> <item>";

        public string Aliases => "spawn035 spawn035pickup";

        public string Execute(Player sender, string[] args)
        { 
            if (args.Length < 1)
            {
                Pickup pickup = Map.SpawnItem(Handlers.SCP035ItemType, sender.Position);
                Handlers.Scp035Pickup = pickup;
                return "Spawned 035 item at your position";
            }
            else if(args.Length==1)
            {
                Pickup pickup = Map.SpawnItem(Handlers.SCP035ItemType, args[0].GetPlayer().Position);
                Handlers.Scp035Pickup = pickup;
                return "Spawned 035 item at your position";
            }
            else
            {
                Pickup pickup = Map.SpawnItem(args[1].GetItem(), args[0].GetPlayer().Position);
                Handlers.SCP035ItemType = args[1].GetItem();
                Handlers.Scp035Pickup = pickup;
                return "Spawned 035 item at your position";
            }
        }
    }
}
