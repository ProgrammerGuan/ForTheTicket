using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable] public class SensorEvent : UnityEvent<Collider2D> { }

public class Sensor : MonoBehaviour
{
    public SensorEvent OnTriggerEnter;
    public SensorEvent OnTriggerStay;
    public SensorEvent OnTriggerExit;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        OnTriggerEnter.Invoke(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        OnTriggerStay.Invoke(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        OnTriggerExit.Invoke(collision);
    }
}
