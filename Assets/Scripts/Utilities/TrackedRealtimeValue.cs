using System;
using UnityEngine;

public class TrackedRealtimeValue {
    public DateTime LastUpdatedUTC;

    public float Velocity;

    public float MaxValue;

    public float MinValue;

    public TimeSpan VelocityInterval;

    public float Value {
        get {
            UpdateValue();
            return lastUpdateValue;
        }
        set {
            lastUpdateValue = value;
            LastUpdatedUTC = DateTime.UtcNow;
        }
    }
    private float lastUpdateValue;

    private void UpdateValue() {
        TimeSpan timeSinceLastUpdate = DateTime.UtcNow - LastUpdatedUTC;
        float velocityIntervals = (float)timeSinceLastUpdate.TotalSeconds / (float)VelocityInterval.TotalSeconds;
        Value = Mathf.Clamp(lastUpdateValue + (Velocity * velocityIntervals), MinValue, MaxValue);
    }

    public void Save(string savePrefix) {
        PlayerPrefs.SetString($"{savePrefix}_LastUpdatedUTC", LastUpdatedUTC.ToString());
        PlayerPrefs.SetFloat($"{savePrefix}_Velocity", Velocity);
        PlayerPrefs.SetString($"{savePrefix}_VelocityInterval", VelocityInterval.Ticks.ToString());
        PlayerPrefs.SetFloat($"{savePrefix}_Value", lastUpdateValue);
        PlayerPrefs.SetFloat($"{savePrefix}_MaxValue", MaxValue);
        PlayerPrefs.SetFloat($"{savePrefix}_MinValue", MinValue);
    }

    public void Load(string loadPrefix) {
        if(PlayerPrefs.HasKey($"{loadPrefix}_LastUpdatedUTC")) {
            LastUpdatedUTC = DateTime.Parse(PlayerPrefs.GetString($"{loadPrefix}_LastUpdatedUTC"));
        } else {
            LastUpdatedUTC = DateTime.UtcNow;
        }
        if(PlayerPrefs.HasKey($"{loadPrefix}_Velocity")) {
            Velocity = PlayerPrefs.GetFloat($"{loadPrefix}_Velocity", Velocity);
        }
        if(PlayerPrefs.HasKey($"{loadPrefix}_VelocityInterval")) {
            VelocityInterval = new TimeSpan(long.Parse(PlayerPrefs.GetString($"{loadPrefix}_VelocityInterval", VelocityInterval.Ticks.ToString())));
        }
        if(PlayerPrefs.HasKey($"{loadPrefix}_Value")) {
            lastUpdateValue = PlayerPrefs.GetFloat($"{loadPrefix}_Value", Value);
        }
        if(PlayerPrefs.HasKey($"{loadPrefix}_MaxValue")) {
            MaxValue = PlayerPrefs.GetFloat($"{loadPrefix}_MaxValue", MaxValue);
        }
        if(PlayerPrefs.HasKey($"{loadPrefix}_MinValue")) {
            MinValue = PlayerPrefs.GetFloat($"{loadPrefix}_MinValue", MinValue);
        }
    }
}
