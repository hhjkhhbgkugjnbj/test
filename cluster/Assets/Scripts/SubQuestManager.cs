using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;

[System.Serializable]
public class sub_ScriptEntry
{
    public int speakerId;  // NPC 번호
    public string dialogue;  // 대사
}

[System.Serializable]
public class sub_NPC
{
    public int NPC_num;
    public int scene_num;
    public float x_position;
    public float y_position;
}

[System.Serializable]
public class sub_PointOfArrival
{
    public int scene;
    public float x_position;
    public float y_position;
}

[System.Serializable]
public class sub_Quest
{
    public string quest_name;
    public int sub_quest;
    public sub_NPC NPC;
    public sub_PointOfArrival point_of_arrival;
    public object item;  // 나중에 특정 아이템 클래스로 변경 가능
    public int minigame_num;  // 나중에 특정 미니게임 클래스로 변경 가능
    public List<sub_ScriptEntry> scripts;  // 변경된 부분
}

[System.Serializable]
public class sub_QuestList
{
    public List<sub_Quest> sub_quests;
}

public class SubQuestManager : MonoBehaviour
{
    public TextAsset jsonFile;
    public sub_QuestList sub_questList;

    public GameObject client_character;
    public GameObject sub_quest_process;

    public List<GameObject> sub_quest_position = new List<GameObject>();
    public List<string> sub_character = new List<string>();
    public GameObject Script_Object;

    public sub_Quest current_sub_quest;
    //public NPCManager npc_position_manager;
    public QuestManager minigame_manager;

