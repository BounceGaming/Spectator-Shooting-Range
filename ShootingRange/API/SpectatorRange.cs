using System.Collections.Generic;
using AdminToys;
using UnityEngine;
using MEC;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Toys;
using InventorySystem.Items.Firearms.Attachments;
using Mirror;
using Utf8Json.Internal.DoubleConversion;
using Light = Exiled.API.Features.Toys.Light;

namespace ShootingRange.API
{
    public class SpectatorRange
    {
        private Vector3 _smallBound = new Vector3(205, 997, -54);
        private Vector3 _bigBound = new Vector3(237, 1015, -37);
        public Vector3 Spawn { get; } = new Vector3(218.5f, 999.1f, -43.0f);
        public bool IsOpen => Round.IsStarted && Respawn.TimeUntilRespawn > 20;
        public SpectatorRange() { }
        public SpectatorRange(Vector4 v4)
        {
            Spawn = new Vector3(v4.x, v4.y, v4.z);
            Vector3 offset = new Vector3(v4.w, v4.w, v4.w);

            _smallBound = Spawn - offset;
            _bigBound = Spawn + offset;
        }

        public List<int> RangePlayers = new List<int>();

        public bool HasPlayer(Player plyr)
        {
            if(plyr.Role.Type != RoleType.Tutorial)
                return false;
            for (int i = 0; i < 3; i++)
            {
                float pos = plyr.Position[i];
                if (pos > _bigBound[i] || pos < _smallBound[i])
                    return false;
            }
            return true;
        }

        public IEnumerator<float> AntiExitCoroutine()
        {
            while (true)
            {
                foreach (var s in RangePlayers)
                {
                    var p = Player.Get(s);
                    if (p != null && RangePlayers.Contains(p.Id) && !HasPlayer(p))
                    {
                        p.Position = Spawn;
                        p.Broadcast(6, "<i>¿Donde pensabas ir pillín?</i>", Broadcast.BroadcastFlags.Normal, true);
                    }
                }
                yield return Timing.WaitForSeconds(1f);
            }
        }

        public bool DeletePlayer(Player player)
        {
            if (!RangePlayers.Contains(player.Id))
                return false;
            RangePlayers.Remove(player.Id);
            player.ClearInventory();
            player.SetRole(RoleType.Spectator);
            return true;
        }
        
