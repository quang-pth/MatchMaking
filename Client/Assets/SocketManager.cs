﻿using System;
using System.Collections;
using System.Collections.Generic;
using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Linq;

public class Player
{
    [JsonProperty("user_id")]
    public string user_id;
    [JsonProperty("display_name")]
    public string display_name;
}

public class SocketManager : MonoBehaviour
{
    public SocketIOUnity socket;

    [Tooltip("The server URL for ScoketIO to make request")]
    public string serverURL;

    public Canvas mainCanvas;
    public InputField DataTxt;
    public Text ReceivedText;  

    private Dictionary<string, List<Player>> matchTeam = new Dictionary<string, List<Player>>();
    public Dictionary<string, List<Player>> MatchTeam
    {
        get { return matchTeam; }
    }


    void Start()
    {
        Uri uri = new Uri(serverURL);
        socket = new SocketIOUnity(uri, new SocketIOOptions
        {
            Query = new Dictionary<string, string>
                {
                    {"token", "UNITY" }
                }
            ,
            EIO = 4
            ,
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        });
        socket.JsonSerializer = new NewtonsoftJsonSerializer();

        socket.OnConnected += (sender, e) =>
        {
            Debug.Log("socket.OnConnected");
        };
        socket.OnPing += (sender, e) =>
        {
            Debug.Log("Ping");
        };
        socket.OnPong += (sender, e) =>
        {
            Debug.Log("Pong: " + e.TotalMilliseconds);
        };
        socket.OnDisconnected += (sender, e) =>
        {
            Debug.Log("disconnect: " + e);
        };
        socket.OnReconnectAttempt += (sender, e) =>
        {
            Debug.Log($"{DateTime.Now} Reconnecting: attempt = {e}");
        };

        Debug.Log("Connecting...");
        socket.Connect();

        // On response match found event
        socket.OnUnityThread("found_match", (data) =>
        {
            StartCoroutine(StartMatch(data));
        });

        ReceivedText.text = "";
    }

    private void OnDestroy()
    {
        socket.Disconnect();
    }

    private IEnumerator StartMatch(SocketIOResponse data)
    {
        JObject resJson = JObject.Parse(data.ToString()[1..^1]);
        var dictTeam = resJson["dict_team"].ToObject<Dictionary<string, JArray>>();

        foreach(KeyValuePair<string, JArray> keyValuePair in dictTeam)
        {
            JArray team = keyValuePair.Value;
            List<Player> playerList = new List<Player>();
                
            foreach(var playerObj in team)
            {
                Player player = playerObj.ToObject<Player>();
                playerList.Add(player);
            }

            this.matchTeam.Add(keyValuePair.Key, playerList);
        }

        float matchStartDuration = 5.0f;
        ReceivedText.text = "Match found! The match will start in " + matchStartDuration + " seconds";
        yield return new WaitForSeconds(matchStartDuration);
        SceneManager.LoadScene("RoomScene", LoadSceneMode.Additive);
        mainCanvas.gameObject.SetActive(false);
    }

    public void FindMatch()
    {
        string eventName = "find";
        string txt = "{\"user_id\": \"" + DataTxt.text + "\"}";

        if (!IsJSON(txt))
        {
            socket.Emit(eventName, txt);
        }
        else
        {
            socket.EmitStringAsJSON(eventName, txt);
        }

        ReceivedText.text = "Finding Match...";
    }

    public static bool IsJSON(string str)
    {
        if (string.IsNullOrWhiteSpace(str)) { return false; }
        str = str.Trim();
        if ((str.StartsWith("{") && str.EndsWith("}")) || //For object
            (str.StartsWith("[") && str.EndsWith("]"))) //For array
        {
            try
            {
                var obj = JToken.Parse(str);
                return true;
            }catch (Exception ex) //some other exception
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }
        else
        {
            return false;
        }
    }
}