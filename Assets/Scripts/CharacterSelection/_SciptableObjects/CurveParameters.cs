using UnityEngine;

[CreateAssetMenu(fileName = "CurveParameters", menuName = "Character Curve Parameters")]
public class CurveParameters : ScriptableObject
{
    public AnimationCurve positioning;
    public float positioningInfluence = .1f;
    public AnimationCurve rotation;
    public float rotationInfluence = 10f;
}
