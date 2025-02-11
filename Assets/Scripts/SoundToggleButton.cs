using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 사운드 재생 여부를 설정하는 버튼 클래스입니다.
/// </summary>
public class SoundToggleButton : MonoBehaviour
{
    private const string IsSoundMutedKey = "IsSoundMuted"; // 음소거 상태 저장 키값

    [SerializeField] private Image _buttonImage;    // 사운드 재생 버튼 이미지
    [SerializeField] private Sprite _soundOnSprite; // 사운드 활성화 상태 스프라이트
    [SerializeField] private Sprite _soundOffSprite;    // 사운드 비활성화 상태 스프라이트

    private bool _isSoundMuted; // 음소거 상태를 나타내는 플래그 변수

    private void Start()
    {
        ValidateComponents();
        LoadSoundState();
        UpdateButtonSprite();
    }

    /// <summary>
    /// 필수 컴포넌트들이 할당되었는지 확인하는 메서드입니다.
    /// 할당되지 않은 경우 경고 메시지를 출력합니다.
    /// </summary>
    private void ValidateComponents()
    {
        if (_buttonImage == null) LogWarning("buttonImage");
        if (_soundOnSprite == null) LogWarning("soundOnSprite");
        if (_soundOffSprite == null) LogWarning("soundOffSprite");
    }

    /// <summary>
    /// 경고 로그를 출력하는 메서드입니다.
    /// </summary>
    /// <param name="componenetName">할당되지 않은 컴포넌트 이름</param>
    private void LogWarning(string componenetName)
    {
        Debug.LogWarning($"{nameof(SoundToggleButton)}에서 {componenetName}가 할당되지 않았습니다.");
    }

    /// <summary>
    /// 음소거를 처리하는 메서드입니다.
    /// </summary>
    public void ToggleSound()
    {
        UpdateMuteState();
        UpdateButtonSprite();
        UpdateSoundVolume();
        SaveSoundState();
    }

    /// <summary>
    /// 음소거 상태를 변경하는 메서드입니다.
    /// </summary>
    private void UpdateMuteState()
    {
        _isSoundMuted = !_isSoundMuted;
    }

    /// <summary>
    /// 음소거 상태 여부에 따라 버튼 이미지 스프라이트를 업데이트하는 메서드입니다.
    /// </summary>
    private void UpdateButtonSprite()
    {
        if (_buttonImage == null || _soundOnSprite == null || _soundOffSprite == null)
            return;

        _buttonImage.sprite = _isSoundMuted ? _soundOffSprite : _soundOnSprite;
    }
    
    /// <summary>
    /// 사운드 볼륨을 업데이트하는 메서드입니다.
    /// </summary>
    private void UpdateSoundVolume()
    {
        if (SoundManager.Instance == null)
            return;

        if(_isSoundMuted)
        {
            SoundManager.Instance.Mute();
        }
        else
        {
            SoundManager.Instance.ResetVolume();
        }
    }

    /// <summary>
    /// 사운드 재생 상태를 저장하는 메서드입니다.
    /// </summary>
    private void SaveSoundState()
    {
        PlayerPrefs.SetInt(IsSoundMutedKey, _isSoundMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 사운드 재생 상태를 불러오는 메서드입니다.
    /// </summary>
    private void LoadSoundState()
    {
        _isSoundMuted = PlayerPrefs.GetInt(IsSoundMutedKey, 0) == 1;
        UpdateSoundVolume();
    }
}