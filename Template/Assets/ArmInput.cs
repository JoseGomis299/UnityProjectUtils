using UnityEngine;

public static class ArmInput{
    
    //Inputs
    public static KeyCode inputForSignal1 = KeyCode.F;
    public static KeyCode inputForSignal2 = KeyCode.J;
    public static KeyCode inputForSignal3 = KeyCode.D;
    public static KeyCode inputForSignal4 = KeyCode.K;
    
    //Cada señal es equivalente a un eje
    public enum Signal{
        S1, S2, S3, S4,
        LBiceps, RBiceps, LTriceps, RTriceps
    }

    public static float GetSignal(string SignalName) {
        switch (SignalName) {
            case "S1":
            case "LBiceps":
                return Input.GetKey(inputForSignal1)?  1 :  0;
            case "S2":
            case "RBiceps":
                return Input.GetKey(inputForSignal2)?  1 :  0;
            case "S3":
            case "LTriceps":
                return Input.GetKey(inputForSignal3)?  1 :  0;
            case "S4":
            case "RTriceps":
                return Input.GetKey(inputForSignal4)?  1 :  0;
        }
        return 0;
    }
    public static float GetSignal(Signal signal)
    {
        switch (signal)
        {
            case Signal.S1:
            case Signal.LBiceps:
                return Input.GetKey(inputForSignal1)?  1 :  0;
            case Signal.S2:
            case Signal.RBiceps:
                return Input.GetKey(inputForSignal2)?  1 :  0;
            case Signal.S3:
            case Signal.LTriceps:
                return Input.GetKey(inputForSignal3)?  1 :  0;
            case Signal.S4:
            case Signal.RTriceps:
                return Input.GetKey(inputForSignal4)?  1 :  0;
        }
        return 0;
    }

    public static Vector3 GetAxisLeftArm()
    {
        return new Vector3(GetSignal(Signal.LBiceps), GetSignal(Signal.LTriceps));
    }

    public static Vector3 GetAxisRightArm()
    {
        return new Vector3(GetSignal(Signal.RBiceps), GetSignal(Signal.RTriceps));
    }
}