using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 게임의 포커스 화면을 처리하는 클래스입니다.
/// </summary>
public class FocusGuide : MonoBehaviour
{
    [SerializeField] private Text _guideText;  // 설명을 표시하기 위한 텍스트 변수

    private void Start()
    {
        if (_guideText == null)
        {
#if UNITY_EDITOR
            Debug.LogError("FocusGuide에서 guideText가 할당되지 않았습니다!");
#endif
        }
        else
        {
            SetGuideText();
        }
    }

    /// <summary>
    /// 가이드 텍스트를 설정하는 메서드입니다.
    /// </summary>
    private void SetGuideText()
    {
        string newText;
#if UNITY_WEBGL
        bool isTouchDevice = Input.touchSupported;
        newText = isTouchDevice ? "[ Touch to focus ]" : "[ Click to focus ]";
#elif UNITY_ANDROID || UNITY_IOS
        newText = "[ Touch to start ] ";
#else
        newText = "[ Click to start ]";
#endif
        _guideText.text = newText;
    }

    private void Update()
    {
        // 클릭 혹은 터치시 게임 시작 및 오브젝트 제거
        if (Input.GetMouseButtonDown(0))
        {
            StartGame();
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 게임을 시작하는 메서드입니다.
    /// </summary>
    private void StartGame()
    {
        HandController.IsActivated = true;
    }
}
