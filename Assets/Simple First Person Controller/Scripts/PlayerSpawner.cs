using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;

public class PlayerSpawner : NetworkBehaviour
{

  CharacterController cc;
  Renderer[] renderers;

  // Other scripts attached to player
  public Behaviour[] scripts;

  // Start is called before the first frame update
  void Start()
  {
    cc = GetComponent<CharacterController>();
    renderers = GetComponentsInChildren<Renderer>();
  }

  // Update is called once per frame
  void Update()
  {
    if (IsLocalPlayer && Input.GetKeyDown(KeyCode.Y))
    {
      Respawn();
    }
  }

  public void Respawn()
  {
    RespawnServerRpc();
  }

  [ServerRpc]
  void RespawnServerRpc()
  {
    RespawnClientRpc(SpawnPoint());
  }

  [ClientRpc]
  void RespawnClientRpc(Vector3 spawnPoint)
  {
    StartCoroutine(RespawnCoroutine(spawnPoint));
  }

  Vector3 SpawnPoint()
  {
    float x = Random.Range(-5f, 5f);
    float y = 2f;
    float z = Random.Range(-5f, 5f);
    return new Vector3(x, y, z);
  }

  IEnumerator RespawnCoroutine(Vector3 spawnPoint)
  {
    cc.enabled = false;
    EnablePlayerScripts(false);
    yield return new WaitForSeconds(3f);
    transform.position = spawnPoint;
    cc.enabled = true;
    EnablePlayerScripts(true);
  }

  void EnablePlayerScripts(bool enable)
  {
    foreach (var script in scripts)
    {
      script.enabled = enable;
    }
    foreach (var renderer in renderers)
    {
      renderer.enabled = enable;
    }
  }
}
