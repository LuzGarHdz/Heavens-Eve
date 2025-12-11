using UnityEngine;

[CreateAssetMenu(fileName = "MissionFlags", menuName = "HeavensEve/Mission Flags")]
public class MissionFlagsSO : ScriptableObject
{
    [Header("Misiones principales")]
    public bool bosqueCompleted;
    public bool cuartoCompleted;
    public bool sotanoBikeCompleted;

    [Header("Final")]
    public bool tocadiscosCompleted;

    public bool AllCoreCompleted()
    {
        return bosqueCompleted && cuartoCompleted && sotanoBikeCompleted;
    }
    public void ResetAll()
    {
        bosqueCompleted = false;
        cuartoCompleted = false;
        sotanoBikeCompleted = false;
        tocadiscosCompleted = false;
    }
}