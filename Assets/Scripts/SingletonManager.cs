using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 싱글톤 클래스들을 관리하는 추상 클래스입니다.
/// </summary>
[DisallowMultipleComponent]
public abstract class SingletonManager<T> : MonoBehaviour where T : MonoBehaviour
{
    // 싱글톤 클래스 인스턴스
    private static T _instance;
    private static bool DestroyedSingleton = false;

    /// <summary>
    /// 싱글톤 클래스 인스턴스의 프로퍼티입니다.
    /// </summary>
    public static T Instance
    {
        get
        {
            if (DestroyedSingleton)
            {
                return null;
            }
            else
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<T>();

                    if (_instance == null)
                    {
                        var gameObject = new GameObject(typeof(T).Name);
                        _instance = gameObject.AddComponent<T>();
                    }

                    DontDestroyOnLoad(_instance.gameObject);
                }
                return _instance;
            }
        }
        set => _instance = value;
    }

    protected virtual void Awake()
    {
        // 인스턴스가 NULL값이면 초기화
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this) 
        {
            Destroy(gameObject);
        }
    }

    protected virtual void OnDestroy()
    {
        if(_instance == this as T)
        {
            _instance = null;
            DestroyedSingleton = true;
        }
    }
}
