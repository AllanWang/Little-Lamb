using UnityEngine;

namespace LittleLamb.Client
{
    // [RequireComponent(typeof(LittleLamb.NetworkCharacterState))]
    public class ClientCharacter : MLAPI.NetworkBehaviour
    {
        /// <summary>
        /// The Vizualization GameObject isn't in the same transform hierarchy as the object, but it registers itself here
        /// so that the visual GameObject can be found from a NetworkObjectId.
        /// </summary>
        public LittleLamb.Visual.ClientCharacterVisualization ChildVizObject { get; set; }

        public override void NetworkStart()
        {
            if (!IsClient) { this.enabled = false; }
        }

    }

}
