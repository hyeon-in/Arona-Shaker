using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 코루틴 대기 시간을 최적화하기 위해 코루틴 관련 인스턴스를 캐시하는 유틸리티 클래스입니다.
/// </summary>
public static class YieldInstructionCache
{
    /// <summary>
    /// float 값 비교를 위한 IEqualityComparer를 구현한 클래스입니다.
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

    // WaitForSeconds와 WaitForSecondsRealtime 인스턴스를 캐시하는 Dictionary
    private static readonly Dictionary<float, WaitForSeconds> _timeInterval = new Dictionary<float, WaitForSeconds>(new FloatComparer());
    private static readonly Dictionary<float, WaitForSecondsRealtime> _realTimeInterval = new Dictionary<float, WaitForSecondsRealtime>(new FloatComparer());

    // WaitUntil 인스턴스를 캐시하는 Dictionary
    private static readonly Dictionary<Func<bool>, WaitUntil> _waitUntilCache = new Dictionary<Func<bool>, WaitUntil>();

    // WaitWhile 인스턴스를 캐시하는 Dictionary
    private static readonly Dictionary<Func<bool>, WaitWhile> _waitWhileCache = new Dictionary<Func<bool>, WaitWhile>();

    /// <summary>
    /// 주어진 시간만큼 대기하는 WaitForSeconds 인스턴스를 반환하는 정적 메서드입니다.
    /// </summary>
    /// <param name="seconds">대기 시간(초)</param>
    /// <returns>WaitForSeconds 인스턴스</returns>
    public static WaitForSeconds WaitForSeconds(float seconds)
    {
        // 동일한 대기 시간을 가진 인스턴스가 존재하면 찾아서 반환하고, 없으면 새로 생성하여 반환
        if (!_timeInterval.TryGetValue(seconds, out WaitForSeconds waitForSeconds))
        {
            _timeInterval.Add(seconds, waitForSeconds = new WaitForSeconds(seconds));
        }
        return waitForSeconds;
    }

    /// <summary>
    /// 주어진 시간만큼 대기하는 WaitForSecondsRealtime 인스턴스를 반환하는 정적 메서드입니다.
    /// </summary>
    /// <param name="seconds">대기 시간(초)</param>
    /// <returns>WaitForSecondsRealtime 인스턴스</returns>
    public static WaitForSecondsRealtime WaitForSecondsRealtime(float seconds)
    {
        // 동일한 대기 시간을 가진 인스턴스가 존재하면 찾아서 반환하고, 없으면 새로 생성하여 반환
        if (!_realTimeInterval.TryGetValue(seconds, out WaitForSecondsRealtime waitForSecondsRealtime))
        {
            _realTimeInterval.Add(seconds, waitForSecondsRealtime = new WaitForSecondsRealtime(seconds));
        }
        return waitForSecondsRealtime;
    }

    /// <summary>
    /// 주어진 조건이 true가 될 때까지 대기하는 WaitUntil 인스턴스를 반환하는 정적 메서드입니다.
    /// </summary>
    /// <param name="predicate">true가 될 때까지 대기할 조건</param>
    /// <returns>WaitUntil 인스턴스</returns>
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
    /// 주어진 조건이 false가 될 때까지 대기하는 WaitWhile 인스턴스를 반환하는 정적 메서드입니다.
    /// </summary>
    /// <param name="predicate">false가 될 때까지 대기할 조건</param>
    /// <returns>WaitWhile 인스턴스</returns>
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