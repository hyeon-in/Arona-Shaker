using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// 오브젝트 풀의 구성을 정의하는 데이터 클래스입니다.
/// 이 클래스는 오브젝트 풀 매니저에 접근하여 오브젝트 풀을 생성하는 기능을 가지고 있습니다.
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
    /// 오브젝트 풀을 초기화하는 메서드입니다.
    /// </summary>
    public void Initialize()
    {
        if(string.IsNullOrWhiteSpace(key))
        {
            Debug.LogError("키 값이 비어있는 풀의 초기화를 시도했습니다!");
            return;
        }
        if(prefab == null)
        {
            Debug.LogError("프리팹이 없는 풀의 초기화를 시도했습니다!");
            return;
        }
        if (maxSize < defaultCapacity)
        {
            Debug.LogError($"최대 크기({maxSize})가 기본 용량({defaultCapacity})보다 작습니다!");
            return;
        }

        if (ObjectPoolManager.Instance != null)
        {
            ObjectPoolManager.Instance.CreatePool(this);
        }
    }
}

/// <summary>
/// 오브젝트 풀을 처리하는 싱글톤 클래스입니다. 다른 싱글톤 클래스들과 달리 씬이 옮겨지면 파괴됩니다.
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
    /// 새로운 오브젝트 풀을 생성하는 메서드입니다.
    /// </summary>
    /// <param name="pool">생성하고자 하는 풀</param>
    /// <remarks>이미 같은 키 값을 가진 풀이 존재하면 실행되지 않고 즉시 반환합니다.</remarks>
    public void CreatePool(PoolConfig pool)
    {
        // 중복 생성 방지
        if (_cachedObjectPools.ContainsKey(pool.key))
        {
#if UNITY_EDITOR
            Debug.Log($"이미 존재하는 오브젝트 풀을 생성하려고 하여 취소됩니다. ({pool.key})");
#endif
            return;
        }

        if (pool.defaultCapacity > pool.maxSize)
        {
#if UNITY_EDITOR
            Debug.LogError($"기본 오브젝트 풀의 사이즈가 ({pool.defaultCapacity}) 최대 풀의 사이즈보다 큽니다. ({pool.maxSize})");
#endif
            return;
        }

        var parentObject = new GameObject($"Pool_{pool.key}");
        parentObject.transform.SetParent(_poolContainer);

        // 새로운 풀 객체 생성
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
    /// 지정된 키로부터 풀에서 오브젝트를 가져옵니다.
    /// </summary>
    /// <param name="key">가져오려는 오브젝트의 풀 키값</param>
    /// <returns>가져오려는 오브젝트</returns>
    public GameObject GetObjectFromPool(string key)
    {
        if(!_cachedObjectPools.ContainsKey(key))
        {
            Debug.LogError($"{key}라는 키 값을 가진 오브젝트 풀을 찾을 수 없습니다!");
            return null;
        }

        // 오브젝트 풀에서 객체를 가져옴
        return _cachedObjectPools[key].Get();
    }

    /// <summary>
    /// 풀에 객체를 반환합니다.
    /// </summary>
    /// <param name="obj">반환하려는 게임 오브젝트</param>
    public bool TryReturnToPool(GameObject obj)
    {
        if (!_poolKeyByGameObjects.TryGetValue(obj, out string key))
        {
            Debug.LogError($"{obj.name}이 돌아가려는 오브젝트 풀이 존재하지 않습니다!");
            return false;
        }

        if (!_cachedObjectPools.ContainsKey(key))
        {
            Debug.LogError($"오브젝트 풀의 키 '{key}'는 존재하지 않습니다!");
            return false;
        }

        // 해당 풀에 객체 반환
        _cachedObjectPools[key].Release(obj);
        return true;
    }

    /// <summary>
    /// 게임 오브젝트가 파괴되면 호출되는 메서드입니다.
    /// </summary>
    private void OnDestroy()
    {
        // 모든 오브젝트 비우기
        foreach (var pool in _cachedObjectPools.Values)
        {
            pool.Clear();
        }

        _cachedObjectPools.Clear();
        _poolKeyByGameObjects.Clear();

        // 파괴된 오브젝트의 Instance가 같다면 Instance값을 null로 설정
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
