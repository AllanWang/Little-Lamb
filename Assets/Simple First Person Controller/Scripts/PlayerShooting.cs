using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;

public class PlayerShooting : NetworkBehaviour
{

  public TrailRenderer bulletTrail;

  public Transform gunBarrel;

  // Start is called before the first frame update
  void Start()
  {

  }

  // Update is called once per frame
  void Update()
  {
    if (IsLocalPlayer)
    {
      if (Input.GetButtonDown("Fire1"))
      {
        ShootServerRpc();
      }
    }
  }

  [ServerRpc]
  void ShootServerRpc()
  {
    if (Physics.Raycast(gunBarrel.position, gunBarrel.forward, out RaycastHit hit, 200f))
    {
      var enemyHealth = hit.transform.GetComponent<PlayerHealth>();
      if (enemyHealth != null)
      {
        // Hit player
        enemyHealth.TakeDamage(10);
      }
    }
    ShootClientRpc();
  }

  [ClientRpc]
  void ShootClientRpc()
  {
    var bullet = Instantiate(bulletTrail, gunBarrel.position, Quaternion.identity);
    bullet.AddPosition(gunBarrel.position);
    if (Physics.Raycast(gunBarrel.position, gunBarrel.forward, out RaycastHit hit, 200f))
    {
      bullet.transform.position = hit.point;
    }
    else
    {
      bullet.transform.position = gunBarrel.position + (gunBarrel.forward * 200f);
    }
  }
}
