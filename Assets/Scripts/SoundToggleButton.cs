using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ���� ��� ���θ� �����ϴ� ��ư Ŭ�����Դϴ�.
/// </summary>
public class SoundToggleButton : MonoBehaviour
{
    private const string IsSoundMutedKey = "IsSoundMuted"; // ���Ұ� ���� ���� Ű��

    [SerializeField] private Image _buttonImage;    // ���� ��� ��ư �̹���
    [SerializeField] private Sprite _soundOnSprite; // ���� Ȱ��ȭ ���� ��������Ʈ
    [SerializeField] private Sprite _soundOffSprite;    // ���� ��Ȱ��ȭ ���� ��������Ʈ

    private bool _isSoundMuted; // ���Ұ� ���¸� ��Ÿ���� �÷��� ����

    private void Start()
    {
        ValidateComponents();
        LoadSoundState();
        UpdateButtonSprite();
    }

    /// <summary>
    /// �ʼ� ������Ʈ���� �Ҵ�Ǿ����� Ȯ���ϴ� �޼����Դϴ�.
    /// �Ҵ���� ���� ��� ��� �޽����� ����մϴ�.
    /// </summary>
    private void ValidateComponents()
    {
        if (_buttonImage == null) LogWarning("buttonImage");
        if (_soundOnSprite == null) LogWarning("soundOnSprite");
        if (_soundOffSprite == null) LogWarning("soundOffSprite");
    }

    /// <summary>
    /// ��� �α׸� ����ϴ� �޼����Դϴ�.
    /// </summary>
    /// <param name="componenetName">�Ҵ���� ���� ������Ʈ �̸�</param>
    private void LogWarning(string componenetName)
    {
        Debug.LogWarning($"{nameof(SoundToggleButton)}���� {componenetName}�� �Ҵ���� �ʾҽ��ϴ�.");
    }

    /// <summary>
    /// ���ҰŸ� ó���ϴ� �޼����Դϴ�.
    /// </summary>
    public void ToggleSound()
    {
        UpdateMuteState();
        UpdateButtonSprite();
        UpdateSoundVolume();
        SaveSoundState();
    }

    /// <summary>
    /// ���Ұ� ���¸� �����ϴ� �޼����Դϴ�.
    /// </summary>
    private void UpdateMuteState()
    {
        _isSoundMuted = !_isSoundMuted;
    }

    /// <summary>
    /// ���Ұ� ���� ���ο� ���� ��ư �̹��� ��������Ʈ�� ������Ʈ�ϴ� �޼����Դϴ�.
    /// </summary>
    private void UpdateButtonSprite()
    {
        if (_buttonImage == null || _soundOnSprite == null || _soundOffSprite == null)
            return;

        _buttonImage.sprite = _isSoundMuted ? _soundOffSprite : _soundOnSprite;
    }
    
    /// <summary>
    /// ���� ������ ������Ʈ�ϴ� �޼����Դϴ�.
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
    /// ���� ��� ���¸� �����ϴ� �޼����Դϴ�.
    /// </summary>
    private void SaveSoundState()
    {
        PlayerPrefs.SetInt(IsSoundMutedKey, _isSoundMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// ���� ��� ���¸� �ҷ����� �޼����Դϴ�.
    /// </summary>
    private void LoadSoundState()
    {
        _isSoundMuted = PlayerPrefs.GetInt(IsSoundMutedKey, 0) == 1;
        UpdateSoundVolume();
    }
}