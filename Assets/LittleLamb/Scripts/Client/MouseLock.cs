using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLock : MonoBehaviour
{

  void Start()
  {
    LockMouse(true);
  }
  void Update()
  {
    if (Input.GetMouseButtonDown(0))
    {
      LockMouse(true);
    }
  }

  private void LockMouse(bool lockMouse)
  {
    Cursor.lockState = lockMouse ? CursorLockMode.Locked : CursorLockMode.None;
    Cursor.visible = !lockMouse;
  }
}
