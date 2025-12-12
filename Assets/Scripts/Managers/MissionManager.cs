using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance;

    public enum MissionPhase
    {
        None,
        TalkToNPC,
        FindGifts,
        GiftsFound,
        DeliverGifts,
        ClosetOpen,
        ClosetCollect,
        BikeRepair,
        PlaceDisk,
        Completed
    }

    [Header("Configuración")]
    public int giftsTarget = 3;

    [Header("Mensajes")]
    public string msgTalkToNPC = "Habla con el NPC";
    public string msgFindGifts = "Encuentra los regalos";
    public string msgGiftsProgress = "Regalos: {0}/{1}";
    public string msgGiftsFound = "Regalos encontrados";
    public string msgDeliverGifts = "Entrega los regalos";
    public string msgDeliverGiftsProgress = "Entrega los regalos";
    public string msgOpenCloset = "Abre el closet";
    public string msgClosetCollect = "Saca los peluches favoritos de tu hija";
    public string msgClosetDone = "Peluche de Pinguino entregado";
    public string msgRepairBike = "Repara la bici";
    public string msgBikeDone = "Bici reparada";
    public string msgPlaceDisk = "Coloca el disco";
    public string msgDiskPlaced = "Tocadiscos activado";

    [Header("UI (opcional)")]
    public TextMeshProUGUI missionText;
    public string missionTextTag = "MissionText";

    [Header("Flags (opcional)")]
    public MissionFlagsSO flagsAsset;

    [Header("Estado (debug)")]
    public MissionPhase currentPhase = MissionPhase.None;
    public int giftsCollected = 0;      // Solo para bosque/entrega
    public bool bosqueCompleted = false;

    private HashSet<int> collectedGiftIDs = new HashSet<int>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        TryAssignMissionUI();
        EnsureFlags();
        OnSceneEntered(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryAssignMissionUI();
        EnsureFlags();
        OnSceneEntered(scene.name);
    }

    private void EnsureFlags()
    {
        if (flagsAsset == null)
        {
            flagsAsset = Resources.Load<MissionFlagsSO>("MissionFlags");
        }
    }

    private void TryAssignMissionUI()
    {
        if (missionText != null) return;

        // Busca por tag, incluyendo objetos inactivos
        var taggedObjs = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
        foreach (var t in taggedObjs)
        {
            if (t != null && t.CompareTag(missionTextTag))
            {
                missionText = t;
                Debug.Log($"[MissionManager] missionText asignado por tag '{missionTextTag}' en objeto '{t.name}' (activo={t.gameObject.activeInHierarchy}).");
                return;
            }
        }

        // Fallback: toma el primero en escena si no hay tag
        var any = FindFirstObjectByType<TextMeshProUGUI>(FindObjectsInactive.Include);
        if (any != null)
        {
            missionText = any;
            Debug.Log($"[MissionManager] missionText asignado por fallback en objeto '{any.name}' (activo={any.gameObject.activeInHierarchy}).");
        }
        else
        {
            Debug.LogWarning("[MissionManager] No se encontró TextMeshProUGUI para missionText. Asigna en Inspector o pon tag 'MissionText'.");
        }
    }

    private void UpdateMissionText(string text)
    {
        if (missionText == null) TryAssignMissionUI();
        if (missionText != null)
        {
            missionText.text = text;
            Debug.Log($"[MissionManager] UI actualizado -> \"{text}\"");
        }
        else
        {
            Debug.LogWarning($"[MissionManager] No missionText; no se pudo mostrar: \"{text}\"");
        }
    }

    // ---------------- Scene entry ----------------
    public void OnSceneEntered(string sceneName)
    {
        Debug.Log($"[MissionManager] OnSceneEntered: {sceneName}. CurrentPhase={currentPhase}, giftsCollected={giftsCollected}/{giftsTarget}");

        if (sceneName == "Bosque")
        {
            currentPhase = MissionPhase.TalkToNPC;
            giftsCollected = 0;
            collectedGiftIDs.Clear();
            UpdateMissionText(msgTalkToNPC);
        }
        else if (sceneName == "Exterior" || sceneName == "Sala")
        {
            // Entrega de regalos (texto de transición)
            if (bosqueCompleted || (flagsAsset && flagsAsset.bosqueCompleted))
            {
                currentPhase = MissionPhase.DeliverGifts;
                UpdateMissionText(msgDeliverGifts);
            }
            else
            {
                currentPhase = MissionPhase.DeliverGifts;
                UpdateMissionText(msgDeliverGifts);
            }
        }
        else if (sceneName == "Cuarto")
        {
            currentPhase = MissionPhase.ClosetOpen;
            UpdateMissionText(msgOpenCloset);
        }
        else if (sceneName == "Sotano")
        {
            bool bikeDone = flagsAsset && flagsAsset.sotanoBikeCompleted;
            bool allCore = flagsAsset && flagsAsset.AllCoreCompleted();
            if (!bikeDone)
            {
                currentPhase = MissionPhase.BikeRepair;
                UpdateMissionText(msgRepairBike);
            }
            else if (allCore && !(flagsAsset && flagsAsset.tocadiscosCompleted))
            {
                currentPhase = MissionPhase.PlaceDisk;
                UpdateMissionText(msgPlaceDisk);
            }
            else
            {
                currentPhase = MissionPhase.Completed;
                UpdateMissionText(msgDiskPlaced);
            }
        }
        else
        {
            // Otras escenas / MainMenu
            UpdateMissionText("");
        }
    }

    // ---------------- Bosque flow ----------------
    public void OnTalkedToNPC()
    {
        Debug.Log("[MissionManager] OnTalkedToNPC called.");
        if (currentPhase == MissionPhase.TalkToNPC || currentPhase == MissionPhase.None)
        {
            currentPhase = MissionPhase.FindGifts;
            UpdateMissionText(string.Format(msgGiftsProgress, giftsCollected, giftsTarget));
        }
    }
    public void OnGiftCollected(GameObject giftGO)
    {
        if (!ShouldCountGift()) return;

        int id = giftGO != null ? giftGO.GetInstanceID() : -1;
        if (id != -1 && collectedGiftIDs.Contains(id)) return;
        if (id != -1) collectedGiftIDs.Add(id);

        IncrementGiftCount();
    }

    // Sobrecarga para llamadas que envían GiftData (ej. InteractableObject)
    public void OnGiftCollected(GiftData giftData)
    {
        if (!ShouldCountGift()) return;
        // No hay instanceID, así que no hacemos de-dupe por ID; sirve para pickups de inventario
        IncrementGiftCount();
    }

    private bool ShouldCountGift()
    {
        return currentPhase == MissionPhase.FindGifts
            || currentPhase == MissionPhase.DeliverGifts
            || currentPhase == MissionPhase.TalkToNPC; // por si aún no cambiaste fase tras diálogo
    }

    private void IncrementGiftCount()
    {
        giftsCollected = Mathf.Min(giftsCollected + 1, giftsTarget);
        UpdateMissionText(string.Format(msgGiftsProgress, giftsCollected, giftsTarget));

        if (giftsCollected >= giftsTarget)
        {
            currentPhase = MissionPhase.GiftsFound;
            UpdateMissionText(msgGiftsFound);
            bosqueCompleted = true;
            if (flagsAsset != null) flagsAsset.bosqueCompleted = true;
        }
    }

    // ---------------- Closet flow ----------------
    public void OnClosetOpened()
    {
        currentPhase = MissionPhase.ClosetOpen;
        UpdateMissionText(msgClosetCollect);
    }

    public void OnClosetMissionCompleted()
    {
        currentPhase = MissionPhase.ClosetCollect;
        UpdateMissionText(msgClosetDone);
        if (flagsAsset != null) flagsAsset.cuartoCompleted = true;
    }

    // ---------------- Bike flow ----------------
    public void OnBikeRepaired()
    {
        currentPhase = MissionPhase.BikeRepair;
        UpdateMissionText(msgBikeDone);
        if (flagsAsset != null) flagsAsset.sotanoBikeCompleted = true;

        // Si todas core están completas, pasar a PlaceDisk
        if (flagsAsset != null && flagsAsset.AllCoreCompleted())
        {
            currentPhase = MissionPhase.PlaceDisk;
            UpdateMissionText(msgPlaceDisk);
        }
    }

    // ---------------- Tocadiscos ----------------
    public void OnPlaceDisk()
    {
        currentPhase = MissionPhase.Completed;
        UpdateMissionText(msgDiskPlaced);
        if (flagsAsset != null) flagsAsset.tocadiscosCompleted = true;
    }
}