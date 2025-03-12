using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using TMPro;

public class PlayerInfo : NetworkBehaviour
{
    public NetworkVariable<string> userName = new NetworkVariable<string>("Guest");
    public TMP_Text userNameLabel; // 场景中显示用户名的UI文本

    public override void OnNetworkSpawn()
    {
        // 如果是本地玩家，设置用户名
        if (IsOwner)
        {
            SetUserNameServerRpc("Player_" + Random.Range(1000, 9999));
        }

        // 同步用户名到UI
        userName.OnValueChanged += (oldName, newName) =>
        {
            userNameLabel.text = newName;
        };
    }

    [ServerRpc]
    private void SetUserNameServerRpc(string name)
    {
        userName.Value = name;
    }
}
