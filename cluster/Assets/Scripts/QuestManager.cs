using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;

[System.Serializable]
public class ScriptEntry
{
    public int speakerId;  // NPC 번호
    public string dialogue;  // 대사
}

[System.Serializable]
public class NPC
{
    public int NPC_num;
    public int scene_num;
    public float x_position;
    public float y_position;
}

[System.Serializable]
public class PointOfArrival
{
    public int scene;
    public float x_position;
    public float y_position;
}

[System.Serializable]
public class Quest
{
    public string quest_name;
    public int main_quest;
    public int detail_quest;
    public NPC NPC;
    public PointOfArrival point_of_arrival;
    public object item;  // 나중에 특정 아이템 클래스로 변경 가능
    public int minigame_num;  // 나중에 특정 미니게임 클래스로 변경 가능
    public List<ScriptEntry> scripts;  // 변경된 부분
}

[System.Serializable]
public class QuestList
{
    public List<Quest> quests;
}

public class QuestManager : MonoBehaviour
{
    public GameObject Quest_Progress;
    public GameObject client_character;
    public GameObject Script_Object;

    public NPCManager npc_manager_object;

    public Quest current_quest;

    public TextAsset jsonFile;  // 유니티 에디터에서 json 파일을 드래그 앤 드롭하세요
    public QuestList questList;

    public List<GameObject> quest_position = new List<GameObject>();
    public List<string> character = new List<string>();
    public List<string> minigame = new List<string>();

    public int before_minigame_scene_num;
    public Status status_manager;

    void Start()
    {
        character.Add("haeyang");
        character.Add("Anchor_Top");
        character.Add("Kim");
        character.Add("Kang");
        minigame.Add("ExampleScene");
        minigame.Add("MiniGame");
        DontDestroyOnLoad(this);
        Script_Object.SetActive(false);
        Quest_Progress.SetActive(false);
        LoadQuests();
        setting_quest_state();
        quest_position.Add(GameObject.Find("Anchor_Top/Anchor_Top_Start"));
        quest_position.Add(GameObject.Find("Kim/Kim_Start"));
        quest_position.Add(GameObject.Find("Kang/Kang_Start"));
        StartCoroutine(CheckDistanceToTarget());
    }

    IEnumerator CheckDistanceToTarget()
    {
        while (true)
        {
            if(current_quest != null)
            {
                if (current_quest.NPC != null && NetworkManager.Instance.quest_state == 1 && current_quest.NPC.scene_num == NetworkManager.Instance.scene_num)
                {
                    Vector2 currentPosition = new Vector2(client_character.transform.position.x, client_character.transform.position.y);
                    Vector2 targetPosition = new Vector2(quest_position[current_quest.NPC.NPC_num - 1].transform.position.x, quest_position[current_quest.NPC.NPC_num - 1].transform.position.y);
                    float distanceToTarget = Vector2.Distance(currentPosition, targetPosition);
                    //Debug.Log(distanceToTarget);
                    if (distanceToTarget < 30.0 && script_process == false)
                    {
                        Quest_Progress.SetActive(true);
                    }
                    else
                    {
                        Quest_Progress.SetActive(false);
                    }
                }
                else if (current_quest.point_of_arrival != null && NetworkManager.Instance.quest_state == 1 && current_quest.point_of_arrival.scene == NetworkManager.Instance.scene_num)
                {
                    Vector2 currentPosition = new Vector2(client_character.transform.position.x, client_character.transform.position.y);
                    Vector2 targetPosition = new Vector2(current_quest.point_of_arrival.x_position, current_quest.point_of_arrival.y_position);
                    float distanceToTarget = Vector2.Distance(currentPosition, targetPosition);
                    //Debug.Log(distanceToTarget);
                    if (distanceToTarget < 30.0 && script_process == false)
                    {
                        Quest_Progress.SetActive(true);
                    }
                    else
                    {
                        Quest_Progress.SetActive(false);
                    }
                }
            }
            yield return new WaitForSeconds(0.05f);
        }
    }

