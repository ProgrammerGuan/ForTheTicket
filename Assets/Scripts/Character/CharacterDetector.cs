using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterDetector : MonoBehaviour
{
    MainGame MainGame;
    string myName;
    private void Awake()
    {
        MainGame = GameObject.FindGameObjectWithTag("MainGameManager").GetComponent<MainGame>();
    }
    public void SetMyName(string name)
    {
        myName = name;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "AttackCollision")
        {
            MainGame.PlayerGotDamage(myName);
        }
    }
}