    void Start()
    {
        sub_character.Add("haeyang");
        sub_character.Add("SubQuest");
        DontDestroyOnLoad(this);
        LoadQuests();
        sub_quest_position.Add(GameObject.Find("SubQuest/SubQuest_Start"));
        Debug.Log("SubQuestManager start");
        Script_Object.SetActive(false);
        sub_quest_process.SetActive(false);
        StartCoroutine(CheckDistanceToSubTarget());
    }
    IEnumerator CheckDistanceToSubTarget()
    {
        Debug.Log("SubQuestManager Coroutine start");
        while(true)
        {
            foreach(sub_Quest sub_q in sub_questList.sub_quests)
            {
                //Debug.Log(sub_quest_position[sub_q.NPC.NPC_num - 1].transform.position.x + ", " + sub_quest_position[sub_q.NPC.NPC_num - 1].transform.position.y);
                if(sub_q.NPC != null && sub_q.NPC.scene_num == NetworkManager.Instance.scene_num)
                {
                    Vector2 currentPosition = new Vector2(client_character.transform.position.x, client_character.transform.position.y);
                    Vector2 targetPosition = new Vector2(sub_quest_position[sub_q.NPC.NPC_num - 1].transform.position.x, sub_quest_position[sub_q.NPC.NPC_num - 1].transform.position.y);
                    float distanceToTarget = Vector2.Distance(currentPosition, targetPosition);
                    //Debug.Log(distanceToTarget);
                    if (distanceToTarget < 30.0)
                    {
                        GameObject.Find("Canvas/SubQuestStatus/CurQuest_Text").GetComponent<TMP_Text>().text = sub_q.quest_name;
                        current_sub_quest = sub_q;
                        if (script_process == false)
                        {
                            sub_quest_process.SetActive(true);
                        }
                        else
                        {
                            sub_quest_process.SetActive(false);
                        }
                    }
                    else
                    {
                        sub_quest_process.SetActive(false);
                        GameObject.Find("Canvas/SubQuestStatus/CurQuest_Text").GetComponent<TMP_Text>().text = "";
                    }
                }
                else if(sub_q.point_of_arrival != null && sub_q.point_of_arrival.scene == NetworkManager.Instance.scene_num)
                {
                    Vector2 currentPosition = new Vector2(client_character.transform.position.x, client_character.transform.position.y);
                    Vector2 targetPosition = new Vector2(sub_q.point_of_arrival.x_position, sub_q.point_of_arrival.y_position);
                    float distanceToTarget = Vector2.Distance(currentPosition, targetPosition);
                    //Debug.Log(distanceToTarget);
                    if (distanceToTarget < 30.0)
                    {
                        GameObject.Find("Canvas/SubQuestStatus/CurQuest_Text").GetComponent<TMP_Text>().text = sub_q.quest_name;
                        current_sub_quest = sub_q;
                        if (script_process == false)
                        {
                            sub_quest_process.SetActive(true);
                        }
                        else
                        {
                            sub_quest_process.SetActive(false);
                        }
                    }
                    else
                    {
                        sub_quest_process.SetActive(false);
                        GameObject.Find("Canvas/SubQuestStatus/CurQuest_Text").GetComponent<TMP_Text>().text = "";
                    }
                }
                else
                {
                    sub_quest_process.SetActive(false);
                }
            }
            yield return new WaitForSeconds(0.05f);
        }
    }
    void Update()
    {
        lock (NetworkManager.Instance.sub_quest_message)
        {
            if (NetworkManager.Instance.sub_quest_message.Count != 0)
            {
                message new_queue_message = NetworkManager.Instance.sub_quest_message.Dequeue();
                if (new_queue_message.pt_id == PROTOCOL.Sub_Quest_End_Success)
                {
                    Debug.Log("Sub Quest End Success");
                    //씬 넘기기 new_queue_message.ingame_info.scene_num
                    string pre_scene_name = NetworkManager.Instance.search_scene_name(new_queue_message.ingame_info.scene_num);
                    SceneManager.LoadScene(pre_scene_name);
                }
            }
        }
    }
    void LoadQuests()
    {
        Debug.Log("subquests loaded");
        if (jsonFile == null)
        {
            Debug.LogError("JSON file is not assigned.");
            return;
        }

        // JSON 파일을 객체로 파싱
        sub_questList = JsonConvert.DeserializeObject<sub_QuestList>(jsonFile.text);
        Debug.Log(sub_questList.sub_quests);
        foreach (var quest in sub_questList.sub_quests)
        {
            Debug.Log("Quest Name: " + quest.quest_name);
        }
    }
    public void sub_quest_progress()
    {
        Script_Object.SetActive(true);
        next_script();
    }
    public int script_num = 0;
    public bool script_process = false;
    public void next_script()
    {
        if (script_num == current_sub_quest.scripts.Count)
        {
            script_process = false;
            script_num = 0;
            Script_Object.SetActive(false);
            //미니게임 시작지점 -> 퀘스트 json 파일에 미니게임 조건이 있다면 실행
            if (current_sub_quest.minigame_num != -1)
            {
                Debug.Log(current_sub_quest.minigame_num);
                //sub_quest_process.SetActive(true);
                //GameObject.Find("Canvas/Quest_State").GetComponent<TMP_Text>().text = "";
                SceneManager.LoadScene(minigame_manager.get_minigame_scene(current_sub_quest.minigame_num));
            }
            else
            {
                //send_quest_end();
                //서브 퀘스트가 미니게임 없는게 있다면 여기서 send_quest_end 신호
            }
        }
        else
        {
            script_process = true;
            foreach (var c in sub_character)
            {
                GameObject.Find("Canvas/SubScript/" + c).SetActive(false);
            }
            var script = current_sub_quest.scripts[script_num];
            GameObject.Find("Canvas/SubScript/" + sub_character[script.speakerId]).SetActive(true);

            string dialogueText = script.dialogue;
            string playerName = NetworkManager.Instance.nickname;  // 기본값은 "Player"
            dialogueText = dialogueText.Replace("{c}", playerName);

            GameObject.Find("Canvas/SubScript/Sub_Script_Text").GetComponent<TMP_Text>().text = dialogueText;
            script_num += 1;
        }
    }
}
