using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
//using UnityEngine.UI;
public class MainGame : MonoBehaviour
{
    // 自分のコントロール
    CharacterController Controller;
    //  UI
    GameUI gameUi;
    //　プレイヤーの名前対キャラクター
    public Dictionary<string, Character> PlayerList;
    //自分のキャラクターの資料
    Vector3 myPos;
    public string myName;

    //　Server
    bool Logined;
    bool startGameFlag;
    WsClient client;
    Stack<Message> messages = new Stack<Message>();
    bool sendIdle = false;
    float sendTime = 0;
    float perTime = 0;
    ControlOrder nowOrder;

    //Camera Shake
    Vector3 cameraOriPos;
    Vector3 shakeDir;
    float currentTime = 0;
    float totalTime = 0;
    bool resetCameraFlag = false;
    private void Awake()
    {
        Controller = new CharacterController();
        PlayerList = new Dictionary<string, Character>();
        gameUi = new GameUI(this);
        startGameFlag = false;
        Logined = false;
    }

    void Start()
    {
        //192.168.8.53 クァンのIP
        //192.168.11.17 クァンのパソコンip
        client = new WsClient("ws://192.168.11.17:4000");

        //string hostname = Dns.GetHostName();
        //IPAddress[] adrList = Dns.GetHostAddresses(hostname);
        //client = new WsClient("ws://" + adrList[1].ToString() + ":4000");

        client.OnMessage = onMessage;
        cameraOriPos = Camera.main.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        ControlAndSendToServer();
        gameUi.UpdatePlayerNameLocation(PlayerList);
        if (messages.Count > 0)
        {
            var msg = messages.Pop();
            if(Logined) UpdateMessage(msg);
        }
        if (startGameFlag) gameUi.UpdateTime();

    }
    private void LateUpdate()
    {
        CameraUpdateShake();
    }
    #region CameraShake
    public void CameraShakeTrigger(float time,float range)
    {
        totalTime = time;
        currentTime = time;
        shakeDir = Vector3.one * range;
        resetCameraFlag = true;
    }
    public void CameraUpdateShake()
    {
        if (currentTime > 0 && totalTime > 0)
        {
            var percent = currentTime / totalTime;
            var shakePos = Vector3.zero;

            shakePos.x = UnityEngine.Random.Range(-Mathf.Abs(shakeDir.x) * percent, Mathf.Abs(shakeDir.x) * percent);
            shakePos.y = UnityEngine.Random.Range(-Mathf.Abs(shakeDir.y) * percent, Mathf.Abs(shakeDir.y) * percent);
            shakePos.z = Camera.main.transform.position.z + UnityEngine.Random.Range(-Mathf.Abs(shakeDir.z) * percent, Mathf.Abs(shakeDir.z) * percent);
            Camera.main.transform.position = shakePos;
            currentTime -= Time.deltaTime;
        }
        else
        {
            if (resetCameraFlag) ResetCamera();
            currentTime = 0f;
            totalTime = 0f;
        }
    }
    private void ResetCamera()
    {
        Camera.main.transform.position = cameraOriPos;
        resetCameraFlag = false;
    }
    #endregion
    #region Character
    public void ControlAndSendToServer()
    {
        if (PlayerList.Count <= 0) return;
        var ctrl = Controller.GetControl();
        if (ctrl != ControlOrder.None)
        {
            if (ctrl != ControlOrder.Idle) sendIdle = true;
            if (ctrl != ControlOrder.Kick)
                PlayerList[myName].Action(ctrl, PlayerList[myName].transform.position.x, PlayerList[myName].transform.position.y, false, false, Parameters.MoveSpeed);
            if (ctrl == ControlOrder.Kick || sendIdle ||
                myPos.x != PlayerList[myName].transform.position.x)
            {
                if (sendIdle && ctrl == ControlOrder.Idle) sendIdle = false;
                var actMessage = new PlayerActionMessage();
                var act = new PlayerAction();
                act.Name = myName;
                act.Control = ctrl;
                act.X = PlayerList[myName].transform.position.x;
                act.Y = PlayerList[myName].transform.position.y;
                act.Turn = PlayerList[myName].TurnFlag;
                act.Vx = Parameters.Vx * (act.Turn ? -1 : 1);
                actMessage.Data = act;
                if (Time.time > sendTime || (ctrl != ControlOrder.moveLeft && ctrl != ControlOrder.moveRight) || nowOrder != ctrl)
                {
                    Send(Message.Act, actMessage);
                    sendTime = Time.time + Time.deltaTime * Parameters.SendDistance;
                    perTime = (sendTime - Time.time) / Parameters.SendDistance;
                }
                myPos = PlayerList[myName].transform.position;
            }
            nowOrder = ctrl;
        }
    }
    public void MineGotDamage(string playerName, bool DamageForward, string kickPlayerName, bool skill)
    {
        if (playerName != myName) return;
        CameraShakeTrigger(Parameters.KickShakeTime,Parameters.KickShakeRange);
        PlayerList[playerName].AnimatorFrameStop();
        var message = new PlayerGotDamageMessage();
        var data = new PlayerGotDamageData();
        data.Name = playerName;
        data.GotDamageForward = DamageForward;
        data.KickerName = kickPlayerName;
        data.Skill = skill;
        message.Data = data;
        Send(Message.GotDamage, message);
        Debug.Log(playerName + " got damage and send");
    }
    #endregion
    #region GameSetting
    public void CreatPlayer(string playerName,string characterName,float x,float y,bool Turn,bool havingTicket)
    {
        var newplayer = Instantiate(Resources.Load(string.Format("Prefabs/{0}", characterName)), new Vector3(x,y,0), Quaternion.identity);
        newplayer.name = playerName;
        GameObject.Find(newplayer.name).GetComponent<CharacterDetector>().SetMyName(newplayer.name);
        var Character = new Character(this,playerName,characterName);
        PlayerList.Add(playerName, Character);
        Character.HaveTicket(havingTicket);
        gameUi.AddPlayerName(playerName);
    }
    public void Login(string name,string character)
    {
        Logined = true;
        myName = name;
        var data = new JoinMessage();
        var Data = new PlayerData();
        Data.Name = myName;
        Data.X = 0;
        Data.Y = 0;
        Data.Turn = false;
        Data.Character = character;
        data.Data = Data;
        Send(Message.Login, data);
    }
    public void StartGame()
    {
        Send(Message.StartGame, null);
    }
    public void GameSetting(GameSettingMessage message)
    {
        startGameFlag = true;
        foreach (var pD in message.Data.PlayerData)
        {
            var c = PlayerList[pD.Name];
            //Debug.Log(pD.Name + " ori position X : " + c.transform.position.x);
            //Debug.Log(pD.Name + " new position X : " + pD.X);
            c.transform.position = new Vector3(pD.X, pD.Y, 0);
            if (pD.Turn) c.Turn("left");
            else c.Turn("right");
            c.HaveTicket(pD.HavingTicket);
        }
        Destroy(GameObject.Find("Ticket"));
        CreatTicket(message.Data.TicketData.FromPlayer, message.Data.TicketData.X, message.Data.TicketData.Y);
    }
    IEnumerator ReadyToStart(GameSettingMessage message)
    {
        gameUi.StartGame(message.Data.RemainingTime);
        for (int cnt = 3; cnt > 0; cnt--)
        {
            gameUi.UpdateReadyCount(cnt);
            yield return new WaitForSeconds(1);
        }
        gameUi.CloseReadyCount();
        GameSetting(message);
    }
    public void GameEnd(string winnerName,int kickCnt)
    {
        startGameFlag = false;
        Debug.Log(winnerName + " kick " + kickCnt + "times");
        if (winnerName!= "N;O:N-E,") gameUi.GameEnd(winnerName,PlayerList[winnerName].CharacterName,kickCnt);
        else gameUi.GameEnd(winnerName, "null",0);
    }
    void CreatTicket(bool fromPlayer, float x, float y)
    {
        var ticket = Instantiate(Resources.Load("Prefabs/Ticket"), new Vector3(x, y, 0), Quaternion.identity);
        ticket.name = "Ticket";
        if (fromPlayer) GameObject.Find("Ticket").GetComponent<Rigidbody2D>().velocity = Vector3.up * Parameters.FallTicketHeight;
    }
    public void FallTicket(string playerName, float x, float y)
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
    #endregion
    #region Message
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
                
