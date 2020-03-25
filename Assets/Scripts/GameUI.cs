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
    Text TimeCount;
    MainGame MainGame;
    List<GameObject> PlayerNames;
    public GameUI(MainGame game)
    {
        MainGame = game;
        LoginButton = GameObject.Find("LoginButton").GetComponent<Button>();
        LoginButton.onClick.AddListener(Login);
        //StartButton = GameObject.Find("StartBtn").GetComponent<Button>();
        //StartButton.onClick.AddListener(delegate {MainGame.StartGame(); });
        TimeCount = GameObject.Find("TimeCount").GetComponent<Text>();
        CharacterBtns = new List<Button>();
        InitialCharacterBtns();
        PlayerNames = new List<GameObject>();
        selectCharacter = "youngman";
    }
    void Login()
    {
        LoginButton.interactable = GameObject.Find("NameInput").GetComponent<InputField>().interactable = false;
        foreach (var c in CharacterBtns) c.gameObject.SetActive(false);
        GameObject.Find("SelectCharacterText").SetActive(false);
        var myName = GameObject.Find("NameInput").transform.GetChild(2).GetComponent<Text>().text;
        MainGame.Login(myName,selectCharacter);
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
        PlayerNames.Add(name_ui);
    }

    public void UpdatePlayerNameLocation(Dictionary<string, Character> playerList)
    {
        foreach (var name_ui in PlayerNames)
        {
            name_ui.transform.position = RectTransformUtility.WorldToScreenPoint(Camera.main, playerList[name_ui.GetComponent<Text>().text].transform.position + Vector3.up * 0.5f);
        }
    }

    public void StartGame()
    {
        
    }
}