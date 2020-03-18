using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MainGame : MonoBehaviour
{
    public GameObject prefab;
    CharacterController Controller;
    Dictionary<string, Character> PlayerList;
    List<GameObject> PlayerNames;
    Button LoginButton;
    string myName;
    bool startGameFlag;
    WsClient client;
    Stack<Message> messages = new Stack<Message>();
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
        if(PlayerList.Count>0)
            PlayerList[myName].Action(Controller.GetControl());
        foreach(var name_ui in PlayerNames)
        {
            name_ui.transform.position = RectTransformUtility.WorldToScreenPoint(Camera.main, PlayerList[name_ui.GetComponent<Text>().text].transform.position + Vector3.up * 0.5f);
        }
        if (messages.Count > 0)
        {
            var msg = messages.Pop();
            UpdateMessage(msg);
        }
        if (startGameFlag)
            UpdateMyself();
        
    }

    public void PlayerGotDamage(string playerName)
    {

    }

    public void CreatPlayer(string playerName,string characterName,float x,float y)
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
        var data = new LoginMessage();
        data.Name = myName;
        data.X = 0;
        data.Y = 0;
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
        switch (msg.Type)
        {
            case Message.Login:
                var data = JsonUtility.FromJson<LoginMessage>(msg.Data);
                CreatPlayer(data.Name, "man",data.X,data.Y);
                Debug.Log(data.Name + " Login");
                break;
        }
        
    }

    void UpdateMyself()
    {
        var data = new UpdateMyselfMessage();
        
    }

}


public partial struct Message
{
    public const string Login = "Login";
}

class LoginMessage
{
    public string Name;
    public float X;
    public float Y;
}

class UpdateMyselfMessage
{
    public PlayerData MyDaras;
}

[SerializeField]
public struct PlayerData
{
    public string Name;
    public float X;
    public float Y;
}

