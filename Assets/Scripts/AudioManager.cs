using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
   [Tooltip("Attack Sounds On Hit Medal")]
   [SerializeField]
   private AudioClip[] AttackSounds;
   [SerializeField]
   private AudioClip[] HitSounds;
   public static AudioManager Instance;
    private static bool isPaused = false; // Biến tạm dừng âm thanh
    private void Awake()
   {
      Instance = this;
   }
    //public static void PauseAudio(bool pause)
    //{
    //    isPaused = pause;
    //    if (pause)
    //    {
    //        AudioListener.pause = true; // Tạm dừng tất cả âm thanh
    //    }
    //    else
    //    {
    //        AudioListener.pause = false; // Phát lại tất cả âm thanh
    //    }
    //}
    //void Update()
    //{
    //    // Kiểm tra trạng thái Time.timeScale
    //    if (Time.timeScale == 0)
    //    {
    //        PauseAudio(true);
    //    }
    //    else
    //    {
    //        PauseAudio(false);
    //    }
    //}
    public AudioClip GetAudioAttackClip()
   {
      return AttackSounds[Random.Range(0, AttackSounds.Length)];
   }
   public AudioClip GetAudioHitClip()
   {
      return HitSounds[Random.Range(0, HitSounds.Length)];
   }
}
