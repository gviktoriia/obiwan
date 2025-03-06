using UnityEngine;

public class ObjectAccessHandler : MonoBehaviour
{
    private bool isAccessGranted = false;

    public bool RequestAccess
    {
        get
        {
            if (!isAccessGranted)
            {
                isAccessGranted = true;
                return true;
            }
            return false;
        }
    }

    public void Release()
    {
        isAccessGranted = false;
    }
}


