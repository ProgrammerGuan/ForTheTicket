using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainGame : MonoBehaviour
{
    CharacterController Controller;
    Dictionary<string, Character> PlayerList;
    string myName;
    // Start is called before the first frame update
    private void Awake()
    {
        Controller = new CharacterController();
        PlayerList = new Dictionary<string, Character>();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        PlayerList[myName].Action(Controller.GetControl());
    }

    public void PlayerGotDamage(string playerName)
    {

    }

    public void CreatPlayer(string playerName)
    {
        var Character = new Character(this,playerName);
        PlayerList.Add(playerName, Character);
    }

}
