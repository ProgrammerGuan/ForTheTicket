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
            if (collision.transform.position.x> gameObject.transform.position.x)　damageForward = true;
            else　damageForward = false;
            DamageTime = Time.time + Parameters.DamageCoolDownTime;
            var kickerName = collision.gameObject.transform.parent.name;
            MainGame.MineGotDamage(myName,damageForward,kickerName,false);// false is not skill damage
        }
        else if(collision.tag == "SkillAttackCollision" && usefulDamage)
        {
            var skillRangeData = collision.gameObject.GetComponent<SkillRange>();
            if (myName == skillRangeData.SkillUser) return;
            var damageForward = true;
            print(myName + " SkillDamage");
            var CenterPosX = skillRangeData.SkillPosX;
            if (CenterPosX > gameObject.transform.position.x) damageForward = true;
            else damageForward = false;
            DamageTime = Time.time + Parameters.DamageCoolDownTime;
            var kickerName = collision.gameObject.transform.parent.name;
            MainGame.MineGotDamage(myName, damageForward, kickerName, true);//true is skill damage
        }
        else if (collision.gameObject.name == "Ticket" && !collision.gameObject.GetComponent<Ticket>().BeGetted)
        {
            MainGame.GetTicket(myName);
            Destroy(collision.gameObject);
            collision.gameObject.GetComponent<Ticket>().BeGetted = true;
            var kickEffect = Instantiate(Resources.Load("Prefabs/GETEFFECT"), gameObject.transform.position+Vector3.up*0.5f, Quaternion.identity);
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
            if(myName == MainGame.myName) MainGame.CameraShakeTrigger(Parameters.KickShakeTime,Parameters.KickShakeRange);
            MainGame.PlayerList[myName].AnimatorFrameStop();
            // Effect
            var kickEffect = Instantiate(Resources.Load("Prefabs/KICKEFFECT"), gameObject.transform.GetChild(0).transform.position, Quaternion.identity);
            Debug.Log("Kick Effect : " + kickEffect.name);
        }
    }

    //Skill on floor
    public void SkillAttack(Collider2D collision)
    {
        if (collision.gameObject.tag == "floor")
        {
            MainGame.CameraShakeTrigger(Parameters.SkillShakeTime,Parameters.SkillShakeRange);
            gameObject.GetComponent<Rigidbody2D>().gravityScale = 1;
            transform.GetChild(7).gameObject.SetActive(false);
            var SkillRange = collision.transform.GetChild(0).gameObject;
            print("Skill range poxX : " + SkillRange.GetComponent<SkillRange>().SkillPosX);
            MainGame.StartCoroutine(MainGame.PlayerList[myName].SkillExplosion(SkillRange));
            var skillRange = SkillRange.GetComponent<SkillRange>();
            skillRange.SkillPosX = gameObject.transform.position.x;
            skillRange.SkillUser = myName;
        }
    }

}
