using UnityEngine;

/// <summary>
/// ������ �ʱ� ������ ���� Ŭ�����Դϴ�.
/// </summary>
public class GameInitializer : MonoBehaviour
{
    void Awake()
    {
#if UNITY_ANDROID
        // ȭ���� ������ �ʰ��ϰ� �������� 60���� ����
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Application.targetFrameRate = 60;
#endif
        Destroy(gameObject);
    }
}