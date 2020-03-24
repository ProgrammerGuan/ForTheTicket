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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var usefulDamage = Time.time - Parameters.DamageTime > Parameters.DamageCoolDownTime;
        if (collision.tag == "AttackCollision" && usefulDamage)
        {
            var damageForward = true;
            Debug.Log(myName + " GetDamage");
            if (collision.transform.position.x > gameObject.transform.position.x)
                damageForward = true;
            else
                damageForward = false;
            MainGame.MineGotDamage(myName,damageForward);
        }
        else if (collision.gameObject.name == "Ticket")
        {
            MainGame.GetTicket(myName);
            Destroy(collision.gameObject);
            Debug.Log("GotTicket");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        
    }
}
