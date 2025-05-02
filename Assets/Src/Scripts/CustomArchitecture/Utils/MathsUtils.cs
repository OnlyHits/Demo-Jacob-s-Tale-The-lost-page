using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace CustomArchitecture
{
    public static class MathsUtils
    {
        // put it in a TransformUtils class
        // Make it an extension method ?
        public static Transform GetNearest(Vector2 fromPosition, List<Transform> list, float minDistance = 0, float maxDistance = 100, List<Transform> exemptions = null)
        {
            Transform nearest = null;
            float minDist = Mathf.Infinity;

            foreach (Transform current in list)
            {
                if (current == null) { continue; }

                float dist = Vector2.Distance(fromPosition, current.position);

                if (exemptions != null)
                {
                    if (exemptions.Contains(current))
                    {
                        continue;
                    }
                }

                if (minDistance < dist && dist < maxDistance)
                {
                    if (dist < minDist)
                    {
                        minDist = dist;
                        nearest = current;
                    }
                }
            }
            return nearest;
        }

        // put it in a TransformUtils class or not idk
        public static Vector3 GetMedianPos(List<Transform> transforms)
        {
            Vector3 medianPos = Vector3.zero;
            int count = transforms.Count;

            if (transforms == null || transforms.Count == 0)
            {
                return medianPos;
            }

            foreach (Transform trf in transforms)
            {
                if (trf == null || trf?.transform == null)
                {
                    count = (int)Mathf.Clamp(count - 1, 1, Mathf.Infinity);
                    continue;
                }
                medianPos += trf.transform.position;
            }

            medianPos /= transforms.Count;

            return medianPos;
        }

        //Get random elements on list with Fisher-Yates Shuffle
        // More optimize with large list
        // Todo : make an extension method for List
        //public static List<T> GetRandomInList<T>(List<T> originalList, int elementNumber)
        //{
        //    if (originalList == null || originalList.Count == 0)
        //        return new List<T>();

        //    elementNumber = Mathf.Clamp(elementNumber, 0, originalList.Count);

        //    List<T> shuffledList = new List<T>(originalList);

        //    for (int i = 0; i < elementNumber; i++)
        //    {
        //        int randomIndex = Random.Range(i, shuffledList.Count);
        //        (shuffledList[i], shuffledList[randomIndex]) = (shuffledList[randomIndex], shuffledList[i]);
        //    }

        //    return shuffledList.GetRange(0, elementNumber);
        //}


        //public static List<T> GetRandomInList<T>(List<T> originalList, int elementNumber)
        //{
        //    if (originalList == null || originalList.Count == 0)
        //        return null;

        //    elementNumber = Mathf.Clamp(elementNumber, 0, originalList.Count);

        //    List<T> newList = new List<T>();
        //    List<T> tempList = new List<T>(originalList);

        //    for (int i = 0; i < elementNumber; i++)
        //    {
        //        int randomIndex = Random.Range(0, tempList.Count);

        //        newList.Add(tempList[randomIndex]);
        //        tempList.RemoveAt(randomIndex);
        //    }

        //    return newList;
        //}

        //[Obsolete("Use GetRandomInList<T>() instead")]
        //public static List<Transform> GetRandomInList(List<Transform> originalList, int x)
        //{
        //    List<Transform> randomTransforms = new List<Transform>();
        //    List<Transform> tempList = new List<Transform>(originalList);

        //    for (int i = 0; i < x; i++)
        //    {
        //        if (tempList.Count == 0)
        //            break;

        //        int randomIndex = Random.Range(0, tempList.Count);

        //        randomTransforms.Add(tempList[randomIndex]);
        //        tempList.RemoveAt(randomIndex);
        //    }

        //    return randomTransforms;
        //}

        //[Obsolete("Use GetRandomInList<T>() instead")]
        //public static List<GameObject> GetRandomInList(List<GameObject> originalList, int x)
        //{
        //    List<GameObject> randomGameObjects = new List<GameObject>();
        //    List<GameObject> tempList = new List<GameObject>(originalList);

        //    for (int i = 0; i < x; i++)
        //    {
        //        if (tempList.Count == 0)
        //            break;

        //        int randomIndex = Random.Range(0, tempList.Count);

        //        randomGameObjects.Add(tempList[randomIndex]);
        //        tempList.RemoveAt(randomIndex);
        //    }

        //    return randomGameObjects;
        //}

        //public static List<T> RetrieveListInList<T>(List<T> desired, List<T> toRetrive)
        //{
        //    if (toRetrive == null)
        //    {
        //        return desired;
        //    }

        //    foreach (T item in desired.ToList())
        //    {
        //        if (toRetrive.Contains(item))
        //        {
        //            desired.Remove(item);
        //        }
        //    }

        //    return desired;
        //}

        public static bool AlmostEqual(Vector3 pos1, Vector3 pos2, Vector3 step)
        {
            bool isInRangeX = pos1.x - step.x < pos2.x && pos2.x < pos1.x + step.x;
            bool isInRangeY = pos1.y - step.y < pos2.y && pos2.y < pos1.y + step.y;
            bool isInRangeZ = pos1.z - step.z < pos2.z && pos2.z < pos1.z + step.z;

            return isInRangeX && isInRangeY && isInRangeZ;
        }

        public static bool AlmostEqual(float pos1, float pos2, float step)
        {
            return pos1 - step < pos2 && pos2 < pos1 + step;
        }

        public static float BiggestAbs(float x, float y)
        {
            return Mathf.Abs(Mathf.Abs(x) > Mathf.Abs(y) ? x : y);
        }

        public static float SmallestAbs(float x, float y)
        {
            return Mathf.Abs(Mathf.Abs(x) < Mathf.Abs(y) ? x : y);
        }

        public static float ClampNegativeOnly(float value)
        {
            if (value < 0)
                return 0f;
            return value;
        }
    }
}