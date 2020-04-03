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
    public bool CanMove;
    private bool HavingTicket;
    private IEnumerator updatePositionCoroutine;
    private bool CRisRunning;
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
        CRisRunning = false;
        CanMove = true;
    }
    public Transform transform => myGameObject.transform;

    public void Action(ControlOrder control, float x, float y, bool turn, bool fromServer, float vx)
    {

        switch (control)
        {
            case ControlOrder.moveLeft:
                //Debug.Log("move left");
                if (myGameObject.transform.eulerAngles != new Vector3(0, -180, 0))
                    Turn("left");
                if (fromServer) Move(x, y, vx);
                else Move(-Parameters.MoveSpeed + myGameObject.transform.position.x, myGameObject.transform.position.y);
                break;
            case ControlOrder.moveRight:
                //Debug.Log("move right");
                if (myGameObject.transform.eulerAngles != new Vector3(0, 0, 0))
                    Turn("right");
                if (fromServer) Move(x, y, vx);
                else Move(Parameters.MoveSpeed + myGameObject.transform.position.x, myGameObject.transform.position.y);
                break;
            case ControlOrder.Jump:
                if (!fromServer && (Parameters.CanJump > Time.time || Animator.GetBool("Jump"))) break;
                else if (Animator.GetBool("Jump")) break;
                Jump();
                break;
            case ControlOrder.Kick:
                if (fromServer && CRisRunning) MainGame.StopCoroutine(updatePositionCoroutine);
                Kick();
                break;
            case ControlOrder.Idle:
                //if(fromServer) Debug.Log("Idle turn is " + turn);
                if (fromServer && CRisRunning) MainGame.StopCoroutine(updatePositionCoroutine);
                if(fromServer) Idle(x, y, turn);
                else Idle(x, y,TurnFlag);
                break;
            }
        
        
    }


    private void Idle(float x,float y,bool turn)
    {
        if (!Animator.GetBool("Jump")) SetAnimation("Idle");
        myGameObject.transform.position = new Vector3(x, y);
        if (turn) Turn("left");
        else Turn("right");
    }

    private void Jump()
    {
        SetAnimation("Jump");
        //Debug.Log("Add force");
        //Debug.Log(Name + " Jump");
        //GameObject.Find(Name).GetComponent<Rigidbody2D>().velocity *= Vector2.right;
        var rigidbody = myGameObject.GetComponent<Rigidbody2D>();
        if (rigidbody.velocity.y > 0) rigidbody.velocity = new Vector2(rigidbody.velocity.x,0);
        rigidbody.AddForce(Vector2.up * Parameters.JumpHeight, ForceMode2D.Impulse);
    }

    public void EndJump()
    {
        //Debug.Log("End Jump");
        SetAnimation("Idle");
    }


    private void Kick()
    {
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
        MainGame.StartCoroutine(SetAction("Kick", Parameters.KickCoolDownTime-0.1f));
        MainGame.StartCoroutine(SetKickRange());
    }



    private void SetAnimation(string action)
    {
        if (!Animator.GetBool(action))
        {
            Animator.SetBool("Kick", (action == "Kick") ? true : false);
            Animator.SetBool("Jump", (action == "Jump") ? true : false);
            Animator.SetBool("Walk", (action == "Walk") ? true : false);
            Animator.SetBool("Idle", (action == "Idle") ? true : false);
            //Debug.Log(Name + " Animation Set" + action);
            //Debug.Log("Idle is " + Animator.GetBool("Idle"));
            //Debug.Log("Jump is " + Animator.GetBool("Jump"));
        }
            
    }

    private void Move(float x,float y,float vx)
    {
        Move(x, y);
        if (CRisRunning)
        {
            CRisRunning = false;
            MainGame.StopCoroutine(updatePositionCoroutine);
        }
        updatePositionCoroutine = UpdatePosition(vx);
        MainGame.StartCoroutine(updatePositionCoroutine);
    }

    private void Move(float x, float y)
    {
        if (!Animator.GetBool("Jump"))
            SetAnimation("Walk");
        if (CanMove) myGameObject.transform.position = new Vector3(x, y, 0);
            
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
        if(MainGame.myName == Name) Parameters.DamageTime = Time.time;
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

    public void AnimatorFrameStop()
    {
        var HitStop = 0.1f;
        MainGame.StartCoroutine(ResumeAnimator(HitStop));
    }

    IEnumerator ResumeAnimator(float hitStop)
    {
        yield return new WaitForSeconds(hitStop);
        var rigidbody = myGameObject.GetComponent<Rigidbody2D>();
        Vector2 nowSpeed = rigidbody.velocity;
        Debug.Log(Name + "ori velocity : " + nowSpeed);
        rigidbody.velocity = Vector2.zero;
        Debug.Log(Name + "time up velocity : " + rigidbody.velocity);
        Animator.speed = 0;
        yield return new WaitForSeconds(hitStop);
        rigidbody.velocity = nowSpeed;
        Debug.Log(Name + "reset velocity : " + nowSpeed);
        Animator.speed = 1;
        
    }

    private IEnumerator SetKickRange()
    {
        yield return new WaitForSeconds(0.3f);
        AttackRange.SetActive(true);
        yield return new WaitForSeconds(0.65f);
        AttackRange.SetActive(false);
        MainGame.StopCoroutine(SetKickRange());
    }

    public void HaveTicket(bool have)
    {
        HavingTicket = have;
        myGameObject.transform.GetChild(1).gameObject.SetActive(HavingTicket);
    }

    float UpdatePoCnt = 0;
    private IEnumerator UpdatePosition(float vx)
    {
        CRisRunning = true;
        UpdatePoCnt++;
        int cnt = 0;
        while(cnt < Parameters.SendDistance)
        {
            //Debug.Log("vx is " + vx);
            //Debug.Log("update " + UpdatePoCnt + " - " + cnt);
            if (CanMove) myGameObject.transform.position += new Vector3(vx * 0.001f, 0, 0);
            cnt++;
            yield return new WaitForSeconds(0.001f);
        }
    }

}
