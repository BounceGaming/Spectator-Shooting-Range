using System;
using Exiled.API.Features;
using ShootingRange.API;

namespace ShootingRange
{
    public class PluginMain : Plugin<Config>
    {
        public static PluginMain Singleton;
        public override string Author => "rayzer = Modified by BounceGaming-Team";
        public override string Name => "Spectator Shooting Range";
        public override Version Version => new Version(3, 0, 0);
        public EventHandlers EventHandler;
        public SpectatorRange ActiveRange;

        public override void OnEnabled()
        {
            Singleton = this;   
            EventHandler = new EventHandlers(this);
            Exiled.Events.Handlers.Player.Died += EventHandler.OnDied;
            Exiled.Events.Handlers.Player.Shooting += EventHandler.OnShooting;
            Exiled.Events.Handlers.Player.DroppingItem += EventHandler.OnDroppingItem;
            Exiled.Events.Handlers.Server.RoundStarted += EventHandler.OnRoundStarted;
            Exiled.Events.Handlers.Player.Hurting += EventHandler.OnHurting;
            Config.DeathBroadcast.Show = !Config.ForceSpectators;
            base.OnEnabled();
        }
        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Player.Hurting -= EventHandler.OnHurting;
            Exiled.Events.Handlers.Server.RoundStarted -= EventHandler.OnRoundStarted;
            Exiled.Events.Handlers.Player.Died -= EventHandler.OnDied;
            Exiled.Events.Handlers.Player.Shooting -= EventHandler.OnShooting;
            Exiled.Events.Handlers.Player.DroppingItem -= EventHandler.OnDroppingItem;
            EventHandler = null;
            Singleton = null;
            base.OnDisabled();
        }
    }
}
