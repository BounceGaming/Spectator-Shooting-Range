using System.Collections.Generic;
using MEC;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Exiled.API.Features.Items;
using ShootingRange.API;

namespace ShootingRange 
{
    public class EventHandlers
    {
        private readonly PluginMain _plugin;
        public List<Player> FreshlyDead { get; } = new List<Player>();

        public EventHandlers(PluginMain plugin)
        {
            _plugin = plugin;
        }

        public void OnRoundStarted()
        {
            _plugin.ActiveRange?.RangePlayers?.Clear();
            SpectatorRange range = _plugin.Config.UseRangeLocation ?  new SpectatorRange(_plugin.Config.RangeLocation) : new SpectatorRange();
            range.SpawnTargets();

            if (_plugin.Config.UsePrimitives)
                range.SpawnPrimitives();

            _plugin.ActiveRange = range;
            
            PluginMain.Singleton.CoroutineHandles.Add(Timing.RunCoroutine(WaitForRespawnCoroutine()));
            PluginMain.Singleton.CoroutineHandles.Add(Timing.RunCoroutine(_plugin.ActiveRange.AntiExitCoroutine()));
        }
        
        public void OnDied(DiedEventArgs ev) => 
            PluginMain.Singleton.CoroutineHandles.Add(Timing.RunCoroutine(OnDiedCoroutine(ev.Target, ev.Killer != null && ev.Killer.Role.Type == RoleType.Scp049)));
        
        private IEnumerator<float> OnDiedCoroutine(Player plyr, bool byDoctor)
        {
            if (byDoctor)
            {
                FreshlyDead.Add(plyr);
                yield return Timing.WaitForSeconds(10f);
                FreshlyDead.Remove(plyr);
            }

            if (_plugin.Config.ForceSpectators)
            {
                yield return Timing.WaitForSeconds(0.5f);
                _plugin.ActiveRange.TryAdmit(plyr);
            }
            else
            {
                yield return Timing.WaitForSeconds(5f);
                plyr.Broadcast(_plugin.Config.DeathBroadcast);
            }
        }
        public void OnShooting(ShootingEventArgs ev)
        {
            if (_plugin.ActiveRange.HasPlayer(ev.Shooter))
            {
                Firearm gun = (Firearm)ev.Shooter.CurrentItem;
                gun.Ammo = gun.MaxAmmo;
            }
        }

        public void OnDroppingItem(DroppingItemEventArgs ev)
        {
            if (_plugin.ActiveRange.RangePlayers.Contains(ev.Player.Id))
                ev.IsAllowed = false;
        }
        
        private IEnumerator<float> WaitForRespawnCoroutine()
        {
            for (;;)
            {
                if (!Round.IsStarted)
                    break;
                if (Respawn.TimeUntilRespawn < 20)
                    _plugin.ActiveRange.RemovePlayers();

                yield return Timing.WaitForSeconds(2f);
            }
        }

        public void OnHurting(HurtingEventArgs ev)
        {
            if(!ev.IsAllowed || ev.Target == null || ev.Attacker == null)
                return;
            if(_plugin.ActiveRange.RangePlayers.Contains(ev.Attacker.Id) || _plugin.ActiveRange.RangePlayers.Contains(ev.Target.Id))
            {
                ev.Amount = 0f;
                ev.IsAllowed = false;
            }
        }

        public void OnRoundEnded(RoundEndedEventArgs ev)
        {
            foreach (var coroutine in PluginMain.Singleton.CoroutineHandles)
                Timing.KillCoroutines(coroutine);
        }
    }
}
