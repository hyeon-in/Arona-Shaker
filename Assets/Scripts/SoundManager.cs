using UnityEngine;

/// <summary>
/// ���带 �����ϴ� �̱��� Ŭ�����Դϴ�.
/// </summary>
public class SoundManager : SingletonManager<SoundManager>
{
    private const float DefaultVolume = 0.7f; // �⺻ ����

    private AudioSource _sfxAudioSource; // ȿ���� ����� �ҽ�

    protected override void Awake()
    {
        base.Awake();
        SetupSFXAudioSource();
    }

    /// <summary>
    /// ȿ���� ����� �ҽ� ������Ʈ�� �ʱ�ȭ�ϴ� �޼����Դϴ�.
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
    /// ȿ������ ����ϴ� �޼����Դϴ�.
    /// </summary>
    /// <param name="audioClip">����Ϸ��� ����� Ŭ��</param>
    public void PlaySFX(AudioClip audioClip)
    {
        if (audioClip == null || _sfxAudioSource == null)
            return;

        _sfxAudioSource.PlayOneShot(audioClip);
    }

    /// <summary>
    /// ���ҰŸ� ó���ϴ� �޼����Դϴ�.
    /// ���� ������ 0���� �����մϴ�.
    /// </summary>
    public void Mute()
    {
        SetVolume(0f);
    }

    /// <summary>
    /// ���� ������ �⺻������ �����ϴ� �޼����Դϴ�.
    /// </summary>
    public void ResetVolume()
    {
        SetVolume(DefaultVolume);
    }

    /// <summary>
    /// ���� ������ �����ϴ� �޼����Դϴ�.
    /// </summary>
    /// <param name="volume">�����Ϸ��� ���� ��(0 ~ 1)</param>
    public void SetVolume(float volume)
    {
        if (_sfxAudioSource == null)
            return;

        volume = Mathf.Clamp01(volume);
        _sfxAudioSource.volume = volume;
    }
}
