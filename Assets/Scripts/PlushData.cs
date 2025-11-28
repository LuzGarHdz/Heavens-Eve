using UnityEngine;

[CreateAssetMenu(fileName = "NewPlush", menuName = "HeavensEve/Plush")]
public class PlushData : ScriptableObject
{
    public string plushName = "Peluche";
    [TextArea] public string[] dialogLines;
    public bool isNegative = false;      // Si es negativo, quita vida
    public int damageOnPick = 1;

    public bool isCorrect = false;       // NUEVO: marca este peluche como "correcto" (opcional, por si no quieres usar la lista en ClosetUI)

    public Sprite iconSprite;
    public Sprite detailSprite;
}