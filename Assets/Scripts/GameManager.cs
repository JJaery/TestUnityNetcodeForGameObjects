using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GameManager : MonoBehaviour
{
    #region SingleTon
    public static GameManager Instance
    {
        get
        {
            if (_Instance == null)
                _Instance = FindObjectOfType<GameManager>();
            return _Instance;
        }
    }

    private static GameManager _Instance;
    #endregion



    public static int LAYER_PLAYER {
        get => LayerMask.NameToLayer(LAYER_PLAYER_STRING);
    }
    public static string LAYER_PLAYER_STRING = "Player";

    public static int LAYER_OTHER_PLAYER
    {
        get => LayerMask.NameToLayer(LAYER_OTHER_PLAYER_STRING);
    }
    public static string LAYER_OTHER_PLAYER_STRING = "NPlayer";


    public PhysicMaterial groundMat;
    public PhysicMaterial playerMat;

    public Transform spawnPoint;

}
