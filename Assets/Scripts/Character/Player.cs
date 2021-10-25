using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using System.IO;

public class Player : NetworkBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public SampleTest test;

    public SpriteRenderer arrowSprite;
    public Rigidbody physicBody;
    public SpriteRenderer cooldownSprite;
    public GameObject cooldownSpriteParent;
    public Transform model;
    public AnimatorController modelAnim;


    private Vector3 _power;

    private float? _startCoolDown = null;

    public NetworkVariable<float> coolDown = new NetworkVariable<float>(NetworkVariableReadPermission.OwnerOnly);

    public NetworkVariable<Vector3> position = new NetworkVariable<Vector3>(NetworkVariableReadPermission.OwnerOnly);

    public NetworkVariable<bool> isMovable = new NetworkVariable<bool>(NetworkVariableReadPermission.OwnerOnly);

    public NetworkVariable<Vector3> velocity = new NetworkVariable<Vector3>(NetworkVariableReadPermission.OwnerOnly);


    private bool _isDragging = false;

    private void Awake()
    {

        arrowSprite.gameObject.SetActive(false);
        cooldownSpriteParent.SetActive(false);

        if (IsServer)
        {
            transform.position = GameManager.Instance.spawnPoint.position;
            StartCoroutine(WaitUntilVelocityZero());
        }

        coolDown.OnValueChanged += OnChangeCoolDown;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        coolDown.OnValueChanged -= OnChangeCoolDown;
    }

    private void OnChangeCoolDown(float prev, float cur)
    {
        if (_startCoolDown == null || cur > prev)
        {
            _startCoolDown = cur;
        }

        if (cur > 0)
        {
            if (IsHost == true || IsServer == false)
            {
                cooldownSpriteParent.SetActive(true);
                cooldownSprite.transform.localScale = new Vector3(coolDown.Value / _startCoolDown.Value, 1, 1);
            }

            if (IsServer)
            {
                isMovable.Value = false;
            }
        }
        else
        {
            if (IsHost == true || IsServer == false)
            {
                cooldownSpriteParent.SetActive(false);
            }

            if (IsServer)
            {
                isMovable.Value = true;
            }
        }
    }


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    
        if (IsOwner)
        {
            FollowCam.Instance.SetTarget(transform);
            gameObject.layer = LayerMask.NameToLayer("Player");
        }
        else
        {
            gameObject.layer = LayerMask.NameToLayer("NPlayer");
        }

        if (IsServer == false)
        {
            Destroy(physicBody);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isMovable.Value == false) return;

        arrowSprite.gameObject.SetActive(true);
        Cursor.visible = false;
        _isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isMovable.Value == false || _isDragging == false) return;
        _power = new Vector3(0, 0, Mathf.Min(_power.z + eventData.delta.y, -0.01f));
        _power = Vector3.ClampMagnitude(_power, 100);
        FollowCam.Instance.ZoomOut(_power.magnitude * 0.01f);
        model.transform.rotation *= Quaternion.Euler(0, eventData.delta.x, 0);
        RotateModelServerRpc(eventData.delta.x);
        arrowSprite.transform.localScale = new Vector3(_power.magnitude * 0.014f, 0.5f, 1);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isMovable.Value == false || _isDragging == false) return;

        Cursor.visible = true;
        _isDragging = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.lockState = CursorLockMode.None;
        arrowSprite.gameObject.SetActive(false);
        FollowCam.Instance.ZoomIn();
        RequestMove();
        _power = Vector3.zero;
    }


    public void RequestMove()
    {
        if (isMovable.Value == false)
        {
            return;
        }

        MoveServerRpc(_power);
    }

    [ServerRpc]
    private void MoveServerRpc(Vector3 pow)
    {
        var forward = model.transform.forward;
        pow = forward * pow.magnitude;
        _power = pow * SampleTest.power.Value * 0.001f;
        Move();
    }

    private void Move()
    {
        arrowSprite.gameObject.SetActive(false);
        physicBody.AddForce(_power, ForceMode.Impulse);
        StartCoroutine(WaitUntilVelocityZero());
    }

    IEnumerator WaitUntilVelocityZero()
    {
        isMovable.Value = false;
        modelAnim.SetBool(AnimatorController.eAnimatorKey.IsRushing, true);
        yield return new WaitUntil(() => physicBody.velocity != Vector3.zero);
        yield return new WaitUntil(() => physicBody.velocity == Vector3.zero);
        modelAnim.SetBool(AnimatorController.eAnimatorKey.IsRushing, false);
        coolDown.Value = Random.Range(0f, 1f);
    }

    private void FixedUpdate()
    {
        if (IsServer)
            velocity.Value = physicBody.velocity;

        modelAnim.SetBool(AnimatorController.eAnimatorKey.IsRushing, velocity.Value != Vector3.zero);
    }

    [ServerRpc]
    private void RotateModelServerRpc(float deltaX)
    {
        model.transform.rotation *= Quaternion.Euler(0, deltaX, 0);
    }


    private void Update()
    {
        if (IsServer)
        {
            ServerUpdate();
        }
        else
        {
            ClientUpdate();
        }
    }

    private void ServerUpdate()
    {
        if (transform.position.y < -20)
        {
            ResetPosition();
        }

        position.Value = transform.position;

        if (coolDown.Value > 0)
        {
            coolDown.Value -= Time.deltaTime;
        }
    }

    private void ClientUpdate()
    {
        transform.position = position.Value;
    }

    private void ResetPosition()
    {
        transform.position = GameManager.Instance.spawnPoint.position;
        physicBody.velocity = Vector3.zero;
    }
}