using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimatorController : MonoBehaviour
{
    private Animator _animator;

    private void Awake()
    {
        if(_animator == null)
        {
            _animator = GetComponent<Animator>();
        }
    }

    public void SetBool(eAnimatorKey key, bool value)
    {
        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }

        _animator.SetBool(key.ToString(), value);
    }

    public enum eAnimatorKey
    {
        IsRushing,
    }
}