    void LoadQuests()
    {
        if (jsonFile == null)
        {
            Debug.LogError("JSON file is not assigned.");
            return;
        }

        // JSON 파일을 객체로 파싱
        questList = JsonConvert.DeserializeObject<QuestList>(jsonFile.text);

        foreach (var quest in questList.quests)
        {
            //Debug.Log("Quest Name: " + quest.quest_name);
            // 필요한 추가 작업 수행
        }
    }

    void Update()
    {
        lock (NetworkManager.Instance.quest_message)
        {
            if (NetworkManager.Instance.quest_message.Count != 0)
            {
                message new_queue_message = NetworkManager.Instance.quest_message.Dequeue();
                if (new_queue_message.pt_id == PROTOCOL.Quest_Start_Success)
                {
                    Debug.Log("Quest Start Success");
                    GameObject.Find("Canvas/Quest_State").GetComponent<TMP_Text>().text = "Server Access Quest Start >> (" + new_queue_message.ingame_info.main_quest_num + ", " + new_queue_message.ingame_info.detail_quest_num + ")";
                    //Quest_Start_Send.SetActive(false);
                    Quest_Progress.SetActive(false);
                    NetworkManager.Instance.quest_state = 1;
                    NetworkManager.Instance.main_quest_num = new_queue_message.ingame_info.main_quest_num;
                    NetworkManager.Instance.detail_quest_num = new_queue_message.ingame_info.detail_quest_num;
                    NetworkManager.Instance.semester = new_queue_message.ingame_info.semester;
                }
                else if (new_queue_message.pt_id == PROTOCOL.Quest_Complete_Success)
                {
                    Debug.Log("Quest Complete Success");
                    GameObject.Find("Canvas/Quest_State").GetComponent<TMP_Text>().text = "Server Access Quest Complete >> (" + NetworkManager.Instance.main_quest_num + ", " + NetworkManager.Instance.detail_quest_num + ")";
                    //Quest_Start_Send.SetActive(true);
                    Quest_Progress.SetActive(false);
                    NetworkManager.Instance.quest_state = 0;

                    NetworkManager.Instance.main_quest_num = new_queue_message.ingame_info.main_quest_num;
                    NetworkManager.Instance.detail_quest_num = new_queue_message.ingame_info.detail_quest_num;
                    NetworkManager.Instance.semester = new_queue_message.ingame_info.semester;

                    setting_quest_info();
                    send_quest_start();
                }
                else if(new_queue_message.pt_id == PROTOCOL.MiniGame_End_Success)
                {
                    Debug.Log("MiniGame End Success");
                    //씬 넘기기 new_queue_message.ingame_info.scene_num
                    string pre_scene_name = NetworkManager.Instance.search_scene_name(new_queue_message.ingame_info.scene_num);
                    SceneManager.LoadScene(pre_scene_name);
                    send_quest_end();
                }
            }
        }
    }

