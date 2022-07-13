using UnityEngine;

public class ConnectionHandler : MonoBehaviour
{
    public void Reconnect()
    {
        PunServer.instance.ConnectToMaster();
    }

    public void OfflineMode()
    {
        TabGroup.instance.InitTabs(false);
        PunServer.instance.RemoveConnectionPrompt();
    }
}
