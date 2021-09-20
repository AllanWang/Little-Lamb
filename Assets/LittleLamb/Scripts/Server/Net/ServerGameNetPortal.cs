using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI.SceneManagement;

namespace LittleLamb.Server
{
  /// <summary>
  /// Represents a single player on the game server
  /// </summary>
  public struct PlayerData
  {
    public string playerName;  //name of the player
    public ulong clientId; //the identifying id of the client

    public PlayerData(string playerName, ulong clientId)
    {
      this.playerName = playerName;
      this.clientId = clientId;
    }
  }

  /// <summary>
  /// Server logic plugin for the GameNetHub. Contains implementations for all GameNetHub's C2S RPCs.
  /// </summary>
  [RequireComponent(typeof(GameNetPortal))]
  public class ServerGameNetPortal : MonoBehaviour
  {
    private GameNetPortal portal;

    /// <summary>
    /// Maps a given client guid to the data for a given client player.
    /// </summary>
    private Dictionary<string, PlayerData> clientData;

    /// <summary>
    /// Map to allow us to cheaply map from guid to player data.
    /// </summary>
    private Dictionary<ulong, string> clientIdToGuid;

    // used in ApprovalCheck. This is intended as a bit of light protection against DOS attacks that rely on sending silly big buffers of garbage.
    private const int k_MaxConnectPayload = 1024;

    /// <summary>
    /// Keeps a list of what clients are in what scenes.
    /// </summary>
    private Dictionary<ulong, int> clientSceneMap = new Dictionary<ulong, int>();

    /// <summary>
    /// The active server scene index.
    /// </summary>
    public int ServerScene { get { return UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex; } }

    void Start()
    {
      portal = GetComponent<GameNetPortal>();
      portal.NetworkReadied += OnNetworkReady;

      // we add ApprovalCheck callback BEFORE NetworkStart to avoid spurious MLAPI warning:
      // "No ConnectionApproval callback defined. Connection approval will timeout"
      portal.NetManager.ConnectionApprovalCallback += ApprovalCheck;
      portal.NetManager.OnServerStarted += ServerStartedHandler;
      clientData = new Dictionary<string, PlayerData>();
      clientIdToGuid = new Dictionary<ulong, string>();
    }

    void OnDestroy()
    {
      if (portal != null)
      {
        portal.NetworkReadied -= OnNetworkReady;

        if (portal.NetManager != null)
        {
          portal.NetManager.ConnectionApprovalCallback -= ApprovalCheck;
          portal.NetManager.OnServerStarted -= ServerStartedHandler;
        }
      }
    }

    private void OnNetworkReady()
    {
      if (!portal.NetManager.IsServer)
      {
        enabled = false;
      }
      else
      {
        //O__O if adding any event registrations here, please add an unregistration in OnClientDisconnect.
        portal.UserDisconnectRequested += OnUserDisconnectRequest;
        portal.NetManager.OnClientDisconnectCallback += OnClientDisconnect;
        portal.ClientSceneChanged += OnClientSceneChanged;

        // TODO remove char selection?
        //The "BossRoom" server always advances to CharSelect immediately on start. Different games
        //may do this differently.
        NetworkSceneManager.SwitchScene("Lobby");

        if (portal.NetManager.IsHost)
        {
          clientSceneMap[portal.NetManager.LocalClientId] = ServerScene;
        }
      }
    }

    /// <summary>
    /// Handles the case where NetworkManager has told us a client has disconnected. This includes ourselves, if we're the host,
    /// and the server is stopped."
    /// </summary>
    private void OnClientDisconnect(ulong clientId)
    {
      clientSceneMap.Remove(clientId);
      if (clientIdToGuid.TryGetValue(clientId, out var guid))
      {
        clientIdToGuid.Remove(clientId);

        if (clientData[guid].clientId == clientId)
        {
          //be careful to only remove the ClientData if it is associated with THIS clientId; in a case where a new connection
          //for the same GUID kicks the old connection, this could get complicated. In a game that fully supported the reconnect flow,
          //we would NOT remove ClientData here, but instead time it out after a certain period, since the whole point of it is
          //to remember client information on a per-guid basis after the connection has been lost.
          clientData.Remove(guid);
        }
      }

      if (clientId == portal.NetManager.LocalClientId)
      {
        //the ServerGameNetPortal may be initialized again, which will cause its NetworkStart to be called again.
        //Consequently we need to unregister anything we registered, when the NetworkManager is shutting down.
        portal.UserDisconnectRequested -= OnUserDisconnectRequest;
        portal.NetManager.OnClientDisconnectCallback -= OnClientDisconnect;
        portal.ClientSceneChanged -= OnClientSceneChanged;
      }
    }

