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

        public static string ConvertSecondsToTimer(int sec)
        {
            string minutes = Mathf.Floor(sec / 60).ToString("00");
            string seconds = Mathf.Floor(sec % 60).ToString("00");

            return minutes + " :" + seconds;
        }

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
        
        public static bool IsUnityNull(this UnityEngine.Object obj)
        {
            return obj == null;
        }
        
        public static IEnumerator WaitAndExecute(float sec, UnityAction action)
        {
            yield return new WaitForSeconds(sec);

            action();
        }

        public static async Task TryRunAsync(Func<Task> asyncOperation)
        {
            try
            {
                await asyncOperation();
            }
            catch (Exception ex)
            {
                // Log the exception to avoid crashes due to unobserved exceptions
                Debug.LogError($"Exception occurred during async operation: {ex}");
            }
        }
        
        //Remove the clone part of a gameobject created by unity
        public static string TrimUnityCloneName(string s)
        {
            string[] stringArr = s.Split('(');
            return stringArr[0].Trim();
        }

        public static Vector3 WorldToUISpace(Canvas parentCanvas, Vector3 worldPos)
        {
            //Convert the world for screen point so that it can be used with ScreenPointToLocalPointInRectangle function
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
            Vector2 movePos;

            //Convert the screenpoint to ui rectangle local point
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentCanvas.transform as RectTransform, screenPos, parentCanvas.worldCamera, out movePos);
            //Convert the local point to world point
            return parentCanvas.transform.TransformPoint(movePos);
        }

        public static bool CompareColors(Color32 one, Color32 two, float threshold)
        {
            //if (Mathf.Approximately(one.r, two.r) &&
            //    Mathf.Approximately(one.g, two.g) &&
            //    Mathf.Approximately(one.b, two.b) &&
            //    Mathf.Approximately(one.a, two.a))
            //{
            //    return true;
            //}
            if (Mathf.Abs(one.r - two.r) > threshold &&
                Mathf.Abs(one.g - two.g) > threshold &&
                Mathf.Abs(one.b - two.b) > threshold &&
                Mathf.Abs(one.a - two.a) > threshold)
            {
                return true;
            }

            return false;
        }

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

        public static string RemoveBrackets(string s)
        {
            if (s.Trim().StartsWith("[") && s.Trim().EndsWith("]"))
            {
                return s.Substring(1, s.Length - 2);
            }

            return s;
        }

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

        public static void DeleteChildren(Transform transform)
        {
            int ids = transform.childCount;
            for (int i = ids - 1; i >= 0; i--)
            {
                GameObject.Destroy(transform.GetChild(i).gameObject);
            }
        }

        public static void DeleteImmediateChildren(Transform transform)
        {
            int ids = transform.childCount;
            for (int i = ids - 1; i >= 0; i--)
            {
                GameObject.DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }

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

        public static string ReverseString(string s)
        {
            string res = "";

            for(int i = s.Length - 1; i >= 0; i--)
            {
                res += s[i];
            }
        
            return res;
        }

        public static void AppendToList<T>(List<T> OriginalList, List<T> AppendList)
        {
            foreach (T item in AppendList)
            {
                OriginalList.Add(item);
            }
        }

        public static void CopyList<T>(List<T> originalList, List<T> newList)
        {
            for (int i = 0; i < newList.Count; i++)
            {
                originalList.Add(newList[i]);
            }
        }

        public static string ColorToHex(Color32 color)
        {
            string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
            return hex;
        }
    
        public static Color HexToColor(string hex)
        {
            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            return new Color32(r, g, b, 255);
        }

        public static string RichTextColorStartTag(Color color)
        {
            return "<color=#" + ColorToHex(color) + ">";
        }

        public static string RichTextColorEndTag()
        {
            return "</color>";
        }

        public static void Log(string s, Color c)
        {
            Debug.Log(RichTextColorStartTag(c) + s + RichTextColorEndTag());
        }

        public static float GetPercentage(float currentValue, float maxValue)
        {
            if (currentValue == 0 || maxValue == 0) { return 0; }

            float finalValue = (currentValue / maxValue) * 100;
            return finalValue;
        }

        public static float GetPercentageOf(float currentValue, float percentageValue)
        {
            if (currentValue == 0 || percentageValue == 0) { return 0; }

            return currentValue * percentageValue / 100;
        }
    
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

        public static Vector3 LerpByDistance(Vector3 A, Vector3 B, float x)
        {
            Vector3 P = x * Vector3.Normalize(B - A) + A;
            return P;
        }

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

        public static Vector3 AddToVectorComponent(Vector3 orig, float x, float y, float z)
        {
            return new Vector3(orig.x + x, orig.y + y, orig.z + z);
        }

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
        /// <param name="ChildName">Name of gameobject to get</param>
        /// <returns></returns>
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
        /// Clamp the angle between a given minimum and maximum rotation
        /// </summary>
        /// <param name="angle">Current Angle</param>
        /// <param name="min">Minimum Rotation</param>
        /// <param name="max">Maximum Rotation</param>
        /// <returns></returns>
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

        public static bool HasExceededLimit(float preScore, float postScore, float limit)
        {
            if (preScore < limit && postScore >= limit)
            {
                return true;
            }

            return false;
        }

        public static bool HasDeceededLimit(float preScore, float postScore, float limit)
        {
            if (preScore >= limit && postScore < limit)
            {
                return true;
            }

            return false;
        }

        public static string PadStringRight(string s, int colWidth = 40)
        {
            return s.PadRight(colWidth);
        }

        public static void SetMeshRendererMaterialsArray(MeshRenderer mr, Material[] newMaterials)
        {
            Material[] mats = new Material[newMaterials.Length];

            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = newMaterials[i];
            }

            mr.materials = mats;
        }

        public static float PingPong(float value, float min, float max)
        {
            return Mathf.PingPong(value, max - min) + min;
        }
    }
}
