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

public class LoginManager : MonoBehaviour
{
    int last_scene;
    float last_x_position;
    float last_y_position;
    int semester;
    int main_quest_num;
    int detail_quest_num;
    string client_nickname;
    int quest_state;

    void Update()
    {
        if (NetworkManager.Instance.login_messages.Count != 0)
        {
            lock (NetworkManager.Instance.login_messages)
            {
                message login_react = NetworkManager.Instance.login_messages.Dequeue();
                if (login_react.pt_id == PROTOCOL.LOGIN_Success)
                {
                    Debug.Log("로그인 성공!");
                    last_scene = (int)login_react.first_login_info.scene_num;
                    last_x_position = (float)login_react.first_login_info.x_position;
                    last_y_position = (float)login_react.first_login_info.y_position;
                    semester = (int)login_react.first_login_info.semester;
                    main_quest_num = (int)login_react.first_login_info.main_quest_num;
                    detail_quest_num = (int)login_react.first_login_info.detail_quest_num;
                    quest_state = (int)login_react.first_login_info.quest_state;
                    List<int> minigamenums = login_react.first_login_info.minigame_nums;
                    List<double> scores = login_react.first_login_info.scores;

                    NetworkManager.Instance.last_scene_num = last_scene;
                    NetworkManager.Instance.last_x_position = last_x_position;
                    NetworkManager.Instance.last_y_position = last_y_position;
                    NetworkManager.Instance.semester = semester;
                    NetworkManager.Instance.main_quest_num = main_quest_num;
                    NetworkManager.Instance.detail_quest_num = detail_quest_num;
                    NetworkManager.Instance.quest_state = quest_state;
                    NetworkManager.Instance.minigame_nums = minigamenums;
                    NetworkManager.Instance.scores = scores;

                    client_nickname = login_react.first_login_info.Nickname;
                    NetworkManager.Instance.nickname = client_nickname;

                    Debug.Log("last_scene: "+ NetworkManager.Instance.last_scene_num + ", last_x_position: "+ NetworkManager.Instance.last_x_position + ", last_y_position: "+ NetworkManager.Instance.last_y_position);

                    //PlayerPrefs.SetInt(client_nickname + "_scene_num", last_scene);
                    //PlayerPrefs.SetFloat(client_nickname + "_last_x_position", last_x_position);
                    //PlayerPrefs.SetFloat(client_nickname + "_last_y_position", last_y_position);
                    //PlayerPrefs.SetString(client_nickname + "_client_nickname", client_nickname);
                    //PlayerPrefs.SetInt(client_nickname + "_semester", semester);
                    //PlayerPrefs.SetInt(client_nickname + "_main_quest_num", main_quest_num);
                    //PlayerPrefs.SetInt(client_nickname + "_detail_quest_num", detail_quest_num);
                    //PlayerPrefs.SetInt(client_nickname + "_quest_state", quest_state);
                    //Debug.Log(minigamenums.Count + ", " + scores.Count);
                    //if(minigamenums.Count!=0)
                    //{
                    //    for (int i = 0; i < minigamenums.Count; i++)
                    //    {
                    //        PlayerPrefs.SetInt(client_nickname + "_minigame_num_" + i, minigamenums[i]);
                    //    }
                    //    for (int i = 0; i < scores.Count; i++)
                    //    {
                    //        PlayerPrefs.SetFloat(client_nickname + "_score_" + i, (float)scores[i]);
                    //    }
                    //}
                    //Debug.Log(PlayerPrefs.GetInt(client_nickname + "_minigame_num_0", 0) + ", " + PlayerPrefs.GetFloat(client_nickname + "_score_0", 0f));
                    SceneManager.LoadScene("Waiting_Room");
                }
                else
                {
                    Debug.Log("로그인 실패. 다시 시도해주세요.");
                }
            }
        }
    }

    public void Login_info_send()
    {
        message send_login = new message();
        send_login.pt_id = PROTOCOL.LOGIN_Request;
        login_info login_account = new login_info();
        login_account.Email = GameObject.Find("Canvas/InputID").GetComponent<TMP_InputField>().text;
        login_account.PW = GameObject.Find("Canvas/InputPW").GetComponent<TMP_InputField>().text;
        send_login.signup_login_info = login_account;
        NetworkManager.Instance.SendData(send_login);
    }
}
