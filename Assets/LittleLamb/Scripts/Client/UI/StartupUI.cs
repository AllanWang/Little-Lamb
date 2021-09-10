using UnityEngine;
using UnityEngine.SceneManagement;

namespace LittleLamb.Client
{
    /// <summary>
    /// Provides backing logic for any UI before MainMenu stage.
    /// </summary>
    public class StartupUI : MonoBehaviour
    {
        void Start()
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}

