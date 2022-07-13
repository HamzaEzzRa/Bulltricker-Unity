using UnityEngine;
using UnityEngine.SceneManagement;
using System.Net.Sockets;
using System.IO;
using System;

public class Client : MonoBehaviour
{
    #region Members
    public static bool isHost = false;
    private bool socketReady, isGameOver = false;
    private TcpClient socket;
    private NetworkStream stream;
    private StreamWriter streamWriter;
    private StreamReader streamReader;
    Piece selectedPiece;
    #endregion

    #region Start
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    #region ConnectToServer
    public bool ConnectToServer(string host, int port)
    {
        if(socketReady)
        {
            return false;
        }
        try
        {
            socket = new TcpClient(host, port);
            stream = socket.GetStream();
            streamWriter = new StreamWriter(stream);
            streamReader = new StreamReader(stream);
            socketReady = true;
            Send("Client Connected");
        }
        catch (Exception exception)
        {
            Debug.Log("Socket Error:" + exception.Message);
        }
        return socketReady;
    }
    #endregion

    #region Update
    private void Update()
    {
        if (socketReady)
        {
            if (stream.DataAvailable)
            {
                string data = streamReader.ReadLine();
                if (data != null)
                {
                    OnIncomingData(data);
                }
            }
        }
    }
    #endregion

    #region OnIncomingData
    private void OnIncomingData(string data)
    {
        string[] dataList = data.Split('|');
        switch (dataList[0])
        {
            case "Start Game":
                Preserve.instance.isLocalOnline = true;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
                Send("Game Started");
                break;
            case "Self Destruct":
                if (!isHost)
                {
                    CloseSocket();
                    Destroy(gameObject);
                    Send("Client Destroyed");
                    Preserve.instance.isLocalOnline = false;
                    isHost = false;
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
                }
                break;
            case "Return":
                if (isHost)
                {
                    CloseSocket();
                    Destroy(gameObject);
                    GameObject server = GameObject.Find("Server(Clone)");
                    if (server != null)
                    {
                        Destroy(server.gameObject);
                    }
                    Preserve.instance.isLocalOnline = false;
                    isHost = false;
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
                }
                break;
            case "Move":
                if (!isGameOver)
                {
                    int position = int.Parse(dataList[1]);
                    string command = dataList[3];
                    switch (command)
                    {
                        case "HPU":
                            if (!isHost)
                            {
                                selectedPiece = Board.instance.allCells[position].currentPiece;
                                selectedPiece.transform.position += 2 * Vector3.up;
                            }
                            break;
                        case "HPD":
                            if (!isHost)
                            {
                                selectedPiece = null;
                                Piece piece = Board.instance.allCells[position].currentPiece;
                                piece.transform.position -= 2 * Vector3.up;
                            }
                            break;
                        case "NHPU":
                            if (isHost)
                            {
                                selectedPiece = Board.instance.allCells[position].currentPiece;
                                selectedPiece.transform.position += 2 * Vector3.up;
                            }
                            break;
                        case "NHPD":
                            if (isHost)
                            {
                                selectedPiece = null;
                                Piece piece = Board.instance.allCells[position].currentPiece;
                                piece.transform.position -= 2 * Vector3.up;
                            }
                            break;
                        case "HM":
                            if (!isHost)
                            {
                                Debug.Log("HM");
                                Cell cell = Board.instance.allCells[position];
                                selectedPiece.targetCell = cell;
                                Tuple<int, int> action = new Tuple<int, int>(selectedPiece.currentCell.boardPos, cell.boardPos);
                                StartCoroutine(Board.instance.PerformAction(action));
                                if (PieceManager.instance.isGameOver)
                                {
                                    Send("Game Over|HW");
                                }
                            }
                            break;
                        case "NHM":
                            if (isHost)
                            {
                                Debug.Log("NHM");
                                Cell cell = Board.instance.allCells[position];
                                selectedPiece.targetCell = cell;
                                Tuple<int, int> action = new Tuple<int, int>(selectedPiece.currentCell.boardPos, cell.boardPos);
                                StartCoroutine(Board.instance.PerformAction(action));
                                if (PieceManager.instance.isGameOver)
                                {
                                    Send("Game Over|NHW");
                                }
                            }
                            break;
                    }
                }
                break;
            case "Game Over":
                PieceManager.instance.SetInteractive(PieceManager.instance.whitePawns, false);
                PieceManager.instance.SetInteractive(PieceManager.instance.blackPawns, false);
                isGameOver = true;
                break;
        }
    }
    #endregion

    #region Send
    public void Send(string data)
    {
        if(!socketReady)
        {
            return;
        }
        streamWriter.WriteLine(data);
        streamWriter.Flush();
    }
    #endregion

    #region CloseSocket
    private void CloseSocket()
    {
        if (!socketReady)
        {
            return;
        }
        streamWriter.Close();
        streamReader.Close();
        socket.Close();
        socketReady = false;
    }
    #endregion

    #region OnApplicationQuit
    private void OnApplicationQuit()
    {
        if (!isHost)
        {
            Send("NHC Destroyed");
        }
        CloseSocket();
    }
    #endregion

    #region OnDisable
    private void OnDisable()
    {
        CloseSocket();
    }
    #endregion

    #region OnDestroy
    private void OnDestroy()
    {
        CloseSocket();
    }
    #endregion
}

public class GameClient
{
    #region Members
    public string name;
    public bool isHost;
    #endregion
}