using MLAPI;
using MLAPI.NetworkVariable.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;

namespace LittleLamb.Client
{
  /// <summary>
  /// Client specialization of the Character Select game state. Mainly controls the UI during character-select.
  /// </summary>
  [RequireComponent(typeof(LobbyData))]
  public class ClientLobbyState : GameStateBehaviour
  {
    /// <summary>
    /// Reference to the scene's state object so that UI can access state
    /// </summary>
    public static ClientLobbyState Instance { get; private set; }

    public override GameState ActiveState { get { return GameState.Lobby; } }
    public LobbyData LobbyData { get; private set; }

    [SerializeField]
    [Tooltip("Text element containing player names")]
    private TextMeshProUGUI playerList;

    [SerializeField]
    private Button startButton;

    [SerializeField]
    [Tooltip("Error message text when lobby is full")]
    private string m_FatalErrorLobbyFullMsg = "Error: lobby is full! You cannot play.";

    /// <summary>
    /// Conceptual modes or stages that the lobby can be in. We don't actually
    /// bother to keep track of what LobbyMode we're in at any given time; it's just
    /// an abstraction that makes it easier to configure which UI elements should
    /// be enabled/disabled in each stage of the lobby.
    /// </summary>
    private enum LobbyMode
    {
      Ready, // In valid lobby
      LobbyEnding, // "Get ready! Game is starting!" stage
      FatalError, // "Fatal Error" stage
    }
    // private Dictionary<LobbyMode, List<GameObject>> m_LobbyUIElementsByMode;

    private void Awake()
    {
      Instance = this;
      LobbyData = GetComponent<LobbyData>();
      // m_LobbyUIElementsByMode = new Dictionary<LobbyMode, List<GameObject>>()
      //       {
      //           { LobbyMode.ChooseSeat, m_UIElementsForNoSeatChosen },
      //           { LobbyMode.SeatChosen, m_UIElementsForSeatChosen },
      //           { LobbyMode.LobbyEnding, m_UIElementsForLobbyEnding },
      //           { LobbyMode.FatalError, m_UIElementsForFatalError },
      //       };
    }

    protected override void Start()
    {
      base.Start();
      // Not clicked; we join immediately and are ready
      OnPlayerClickedSeat();
      startButton.gameObject.SetActive(IsHost);
    }

    protected override void OnDestroy()
    {
      base.OnDestroy();
      if (LobbyData)
      {
        LobbyData.OnFatalLobbyError -= OnFatalLobbyError;
        LobbyData.LobbyPlayers.OnListChanged -= OnLobbyPlayerStateChanged;
      }
      if (Instance == this)
        Instance = null;
    }

    public override void NetworkStart()
    {
      base.NetworkStart();
      if (!IsClient)
      {
        enabled = false;
      }
      else
      {
        LobbyData.OnFatalLobbyError += OnFatalLobbyError;
        LobbyData.LobbyPlayers.OnListChanged += OnLobbyPlayerStateChanged;
      }
    }

    /// <summary>
    /// Called by the server when any of the seats in the lobby have changed. (Including ours!)
    /// </summary>
    private void OnLobbyPlayerStateChanged(NetworkListEvent<LobbyData.LobbyPlayerState> lobbyArray)
    {
      var playerNames = new List<string>();
      // now let's find our local player in the list and update the character/info box appropriately
      int localPlayerIdx = -1;
      for (int i = 0; i < LobbyData.LobbyPlayers.Count; ++i)
      {
        if (LobbyData.LobbyPlayers[i].ClientId == NetworkManager.Singleton.LocalClientId)
        {
          localPlayerIdx = i;
        }
        playerNames.Add(LobbyData.LobbyPlayers[i].PlayerName);
      }

      playerList.text = String.Join(", ", playerNames.ToArray());
    }

    /// <summary>
    /// Called by server when there is a fatal error
    /// </summary>
    /// <param name="error"></param>
    private void OnFatalLobbyError(LobbyData.FatalLobbyError error)
    {
      Debug.Log($"Lobby error {nameof(error)}");
    }

    /// <summary>
    /// Called directly by UI elements!
    /// </summary>
    /// <param name="seatIdx"></param>
    public void OnPlayerClickedSeat()
    {
      LobbyData.ChangeSeatServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    /// <summary>
    /// Called directly by UI elements!
    /// </summary>
    public void OnPlayerExit()
    {
      // Player is leaving the group
      // first disconnect then return to menu
      var gameNetPortal = GameObject.FindGameObjectWithTag("GameNetPortal").GetComponent<GameNetPortal>();
      gameNetPortal.RequestDisconnect();
      SceneManager.LoadScene("MainMenu");
    }

  }
}
