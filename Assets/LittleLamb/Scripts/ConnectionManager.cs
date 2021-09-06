using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Spawning;
using MLAPI.Transports.UNET;

public class ConnectionManager : MonoBehaviour
{

  public GameObject lobbyPanel;
  private Logger logger = new Logger(new LoggerHandler());

  public Camera lobbyCamera;
  public void Host()
  {
    logger.Log("Host");
    lobbyPanel.SetActive(false);
    lobbyCamera.gameObject.SetActive(false);
    NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
    NetworkManager.Singleton.StartHost(SpawnPoint(), Quaternion.identity);
  }

  public void Join()
  {
    logger.Log("Join");
    lobbyPanel.SetActive(false);
    lobbyCamera.gameObject.SetActive(false);
    NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes("test");
    NetworkManager.Singleton.StartClient();
  }

  private void ApprovalCheck(byte[] bytes, ulong clientId, NetworkManager.ConnectionApprovedDelegate callback)
  {
    bool approve = System.Text.Encoding.ASCII.GetString(bytes) == "test";
    logger.Log("Approving connection");
    callback(true, null, approve, SpawnPoint(), Quaternion.identity);
  }

  Vector3 SpawnPoint()
  {
    float x = Random.Range(-5f, 5f);
    float y = 2f;
    float z = Random.Range(-5f, 5f);
    return new Vector3(x, y, z);
  }

}
