using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character
{
    MainGame MainGame;
    GameObject myGameObject;
    public Animator Animator;
    CharacterDetector Detector;
    GameObject AttackRange;
    string Name;
    public string CharacterName;
    public bool TurnFlag;
    private bool HavingTicket;
    public Character(MainGame mainGame,string name,string characterName)
    {
        Name = name;
        CharacterName = characterName;
        TurnFlag = false;
        myGameObject = GameObject.Find(Name);
        Animator = myGameObject.GetComponent<Animator>();
        Detector = myGameObject.GetComponent<CharacterDetector>();
        MainGame = mainGame;
        AttackRange = myGameObject.transform.GetChild(0).gameObject;
        AttackRange.SetActive(false);
        HavingTicket = false;

    }
    public Transform transform => myGameObject.transform;

    public void Action(ControlOrder control,float x,float y,bool fromServer,float vx)
    {

        switch (control)
        {
            case ControlOrder.moveLeft:
                if (myGameObject.transform.eulerAngles != new Vector3(0, -180, 0))
                    Turn("left");
                if(fromServer) Move(x,y,vx);
                else Move(-Parameters.MoveSpeed + myGameObject.transform.position.x, myGameObject.transform.position.y);
                break;
            case ControlOrder.moveRight:
                if (myGameObject.transform.eulerAngles != new Vector3(0, 0, 0))
                    Turn("right");
                if(fromServer) Move(x,y,vx);
                else Move(Parameters.MoveSpeed + myGameObject.transform.position.x, myGameObject.transform.position.y);
                break;
            case ControlOrder.Jump:
                Jump();
                break;
            case ControlOrder.Kick:
                Kick();
                break;
            case ControlOrder.Idle:
                SetAnimation("Idle");
                break;
            }
        
        
    }
    private void Jump()
    {
        Parameters.JumpTime = Time.time;
        MainGame.StartCoroutine(SetAction("Jump", Parameters.JumpCoolDownTime - 0.5f));
        myGameObject.GetComponent<Rigidbody2D>().AddForce(Vector2.up * Parameters.JumpHeight, ForceMode2D.Impulse);
    }

    private void Kick()
    {
        Parameters.ActionTime = Time.time;
        if (myGameObject.transform.eulerAngles == new Vector3(0, 0, 0))
        {
            if (Animator.GetBool("Walk") || Animator.GetBool("Jump"))
                myGameObject.GetComponent<Rigidbody2D>().velocity = Vector3.right * Parameters.JumpKickDistance;
            else myGameObject.GetComponent<Rigidbody2D>().velocity = Vector3.right * Parameters.NormalKickDistance;

        }
        else
        {
            if (Animator.GetBool("Walk") || Animator.GetBool("Jump"))
                myGameObject.GetComponent<Rigidbody2D>().velocity = Vector3.left * Parameters.JumpKickDistance;
            else myGameObject.GetComponent<Rigidbody2D>().velocity = Vector3.left * Parameters.NormalKickDistance;
        }
        MainGame.StartCoroutine(SetAction("Kick", Parameters.KickCoolDownTime-0.5f));
        MainGame.StartCoroutine(SetKickRange());
    }



    private void SetAnimation(string action)
    {
        if (!Animator.GetBool(action))
        {
            Animator.SetBool("Walk", (action == "Walk") ? true : false);
            Animator.SetBool("Jump", (action == "Jump") ? true : false);
            Animator.SetBool("Kick", (action == "Kick") ? true : false);
            Animator.SetBool("Idle", (action == "Idle") ? true : false);
            Debug.Log(Name + "Set" + action);
        }
            
    }

    private void Move(float x,float y,float vx)
    {
        Move(x, y);
        MainGame.StartCoroutine(UpdatePosition(vx));
    }

    private void Move(float x, float y)
    {
        if (!Animator.GetBool("Jump"))
            SetAnimation("Walk");
        myGameObject.transform.position = new Vector3(x,y, 0);
    }

    public void Turn(string turnTo)
    {
        switch (turnTo)
        {
            case "right":
                myGameObject.transform.eulerAngles = new Vector3(0, 0, 0);
                TurnFlag = false;
                break;
            case "left":
                myGameObject.transform.eulerAngles = new Vector3(0, -180, 0);
                TurnFlag = true;
                break;
        }
        
    }

    private IEnumerator SetAction(string action,float time)
    {
        SetAnimation(action);
        yield return new WaitForSeconds(time);
        Animator.SetBool(action, false);
        MainGame.StopCoroutine(SetAction(action,time));
    }

    public void GotDamage(bool GotDamageForward)
    {
        //Right side got damage
        if (GotDamageForward) Turn("right");
        else Turn("left");  // Left side got damage
        SetAnimation("GotDamage");
        Animator.SetTrigger("GotDamage");
        if (GotDamageForward) myGameObject.GetComponent<Rigidbody2D>().velocity = Vector3.left * Parameters.GotDamageDistance;
        else myGameObject.GetComponent<Rigidbody2D>().velocity = Vector3.right * Parameters.GotDamageDistance;
        if (HavingTicket)
        {
            MainGame.FallTicket(Name,myGameObject.transform.position.x, myGameObject.transform.position.y + 1);
            HaveTicket(false);
        }
    }

    private IEnumerator SetKickRange()
    {
        yield return new WaitForSeconds(0.3f);
        AttackRange.SetActive(true);
        yield return new WaitForSeconds(0.8f);
        AttackRange.SetActive(false);
        MainGame.StopCoroutine(SetKickRange());
    }

    public void HaveTicket(bool have)
    {
        HavingTicket = have;
        myGameObject.transform.GetChild(1).gameObject.SetActive(HavingTicket);
    }
    
    private IEnumerator UpdatePosition(float vx)
    {
        int cnt = 0;
        while(cnt < 10)
        {
            Debug.Log("update " + cnt);
            myGameObject.transform.position += new Vector3(vx * 0.001f, 0, 0);
            cnt++;
            yield return new WaitForEndOfFrame();
        }
        MainGame.StopCoroutine(UpdatePosition(vx));
    }

}
