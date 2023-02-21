using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomManager : MonoBehaviour
{
    public TextMeshProUGUI roomName;
    public GameObject playerGroup;
    private List<TextMeshProUGUI> playerTextMesh = new List<TextMeshProUGUI>();
    
    private void Awake()
    {
        foreach (TextMeshProUGUI textMesh in playerGroup.GetComponentsInChildren<TextMeshProUGUI>())
        {
            playerTextMesh.Add(textMesh);
        }
    }

    private void Start()
    {
        SocketManager socket = GameManager.Instance.socketManager;
        if (socket)
        {
            List<Player> playerList = socket.MatchTeam["team_1"];

            for (int i = 0; i < playerList.Count; i++)
            {
                Player player = playerList[i];
                playerTextMesh[i].text = player.display_name;
            }
        }
        else
        {
            Debug.Log("No socket instance found!");
        }
    }

    public void UnloadRoomScene()
    {
        SceneManager.UnloadSceneAsync("RoomScene");
        SceneManager.LoadScene("MainScene");
    }
}
