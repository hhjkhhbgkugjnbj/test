using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using UnityEngine;
using Newtonsoft.Json;
using TMPro;
using UnityEngine.SceneManagement;
using System.Threading;

public class To_InGame_Manager : MonoBehaviour
{
    Dictionary<int, string> scene_num_to_name = new Dictionary<int, string>();
    public string nickname;

    // Start is called before the first frame update
    void Start()
    {
        nickname = NetworkManager.Instance.nickname;
        scene_num_to_name.Add(0, "Bridge");
        scene_num_to_name.Add(1, "Bridge 1");
        scene_num_to_name.Add(-1, "Waiting_Room");

        //NetworkManager.Instance.main_quest_num = PlayerPrefs.GetInt(nickname+ "_main_quest_num");
        //NetworkManager.Instance.detail_quest_num = PlayerPrefs.GetInt(nickname + "_detail_quest_num");
        //NetworkManager.Instance.semester = PlayerPrefs.GetInt(nickname + "_semester");
        //NetworkManager.Instance.quest_state = PlayerPrefs.GetInt(nickname + "_quest_state");
        //NetworkManager.Instance.nickname = PlayerPrefs.GetString("client_nickname");

        string to_last_scene = scene_num_to_name[NetworkManager.Instance.last_scene_num];
        Debug.Log(NetworkManager.Instance.scene_num);
        SceneManager.LoadScene(to_last_scene);
    }
}
