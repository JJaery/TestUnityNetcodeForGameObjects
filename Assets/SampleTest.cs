using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SampleTest : NetworkBehaviour
{
    public static NetworkVariable<float> power = new NetworkVariable<float>(NetworkVariableReadPermission.Everyone);

    public static NetworkVariable<float> groundDynamicFrition = new NetworkVariable<float>(NetworkVariableReadPermission.Everyone);

    public static NetworkVariable<float> groundStaticFrition = new NetworkVariable<float>(NetworkVariableReadPermission.Everyone);

    public static NetworkVariable<float> groundBounciness = new NetworkVariable<float>(NetworkVariableReadPermission.Everyone);

    public static NetworkVariable<float> playerDynamicFrition = new NetworkVariable<float>(NetworkVariableReadPermission.Everyone);

    public static NetworkVariable<float> playerStaticFrition = new NetworkVariable<float>(NetworkVariableReadPermission.Everyone);

    public static NetworkVariable<float> playerBounciness = new NetworkVariable<float>(NetworkVariableReadPermission.Everyone);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            playerDynamicFrition.Value = GameManager.Instance.playerMat.dynamicFriction;
            playerStaticFrition.Value = GameManager.Instance.playerMat.staticFriction;
            playerBounciness.Value = GameManager.Instance.playerMat.bounciness;

            groundDynamicFrition.Value = GameManager.Instance.groundMat.dynamicFriction;
            groundStaticFrition.Value = GameManager.Instance.groundMat.staticFriction;
            groundBounciness.Value = GameManager.Instance.groundMat.bounciness;
            power.Value = 500;
        }
        
        if (IsOwner)
        {
            PowerUIInit();
            UITestPanel.Instance.playerMatUI.Init(playerDynamicFrition, playerStaticFrition, playerBounciness, SendToServer);
            UITestPanel.Instance.groundMatUI.Init(groundDynamicFrition, groundStaticFrition, groundBounciness, SendToServer);
        }

        StartCoroutine(RefreshPerSec());
    }


    private IEnumerator RefreshPerSec()
    {
        while (true)
        {
            if (IsServer)
            {
                GameManager.Instance.playerMat.dynamicFriction = playerDynamicFrition.Value;
                GameManager.Instance.playerMat.staticFriction = playerStaticFrition.Value;
                GameManager.Instance.playerMat.bounciness = playerBounciness.Value;

                GameManager.Instance.groundMat.dynamicFriction = groundDynamicFrition.Value;
                GameManager.Instance.groundMat.staticFriction = groundStaticFrition.Value;
                GameManager.Instance.groundMat.bounciness = groundBounciness.Value;

                RefreshClientRPC(GameManager.Instance.playerMat.dynamicFriction,
                    GameManager.Instance.playerMat.staticFriction,
                    GameManager.Instance.playerMat.bounciness,
                    GameManager.Instance.groundMat.dynamicFriction,
                    GameManager.Instance.groundMat.staticFriction,
                    GameManager.Instance.groundMat.bounciness,
                    power.Value);

                yield return new WaitForSeconds(1);
            }
            else
            {
                break;
            }
        }
    }

    [ClientRpc]
    private void RefreshClientRPC(params float[] datas)
    {
        if (IsOwner)
        {
            playerDynamicFrition.Value = datas[0];
            playerStaticFrition.Value = datas[1];
            playerBounciness.Value = datas[2];

            groundDynamicFrition.Value = datas[3];
            groundStaticFrition.Value = datas[4];
            groundBounciness.Value = datas[5];
            power.Value = datas[6];
        }
    }

    [ServerRpc]
    private void RefreshServerRPC(params float[] datas)
    {
        playerDynamicFrition.Value = datas[0];
        playerStaticFrition.Value = datas[1];
        playerBounciness.Value = datas[2];

        groundDynamicFrition.Value = datas[3];
        groundStaticFrition.Value = datas[4];
        groundBounciness.Value = datas[5];
        power.Value = datas[6];
    }

    private void SendToServer()
    {
        RefreshServerRPC(playerDynamicFrition.Value,
         playerStaticFrition.Value,
         playerBounciness.Value,
         groundDynamicFrition.Value,
         groundStaticFrition.Value,
         groundBounciness.Value,
         power.Value);
    }


    private void PowerUIInit()
    {
        UITestPanel.Instance.powerInput.text = $"{power.Value}";
        UITestPanel.Instance.powerInput.onValueChanged.AddListener((text) =>
        {
            if (string.IsNullOrWhiteSpace(text) == false)
            {
                power.Value = System.Convert.ToSingle(text);
            }
        });

        power.OnValueChanged += (prev, cur) =>
        {
            UITestPanel.Instance.powerInput.text = $"{power.Value}";
            SendToServer();
        };
    }
}
