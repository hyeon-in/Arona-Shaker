using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ������ �����ϴ� Ŭ�����Դϴ�.
/// </summary>
public class ScoreController : MonoBehaviour
{
    private const string ScoreKey = "Score";    // ���ھ� ���� Ű��
    private const string IsOverflowedKey = "IsOverFlowed";  // �����÷ο� ���� ���� Ű��

    [SerializeField]
    private Text _scoreText;    // ������ ǥ���ϴ� �ؽ�Ʈ

    private int _currentScore;  // ���� ����
    private bool _isOverflowed; // �����÷ο� ���θ� üũ�ϴ� �÷��� ����

    void Start()
    {
        if(_scoreText == null)
        {
#if UNITY_EDITOR
            Debug.LogError("ScoreController���� scoreText�� �Ҵ���� �ʾҽ��ϴ�!");
#endif
        }
        LoadData();
        UpdateScoreText();
    }

    /// <summary>
    /// ���� ������ ó���ϴ� �޼����Դϴ�.
    /// </summary>
    /// <param name="scoreAmount">�����ϴ� ������ ��</param>
    public void AddScore(int scoreAmount = 1)
    {
        if (_isOverflowed) return;

        if(_currentScore >= int.MaxValue - scoreAmount)
        {
            // ���� ������ �ִ밪�� �Ѿ�� �����÷ο� ���·� ����
            _isOverflowed = true;
            SaveIsOverflowed(true);
            UpdateScoreText();
        }
        else
        {
            _currentScore += scoreAmount;
            UpdateScoreText();
            SaveScore();
        }
    }

    /// <summary>
    /// ���� �ؽ�Ʈ�� ������Ʈ�ϴ� �޼����Դϴ�.
    /// </summary>
    private void UpdateScoreText()
    {
        _scoreText.text = !_isOverflowed ? _currentScore.ToString("N0") : "Overflow!";
    }

    /// <summary>
    /// ������ �����ϴ� �޼����Դϴ�.
    /// </summary>
    private void SaveScore()
    {
        PlayerPrefs.SetInt(ScoreKey, _currentScore);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// �����÷ο� ���θ� �����ϴ� �޼����Դϴ�.
    /// </summary>
    private void SaveIsOverflowed(bool isOverFlowed)
    {
        PlayerPrefs.SetInt(IsOverflowedKey, isOverFlowed ? 1 : 0);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// ����� �����͸� �ҷ����� �޼����Դϴ�.
    /// </summary>
    private void LoadData()
    {
        if (PlayerPrefs.HasKey(ScoreKey))
        {
            _currentScore = PlayerPrefs.GetInt(ScoreKey, 0);
        }
        if (PlayerPrefs.HasKey(IsOverflowedKey))
        {
            _isOverflowed = PlayerPrefs.GetInt(IsOverflowedKey, 0) == 1;
        }
    }
}
