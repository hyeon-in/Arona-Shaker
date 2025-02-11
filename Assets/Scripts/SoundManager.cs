using UnityEngine;

/// <summary>
/// 사운드를 제어하는 싱글톤 클래스입니다.
/// </summary>
public class SoundManager : SingletonManager<SoundManager>
{
    private const float DefaultVolume = 0.7f; // 기본 볼륨

    private AudioSource _sfxAudioSource; // 효과음 오디오 소스

    protected override void Awake()
    {
        base.Awake();
        SetupSFXAudioSource();
    }

    /// <summary>
    /// 효과음 오디오 소스 오브젝트를 초기화하는 메서드입니다.
    /// </summary>
    private void SetupSFXAudioSource()
    {
        var sfxObject = new GameObject { name = "SFX" };
        sfxObject.transform.parent = transform;
        _sfxAudioSource = sfxObject.AddComponent<AudioSource>();
        _sfxAudioSource.volume = DefaultVolume;
        _sfxAudioSource.dopplerLevel = 0f;
        _sfxAudioSource.reverbZoneMix = 0f;
    }

    /// <summary>
    /// 효과음을 재생하는 메서드입니다.
    /// </summary>
    /// <param name="audioClip">재생하려는 오디오 클립</param>
    public void PlaySFX(AudioClip audioClip)
    {
        if (audioClip == null || _sfxAudioSource == null)
            return;

        _sfxAudioSource.PlayOneShot(audioClip);
    }

    /// <summary>
    /// 음소거를 처리하는 메서드입니다.
    /// 사운드 볼륨을 0으로 변경합니다.
    /// </summary>
    public void Mute()
    {
        SetVolume(0f);
    }

    /// <summary>
    /// 사운드 볼륨을 기본값으로 리셋하는 메서드입니다.
    /// </summary>
    public void ResetVolume()
    {
        SetVolume(DefaultVolume);
    }

    /// <summary>
    /// 사운드 볼륨을 설정하는 메서드입니다.
    /// </summary>
    /// <param name="volume">설정하려는 볼륨 값(0 ~ 1)</param>
    public void SetVolume(float volume)
    {
        if (_sfxAudioSource == null)
            return;

        volume = Mathf.Clamp01(volume);
        _sfxAudioSource.volume = volume;
    }
}
