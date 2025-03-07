using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Content;
using UnityEngine;
using Unity.Netcode;

public class MoleController : NetworkBehaviour
{
    //Network Variable
    public NetworkVariable<bool> IsActive = new NetworkVariable<bool>();
    public NetworkVariable<ulong> OwnerClientID = new NetworkVariable<ulong>();


    public float popUpSpeed = 5f; //mole pop up
    public float hideSpeed = 5f; //mole back
    public float stayDuration = 12f; //stay time

    private Vector3 hiddenPosition; //mole original hidden position
    private Vector3 targetPosition; //mole out position
    public bool isActive = false;
    
    // Start is called before the first frame update
    void Start()
    {
        hiddenPosition = transform.position + Vector3.down * 0.5f; // Invisible position
        targetPosition = transform.position + Vector3.up * 0.1f; // pop up; (visible position)
        Hide(); // hide the mole at the beginning
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PopUp()
    {
        if (isActive)
        {
            return;
        }
        isActive = true;
        StopAllCoroutines();
        StartCoroutine(MoveTo(targetPosition, popUpSpeed));
        StartCoroutine(HideAfterDelay());
    }

    public void Hide()
    {
        isActive = false;
        StopAllCoroutines();
        StartCoroutine(MoveTo(hiddenPosition,hideSpeed));
    }

    IEnumerator MoveTo(Vector3 target, float speed) // Enumerator make it moves like animation
    {
        while (Vector3.Distance(transform.position, target) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            yield return null;
        }
    }

    IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(stayDuration);
        Hide();
    }

    //When hit
    public void OnHit()
    {
        Hide();
    }

    public override void OnNetworkSpawn()
    {
        IsActive.OnValueChanged += OnActiveStateChanged;
    }

    private void OnActiveStateChanged(bool oldValue, bool newValue)
    {
        if (newValue)
            PopUp(); // To Show mole on every user
        else
            Hide();
    }

    [ServerRpc]
    public void PopUpServerRpc()
    {
        if (!IsActive.Value)
        {
            IsActive.Value = true;
            // if nobody plays
        }
    }

    [ServerRpc]
    public void HitServerRpc(ulong hitterClientId)
    {
        if (IsActive.Value)
        {
            IsActive.Value = false;
            // add score to visible to all user
            //GameManager.Instance.AddScore(hitterClientId, 10);
        }
    }
}
