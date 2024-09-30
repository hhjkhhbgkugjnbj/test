using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Status : MonoBehaviour
{
    public float curQuest; // 현재 퀘스트 진행도
    public float maxQuest = 6; // 최대 퀘스트 수
    public Slider QuestBarSlider; // 퀘스트 진행도를 나타낼 슬라이더
    public Text QuestBarText; // 퀘스트 진행도를 표시할 텍스트

    private void Start()
    {
        DontDestroyOnLoad(this);
    }

    public void SetQuest(float maxAmount) { // 퀘스트바 최대값 설정
        maxQuest = maxAmount;
        curQuest = 0; // 현재 퀘스트 진행도 0으로 초기화
        if (QuestBarSlider != null) {
            QuestBarSlider.value = curQuest / maxQuest; // 슬라이더 현재값 업뎃
        }
        UpdateQuestDisplay();
    }

    public void UpdateQuestProgress(float progress) { // 퀘스트 진행도 업뎃
        curQuest = Mathf.Clamp(progress, 0, maxQuest); // 진행도가 최대값 초과하지 않도록 제한
        if (QuestBarSlider != null) {
            QuestBarSlider.value = curQuest / maxQuest; // 슬라이더 값 업뎃
        }
        UpdateQuestDisplay();
    }

    public void CheckQuest() { // 퀘스트 진행률 갱신
        if (NetworkManager.Instance.quest_message.Count > 0) {
            message new_queue_message = NetworkManager.Instance.quest_message.Dequeue();
            if (new_queue_message.pt_id == PROTOCOL.Quest_Complete_Success) {
                // 퀘스트 완료 메시지 처리하여 진행도 업뎃
                UpdateQuestProgress(curQuest + 1); // 완료 시, 현재 퀘스트 진행도 1 증가
            }
        }
    }

    private void UpdateQuestDisplay() {
        if (QuestBarText != null) {
            QuestBarText.text = $"{curQuest} / {maxQuest}"; // (현재 퀘스트 진행도 / 최대 퀘스트 수) 텍스트로 표시
        }
    }
}