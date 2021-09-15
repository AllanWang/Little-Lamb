using System;
using MLAPI;
using MLAPI.NetworkVariable;
using UnityEngine;

namespace LittleLamb
{
    /// <summary>
    /// MonoBehaviour containing only one NetworkVariable of type LifeState which represents this object's life state.
    /// </summary>
    public class NetworkLifeState : NetworkBehaviour
    {
        [SerializeField]
        NetworkVariable<LifeState> m_LifeState = new NetworkVariable<LifeState>(LittleLamb.LifeState.Alive);

        public NetworkVariable<LifeState> LifeState => m_LifeState;
    }
}
