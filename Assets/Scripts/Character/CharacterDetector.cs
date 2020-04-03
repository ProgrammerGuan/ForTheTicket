using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterDetector : MonoBehaviour
{
    MainGame MainGame;
    string myName;
    float DamageTime;
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
        var usefulDamage = Time.time > DamageTime;
        if (collision.tag == "AttackCollision" && usefulDamage)
        {
            var damageForward = true;
            Debug.Log(myName + " GetDamage");
            if (collision.transform.position.x > gameObject.transform.position.x)
                damageForward = true;
            else
                damageForward = false;
            DamageTime = Time.time + Parameters.DamageCoolDownTime;
            MainGame.MineGotDamage(myName,damageForward);
        }
        else if (collision.gameObject.name == "Ticket" && !collision.gameObject.GetComponent<Ticket>().BeGetted)
        {
            MainGame.GetTicket(myName);
            Destroy(collision.gameObject);
            collision.gameObject.GetComponent<Ticket>().BeGetted = true;
            Debug.Log(myName + " GotTicket");
        }
    }

    // bottomのObjectで使う
    public void UpdateJump(Collider2D collision)
    {
        if ((collision.gameObject.tag == "floor" || collision.gameObject.tag == "Player") && MainGame.PlayerList[myName].Animator.GetBool("Jump"))
        {
            MainGame.PlayerList[myName].EndJump();
            if (myName == MainGame.myName) Parameters.CanJump = Time.time + 0.01f;
        }
    }

    //FromtのObjectで使う
    public void CanNotMove(Collider2D collision)
    {
        //Debug.Log(collision.gameObject.tag);
        if (collision.gameObject.layer == 8 || collision.gameObject.tag == "Player") // layer 8 is Stage
        {
            MainGame.PlayerList[myName].CanMove = false;
        }
    }

    //FromtのObjectで使う
    public void CanMove(Collider2D collision)
    {
        //Debug.Log(collision.gameObject.tag);

        if (collision.gameObject.layer == 8 || collision.gameObject.tag == "Player") // layer 8 is Stage
        {
            //Debug.Log("can move");

            MainGame.PlayerList[myName].CanMove = true;
        }
    }

    //Kicked player -> MainGame Shake Camera
    public void KickedPlayer(Collider2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            MainGame.CameraShakeTrigger();
        }
    }

}
