using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;

public class LobbyHandler : MonoBehaviourPunCallbacks
{
    #region Members
    public static GameObject newWait;
    public GameObject roomInfoPrefab, roomView, waitingPrefab;
    public static GameObject createdRoom;
    [HideInInspector] public List<RoomListing> roomListings;
    [SerializeField] private TMPro.TMP_InputField roomNameInput = null;
    public TMPro.TMP_Text availableRoomsText;
    #endregion

    #region Update
    private void Update()
    {
        if (!PhotonNetwork.InLobby && PhotonNetwork.IsConnectedAndReady && PhotonNetwork.NetworkClientState != ClientState.JoiningLobby)
            PhotonNetwork.JoinLobby();
    }
    #endregion

    #region PlayClickSound
    public void PlayClickSound()
    {
        AudioManager.instance.PlaySound("Click_UI_2");
    }
    #endregion

    #region HideLobby
    public void HideLobby()
    {
        if (EventHandler.instance.boardObject != null)
            EventHandler.instance.boardObject.SetActive(true);
        if (EventHandler.instance.boardOriginalScale != null)
            LeanTween.scale(EventHandler.instance.boardObject, EventHandler.instance.boardOriginalScale, 20f * Time.deltaTime);
        if (EventHandler.newLobby != null)
        {
            Destroy(EventHandler.newLobby.gameObject);
            EventHandler.newLobby = null;
        }
        TabGroup.isOnFront = true;
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
            Debug.Log(PhotonNetwork.LocalPlayer.NickName + " has left the lobby ...");
        }
    }
    #endregion

    #region ShowRoomMax
    private void ShowRoomMax()
    {
        availableRoomsText.SetText("Available Rooms: " + roomListings.Count + "/" + MasterManager.GameSettings.MaxRoomCount);
    }

    private void ShowRoomMax(int offset)
    {
        availableRoomsText.SetText("Available Rooms: " + (roomListings.Count + offset) + "/" + MasterManager.GameSettings.MaxRoomCount);
    }
    #endregion

    #region AddRoom
    public void AddRoom()
    {
        if (roomView != null && roomNameInput != null && PhotonNetwork.IsConnected && createdRoom == null && PhotonNetwork.CountOfRooms < MasterManager.GameSettings.MaxRoomCount)
        {
            RoomOptions roomOptions = new RoomOptions
            {
                MaxPlayers = 2,
                PlayerTtl = 20000
            };
            string name = roomNameInput.text;
            if (name == "")
                name = "Default_0" + PhotonNetwork.CountOfRooms;
            if (PhotonNetwork.JoinOrCreateRoom(name, roomOptions, TypedLobby.Default))
            {
                if (newWait == null)
                    newWait = Instantiate(waitingPrefab, EventHandler.newLobby.transform);
            }
        }
    }

    public void CancelWait()
    {
        if (newWait != null)
        {
            if (PhotonNetwork.InRoom)
                PhotonNetwork.LeaveRoom(true);
            Destroy(newWait.gameObject);
            newWait = null;
            createdRoom = null;
            HideLobby();
            GameObject.Find("Page Area").GetComponent<EventHandler>().ShowLobby();
        }
    }

    public override void OnCreatedRoom()
    {
        createdRoom = CreateRoomListing();
        createdRoom.GetComponentInChildren<TMPro.TMP_Text>().SetText(name);
        ShowRoomMax(1);
        Debug.Log("Room creation was successful ...");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Room Creation has failed: " + message);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomInfo info in roomList)
        {
            if (info.RemovedFromList)
            {
                int index = roomListings.FindIndex(x => x.RoomInfo.Name == info.Name);
                if (index > -1)
                {
                    Destroy(roomListings[index].gameObject);
                    roomListings.RemoveAt(index);
                }
            }
            else
            {
                GameObject newRoom = CreateRoomListing(info);
            }
            ShowRoomMax();
            Debug.Log("Room listing has been updated ...");
        }
    }

    public GameObject CreateRoomListing(RoomInfo info)
    {
        GameObject newRoom = Instantiate(roomInfoPrefab, roomView.transform);
        RoomListing listing = newRoom.GetComponent<RoomListing>();
        if (listing != null)
        {
            listing.SetRoomInfo(info);
            roomListings.Add(listing);
        }
        return newRoom;
    }

    public GameObject CreateRoomListing()
    {
        GameObject newRoom = Instantiate(roomInfoPrefab, roomView.transform);
        return newRoom;
    }
    #endregion
}