    private void OnClientSceneChanged(ulong clientId, int sceneIndex)
    {
      clientSceneMap[clientId] = sceneIndex;
    }

    /// <summary>
    /// Handles the flow when a user has requested a disconnect via UI (which can be invoked on the Host, and thus must be
    /// handled in server code). 
    /// </summary>
    private void OnUserDisconnectRequest()
    {
      if (portal.NetManager.IsServer)
      {
        portal.NetManager.StopServer();
      }

      Clear();
    }

    private void Clear()
    {
      //resets all our runtime state.
      clientData.Clear();
      clientIdToGuid.Clear();
      clientSceneMap.Clear();
    }

    /// <summary>
    /// </summary>
    /// <param name="clientId"> guid of the client whose data is requested</param>
    /// <returns>Player data struct matching the given ID</returns>
    public bool AreAllClientsInServerScene()
    {
      foreach (var kvp in clientSceneMap)
      {
        if (kvp.Value != ServerScene) { return false; }
      }

      return true;
    }

    /// <summary>
    /// Returns true if the given client is currently in the server scene.
    /// </summary>
    /// <param name="clientId"></param>
    /// <returns></returns>
    public bool IsClientInServerScene(ulong clientId)
    {
      return clientSceneMap.TryGetValue(clientId, out int clientScene) && clientScene == ServerScene;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="clientId"> guid of the client whose data is requested</param>
    /// <returns>Player data struct matching the given ID</returns>
    public PlayerData? GetPlayerData(ulong clientId)
    {
      //First see if we have a guid matching the clientID given.

      if (clientIdToGuid.TryGetValue(clientId, out string clientguid))
      {
        if (clientData.TryGetValue(clientguid, out PlayerData data))
        {
          return data;
        }
        else
        {
          Debug.Log("No PlayerData of matching guid found");
        }
      }
      else
      {
        Debug.Log("No client guid found mapped to the given client ID");
      }
      return null;
    }

    /// <summary>
    /// Convenience method to get player name from player data
    /// Returns name in data or default name using playerNum
    /// </summary>
    public string GetPlayerName(ulong clientId, int playerNum)
    {
      var playerData = GetPlayerData(clientId);
      return (playerData != null) ? playerData.Value.playerName : ("Player" + playerNum);
    }


    /// <summary>
    /// This logic plugs into the "ConnectionApprovalCallback" exposed by MLAPI.NetworkManager, and is run every time a client connects to us.
    /// See GNH_Client.StartClient for the complementary logic that runs when the client starts its connection.
    /// </summary>
    /// <remarks>
    /// Since our game doesn't have to interact with some third party authentication service to validate the identity of the new connection, our ApprovalCheck
    /// method is simple, and runs synchronously, invoking "callback" to signal approval at the end of the method. MLAPI currently doesn't support the ability
    /// to send back more than a "true/false", which means we have to work a little harder to provide a useful error return to the client. To do that, we invoke a
    /// client RPC in the same channel that MLAPI uses for its connection callback. Since that channel ("MLAPI_INTERNAL") is both reliable and sequenced, we can be
    /// confident that our login result message will execute before any disconnect message.
    /// </remarks>
    /// <param name="connectionData">binary data passed into StartClient. In our case this is the client's GUID, which is a unique identifier for their install of the game that persists across app restarts. </param>
    /// <param name="clientId">This is the clientId that MLAPI assigned us on login. It does not persist across multiple logins from the same client. </param>
    /// <param name="callback">The delegate we must invoke to signal that the connection was approved or not. </param>
    private void ApprovalCheck(byte[] connectionData, ulong clientId, MLAPI.NetworkManager.ConnectionApprovedDelegate callback)
    {
      if (connectionData.Length > k_MaxConnectPayload)
      {
        callback(false, 0, false, null, null);
        return;
      }

      string payload = System.Text.Encoding.UTF8.GetString(connectionData);
      var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload); // https://docs.unity3d.com/2020.2/Documentation/Manual/JSONSerialization.html
      int clientScene = connectionPayload.clientScene;

      //a nice addition in the future will be to support rejoining the game and getting your same character back. This will require tracking a map of the GUID
      //to the player's owned character object, and cleaning that object on a timer, rather than doing so immediately when a connection is lost. 
      Debug.Log("Host ApprovalCheck: connecting client GUID: " + connectionPayload.clientGUID);

      //TODO: GOMPS-78. We are saving the GUID, but we have more to do to fully support a reconnect flow (where you get your same character back after disconnect/reconnect).

      ConnectStatus gameReturnStatus = ConnectStatus.Success;

      //Test for Duplicate Login. 
      if (clientData.ContainsKey(connectionPayload.clientGUID))
      {
        if (Debug.isDebugBuild)
        {
          Debug.Log($"Client GUID {connectionPayload.clientGUID} already exists. Because this is a debug build, we will still accept the connection");
          while (clientData.ContainsKey(connectionPayload.clientGUID)) { connectionPayload.clientGUID += "_Secondary"; }
        }
        else
        {
          ulong oldClientId = clientData[connectionPayload.clientGUID].clientId;
          StartCoroutine(WaitToDisconnectClient(oldClientId, ConnectStatus.LoggedInAgain));
        }
      }

      //Test for over-capacity Login.
      // if (clientData.Count >= CharSelectData.k_MaxLobbyPlayers)
      // {
      //   gameReturnStatus = ConnectStatus.ServerFull;
      // }

      //Populate our dictionaries with the playerData
      if (gameReturnStatus == ConnectStatus.Success)
      {
        clientSceneMap[clientId] = clientScene;
        clientIdToGuid[clientId] = connectionPayload.clientGUID;
        clientData[connectionPayload.clientGUID] = new PlayerData(connectionPayload.playerName, clientId);
      }

      callback(false, 0, true, null, null);

      //TODO:MLAPI: this must be done after the callback for now. In the future we expect MLAPI to allow us to return more information as part of
      //the approval callback, so that we can provide more context on a reject. In the meantime we must provide the extra information ourselves,
      //and then manually close down the connection.
      portal.ServerToClientConnectResult(clientId, gameReturnStatus);
      if (gameReturnStatus != ConnectStatus.Success)
      {
        //TODO-FIXME:MLAPI Issue #796. We should be able to send a reason and disconnect without a coroutine delay.
        StartCoroutine(WaitToDisconnectClient(clientId, gameReturnStatus));
      }
    }

