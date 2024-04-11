using System;
using System.Collections.Generic;
using UnityEngine;

public class ColorUtils {

    private const int NUM_COLOURS_TO_GENERATE = 200;
    private const int NUM_COLOURS_TO_SHORTLIST = 30;

    /// <summary>
    /// When provided with a function to rate colours for their uniqueness (high values are more unique), 
    /// this function will generate a range of colours and return one of the most unique colours.
    /// </summary>
    public static Color GetUniqueishColour(Func<Color, float> getColourUniquenessScore = null) {
        List<KeyValuePair<Color, float>> colorAndUniqueness = new List<KeyValuePair<Color, float>>();
        
        // Generate a range of colours
        for(int x = 0; x < NUM_COLOURS_TO_GENERATE; x++) {
            Color newColour = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);

            if(newColour.r + newColour.g + newColour.b < 2.25f) {
                // If the colour is too dark, make it lighter
                newColour = newColour * 1.35f;
                newColour.r = Mathf.Min(newColour.r, 1f);
                newColour.g = Mathf.Min(newColour.g, 1f);
                newColour.b = Mathf.Min(newColour.b, 1f);
            }

            // Figure out how different the new colour is from the existing agent colours.
            colorAndUniqueness.Add(new KeyValuePair<Color, float>(newColour, getColourUniquenessScore == null? 0f : getColourUniquenessScore(newColour)));
        }

        if(getColourUniquenessScore != null) {
            // Sort the colours by uniqueness
            colorAndUniqueness.Sort((a, b) => a.Value.CompareTo(b.Value));
            // Cull the least-unique of the colours to make a short-list
            colorAndUniqueness.RemoveRange(0, NUM_COLOURS_TO_GENERATE - NUM_COLOURS_TO_SHORTLIST);
        }
        
        // return a random colour from the remaining colours
        return colorAndUniqueness[UnityEngine.Random.Range(0, colorAndUniqueness.Count)].Key;
    }

    public static Color GetUniqueishColour(int numOtherColours, Func<int,Color> getOtherColour) {
        if(numOtherColours <= 0 || getOtherColour == null) {
            return GetUniqueishColour();
        }

        float GetColourUniquenessScore(Color c) {
            float returnValue = float.MaxValue;
            for(int i = 0; i < numOtherColours; i++) {
                Color otherColour = getOtherColour(i);
                if(otherColour == default) {
                    continue;
                }
                // The combined value of the absolute diff between the RGB values of the colours
                float comparisonValue = Mathf.Min(returnValue, Mathf.Abs(c.r - otherColour.r) + Mathf.Abs(c.g - otherColour.g) + Mathf.Abs(c.b - otherColour.b));

                // If the new comparison is less unique, then take that value instead
                returnValue = Mathf.Min(returnValue, comparisonValue);
            }
            return returnValue;
        }

        return GetUniqueishColour(GetColourUniquenessScore);
    }
}
