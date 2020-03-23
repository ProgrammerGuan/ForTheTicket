using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MainGame : MonoBehaviour
{
    public GameObject prefab;
    CharacterController Controller;
    Dictionary<string, Character> PlayerList;
    Vector3 myPos;
    List<GameObject> PlayerNames;
    Button LoginButton;
    string myName;
    bool startGameFlag;
    WsClient client;
    Stack<Message> messages = new Stack<Message>();
    bool sendIdle = false;
    // Start is called before the first frame update
    private void Awake()
    {
        PlayerNames = new List<GameObject>();
        Controller = new CharacterController();
        PlayerList = new Dictionary<string, Character>();
        LoginButton = GameObject.Find("LoginButton").GetComponent<Button>();
        LoginButton.onClick.AddListener(Login);
        startGameFlag = false;
    }

    void Start()
    {
        client = new WsClient("ws://192.168.8.53:4000");
        client.OnMessage = onMessage;
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerList.Count > 0)
        {
            var ctrl = Controller.GetControl();
            PlayerList[myName].Action(ctrl);
            if (myPos != PlayerList[myName].transform.position)
            {
                if (!sendIdle) sendIdle = true;
                var actMessage = new PlayerActionMessage();
                var act = new PlayerAction();
                act.Name = myName;
                act.Control = ctrl;
                act.X = PlayerList[myName].transform.position.x;
                act.Y = PlayerList[myName].transform.position.y;
                act.Turn = PlayerList[myName].TurnFlag;
                actMessage.Data = act;
                Send(Message.Act, actMessage);
                myPos = PlayerList[myName].transform.position;
            }
            else if (sendIdle)
            {
                sendIdle = false;
                var actMessage = new PlayerActionMessage();
                var act = new PlayerAction();
                act.Name = myName;
                act.Control = ctrl;
                Debug.Log(ctrl);
                act.X = PlayerList[myName].transform.position.x;
                act.Y = PlayerList[myName].transform.position.y;
                act.Turn = PlayerList[myName].TurnFlag;
                actMessage.Data = act;
                Send(Message.Act, actMessage);
                myPos = PlayerList[myName].transform.position;
            }

        }
        foreach (var name_ui in PlayerNames)
        {
            name_ui.transform.position = RectTransformUtility.WorldToScreenPoint(Camera.main, PlayerList[name_ui.GetComponent<Text>().text].transform.position + Vector3.up * 0.5f);
        }
        if (messages.Count > 0)
        {
            var msg = messages.Pop();
            UpdateMessage(msg);
        }
        //if (startGameFlag)
        //    UpdateMyself();
        
    }

    public void MineGotDamage(string playerName)
    {
        var message = new PlayerGotDamageMessage();
        var data = new PlayerGotDamageData();
        data.Name = myName;
        message.Data = data;
        Send(Message.GotDamage,message);
    }

    public void CreatPlayer(string playerName,string characterName,float x,float y,bool Turn)
    {
        var newplayer = Instantiate(Resources.Load(string.Format("Prefabs/man", characterName)), new Vector3(x,y,0), Quaternion.identity);
        newplayer.name = playerName;
        var Character = new Character(this,playerName);
        PlayerList.Add(playerName, Character);
        var name_ui = Instantiate((GameObject)Resources.Load("Prefabs/PlayerName", typeof(GameObject)));
        name_ui.GetComponent<Text>().text = playerName;
        name_ui.transform.parent = GameObject.Find("Canvas").transform;
        PlayerNames.Add(name_ui);
    }

    public void Login()
    {
        LoginButton.interactable = GameObject.Find("NameInput").GetComponent<InputField>().interactable = false;
        myName = GameObject.Find("NameInput").transform.GetChild(2).GetComponent<Text>().text;
        startGameFlag = true;
        var data = new JoinMessage();
        var Data = new PlayerData();
        Data.Name = myName;
        Data.X = 0;
        Data.Y = 0;
        Data.Turn = false;
        data.Data = Data;
        Send(Message.Login, data);
    }
    
    void onMessage(Message msg)
    {
        messages.Push(msg);
    }

    public void Send(string type,object data)
    {
        client.SendMessage(type, data);
    }

    void UpdateMessage(Message msg)
    {
        //Debug.Log(messages.Count);
        switch (msg.Type)
        {
            case Message.Login:
                var d = msg.Data;
                Debug.Log(d.Length);
                var LoginData = JsonUtility.FromJson<LoginMessage>(msg.Data);
                foreach (var player in LoginData.Players)
                {
                    CreatPlayer(player.Name, "man", player.X, player.Y, player.Turn);
                }
                //Debug.Log("Login");
                break;
            case Message.Join:
                var JoinData = JsonUtility.FromJson<JoinMessage>(msg.Data);
                CreatPlayer(JoinData.Data.Name, "man", JoinData.Data.X, JoinData.Data.Y, JoinData.Data.Turn);
                //CreatPlayer(JoinData.Data.Name, "man", JoinData.Data.X, JoinData.Data.Y,JoinData.Data.Turn);
                //Debug.Log(JoinData.Data.Name + " Join");
                break;
            case Message.Act:
                var ActData = JsonUtility.FromJson<PlayerActionMessage>(msg.Data);
                PlayerList[ActData.Data.Name].Action(ActData.Data.Control);
                break;
            case Message.GotDamage:
                var DamageData = JsonUtility.FromJson<PlayerGotDamageMessage>(msg.Data);
                PlayerList[DamageData.Data.Name].GotDamage();
                break;
            //case Message.Move:
            //    var MoveData = JsonUtility.FromJson<PlayerData>(msg.Data);
            //    PlayerList[MoveData.Name].transform.position = new Vector3(MoveData.X, MoveData.Y, 0);
            //    if (!MoveData.Turn)
            //        PlayerList[MoveData.Name].transform.eulerAngles = new Vector3(0, 0, 0);
            //    else PlayerList[MoveData.Name].transform.eulerAngles = new Vector3(0, -180, 0);
            //    Debug.Log(MoveData.Name + "Moving");
            //    break;
            default:
                Debug.Log("unknowed msg");
                Debug.Log(msg.Type);
                break;
        }
        
    }

    //void UpdateMyself()
    //{
    //    if (PlayerList.Count < 1) return;
    //    if (myPos != PlayerList[myName].transform.position)
    //    {
    //        var data = new UpdateMyselfMessage();
    //        var Data = new PlayerData();
    //        Data.Name = myName;
    //        Data.X = PlayerList[myName].transform.position.x;
    //        Data.Y = PlayerList[myName].transform.position.y;
    //        Data.Turn = PlayerList[myName].TurnFlag;
    //        data.Data = Data;
    //        Send(Message.Move, data);
    //        myPos = PlayerList[myName].transform.position;
    //    }

    //}

}


public partial struct Message
{
    public const string Login = "Login";
    public const string Act = "Act";
    public const string Join = "Join";
    public const string GotDamage = "GotDamage";
}

[Serializable]
class JoinMessage
{
    public PlayerData Data;
}

[Serializable]
class LoginMessage
{
    public List<PlayerData> Players;
}

[Serializable]
class UpdateMyselfMessage
{
    public PlayerData Data;
}

[Serializable]
class PlayerActionMessage
{
    public PlayerAction Data;
}

[Serializable]
class PlayerGotDamageMessage
{
    public PlayerGotDamageData Data;
}

[Serializable]
class PlayerGotDamageData
{
    public string Name;
}

[Serializable]
public struct PlayerData
{
    public string Name;
    public float X;
    public float Y;
    public bool Turn;
}

[Serializable]
public struct PlayerAction
{
    public string Name;
    public ControlOrder Control;
    public float X;
    public float Y;
    public bool Turn;
}