    private IEnumerator WaitToDisconnectClient(ulong clientId, ConnectStatus reason)
    {
      portal.ServerToClientSetDisconnectReason(clientId, reason);

      // TODO fix once this is solved: Issue 796 Unity-Technologies/com.unity.multiplayer.mlapi#796
      // this wait is a workaround to give the client time to receive the above RPC before closing the connection
      yield return new WaitForSeconds(0);

      BootClient(clientId);
    }

    /// <summary>
    /// This method will summarily remove a player connection, as well as its controlled object.
    /// </summary>
    /// <param name="clientId">the ID of the client to boot.</param>
    public void BootClient(ulong clientId)
    {
      var netObj = MLAPI.Spawning.NetworkSpawnManager.GetPlayerNetworkObject(clientId);
      if (netObj)
      {
        //TODO-FIXME:MLAPI Issue #795. Should not need to explicitly despawn player objects.
        netObj.Despawn(true);
      }
      portal.NetManager.DisconnectClient(clientId);
    }

    /// <summary>
    /// Called after the server is created-  This is primarily meant for the host server to clean up or handle/set state as its starting up
    /// </summary>
    private void ServerStartedHandler()
    {
      clientData.Add("host_guid", new PlayerData(portal.PlayerName, portal.NetManager.LocalClientId));
      clientIdToGuid.Add(portal.NetManager.LocalClientId, "host_guid");
    }

  }
}
