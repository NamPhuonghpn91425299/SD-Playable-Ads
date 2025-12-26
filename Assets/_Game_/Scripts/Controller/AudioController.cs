using System.Collections;
using Assets._Develop_.ThanhNT.Scripts.Observer;
using UnityEngine;
using static GameConstants;

public class AudioController : MonoBehaviour,
IObserver<GameStateChangedEvent>,

IObserver<BossSpawnEvent>
{
    [Header("Audio Clips")]
    [SerializeField] private SoundSource _backgroundMusicClips;
    [SerializeField] private SoundSource _enemyCallTeamMoveSoundClips;
    [SerializeField] private SoundSource _enemyCallTeamAttackSoundClips;


    [Header("Audio Config")]
    [SerializeField] private AudioConfig _audioConfig_EnemyCallTeamMoveSoundClips;
    [SerializeField] private AudioConfig _audioConfig_EnemyCallTeamAttackSoundClips;
    [SerializeField] private AudioConfig _audioConfig_EnemySuicideSoundClips;

    // Cache cho coroutines để tránh chạy nhiều cùng lúc
    private Coroutine _moveTeamSoundCoroutine;
    private Coroutine _attackTeamSoundCoroutine;
    private bool isPlayEndGameAudio = true;

    void Start()
    {
        EventManager.Instance?.Subscribe<GameStateChangedEvent>(this);
        EventManager.Instance?.Subscribe<BossSpawnEvent>(this);
    }

    private void OnDestroy()
    {
        EventManager.Instance?.Unsubscribe<GameStateChangedEvent>(this);
        EventManager.Instance?.Unsubscribe<BossSpawnEvent>(this);
    }

    private void OnDisable()
    {
        EventManager.Instance?.Unsubscribe<GameStateChangedEvent>(this);
        EventManager.Instance?.Unsubscribe<BossSpawnEvent>(this);
    }

    void Update()
    {
        this._backgroundMusicClips.audioSource.volume = this._backgroundMusicClips.volume;
        this._enemyCallTeamMoveSoundClips.audioSource.volume = this._enemyCallTeamMoveSoundClips.volume;
        
    }

    public void OnNotify(GameStateChangedEvent data)
    {
        switch (data.NewState)
        {
            case GameState.InGame:
                PlayAudio(GameConstants.AudioType.GameLooping);

                // Stop existing coroutines trước khi start mới
                if (_moveTeamSoundCoroutine != null)
                    StopCoroutine(_moveTeamSoundCoroutine);
                if (_attackTeamSoundCoroutine != null)
                    StopCoroutine(_attackTeamSoundCoroutine);

                _moveTeamSoundCoroutine = StartCoroutine(CallTeamSoundCoroutine(_enemyCallTeamMoveSoundClips, _audioConfig_EnemyCallTeamMoveSoundClips));
                _attackTeamSoundCoroutine = StartCoroutine(CallTeamSoundCoroutine(_enemyCallTeamAttackSoundClips, _audioConfig_EnemyCallTeamAttackSoundClips));
                break;

            case GameState.GameWin:
                StopAllCoroutines();
                StopAllAudio();
                PlayAudio(GameConstants.AudioType.GameWin);
                break;

            case GameState.GameOver:
                StopAllCoroutines();
                StopAllAudio();
                PlayAudio(GameConstants.AudioType.GameOver);
                break;
        }
    }

    private void PlayAudio(GameConstants.AudioType audioType)
    {
        switch (audioType)
        {
            case GameConstants.AudioType.GameLooping:
                _backgroundMusicClips.PlayByIndexWithLoop(0);
                break;
            case GameConstants.AudioType.GameWin:
                if (isPlayEndGameAudio)
                {
                    isPlayEndGameAudio = false;
                    StartCoroutine(PlayGameWinSequence());
                }
                break;
            case GameConstants.AudioType.GameOver:
                if (isPlayEndGameAudio)
                {
                    isPlayEndGameAudio = false;
                    StartCoroutine(PlayGameOverSequence());

                }

                break;
        }
    }

    private IEnumerator CallTeamSoundCoroutine(SoundSource soundSource, AudioConfig audioConfig)
    {
        // Delay ban đầu trước khi bắt đầu phát âm thanh
        yield return HelperCoroutine.GetWait(audioConfig._timerToActiveCallTeamSound);

        for (int i = 0; i < audioConfig.loopCount; i++)
        {
            // Kiểm tra xem AudioSource có đang phát không trước khi phát mới
            if (!soundSource.audioSource.isPlaying || soundSource.CanPlayOneShot())
            {
                soundSource.PlayOneShotRandomSound();
                //                Debug.Log($"Playing call team sound {i + 1}/{audioConfig.loopCount}");
            }

            // Chờ giữa các lần phát (chỉ chờ nếu không phải lần cuối)
            if (i < audioConfig.loopCount - 1)
            {
                yield return HelperCoroutine.GetWait(audioConfig._timeBetweenCallTeamSound);
            }
        }

        //        Debug.Log("Call team sound sequence completed");
    }

    private void StopAllAudio()
    {
        // Stop tất cả audio sources một cách an toàn
        _backgroundMusicClips.SafeStop();
        _enemyCallTeamMoveSoundClips.SafeStop();
        _enemyCallTeamAttackSoundClips.SafeStop();
    }



    private IEnumerator PlayGameWinSequence()
    {
        _backgroundMusicClips.PlayByIndex(3);
        yield return HelperCoroutine.GetWait(_backgroundMusicClips.soundClips[3].length);
        _backgroundMusicClips.PlayByIndexWithLoop(1);
    }

    private IEnumerator PlayGameOverSequence()
    {
        // _backgroundMusicClips.PlayByIndex(2);
        // yield return HelperCoroutine.GetWait(_backgroundMusicClips.soundClips[2].length);
        // _backgroundMusicClips.PlayByIndexWithLoop(5);
        yield return null;
    }

    public void OnNotify(BossSpawnEvent data)
    {
        // play 3 lần cảnh báo boss
        StartCoroutine(_backgroundMusicClips.IEPlayAudio(index: 4, count: 3));
    }
}

