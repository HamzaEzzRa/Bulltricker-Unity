using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class EventHandler : MonoBehaviourPunCallbacks
{
    #region Members
    public static EventHandler instance;
    public static GameObject newLobby;
    [HideInInspector] public Vector3 boardOriginalScale;
    public GameObject lobbyPrefab, boardObject;
    public static List<RoomInfo> forwardedList;
    #endregion

    #region Awake
    private void Awake()
    {
        instance = this;
        if (boardObject != null)
            boardOriginalScale = boardObject.transform.localScale;
    }
    #endregion

    #region PlayClickSound
    public void PlayClickSound()
    {
        AudioManager.instance.PlaySound("Click_UI_2");
    }
    #endregion

    #region Gameplay
    private void SelectModeAndLoad(bool againstAI, bool localOnline)
    {
        Preserve.instance.againstAI = againstAI;
        Preserve.instance.isLocalOnline = localOnline;
        int[] indexesToUnload = new int[1] { (int)SceneIndex.MAIN_MENU };
        SceneHandler.instance.LoadScene((int)SceneIndex.GAME_SCREEN, indexesToUnload);
    }

    public void QuickGame()
    {
        SelectModeAndLoad(false, false);
    }

    public void PlayAgainstAI()
    {
        SelectModeAndLoad(true, false);
    }

    public void LocalGame()
    {
        SelectModeAndLoad(false, true);
    }
    #endregion

    #region ShowLobby
    public void ShowLobby()
    {
        TabGroup.isOnFront = false;
        if (boardObject != null)
        {
            LeanTween.scale(boardObject, Vector3.zero, 20f * Time.deltaTime);
            boardObject.SetActive(false);
        }
        if (newLobby == null && lobbyPrefab != null)
        {
            newLobby = Instantiate(lobbyPrefab, GameObject.Find("Online Page").transform);
        }
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
            Debug.Log(PhotonNetwork.LocalPlayer.NickName + " has joined the lobby ...");
        }
    }
    #endregion

    #region QuitGame
    public void QuitGame()
    {
        Application.Quit();
    }
    #endregion
}