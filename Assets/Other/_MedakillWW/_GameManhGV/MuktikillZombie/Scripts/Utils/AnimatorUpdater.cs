using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimatorUpdater : MonoBehaviour
{
    // [SerializeField] private TimeCategory _updateMode;
    // private Animator _animator;
    //
    // private void Awake()
    // {
    //     _animator = GetComponent<Animator>();
    // }
    //
    // private void Update()
    // {
    //     switch (_updateMode)
    //     {
    //         case TimeCategory.General:
    //             _animator.Update(Time.deltaTime);
    //             break;
    //         case TimeCategory.Player:
    //             _animator.Update(PlayerTime.deltaTime);
    //             break;
    //         case TimeCategory.Game:
    //             _animator.Update(GameTime.deltaTime);
    //             break;
    //         case TimeCategory.Exactly:
    //             _animator.Update(Time.unscaledDeltaTime);
    //             break;
    //         default:
    //             throw new ArgumentOutOfRangeException();
    //     }
    // }
}