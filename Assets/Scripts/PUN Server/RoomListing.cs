using Photon.Realtime;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomListing : MonoBehaviourPunCallbacks
{
    public RoomInfo RoomInfo { get; private set; }
    [SerializeField] TMPro.TMP_Text roomName = null;
    public static bool IsOnline { get; private set; } = false;

    public void SetRoomInfo(RoomInfo info)
    {
        RoomInfo = info;
        roomName.SetText(info.Name);
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(RoomInfo.Name);
        Debug.Log("Room Joined: " + RoomInfo.Name);
    }

    public override void OnJoinedRoom()
    {
        IsOnline = true;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        IsOnline = true;
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.LoadLevel((int)SceneIndex.GAME_SCREEN);
        }
    }

    public static void LeaveRoom()
    {
        TabGroup.isOnFront = true;
        IsOnline = false;
        int[] indexesToUnload = new int[] { (int)SceneIndex.GAME_SCREEN };
        if (!SceneManager.GetSceneByBuildIndex((int)SceneIndex.INITIAL).isLoaded)
            SceneHandler.instance.LoadScene((int)SceneIndex.INITIAL, new int[] { });
        SceneHandler.instance.LoadScene((int)SceneIndex.MAIN_MENU, indexesToUnload);
    }
}
