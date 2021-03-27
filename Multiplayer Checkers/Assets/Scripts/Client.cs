using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class Client : MonoBehaviour
{
    public string Name;
    public bool IsHost = false;
    
    private bool socketReady;
    private TcpClient socket;
    private NetworkStream stream;
    private StreamWriter writer;
    private StreamReader reader;

    private List<GameClient> players = new List<GameClient>();

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public bool ConnectToServer(string host, int port)
    {
        if (socketReady)
        {
            return false;
        }

        try
        {
            socket = new TcpClient(host, port);
            stream = socket.GetStream();
            writer = new StreamWriter(stream);
            reader = new StreamReader(stream);

            socketReady = true;
        }
        catch (Exception e)
        {
            Debug.Log("Socket error: " + e.Message);
        }

        return socketReady;
    }
    private void Update()
    {
        if (socketReady)
        {
            if (stream.DataAvailable)
            {
                string data = reader.ReadLine();
                if (data != null)
                {
                    OnIncomingData(data);
                }
            }
        }
    }

    //sending to server
    public void Send(string data)
    {
        if (!socketReady)
        {
            return;
        }
        
        writer.WriteLine(data);
        writer.Flush();
    }
    // read from server
    private void OnIncomingData(string data)
    {
        Debug.Log("Client: " + data);
        string[] aData = data.Split('|');

        switch (aData[0])
        {
            case "SWHO":
                for (int i = 1; i < aData.Length - 1; i++)
                {
                    UserConnected(aData[i], false);
                }
                Send("CWHO|" + Name + "|" + ((IsHost) ? 1 : 0));
                break;
            case "SCNN":
                UserConnected(aData[1], false);
                break;
            case "SMOV":
                CheckerBoard.Instance.TryMove(
                    new Vector2Int(int.Parse(aData[1]), int.Parse(aData[2])), 
                    new Vector2Int(int.Parse(aData[3]), int.Parse(aData[4])));
                break;
        }
    }

    private void UserConnected(string name, bool host)
    {
        GameClient client = new GameClient();
        client.Name = name;
        
        players.Add(client);
        
        if (players.Count == 2)
        {
            GameManager.Instance.StartGame();
        }
    }

    private void OnApplicationQuit()
    {
        CloseSocket();
    }
    private void OnDisable()
    {
        CloseSocket();
    }
    private void CloseSocket()
    {
        if (!socketReady)
        {
            return;
        }
        
        writer.Close();
        reader.Close();
        socket.Close();
        socketReady = false;
    }
}

public class GameClient
{
    public string Name;
    public bool isHost;
}
