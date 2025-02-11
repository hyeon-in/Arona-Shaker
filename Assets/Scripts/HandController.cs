using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 손 오브젝트를 제어하는 클래스입니다.
/// </summary>
public class HandController : MonoBehaviour
{
    private const float DropTriggerRotation = 20f;  // 아이템 드롭을 트리거하는 회전값
    private const float ResetDropRotation = 15f;    // 아이템 드롭을 리셋하는 회전값
    private const float HighDropRate = 0.15f;    // 아이템이 많이 나오는 확률
    private const float MediumDropRate = 0.4f;  // 아이템이 중간 정도로 나오는 확률
    private const int HighDropCount = 5;    // 많이 나오는 아이템의 수량
    private const int MediumDropCount = 3;  // 중간 정도로 나오는 아이템의 수량
    private const int LowDropCount = 1; // 적게 나오는 아이템의 수량
    private const float RandomDirectionRange = 20f; // 무작위 방향 범위
    private const float RandomOffsetRange = 0.2f; // 무작위 오프셋 범위
    private const float ForceMultiplier = 10f; // 날아가는 힘 곱셈 값

    private readonly static Vector2 HandDirectionPoint = new Vector2(1.5f, 0.5f);  // 손이 향하는 지점을 나타내는 상수

    /// <summary>
    /// 손 오브젝트 활성화 여부를 나타내는 프로퍼티입니다.
    /// </summary>
    public static bool IsActivated { get; set; } = false;

    [Header("Components")]
    [SerializeField] private Rigidbody2D _aronaRigidbody; // 아로나의 리지드바디
    [SerializeField] private Transform _handSpriteTransform;
    [SerializeField] private Transform _spawnPoint;

    [Header("Rotation And Movement")]
    [SerializeField] private float _maxTiltAngle = 45f;
    [SerializeField] private float _tiltForce = 5f;
    [SerializeField] private float _returnForce = 3f;
    [SerializeField] private float _handTrackingSpeed = 20f;
    [SerializeField] private float _scaleTransitionSpeed = 10f;
    [SerializeField] private float _squashStretchAmount = 0.3f;

    [Header("Spawn and Drop")]
    [SerializeField] private float _pinkEnvelopeDropRate = 0.2f;
    [SerializeField] private PoolConfig _pyroxenePool;
    [SerializeField] private PoolConfig _pinkEnvelopePool;

    [Header("Other")]
    [SerializeField] private ScoreController _score;

    private Camera _camera; // 카메라
    private Transform _cameraTransform; // 카메라 트랜스폼
    private Transform _aronaTransform; // 아로나의 트랜스폼
    private Transform _handTransform; // 손 트랜스폼

    private Vector2 _prevHandPosition;  // 이전 손 좌표
    private Vector2 _handTargetPosition;    // 손이 향할 좌표
    private Vector3 _aronaOriginalScale;    // 아로나의 원본 크기
    private Vector3 _aronaTargetScale;  // 아로나의 타겟 크기

    private bool _isDropped;    // 아이템이 드롭됐는지 여부 설정
    private Dictionary<GameObject, Transform> _itemTransformCache = new Dictionary<GameObject, Transform>();    // 아이템의 트랜스폼 캐시
    private Dictionary<GameObject, Rigidbody2D> _itemRigidbodyCache = new Dictionary<GameObject, Rigidbody2D>();    // 아이템의 리지드바디를 캐시

    void Start()
    {
        ValidateComponents();
        CacheComponents();
        InitializePools();
        InitializeAronaScales();
        _handTargetPosition = GetMousePosition();
    }

    /// <summary>
    /// 필수 컴포넌트들이 할당되었는지 확인하는 메서드입니다.
    /// 할당되지 않은 경우 에러 메시지를 출력합니다.
    /// </summary>
    private void ValidateComponents()
    {
        if (_aronaRigidbody == null) LogError("aronaRigidbody");
        if (_handSpriteTransform == null) LogError("handSpriteTransform");
        if (_spawnPoint == null) LogError("spawnPoint");
    }

