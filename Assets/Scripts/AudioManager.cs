using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
   [Tooltip("Attack Sounds On Hit Medal")]
   [SerializeField]
   private AudioClip[] AttackSounds;
   [SerializeField]
   private AudioClip concreteSounds;
   [SerializeField]
   private AudioClip[] HitSounds;
   public static AudioManager Instance;
    private static bool isPaused = false; // Biến tạm dừng âm thanh
    private void Awake()
   {
      Instance = this;
   }

    public AudioClip GetAudioAttackClip()
   {
      return AttackSounds[Random.Range(0, AttackSounds.Length)];
   }
   public AudioClip GetAudioHitClip()
   {
      return HitSounds[Random.Range(0, HitSounds.Length)];
   }   
   public AudioClip GetAudioConcreteClip()
   {
      return concreteSounds;
   }
}
