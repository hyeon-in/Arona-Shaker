using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 점수를 제어하는 클래스입니다.
/// </summary>
public class ScoreController : MonoBehaviour
{
    private const string ScoreKey = "Score";    // 스코어 저장 키값
    private const string IsOverflowedKey = "IsOverFlowed";  // 오버플로우 여부 저장 키값

    [SerializeField]
    private Text _scoreText;    // 점수를 표기하는 텍스트

    private int _currentScore;  // 현재 점수
    private bool _isOverflowed; // 오버플로우 여부를 체크하는 플래그 변수

    void Start()
    {
        if(_scoreText == null)
        {
#if UNITY_EDITOR
            Debug.LogError("ScoreController에서 scoreText가 할당되지 않았습니다!");
#endif
        }
        LoadData();
        UpdateScoreText();
    }

    /// <summary>
    /// 점수 증가를 처리하는 메서드입니다.
    /// </summary>
    /// <param name="scoreAmount">증가하는 점수의 량</param>
    public void AddScore(int scoreAmount = 1)
    {
        if (_isOverflowed) return;

        if(_currentScore >= int.MaxValue - scoreAmount)
        {
            // 현재 점수가 최대값을 넘어가면 오버플로우 상태로 변경
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
    /// 점수 텍스트를 업데이트하는 메서드입니다.
    /// </summary>
    private void UpdateScoreText()
    {
        _scoreText.text = !_isOverflowed ? _currentScore.ToString("N0") : "Overflow!";
    }

    /// <summary>
    /// 점수를 저장하는 메서드입니다.
    /// </summary>
    private void SaveScore()
    {
        PlayerPrefs.SetInt(ScoreKey, _currentScore);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 오버플로우 여부를 저장하는 메서드입니다.
    /// </summary>
    private void SaveIsOverflowed(bool isOverFlowed)
    {
        PlayerPrefs.SetInt(IsOverflowedKey, isOverFlowed ? 1 : 0);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 저장된 데이터를 불러오는 메서드입니다.
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
