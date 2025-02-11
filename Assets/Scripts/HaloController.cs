using UnityEngine;

/// <summary>
/// �Ʒγ��� ���Ϸ��� �������� �����ϴ� Ŭ�����Դϴ�.
/// </summary>
public class HaloController : MonoBehaviour
{
    [SerializeField] private Transform _haloTargetTransform;    // ���Ϸΰ� �����Ϸ��� Ʈ������ ������Ʈ
    [SerializeField] private float _trackingSpeed = 5f;     // ���Ϸ��� ���� �ӵ�

    private Transform _haloTransform;   // ���Ϸ��� Ʈ������ ĳ��

    private void Start()
    {
        if(_haloTargetTransform == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning("HaloController���� haloTargetTransform�� �Ҵ���� �ʾҽ��ϴ�.");
#endif
        }

        _haloTransform = transform;
    }

    private void Update()
    {
        MoveToTarget();
    }

    /// <summary>
    /// ���Ϸΰ� Ÿ�� Ʈ�������� �����ϴ� �޼����Դϴ�.
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
