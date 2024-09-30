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

public class message
{
    public PROTOCOL pt_id { get; set; }
    public login_info signup_login_info { get; set; }
    public login_success_info first_login_info { get; set; }
    public InGame_message ingame_info { get; set; }
}

public class login_info
{
    public string Email { get; set; }
    public string PW { get; set; }
    public string Nickname { get; set; }
}

public class login_success_info
{
    public string Nickname { get; set; }
    public int scene_num { get; set; }
    public double x_position { get; set; }
    public double y_position { get; set; }
    public int semester { get; set; }
    public int main_quest_num { get; set; }
    public int detail_quest_num { get; set; }
    public int quest_state { get; set; }
    public List<int> minigame_nums { get; set; }
    public List<double> scores { get; set; }
}

public class InGame_message
{
    public double x_position { get; set; }
    public double y_position { get; set; }
    public int scene_num { get; set; }
    public int room_num { get; set; }
    public string target_nickname { get; set; }
    public string message { get; set; }
    public int semester { get; set; }
    public int main_quest_num { get; set; }
    public int detail_quest_num { get; set; }
    public int quest_state { get; set; }
    public string own_nickname { get; set; }
    public double sub_quest_score { get; set; }
    public int minigame_num { get; set; }
}

public class NetworkManager : MonoBehaviour
{
    private static NetworkManager _instance;
    private Socket _socket;
    private SocketAsyncEventArgs _receiveEventArgs;
    private byte[] _receiveBuffer;
    public bool IsConnected = false;
    public PROTOCOL current_pt_id = PROTOCOL.Setting;

    public Queue<message> every_messages = new Queue<message>();
    public Queue<message> signup_messages = new Queue<message>();
    public Queue<message> login_messages = new Queue<message>();
    public Queue<message> chat_messages = new Queue<message>();
    public Queue<message> other_users = new Queue<message>();
    public Queue<message> quest_message = new Queue<message>();
    public Queue<message> sub_quest_message = new Queue<message>();

    public Queue<message> send_message = new Queue<message>();
    public int scene_num;
    public int last_scene_num;
    public string nickname;

    public int main_quest_num;
    public int detail_quest_num;
    public int semester;
    public int quest_state;
    Dictionary<string, int> scene_name_to_num = new Dictionary<string, int>();
    public bool game_process = false;

    public List<int> minigame_nums = new List<int>();
    public List<double> scores = new List<double>();
    public float last_x_position;
    public float last_y_position;

