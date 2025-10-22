#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public static class TestUtils
{
    private static readonly Stopwatch Stopwatch = new Stopwatch();

    public static void TimerStart()
    {
        Stopwatch.Start();
    }

    public static void TimerStop()
    {
        UnityEngine.Debug.Log($"{Stopwatch.ElapsedMilliseconds} ms 경과");
        Stopwatch.Stop();
        Stopwatch.Reset();
    }
}
#endif
