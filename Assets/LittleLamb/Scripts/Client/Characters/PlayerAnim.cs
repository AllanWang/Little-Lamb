using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnim : MonoBehaviour
{

  Animator animator;
  private int isWalkingHash;
  private int isRunningHash;

  // Start is called before the first frame update
  void Start()
  {
    animator = gameObject.GetComponentInChildren<Animator>();
    isWalkingHash = Animator.StringToHash("isWalking");
    isRunningHash = Animator.StringToHash("isRunning");
  }

  // Update is called once per frame
  void Update()
  {

  }
}
