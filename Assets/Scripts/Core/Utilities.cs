using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Core
{
    public static class Utilities
    {
        public enum Axis { x, y, z }
        /// <summary>
        /// Formats an integer value of seconds into a mm:ss timer string.
        /// </summary>
        public static string ConvertSecondsToTimer(int sec)
        {
            string minutes = Mathf.Floor(sec / 60).ToString("00");
            string seconds = Mathf.Floor(sec % 60).ToString("00");

            return minutes + " :" + seconds;
        }

        /// <summary>
        /// Gets whether the current platform is a mobile device.
        /// </summary>
        public static bool IsOnMobile
        {
            get
            {
#if UNITY_IOS || UNITY_ANDROID
            return true;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// Gets whether the current platform is Android.
        /// </summary>
        public static bool IsOnAndroid
        {
            get
            {
#if UNITY_ANDROID
            return true;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// Gets whether the current platform is iOS.
        /// </summary>
        public static bool IsOnIos
        {
            get
            {
#if UNITY_IOS
            return true;
#else
                return false;
#endif
            }
        }
        
        /// <summary>
        /// Performs a Unity-specific null check.
        /// </summary>
        public static bool IsUnityNull(this UnityEngine.Object obj)
        {
            return obj == null;
        }
        
        /// <summary>
        /// Coroutine that waits for a specified time before executing an action.
        /// </summary>
        public static IEnumerator WaitAndExecute(float sec, UnityAction action)
        {
            yield return new WaitForSeconds(sec);

            action();
        }

        /// <summary>
        /// Attempts to run an asynchronous task within a try-catch block to handle unobserved exceptions.
        /// </summary>
        public static async Task TryRunAsync(Func<Task> asyncOperation)
        {
            try
            {
                await asyncOperation();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception occurred during async operation: {ex}");
            }
        }
        
        /// <summary>
        /// Strips the "(Clone)" suffix from a GameObject's name.
        /// </summary>
        public static string TrimUnityCloneName(string s)
        {
            string[] stringArr = s.Split('(');
            return stringArr[0].Trim();
        }

        /// <summary>
        /// Projects a world position into the local space of a specific UI Canvas.
        /// </summary>
        public static Vector3 WorldToUISpace(Canvas parentCanvas, Vector3 worldPos)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
            Vector2 movePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentCanvas.transform as RectTransform, screenPos, parentCanvas.worldCamera, out movePos);
            return parentCanvas.transform.TransformPoint(movePos);
        }

        /// <summary>
        /// Compares two Color32 values to see if any channel difference exceeds a threshold.
        /// </summary>
        public static bool CompareColors(Color32 one, Color32 two, float threshold)
        {
            if (Mathf.Abs(one.r - two.r) > threshold &&
                Mathf.Abs(one.g - two.g) > threshold &&
                Mathf.Abs(one.b - two.b) > threshold &&
                Mathf.Abs(one.a - two.a) > threshold)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Shuffles the elements of an ArrayList.
        /// </summary>
        public static void RandomizeArrayList(ArrayList list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                object temp = list[i];
                int randomIndex = UnityEngine.Random.Range(0, list.Count);
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }
        }

        /// <summary>
        /// Checks if a list contains any null elements.
        /// </summary>
        public static bool HasNullValues<T>(List<T> list)
        {
            foreach (T item in list)
            {
                if (item == null)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Swaps an element at an index with the one before it in the list.
        /// </summary>
        public static void MoveUp<T>(List<T> list, int indexToMove)
        {
            if (indexToMove == 0)
            {
                return;
            }

            T originalIndex = list[indexToMove];
            T newIndex = list[indexToMove - 1];
            list[indexToMove] = newIndex;
            list[indexToMove - 1] = originalIndex;
        }

        /// <summary>
        /// Swaps an element at an index with the one after it in the list.
        /// </summary>
        public static void MoveDown<T>(List<T> list, int indexToMove)
        {
            if (indexToMove == list.Count - 1)
            {
                return;
            }

            T temp = list[indexToMove];
            list[indexToMove] = list[indexToMove + 1];
            list[indexToMove + 1] = temp;
        }

        /// <summary>
        /// Returns a new list containing only non-null values from the original.
        /// </summary>
        public static List<T> RemoveNullValues<T>(List<T> list)
        {
            List<T> res = new List<T>();

            foreach (T item in list)
            {
                if (item != null)
                {
                    res.Add(item);
                }
            }

            return res;
        }

        /// <summary>
        /// Strips square brackets from the start and end of a string.
        /// </summary>
        public static string RemoveBrackets(string s)
        {
            if (s.Trim().StartsWith("[") && s.Trim().EndsWith("]"))
            {
                return s.Substring(1, s.Length - 2);
            }

            return s;
        }

        /// <summary>
        /// Shuffles the elements of a generic list.
        /// </summary>
        public static void RandomizeList<T>(List<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                T temp = list[i];
                int randomIndex = UnityEngine.Random.Range(0, list.Count);
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }
        }

        /// <summary>
        /// Shuffles the elements of an array.
        /// </summary>
        public static void RandomizeArray(object[] list)
        {
            for (int i = 0; i < list.Length; i++)
            {
                object temp = list[i];
                int randomIndex = UnityEngine.Random.Range(0, list.Length);
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }
        }

        /// <summary>
        /// Destroys all child objects of a transform.
        /// </summary>
        public static void DeleteChildren(Transform transform)
        {
            int ids = transform.childCount;
            for (int i = ids - 1; i >= 0; i--)
            {
                GameObject.Destroy(transform.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// Immediately destroys all child objects of a transform.
        /// </summary>
        public static void DeleteImmediateChildren(Transform transform)
        {
            int ids = transform.childCount;
            for (int i = ids - 1; i >= 0; i--)
            {
                GameObject.DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// Formats an integer score with spaces and a currency postfix.
        /// </summary>
        public static string CashifyScore(int score)
        {
            string res = "";
            string reversed = ReverseString(score.ToString());
            int index = 0;
        
            foreach (char c in reversed)
            {
                if (index % 3 == 0)
                {
                    res += " " + c;
                }
                else
                {
                    res += c;
                }

                index++;
            }

            return ReverseString(res) + "kr";
        }

        /// <summary>
        /// Inserts spaces into a string to separate characters in chunks of three.
        /// </summary>
        public static string SeparateChars(string s)
        {
            string value = "";
            string reversed = ReverseString(s);
            int index = 0;

            foreach (char c in reversed)
            {
                if (index % 3 == 0)
                {
                    value += " " + c;
                }
                else
                {
                    value += c;
                }

                index++;
            }

            return ReverseString(value);
        }

        /// <summary>
        /// Appends a currency postfix to a string.
        /// </summary>
        public static string AppendCurrancyPostfix(string s, string currancy = "")
        {
            string res = "";

            if (string.IsNullOrEmpty(currancy))
            {
                res += "kr";
            }
            else
            {
                res += currancy;
            }

            return res;
        }

        /// <summary>
        /// Reverses the characters in a string.
        /// </summary>
        public static string ReverseString(string s)
        {
            string res = "";

            for(int i = s.Length - 1; i >= 0; i--)
            {
                res += s[i];
            }
        
            return res;
        }

        /// <summary>
        /// Iteratively adds all elements from an append list to an original list.
        /// </summary>
        public static void AppendToList<T>(List<T> OriginalList, List<T> AppendList)
        {
            foreach (T item in AppendList)
            {
                OriginalList.Add(item);
            }
        }

        /// <summary>
        /// Copies all elements from a new list into an original list.
        /// </summary>
        public static void CopyList<T>(List<T> originalList, List<T> newList)
        {
            for (int i = 0; i < newList.Count; i++)
            {
                originalList.Add(newList[i]);
            }
        }

        /// <summary>
        /// Converts a Color32 to a hexadecimal string.
        /// </summary>
        public static string ColorToHex(Color32 color)
        {
            string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
            return hex;
        }
    
        /// <summary>
        /// Converts a hexadecimal string into a Unity Color.
        /// </summary>
        public static Color HexToColor(string hex)
        {
            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            return new Color32(r, g, b, 255);
        }

        /// <summary>
        /// Returns the opening Rich Text color tag for a specific color.
        /// </summary>
        public static string RichTextColorStartTag(Color color)
        {
            return "<color=#" + ColorToHex(color) + ">";
        }

        /// <summary>
        /// Returns the closing Rich Text color tag.
        /// </summary>
        public static string RichTextColorEndTag()
        {
            return "</color>";
        }

        /// <summary>
        /// Logs a message to the console with a specific Rich Text color.
        /// </summary>
        public static void Log(string s, Color c)
        {
            Debug.Log(RichTextColorStartTag(c) + s + RichTextColorEndTag());
        }
        /// <summary>
        /// Calculates the percentage of current value relative to max.
        /// </summary>
        public static float GetPercentage(float currentValue, float maxValue)
        {
            if (currentValue == 0 || maxValue == 0) { return 0; }

            float finalValue = (currentValue / maxValue) * 100;
            return finalValue;
        }

        /// <summary>
        /// Calculates the value of a specific percentage of the current value.
        /// </summary>
        public static float GetPercentageOf(float currentValue, float percentageValue)
        {
            if (currentValue == 0 || percentageValue == 0) { return 0; }

            return currentValue * percentageValue / 100;
        }
        /// <summary>
        /// Returns a new list with all null references removed.
        /// </summary>
        public static List<T> ClearListNulls<T>(List<T> list)
        {
            List<T> newList = new List<T>();

            foreach (var i in list)
            {
                if (i != null)
                {
                    newList.Add(i);
                }
            }

            return newList;
        }

        /// <summary>
        /// Linear interpolation between two vectors based on a specific distance offset.
        /// </summary>
        public static Vector3 LerpByDistance(Vector3 A, Vector3 B, float x)
        {
            Vector3 P = x * Vector3.Normalize(B - A) + A;
            return P;
        }

        /// <summary>
        /// Returns a new Vector3 with one specific axis value modified.
        /// </summary>
        public static Vector3 OverwriteSingleVectorComponent(Vector3 orig, Axis a, float newValue)
        {
            if (a == Axis.x)
            {
                orig = new Vector3(newValue, orig.y, orig.z);
            }
            else if (a == Axis.y)
            {
                orig = new Vector3(orig.x, newValue, orig.z);
            }
            else
            {
                orig = new Vector3(orig.x, orig.y, newValue);
            }
        
            return orig;
        }

        /// <summary>
        /// Returns a new Vector3 with one specific axis value replaced.
        /// </summary>
        public static Vector3 ReplaceSingleVectorComponent(Vector3 orig, Axis a, float newValue)
        {
            Vector3 res;

            if (a == Axis.x)
            {
                res = new Vector3(newValue, orig.y, orig.z);
            }
            else if (a == Axis.y)
            {
                res = new Vector3(orig.x, newValue, orig.z);
            }
            else
            {
                res = new Vector3(orig.x, orig.y, newValue);
            }

            return res;
        }

        /// <summary>
        /// Returns a new Vector3 with additions applied to each component.
        /// </summary>
        public static Vector3 AddToVectorComponent(Vector3 orig, float x, float y, float z)
        {
            return new Vector3(orig.x + x, orig.y + y, orig.z + z);
        }

        /// <summary>
        /// Returns a new Vector3 with subtractions applied to each component.
        /// </summary>
        public static Vector3 SubtractFromVectorComponent(Vector3 orig, float x, float y, float z)
        {
            return new Vector3(orig.x - x, orig.y - y, orig.z - z);
        }

        private static bool IsInLayerMask(GameObject obj, LayerMask layerMask)
        {
            int objLayerMask = (1 << obj.layer);

            if ((layerMask.value & objLayerMask) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Get the first instance of the children where its name matches the given name.
        /// </summary>
        public static Transform GetChildByName(string ChildName, Transform parent)
        {
            foreach (Transform t in parent)
            {
                if (t.name == ChildName)
                {
                    return t;
                }
            }

            return null;
        }

        /// <summary>
        /// Recursively searches for a child transform with a matching name.
        /// </summary>
        public static Transform RecursiveFindChild(string childName, Transform parent)
        {
            Transform findedObject = null;
            foreach (Transform child in parent)
            {
                if (child.name == childName)
                {
                    return child;
                }
                else
                {
                    findedObject = RecursiveFindChild(childName, child);
                }
            }
            return findedObject;
        }

        /// <summary>
        /// Recursively searches for a child GameObject with a matching name.
        /// </summary>
        public static GameObject RecursiveFindChildGameobject(string childName, Transform parent)
        {
            GameObject findedObject = null;
            foreach (Transform child in parent)
            {
                if (child.name == childName)
                {
                    return child.gameObject;
                }
                else
                {
                    findedObject = RecursiveFindChildGameobject(childName, child);
                }
            }
            return findedObject;
        }

        /// <summary>
        /// Restricts an angle to stay within a specified range, handling 360-degree wrap.
        /// </summary>
        public static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360F)
            {
                angle += 360F;
            }

            if (angle > 360F)
            {
                angle -= 360F;
            }

            return Mathf.Clamp(angle, min, max);
        }

        /// <summary>
        /// Checks if a value transition has exceeded a defined limit.
        /// </summary>
        public static bool HasExceededLimit(float preScore, float postScore, float limit)
        {
            if (preScore < limit && postScore >= limit)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if a value transition has dropped below a defined limit.
        /// </summary>
        public static bool HasDeceededLimit(float preScore, float postScore, float limit)
        {
            if (preScore >= limit && postScore < limit)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Pads a string to a specific column width.
        /// </summary>
        public static string PadStringRight(string s, int colWidth = 40)
        {
            return s.PadRight(colWidth);
        }

        /// <summary>
        /// Overwrites the materials array of a MeshRenderer.
        /// </summary>
        public static void SetMeshRendererMaterialsArray(MeshRenderer mr, Material[] newMaterials)
        {
            Material[] mats = new Material[newMaterials.Length];

            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = newMaterials[i];
            }

            mr.materials = mats;
        }

        /// <summary>
        /// Performs a ping-pong interpolation between a minimum and maximum value.
        /// </summary>
        public static float PingPong(float value, float min, float max)
        {
            return Mathf.PingPong(value, max - min) + min;
        }
    }
}
