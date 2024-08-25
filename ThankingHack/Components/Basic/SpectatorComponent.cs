using SDG.Unturned;
using Thanking.Attributes;
using Thanking.Coroutines;
using Thanking.Options;
using Thanking.Utilities;
using Thanking.Variables;
using UnityEngine;

namespace Thanking.Components.Basic
{
    [Component]
    public class SpectatorComponent : MonoBehaviour
    {
        public void FixedUpdate()
        {
            if (!DrawUtilities.ShouldRun())
                return;
            
            if (MiscOptions.SpectatedPlayer != null && !PlayerCoroutines.IsSpying)
            {
                Player.player.look.IsControllingFreecam = true;

                Player.player.look.orbitPosition =
                    MiscOptions.SpectatedPlayer.transform.position -
                    Player.player.transform.position;
                
                Player.player.look.orbitPosition += new Vector3(0, 3, 0);
            }
            else
                Player.player.look.IsControllingFreecam = MiscOptions.Freecam;
        }
    }
}