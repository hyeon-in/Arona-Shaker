using DG.Tweening;
using UnityEngine;

/// <summary>
/// 아로나에게서 떨어지는 아이템을 제어하는 클래스입니다.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Item : MonoBehaviour
{
    private const float LifeTime = 1.5f;    // 유지 시간
    private const float RotateForce = 360f;  // 회전하는 정도
    private const float BoundaryOffset = 0.1f;  // 사라지는 화면 경계

    [SerializeField] private AudioClip _spawnSound; // 스폰 사운드

    private Rigidbody2D _rb;
    private Camera _camera;
    private Transform _itemTransform;

    private Tween _lifeTimeTween; // 오브젝트 풀 반환을 처리하는 트윈

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _camera = Camera.main;
        _itemTransform = transform;
    }

    void OnEnable()
    {
        // 스폰 사운드 재생
        SoundManager.Instance.PlaySFX(_spawnSound);
        // 라이프 타임 이후 오브젝트 풀에 반환
        _lifeTimeTween = DOVirtual.DelayedCall(LifeTime, ReturnToPool);
        // AngularVelocity 설정
        SetAngularVelocity(Random.Range(-RotateForce, RotateForce));
        // 무작위 각도 설정
        _itemTransform.eulerAngles = new Vector3(0f, 0f, Random.Range(0f, 360f));
    }

    private void Update()
    {
        // 화면 경계를 벗어나면 오브젝트 풀에 반환
        if (IsOutOfBounds())
        {
            ReturnToPool();
        }
    }

    /// <summary>
    /// 아이템이 화면 경계를 벗어났는지 체크하는 메서드입니다.
    /// </summary>
    private bool IsOutOfBounds()
    {
        Vector2 viewportPoint = _camera.WorldToViewportPoint(_itemTransform.position);
        return viewportPoint.x < -BoundaryOffset || viewportPoint.x > 1 + BoundaryOffset || viewportPoint.y < -BoundaryOffset || viewportPoint.y > 1 + BoundaryOffset;
    }

    /// <summary>
    /// 오브젝트를 오브젝트 풀에 반환하는 메서드입니다.
    /// </summary>
    private void ReturnToPool()
    {
        _lifeTimeTween?.Kill();
        SetAngularVelocity(0f);

        if (ObjectPoolManager.Instance != null)
        {
            ObjectPoolManager.Instance.TryReturnToPool(gameObject);
        }
        else
        {
            // 오브젝트 풀 매니저가 존재하지 않으면 오브젝트 삭제
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 리지드바디의 AngularVelocity를 설정합니다.
    /// </summary>
    /// <param name="angularVelocity">설정하려는 AngularVelocity 값</param>
    private void SetAngularVelocity(float angularVelocity)
    {
        if (_rb != null)
        {
            _rb.angularVelocity = angularVelocity;
        }
    }
}