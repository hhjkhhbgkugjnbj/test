using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NPCManager : MonoBehaviour
{
    class NPC_info
    {
        public GameObject Npc_Object;
        public int scene_num;
        public float x_position;
        public float y_position;
    }
    List<NPC_info> NPC_list = new List<NPC_info>();
    public int scene_num;

    Dictionary<string, int> scene_name_to_num = new Dictionary<string, int>();
    private void Start()
    {
        DontDestroyOnLoad(this);
        DontDestroyOnLoad(GameObject.Find("Anchor_Top"));
        DontDestroyOnLoad(GameObject.Find("Kim"));
        DontDestroyOnLoad(GameObject.Find("Kang"));
        DontDestroyOnLoad(GameObject.Find("SubQuest"));

        NPC_info anchor_top = new NPC_info();
        anchor_top.Npc_Object = GameObject.Find("Anchor_Top");
        anchor_top.scene_num = 0;
        anchor_top.x_position = -2885;
        anchor_top.y_position = 3501;
        NPC_list.Add(anchor_top);

        NPC_info Kim = new NPC_info();
        Kim.Npc_Object = GameObject.Find("Kim");
        Kim.scene_num = 1;
        Kim.x_position = -3588;
        Kim.y_position = 4372;
        NPC_list.Add(Kim);

        NPC_info Kang = new NPC_info();
        Kang.Npc_Object = GameObject.Find("Kang");
        Kang.scene_num = 1;
        Kang.x_position = -4670;
        Kang.y_position = 4194;
        NPC_list.Add(Kang);

        NPC_info SubQuest = new NPC_info();
        SubQuest.Npc_Object = GameObject.Find("SubQuest");
        SubQuest.scene_num = 1;
        SubQuest.x_position = -3913;
        SubQuest.y_position = 4446;
        NPC_list.Add(SubQuest);
    }
    private void Awake()
    {
        scene_name_to_num.Add("Bridge", 0);
        scene_name_to_num.Add("Bridge 1", 1);
        scene_name_to_num.Add("Waiting_Room", -1);
        scene_name_to_num.Add("Title", -1);
        scene_name_to_num.Add("SignUp", -1);
        scene_name_to_num.Add("SampleScene", -1);
        scene_name_to_num.Add("Login", -1);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDestroy()
    {
        // ¾À ÀüÈ¯ ÀÌº¥Æ® ÇÚµé·¯ ÇØÁ¦
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene_name_to_num.TryGetValue(scene.name, out int sceneNumber))
        {
            scene_num = sceneNumber;
            Debug.Log("»õ·Î¿î ¾À ·ÎµåµÊ: " + scene.name + " - Scene Num: " + scene_num);
            foreach(NPC_info npc in NPC_list)
            {
                if (npc.scene_num == scene_num) npc.Npc_Object.SetActive(true);
                else npc.Npc_Object.SetActive(false);
            }
        }
        else
        {
            Debug.LogWarning("¾À ÀÌ¸§ÀÌ µñ¼Å³Ê¸®¿¡ ¾øÀ½: " + scene.name);
        }
    }
}
