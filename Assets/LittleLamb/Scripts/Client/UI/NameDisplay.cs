using UnityEngine.UI;
using UnityEngine.Assertions;
using UnityEngine;
using TMPro;

namespace LittleLamb.Visual
{
  /// <summary>
  /// This is responsible for displaying and updating the player's chosen name.  Currently, the game does not allow just any
  /// input for a name, but instead creates a randomized 2 word combination for the player.  The player is then able to click randomize
  /// to receive a new name
  /// EDIT: Temp show client id
  /// </summary>
  public class NameDisplay : MonoBehaviour
  {
    private GameNetPortal m_GameNetPortal;

    [SerializeField]
    private TextMeshProUGUI m_CurrentName;

    /// <summary>
    /// This is where we pull our name data from
    /// </summary>
    // [SerializeField]
    // private NameGenerationData m_NameData;

    public void Start()
    {
      // Find the game Net Portal by tag - it should have been created by Startup
      GameObject GamePortalGO = GameObject.FindGameObjectWithTag("GameNetPortal");
      Assert.IsNotNull("No GameNetPortal found, Did you start the game from the Startup scene?");
      m_GameNetPortal = GamePortalGO.GetComponent<GameNetPortal>();

      ChooseNewName();
    }

    public string GetCurrentName()
    {
      return m_CurrentName.text;
    }

    /// <summary>
    /// Called to randomly select a new name for the player and displays it.
    /// </summary>
    public void ChooseNewName()
    {
      // var firstWord = m_NameData.FirstWordList[Random.Range(0, m_NameData.FirstWordList.Length - 1)];
      // var secondWord = m_NameData.SecondWordList[Random.Range(0, m_NameData.SecondWordList.Length - 1)];

      m_CurrentName.text = "TestPlayer" + m_GameNetPortal.NetManager.LocalClientId;
    }
  }
}