    /// <summary>
    /// 에러 로그를 출력하는 메서드입니다.
    /// </summary>
    /// <param name="componenetName">할당되지 않은 컴포넌트 이름</param>
    private void LogError(string componentName)
    {
#if UNITY_EDITOR
        Debug.LogError($"{nameof(HandController)}에서 {componentName}가 할당되지 않았습니다.");
#endif
    }

    /// <summary>
    /// 컴포넌트들을 캐시하는 메서드입니다.
    /// </summary>
    private void CacheComponents()
    {
        _handTransform = transform;
        _camera = Camera.main;
        _cameraTransform = _camera.transform;

        if (_aronaRigidbody != null)
        {
            _aronaTransform = _aronaRigidbody.transform;
        }
    }

    /// <summary>
    /// 오브젝트 풀을 초기화하는 메서드입니다.
    /// </summary>
    private void InitializePools()
    {
        _pyroxenePool.Initialize();
        _pinkEnvelopePool.Initialize();
    }

    /// <summary>
    /// 아로나의 원본 크기 초기값을 설정하는 메서드입니다.
    /// </summary>
    private void InitializeAronaScales()
    {
        if (_aronaTransform == null)
            return;

        _aronaOriginalScale = _aronaTargetScale = _aronaTransform.localScale;
    }

    private void Update()
    {
        if (!IsActivated) return;

        UpdateHandPosition();
        UpdateHandRotation();
        HandleItemDropBasedOnRotation();
    }

