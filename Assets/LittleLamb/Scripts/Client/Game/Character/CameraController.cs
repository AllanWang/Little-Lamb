using Cinemachine;
using UnityEngine;
using UnityEngine.Assertions;

namespace LittleLamb.Visual
{
  public class CameraController : MonoBehaviour
  {
    private CinemachineVirtualCamera m_MainCamera;

    void Start()
    {
      AttachCamera();
    }

    private void AttachCamera()
    {
      // Boss Room uses free look whereas tutorials I followed use virtual camera. I don't see why free look was needed.
      m_MainCamera = GameObject.FindObjectOfType<CinemachineVirtualCamera>();
      Assert.IsNotNull(m_MainCamera, "CameraController.AttachCamera: Couldn't find gameplay virtual camera");

      if (m_MainCamera)
      {
        // camera body / aim 
        m_MainCamera.Follow = transform;
        m_MainCamera.LookAt = transform;
      }
    }
  }
}