    public static NetworkManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("NetworkManager");
                _instance = go.AddComponent<NetworkManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }
    public string search_scene_name(int targetValue)
    {
        string key = scene_name_to_num.FirstOrDefault(x => x.Value == targetValue).Key;
        return key;
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
        scene_name_to_num.Add("ExampleScene", 2);
        scene_name_to_num.Add("MiniGame", 3);
        // 씬 전환 이벤트 핸들러 등록
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // 씬 전환 이벤트 핸들러 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene_name_to_num.TryGetValue(scene.name, out int sceneNumber))
        {
            scene_num = sceneNumber;
            Debug.Log("새로운 씬 로드됨: " + scene.name + " - Scene Num: " + scene_num);
        }
        else
        {
            Debug.LogWarning("씬 이름이 딕셔너리에 없음: " + scene.name);
        }
    }

    public void Connect(string ipAddress, int port)
    {
        try
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Connect(ipAddress, port);
            IsConnected = true;
            Debug.Log("Connected to server.");

            // 비동기 수신 설정
            _receiveBuffer = new byte[1024];
            _receiveEventArgs = new SocketAsyncEventArgs();
            _receiveEventArgs.SetBuffer(_receiveBuffer, 0, _receiveBuffer.Length);
            _receiveEventArgs.Completed += OnReceiveCompleted;

            StartReceive();
        }
        catch (Exception ex)
        {
            Debug.Log("Connection error: " + ex.Message);
        }
    }

    private void StartReceive()
    {
        game_process = true;
        if (_socket != null && _socket.Connected)
        {
            bool pending = _socket.ReceiveAsync(_receiveEventArgs);
            if (!pending)
            {
                OnReceiveCompleted(this, _receiveEventArgs);
            }
        }
    }

    private int _expectedMessageLength = -1; // 수신할 메시지의 예상 길이 (-1은 아직 메시지 길이를 받지 않았다는 뜻)
    private List<byte> _receivedDataBuffer = new List<byte>(); // 수신한 데이터를 저장하는 버퍼

    private void OnReceiveCompleted(object sender, SocketAsyncEventArgs e)
    {
        if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
        {
            try
            {
                byte[] receivedData = new byte[e.BytesTransferred];
                Array.Copy(e.Buffer, 0, receivedData, 0, e.BytesTransferred);
                _receivedDataBuffer.AddRange(receivedData);

                while (_receivedDataBuffer.Count > 0)
                {
                    // 아직 메시지 길이를 읽지 않았다면, 길이부터 읽는다.
                    if (_expectedMessageLength == -1 && _receivedDataBuffer.Count >= 4)
                    {
                        _expectedMessageLength = BitConverter.ToInt32(_receivedDataBuffer.GetRange(0, 4).ToArray(), 0);
                        _receivedDataBuffer.RemoveRange(0, 4); // 길이 데이터를 버퍼에서 제거
                    }

                    // 메시지 길이를 알고 있고, 충분한 데이터가 수신되었을 때 메시지를 처리한다.
                    if (_expectedMessageLength != -1 && _receivedDataBuffer.Count >= _expectedMessageLength)
                    {
                        byte[] completeMessageData = _receivedDataBuffer.GetRange(0, _expectedMessageLength).ToArray();
                        _receivedDataBuffer.RemoveRange(0, _expectedMessageLength);

                        string receivedJson = Encoding.UTF8.GetString(completeMessageData);
                        message receivedInfo = JsonConvert.DeserializeObject<message>(receivedJson);

                        every_messages.Enqueue(receivedInfo);
                        Debug.Log(receivedJson);

                        _expectedMessageLength = -1;
                    }
                    else
                    {
                        break; // 메시지가 완전하지 않다면 루프를 빠져나간다.
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Error handling receive: " + ex.Message);
                ResetAndReturnToInitialState();
            }

            // 다음 수신 시작
            StartReceive();
        }
        else if (e.SocketError == SocketError.OperationAborted)
        {
            Debug.LogWarning("Receive operation was aborted.");
        }
        else
        {
            Debug.Log("Receive error: " + e.SocketError.ToString());
            ResetAndReturnToInitialState();
        }
    }

    private void Update()
    {
        lock (every_messages)
        {
            if (every_messages.Count > 0)
            {
                message new_message = every_messages.Dequeue();
                Debug.Log(new_message.pt_id);
                if (current_pt_id == PROTOCOL.SIGNUP_Request && (new_message.pt_id == PROTOCOL.SIGNUP_Success || new_message.pt_id == PROTOCOL.SIGNUP_Fail))
                {
                    signup_messages.Enqueue(new_message);
                }
                else if (current_pt_id == PROTOCOL.LOGIN_Request && (new_message.pt_id == PROTOCOL.LOGIN_Success || new_message.pt_id == PROTOCOL.LOGIN_Fail))
                {
                    Debug.Log("receive login success");
                    login_messages.Enqueue(new_message);
                }
                else if (new_message.pt_id == PROTOCOL.Deliver_Message)
                {
                    chat_messages.Enqueue(new_message);
                }
                else if (new_message.pt_id == PROTOCOL.Deliver_Position || new_message.pt_id == PROTOCOL.Delete_User)
                {
                    if (new_message.first_login_info.scene_num == scene_num)
                    {
                        if (new_message.pt_id == PROTOCOL.Delete_User) Debug.Log("Delete User");
                        other_users.Enqueue(new_message);
                    }
                }
                else if ((current_pt_id == PROTOCOL.Quest_Start_Request && new_message.pt_id == PROTOCOL.Quest_Start_Success) || (current_pt_id == PROTOCOL.Quest_Complete_Request && new_message.pt_id == PROTOCOL.Quest_Complete_Success) || (current_pt_id == PROTOCOL.MiniGame_End_Request && new_message.pt_id == PROTOCOL.MiniGame_End_Success))
                {
                    quest_message.Enqueue(new_message);
                }
                else if (current_pt_id == PROTOCOL.Sub_Quest_End_Request && new_message.pt_id == PROTOCOL.Sub_Quest_End_Success)
                {
                    sub_quest_message.Enqueue(new_message);
                }
            }
        }

        lock (send_message)
        {
            if (send_message.Count > 0)
            {
                if (_socket != null && IsConnected)
                {
                    try
                    {
                        message send_new_message = send_message.Dequeue();
                        if (send_new_message.pt_id != PROTOCOL.Position_Update)
                            current_pt_id = send_new_message.pt_id;

                        // 메시지를 JSON으로 직렬화
                        string jsonMessage = JsonConvert.SerializeObject(send_new_message);
                        byte[] messageBuffer = Encoding.UTF8.GetBytes(jsonMessage);

                        // 메시지의 길이를 계산하고, 이를 바이트 배열로 변환
                        int messageLength = messageBuffer.Length;
                        byte[] lengthBuffer = BitConverter.GetBytes(messageLength);

                        // 길이 정보와 메시지를 하나의 배열로 결합
                        byte[] combinedBuffer = new byte[lengthBuffer.Length + messageBuffer.Length];
                        Buffer.BlockCopy(lengthBuffer, 0, combinedBuffer, 0, lengthBuffer.Length);
                        Buffer.BlockCopy(messageBuffer, 0, combinedBuffer, lengthBuffer.Length, messageBuffer.Length);

                        if(send_new_message.pt_id == PROTOCOL.LOGIN_Request || send_new_message.pt_id == PROTOCOL.SIGNUP_Request)
                        {
                            _socket.Send(messageBuffer);
                        }
                        else
                        {
                            _socket.Send(combinedBuffer);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.Log("Send error: " + ex.Message);
                        // ResetAndReturnToInitialState();
                    }
                }
                else
                {
                    Debug.Log("Socket is not connected. Unable to send data.");
                    ResetAndReturnToInitialState();
                }
            }
        }
    }

    public void SendData(message data)
    {
        send_message.Enqueue(data);
    }

    void OnApplicationQuit()
    {
        CloseSocket();
    }

    public void CloseSocket()
    {
        if (_socket != null && _socket.Connected)
        {
            try
            {
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
                IsConnected = false;
                Debug.Log("Socket closed.");
            }
            catch (Exception ex)
            {
                Debug.Log("Error closing socket: " + ex.Message);
                ResetAndReturnToInitialState();
            }
        }
    }
    private void ResetAndReturnToInitialState()
    {
        ClearMessageQueues();
        if (_socket != null) CloseSocket();
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            Destroy(obj);
        }
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            SceneManager.UnloadSceneAsync(scene);
        }
        SceneManager.LoadScene("SampleScene");
    }
    private void ClearMessageQueues()
    {
        lock (every_messages)
        {
            every_messages.Clear();
        }

        lock (send_message)
        {
            send_message.Clear();
        }

        lock (signup_messages)
        {
            signup_messages.Clear();
        }

        lock (login_messages)
        {
            login_messages.Clear();
        }

        lock (chat_messages)
        {
            chat_messages.Clear();
        }

        lock (other_users)
        {
            other_users.Clear();
        }

        lock (quest_message)
        {
            quest_message.Clear();
        }

        lock (sub_quest_message)
        {
            sub_quest_message.Clear();
        }
    }

}
