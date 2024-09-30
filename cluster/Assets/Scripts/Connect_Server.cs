using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Connect_Server : MonoBehaviour
{
    public GameObject reconnect_alarm;
    public float timeoutDuration = 1f; // Ÿ�Ӿƿ� �ð� ���� (�� ����)
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
        // DontDestroyOnLoad�� ������ ��� ��ü�� �����մϴ�.
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (obj.scene.name == null) // `DontDestroyOnLoad`�� ������ ��ü�� `scene.name`�� null�Դϴ�.
            {
                Destroy(obj);
            }
        }
    }
    public void connect_start()
    {
        // ��Ʈ��ũ ���� �õ�
        NetworkManager.Instance.Connect(server_address.text, 7979);

        // �ڷ�ƾ ����
        StartCoroutine(ConnectWithTimeout());
    }

    private IEnumerator ConnectWithTimeout()
    {
        float elapsedTime = 0f;

        // Ÿ�Ӿƿ� �ð� ���� �ֱ������� ���� ���� Ȯ��
        while (elapsedTime < timeoutDuration)
        {
            if (NetworkManager.Instance.IsConnected)
            {
                SceneManager.LoadScene("Title");
                yield break;
            }

            elapsedTime += 0.5f; // 0.5�� �������� Ȯ��
            yield return new WaitForSeconds(0.5f);
        }

        reconnect_alarm.GetComponent<TMP_Text>().text = "You fail to connect to server. \nPlease check address and try again.";
        reconnect_alarm.SetActive(true);
    }
}
