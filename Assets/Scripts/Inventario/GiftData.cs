using UnityEngine;
public class GiftData : MonoBehaviour
{
    public string giftName = "Regalo";
    [TextArea] public string description;
    public Sprite iconSprite;     // Sprite pequeño que se mostrará en el slot
    public Sprite detailSprite;   // Sprite grande para el panel de detalle (puede ser el mismo)
}