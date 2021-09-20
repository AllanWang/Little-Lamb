using MLAPI;
using MLAPI.Messaging;
using System.Collections;
using UnityEngine;

namespace LittleLamb.Server
{
  /// <summary>
  /// Server specialization of Character Select game state.
  /// </summary>
  [RequireComponent(typeof(LobbyData))]
  public class ServerLobbyState : GameStateBehaviour
  {
    public override GameState ActiveState { get { return GameState.Lobby; } }
    public LobbyData LobbyData { get; private set; }

    private ServerGameNetPortal m_ServerNetPortal;

    private void Awake()
    {
      LobbyData = GetComponent<LobbyData>();
      m_ServerNetPortal = GameObject.FindGameObjectWithTag("GameNetPortal").GetComponent<ServerGameNetPortal>();
    }

    private void OnClientChangedSeat(ulong clientId)
    {
      int idx = FindLobbyPlayerIdx(clientId);
      if (idx == -1)
      {
        //TODO-FIXME:MLAPI See note about MLAPI issue 745 in WaitToSeatNowPlayer.
        //while this workaround is in place, we must simply ignore these update requests from the client.
        //throw new System.Exception($"OnClientChangedSeat: client ID {clientId} is not a lobby player and cannot change seats!");
        return;
      }


      if (LobbyData.IsLobbyClosed.Value)
      {
        // The user tried to change their class after everything was locked in... too late! Discard this choice
        return;
      }

      LobbyData.LobbyPlayers[idx] = new LobbyData.LobbyPlayerState(clientId,
          LobbyData.LobbyPlayers[idx].PlayerName,
          LobbyData.LobbyPlayers[idx].PlayerNum,
          Time.time);
    }

    /// <summary>
    /// Returns the index of a client in the master LobbyPlayer list, or -1 if not found
    /// </summary>
    private int FindLobbyPlayerIdx(ulong clientId)
    {
      for (int i = 0; i < LobbyData.LobbyPlayers.Count; ++i)
      {
        if (LobbyData.LobbyPlayers[i].ClientId == clientId)
          return i;
      }
      return -1;
    }

    /// <summary>
    /// Looks through all our connections and sees if everyone has locked in their choice;
    /// if so, we lock in the whole lobby, save state, and begin the transition to gameplay
    /// </summary>
    private void OnCloseLobby()
    {
      // everybody's ready at the same time! Lock it down!
      LobbyData.IsLobbyClosed.Value = true;

      // remember our choices so the next scene can use the info
      SaveLobbyResults();

      // Delay a few seconds to give the UI time to react, then switch scenes
      StartCoroutine(WaitToEndLobby());
    }

    private void SaveLobbyResults()
    {
      LobbyResults lobbyResults = new LobbyResults();
      foreach (LobbyData.LobbyPlayerState playerInfo in LobbyData.LobbyPlayers)
      {
        lobbyResults.Choices[playerInfo.ClientId] = new LobbyResults.CharSelectChoice(playerInfo.PlayerNum);
      }
      GameStateRelay.SetRelayObject(lobbyResults);
    }

    private IEnumerator WaitToEndLobby()
    {
      yield return new WaitForSeconds(3);
      MLAPI.SceneManagement.NetworkSceneManager.SwitchScene("GameRoom");
    }

    protected override void OnDestroy()
    {
      base.OnDestroy();
      if (NetworkManager.Singleton)
      {
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
      }
      if (LobbyData)
      {
        LobbyData.OnClientChangedSeat -= OnClientChangedSeat;
        LobbyData.OnCloseLobby -= OnCloseLobby;
      }
    }

    public override void NetworkStart()
    {
      base.NetworkStart();
      if (!IsServer)
      {
        enabled = false;
      }
      else
      {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
        LobbyData.OnClientChangedSeat += OnClientChangedSeat;
        LobbyData.OnCloseLobby += OnCloseLobby;

        if (IsHost)
        {
          // host doesn't get an OnClientConnected()
          // and other clients could be connects from last game
          // So look for any existing connections to do intiial setup
          var clients = NetworkManager.Singleton.ConnectedClientsList;
          foreach (var net_cl in clients)
          {
            OnClientConnected(net_cl.ClientId);
          }
        }
      }
    }

    private void OnClientConnected(ulong clientId)
    {
      StartCoroutine(WaitToSeatNewPlayer(clientId));
    }

    private IEnumerator WaitToSeatNewPlayer(ulong clientId)
    {
      //TODO-FIXME:MLAPI We are receiving NetworkVar updates too early on the client when doing this immediately on client connection,
      //causing the NetworkList of lobby players to get out of sync.
      //tracking MLAPI issue: https://github.com/Unity-Technologies/com.unity.multiplayer.mlapi/issues/745
      //When issue is resolved, we should be able to call SeatNewPlayer directly in the client connection callback. 
      yield return new WaitForSeconds(2.5f);
      SeatNewPlayer(clientId);
    }

    private int GetAvailablePlayerNum()
    {
      for (int possiblePlayerNum = 0; possiblePlayerNum < LobbyData.k_MaxLobbyPlayers; ++possiblePlayerNum)
      {
        bool found = false;
        foreach (LobbyData.LobbyPlayerState playerState in LobbyData.LobbyPlayers)
        {
          if (playerState.PlayerNum == possiblePlayerNum)
          {
            found = true;
            break;
          }
        }
        if (!found)
        {
          return possiblePlayerNum;
        }
      }
      // we couldn't get a Player# for this person... which means the lobby is full!
      return -1;
    }

    private void SeatNewPlayer(ulong clientId)
    {
      int playerNum = GetAvailablePlayerNum();
      if (playerNum == -1)
      {
        // we ran out of seats... there was no room!
        LobbyData.FatalLobbyErrorClientRpc(LobbyData.FatalLobbyError.LobbyFull,
            new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { clientId } } });
        return;
      }

      string playerName = m_ServerNetPortal.GetPlayerName(clientId, playerNum);
      LobbyData.LobbyPlayers.Add(new LobbyData.LobbyPlayerState(clientId, playerName, playerNum));
    }

    private void OnClientDisconnectCallback(ulong clientId)
    {
      // clear this client's PlayerNumber and any associated visuals (so other players know they're gone).
      for (int i = 0; i < LobbyData.LobbyPlayers.Count; ++i)
      {
        if (LobbyData.LobbyPlayers[i].ClientId == clientId)
        {
          LobbyData.LobbyPlayers.RemoveAt(i);
          break;
        }
      }
    }
  }
}
