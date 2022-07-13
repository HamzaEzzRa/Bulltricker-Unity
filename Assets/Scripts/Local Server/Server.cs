using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;

public class Server : MonoBehaviour
{
    #region Members
    public static int port = 3128;
    private List<ServerClient> clientList;
    private TcpListener server;
    private bool serverStarted, gameStarted;
    public static bool isWaiting = false;
    #endregion

    #region Setup
    public void Setup()
    {
        DontDestroyOnLoad(gameObject);
        clientList = new List<ServerClient>();
        try
        {
            server = new TcpListener(IPAddress.Any, port);
            server.Start();
            serverStarted = true;
            StartListening();
        }
        catch (Exception exception)
        {
            Debug.Log("Socket Error: " + exception.Message);
        }
    }
    #endregion

    #region Update
    private void Update()
    {
        if(!serverStarted)
        {
            return;
        }
        for (int i = 0; i < clientList.Count; i++)
        {
            if (!IsConnected(clientList[i].tcp))
            {
                clientList[i].tcp.Close();
                clientList.RemoveAt(i);
                Debug.Log(i + " has disconnected");
                if (i == 1)
                {
                    Broadcast("Return", clientList);
                }
                continue;
            }
            else
            {
                NetworkStream networkStream = clientList[i].tcp.GetStream();
                if(networkStream.DataAvailable)
                {
                    StreamReader streamReader = new StreamReader(networkStream, true);
                    string data = streamReader.ReadLine();
                    if (data != null)
                    {
                        OnIncomingData(clientList[i], data);
                    }
                }
            }
        }
    }
    #endregion

    #region OnIncomingData
    private void OnIncomingData(ServerClient serverClient, String data)
    {
        if (!gameStarted)
        {
            if (data == "Client Connected" && clientList.Count == 2)
            {
                Broadcast("Start Game", clientList);
            }
            if (data == "Game Started")
            {
                gameStarted = true;
            }
        }
        string[] dataList = data.Split('|');
        switch (dataList[0])
        {
            case "NHC Destroyed":
                Broadcast("Return", clientList);
                break;
            case "HC Destroyed":
                isWaiting = true;
                Broadcast("Self Destruct", clientList);
                break;
            case "Client Destroyed":
                isWaiting = false;
                break;
            case "Move":
                Broadcast(data, clientList);
                break;
            case "Game Over":
                Broadcast(data, clientList);
                break;
        }
    }
    #endregion

    #region Broadcast
    private void Broadcast(string data, List<ServerClient> clientList)
    {
        foreach (ServerClient serverClient in clientList)
        {
            try
            {
                StreamWriter streamWriter = new StreamWriter(serverClient.tcp.GetStream());
                streamWriter.WriteLine(data);
                streamWriter.Flush();
            }
            catch (Exception exception)
            {
                Debug.Log("Write Error:" + exception.Message);
            }
        }
    }

    private void Broadcast(string data, ServerClient serverClient)
    {
        List<ServerClient> newList = new List<ServerClient> { serverClient };
        Broadcast(data, newList);
    }
    #endregion

    #region StartListening
    private void StartListening()
    {
        server.BeginAcceptTcpClient(AcceptTcpClient, server);
    }
    #endregion

    #region AcceptTcpClient
    private void AcceptTcpClient(IAsyncResult ar)
    {
        TcpListener tcpListener = (TcpListener)ar.AsyncState;
        ServerClient serverClient = new ServerClient(tcpListener.EndAcceptTcpClient(ar));
        int i = clientList.Count;
        clientList.Add(serverClient);
        StartListening();
        Debug.Log(i + " has connected!\n");
    }
    #endregion

    #region IsConnected
    private bool IsConnected(TcpClient tcpClient)
    {
        try
        {
            if (tcpClient != null && tcpClient.Client != null && tcpClient.Client.Connected)
            {
                if (tcpClient.Client.Poll(0, SelectMode.SelectRead))
                {
                    return !(tcpClient.Client.Receive(new byte[1], SocketFlags.Peek) == 0);
                }
                return true;
            }
            return false;
        }
        catch (Exception exception)
        {
            Debug.Log(exception.Message);
            return false;
        }
    }
    #endregion

    #region OnDestroy
    public void OnDestroy()
    {
        Broadcast("Self Destruct", clientList);
        serverStarted = false;
        foreach (ServerClient client in clientList)
        {
            client.tcp.Close();
        }
        clientList.Clear();
        server.Stop();
        server = null;
    }
    #endregion
}

public class ServerClient
{
    #region Members
    public string clientName;
    public TcpClient tcp;
    #endregion

    #region ServerClient
    public ServerClient(TcpClient tcp)
    {
        this.tcp = tcp;
    }
    #endregion
}