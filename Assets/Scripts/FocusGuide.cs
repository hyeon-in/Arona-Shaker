using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ������ ��Ŀ�� ȭ���� ó���ϴ� Ŭ�����Դϴ�.
/// </summary>
public class FocusGuide : MonoBehaviour
{
    [SerializeField] private Text _guideText;  // ������ ǥ���ϱ� ���� �ؽ�Ʈ ����

    private void Start()
    {
        if (_guideText == null)
        {
#if UNITY_EDITOR
            Debug.LogError("FocusGuide���� guideText�� �Ҵ���� �ʾҽ��ϴ�!");
#endif
        }
        else
        {
            SetGuideText();
        }
    }

    /// <summary>
    /// ���̵� �ؽ�Ʈ�� �����ϴ� �޼����Դϴ�.
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
        // Ŭ�� Ȥ�� ��ġ�� ���� ���� �� ������Ʈ ����
        if (Input.GetMouseButtonDown(0))
        {
            StartGame();
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// ������ �����ϴ� �޼����Դϴ�.
    /// </summary>
    private void StartGame()
    {
        HandController.IsActivated = true;
    }
}
