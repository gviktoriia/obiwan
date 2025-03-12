using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Content;
using UnityEngine;
using Unity.Netcode;
using static GameManager;

public class MoleController : NetworkBehaviour
{
    //Network Variable
    public NetworkVariable<bool> IsActive = new NetworkVariable<bool>();
    public NetworkVariable<ulong> OwnerClientID = new NetworkVariable<ulong>();


    public float popUpSpeed = 5f; //mole pop up
    public float hideSpeed = 5f; //mole back
    public float _currentStayDuration = 15f; //stay time

    private Vector3 hiddenPosition; //mole original hidden position
    private Vector3 targetPosition; //mole out position
    public bool isActive = false;
    
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("[MoleController] Mole initialized.");
        hiddenPosition = transform.position + Vector3.down * 0.5f; // Invisible position
        targetPosition = transform.position + Vector3.up * 0.05f; // pop up; (visible position)
        Hide(); // hide the mole at the beginning
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetStayDuration(float duration)
    {
        _currentStayDuration = duration;
    }

    public void PopUp()
    {
        if (isActive)
            return;

        Debug.Log($"[MoleController] Mole popping up. Active before: {GameManager.Instance.activeMoles.Value}");

        isActive = true;
        IsActive.Value = true;
        StopAllCoroutines();
        StartCoroutine(MoveTo(targetPosition, popUpSpeed));
        StartCoroutine(HideAfterDelay());

        // 确保在服务器端增加 activeMoles
        if (IsServer)
        {
            GameManager.Instance.IncreaseActiveMoles();
        }
    }

    public void Hide()
    {
        if (!isActive) return; // 避免重复执行
        Debug.Log($"[MoleController] Mole hiding. Active before: {GameManager.Instance.activeMoles.Value}");

        isActive = false;
        IsActive.Value = false;
        StopAllCoroutines();
        StartCoroutine(MoveTo(hiddenPosition, hideSpeed));

        // 确保在服务器端减少 activeMoles
        if (IsServer)
        {
            GameManager.Instance.DecreaseActiveMoles();
        }
    }

    IEnumerator MoveTo(Vector3 target, float speed) // Enumerator make it moves like animation
    {
        while (Vector3.Distance(transform.position, target) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            yield return null;
            Debug.Log($"[MoleController] Mole current position: {transform.position}.");
        }
        Debug.Log($"[MoleController] Moving from {transform.position} to {target}.");
    }

    IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(_currentStayDuration);
        Debug.Log($"[MoleController] Stay duration ended. Hiding mole.");
        Hide();
        
        //GameManager.Instance.activeMoles.Value--;
        Debug.Log($"[MoleController] Mole hidden after stay duration. Active moles: {GameManager.Instance.activeMoles.Value}");
    }

    //When hit
    public void OnHit()
    {
        Hide();
        //GameManager.Instance.activeMoles.Value--;
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log($"[MoleController] Mole spawned on network.Is Server: {IsServer}");
        IsActive.OnValueChanged += OnActiveStateChanged;
    }

    private void OnActiveStateChanged(bool oldValue, bool newValue)
    {
        Debug.Log($"[MoleContrller] Mole active state changed from {oldValue} to {newValue}.");
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
            // GameManager.Instance.activeMoles.Value++;
            StartCoroutine(HideAfterDelay());//?
            Debug.Log($"[Server] Mole popped up. Active moles now: {GameManager.Instance.activeMoles.Value}");
        }
    }

    [ServerRpc]
    public void HitServerRpc(ulong hitterClientId)
    {
        Debug.Log($"[MoleController] HitServerRpc called. IsActive: {IsActive.Value}");
        if (IsActive.Value)
        {
            IsActive.Value = false;
            // GameManager.Instance.activeMoles.Value--;
            GameManager.Instance.AddScoreServerRpc(hitterClientId, 10);
            Debug.Log($"[MoleController] Mole hidden. Active moles: {GameManager.Instance.activeMoles.Value}");
        }
    }
}
