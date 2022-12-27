using System;
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
    [JsonProperty("id")]
    public String id;
    [JsonProperty("name")]
    public String name;
}

public class SocketManager : MonoBehaviour
{
    private static SocketManager instance;
    public static SocketManager Instance { 
        get {
            return instance;
        } 
    }

    public SocketIOUnity socket;

    [Tooltip("The server URL for ScoketIO to make request")]
    public String serverURL;

    public Canvas mainCanvas;
    public InputField EventNameTxt;
    public InputField DataTxt;
    public Text ReceivedText;  

    public GameObject objectToSpin;

    private float rotateAngle = 45;
    private readonly float MaxRotateAngle = 45;

    private List<Player> playerList = new List<Player>();
    public List<Player> PlayerList
    {
        get { return playerList; }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
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

        socket.OnUnityThread("spin", (data) =>
        {
            rotateAngle = 0;
        });

        socket.OnUnityThread("found_match", (data) =>
        {
            StartCoroutine(CollectUsers(data));
        });

        ReceivedText.text = "";
    }

    private void OnDestroy()
    {
        socket.Disconnect();
    }

    private IEnumerator CollectUsers(SocketIOResponse data)
    {
        if (playerList.Count > 5)
        {
            Debug.Log("Match has already been found!");
            yield return null;
        }
        else
        {
            JObject resJson = JObject.Parse(data.ToString()[1..^1]);
            IList<JToken> results = resJson["players"].ToList();
            foreach (JToken result in results)
            {
                Player player = result.ToObject<Player>();
                playerList.Add(player);
            }

            float matchStartDuration = 5.0f;
            ReceivedText.text = "Match found! The match will start in " + matchStartDuration + " seconds";
            yield return new WaitForSeconds(matchStartDuration);
            SceneManager.LoadScene("RoomScene", LoadSceneMode.Additive);
            mainCanvas.gameObject.SetActive(false);
            objectToSpin.SetActive(false);
        }
    }

    public void EmitTest()
    {
        string eventName = EventNameTxt.text.Trim().Length < 1 ? "hello" : EventNameTxt.text;
        string txt = DataTxt.text;
        if (!IsJSON(txt))
        {
            socket.Emit(eventName, txt);
        }
        else
        {
            socket.EmitStringAsJSON(eventName, txt);
        }
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

    public void EmitSpin()
    {
        socket.Emit("spin");
    }

    public void EmitClass()
    {
        TestClass2 testClass2 = new TestClass2("lorem ipsum");
        socket.Emit("class", testClass2);
    }

    [System.Serializable]
    class TestClass2
    {
        public string text;

        public TestClass2(string text)
        {
            this.text = text;
        }
    }

    void Update()
    {
        if(rotateAngle < MaxRotateAngle)
        {
            rotateAngle++;
            objectToSpin.transform.Rotate(0, 1, 0);
        }
    }
}