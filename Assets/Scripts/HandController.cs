using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �� ������Ʈ�� �����ϴ� Ŭ�����Դϴ�.
/// </summary>
public class HandController : MonoBehaviour
{
    private const float DropTriggerRotation = 20f;  // ������ ����� Ʈ�����ϴ� ȸ����
    private const float ResetDropRotation = 15f;    // ������ ����� �����ϴ� ȸ����
    private const float HighDropRate = 0.15f;    // �������� ���� ������ Ȯ��
    private const float MediumDropRate = 0.4f;  // �������� �߰� ������ ������ Ȯ��
    private const int HighDropCount = 5;    // ���� ������ �������� ����
    private const int MediumDropCount = 3;  // �߰� ������ ������ �������� ����
    private const int LowDropCount = 1; // ���� ������ �������� ����
    private const float RandomDirectionRange = 20f; // ������ ���� ����
    private const float RandomOffsetRange = 0.2f; // ������ ������ ����
    private const float ForceMultiplier = 10f; // ���ư��� �� ���� ��

    private readonly static Vector2 HandDirectionPoint = new Vector2(1.5f, 0.5f);  // ���� ���ϴ� ������ ��Ÿ���� ���

    /// <summary>
    /// �� ������Ʈ Ȱ��ȭ ���θ� ��Ÿ���� ������Ƽ�Դϴ�.
    /// </summary>
    public static bool IsActivated { get; set; } = false;

    [Header("Components")]
    [SerializeField] private Rigidbody2D _aronaRigidbody; // �Ʒγ��� ������ٵ�
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

    private Camera _camera; // ī�޶�
    private Transform _cameraTransform; // ī�޶� Ʈ������
    private Transform _aronaTransform; // �Ʒγ��� Ʈ������
    private Transform _handTransform; // �� Ʈ������

    private Vector2 _prevHandPosition;  // ���� �� ��ǥ
    private Vector2 _handTargetPosition;    // ���� ���� ��ǥ
    private Vector3 _aronaOriginalScale;    // �Ʒγ��� ���� ũ��
    private Vector3 _aronaTargetScale;  // �Ʒγ��� Ÿ�� ũ��

    private bool _isDropped;    // �������� ��ӵƴ��� ���� ����
    private Dictionary<GameObject, Transform> _itemTransformCache = new Dictionary<GameObject, Transform>();    // �������� Ʈ������ ĳ��
    private Dictionary<GameObject, Rigidbody2D> _itemRigidbodyCache = new Dictionary<GameObject, Rigidbody2D>();    // �������� ������ٵ� ĳ��

    void Start()
    {
        ValidateComponents();
        CacheComponents();
        InitializePools();
        InitializeAronaScales();
        _handTargetPosition = GetMousePosition();
    }

    /// <summary>
    /// �ʼ� ������Ʈ���� �Ҵ�Ǿ����� Ȯ���ϴ� �޼����Դϴ�.
    /// �Ҵ���� ���� ��� ���� �޽����� ����մϴ�.
    /// </summary>
    private void ValidateComponents()
    {
        if (_aronaRigidbody == null) LogError("aronaRigidbody");
        if (_handSpriteTransform == null) LogError("handSpriteTransform");
        if (_spawnPoint == null) LogError("spawnPoint");
    }

    /// <summary>
    /// ���� �α׸� ����ϴ� �޼����Դϴ�.
    /// </summary>
    /// <param name="componenetName">�Ҵ���� ���� ������Ʈ �̸�</param>
    private void LogError(string componentName)
    {
#if UNITY_EDITOR
        Debug.LogError($"{nameof(HandController)}���� {componentName}�� �Ҵ���� �ʾҽ��ϴ�.");
#endif
    }

    /// <summary>
    /// ������Ʈ���� ĳ���ϴ� �޼����Դϴ�.
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
    /// ������Ʈ Ǯ�� �ʱ�ȭ�ϴ� �޼����Դϴ�.
    /// </summary>
    private void InitializePools()
    {
        _pyroxenePool.Initialize();
        _pinkEnvelopePool.Initialize();
    }

    /// <summary>
    /// �Ʒγ��� ���� ũ�� �ʱⰪ�� �����ϴ� �޼����Դϴ�.
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
    /// �� ��ǥ�� ������Ʈ�ϴ� �޼����Դϴ�.
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
    /// ���� ��ġ�� ���� ���� ������ ������Ʈ�ϴ� �޼����Դϴ�.
    /// </summary>
    private void UpdateHandRotation()
    {
        // ���� ��ǥ�� Viewport �������� ����
        Vector2 handViewportPoint = _camera.WorldToViewportPoint(_handSpriteTransform.position);

        // ���� ������ �̿��� ���� ��� �� Rotation ������Ʈ
        Vector2 direction = (HandDirectionPoint - handViewportPoint).normalized;
        float angle = Vector2.SignedAngle(Vector2.right, direction);
        _handSpriteTransform.rotation = Quaternion.Euler(0, 0, angle);
    }

    /// <summary>
    /// �Ʒγ��� ������ٵ� Rotation ���� ���� ������ ����� ó���ϴ� �޼����Դϴ�.
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
    /// ������ ����� ó���ϴ� �޼����Դϴ�.
    /// </summary>
    private void TriggerItemDrop()
    {
        for (int i = 0; i < GetRandomSpawnCount(); i++)
        {
            SpawnItem();
        }
    }

