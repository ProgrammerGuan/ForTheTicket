using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ControlOrder
{
    moveLeft,
    moveRight,
    Jump,
    Kick,
    Idle,
    None,
}

public class CharacterController
{
    public CharacterController()
    {
 
    }

    public ControlOrder GetControl()
    {
        if (Do("action"))
        {
            if (Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.K)) return ControlOrder.Kick;
            else if (Input.GetKey(KeyCode.X) || Input.GetKey(KeyCode.J) && Do("jump")) return ControlOrder.Jump;
            else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) return ControlOrder.moveLeft;
            else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) return ControlOrder.moveRight;
            else return ControlOrder.Idle;
        }
        return ControlOrder.None;
    }

    public bool Do(string action)
    {
        switch (action)
        {
            case "jump":
                return Time.time - Parameters.JumpTime > Parameters.JumpCoolDownTime;
            case "action":
                return Time.time - Parameters.ActionTime > Parameters.KickCoolDownTime;
            default:
                return false;
        }
    }
}