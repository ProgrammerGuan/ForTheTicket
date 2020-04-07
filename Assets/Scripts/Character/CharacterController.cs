﻿using System.Collections;
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
    public ControlOrder GetControl()
    {
        if (Do("action"))
        {
            if (Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.K))
            {
                Parameters.ActionTime = Time.time;
                return ControlOrder.Kick;
            }
            else if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.J)) return ControlOrder.Jump;
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
            case "action":
                return Time.time - Parameters.ActionTime > Parameters.KickCoolDownTime && Time.time - Parameters.DamageTime > Parameters.DamageCoolDownTime;
            default:
                return false;
        }
    }
}