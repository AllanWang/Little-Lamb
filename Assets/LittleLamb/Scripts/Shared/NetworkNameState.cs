using MLAPI;
using MLAPI.NetworkVariable;
using UnityEngine;

namespace LittleLamb
{
    /// <summary>
    /// NetworkBehaviour containing only one NetworkVariableString which represents this object's name.
    /// </summary>
    public class NetworkNameState : NetworkBehaviour
    {
        [HideInInspector]
        public NetworkVariableString Name = new NetworkVariableString();
    }
}