    /// <summary>
    /// �������� ��ӵǴ� �������� ������ �������� �޼����Դϴ�.
    /// </summary>
    /// <returns>��ӵǴ� �������� ��</returns>
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
    /// �������� �����մϴ�.
    /// </summary>
    private void SpawnItem()
    {
        // ������ ������Ʈ�� ������Ʈ Ǯ���� ������ ���� �� ó��
        var itemObject = GetRandomItemFromPool();
        itemObject.transform.position = (Vector2)_spawnPoint.position + Random.insideUnitCircle;
        GetOrAddItemRigidbody(itemObject, out Rigidbody2D rb);
        ApplyRandomForceToItem(rb);

        // ���� ���� ó��
        _score.AddScore();
    }

    /// <summary>
    /// �������� �������� ������Ʈ Ǯ���� �������� �޼����Դϴ�.
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
    /// �������� Ʈ������ ������Ʈ�� �����ɴϴ�. ���� ĳ�õ��� ���� �����̸� ������Ʈ�� �����ͼ� ĳ�� ������ �մϴ�.
    /// </summary>
    /// <param name="itemObject">������ٵ� ���������� ������ ������Ʈ</param>
    /// <param name="rb">�������ų� ���� �߰��� ������ٵ� ������Ʈ</param>
    private void GetOrAddItemTransform(GameObject itemObject, out Rigidbody2D rb)
    {
        if (!_itemRigidbodyCache.TryGetValue(itemObject, out rb))
        {
            rb = itemObject.GetComponent<Rigidbody2D>();

            // ������ ������Ʈ�� Rigidbody2D ������Ʈ�� ������ ���� ���
            if (rb == null)
            {
                Debug.LogError($"{itemObject.name}�� Rigidbody2D ������Ʈ�� �������� �ʽ��ϴ�!");
                return;
            }

            // ������ٵ� ĳ�ÿ� ����
            _itemRigidbodyCache[itemObject] = rb;
        }
    }

    /// <summary>
    /// �������� ������ٵ� ������Ʈ�� �����ɴϴ�. ���� ĳ�õ��� ���� �����̸� ������Ʈ�� �����ͼ� ĳ�� ������ �մϴ�.
    /// </summary>
    /// <param name="itemObject">������ٵ� ���������� ������ ������Ʈ</param>
    /// <param name="rb">�������ų� ���� �߰��� ������ٵ� ������Ʈ</param>
    private void GetOrAddItemRigidbody(GameObject itemObject, out Rigidbody2D rb)
    {
        if (!_itemRigidbodyCache.TryGetValue(itemObject, out rb))
        {
            rb = itemObject.GetComponent<Rigidbody2D>();

            // ������ ������Ʈ�� Rigidbody2D ������Ʈ�� ������ ���� ���
            if(rb == null)
            {
                Debug.LogError($"{itemObject.name}�� Rigidbody2D ������Ʈ�� �������� �ʽ��ϴ�!");
                return;
            }

            // ������ٵ� ĳ�ÿ� ����
            _itemRigidbodyCache[itemObject] = rb;
        }
    }

    /// <summary>
    /// �������� �������� ���ư��� ������ �����ϴ� �޼����Դϴ�.
    /// </summary>
    /// <param name="rb">���ư� �������� ������ٵ�</param>
    private void ApplyRandomForceToItem(Rigidbody2D rb)
    {
        if (rb == null)
            return;

        // �������� ���ư��� ���� ����
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
    /// �Ʒγ��� ũ�⸦ ������Ʈ�ϴ� �޼����Դϴ�.
    /// </summary>
    /// <param name="movementDelta">���� �� ��ǥ�� ���� �� ��ǥ ���̰�</param>
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
    /// �Ʒγ��� ȸ���� ������Ʈ�ϴ� �޼����Դϴ�.
    /// </summary>
    /// <param name="movementDelta">���� �� ��ǥ�� ���� �� ��ǥ ���̰�</param>
    private void UpdateAronaRotation(Vector2 movementDelta)
    {
        if (movementDelta.magnitude > 0.01f)
        {
            float currentAngle = _aronaRigidbody.rotation;

            // �������� ������ ���� ȸ���� ���
            float horizontalRotation = -movementDelta.x * _maxTiltAngle;
            float verticalRotation = movementDelta.y * _maxTiltAngle;
            float targetRotation = horizontalRotation + verticalRotation;

            // �ִ� ������ ����
            targetRotation = Mathf.Clamp(targetRotation, -_maxTiltAngle, _maxTiltAngle);

            float angleDifference = Mathf.DeltaAngle(currentAngle, targetRotation);
            _aronaRigidbody.AddTorque(angleDifference * _tiltForce);
        }
        else
        {
            // �������� ������ ������ �ʱ� ������ ���ư�
            float currentAngle = _aronaRigidbody.rotation;
            float angleDifference = Mathf.DeltaAngle(currentAngle, 0f);
            _aronaRigidbody.AddTorque(angleDifference * _returnForce);
        }
    }

    /// <summary>
    /// ���콺 ��ǥ�� ��ũ�� ��ǥ��� �������� �޼����Դϴ�.
    /// </summary>
    private Vector2 GetMousePosition() => Input.mousePosition;

    /// <summary>
    /// �Էµ� ��ǥ�� ȭ�� ���� ���� �����ϴ� �޼����Դϴ�.
    /// </summary>
    /// <param name="position">������ ��ũ�� ��ǥ</param>
    /// <returns>ȭ�� ���� ���� ���ѵ� ���� ��ǥ</returns>
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