[System.Serializable]
public struct SoundSource
{
    public bool isCanHearByDistance;
    public AudioSource audioSource;
    public float volume;
    public AudioClip[] soundClips;

    // Thêm flag để track trạng thái
    private bool _isPlayingOneShot;

    public void PlayByIndex(int index)
    {
        if (!IsValidIndex(index)) return;

        // Stop audio trước khi play mới để tránh overlap
        SafeStop();

        audioSource.clip = soundClips[index];
        audioSource.volume = volume;
        audioSource.loop = false;
        audioSource.Play();
    }

    public void PlayByIndexByCount(int index, int count)
    {
        if (!IsValidIndex(index)) return;

        // Stop audio trước khi play mới
        SafeStop();

        audioSource.clip = soundClips[index];
        audioSource.volume = volume;
        audioSource.loop = false;

        for (int i = 0; i < count; i++)
        {
            audioSource.Play();
        }
    }
    public IEnumerator IEPlayAudio(int index, int count)
    {
        if (!IsValidIndex(index)) 
            yield break;

        // Stop audio trước khi play mới
        SafeStop();

        audioSource.clip = soundClips[index];
        audioSource.volume = volume;
        audioSource.loop = false;

        for (int i = 0; i < count; i++)
        {
            audioSource.Play();
            yield return new WaitForSeconds(soundClips[index].length);
        }
    }

    public void PlayByIndexWithLoop(int index)
    {
        if (!IsValidIndex(index)) return;

        // Stop audio trước khi play mới
        SafeStop();

        audioSource.clip = soundClips[index];
        audioSource.volume = volume;
        audioSource.loop = true;
        audioSource.Play();
    }

    public void PlayOneShotRandomSound()
    {
        if (soundClips.Length == 0)
        {
            Debug.LogWarning("No sound clips available to play.");
            return;
        }

        int randomIndex = Random.Range(0, soundClips.Length);
        PlayOneShotByIndex(randomIndex);
    }

    public void PlayOneShotByIndex(int index)
    {
        if (!IsValidIndex(index)) return;

        // Sử dụng PlayOneShot thay vì Play() để tránh conflict
        audioSource.volume = volume;
        audioSource.PlayOneShot(soundClips[index]);
    }

    public bool CanPlayOneShot()
    {
        // Kiểm tra xem có thể phát OneShot không
        return audioSource != null && !audioSource.isPlaying;
    }

    public void SafeStop()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }
    }

    private bool IsValidIndex(int index)
    {
        if (index < 0 || index >= soundClips.Length)
        {
            Debug.LogWarning($"Index {index} is out of bounds for sound clips array.");
            return false;
        }
        return audioSource != null && soundClips[index] != null;
    }

    public IEnumerator PlayBySequence(AudioClip[] audioClips)
    {
        if (audioClips == null || audioClips.Length == 0) yield break;

        foreach (var clip in audioClips)
        {
            if (clip == null) continue;

            audioSource.clip = clip;
            audioSource.Play();

            yield return new WaitForSeconds(clip.length);
        }
    }

}

[System.Serializable]
public struct AudioConfig
{
    [Tooltip("Thời gian delay để phát âm thanh gọi đồng đội di chuyển hay tấn công")]
    public float _timerToActiveCallTeamSound;

    [Tooltip("Số lần lặp lại âm thanh gọi đồng đội di chuyển hay tấn công")]
    public int loopCount;

    [Tooltip("Thời gian giữa các lần phát âm thanh gọi đồng đội di chuyển hay tấn công")]
    public float _timeBetweenCallTeamSound;
}