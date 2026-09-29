using UnityEngine;

public enum PatternType
{
    Cross,
    Frame,
    FilledSquare
}

[System.Serializable]
public struct PatternParams
{
    public PatternType patternType;
    public UnityEngine.Vector2 offsetXY;   // нормализовано или в UV
    public int rotationSteps45;            // 0..7 (0=0°, 1=45°...)
    public float scale;                    // например 0.5..1.5
}