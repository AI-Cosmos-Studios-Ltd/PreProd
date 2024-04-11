using Newtonsoft.Json;
using System;
using UnityEngine;

public static class MathUtils 
{
    public enum RoundingMode {
        Nearest,
        Ceil,
        Floor
    }

    [Serializable]
    public class IteratorData {
        // Emptry constructor for deserialization
        public IteratorData() { }
        public IteratorData(string name, int maxValue, int minValue = 1, int iteratorStep = 1) {
            Name = name;
            MaxValue = maxValue;
            MinValue = minValue;
            IteratorStep = iteratorStep;
        }
        public string Name;
        /// <summary>
        /// Max is inclusive
        /// </summary>
        public float MaxValue;
        /// <summary>
        /// Min is inclusive
        /// </summary>
        public float MinValue = 1;
        public float IteratorStep = 1;

        [JsonIgnore]
        public float CurrValue { get; private set; }
        [JsonIgnore]
        public int CurrValueInt { get {
                return Mathf.RoundToInt(CurrValue);
            }
        }
        /// <summary>
        /// Please note this is an index and so it starts at 0.
        /// </summary>
        [JsonIgnore]
        public int CurrIterationIndex {
            get {
                return Mathf.RoundToInt((CurrValue - MinValue) / IteratorStep);
            }
        }

        public int GetNumIterations() {
            // We add 1 because we include the initial state as one of the iterations (the default min is 1)
            return Mathf.RoundToInt((MaxValue - MinValue) / IteratorStep) + 1;
        }
        public int GetIteration(int numIterations) {
            return Mathf.RoundToInt(Mathf.Clamp(MinValue + (IteratorStep * numIterations), MinValue, MaxValue));
        }
        public void Reset() {
            CurrValue = MinValue;
        }
        public void SetToMax() {
            CurrValue = MaxValue;
        }
        public bool Increment() {
            if(CurrValue + IteratorStep <= MaxValue) {
                CurrValue += IteratorStep;
                return true;
            }
            return false;
        }
        public bool Decrement() {
            if(CurrValue - IteratorStep >= MinValue) {
                CurrValue -= IteratorStep;
                return true;
            }
            return false;
        }
        public bool Iterate(int numIterations) {
            if(numIterations < 0) {
                int numReverse = Mathf.Abs(numIterations);
                for(int i = 0; i < numReverse; i++) {
                    if(!Decrement()) {
                        return false;
                    }
                }
            }
            else if(numIterations > 0) {
                for(int i = 0; i < numIterations; i++) {
                    if(!Increment()) {
                        return false;
                    }
                }
            }
            return true;
        }
        public override string ToString() {
            return $"[IteratorData:Name={Name},Min={MinValue},Max={MaxValue},Curr={CurrValue},Iterations={GetNumIterations()}]";
        }
    }

    public static int IntPow(int x, int pow) {
        if(pow < 0) {
            return 0;
        }
        if(pow == 0) {
            return 1;
        }
        int ret = x;
        if(pow > 1) {
            for(int i = 1; i < pow; i++) {
                ret *= x;
            }
        }
        return ret;
    }

    public static int RoundToNearest(int rawNum, int sigFiguresToRoundTo, RoundingMode mode = RoundingMode.Nearest) {
        float sigFiguresToRound = (float)rawNum / sigFiguresToRoundTo;

        switch(mode) {
            case RoundingMode.Ceil:
                return Mathf.CeilToInt(sigFiguresToRound) * sigFiguresToRoundTo;

            case RoundingMode.Floor:
                return Mathf.FloorToInt(sigFiguresToRound) * sigFiguresToRoundTo;

            case RoundingMode.Nearest:
            default:
                return Mathf.RoundToInt(sigFiguresToRound) * sigFiguresToRoundTo;
        }
    }

    public static int RoundToSignificantFigures(int rawNum, int sigFigures, RoundingMode mode = RoundingMode.Nearest) {
        int sigFiguresToRoundTo = IntPow(10, sigFigures - 1);
        if(sigFiguresToRoundTo < 1) {
            sigFiguresToRoundTo = 1;
        }
        return RoundToNearest(rawNum, sigFiguresToRoundTo, mode);
    }

}
