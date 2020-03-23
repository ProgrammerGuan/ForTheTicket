using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character
{
    MainGame MainGame;
    GameObject myGameObject;
    Animator Animator;
    CharacterDetector Detector;
    GameObject AttackRange;
    string Name;
    public bool TurnFlag;
    public Character(MainGame mainGame,string name)
    {
        Name = name;
        TurnFlag = false;
        myGameObject = GameObject.Find(Name);
        Animator = myGameObject.GetComponent<Animator>();
        Detector = myGameObject.GetComponent<CharacterDetector>();
        MainGame = mainGame;
        AttackRange = myGameObject.transform.GetChild(0).gameObject;
        AttackRange.SetActive(false);
    }
    public Transform transform => myGameObject.transform;

    public void Action(ControlOrder control)
    {

        switch (control)
        {
            case ControlOrder.moveLeft:
                if (myGameObject.transform.eulerAngles != new Vector3(0, -180, 0))
                    Turn("left");
                Move("left");
                break;
            case ControlOrder.moveRight:
                if (myGameObject.transform.eulerAngles != new Vector3(0, 0, 0))
                    Turn("right");
                Move("right");
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
        MainGame.StartCoroutine(SetAction("Jump",Parameters.JumpCoolDownTime));
        myGameObject.GetComponent<Rigidbody2D>().velocity = Vector3.up*5;
    }

    private void Kick()
    {
        Parameters.ActionTime = Time.time;
        if (myGameObject.transform.eulerAngles == new Vector3(0, 0, 0))
        {
            if (Animator.GetBool("Walk") || Animator.GetBool("Jump"))
                myGameObject.GetComponent<Rigidbody2D>().velocity = Vector3.right * 6;
            else myGameObject.GetComponent<Rigidbody2D>().velocity = Vector3.right * 4;

        }
        else
        {
            if (Animator.GetBool("Walk") || Animator.GetBool("Jump"))
                myGameObject.GetComponent<Rigidbody2D>().velocity = Vector3.left * 6;
            else myGameObject.GetComponent<Rigidbody2D>().velocity = Vector3.left * 4;
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

    private void Move(string moveForward)
    {
        if (!Animator.GetBool("Jump"))
            SetAnimation("Walk");
        switch (moveForward)
        {
            case "right":
                myGameObject.transform.position += new Vector3(Parameters.MoveSpeed, 0, 0);
                break;
            case "left":
                myGameObject.transform.position -= new Vector3(Parameters.MoveSpeed, 0, 0);
                break;
        }
    }

    private void Turn(string turnTo)
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
    }

    public void GotDamage()
    {
        SetAnimation("GotDamage");
        Animator.SetTrigger("GotDamage");
    }

    private IEnumerator SetKickRange()
    {
        yield return new WaitForSeconds(0.3f);
        AttackRange.SetActive(true);
        yield return new WaitForSeconds(0.8f);
        AttackRange.SetActive(false);
    }
    
}