                //Debug.Log(d.Length);
                var LoginData = JsonUtility.FromJson<LoginMessage>(msg.Data);
                foreach (var player in LoginData.Players)
                {
                    CreatPlayer(player.Name, player.Character, player.X, player.Y, player.Turn,player.HavingTicket);
                }
                Debug.Log(LoginData.remainingTime);
                if (LoginData.remainingTime > 0)
                {
                    gameUi.StartGame(LoginData.remainingTime);
                    startGameFlag = true;
                }
                //Debug.Log("Login");
                break;
            case Message.LoginFail:
                gameUi.LoginFail();
                break;
            case Message.FirstTicket:
                var firstTicketData = JsonUtility.FromJson<FirstTicketMessage>(msg.Data);
                CreatTicket(false, firstTicketData.TicketData.X, firstTicketData.TicketData.Y);
                break;
            case Message.Join:
                var JoinData = JsonUtility.FromJson<JoinMessage>(msg.Data);
                CreatPlayer(JoinData.Data.Name, JoinData.Data.Character, JoinData.Data.X, JoinData.Data.Y, JoinData.Data.Turn,false);
                //Debug.Log(JoinData.Data.Name + " Join");
                break;
            case Message.Act:
                var ActData = JsonUtility.FromJson<PlayerActionMessage>(msg.Data);
                if(PlayerList.ContainsKey(ActData.Data.Name))
                    PlayerList[ActData.Data.Name].Action(ActData.Data.Control,ActData.Data.X,ActData.Data.Y,ActData.Data.Turn,true,ActData.Data.Vx);
                //myPos = PlayerList[myName].transform.position;
                break;
            case Message.GotDamage:
                var DamageData = JsonUtility.FromJson<PlayerGotDamageMessage>(msg.Data);
                PlayerList[DamageData.Data.Name].GotDamage(DamageData.Data.GotDamageForward,DamageData.Data.Skill);
                break;
            case Message.BornTicket:
                var BornTicketData = JsonUtility.FromJson<BronTicketMessage>(msg.Data);
                CreatTicket(BornTicketData.Data.FromPlayer, BornTicketData.Data.X, BornTicketData.Data.Y);
                break;
            case Message.GetTicket:
                var GetTicketData = JsonUtility.FromJson<GetTicketMessage>(msg.Data);
                PlayerList[GetTicketData.Data.Name].HaveTicket(true);
                if(GameObject.Find("Ticket"))Destroy(GameObject.Find("Ticket"));
                break;
            case Message.Exit:
                var ExitMessage = JsonUtility.FromJson<ExitMessage>(msg.Data);
                Debug.Log(ExitMessage.Data.Name);
                Destroy(GameObject.Find(ExitMessage.Data.Name));
                PlayerList.Remove(ExitMessage.Data.Name);
                gameUi.RemovePlayerName(ExitMessage.Data.Name);
                break;
            case Message.StartGame:
                var StartGameMessage = JsonUtility.FromJson<GameSettingMessage>(msg.Data);
                StartCoroutine(ReadyToStart(StartGameMessage));
                //GameSetting(StartGameMessage);
                break;
            case Message.GameEnd:
                var GameEndMessage = JsonUtility.FromJson<GameEndMessage>(msg.Data);
                GameEnd(GameEndMessage.Data.WinnerName,GameEndMessage.Data.KickCnt);
                break;
            default:
                //Debug.Log("unknowed msg");
                //Debug.Log(msg.Type);
                break;
        }
        
    }
    #endregion
}

