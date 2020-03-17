using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character
{
    MainGame MainGame;
    GameObject myGameObject;
    Animator Animator;
    bool KickFlag;
    public Character(MainGame mainGame)
    {
        myGameObject = GameObject.FindGameObjectWithTag("Player");
        Animator = myGameObject.GetComponent<Animator>();
        KickFlag = false;
        MainGame = mainGame;
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
                KickFlag = false;
                SetAnimation("Idle");
                break;
            }
        
        
    }

    private void Jump()
    {
        Parameters.JumpTime = Time.time;
        MainGame.StartCoroutine(SetJumpAnimation());
        myGameObject.GetComponent<Rigidbody2D>().velocity = Vector3.up*5;
    }

    private void Kick()
    {
        Parameters.ActionTime = Time.time;
        KickFlag = true;
        SetAnimation("Kick");
    }



    private void SetAnimation(string action)
    {
        Animator.SetBool("Walk", (action == "Walk") ? true : false);
        Animator.SetBool("Jump", (action == "Jump") ? true : false);
        Animator.SetBool("Kick", (action == "Kick") ? true : false);

    }

    private void Move(string moveForward)
    {
        if(!Animator.GetBool("Jump"))
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
                break;
            case "left":
                myGameObject.transform.eulerAngles = new Vector3(0, -180, 0);
                break;
        }
        
    }

    private IEnumerator SetJumpAnimation()
    {
        SetAnimation("Jump");
        yield return new WaitForSeconds(Parameters.JumpCoolDownTime);
        Animator.SetBool("Jump", false);
    }
}