    /// <summary>
    /// 손 좌표를 업데이트하는 메서드입니다.
    /// </summary>
    private void UpdateHandPosition()
    {
        Vector2 mousePosition = GetMousePosition();
        if (mousePosition != _handTargetPosition)
        {
            _handTargetPosition = ClampToScreen(mousePosition);
            _handTransform.position = Vector2.MoveTowards(_handTransform.position, _handTargetPosition, _handTrackingSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// 손의 위치에 따라 방향 각도를 업데이트하는 메서드입니다.
    /// </summary>
    private void UpdateHandRotation()
    {
        // 손의 좌표를 Viewport 기준으로 설정
        Vector2 handViewportPoint = _camera.WorldToViewportPoint(_handSpriteTransform.position);

        // 손의 방향을 이용해 각도 계산 후 Rotation 업데이트
        Vector2 direction = (HandDirectionPoint - handViewportPoint).normalized;
        float angle = Vector2.SignedAngle(Vector2.right, direction);
        _handSpriteTransform.rotation = Quaternion.Euler(0, 0, angle);
    }

    /// <summary>
    /// 아로나의 리지드바디 Rotation 값에 따른 아이템 드롭을 처리하는 메서드입니다.
    /// </summary>
    private void HandleItemDropBasedOnRotation()
    {
        float currentRotation = Mathf.Abs(Mathf.DeltaAngle(0f, _aronaRigidbody.rotation));
        if (currentRotation >= DropTriggerRotation && !_isDropped)
        {
            _isDropped = true;
            TriggerItemDrop();
        }
        else if (currentRotation < ResetDropRotation)
        {
            _isDropped = false;
        }
    }

    /// <summary>
    /// 아이템 드롭을 처리하는 메서드입니다.
    /// </summary>
    private void TriggerItemDrop()
    {
        for (int i = 0; i < GetRandomSpawnCount(); i++)
        {
            SpawnItem();
        }
    }

    /// <summary>
    /// 무작위로 드롭되는 아이템의 수량을 가져오는 메서드입니다.
    /// </summary>
    /// <returns>드롭되는 아이템의 량</returns>
    private int GetRandomSpawnCount()
    {
        float randomValue = Random.value;
        if (randomValue < HighDropRate)
        {
            return HighDropCount;
        }
        else if (randomValue < MediumDropRate)
        {
            return MediumDropCount;
        }
        return LowDropCount;
    }

    /// <summary>
    /// 아이템을 스폰합니다.
    /// </summary>
    private void SpawnItem()
    {
        // 아이템 오브젝트를 오브젝트 풀에서 가져와 스폰 후 처리
        var itemObject = GetRandomItemFromPool();
        itemObject.transform.position = (Vector2)_spawnPoint.position + Random.insideUnitCircle;
        GetOrAddItemRigidbody(itemObject, out Rigidbody2D rb);
        ApplyRandomForceToItem(rb);

        // 점수 증가 처리
        _score.AddScore();
    }

    /// <summary>
    /// 무작위로 아이템을 오브젝트 풀에서 가져오는 메서드입니다.
    /// </summary>
    private GameObject GetRandomItemFromPool()
    {
        if(Random.value > _pinkEnvelopeDropRate)
        {
            return ObjectPoolManager.Instance.GetObjectFromPool(_pyroxenePool.key);
        }
        else
        {
            return ObjectPoolManager.Instance.GetObjectFromPool(_pinkEnvelopePool.key);
        }                                                 
    }

    /// <summary>
    /// 아이템의 트랜스폼 컴포넌트를 가져옵니다. 만약 캐시되지 않은 상태이면 컴포넌트를 가져와서 캐시 설정을 합니다.
    /// </summary>
    /// <param name="itemObject">리지드바디를 가져오려는 아이템 오브젝트</param>
    /// <param name="rb">가져오거나 새로 추가된 리지드바디 컴포넌트</param>
    private void GetOrAddItemTransform(GameObject itemObject, out Rigidbody2D rb)
    {
        if (!_itemRigidbodyCache.TryGetValue(itemObject, out rb))
        {
            rb = itemObject.GetComponent<Rigidbody2D>();

            // 아이템 오브젝트에 Rigidbody2D 컴포넌트가 없으면 에러 출력
            if (rb == null)
            {
                Debug.LogError($"{itemObject.name}에 Rigidbody2D 컴포넌트가 존재하지 않습니다!");
                return;
            }

            // 리지드바디를 캐시에 저장
            _itemRigidbodyCache[itemObject] = rb;
        }
    }

    /// <summary>
    /// 아이템의 리지드바디 컴포넌트를 가져옵니다. 만약 캐시되지 않은 상태이면 컴포넌트를 가져와서 캐시 설정을 합니다.
    /// </summary>
    /// <param name="itemObject">리지드바디를 가져오려는 아이템 오브젝트</param>
    /// <param name="rb">가져오거나 새로 추가된 리지드바디 컴포넌트</param>
    private void GetOrAddItemRigidbody(GameObject itemObject, out Rigidbody2D rb)
    {
        if (!_itemRigidbodyCache.TryGetValue(itemObject, out rb))
        {
            rb = itemObject.GetComponent<Rigidbody2D>();

            // 아이템 오브젝트에 Rigidbody2D 컴포넌트가 없으면 에러 출력
            if(rb == null)
            {
                Debug.LogError($"{itemObject.name}에 Rigidbody2D 컴포넌트가 존재하지 않습니다!");
                return;
            }

            // 리지드바디를 캐시에 저장
            _itemRigidbodyCache[itemObject] = rb;
        }
    }

    /// <summary>
    /// 무작위로 아이템의 날아가는 정도를 설정하는 메서드입니다.
    /// </summary>
    /// <param name="rb">날아갈 아이템의 리지드바디</param>
    private void ApplyRandomForceToItem(Rigidbody2D rb)
    {
        if (rb == null)
            return;

        // 무작위로 날아가는 방향 설정
        float angle = Mathf.DeltaAngle(0f, _aronaRigidbody.rotation) * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(angle < 0 ? -1 : 1, Mathf.Sin(Mathf.Abs(angle + Random.Range(0f, RandomDirectionRange))));
        Vector2 randomOffset = Random.insideUnitCircle * RandomOffsetRange;
        Vector2 force = (direction + randomOffset) * ForceMultiplier;

        rb.AddForce(force, ForceMode2D.Impulse);
    }

    private void FixedUpdate()
    {
        if (!IsActivated) return;

        Vector2 currentHandPosition = _handTransform.position;
        Vector2 movementDelta = currentHandPosition - _prevHandPosition;

        UpdateAronaScale(movementDelta);
        UpdateAronaRotation(movementDelta);

        _prevHandPosition = currentHandPosition;
    }

    /// <summary>
    /// 아로나의 크기를 업데이트하는 메서드입니다.
    /// </summary>
    /// <param name="movementDelta">이전 손 좌표와 현재 손 좌표 차이값</param>
    private void UpdateAronaScale(Vector2 movementDelta)
    {
        if (Mathf.Abs(movementDelta.y) > 0.01f)
        {
            if (movementDelta.y > 0)
            {
                _aronaTargetScale = new Vector3(
                    _aronaOriginalScale.x * (1 - (movementDelta.y * _squashStretchAmount)),
                    _aronaOriginalScale.y * (1 + (movementDelta.y * _squashStretchAmount)),
                    _aronaOriginalScale.z
                );
            }
            else
            {
                _aronaTargetScale = new Vector3(
                    _aronaOriginalScale.x * (1 + (-movementDelta.y * _squashStretchAmount)),
                    _aronaOriginalScale.y * (1 - (-movementDelta.y * _squashStretchAmount)),
                    _aronaOriginalScale.z
                );
            }
        }
        else
        {
            _aronaTargetScale = _aronaOriginalScale;
        }

        _aronaTransform.localScale = Vector3.Lerp(
            _aronaTransform.localScale,
            _aronaTargetScale,
            Time.fixedDeltaTime * _scaleTransitionSpeed
        );
    }

    /// <summary>
    /// 아로나의 회전을 업데이트하는 메서드입니다.
    /// </summary>
    /// <param name="movementDelta">이전 손 좌표와 현재 손 좌표 차이값</param>
    private void UpdateAronaRotation(Vector2 movementDelta)
    {
        if (movementDelta.magnitude > 0.01f)
        {
            float currentAngle = _aronaRigidbody.rotation;

            // 움직임의 정도에 따라 회전값 계산
            float horizontalRotation = -movementDelta.x * _maxTiltAngle;
            float verticalRotation = movementDelta.y * _maxTiltAngle;
            float targetRotation = horizontalRotation + verticalRotation;

            // 최대 각도값 설정
            targetRotation = Mathf.Clamp(targetRotation, -_maxTiltAngle, _maxTiltAngle);

            float angleDifference = Mathf.DeltaAngle(currentAngle, targetRotation);
            _aronaRigidbody.AddTorque(angleDifference * _tiltForce);
        }
        else
        {
            // 움직임이 없으면 서서히 초기 각도로 돌아감
            float currentAngle = _aronaRigidbody.rotation;
            float angleDifference = Mathf.DeltaAngle(currentAngle, 0f);
            _aronaRigidbody.AddTorque(angleDifference * _returnForce);
        }
    }

    /// <summary>
    /// 마우스 좌표를 스크린 좌표계로 가져오는 메서드입니다.
    /// </summary>
    private Vector2 GetMousePosition() => Input.mousePosition;

    /// <summary>
    /// 입력된 좌표를 화면 범위 내로 제한하는 메서드입니다.
    /// </summary>
    /// <param name="position">제한할 스크린 좌표</param>
    /// <returns>화면 범위 내로 제한된 월드 좌표</returns>
    private Vector2 ClampToScreen(Vector2 position)
    {
        Vector2 viewportPoint = _camera.ScreenToViewportPoint(position);
        Vector2 clampedViewPortPoint = new Vector2(
                Mathf.Clamp(viewportPoint.x, 0f, 1f),
                Mathf.Clamp(viewportPoint.y, 0f, 1f)
            );

        return _camera.ViewportToWorldPoint(new Vector3(clampedViewPortPoint.x, clampedViewPortPoint.y, _cameraTransform.position.z));
    }
}
