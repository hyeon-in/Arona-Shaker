using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// ������Ʈ Ǯ�� ������ �����ϴ� ������ Ŭ�����Դϴ�.
/// �� Ŭ������ ������Ʈ Ǯ �Ŵ����� �����Ͽ� ������Ʈ Ǯ�� �����ϴ� ����� ������ �ֽ��ϴ�.
/// </summary>
[System.Serializable]
public class PoolConfig
{
    private const int MinRange = 1;
    private const int MaxRange = 1000;

    public string key = "Key";
    public GameObject prefab;
    [Range(MinRange, MaxRange)]
    public int defaultCapacity = 5;
    [Range(MinRange, MaxRange)]
    public int maxSize = 50;

    /// <summary>
    /// ������Ʈ Ǯ�� �ʱ�ȭ�ϴ� �޼����Դϴ�.
    /// </summary>
    public void Initialize()
    {
        if(string.IsNullOrWhiteSpace(key))
        {
            Debug.LogError("Ű ���� ����ִ� Ǯ�� �ʱ�ȭ�� �õ��߽��ϴ�!");
            return;
        }
        if(prefab == null)
        {
            Debug.LogError("�������� ���� Ǯ�� �ʱ�ȭ�� �õ��߽��ϴ�!");
            return;
        }
        if (maxSize < defaultCapacity)
        {
            Debug.LogError($"�ִ� ũ��({maxSize})�� �⺻ �뷮({defaultCapacity})���� �۽��ϴ�!");
            return;
        }

        if (ObjectPoolManager.Instance != null)
        {
            ObjectPoolManager.Instance.CreatePool(this);
        }
    }
}

/// <summary>
/// ������Ʈ Ǯ�� ó���ϴ� �̱��� Ŭ�����Դϴ�. �ٸ� �̱��� Ŭ������� �޸� ���� �Ű����� �ı��˴ϴ�.
/// </summary>
[DisallowMultipleComponent]
public class ObjectPoolManager : MonoBehaviour
{
    private static ObjectPoolManager _instance;

    public static ObjectPoolManager Instance 
    {
        get
        {
            if(_instance == null)
            {
                _instance = FindFirstObjectByType<ObjectPoolManager>();
                if(_instance == null)
                {
                    var obj = new GameObject(typeof(ObjectPoolManager).Name);
                    _instance = obj.AddComponent<ObjectPoolManager>();
                }
            }

            return _instance;
        }
        private set => _instance = value;
    } 

    private Dictionary<string, ObjectPool<GameObject>> _cachedObjectPools = new Dictionary<string, ObjectPool<GameObject>>();
    private Dictionary<GameObject, string> _poolKeyByGameObjects = new Dictionary<GameObject, string>();
    private Transform _poolContainer;

    private void Awake()
    {
        if (_instance != this && _instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _poolContainer = transform;
    }

    /// <summary>
    /// ���ο� ������Ʈ Ǯ�� �����ϴ� �޼����Դϴ�.
    /// </summary>
    /// <param name="pool">�����ϰ��� �ϴ� Ǯ</param>
    /// <remarks>�̹� ���� Ű ���� ���� Ǯ�� �����ϸ� ������� �ʰ� ��� ��ȯ�մϴ�.</remarks>
    public void CreatePool(PoolConfig pool)
    {
        // �ߺ� ���� ����
        if (_cachedObjectPools.ContainsKey(pool.key))
        {
#if UNITY_EDITOR
            Debug.Log($"�̹� �����ϴ� ������Ʈ Ǯ�� �����Ϸ��� �Ͽ� ��ҵ˴ϴ�. ({pool.key})");
#endif
            return;
        }

        if (pool.defaultCapacity > pool.maxSize)
        {
#if UNITY_EDITOR
            Debug.LogError($"�⺻ ������Ʈ Ǯ�� ����� ({pool.defaultCapacity}) �ִ� Ǯ�� ������� Ů�ϴ�. ({pool.maxSize})");
#endif
            return;
        }

        var parentObject = new GameObject($"Pool_{pool.key}");
        parentObject.transform.SetParent(_poolContainer);

        // ���ο� Ǯ ��ü ����
        _cachedObjectPools[pool.key] = new ObjectPool<GameObject>(
            createFunc: () =>
            {
                var obj = Instantiate(pool.prefab, parentObject.transform);
                _poolKeyByGameObjects[obj] = pool.key;
                return obj;
            },
            actionOnGet: obj => obj.SetActive(true),
            actionOnRelease: obj =>
            {
                obj.transform.SetParent(parentObject.transform);
                obj.SetActive(false);
            },
            actionOnDestroy: obj =>
            {
                _poolKeyByGameObjects.Remove(obj);
                Destroy(obj);
            },
            maxSize: pool.maxSize,
            defaultCapacity: pool.defaultCapacity
            );
    }

    /// <summary>
    /// ������ Ű�κ��� Ǯ���� ������Ʈ�� �����ɴϴ�.
    /// </summary>
    /// <param name="key">���������� ������Ʈ�� Ǯ Ű��</param>
    /// <returns>���������� ������Ʈ</returns>
    public GameObject GetObjectFromPool(string key)
    {
        if(!_cachedObjectPools.ContainsKey(key))
        {
            Debug.LogError($"{key}��� Ű ���� ���� ������Ʈ Ǯ�� ã�� �� �����ϴ�!");
            return null;
        }

        // ������Ʈ Ǯ���� ��ü�� ������
        return _cachedObjectPools[key].Get();
    }

    /// <summary>
    /// Ǯ�� ��ü�� ��ȯ�մϴ�.
    /// </summary>
    /// <param name="obj">��ȯ�Ϸ��� ���� ������Ʈ</param>
    public bool TryReturnToPool(GameObject obj)
    {
        if (!_poolKeyByGameObjects.TryGetValue(obj, out string key))
        {
            Debug.LogError($"{obj.name}�� ���ư����� ������Ʈ Ǯ�� �������� �ʽ��ϴ�!");
            return false;
        }

        if (!_cachedObjectPools.ContainsKey(key))
        {
            Debug.LogError($"������Ʈ Ǯ�� Ű '{key}'�� �������� �ʽ��ϴ�!");
            return false;
        }

        // �ش� Ǯ�� ��ü ��ȯ
        _cachedObjectPools[key].Release(obj);
        return true;
    }

    /// <summary>
    /// ���� ������Ʈ�� �ı��Ǹ� ȣ��Ǵ� �޼����Դϴ�.
    /// </summary>
    private void OnDestroy()
    {
        // ��� ������Ʈ ����
        foreach (var pool in _cachedObjectPools.Values)
        {
            pool.Clear();
        }

        _cachedObjectPools.Clear();
        _poolKeyByGameObjects.Clear();

        // �ı��� ������Ʈ�� Instance�� ���ٸ� Instance���� null�� ����
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
