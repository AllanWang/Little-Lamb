using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.NetworkVariable;

public class PlayerHealth : NetworkBehaviour
{

  [SerializeField]
  NetworkVariableInt health = new NetworkVariableInt(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.OwnerOnly }, 100);

  PlayerSpawner playerSpawner;

  // Start is called before the first frame update
  void Start()
  {
    playerSpawner = GetComponent<PlayerSpawner>();
  }

  // Update is called once per frame
  void Update()
  {
    if (IsLocalPlayer && health.Value <= 0)
    {
      health.Value = 100;
      playerSpawner.Respawn();
    }
  }

  // Runs on server
  public void TakeDamage(int damage)
  {
    health.Value = Mathf.Max(0, health.Value - damage);
  }
}
