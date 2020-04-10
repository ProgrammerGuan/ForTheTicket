using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
public class GameUI
{
    MainGame MainGame;

    string selectCharacter;
    Button LoginButton;
    Button StartButton;
    GameObject SelectCharacterText;
    List<Button> CharacterBtns;

    GameObject WinnerMessage;
    Button ExitWinMessageBtn;

    GameObject LoginFailMessage;
    Button ExitLoginFailBtn;

    Text TimeCount;
    Text ReadyCount;
    Dictionary<string,GameObject> PlayerNames;
    Camera UiCamera;
    float totaltime;
    int min, second;

    List<GameObject> SkillChargePic;
    List<GameObject> SkillChargePicLight;
    GameObject SkillChargeNotFull;
    GameObject SkillChargeFull;
    
    public GameUI(MainGame game)
    {
        MainGame = game;
        UiCamera = GameObject.Find("UiCamera").GetComponent<Camera>();
        SettingLoginButton();
        SettingLoginFailMessage();
        SettingStartButton();
        SettingTimeCounter();
        SettingWinMessage();
        SettingSelectCharacter();
        SettingSkillCharge();
    }

    #region PreSetting
    void SettingLoginButton()
    {
        LoginButton = GameObject.Find("LoginButton").GetComponent<Button>();
        LoginButton.onClick.AddListener(Login);
    }

    void SettingLoginFailMessage()
    {
        ExitLoginFailBtn = GameObject.Find("ExitLoginFailButton").GetComponent<Button>();
        ExitLoginFailBtn.onClick.AddListener(delegate { SetLoginFailMessage(false); });
        LoginFailMessage = GameObject.Find("LoginFailMessage");
        LoginFailMessage.SetActive(false);
    }

    void SettingStartButton()
    {
        StartButton = GameObject.Find("StartBtn").GetComponent<Button>();
        StartButton.onClick.AddListener(delegate { MainGame.StartGame(); });
        StartButton.gameObject.SetActive(false);
    }

    void SettingTimeCounter()
    {
        TimeCount = GameObject.Find("TimeCount").GetComponent<Text>();
        TimeCount.gameObject.SetActive(false);
        ReadyCount = GameObject.Find("ReadyCount").GetComponent<Text>();
        ReadyCount.gameObject.SetActive(false);
    }

    void SettingWinMessage()
    {
        ExitWinMessageBtn = GameObject.Find("ExitWinMessageButton").GetComponent<Button>();
        ExitWinMessageBtn.onClick.AddListener(delegate { ExitWinMessage(); });
        WinnerMessage = GameObject.Find("WinnerMessage");
        WinnerMessage.SetActive(false);
    }

    void SettingSelectCharacter()
    {
        SelectCharacterText = GameObject.Find("SelectCharacterText");
        CharacterBtns = new List<Button>();
        InitialCharacterBtns();
        PlayerNames = new Dictionary<string, GameObject>();
        selectCharacter = "youngman";
    }

    void SettingSkillCharge()
    {
        SkillChargePic = new List<GameObject>();
        SkillChargePicLight = new List<GameObject>();
        for (int i = 1; i <= 4; i++)
        {
            var pic = GameObject.Find(string.Format("Charge{0}", i));
            SkillChargePic.Add(pic);
            var picLight = pic.transform.GetChild(0).gameObject;
            SkillChargePicLight.Add(picLight);
            picLight.SetActive(false);
            pic.SetActive(false);
            Debug.Log(pic.name);
        }
        SkillChargeFull = GameObject.Find("CircleCharged");
        SkillChargeFull.SetActive(false);
        SkillChargeNotFull = GameObject.Find("CircleNotCharge");
        SkillChargeNotFull.SetActive(false);
    }
    #endregion

    void Login()
    {
        SetLoginFailMessage(false);
        LoginButton.interactable = GameObject.Find("NameInput").GetComponent<InputField>().interactable = false;
        foreach (var c in CharacterBtns) c.gameObject.SetActive(false);
        SelectCharacterText.SetActive(false);
        var myName = GameObject.Find("NameInput").transform.GetChild(2).GetComponent<Text>().text;
        MainGame.Login(myName,selectCharacter);
        StartButton.gameObject.SetActive(true);
        SkillChargeNotFull.SetActive(true);
        foreach (var p in SkillChargePic) p.SetActive(true);
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
            name_ui.Value.transform.position = playerList[name_ui.Key].transform.position + Vector3.up * 0.5f;
            //name_ui.Value.transform.position = RectTransformUtility.WorldToScreenPoint(Camera.main, playerList[name_ui.Key].transform.position + Vector3.up * 0.5f);
        }
    }

    public void UpdateReadyCount(int cnt)
    {
        if(!ReadyCount.gameObject.active) ReadyCount.gameObject.SetActive(true);
        ReadyCount.text = cnt.ToString();
    }

    public void CloseReadyCount()
    {
        ReadyCount.gameObject.SetActive(false);
    }

    public void StartGame(int remainingTime)
    {
        StartButton.gameObject.SetActive(false);
        TimeCount.gameObject.SetActive(true);
        ExitWinMessage();
        totaltime = remainingTime;
    }

    public void GameEnd(string winnerName, string characterName,int kickCnt)
    {
        StartButton.gameObject.SetActive(true);
        TimeCount.gameObject.SetActive(false);
        WinnerMessage.SetActive(true);
        if(winnerName != "N;O:N-E,")
        {
            GameObject.Find("WinnerTitle").GetComponent<Text>().text = "WINNER";
            GameObject.Find("NameText").GetComponent<Text>().text = winnerName;
            var showPlayer = MainGame.Instantiate(Resources.Load(string.Format("Prefabs/{0}", characterName + "cheer")), new Vector3(0, 0, 0), Quaternion.identity);
            showPlayer.name = "winnerModel";
            GameObject.Find(showPlayer.name).transform.SetParent(GameObject.Find("Winner").transform);
            GameObject.Find(showPlayer.name).transform.localPosition = Vector3.zero;
            GameObject.Find("KickCount").GetComponent<Text>().text = kickCnt.ToString();
            
        }
        else
        {
            GameObject.Find("NameText").GetComponent<Text>().text = "";
            GameObject.Find("WinnerTitle").GetComponent<Text>().text = "DRAW";
        }

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

    public void SetSkillChargeCnt(int cnt)
    {
        int i = 1;
        foreach (var l in SkillChargePicLight)
        {
            if (i <= cnt) l.SetActive(true);
            else l.SetActive(false);
            i++;
        }
        if (cnt == 4) SkillChargeFull.SetActive(true);
        else SkillChargeFull.SetActive(false);
    }
}