using UnityEngine;

[CreateAssetMenu(fileName = "NewPlush", menuName = "HeavensEve/Plush")]
public class PlushData : ScriptableObject
{
    public string plushName = "Peluche";
    [TextArea] public string[] dialogLines;   // Diálogos que cuenta la hija sobre este peluche (click para avanzar)
    public bool isNegative = false;           // Si es negativo, quita vida al elegirlo
    public int damageOnPick = 1;              // Cuánta vida quita (por defecto 1 toque)

    public Sprite iconSprite;                 // Sprite para el botón/slot del closet
    public Sprite detailSprite;               // Sprite grande para el panel de detalle (opcional)
}