#region ServerMessageType
public partial struct Message
{
    public const string Login = "Login";
    public const string LoginFail = "LoginFail";
    public const string Act = "Act";
    public const string Join = "Join";
    public const string GotDamage = "GotDamage";
    public const string BornTicket = "BornTicket";
    public const string GetTicket = "GetTicket";
    public const string Exit = "Exit";
    public const string FirstTicket = "FirstTicket";
    public const string StartGame = "StartGame";
    public const string GameEnd = "GameEnd";
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
    public int remainingTime;
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
    public string KickerName;
    public bool Skill;
}

[Serializable]
public struct PlayerData
{
    public string Name;
    public float X;
    public float Y;
    public bool Turn;
    public string Character;
    public bool HavingTicket;
}

[Serializable]
public struct PlayerAction
{
    public string Name;
    public ControlOrder Control;
    public float X;
    public float Vx;
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
public struct GetTicketData
{
    public string Name;
}

[Serializable]
public class GameSettingMessage
{
    public GameSettingData Data;
}

[Serializable]
public struct GameSettingData
{
    public List<PlayerData>PlayerData;
    public BornTicketData TicketData;
    public int RemainingTime;
}

[Serializable]
public class GameEndMessage
{
    public GameEndData Data;
}

[Serializable]
public struct GameEndData
{
    public string WinnerName;
    public int KickCnt;
}
#endregion