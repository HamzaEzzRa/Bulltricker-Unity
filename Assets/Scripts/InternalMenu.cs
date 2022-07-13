using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Net;
using System.Net.Sockets;

public class InternalMenu : MonoBehaviour
{
    #region Members
    private GameObject mainMenu, addressInput;
    public GameObject clientPrefab, serverPrefab;
    #endregion

    #region Start
    private void Start()
    {
        mainMenu = GameObject.Find("MainMenu").gameObject;
    }
    #endregion

    #region Online
    public void Online()
    {
        gameObject.SetActive(false);
        MainMenu.onlineMenu.SetActive(true);
    }
    #endregion

    #region Host
    public void Host()
    {
        try
        {
            Server server = Instantiate(serverPrefab).GetComponent<Server>();
            server.Setup();
            Client hostClient = Instantiate(clientPrefab).GetComponent<Client>();
            hostClient.ConnectToServer("127.0.0.1", Server.port);
            Client.isHost = true;
        }
        catch (Exception exception)
        {
            Debug.Log(exception.Message);
        }
        gameObject.SetActive(false);
        MainMenu.hostMenu.SetActive(true);
    }
    #endregion

    #region ConnectMenu
    public void ConnectMenu()
    {
        gameObject.SetActive(false);
        MainMenu.connectMenu.SetActive(true);
    }
    #endregion

    #region Connect
    public void Connect()
    {
        addressInput = GameObject.Find("HostAddress").gameObject;
        string hostAddress = addressInput.GetComponent<TMPro.TMP_InputField>().text;
        if (hostAddress == "")
        {
            hostAddress = "127.0.0.1";
        }
        try
        {
            Client client = Instantiate(clientPrefab).GetComponent<Client>();
            client.ConnectToServer(hostAddress, Server.port);
            MainMenu.connectMenu.SetActive(false);
            Preserve.instance.isLocalOnline = true;
            Client.isHost = false;
        }
        catch (Exception exception)
        {
            Debug.Log(exception.Message);
        }
    }
    #endregion

    #region Offline
    public void Offline()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    #endregion

    #region ViewRotation
    public void ViewRotation()
    {
        if (Preserve.instance.isCameraRotatable)
        {
            MainMenu.onText.SetActive(false);
            MainMenu.offText.SetActive(true);
            Preserve.instance.isCameraRotatable = false;
        }
        else
        {
            MainMenu.onText.SetActive(true);
            MainMenu.offText.SetActive(false);
            Preserve.instance.isCameraRotatable = true;
        }
    }
    #endregion

    #region BackMain
    public void BackMain()
    {
        gameObject.SetActive(false);
        mainMenu.SetActive(true);
    }
    #endregion

    #region BackVersus
    public void BackVersus()
    {
        gameObject.SetActive(false);
        MainMenu.versusMenu.SetActive(true);
    }
    #endregion

    #region BackOnline
    public void BackOnline()
    {
        gameObject.SetActive(false);
        MainMenu.onlineMenu.SetActive(true);
    }
    #endregion

    #region GetLocalIP
    public static string GetLocalIP()
    {
        using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
        {
            socket.Connect("8.8.8.8", 65530);
            IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
            return endPoint.Address.ToString();
        }
    }
    #endregion

    #region Cancel
    public void Cancel()
    {
        gameObject.SetActive(false);
        MainMenu.onlineMenu.SetActive(true);
        Destroy(GameObject.Find("Server(Clone)").gameObject);
        Destroy(GameObject.Find("Client(Clone)").gameObject);
    }
    #endregion
}