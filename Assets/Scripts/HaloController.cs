using UnityEngine;

/// <summary>
/// 아로나의 헤일로의 움직임을 제어하는 클래스입니다.
/// </summary>
public class HaloController : MonoBehaviour
{
    [SerializeField] private Transform _haloTargetTransform;    // 헤일로가 추적하려는 트랜스폼 컴포넌트
    [SerializeField] private float _trackingSpeed = 5f;     // 헤일로의 추적 속도

    private Transform _haloTransform;   // 헤일로의 트랜스폼 캐시

    private void Start()
    {
        if(_haloTargetTransform == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning("HaloController에서 haloTargetTransform이 할당되지 않았습니다.");
#endif
        }

        _haloTransform = transform;
    }

    private void Update()
    {
        MoveToTarget();
    }

    /// <summary>
    /// 헤일로가 타겟 트랜스폼을 추적하는 메서드입니다.
    /// </summary>
    private void MoveToTarget()
    {
        if (_haloTargetTransform == null) 
            return;

        _haloTransform.position = Vector2.Lerp(_haloTransform.position,
                                       _haloTargetTransform.position,
                                       Time.deltaTime * _trackingSpeed);
    }
}
