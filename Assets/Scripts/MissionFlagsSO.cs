using UnityEngine;

[CreateAssetMenu(fileName = "MissionFlags", menuName = "HeavensEve/Mission Flags")]
public class MissionFlagsSO : ScriptableObject
{
    [Header("Misiones principales")]
    public bool bosqueCompleted;        // Bosque: regalos a tiempo
    public bool cuartoCompleted;        // Cuarto: peluches correctos
    public bool sotanoBikeCompleted;    // Sótano 1: reparar bicicleta
    public bool sotanoFinalCompleted;   // Sótano 2: misión final (la bloqueada)

    // ¿Todas las misiones (excepto la final del sótano) están completas?
    public bool AllCoreCompleted()
    {
        return bosqueCompleted && cuartoCompleted && sotanoBikeCompleted;
    }
}