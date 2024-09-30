using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager i;
    Text timeT;
    public GameObject bestT;
    public GameObject previous_bestT;
    public GameObject wait_seconds;

    float t = 0;

    bool isGameOver = false;
    bool isGameStart = false;

    public ObstacleSpawnComponent computer_architecture;
    public ObstacleSpawnComponent network_security;
    public ObstacleSpawnComponent maritime_data_comunication;

    public bool IsGameOver => isGameOver;
    public string client_nickname;

    void Start()
    {
        client_nickname = NetworkManager.Instance.nickname;
        i = this;
        timeT = GameObject.Find("Canvas/TimeText").GetComponent<Text>();
        if(NetworkManager.Instance.scores.Count != 0) previous_bestT.GetComponent<Text>().text = "Your Best: " + (float)NetworkManager.Instance.scores[0];
        else previous_bestT.GetComponent<Text>().text = "Your Best: " + 0;
        StartCoroutine(StartGameAfterDelay());
    }
    private IEnumerator StartGameAfterDelay()
    {
        for (int i = 3; i > 0; i--)
        {
            wait_seconds.GetComponent<Text>().text = i + "초";  // "3초", "2초", "1초" 순으로 표시
            yield return new WaitForSeconds(1.0f);  // 1초 대기
        }
        previous_bestT.SetActive(false);
        wait_seconds.GetComponent<Text>().text = "Start";
        yield return new WaitForSeconds(0.5f);
        wait_seconds.SetActive(false);

        computer_architecture.ObstacleSpawnStart();
        computer_architecture.spawnname = "computer architecture";
        network_security.ObstacleSpawnStart();
        network_security.spawnname = "network security";
        maritime_data_comunication.ObstacleSpawnStart();
        maritime_data_comunication.spawnname = "maritime data communication";
        isGameStart = true;
    }

    void Update()
    {
        if (isGameOver) return;

        if(isGameStart)
        {
            t += Time.deltaTime;
            timeT.text = "TIME\n" + SetTime((int)t);
        }
    }

    string SetTime(int t)
    {
        string min = (t / 60).ToString();

        if (int.Parse(min) < 10) min = "0" + min;

        string sec = (t % 60).ToString();

        if(int.Parse(sec) < 10) sec = "0" + sec;

        return min + ":" + sec;
    }

    public void GameOver()
    {
        Debug.Log("GameManager gameover");
        isGameOver = true;
        SetBestTime();
    }

    void SetBestTime()
    {
        Debug.Log("set best time");
        
        bestT.GetComponent<Text>().text = "Record: " + SetTime((int)t);
        bestT.SetActive(true);

        Destroy(computer_architecture);
        Destroy(network_security);
        Destroy(maritime_data_comunication);

        Debug.Log("record >> " + (float)t);
        if (NetworkManager.Instance.scores.Count == 0) NetworkManager.Instance.scores.Add((double)t);
        else
        {
            if (NetworkManager.Instance.scores[0] < (double)t) NetworkManager.Instance.scores[0] = (double)t;
        }

        StartCoroutine(EndGameAfterDelay());
    }
    private IEnumerator EndGameAfterDelay()
    {
        yield return new WaitForSeconds(1.0f);

        message position_update_info = new message();
        position_update_info.pt_id = PROTOCOL.Sub_Quest_End_Request;
        InGame_message minigame_end = new InGame_message();
        minigame_end.sub_quest_score = (double)t;
        minigame_end.minigame_num = 1;
        position_update_info.ingame_info = minigame_end;
        NetworkManager.Instance.SendData(position_update_info);
    }
}