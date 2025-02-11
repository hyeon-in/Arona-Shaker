using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �ڷ�ƾ ��� �ð��� ����ȭ�ϱ� ���� �ڷ�ƾ ���� �ν��Ͻ��� ĳ���ϴ� ��ƿ��Ƽ Ŭ�����Դϴ�.
/// </summary>
public static class YieldInstructionCache
{
    /// <summary>
    /// float �� �񱳸� ���� IEqualityComparer�� ������ Ŭ�����Դϴ�.
    /// </summary>
    class FloatComparer : IEqualityComparer<float>
    {
        bool IEqualityComparer<float>.Equals(float x, float y)
        {
            return x == y;
        }
        int IEqualityComparer<float>.GetHashCode(float obj)
        {
            return obj.GetHashCode();
        }
    }

    // WaitForSeconds�� WaitForSecondsRealtime �ν��Ͻ��� ĳ���ϴ� Dictionary
    private static readonly Dictionary<float, WaitForSeconds> _timeInterval = new Dictionary<float, WaitForSeconds>(new FloatComparer());
    private static readonly Dictionary<float, WaitForSecondsRealtime> _realTimeInterval = new Dictionary<float, WaitForSecondsRealtime>(new FloatComparer());

    // WaitUntil �ν��Ͻ��� ĳ���ϴ� Dictionary
    private static readonly Dictionary<Func<bool>, WaitUntil> _waitUntilCache = new Dictionary<Func<bool>, WaitUntil>();

    // WaitWhile �ν��Ͻ��� ĳ���ϴ� Dictionary
    private static readonly Dictionary<Func<bool>, WaitWhile> _waitWhileCache = new Dictionary<Func<bool>, WaitWhile>();

    /// <summary>
    /// �־��� �ð���ŭ ����ϴ� WaitForSeconds �ν��Ͻ��� ��ȯ�ϴ� ���� �޼����Դϴ�.
    /// </summary>
    /// <param name="seconds">��� �ð�(��)</param>
    /// <returns>WaitForSeconds �ν��Ͻ�</returns>
    public static WaitForSeconds WaitForSeconds(float seconds)
    {
        // ������ ��� �ð��� ���� �ν��Ͻ��� �����ϸ� ã�Ƽ� ��ȯ�ϰ�, ������ ���� �����Ͽ� ��ȯ
        if (!_timeInterval.TryGetValue(seconds, out WaitForSeconds waitForSeconds))
        {
            _timeInterval.Add(seconds, waitForSeconds = new WaitForSeconds(seconds));
        }
        return waitForSeconds;
    }

    /// <summary>
    /// �־��� �ð���ŭ ����ϴ� WaitForSecondsRealtime �ν��Ͻ��� ��ȯ�ϴ� ���� �޼����Դϴ�.
    /// </summary>
    /// <param name="seconds">��� �ð�(��)</param>
    /// <returns>WaitForSecondsRealtime �ν��Ͻ�</returns>
    public static WaitForSecondsRealtime WaitForSecondsRealtime(float seconds)
    {
        // ������ ��� �ð��� ���� �ν��Ͻ��� �����ϸ� ã�Ƽ� ��ȯ�ϰ�, ������ ���� �����Ͽ� ��ȯ
        if (!_realTimeInterval.TryGetValue(seconds, out WaitForSecondsRealtime waitForSecondsRealtime))
        {
            _realTimeInterval.Add(seconds, waitForSecondsRealtime = new WaitForSecondsRealtime(seconds));
        }
        return waitForSecondsRealtime;
    }

    /// <summary>
    /// �־��� ������ true�� �� ������ ����ϴ� WaitUntil �ν��Ͻ��� ��ȯ�ϴ� ���� �޼����Դϴ�.
    /// </summary>
    /// <param name="predicate">true�� �� ������ ����� ����</param>
    /// <returns>WaitUntil �ν��Ͻ�</returns>
    public static WaitUntil WaitUntil(Func<bool> predicate)
    {
        if (!_waitUntilCache.TryGetValue(predicate, out var waitUntil))
        {
            waitUntil = new WaitUntil(predicate);
            _waitUntilCache[predicate] = waitUntil; // Cache the new instance
        }
        return waitUntil;
    }

    /// <summary>
    /// �־��� ������ false�� �� ������ ����ϴ� WaitWhile �ν��Ͻ��� ��ȯ�ϴ� ���� �޼����Դϴ�.
    /// </summary>
    /// <param name="predicate">false�� �� ������ ����� ����</param>
    /// <returns>WaitWhile �ν��Ͻ�</returns>
    public static WaitWhile WaitWhile(Func<bool> predicate)
    {
        if (!_waitWhileCache.TryGetValue(predicate, out var waitWhile))
        {
            waitWhile = new WaitWhile(predicate);
            _waitWhileCache[predicate] = waitWhile; // Cache the new instance
        }
        return waitWhile;
    }
}