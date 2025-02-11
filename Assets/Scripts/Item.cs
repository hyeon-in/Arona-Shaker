using DG.Tweening;
using UnityEngine;

/// <summary>
/// �Ʒγ����Լ� �������� �������� �����ϴ� Ŭ�����Դϴ�.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Item : MonoBehaviour
{
    private const float LifeTime = 1.5f;    // ���� �ð�
    private const float RotateForce = 360f;  // ȸ���ϴ� ����
    private const float BoundaryOffset = 0.1f;  // ������� ȭ�� ���

    [SerializeField] private AudioClip _spawnSound; // ���� ����

    private Rigidbody2D _rb;
    private Camera _camera;
    private Transform _itemTransform;

    private Tween _lifeTimeTween; // ������Ʈ Ǯ ��ȯ�� ó���ϴ� Ʈ��

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _camera = Camera.main;
        _itemTransform = transform;
    }

    void OnEnable()
    {
        // ���� ���� ���
        SoundManager.Instance.PlaySFX(_spawnSound);
        // ������ Ÿ�� ���� ������Ʈ Ǯ�� ��ȯ
        _lifeTimeTween = DOVirtual.DelayedCall(LifeTime, ReturnToPool);
        // AngularVelocity ����
        SetAngularVelocity(Random.Range(-RotateForce, RotateForce));
        // ������ ���� ����
        _itemTransform.eulerAngles = new Vector3(0f, 0f, Random.Range(0f, 360f));
    }

    private void Update()
    {
        // ȭ�� ��踦 ����� ������Ʈ Ǯ�� ��ȯ
        if (IsOutOfBounds())
        {
            ReturnToPool();
        }
    }

    /// <summary>
    /// �������� ȭ�� ��踦 ������� üũ�ϴ� �޼����Դϴ�.
    /// </summary>
    private bool IsOutOfBounds()
    {
        Vector2 viewportPoint = _camera.WorldToViewportPoint(_itemTransform.position);
        return viewportPoint.x < -BoundaryOffset || viewportPoint.x > 1 + BoundaryOffset || viewportPoint.y < -BoundaryOffset || viewportPoint.y > 1 + BoundaryOffset;
    }

    /// <summary>
    /// ������Ʈ�� ������Ʈ Ǯ�� ��ȯ�ϴ� �޼����Դϴ�.
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
            // ������Ʈ Ǯ �Ŵ����� �������� ������ ������Ʈ ����
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// ������ٵ��� AngularVelocity�� �����մϴ�.
    /// </summary>
    /// <param name="angularVelocity">�����Ϸ��� AngularVelocity ��</param>
    private void SetAngularVelocity(float angularVelocity)
    {
        if (_rb != null)
        {
            _rb.angularVelocity = angularVelocity;
        }
    }
}