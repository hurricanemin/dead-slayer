using System;
using System.Collections;
using UnityEngine;

namespace Helpers.Utilities
{
    public static class CoroutineUtilities
    {
        public static IEnumerator DelayedExecutionCoroutine(float delay, Action target, bool isTimeScaled = true)
        {
            if (!isTimeScaled) yield return new WaitForSecondsRealtime(delay);
            else yield return new WaitForSeconds(delay);
            target?.Invoke();
        }

        public static IEnumerator NextFrameCoroutine(Action target, int frameCount = 1)
        {
            int tempFrameCount = 0;

            while (tempFrameCount < frameCount)
            {
                tempFrameCount++;
                yield return null;
            }

            target?.Invoke();
        }
    }
}