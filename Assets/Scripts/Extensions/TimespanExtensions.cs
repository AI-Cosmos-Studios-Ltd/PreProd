using System;
using System.Collections.Generic;
using System.Text;

public static class TimespanExtensions {
    public static string GetPrettyPrint(this TimeSpan timespan) {
        if(timespan.TotalDays > 1) {
            return $"{timespan.Days}d, {timespan.Hours}h, {timespan.Minutes}m, {timespan.Seconds}s";
        } 
        else if(timespan.TotalHours > 1) {
            return $"{timespan.Hours}h, {timespan.Minutes}m, {timespan.Seconds}s";
        } 
        else if(timespan.TotalMinutes > 1) {
            return $"{timespan.Minutes}m, {timespan.Seconds}s";
        } 
        else if(timespan.TotalSeconds > 1){
            return $"{timespan.Seconds}s, {timespan.Milliseconds}ms";
        } 
        else {
            return $"{timespan.Milliseconds}ms";
        }
    }
}
