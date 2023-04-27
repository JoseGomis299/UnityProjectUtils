
using Templates.TopDownMovement;
using UnityEngine;

public class PlayerController1 : Mover 
{ 
    private Vector3 _direction;

    private void FixedUpdate()
    {
        UpdateMotor(_direction);
    }


}
