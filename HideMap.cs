using UnityEngine;
using System.Collections.Generic;
using CompanionServer.Handlers;
using System.Linq;
using System.Collections;

namespace Oxide.Plugins
{
    [Info("HideMap", "bmgjet", "1.0.0")]
    [Description("Blanks out the map")]

    public class HideMap : RustPlugin
    {
        const string PermHidemap = "HideMap.enabled";
        private Coroutine _routine;
        public List<MapMarkerGenericRadius> markers = new List<MapMarkerGenericRadius>();

        private void Init()
        {
            permission.RegisterPermission(PermHidemap, this);
        }

        void OnServerInitialized()
        {
            if (BasePlayer.activePlayerList.Count != 0 && _routine == null)
            {
                RunMapBlank();
            }
        }

        private void OnPlayerConnected(BasePlayer player)
        {
            if (_routine == null)
            {
                RunMapBlank();
            }
        }

        void Unload()
        {
            MarkerDisplayingDelete(null, null, null);
            if (_routine != null)
            {
                ServerMgr.Instance.StopCoroutine(_routine);
            }
        }

        IEnumerator BlankRoutine()
        {
            do
            {
                List<MapMarker> m = BaseNetworkable.serverEntities.OfType<MapMarker>().ToList();
                if (m.Count > 1)
                {
                    foreach (var mm in m)
                    {
                        if (mm != null)
                        {
                            if (!mm.IsDestroyed)
                            {
                                mm.Kill();
                                mm.SendNetworkUpdateImmediate();
                            }
                        }
                    }
                }
                DrawMap();
                yield return CoroutineEx.waitForSeconds(2f);
            } while (true);
            Puts("BlankMap Thread Stopped!");
        }

        void MarkerDisplayingDelete(BasePlayer player, string command, string[] args)
        {
            foreach (var m in markers)
            {
                if (m != null)
                {
                    m.Kill();
                    m.SendUpdate();
                }
            }
            markers.Clear();
        }

        object CanNetworkTo(MapMarkerGenericRadius marker, BasePlayer player)
        {
            if (!markers.Contains(marker) || (!player.IPlayer.HasPermission(PermHidemap) && markers.Contains(marker)))
            {
                return null;
            }
            return false;
        }

        void RunMapBlank()
        {
            _routine = ServerMgr.Instance.StartCoroutine(BlankRoutine());
            Puts("BlankMap Thread Started!");
        }

        void DrawMap()
        {
            try
            {
                MarkerDisplayingDelete(null, null, null);
            }
            catch { }
            MapMarkerGenericRadius MapMarkerCustom; 
                MapMarkerCustom = GameManager.server.CreateEntity("assets/prefabs/tools/map/genericradiusmarker.prefab", new Vector3(0,0,0)) as MapMarkerGenericRadius;
                MapMarkerCustom.alpha = 1.0f;
                MapMarkerCustom.color1 = Color.black;
                MapMarkerCustom.color2 = Color.black;
                MapMarkerCustom.radius = 100;
                markers.Add(MapMarkerCustom);
            foreach (var m in markers)
            {
                try
                {
                    m.Spawn();
                    MapMarker.serverMapMarkers.Remove(m);
                    m.SendUpdate();
                }
                catch { }
            }               
        }
    }
}