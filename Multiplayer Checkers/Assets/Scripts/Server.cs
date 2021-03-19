using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEditor;
using UnityEngine;

public class Server : MonoBehaviour
{
    public int port = 6321;

    private List<ServerClient> clients;
    private List<ServerClient> disconnectList;

    private TcpListener server;
    private bool serverStarted;

    public void Init()
    {
        DontDestroyOnLoad(gameObject);
        clients = new List<ServerClient>();
        disconnectList = new List<ServerClient>();
        
        try
        {
            server = new TcpListener(IPAddress.Any, port);
            server.Start();
            
            StartListening();
            serverStarted = true;
        }
        catch(Exception e)
        {
            Debug.Log("Socket error: " + e.Message);
        }
    }
    private void Update()
    {
        if (!serverStarted)
        {
            return;
        }

        foreach (ServerClient client in clients)
        {
            // is client still connected?
            if (!IsConnected(client.client))
            {
                client.client.Close();
                disconnectList.Add(client);
                continue;
            }
            else
            {
                NetworkStream stream = client.client.GetStream();
                if (stream.DataAvailable)
                {
                    StreamReader streamReader = new StreamReader(stream, true);
                    string data = streamReader.ReadLine();

                    if (data != null)
                    {
                        OnIncomingData(client, data);
                    }
                }
            }
        }

        for (int i = 0; i < disconnectList.Count - 1; i++)
        {
            //tell our player somebody has disconnected
            
            clients.Remove(disconnectList[i]);
            disconnectList.RemoveAt(i);
        }
    }
    private void StartListening()
    {
        server.BeginAcceptTcpClient(AcceptTcpClient, server);
    }
    private void AcceptTcpClient(IAsyncResult ar)
    {
        TcpListener listener = (TcpListener) ar.AsyncState;
        ServerClient sc = new ServerClient(listener.EndAcceptTcpClient(ar));
        clients.Add(sc);
        
        StartListening();
        
        Debug.Log("Somebody has connected");
    }
    private bool IsConnected(TcpClient client)
    {
        try
        {
            if (client != null && client.Client != null && client.Client.Connected)
            {
                if (client.Client.Poll(0, SelectMode.SelectRead))
                {
                    return !(client.Client.Receive(new byte[1], SocketFlags.Peek) == 0);
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        catch
        {
            return false;
        }
    }
    
    // server send
    private void Broadcast(string data, List<ServerClient> cl)
    {
        foreach (var sc in cl)
        {
            try
            {
                StreamWriter streamWriter = new StreamWriter(sc.client.GetStream());
                streamWriter.WriteLine(data);
                streamWriter.Flush();
            }
            catch (Exception e)
            {
                Debug.Log("Write error: " + e.Message);
            }
        }
    }

    // server read
    private void OnIncomingData(ServerClient client, string data)
    {
        Debug.Log(client.name + " : " + data);
    }
}

public class ServerClient
{
    public string name;
    public TcpClient client;

    public ServerClient(TcpClient client)
    {
        this.client = client;
    }
}