    public void send_quest_start()
    {
        Debug.Log("send quest start");
        message message_send_info = new message();
        message_send_info.pt_id = PROTOCOL.Quest_Start_Request;
        InGame_message message_info = new InGame_message();
        message_info.semester = NetworkManager.Instance.semester;
        message_info.main_quest_num = NetworkManager.Instance.main_quest_num;
        message_info.detail_quest_num = NetworkManager.Instance.detail_quest_num;
        message_send_info.ingame_info = message_info;
        NetworkManager.Instance.SendData(message_send_info);
    }
    public string get_minigame_scene(int minigame_num)
    {
        return minigame[minigame_num];
    }
    public void quest_progress()
    {
        Script_Object.SetActive(true);
        next_script();
    }
    public int script_num=0;
    public bool script_process = false;
    public void next_script()
    {
        if (script_num == current_quest.scripts.Count)
        {
            script_process = false;
            script_num = 0;
            Script_Object.SetActive(false);
            //미니게임 시작지점 -> 퀘스트 json 파일에 미니게임 조건이 있다면 실행
            if (current_quest.minigame_num != -1)
            {
                Debug.Log(current_quest.minigame_num);
                Quest_Progress.SetActive(false);
                GameObject.Find("Canvas/Quest_State").GetComponent<TMP_Text>().text = "";
                SceneManager.LoadScene(minigame[current_quest.minigame_num]);
            }
            else
            {
                send_quest_end();
            }
        }
        else
        {
            script_process = true;
            foreach (var c in character)
            {
                GameObject.Find("Canvas/Script/" + c).SetActive(false);
            }
            var script = current_quest.scripts[script_num];
            GameObject.Find("Canvas/Script/" + character[script.speakerId]).SetActive(true);

            string dialogueText = script.dialogue;
            string playerName = NetworkManager.Instance.nickname;  // 기본값은 "Player"
            dialogueText = dialogueText.Replace("{c}", playerName);

            GameObject.Find("Canvas/Script/Script_Text").GetComponent<TMP_Text>().text = dialogueText;
            script_num += 1;
        }
    }
    public void send_quest_end()
    {
        Debug.Log("send quest complete" + ", " + NetworkManager.Instance.detail_quest_num + ", " + questList.quests.Count);
        if(NetworkManager.Instance.detail_quest_num >= questList.quests.Count-1)
        {
            message message_send_info = new message();
            message_send_info.pt_id = PROTOCOL.Quest_Complete_Request;
            InGame_message message_info = new InGame_message();
            message_info.semester = NetworkManager.Instance.semester+1;
            message_info.main_quest_num = NetworkManager.Instance.main_quest_num+1;
            message_info.detail_quest_num = 0;
            message_send_info.ingame_info = message_info;
            NetworkManager.Instance.SendData(message_send_info);
        }
        else
        {
            message message_send_info = new message();
            message_send_info.pt_id = PROTOCOL.Quest_Complete_Request;
            InGame_message message_info = new InGame_message();
            message_info.semester = NetworkManager.Instance.semester;
            message_info.main_quest_num = NetworkManager.Instance.main_quest_num;
            message_info.detail_quest_num = NetworkManager.Instance.detail_quest_num+1;
            message_send_info.ingame_info = message_info;
            NetworkManager.Instance.SendData(message_send_info);
        }
    }

    public void setting_quest_state()
    {
        if (NetworkManager.Instance.quest_state == 1)
        {
            GameObject.Find("Canvas/Quest_State").GetComponent<TMP_Text>().text = "Server Access Quest Start >> (" + NetworkManager.Instance.main_quest_num + ", " + NetworkManager.Instance.detail_quest_num + ")";
            Quest_Progress.SetActive(false);
            //Quest_Start_Send.SetActive(false);
        }
        else
        {
            //Quest_Start_Send.SetActive(true);
            Quest_Progress.SetActive(false);
            send_quest_start();
        }
        setting_quest_info();
    }

    public void setting_quest_info()
    {
        bool isquest = false;
        foreach (var q in questList.quests)
        {
            if (q.main_quest == NetworkManager.Instance.main_quest_num && q.detail_quest == NetworkManager.Instance.detail_quest_num)
            {
                isquest = true;
                current_quest = q;
            }
        }
        if(isquest)
        {
            foreach (var script in current_quest.scripts)
            {
                Debug.Log($"NPC {script.speakerId}: {script.dialogue ?? "No dialogue"}");
            }
            GameObject.Find("Canvas/QuestStatus/CurQuest_Text").GetComponent<TMP_Text>().text = current_quest.quest_name;

            Debug.Log(current_quest.quest_name);
            status_manager.UpdateQuestProgress(NetworkManager.Instance.detail_quest_num);
        }
        else
        {
            GameObject.Find("Canvas/QuestStatus/CurQuest_Text").GetComponent<TMP_Text>().text = "";
            //Quest_Start_Send.SetActive(false);
            status_manager.UpdateQuestProgress(6);  //완료시 일단 "6/6" 상태에서 멈춰있도록 임의로 조정
            current_quest = null;
        }
    }
}