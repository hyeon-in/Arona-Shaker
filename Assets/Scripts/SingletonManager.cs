using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �̱��� Ŭ�������� �����ϴ� �߻� Ŭ�����Դϴ�.
/// </summary>
[DisallowMultipleComponent]
public abstract class SingletonManager<T> : MonoBehaviour where T : MonoBehaviour
{
    // �̱��� Ŭ���� �ν��Ͻ�
    private static T _instance;
    private static bool DestroyedSingleton = false;

    /// <summary>
    /// �̱��� Ŭ���� �ν��Ͻ��� ������Ƽ�Դϴ�.
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
        // �ν��Ͻ��� NULL���̸� �ʱ�ȭ
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
