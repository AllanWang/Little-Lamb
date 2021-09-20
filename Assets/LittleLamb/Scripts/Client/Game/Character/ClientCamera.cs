using System.Collections;
using System.Collections.Generic;
using MLAPI;
using UnityEngine;
using UnityEngine.Assertions;

namespace LittleLamb.Visual
{
  /// Subsection of ClientCharacterVisualization
  ///
  public class ClientCamera : NetworkBehaviour
  {
    [SerializeField]
    TransformVariable m_RuntimeObjectsParent;

    public override void NetworkStart()
    {
      Assert.IsTrue(m_RuntimeObjectsParent && m_RuntimeObjectsParent.Value,
                     "RuntimeObjectsParent transform is not set!");
      transform.SetParent(m_RuntimeObjectsParent.Value);

      if (IsLocalPlayer) {
         gameObject.AddComponent<CameraController>();
      }
    }
  }
}