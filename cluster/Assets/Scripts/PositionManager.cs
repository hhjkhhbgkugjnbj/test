using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PositionManager : MonoBehaviour
{
    float character_x_position;
    float character_y_position;
    string owner_nickname;

    //public GameObject go_to_next_scene;
    public Vector2 targetPosition = new Vector2(-3918, 4060);

    public GameObject keep_info;
    int st=-1;

    public NPCManager npc_manager_object;
    private void Start()
    {
        targetPosition = new Vector2(-3918, 4060);
        DontDestroyOnLoad(keep_info);
        owner_nickname = NetworkManager.Instance.nickname;
        Debug.Log(owner_nickname + ": " + NetworkManager.Instance.scene_num + ">> " + NetworkManager.Instance.last_x_position + ", " + NetworkManager.Instance.last_y_position);

        character_x_position = NetworkManager.Instance.last_x_position;
        character_y_position = NetworkManager.Instance.last_y_position;
        this.transform.position = new Vector2(character_x_position, character_y_position);

        StartCoroutine(UpdateSendPosition());
        //StartCoroutine(CheckDistanceToTarget());
    }

    IEnumerator UpdateSendPosition()
    {
        while (true)
        {
            if(NetworkManager.Instance.scene_num == 0 || NetworkManager.Instance.scene_num == 1)
            {
                position_info_send();
            }
            yield return new WaitForSeconds(0.25f);
        }
    }

    //IEnumerator CheckDistanceToTarget()
    //{
    //    while (true)
    //    {
    //        Vector2 currentPosition = new Vector2(this.transform.position.x, this.transform.position.y);
    //        float distanceToTarget = Vector2.Distance(currentPosition, targetPosition);
    //        //Debug.Log("Distance to Target: " + distanceToTarget);

    //        switch(NetworkManager.Instance.scene_num)
    //        {
    //            case 0:
    //                if (distanceToTarget < 25.0)
    //                {
    //                    go_to_next_scene.SetActive(true);
    //                    st = 0;
    //                }
    //                else
    //                {
    //                    go_to_next_scene.SetActive(false);
    //                    st = -1;
    //                }
    //                break;
    //            case 1:
    //                if (distanceToTarget < 25.0)
    //                {
    //                    go_to_next_scene.SetActive(true);
    //                    st = 1;
    //                }
    //                else
    //                {
    //                    go_to_next_scene.SetActive(false);
    //                    st = -1;
    //                }
    //                break;
    //            default:
    //                break;
    //        }
    //        yield return new WaitForSeconds(0.05f);
    //    }
    //}

    public void position_info_send()
    {
        message position_update_info = new message();
        position_update_info.pt_id = PROTOCOL.Position_Update;
        InGame_message ingame_position_info = new InGame_message();
        ingame_position_info.own_nickname = owner_nickname;
        if (NetworkManager.Instance.scene_num == -1) ingame_position_info.scene_num = 0;
        else ingame_position_info.scene_num = NetworkManager.Instance.scene_num;
        ingame_position_info.x_position = this.transform.position.x;
        ingame_position_info.y_position = this.transform.position.y;
        position_update_info.ingame_info = ingame_position_info;
        NetworkManager.Instance.SendData(position_update_info);
    }

    public void change_scene()
    {
        switch(st)
        {
            case 0:
                SceneManager.LoadScene("Bridge 1");
                break;
            case 1:
                SceneManager.LoadScene("Bridge");
                break;
            default:
                break;
        }
    }
}

