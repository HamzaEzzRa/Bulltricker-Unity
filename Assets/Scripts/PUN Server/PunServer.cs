using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PunServer : MonoBehaviourPunCallbacks
{
    public static PunServer instance;
    public static bool IsConnectedToMaster { get; private set; }
    public GameObject connectionWaitPrefab, connectionFailurePrefab;
    private GameObject connectionWait, connectionFailure;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(transform.gameObject);
        IsConnectedToMaster = false;
    }

    public void RemoveConnectionPrompt()
    {
        if (connectionFailure != null)
        {
            Destroy(connectionFailure);
            connectionFailure = null;
        }
    }

    public void ConnectToMaster()
    {
        if (!IsConnectedToMaster)
        {
            Debug.Log("Connecting to PUN server ...");
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.GameVersion = MasterManager.GameSettings.GameVersion;
            PhotonNetwork.NickName = MasterManager.GameSettings.NickName;
            PhotonNetwork.ConnectUsingSettings();
            RemoveConnectionPrompt();
            if (connectionWait == null)
                connectionWait = Instantiate(connectionWaitPrefab, null);
        }
        else
        {
            TabGroup.instance.InitTabs(true);
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("User with ID: " + PhotonNetwork.LocalPlayer.NickName + " has connected successfully!");
        IsConnectedToMaster = true;
        Destroy(connectionWait);
        connectionWait = null;
        TabGroup.instance.InitTabs(true);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Disconnected from PUN server for reason: " + cause);
        IsConnectedToMaster = false;
        Destroy(connectionWait);
        connectionWait = null;
        if (cause == DisconnectCause.ClientTimeout || cause == DisconnectCause.ServerTimeout)
        {
            Debug.Log("Trying to reconnect ...");
            PhotonNetwork.Reconnect();
        }
        else if (cause == DisconnectCause.ExceptionOnConnect)
        {
            if (connectionFailure == null)
            {
                Transform interfaceTransform = GameObject.FindGameObjectWithTag("UI").transform;
                connectionFailure = Instantiate(connectionFailurePrefab, interfaceTransform);
            }   
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        RoomListing.LeaveRoom();
        PhotonNetwork.LeaveRoom();
    }
}
