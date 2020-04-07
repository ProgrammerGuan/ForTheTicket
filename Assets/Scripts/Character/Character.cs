using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character
{
    //ゲームマネージャー
    MainGame MainGame;
    //このキャラクターのオブジェクト
    GameObject myGameObject;
    //このキャラクターの資料
    public Animator Animator;
    CharacterDetector Detector;
    GameObject AttackRange;
    public Transform transform => myGameObject.transform;
    string Name;
    public string CharacterName;
    public bool TurnFlag;
    public bool CanMove;
    private bool HavingTicket;
    public int kickCnt;
    //移動の計算
    private IEnumerator updatePositionCoroutine;
    private bool CRisRunning;
    public Character(MainGame mainGame,string name,string characterName)
    {
        //ゲームマネージャー設定
        MainGame = mainGame;
        
        //キャラクターの資料設定
        Name = name;
        CharacterName = characterName;
        TurnFlag = false;
        myGameObject = GameObject.Find(Name);
        Animator = myGameObject.GetComponent<Animator>();
        Detector = myGameObject.GetComponent<CharacterDetector>();
        AttackRange = myGameObject.transform.GetChild(0).gameObject;
        AttackRange.SetActive(false);
        HavingTicket = false;
        CanMove = true;
        kickCnt = 0;
        //移動計算動く判断
        CRisRunning = false;
        
    }

    #region キャラクターのアクション
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
            case ControlOrder.Skill:
                Skill();
                break;
            }
        
        
    }

    private void Skill()
    {
        Animator.SetTrigger("Skill");
        MainGame.StartCoroutine(SkillMove());
    }

    IEnumerator SkillMove()
    {
        var d = Vector3.up * 2;
        var goalPos = myGameObject.transform.position + d;
        var rigidbody = myGameObject.GetComponent<Rigidbody2D>();
        rigidbody.gravityScale = 0;
        rigidbody.velocity = 2 * d;
        while(myGameObject.transform.position.y < goalPos.y && rigidbody.velocity != Vector2.zero)
        {
            yield return new WaitForSeconds(0.01f);
        }
        rigidbody.velocity = Vector2.zero;
        yield return new WaitForSeconds(1.4f);
        rigidbody.gravityScale = 1;
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
        var rigidbody = myGameObject.GetComponent<Rigidbody2D>();
        if (rigidbody.velocity.y > 0) rigidbody.velocity = new Vector2(rigidbody.velocity.x,0);
        rigidbody.AddForce(Vector2.up * Parameters.JumpHeight, ForceMode2D.Impulse);
    }

    public void EndJump()
    {
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

    private void Move(float x, float y, float vx)
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

    private IEnumerator SetAction(string action, float time)
    {
        SetAnimation(action);
        yield return new WaitForSeconds(time);
        Animator.SetBool(action, false);
        MainGame.StopCoroutine(SetAction(action, time));
    }

    public void GotDamage(bool GotDamageForward)
    {
        if (MainGame.myName == Name) Parameters.DamageTime = Time.time + Parameters.DamageCoolDownTime;
        //Right side got damage
        if (GotDamageForward) Turn("right");
        else Turn("left");  // Left side got damage
        SetAnimation("GotDamage");
        Animator.SetTrigger("GotDamage");
        if (GotDamageForward) myGameObject.GetComponent<Rigidbody2D>().velocity = Vector3.left * Parameters.GotDamageDistance;
        else myGameObject.GetComponent<Rigidbody2D>().velocity = Vector3.right * Parameters.GotDamageDistance;
        if (HavingTicket)
        {
            MainGame.FallTicket(Name, myGameObject.transform.position.x, myGameObject.transform.position.y + 1);
            HaveTicket(false);
        }
    }

    #endregion

    #region アニメ
    private void SetAnimation(string action)
    {
        if (!Animator.GetBool(action))
        {
            Animator.SetBool("Kick", (action == "Kick") ? true : false);
            Animator.SetBool("Jump", (action == "Jump") ? true : false);
            Animator.SetBool("Walk", (action == "Walk") ? true : false);
            Animator.SetBool("Idle", (action == "Idle") ? true : false);
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
    #endregion

    #region キャラクターの資料
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
            if (CanMove) myGameObject.transform.position += new Vector3(vx * 0.001f, 0, 0);
            cnt++;
            yield return new WaitForSeconds(0.001f);
        }
    }
    #endregion
}
