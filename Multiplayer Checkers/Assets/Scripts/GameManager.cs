using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; set; }

    public GameObject serverPrefab;
    public GameObject clientPrefab;
    public TMP_InputField hostInput;
    public TMP_InputField nameInput;

    private Server server;
    private Client client;
    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this);
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Game");
    }

    public void ConnectToServerButton()
    {
        string hostAdress = hostInput.text;
        if (hostAdress == "")
        {
            hostAdress = "127.0.0.1";
        }
        
        try
        {
            client = Instantiate(clientPrefab).GetComponent<Client>();
            client.Name = nameInput.text;
            if (client.Name == "")
            {
                client.Name = "Player";
            }
            client.ConnectToServer(hostAdress, 6321);
            Debug.Log("Connect menu set active false");
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    public void HostGame()
    {
        try
        {
            server = Instantiate(serverPrefab).GetComponent<Server>();
            server.Init();
            
            client = Instantiate(clientPrefab).GetComponent<Client>();
            client.Name = nameInput.text;
            client.IsHost = true;
            
            if (client.Name == "")
            {
                client.Name = "Host";
            }
            client.ConnectToServer("127.0.0.1", 6321);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    public void DestroyClientAndServer()
    {
        if (server)
        {
            Destroy(server.gameObject);
        }
        if (client)
        {
            Destroy(client.gameObject);
        }
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Exit game");
    }
}