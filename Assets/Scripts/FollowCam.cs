using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCam : MonoBehaviour
{
    public static FollowCam Instance
    {
        get
        {
            if (_Instacne == null)
                _Instacne = FindObjectOfType<FollowCam>();
            return _Instacne;
        }
    }
    private static FollowCam _Instacne;

    [Header("Normal")]
    public Vector3 offsetPosition;
    public Vector3 offsetRotation;

    [Header("ZoomOut")]
    public Vector3 offsetPositionZoomOut;
    public Vector3 offsetRotationZoomOut;

    public float zoomInTime = 1.0f;
    public AnimationCurve zoomInCurve;
    public AnimationCurve zoomOutCurve;

    private Vector3 _startRotation;
    /// <summary>
    /// 실시간 테스트를 위해 임시 추가
    /// </summary>
    private Vector3 _lastoffsetPosition;

    private Vector3 _resultPos;
    private Transform _target;
    private Player _script;
    private Coroutine _zoomInRotine;

    private Quaternion _smoothResultRotation;

    public void SetTarget(Transform target)
    {
        _script = target.GetComponent<Player>();
        _target = _script.model.transform;
        _startRotation = transform.rotation.eulerAngles;
        _lastoffsetPosition = offsetPosition;
        _resultPos = offsetPosition;
    }


    // Update is called once per frame
    void LateUpdate()
    {
        if (_lastoffsetPosition != offsetPosition)
        {
            _resultPos = offsetPosition;
            _lastoffsetPosition = offsetPosition;
        }

        if(_target != null)
        {
            transform.position = _target.transform.position + _resultPos;
            transform.rotation = Quaternion.Euler(_startRotation) * Quaternion.Euler(offsetRotation);

            if(UITestPanel.Instance.facingCamToggle.isOn)
            {
                transform.RotateAround(_target.position, Vector3.up, Vector3.SignedAngle(_script.model.transform.position, _script.model.transform.forward, Vector3.up) + 90);
            }
        }
    }

    public void ZoomIn()
    {
        if(_zoomInRotine != null)
        {
            StopCoroutine(_zoomInRotine);
        }
        _zoomInRotine = StartCoroutine(ZoomInRoutine(_resultPos, zoomInTime));
    }

    public void ZoomOut(float factor)
    {
        float curveFactor = zoomOutCurve.Evaluate(factor);
        _resultPos = Vector3.Lerp(offsetPosition, offsetPositionZoomOut, curveFactor);
    }


    private IEnumerator ZoomInRoutine(Vector3 startPos, float time)
    {
        float curTime = 0;
        while (curTime < time)
        {
            float curveFactor = zoomInCurve.Evaluate(curTime / time);
            _resultPos = Vector3.Lerp(startPos, offsetPosition, curveFactor);
            curTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
    }
}
