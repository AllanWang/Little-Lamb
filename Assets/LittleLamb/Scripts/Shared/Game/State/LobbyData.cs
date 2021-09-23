
using MLAPI;
using MLAPI.Messaging;
using MLAPI.Serialization;
using MLAPI.NetworkVariable;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MLAPI.NetworkVariable.Collections;
using UnityEngine;

namespace LittleLamb
{
  /// <summary>
  /// Common data and RPCs for the CharSelect stage.
  /// </summary>
  public class LobbyData : NetworkBehaviour
  {
    public enum FatalLobbyError
    {
      LobbyFull,
    }

    public const int k_MaxLobbyPlayers = 8;

    /// <summary>
    /// Describes one of players in the lobby, and their current character-select status.
    /// </summary>
    public struct LobbyPlayerState : INetworkSerializable
    {
      public ulong ClientId;
      public string PlayerName;
      public int PlayerNum; // this player's assigned "P#". (0=P1, 1=P2, etc.)
      public float LastChangeTime;

      public LobbyPlayerState(ulong clientId, string name, int playerNum, float lastChangeTime = 0)
      {
        ClientId = clientId;
        PlayerName = name;
        PlayerNum = playerNum;
        LastChangeTime = lastChangeTime;
      }
      public void NetworkSerialize(NetworkSerializer serializer)
      {
        serializer.Serialize(ref ClientId);
        serializer.Serialize(ref PlayerName);
        serializer.Serialize(ref PlayerNum);
        serializer.Serialize(ref LastChangeTime);
      }
    }

    private NetworkList<LobbyPlayerState> m_LobbyPlayers;

    private void Awake()
    {
      m_LobbyPlayers = new NetworkList<LobbyPlayerState>();
    }

    /// <summary>
    /// Current state of all players in the lobby.
    /// </summary>
    public NetworkList<LobbyPlayerState> LobbyPlayers { get { return m_LobbyPlayers; } }

    /// <summary>
    /// When this becomes true, the lobby is closed and in process of terminating (switching to gameplay).
    /// </summary>
    public MLAPI.NetworkVariable.NetworkVariableBool IsLobbyClosed { get; } = new MLAPI.NetworkVariable.NetworkVariableBool(false);

    /// <summary>
    /// Client notification when the server has assigned this client a player Index (from 0 to 7);
    /// UI uses this tell whether we are "P1", "P2", etc. in the char-select UI
    /// </summary>
    public event Action<int> OnAssignedPlayerNumber;

    /// <summary>
    /// RPC to tell a client which slot in the char-gen screen they will be using.
    /// </summary>
    /// <param name="idx">Index on the UI screen, starting at 0 for the first slot</param>
    [ClientRpc]
    public void AssignPlayerNumberClientRpc(int idx)
    {
      OnAssignedPlayerNumber?.Invoke(idx);
    }

    /// <summary>
    /// Client notification when the server has told us that we cannot participate.
    /// (Client should display an appropriate error and terminate)
    /// </summary>
    public event Action<FatalLobbyError> OnFatalLobbyError;

    /// <summary>
    /// RPC to tell a client that they cannot participate in the game due to a fatal error.
    /// </summary>
    [ClientRpc]
    public void FatalLobbyErrorClientRpc(FatalLobbyError error, ClientRpcParams clientParams = default)
    {
      OnFatalLobbyError?.Invoke(error);
    }

    public event Action OnCloseLobby;

    [ServerRpc]
    public void CloseLobbyServerRpc()
    {
      OnCloseLobby?.Invoke();
    }
  }
}
