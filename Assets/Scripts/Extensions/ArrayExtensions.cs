using System;
using System.Collections.Generic;

public static class ArrayExtensions {
    public static void AddIfUnique<T>(this T[] array, T item) {
        bool contains = false;
        foreach(T t in array) {
            if(t.Equals(item)) {
                contains = true;
                break;
            }
        }
        if(!contains) {
            Array.Resize(ref array, array.Length + 1);
            array[array.Length - 1] = item;
        }
    }

    public static void AddIfUnique<T>(this T[] array, IEnumerable<T> item) {
        // Discover how many unique items we have to add.
        int numUnique = 0;
        bool exists;
        foreach(T t in item) {
            for(int x = 0; x < array.Length; x++) {
                exists = false;
                if(array[x].Equals(t)) {
                    exists = true;
                    break;
                }
                if(!exists) {
                    numUnique++;
                }
            }
        }

        if(numUnique == 0) {
            return;
        }

        // resize the array.
        Array.Resize(ref array, array.Length + numUnique);

        // figure out where to start adding the new items.
        int addIndex = array.Length - numUnique;

        // add the new items.
        foreach(T t in item) {
            for(int x = 0; x < array.Length; x++) {
                exists = false;
                if(array[x].Equals(t)) {
                    exists = true;
                    break;
                }
                if(!exists) {
                    array[addIndex] = t;
                    addIndex++;
                }
            }
        }
    }

    public static T Random<T>(this T[] array) {
        return array[UnityEngine.Random.Range(0, array.Length)];
    }
}