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
    List<Button> CharacterBtns;
    string myName;
    bool startGameFlag;
    WsClient client;
    Stack<Message> messages = new Stack<Message>();
    bool sendIdle = false;
    bool updateIdle = false;
    string selectCharacter;
    // Start is called before the first frame update
    private void Awake()
    {
        PlayerNames = new List<GameObject>();
        Controller = new CharacterController();
        PlayerList = new Dictionary<string, Character>();
        LoginButton = GameObject.Find("LoginButton").GetComponent<Button>();
        LoginButton.onClick.AddListener(Login);
        CharacterBtns = new List<Button>();
        InitialCharacterBtns();
        startGameFlag = false;
        selectCharacter = "youngman";
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
            if (ctrl != ControlOrder.None)
            {
                if (!sendIdle) sendIdle = true;
                if (ctrl!=ControlOrder.Idle)
                {
                    updateIdle = true;
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
                else if (updateIdle)
                {
                    updateIdle = false;
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

    public void MineGotDamage(string playerName, bool DamageForward)
    {
        var message = new PlayerGotDamageMessage();
        var data = new PlayerGotDamageData();
        data.Name = playerName;
        data.GotDamageForward = DamageForward;
        message.Data = data;
        Send(Message.GotDamage,message);
    }

    public void CreatPlayer(string playerName,string characterName,float x,float y,bool Turn)
    {
        var newplayer = Instantiate(Resources.Load(string.Format("Prefabs/{0}", characterName)), new Vector3(x,y,0), Quaternion.identity);
        newplayer.name = playerName;
        GameObject.Find(newplayer.name).GetComponent<CharacterDetector>().SetMyName(newplayer.name);
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
        foreach (var c in CharacterBtns) c.gameObject.SetActive(false);
        GameObject.Find("SelectCharacterText").SetActive(false);
        myName = GameObject.Find("NameInput").transform.GetChild(2).GetComponent<Text>().text;
        startGameFlag = true;
        var data = new JoinMessage();
        var Data = new PlayerData();
        Data.Name = myName;
        Data.X = 0;
        Data.Y = 0;
        Data.Turn = false;
        Data.Character = selectCharacter;
        data.Data = Data;
        Send(Message.Login, data);
    }

    void InitialCharacterBtns()
    {
        CharacterBtns.Add(GameObject.Find("youngmanBtn").GetComponent<Button>());
        CharacterBtns[CharacterBtns.Count-1].onClick.AddListener(delegate { SelectCharacter("youngman"); });
        CharacterBtns.Add(GameObject.Find("manBtn").GetComponent<Button>());
        CharacterBtns[CharacterBtns.Count - 1].onClick.AddListener(delegate { SelectCharacter("man"); });
    }

    public void SelectCharacter(string character)
    {
        selectCharacter = character;
        foreach(var c in CharacterBtns)
        {
            if(c.name != character + "Btn")
            {
                c.interactable = true;
                c.transform.GetChild(2).gameObject.SetActive(false);
            }
            else
            {
                c.interactable = false;
                c.transform.GetChild(2).gameObject.SetActive(true);
            }
        }
    }

    void CreatTicket(bool fromPlayer, float x, float y)
    {
        var ticket = Instantiate(Resources.Load("Prefabs/Ticket"), new Vector3(x, y, 0), Quaternion.identity);
        ticket.name = "Ticket";
        if (fromPlayer) GameObject.Find("Ticket").GetComponent<Rigidbody2D>().velocity = Vector3.up * 6;
    }

    public void FallTicket(string playerName,float x,float y)
    {
        if (playerName != myName) return;
        var creatTicketMessage = new BronTicketMessage();
        var creatTicketData = new BornTicketData();
        creatTicketData.FromPlayer = true;
        creatTicketData.X = x;
        creatTicketData.Y = y;
        creatTicketMessage.Data = creatTicketData;
        Send(Message.BornTicket, creatTicketMessage);
    }

    public void GetTicket(string name)
    {
        var getTicketMessage = new GetTicketMessage();
        var getTicketData = new GetTicketData();
        getTicketData.Name = name;
        getTicketMessage.Data = getTicketData;
        Send(Message.GetTicket, getTicketMessage);
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
                    CreatPlayer(player.Name, player.Character, player.X, player.Y, player.Turn);
                }
                //Debug.Log("Login");
                break;
            case Message.FirstTicket:
                var firstTicketData = JsonUtility.FromJson<FirstTicketMessage>(msg.Data);
                CreatTicket(false, firstTicketData.TicketData.X, firstTicketData.TicketData.Y);
                break;
            case Message.Join:
                var JoinData = JsonUtility.FromJson<JoinMessage>(msg.Data);
                CreatPlayer(JoinData.Data.Name, JoinData.Data.Character, JoinData.Data.X, JoinData.Data.Y, JoinData.Data.Turn);
                //Debug.Log(JoinData.Data.Name + " Join");
                break;
            case Message.Act:
                var ActData = JsonUtility.FromJson<PlayerActionMessage>(msg.Data);
                if(PlayerList.ContainsKey(ActData.Data.Name))
                    PlayerList[ActData.Data.Name].Action(ActData.Data.Control);
                break;
            case Message.GotDamage:
                var DamageData = JsonUtility.FromJson<PlayerGotDamageMessage>(msg.Data);
                PlayerList[DamageData.Data.Name].GotDamage(DamageData.Data.GotDamageForward);
                Debug.Log(DamageData.Data.Name + " Got damage in MainGame");
                break;
            case Message.BornTicket:
                var BornTicketData = JsonUtility.FromJson<BronTicketMessage>(msg.Data);
                CreatTicket(BornTicketData.Data.FromPlayer, BornTicketData.Data.X, BornTicketData.Data.Y);
                break;
            case Message.GetTicket:
                var GetTicketData = JsonUtility.FromJson<GetTicketMessage>(msg.Data);
                PlayerList[GetTicketData.Data.Name].HaveTicket(true); 
                break;
            case Message.Exit:
                var ExitMessage = JsonUtility.FromJson<ExitMessage>(msg.Data);
                PlayerList.Remove(ExitMessage.Data.Name);
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
    public const string BornTicket = "BornTicket";
    public const string GetTicket = "GetTicket";
    public const string Exit = "Exit";
    public const string FirstTicket = "FirstTicket";
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
    public bool GotDamageForward;
}

[Serializable]
public struct PlayerData
{
    public string Name;
    public float X;
    public float Y;
    public bool Turn;
    public string Character;
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

[Serializable]
class ExitMessage
{
    public ExitData Data;
}

[Serializable]
public struct ExitData
{
    public string Name;
}

[Serializable]
class FirstTicketMessage
{
    public FirstTicketData TicketData;
}

[Serializable]
public struct FirstTicketData
{
    public float X;
    public float Y;
}

[Serializable]
class BronTicketMessage
{
    public BornTicketData Data;
}

[Serializable]
public struct BornTicketData
{
    public float X;
    public float Y;
    public bool FromPlayer;
}

[Serializable]
class GetTicketMessage
{
    public GetTicketData Data;
}

[Serializable]
class GetTicketData
{
    public string Name;
}