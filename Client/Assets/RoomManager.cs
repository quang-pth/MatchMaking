using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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
        SocketManager socket = SocketManager.Instance;
        if (socket)
        {
            for (int i = 0; i < socket.PlayerList.Count; i++)
            {
                Player player = socket.PlayerList[i];
                playerTextMesh[i].text = player.name;
            }
        }
        else
        {
            Debug.Log("No socket instance found!");
        }
    }
}
