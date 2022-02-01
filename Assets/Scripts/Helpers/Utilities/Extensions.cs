using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Helpers.Utilities;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Helpers.Utils
{
    public static class Extensions
    {
        public static string Format(this string target, params object[] args)
        {
            return string.Format(target, args);
        }

        public static string ToLowerUS(this string target)
        {
            return target.ToLower(new CultureInfo("en-US", false));
        }

        public static string ToUpperUS(this string target)
        {
            return target.ToUpper(new CultureInfo("en-US", false));
        }

        public static string ReplaceWith(string mainText, char placeHolder, string replacement)
        {
            List<char> taskTextString = new List<char>();
            int textLength = mainText.Length;

            for (int i = 0; i < textLength; i++)
            {
                if (mainText[i] == placeHolder) taskTextString.InsertRange(i, replacement);
                else taskTextString.Add(mainText[i]);
            }

            return new string(taskTextString.ToArray());
        }

        public static string ReplaceWith(string mainText, char[] placeHolder, string[] replacement)
        {
            int placeHolderCount = placeHolder.Length;
            int replacementCount = replacement.Length;
            if (replacementCount != placeHolderCount)
                Debug.LogError(
                    "Placeholder count isn't same with replacement count! {0}/{1}".Format(placeHolderCount,
                        replacementCount));
            for (int i = 0; i < placeHolderCount; i++) mainText = ReplaceWith(mainText, placeHolder[i], replacement[i]);
            return mainText;
        }

        public static string GetSubstring(this string target, int startIndex, int endIndex)
        {
            try
            {
                return target.Substring(startIndex, endIndex - startIndex + 1);
            }
            catch (Exception e)
            {
                Debug.LogError(endIndex + " " + startIndex);
                throw;
            }
        }

        public static int GetWordCount(this string target)
        {
            if (!target.Any()) return 0;
            int wordCount = 0;
            bool isFoundFirstLetter = false;

            foreach (char chr in target)
            {
                if (!isFoundFirstLetter)
                {
                    isFoundFirstLetter = !char.IsWhiteSpace(chr);
                    if (!isFoundFirstLetter) continue;
                    wordCount++;
                }
                else
                {
                    if (!char.IsWhiteSpace(chr)) continue;
                    isFoundFirstLetter = false;
                }
            }

            return wordCount;
        }

        public static string AddSpaces(this string target)
        {
            int targetLength = target.Length;
            int tempCount = 0;
            List<int> insertIndices = new List<int>();

            for (int i = 1; i < targetLength; i++)
            {
                if (!char.IsUpper(target[i])) continue;
                if (char.IsWhiteSpace(target[i - 1])) continue;
                insertIndices.Add(i + tempCount);
                tempCount++;
            }

            for (int i = 0; i < tempCount; i++) target = target.Insert(insertIndices[i], " ");
            return target;
        }

        public static string ReturnRepeatingString(int length, char character)
        {
            if (length == 0) return string.Empty;
            List<char> chars = new List<char>();
            for (int i = 0; i < length; i++) chars.Add(character);
            return new string(chars.ToArray());
        }

        public static string GetRandomText(int length)
        {
            if (length <= 0) return "";
            char[] chars = new char[length];
            for (int i = 0; i < length; i++)
                chars[i] = (char)(Random.value < 0.7f ? Random.Range(97, 123) : Random.Range(65, 91));
            return new string(chars);
        }

        public static float MatchPercentage(this string target, string other)
        {
            int targetLength = target.Length;
            int otherLength = other.Length;
            int maxIndex = Mathf.Max(targetLength, otherLength);
            int minIndex = Mathf.Min(targetLength, otherLength);
            int matchCount = 0;

            for (int i = 0; i < minIndex; i++)
            {
                if (target[i] != other[i]) continue;
                matchCount++;
            }

            return (float)matchCount / maxIndex;
        }

        public static object SafeGet(this Hashtable hashtable, object key)
        {
            return hashtable.ContainsKey(key) ? hashtable[key] : null;
        }

        public static void Shuffle<T>(this List<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var temp = list[i];
                var randomIndex = Random.Range(i, list.Count);
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }
        }

        public static T RemoveAndReturn<T>(this List<T> list, T obj)
        {
            return list.Remove(obj) ? obj : default;
        }

        public static T RemoveAndReturnAt<T>(this List<T> list, int index)
        {
            var o = list[index];
            list.RemoveAt(index);
            return o;
        }

        public static List<T> RemoveMultiple<T>(this List<T> list, List<T> elementsToRemove)
        {
            elementsToRemove.ForEach(x => list.Remove(x));
            return list;
        }

        public static List<T> RemoveMultiple<T>(this List<T> list, T[] elementsToRemove)
        {
            return RemoveMultiple(list, elementsToRemove.ToList());
        }

        public static GameObject[] GetObjectsWithName(string name)
        {
            return Object.FindObjectsOfType<GameObject>().Where(gameObject => gameObject.name == name).ToArray();
        }

        public static GameObject[] GetObjectsContainsName(string name)
        {
            return Object.FindObjectsOfType<GameObject>().Where(gameObject => gameObject.name.Contains(name)).ToArray();
        }

        public static Color WithAlpha(this Color c, float alpha)
        {
            return new Color(c.r, c.g, c.b, alpha);
        }

        public static float Round(this float value, int digits = 0)
        {
            float roundingFactor = Mathf.Pow(10, digits);
            return Mathf.Round(value * roundingFactor) / roundingFactor;
        }

        public static string ReturnWithUnit(this double target, int digits = 1)
        {
            string unit = string.Empty;

            for (int i = 4; i >= 1; i--)
            {
                double current = target / Mathf.Pow(1e+3f, i);
                if (current < 1.0) continue;
                target = Math.Round(current, digits);

                switch (i)
                {
                    case 1:
                        unit = "K";
                        break;
                    case 2:
                        unit = "M";
                        break;
                    case 3:
                        unit = "B";
                        break;
                    case 4:
                        unit = "T";
                        break;
                    default:
                        break;
                }

                break;
            }

            return target.ToString(CultureInfo.CurrentCulture) + unit;
        }

        public static string ReturnWithUnit(this float target, int digits = 1)
        {
            return ((double)target).ReturnWithUnit(digits);
        }

        public static void SetAlpha(this Image image, float a)
        {
            image.color = image.color.WithAlpha(a);
        }

        public static Color Rgb2Gray(this Image image)
        {
            Color currentColor = image.color;
            float grayScaleValue = currentColor.r * 0.2989f + currentColor.g * 0.5870f + currentColor.b * 0.1140f;
            return (Color.white * grayScaleValue).SetAlpha(1);
        }

        public static Color SetAlpha(this Color color, float a)
        {
            color.a = a;
            return color;
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary,
            TKey key, TValue defaultValue = default(TValue))
        {
            TValue value;
            return dictionary.TryGetValue(key, out value) ? value : defaultValue;
        }

        public static int IndexOf<T>(this T[] arr, T value)
        {
            return Array.IndexOf(arr, value);
        }

        #region Transform

        public static Transform FindDeepChild(this Transform parent, string searchName)
        {
            Queue<Transform> queue = new Queue<Transform>();
            queue.Enqueue(parent);
            while (queue.Count > 0)
            {
                var c = queue.Dequeue();
                if (c.name == searchName)
                    return c;
                foreach (Transform t in c)
                    queue.Enqueue(t);
            }

            return null;
        }

        public static Transform FindDeepChildContain(this Transform parent, string searchName, string[] avoid)
        {
            bool shouldAvoid = avoid.Length > 0;
            Queue<Transform> queue = new Queue<Transform>();
            queue.Enqueue(parent);

            while (queue.Count > 0)
            {
                var c = queue.Dequeue();
                if (c.name.Contains(searchName) && c != parent)
                {
                    if (shouldAvoid)
                    {
                        bool isCompatible = true;

                        for (int i = 0; i < avoid.Length; i++)
                        {
                            if (!c.name.Contains(avoid[i])) continue;
                            isCompatible = false;
                            break;
                        }

                        if (isCompatible) return c;
                    }
                    else
                    {
                        return c;
                    }
                }

                foreach (Transform t in c)
                    queue.Enqueue(t);
            }

            return null;
        }

        public static Transform FindDeepChildContain(this Transform parent, string searchName, string avoid)
        {
            bool shouldAvoid = avoid.Any();
            Queue<Transform> queue = new Queue<Transform>();
            queue.Enqueue(parent);

            while (queue.Count > 0)
            {
                var c = queue.Dequeue();
                if (c.name.Contains(searchName) && c != parent)
                {
                    if (shouldAvoid)
                    {
                        if (!c.name.Contains(avoid)) return c;
                    }
                    else
                    {
                        return c;
                    }
                }

                foreach (Transform t in c) queue.Enqueue(t);
            }

            return null;
        }

        public static Transform[] FindDeepChildrenContain(this Transform parent, string searchName, string avoid)
        {
            bool shouldAvoid = avoid.Any();
            List<Transform> found = new List<Transform>();
            List<Transform> children = parent.GetDeepChildren().ToList();
            int childrenCount = children.Count;

            for (int i = 0; i < childrenCount; i++)
            {
                Transform c = children[i];
                if (!c.name.Contains(searchName) || c == parent) continue;

                if (shouldAvoid)
                {
                    if (!c.name.Contains(avoid)) found.Add(c);
                }
                else
                {
                    found.Add(c);
                }
            }

            return found.ToArray();
        }

        public static Transform[] FindDeepChildrenContain(this Transform parent, string searchName, string[] avoid)
        {
            int avoidCount = avoid.Length;
            bool shouldAvoid = avoidCount > 0;
            List<Transform> found = new List<Transform>();
            List<Transform> children = parent.GetDeepChildren().ToList();
            int childrenCount = children.Count;

            for (int i = 0; i < childrenCount; i++)
            {
                Transform c = children[i];
                if (!c.name.Contains(searchName) || c == parent) continue;

                if (shouldAvoid)
                {
                    bool isCompatible = true;

                    for (int j = 0; j < avoid.Length; j++)
                    {
                        if (!c.name.Contains(avoid[j])) continue;
                        isCompatible = false;
                        break;
                    }

                    if (isCompatible) found.Add(c);
                }
                else
                {
                    found.Add(c);
                }
            }

            return found.ToArray();
        }

        public static void ResetLocal(this Transform t)
        {
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
        }

        public static void DestroyChildren(this Transform t)
        {
            t.Cast<Transform>().ToList().ForEach(c => Object.Destroy(c.gameObject));
        }

        public static void DestroyChildrenImmediate(this Transform t)
        {
            t.Cast<Transform>().ToList().ForEach(c => Object.DestroyImmediate(c.gameObject));
        }

        public static Transform ReturnMainParent(this Transform t)
        {
            Transform tempTrans = t;

            while (true)
            {
                if (tempTrans.parent != null)
                {
                    tempTrans = tempTrans.parent;
                }
                else
                {
                    return tempTrans;
                }
            }
        }

        public static Transform[] GetChildren(this Transform transform)
        {
            int childCount = transform.childCount;
            Transform[] children = new Transform[childCount];
            for (int i = 0; i < childCount; i++) children[i] = transform.GetChild(i);
            return children;
        }

        public static Transform[] GetDeepChildren(this Transform transform)
        {
            Queue<Transform> queue = new Queue<Transform>();
            queue.Enqueue(transform);
            List<Transform> children = new List<Transform>();

            while (queue.Count > 0)
            {
                Transform child = queue.Dequeue();
                children.Add(child);
                foreach (Transform t in child) queue.Enqueue(t);
            }

            return children.ToArray();
        }

        #endregion

        #region Mono

        public static Coroutine InvokeWithDelay(this MonoBehaviour mono, float delay, Action target,
            bool isTimeScaled = true)
        {
            return mono.StartCoroutine(CoroutineUtilities.DelayedExecutionCoroutine(delay, target, isTimeScaled));
        }

        public static Coroutine InvokeNextFrame(this MonoBehaviour mono, Action target, int frameCount = 1)
        {
            return mono.StartCoroutine(CoroutineUtilities.NextFrameCoroutine(target, frameCount));
        }

        public static void SafeStop(this MonoBehaviour mono, Coroutine coroutine)
        {
            if (coroutine != null)
                mono.StopCoroutine(coroutine);
        }

        public static void RemoveComponentIfExistsImmediate<T>(this GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            if (component != null) Object.DestroyImmediate(component);
        }

        public static void RemoveComponentsIfExistsImmediate<T>(this GameObject obj) where T : Component // TODO
        {
            var t = obj.GetComponents<T>();
            for (var i = 0; i < t.Length; i++) Object.DestroyImmediate(t[i]);
        }

        public static void RemoveComponentIfExists<T>(this GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            if (component != null) Object.Destroy(component);
        }

        public static void RemoveComponentsIfExists<T>(this GameObject obj) where T : Component // TODO
        {
            var t = obj.GetComponents<T>();
            for (var i = 0; i < t.Length; i++) Object.Destroy(t[i]);
        }

        #endregion

        #region Vector

        #region Vector3

        public static float Distance(this Vector3 v1, Vector3 v2)
        {
            return Vector3.Distance(v1, v2);
        }

        public static Vector3 InverseScale(this Vector3 v)
        {
            return new Vector3(1f / v.x, 1f / v.y, 1f / v.z);
        }

        public static Vector3 CopyWithX(this Vector3 vector3, float x)
        {
            vector3.x = x;
            return vector3;
        }

        public static Vector3 CopyWithY(this Vector3 vector3, float y)
        {
            vector3.y = y;
            return vector3;
        }

        public static Vector3 CopyWithZ(this Vector3 vector3, float z)
        {
            vector3.z = z;
            return vector3;
        }

        public static Vector3 ReflectByNormal(this Vector3 vector3, Vector3 normal)
        {
            return vector3 - normal *
                (2 * (Vector3.Dot(vector3, normal) / Mathf.Pow(normal.magnitude, 2)));
        }

        public static Vector3 ReflectByPlane(this Vector3 vector3, Plane plane)
        {
            return vector3.ReflectByNormal(plane.normal);
        }

        public static Vector3 RotatePositionAroundPoint(this Vector3 vector3, Vector3 point, Quaternion rotationAmount)
        {
            return rotationAmount * (vector3 - point) + point;
        }

        public static Vector3 RotateForward(this Vector3 vector3, Vector3 upDirection, Quaternion rotationAmount)
        {
            Quaternion lookRotation = Quaternion.LookRotation(vector3, upDirection);
            return lookRotation * rotationAmount * Vector3.forward;
        }

        public static Vector3[] LineUpByPoint(Transform pointTransform, int elementCount, float elementDiameter,
            int lateralSize, int verticalSize = -1, bool shouldCenter = true)
        {
            lateralSize = lateralSize <= 0 ? 3 : lateralSize;
            bool isVerticalSizeSet = verticalSize > 0;

            if (isVerticalSizeSet)
            {
                int maxSize = verticalSize * lateralSize;
                elementCount = elementCount > maxSize ? maxSize : elementCount;
            }

            Vector3[] distributedElements = new Vector3[elementCount];
            int viableVerticalSize = Mathf.Clamp(Mathf.CeilToInt((float)elementCount / lateralSize), 1,
                isVerticalSizeSet ? verticalSize : 99999);
            Vector3 targetPosition = pointTransform.position;
            Vector3 targetRight = pointTransform.right;
            Vector3 targetForward = pointTransform.forward;
            float verticalShiftLength = shouldCenter ? elementDiameter * (viableVerticalSize - 1) / 2 : 0;
            int sizeIndex = 0;
            if (isVerticalSizeSet)
                elementCount = lateralSize * verticalSize < elementCount ? lateralSize * verticalSize : elementCount;

            for (int i = 0; i < elementCount; i++)
            {
                int currentBatchSize = elementCount - sizeIndex >= lateralSize ? lateralSize : elementCount - sizeIndex;
                float lateralShiftLength = elementDiameter * (currentBatchSize - 1) / 2;
                Vector3 startPosition = targetPosition - lateralShiftLength * targetRight +
                                        verticalShiftLength * targetForward;

                for (int k = 0; k < currentBatchSize; k++)
                {
                    distributedElements[sizeIndex] =
                        startPosition + elementDiameter * (targetRight * k - targetForward * i);
                    sizeIndex++;
                }
            }

            return distributedElements;
        }

        public static Vector3 Get3D(this Vector2 vector2)
        {
            return new Vector3(vector2.x, vector2.y, 0);
        }

        #endregion

        #region Vector2

        public static Vector2 CopyWithX(this Vector2 vector2, float x)
        {
            vector2.x = x;
            return vector2;
        }

        public static Vector2 CopyWithY(this Vector2 vector2, float y)
        {
            vector2.y = y;
            return vector2;
        }

        public static Vector2 ReflectByNormal(this Vector2 vector2, Vector2 normal)
        {
            return vector2 - 2 * (Vector2.Dot(vector2, normal) / Vector2.Dot(normal, normal)) * normal;
        }

        public static Vector2 GetXzVector(this Vector3 vector3)
        {
            return new Vector2(vector3.x, vector3.z);
        }

        public static Vector2 GetXyVector(this Vector3 vector3)
        {
            return new Vector2(vector3.x, vector3.y);
        }

        public static Vector2 InverseScale(this Vector2 v)
        {
            return new Vector2(1f / v.x, 1f / v.y);
        }

        #endregion

        #endregion

        #region Quaternion

        public static Quaternion RotationTo(this Quaternion a, Quaternion b)
        {
            return (Quaternion.Inverse(a) * b).normalized;
        }

        #endregion

        #region Physics

        public static Vector3[] GetCurrentPath(this Rigidbody rigidbody, float duration = 1f, int resolution = 2,
            float gravity = 9.81f)
        {
            if (resolution < 2) resolution = 2;
            Vector3[] path = new Vector3[resolution];
            Vector3 initialPosition = rigidbody.transform.position;

            for (int i = 0; i < resolution; i++)
            {
                path[i] = initialPosition + (rigidbody.velocity - Vector3.up *
                    (gravity * (duration * i / (resolution + 1)) / 2) *
                    (duration * i / (resolution + 1)));
            }

            return path;
        }

        public static Vector3[] CalculatePath(Vector3 initialVelocity, float duration = 1f, int resolution = 2,
            float gravity = 9.81f)
        {
            if (resolution < 2) resolution = 2;
            Vector3[] path = new Vector3[resolution];

            for (int i = 0; i < resolution; i++)
            {
                path[i] = (initialVelocity - Vector3.up * (gravity * (duration * i / (resolution + 1))) / 2) *
                          (duration * i / (resolution + 1));
            }

            return path;
        }

        public static Vector3[] UniformPointsOnSphere(int numberOfPoints)
        {
            Vector3[] points = new Vector3[numberOfPoints];
            float i = Mathf.PI * (3 - Mathf.Sqrt(5));
            float offset = 2f / numberOfPoints;
            float halfOffset = 0.5f * offset;
            float y;
            float r;
            float phi;

            for (int j = 0; j < numberOfPoints; j++)
            {
                y = j * offset - 1 + halfOffset;
                r = Mathf.Sqrt(1 - y * y);
                phi = j * i;
                Vector3 point = new Vector3(Mathf.Cos(phi) * r, y, Mathf.Sin(phi) * r);
                if (!points.Contains(point)) points[j] = point;
            }

            return points;
        }

        #endregion

        #region Procedural Utilities

        /// <summary>
        /// Return an array of points distributed on given rectangular area.
        /// </summary>
        /// <param name="amount"></param>
        /// <summary>
        /// AMOUNT: How many elements must be created. (Not guaranteed)
        /// </summary>
        /// <param name="agentRadius"></param>
        /// <summary>
        /// AGENT_RADIUS: Minimum acceptable distance between elements.
        /// </summary>
        /// <param name="center"></param>
        /// <summary>
        /// CENTER: Center of the rectangle on world-space.
        /// </summary>
        /// <param name="bounds"></param>
        /// <summary>
        /// BOUNDS: Relative size of the rectangle.
        /// </summary>
        /// <param name="rotation"></param>
        /// <summary>
        /// ROTATION: Rotation of the XY plane.
        /// </summary>
        /// <param name="maximumTryCount"></param>
        /// <summary>
        /// MAX_TRY_COUNT: Maximum try count for each suitable point before giving up.
        /// (Higher the value, higher the accuracy and computational demand)
        /// </summary>
        /// <param name="generatedPoints"></param>
        /// <returns></returns>
        public static bool TryGeneratePoints(int amount, float agentRadius, Vector3 center, Vector2 bounds,
            Quaternion rotation, out Vector3[] generatedPoints, int maximumTryCount = 128 * 128)
        {
            Stopwatch watch = Stopwatch.StartNew();
            // Poisson Disk Sampling - https://www.cs.ubc.ca/~rbridson/docs/bridson-siggraph07-poissondisk.pdf
            float radius = 2 * agentRadius; // Minimum acceptable distance between generated points.
            List<Vector2> points = new List<Vector2>(); // Generated points list.
            List<Vector2> spawnPoints = new List<Vector2>(); // Temp list for spawn points.
            Vector2 center2D = new Vector2(center.x, center.y);
            Vector2 offset = center2D - bounds / 2;
            int maxTryCount = maximumTryCount * amount;
            int tryCount = 0;

            while (points.Count < amount)
            {
                if (tryCount > maxTryCount) break;
                Vector2 spawnCenter = Vector2.zero;

                if (points.Count != 0)
                {
                    int spawnIndex = Random.Range(0, spawnPoints.Count); // Generating a random index.
                    spawnCenter = spawnPoints[spawnIndex];
                }

                for (int i = 0; i < 128; i++)
                {
                    tryCount++;
                    float angle = Random.value * Mathf.PI * 2;
                    float maxDistanceX = Mathf.Max(Mathf.Abs(bounds.x - spawnCenter.x), Mathf.Abs(spawnCenter.x));
                    float maxDistanceY = Mathf.Max(Mathf.Abs(bounds.y - spawnCenter.y), Mathf.Abs(spawnCenter.y));
                    Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
                    float maxRadius = Vector2.Scale(new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)),
                        new Vector2(maxDistanceX, maxDistanceY)).magnitude;
                    Vector2 candidate =
                        points.Count ==
                        0 // A random point from radius to maximum length with respect to the given bounds.
                            ? center2D - offset
                            : spawnCenter + dir * Random.Range(radius, maxRadius);

                    if (IsPointValid(candidate, radius, points, bounds)
                       ) // Checking if the created point meets the given conditions.
                    {
                        points.Add(candidate);
                        spawnPoints.Add(candidate);
                        break;
                    }
                }
            }

            List<Vector3> rotatedPoints = new List<Vector3>();
            bool shouldRotate = rotation != Quaternion.identity;

            for (int i = 0; i < points.Count; i++)
            {
                points[i] += offset;

                if (!shouldRotate)
                {
                    rotatedPoints.Add(new Vector3(points[i].x, points[i].y, center.z));
                    continue;
                }

                rotatedPoints.Add(
                    new Vector3(points[i].x, points[i].y, center.z).RotatePositionAroundPoint(center, rotation));
            }

            generatedPoints = rotatedPoints.ToArray();
            watch.Stop();
            long elapsedMs = watch.ElapsedMilliseconds;
            Debug.Log(
                "Generated {0}/{1} points in {2} trials in {3} ms!".Format(points.Count, amount, tryCount, elapsedMs));
            if (amount == points.Count) return true;
            return false;
        }

        private static bool IsPointValid(Vector2 candidate, float radius, List<Vector2> points, Vector2 bounds)
        {
            if (candidate.x >= 0 && candidate.x <= bounds.x &&
                candidate.y >= 0 && candidate.y <= bounds.y)
            {
                for (int i = 0; i < points.Count; i++)
                {
                    if ((points[i] - candidate).magnitude <= radius) // Acceptable distance check.
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        #endregion

        public static bool IsInViewConeXZ(Vector3 position, Vector3 forward, float fov, float viewRange,
            Vector3 targetPos)
        {
            bool isEditor;
#if UNITY_EDITOR
            isEditor = true;
#else
            isEditor = false;
#endif
            UnityEngine.Camera mainCamera = UnityEngine.Camera.main;
            if (mainCamera == null) return false;
            int screenHeight = isEditor ? mainCamera.pixelHeight : Screen.height;
            int screenWidth = isEditor ? mainCamera.pixelWidth : Screen.width;
            float ratio = (float)screenWidth / screenHeight;
            fov *= ratio;
            Vector2 positionDifference = (targetPos - position).GetXzVector();
            bool isInViewRange = positionDifference.magnitude <= viewRange;
            if (!isInViewRange) return false;
            Vector2 positionForward = positionDifference.normalized;
            float angleBetween = Vector2.Angle(forward.GetXzVector(), positionForward);
            return angleBetween <= fov / 2;
        }

        public static void AddTag(string tag)
        {
#if UNITY_EDITOR
            Object[] asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (asset == null || asset.Length <= 0) return;
            SerializedObject so = new SerializedObject(asset[0]);
            SerializedProperty tags = so.FindProperty("tags");

            for (int i = 0; i < tags.arraySize; ++i)
            {
                if (tags.GetArrayElementAtIndex(i).stringValue != tag) continue;
                return;
            }

            tags.InsertArrayElementAtIndex(tags.arraySize);
            tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
            so.ApplyModifiedProperties();
            so.Update();
#endif
        }


        public static int ReturnSystemVersion()
        {
            string operatingSystem = SystemInfo.operatingSystem;
            int stringLength = operatingSystem.Length;
            int startIndex;
            List<char> versionChars = new List<char>();

#if UNITY_EDITOR
            startIndex = operatingSystem.IndexOf("(", StringComparison.InvariantCulture);

            for (int i = startIndex + 1; i < stringLength; i++)
            {
                if (operatingSystem[i] == '.' || operatingSystem[i] == ',') break;
                versionChars.Add(operatingSystem[i]);
            }
#elif UNITY_ANDROID
            operatingSystem = operatingSystem.Replace("Android OS ", "");
            startIndex = 0;

            for (int i = startIndex; i < stringLength; i++)
            {
                if (operatingSystem[i] == '.' || operatingSystem[i] == ' ') break;
                versionChars.Add(operatingSystem[i]);
            }
#elif UNITY_IOS
            operatingSystem = operatingSystem.Replace("iOS ", "");
            startIndex = 0;

            for (int i = startIndex; i < stringLength; i++)
            {
                if (operatingSystem[i] == '.' || operatingSystem[i] == ' ') break;
                versionChars.Add(operatingSystem[i]);
            }
#endif
            bool isParsedSuccessfully = int.TryParse(new string(versionChars.ToArray()), out int systemVersion);
            if (isParsedSuccessfully) Debug.Log("OS version: {0}".Format(systemVersion));
            return isParsedSuccessfully ? systemVersion : 0;
        }

        #region Animation

        public static void UpdateSkinnedMeshRendererBones(this SkinnedMeshRenderer skinnedMeshRenderer,
            Transform newRootBone)
        {
            string rootName = "";
            if (skinnedMeshRenderer.rootBone != null) rootName = skinnedMeshRenderer.rootBone.name;
            Transform newRoot = null;
            Transform[] bones = skinnedMeshRenderer.bones;
            Transform[] newBones = new Transform[bones.Length];
            Transform[] existingBones = newRootBone.GetComponentsInChildren<Transform>(true);

            for (int i = 0; i < bones.Length; i++)
            {
                if (bones[i] == null)
                {
                    Debug.LogError("Bone index of {0} doesn't exits!".Format(i));
                    return;
                }

                bool found = false;

                foreach (var newBone in existingBones)
                {
                    if (newBone.name == rootName) newRoot = newBone;
                    if (newBone.name != bones[i].name) continue;
                    newBones[i] = newBone;
                    found = true;
                }

                if (found) continue;
                Debug.LogError("Couldn't find bone {0} on target rig!".Format(bones[i].name));
                return;
            }

            if (newRoot == null)
            {
                Debug.LogError("Couldn't find root bone {0} on target rig!".Format(skinnedMeshRenderer.rootBone));
                return;
            }

            skinnedMeshRenderer.bones = newBones;
            skinnedMeshRenderer.rootBone = newRoot;
        }

        #endregion
    }
}