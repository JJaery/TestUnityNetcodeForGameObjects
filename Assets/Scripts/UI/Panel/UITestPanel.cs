using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UNET;

public class UITestPanel : MonoBehaviour
{
    #region SingleTon
    public static UITestPanel Instance
    {
        get
        {
            if (_Instance == null)
                _Instance = FindObjectOfType<UITestPanel>();
            return _Instance;
        }
    }

    private static UITestPanel _Instance;
    #endregion

    public GameObject connectionObject;
    public GameObject ingameObject;
    public InputField addressInputField;
    public InputField powerInput;
    public Toggle facingCamToggle;

    [System.Serializable]
    public class PhysicMaterialUIData
    {
        public InputField dynamicFrition;
        public InputField staticFrition;
        public Slider bounciess;
        public Text bounciessText;

        public void Init(NetworkVariable<float> valDynamic, NetworkVariable<float> valStatic, NetworkVariable<float> valBounciess, System.Action onDirty)
        {
            bounciess.onValueChanged.AddListener(OnSliderChange);

            dynamicFrition.text = $"{valDynamic.Value}";
            staticFrition.text = $"{valStatic.Value}";
            bounciess.value = valBounciess.Value;

            dynamicFrition.onValueChanged.AddListener((text) =>
            {
                if(float.TryParse(text,out float res))
                {
                    valDynamic.Value = res;
                    onDirty?.Invoke();
                }
            });
            staticFrition.onValueChanged.AddListener((text) =>
            {
                if (float.TryParse(text, out float res))
                {
                    valStatic.Value = res;
                    onDirty?.Invoke();
                }
            });
            bounciess.onValueChanged.AddListener((val) =>
            {
                valBounciess.Value = val;
                onDirty?.Invoke();
            });


            valDynamic.OnValueChanged += (prev, cur) =>
            {
                dynamicFrition.text = $"{cur}";
            };
            valStatic.OnValueChanged += (prev, cur) =>
            {
                staticFrition.text = $"{cur}";
            };
            valBounciess.OnValueChanged += (prev, cur) =>
            {
                bounciess.value = cur;
            };

        }

        private void OnSliderChange(float val)
        {
            bounciessText.text = $"{val:0.00}";
        }
    }

    public PhysicMaterialUIData playerMatUI;
    public PhysicMaterialUIData groundMatUI;

    private void Awake()
    {
        connectionObject.SetActive(true);
        ingameObject.SetActive(false);
    }

    public void OnClickMulti()
    {
        var trans = NetworkManager.Singleton.GetComponent<UNetTransport>();
        trans.ConnectAddress = addressInputField.text;
        NetworkManager.Singleton.StartClient();

        connectionObject.SetActive(false);
        ingameObject.SetActive(true);
    }

    public void OnClickSingle()
    {
        NetworkManager.Singleton.StartHost();

        connectionObject.SetActive(false);
        ingameObject.SetActive(true);
    }
}
