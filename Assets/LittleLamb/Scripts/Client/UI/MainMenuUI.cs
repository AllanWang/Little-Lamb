using LittleLamb.Client;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

namespace LittleLamb.Visual
{
  /// <summary>
  /// Provides backing logic for all of the UI that runs in the MainMenu stage.
  /// </summary>
  public class MainMenuUI : MonoBehaviour
  {
    private const string k_DefaultIP = "127.0.0.1";

    private GameNetPortal m_GameNetPortal;

    private Client.ClientGameNetPortal m_ClientNetPortal;

    /// <summary>
    /// This will get more sophisticated as we move to a true relay model.
    /// </summary>
    private const int k_ConnectPort = 9998;

    [SerializeField]
    [Tooltip("Name input field")]
    private InputField nameInputField;

    [SerializeField]
    [Tooltip("Main generic input field (IP Address/Room)")]
    private InputField ipInputField;

    [SerializeField]
    [Tooltip("Port input field")]
    private InputField portInputField;

    [SerializeField]
    [Tooltip("Online mode toggle")]
    private Toggle onlineModeToggle;

    private OnlineMode onlineMode { get { return onlineModeToggle.enabled ? OnlineMode.IpHost : OnlineMode.Relay; } }

    private string playerName { get { return string.IsNullOrEmpty(nameInputField.text) ? "Bob" : nameInputField.text; } }


    void Start()
    {
      // Find the game Net Portal by tag - it should have been created by Startup
      GameObject GamePortalGO = GameObject.FindGameObjectWithTag("GameNetPortal");
      Assert.IsNotNull("No GameNetPortal found, Did you start the game from the Startup scene?");
      m_GameNetPortal = GamePortalGO.GetComponent<GameNetPortal>();
      m_ClientNetPortal = GamePortalGO.GetComponent<Client.ClientGameNetPortal>();

      m_ClientNetPortal.NetworkTimedOut += OnNetworkTimeout;
      m_ClientNetPortal.ConnectFinished += OnConnectFinished;

      //any disconnect reason set? Show it to the user here. 
      ConnectStatusToMessage(m_ClientNetPortal.DisconnectReason.Reason, false);
      m_ClientNetPortal.DisconnectReason.Clear();

      ipInputField.text = k_DefaultIP;
      portInputField.text = k_ConnectPort.ToString();
    }

    public void OnHostClicked()
    {
      m_GameNetPortal.PlayerName = playerName;
      switch (onlineMode)
      {
        case OnlineMode.Relay:
          m_GameNetPortal.StartRelayHost(ipInputField.text);
          break;

        case OnlineMode.IpHost:
          int portNum = k_ConnectPort;
          int.TryParse(portInputField.text, out portNum);
          m_GameNetPortal.StartHost(PostProcessIpInput(ipInputField.text), portNum);
          break;
      }
    }

    public void OnConnectClicked()
    {
      m_GameNetPortal.PlayerName = playerName;
      switch (onlineMode)
      {
        case OnlineMode.Relay:
          m_GameNetPortal.StartRelayHost(ipInputField.text);
          if (ClientGameNetPortal.StartClientRelayMode(m_GameNetPortal, ipInputField.text, out string failMessage) == false)
          {
            Debug.Log(failMessage);
            return;
          }
          break;

        case OnlineMode.IpHost:
          int portNum = k_ConnectPort;
          int.TryParse(portInputField.text, out portNum);
          ClientGameNetPortal.StartClient(m_GameNetPortal, ipInputField.text, portNum);
          break;
      }
    }

    private string PostProcessIpInput(string ipInput)
    {
      string ipAddress = ipInput;
      if (string.IsNullOrEmpty(ipInput))
      {
        ipAddress = k_DefaultIP;
      }

      return ipAddress;
    }

    /// <summary>
    /// Callback when the server sends us back a connection finished event.
    /// </summary>
    /// <param name="status"></param>
    private void OnConnectFinished(ConnectStatus status)
    {
      ConnectStatusToMessage(status, true);
    }

    /// <summary>
    /// Takes a ConnectStatus and shows an appropriate message to the user. This can be called on: (1) successful connect,
    /// (2) failed connect, (3) disconnect. 
    /// </summary>
    /// <param name="connecting">pass true if this is being called in response to a connect finishing.</param>
    private void ConnectStatusToMessage(ConnectStatus status, bool connecting)
    {
      switch (status)
      {
        case ConnectStatus.Undefined:
        case ConnectStatus.UserRequestedDisconnect:
          break;
        case ConnectStatus.ServerFull:
          Debug.Log("Connection Failed: The Host is full and cannot accept any additional connections");
          break;
        case ConnectStatus.Success:
          if (connecting) { Debug.Log("Success!: Joining Now"); }
          break;
        case ConnectStatus.LoggedInAgain:
          Debug.Log("Connection Failed: You have logged in elsewhere using the same account");
          break;
        case ConnectStatus.GenericDisconnect:
          var title = connecting ? "Connection Failed" : "Disconnected From Host";
          var text = connecting ? "Something went wrong" : "The connection to the host was lost";
          Debug.Log(title + ": " + text);
          break;
        default:
          Debug.LogWarning($"New ConnectStatus {status} has been added, but no connect message defined for it.");
          break;
      }
    }

    /// <summary>
    /// Invoked when the client sent a connection request to the server and didn't hear back at all.
    /// This should create a UI letting the player know that something went wrong and to try again
    /// </summary>
    private void OnNetworkTimeout()
    {
      Debug.Log("Connection Failed: Unable to Reach Host/Server: Please try again");
    }

    private void OnDestroy()
    {
      m_ClientNetPortal.NetworkTimedOut -= OnNetworkTimeout;
      m_ClientNetPortal.ConnectFinished -= OnConnectFinished;
    }
  }
}
