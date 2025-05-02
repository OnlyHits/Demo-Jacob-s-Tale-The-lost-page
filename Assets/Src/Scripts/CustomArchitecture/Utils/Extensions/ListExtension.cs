using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CustomArchitecture
{
    public static class ListExtension
    {
        // Get random elements on list with Fisher-Yates Shuffle
        // More optimize with large list
        // Todo : make an extension method for List
        public static List<T> GetRandomInList<T>(this List<T> base_list, int element_number = 1)
        {
            if (base_list == null || base_list.Count == 0)
                return new List<T>();

            element_number = Math.Clamp(element_number, 1, base_list.Count);

            List<T> shuffledList = new List<T>(base_list);

            for (int i = 0; i < element_number; i++)
            {
                int randomIndex = UnityEngine.Random.Range(i, shuffledList.Count);
                (shuffledList[i], shuffledList[randomIndex]) = (shuffledList[randomIndex], shuffledList[i]);
            }

            return shuffledList.GetRange(0, element_number);
        }

        public static List<T> RetrieveListInList<T>(this List<T> base_list, List<T> to_find)
        {
            if (to_find == null || base_list == null)
                return base_list ?? new List<T>();

            HashSet<T> to_find_set = new HashSet<T>(to_find);

            return base_list.Where(item => !to_find_set.Contains(item)).ToList();
        }
    }
}