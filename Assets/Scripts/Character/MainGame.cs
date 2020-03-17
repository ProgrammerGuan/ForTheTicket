using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainGame : MonoBehaviour
{
    CharacterController Controller;
    Character Character;
    // Start is called before the first frame update
    private void Awake()
    {
        Character = new Character(this);
        Controller = new CharacterController();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Character.Action(Controller.GetControl());

    }
}
