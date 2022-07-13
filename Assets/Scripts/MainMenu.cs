using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    #region Members
    public static GameObject onText, offText, onlineMenu, versusMenu, optionsMenu, connectMenu, hostMenu, IPOutput;
    #endregion

    #region Start
    public void Start()
    {
        connectMenu = GameObject.Find("ConnectMenu").gameObject;
        hostMenu = GameObject.Find("HostMenu").gameObject;
        optionsMenu = GameObject.Find("OptionsMenu").gameObject;
        versusMenu = GameObject.Find("1VS1Menu").gameObject;
        onlineMenu = GameObject.Find("OnlineMenu").gameObject;
        onText = GameObject.Find("On").gameObject;
        offText = GameObject.Find("Off").gameObject;
        IPOutput = GameObject.Find("IpAddress").gameObject;
        IPOutput.GetComponent<TMPro.TMP_InputField>().SetTextWithoutNotify(InternalMenu.GetLocalIP());
        if (Preserve.instance.isCameraRotatable)
        {
            offText.SetActive(false);
        }
        else
        {
            onText.SetActive(false);
            offText.SetActive(true);
        }
        connectMenu.SetActive(false);
        hostMenu.SetActive(false);
        optionsMenu.SetActive(false);
        versusMenu.SetActive(false);
        onlineMenu.SetActive(false);
    }
    #endregion

    #region PlayAI
    public void PlayAI()
    {
        Preserve.instance.againstAI = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    #endregion

    #region Play1V1
    public void Play1V1()
    {
        gameObject.SetActive(false);
        versusMenu.SetActive(true);
    }
    #endregion

    #region Options
    public void Options()
    {
        gameObject.SetActive(false);
        optionsMenu.SetActive(true);
    }
    #endregion
}