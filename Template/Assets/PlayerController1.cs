using System;
using System.Collections;
using System.Collections.Generic;
using ProjectUtils.TopDown2D;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController1 : Mover 
{ 
    private Vector3 _direction;

    private void FixedUpdate()
    {
        UpdateMotor(_direction);
    }


}
