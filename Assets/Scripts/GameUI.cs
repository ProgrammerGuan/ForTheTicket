using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GameUI
{
    string selectCharacter;
    Button LoginButton;
    Button StartButton;
    List<Button> CharacterBtns;
    GameObject WinnerMessage;
    Button ExitWinMessageBtn;
    GameObject LoginFailMessage;
    Button ExitLoginFailBtn;
    GameObject SelectCharacterText;
    Text TimeCount;
    MainGame MainGame;
    Dictionary<string,GameObject> PlayerNames;
    float totaltime;
    int min, second;
    public GameUI(MainGame game)
    {
        MainGame = game;
        LoginButton = GameObject.Find("LoginButton").GetComponent<Button>();
        LoginButton.onClick.AddListener(Login);
        ExitLoginFailBtn = GameObject.Find("ExitLoginFailButton").GetComponent<Button>();
        ExitLoginFailBtn.onClick.AddListener(delegate { SetLoginFailMessage(false); });
        LoginFailMessage = GameObject.Find("LoginFailMessage");
        LoginFailMessage.SetActive(false);
        StartButton = GameObject.Find("StartBtn").GetComponent<Button>();
        StartButton.onClick.AddListener(delegate { MainGame.StartGame(); });
        StartButton.gameObject.SetActive(false);
        TimeCount = GameObject.Find("TimeCount").GetComponent<Text>();
        TimeCount.gameObject.SetActive(false);
        ExitWinMessageBtn = GameObject.Find("ExitWinMessageButton").GetComponent<Button>();
        ExitWinMessageBtn.onClick.AddListener(delegate { ExitWinMessage(); });
        WinnerMessage = GameObject.Find("WinnerMessage");
        WinnerMessage.SetActive(false);
        SelectCharacterText = GameObject.Find("SelectCharacterText");
        CharacterBtns = new List<Button>();
        InitialCharacterBtns();
        PlayerNames = new Dictionary<string, GameObject>();
        selectCharacter = "youngman";
        totaltime = 300;
    }
    void Login()
    {
        SetLoginFailMessage(false);
        LoginButton.interactable = GameObject.Find("NameInput").GetComponent<InputField>().interactable = false;
        foreach (var c in CharacterBtns) c.gameObject.SetActive(false);
        SelectCharacterText.SetActive(false);
        var myName = GameObject.Find("NameInput").transform.GetChild(2).GetComponent<Text>().text;
        MainGame.Login(myName,selectCharacter);
        StartButton.gameObject.SetActive(true);
    }

    void InitialCharacterBtns()
    {
        CharacterBtns.Add(GameObject.Find("youngmanBtn").GetComponent<Button>());
        CharacterBtns[CharacterBtns.Count - 1].onClick.AddListener(delegate { SelectCharacter("youngman"); });
        CharacterBtns.Add(GameObject.Find("manBtn").GetComponent<Button>());
        CharacterBtns[CharacterBtns.Count - 1].onClick.AddListener(delegate { SelectCharacter("man"); });
    }

    public void SelectCharacter(string character)
    {
        selectCharacter = character;
        foreach (var c in CharacterBtns)
        {
            if (c.name != character + "Btn")
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

    public void AddPlayerName(string playerName)
    {
        var name_ui = MainGame.Instantiate((GameObject)Resources.Load("Prefabs/PlayerName", typeof(GameObject)));
        name_ui.GetComponent<Text>().text = playerName;
        name_ui.transform.parent = GameObject.Find("Canvas").transform;
        name_ui.transform.SetAsFirstSibling();
        PlayerNames.Add(playerName,name_ui);
    }

    public void RemovePlayerName(string playerName)
    {
        MainGame.Destroy(PlayerNames[playerName]);
        PlayerNames.Remove(playerName);
    }

    public void UpdatePlayerNameLocation(Dictionary<string, Character> playerList)
    {
        foreach (var name_ui in PlayerNames)
        {
            name_ui.Value.transform.position = RectTransformUtility.WorldToScreenPoint(Camera.main, playerList[name_ui.Key].transform.position + Vector3.up * 0.5f);
        }
    }

    public void StartGame(int remainingTime)
    {
        StartButton.gameObject.SetActive(false);
        TimeCount.gameObject.SetActive(true);
        ExitWinMessage();
        totaltime = remainingTime;
    }

    public void GameEnd(string winnerName, string characterName)
    {
        StartButton.gameObject.SetActive(true);
        TimeCount.gameObject.SetActive(false);
        WinnerMessage.SetActive(true);
        
        GameObject.Find("NameText").GetComponent<Text>().text = winnerName;
        var showPlayer = MainGame.Instantiate(Resources.Load(string.Format("Prefabs/{0}", characterName+"cheer")), new Vector3(0, 0, 0), Quaternion.identity);
        showPlayer.name = "winnerModel";
        GameObject.Find(showPlayer.name).transform.SetParent(GameObject.Find("Winner").transform);
        GameObject.Find(showPlayer.name).transform.localPosition = Vector3.zero;
    }

    public void UpdateTime()
    {
        totaltime -= Time.deltaTime;
        min = (int)totaltime / 60;
        second = (int)totaltime % 60;
        if(second<10) TimeCount.text = min.ToString() + ":0" + second.ToString();
        else TimeCount.text = min.ToString() + ":" + second.ToString();
    }

    public void ExitWinMessage()
    {
        WinnerMessage.SetActive(false);
        if(GameObject.Find("Winner").transform.childCount>0)
            MainGame.Destroy(GameObject.Find("winnerModel"));
    }

    public void LoginFail()
    {
        SetLoginFailMessage(true);
        foreach (var c in CharacterBtns) c.gameObject.SetActive(true);
        SelectCharacterText.SetActive(true);
        Debug.Log("login fail");
        LoginButton.interactable = GameObject.Find("NameInput").GetComponent<InputField>().interactable = true;
        StartButton.gameObject.SetActive(false);
    }

    public void SetLoginFailMessage(bool show)
    {
        LoginFailMessage.SetActive(show);
    }
}