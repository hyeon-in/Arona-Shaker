using UnityEngine;

/// <summary>
/// 게임의 초기 설정을 위한 클래스입니다.
/// </summary>
public class GameInitializer : MonoBehaviour
{
    void Awake()
    {
#if UNITY_ANDROID
        // 화면이 꺼지지 않게하고 프레임을 60으로 설정
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Application.targetFrameRate = 60;
#endif
        Destroy(gameObject);
    }
}