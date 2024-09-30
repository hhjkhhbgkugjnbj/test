using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Connect_Server : MonoBehaviour
{
    public GameObject reconnect_alarm;
    public float timeoutDuration = 1f; // 타임아웃 시간 설정 (초 단위)
    public TMP_InputField server_address;

    public void Start()
    {
        Debug.Log("Start");
        if (NetworkManager.Instance.game_process == true)
        {
            reconnect_alarm.GetComponent<TMP_Text>().text = "Lost connection to server. \nPlease retry to connect.";
            reconnect_alarm.SetActive(true);
            //DestroyDontDestroyObjects();
        }
    }
    private void DestroyDontDestroyObjects()
    {
        // DontDestroyOnLoad로 설정된 모든 객체를 삭제합니다.
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (obj.scene.name == null) // `DontDestroyOnLoad`로 설정된 객체는 `scene.name`이 null입니다.
            {
                Destroy(obj);
            }
        }
    }
    public void connect_start()
    {
        // 네트워크 연결 시도
        NetworkManager.Instance.Connect(server_address.text, 7979);

        // 코루틴 실행
        StartCoroutine(ConnectWithTimeout());
    }

    private IEnumerator ConnectWithTimeout()
    {
        float elapsedTime = 0f;

        // 타임아웃 시간 동안 주기적으로 연결 상태 확인
        while (elapsedTime < timeoutDuration)
        {
            if (NetworkManager.Instance.IsConnected)
            {
                SceneManager.LoadScene("Title");
                yield break;
            }

            elapsedTime += 0.5f; // 0.5초 간격으로 확인
            yield return new WaitForSeconds(0.5f);
        }

        reconnect_alarm.GetComponent<TMP_Text>().text = "You fail to connect to server. \nPlease check address and try again.";
        reconnect_alarm.SetActive(true);
    }
}