        public bool TryAdmit(Player player)
        {
            if (!(IsOpen && player.IsDead && !RangePlayers.Contains(player.Id) && !PluginMain.Singleton.EventHandler.FreshlyDead.Contains(player)))
                return false;

            player.SetRole(RoleType.Tutorial);
            player.Broadcast(PluginMain.Singleton.Config.RangeGreeting);
            RangePlayers.Add(player.Id);
            PluginMain.Singleton.CoroutineHandles.Add(Timing.CallDelayed(0.5f, () =>
            {
                player.Position = Spawn;
                player.AddItem(PluginMain.Singleton.Config.RangerInventory);
                player.Health = 10000;
            }));
            return true;
        }
        public void SpawnTargets()
        {
            int absZOffset = PluginMain.Singleton.Config.AbsoluteTargetDistance;
            int relZOffset = PluginMain.Singleton.Config.RelativeTargetDistance;
            float centerX = (_bigBound.x + _smallBound.x) / 2;
            Vector3 rot = new Vector3(0, 90, 0);
            Target[] targets = new Target[9];

            for (int i = 0; i < 3; i++)
            {
                float xOffset = 2.5f * (i + 1);
                float z = _smallBound.z - absZOffset - relZOffset * i;

                var pos1 = new Vector3(_bigBound.x - xOffset, _smallBound.y, z);
                var pos2 = new Vector3(centerX - xOffset, _smallBound.y, z);
                var pos3 = new Vector3(_smallBound.x + 10 - xOffset, _smallBound.y, z);
                
                targets[i * 3] = new Target(ShootingTargetToy.Create(ShootingTargetType.Sport, pos1, rot), Light.Create(pos1 + Vector3.forward * 1f, rot));
                targets[1 + i * 3] = new Target(ShootingTargetToy.Create(ShootingTargetType.ClassD, pos2, rot), Light.Create(pos2 + Vector3.forward * 1f, rot));
                targets[2 + i * 3] = new Target(ShootingTargetToy.Create(ShootingTargetType.Binary, pos3, rot), Light.Create(pos3 + Vector3.forward * 1f, rot));
            }
            
            GameObject bench = Object.Instantiate(NetworkManager.singleton.spawnPrefabs.Find(p => p.gameObject.name == "Work Station"));
            bench.transform.localPosition = new Vector3(205.37f,996.8f,-50.152f);
            bench.transform.Rotate(0,90,0);
            bench.AddComponent<WorkstationController>();
            NetworkServer.Spawn(bench);

            PluginMain.Singleton.CoroutineHandles.Add(Timing.RunCoroutine(_movePrimitives(targets[1],215f, 231f)));
            PluginMain.Singleton.CoroutineHandles.Add(Timing.RunCoroutine(_movePrimitives(targets[4],212f, 230f)));
            PluginMain.Singleton.CoroutineHandles.Add(Timing.RunCoroutine(_movePrimitives(targets[7],210f, 225f)));

            //0 rotation = towards gate a
            //+1 rotation to turn clockwise 90
            //for targets at least
            //x smaller as going to A
            //z bigger going to escape
        }
        public void SpawnPrimitives()
        {
            const float thick = 0.1f;
            const float frontHeight = 1.75f;
            Color color = Color.clear;
            Vector3 dif = _bigBound - _smallBound;
            Vector3 center = (_bigBound + _smallBound) / 2;
            Primitive[] primitives = new Primitive[5];

            primitives[0] = Primitive.Create(new Vector3(center.x, center.y, _bigBound.z), null, new Vector3(dif.x, dif.y, thick));
            primitives[1] = Primitive.Create(new Vector3(center.x, _smallBound.y + frontHeight / 2, _smallBound.z), null, new Vector3(dif.x, frontHeight, thick));
            primitives[2] = Primitive.Create(new Vector3(_bigBound.x, center.y, center.z), null, new Vector3(thick, dif.y, dif.z));
            primitives[3] = Primitive.Create(primitives[2].Position - new Vector3(dif.x, 0, 0), null, primitives[2].Scale);
            primitives[4] = Primitive.Create(new Vector3(center.x, _smallBound.y, center.z), null, new Vector3(dif.x, thick, dif.z));

            for (int i = 0; i < 5; i++)
            {
                primitives[i].Base.NetworkScale = primitives[i].Scale;
                primitives[i].Color = color;
                primitives[i].Type = PrimitiveType.Cube;
            }
        }
        
        public void RemovePlayers()
        {
            foreach (var s in RangePlayers)
            {
                var plyr = Player.Get(s);
                plyr?.ClearInventory();
                plyr?.SetRole(RoleType.Spectator);
                plyr?.Broadcast(PluginMain.Singleton.Config.RespawnBroadcast, true);
            }
            RangePlayers.Clear();
        }

        private static IEnumerator<float> _movePrimitives(Target target, float limitRight, float limitLeft)
        {
            target.TargetToy.Base.NetworkMovementSmoothing = 60;
            target.LightToy.Base.NetworkMovementSmoothing = 60;
            bool leftToRight = true;
            while (true)
            {
                yield return Timing.WaitForSeconds(0.05f);
                if (leftToRight)
                {
                    target.TargetToy.Base.transform.position += Vector3.right * 0.1f;
                    target.LightToy.Base.transform.position += Vector3.right * 0.1f;
                }
                else
                {
                    target.TargetToy.Base.transform.position += Vector3.left * 0.1f;
                    target.LightToy.Base.transform.position += Vector3.left * 0.1f;
                }

                if (target.TargetToy.Base.transform.position.x < limitRight)
                    leftToRight = true;

                else if (target.TargetToy.Base.transform.position.x > limitLeft)
                    leftToRight = false;
                else if (Random.Range(0, 100) < 2)
                    leftToRight = !leftToRight;
            } 
        }
    }
}
