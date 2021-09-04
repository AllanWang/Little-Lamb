using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.NetworkVariable;

public class PlayerHealth : NetworkBehaviour
{

  NetworkVariableInt health = new NetworkVariableInt(100);

  public int actualHealth = 100;

  // Start is called before the first frame update
  void Start()
  {

  }

  // Update is called once per frame
  void Update()
  {
    actualHealth = health.Value;
  }

  public void TakeDamage(int damage)
  {
    health.Value = Mathf.Max(0, health.Value - damage);
  }
}
