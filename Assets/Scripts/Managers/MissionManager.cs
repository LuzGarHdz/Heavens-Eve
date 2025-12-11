using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Si no usas TextMeshPro, cambia a using UnityEngine.UI; y Text en los tipos.

/// <summary>
/// MissionManager centraliza el estado de las misiones y el texto que se muestra en el panel de misiones.
/// - Singleton persistente (DontDestroyOnLoad)
/// - Evita doble conteo de regalos registrando instanceIDs de los objetos recolectados
/// - Expone métodos públicos para conectar con eventos de escenas / diálogos / interacciones
/// 
/// Integración recomendada:
/// - Asignar el TextMeshProUGUI del panel de misiones al campo 'missionText' en el inspector (recomendado).
/// - Llamar a MissionManager.Instance.OnTalkedToNPC() desde SimpleDialogue.onDialogueFinished del NPC.
/// - Llamar a MissionManager.Instance.OnGiftCollected(giftGameObject) desde el código que procesa la recolección de un regalo (InteractableObject.Interact).
/// - Llamar a las demás APIs públicas según lo indicado en los comentarios (OnEnterCuarto, OnClosetOpened, OnClosetMissionCompleted, OnBikeRepaired, OnPlaceDisk, OnSceneEntered).
/// </summary>
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

    [Header("Mensajes (personalizables)")]
    public string msgTalkToNPC = "Habla con el NPC";
    public string msgFindGifts = "Encuentra los regalos";
    public string msgGiftsProgress = "Regalos: {0}/{1}";
    public string msgGiftsFound = "Regalos encontrados";
    public string msgDeliverGifts = "Entrega los regalos";
    public string msgDeliverGiftsProgress = "Entrega los regalos {0}/{1}";
    public string msgOpenCloset = "Abre el closet";
    public string msgClosetCollect = "Saca los peluches favoritos de tu hija";
    public string msgClosetDone = "Peluche de Pinguino entregado";
    public string msgRepairBike = "Repara la bici";
    public string msgPlaceDisk = "Coloca el disco";

    [Header("UI (opcional)")]
    public TextMeshProUGUI missionText;     // Assign en inspector si puedes (preferible)
    public string missionTextTag = "MissionText"; // o asigna Tag al Text para búsqueda automática

    [Header("Estado (debug)")]
    public MissionPhase currentPhase = MissionPhase.None;
    public int giftsCollected = 0;          // progreso numérico manejado por este manager
    public bool bosqueCompleted = false;    // bandera persistente que indica que Bosque fue finalizado

    // Guardar los instanceIDs de regalos ya contabilizados para evitar doble conteo
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
        // Auto asignar UI si no fue asignada manualmente
        TryAssignMissionUI();
        // Inicializar según escena actual
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
        OnSceneEntered(scene.name);
    }

    private void TryAssignMissionUI()
    {
        if (missionText != null) return;

        // Intentar por tag
        if (!string.IsNullOrEmpty(missionTextTag))
        {
            var go = GameObject.FindWithTag(missionTextTag);
            if (go != null)
            {
                var tmp = go.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    missionText = tmp;
                    Debug.Log($"[MissionManager] missionText asignado por tag '{missionTextTag}' (GameObject='{go.name}').");
                    return;
                }
            }
        }

        // Fallback por tipo (si solo hay un TMP en escena relevante)
        var found = FindObjectOfType<TextMeshProUGUI>();
        if (found != null)
        {
            missionText = found;
            Debug.Log($"[MissionManager] missionText asignado por FindObjectOfType ({found.gameObject.name}).");
        }
    }

    // ----------------------------
    // Eventos / APIs públicas
    // ----------------------------

    // Llamar al entrar a cualquier escena (también llamado internamente)
    public void OnSceneEntered(string sceneName)
    {
        Debug.Log($"[MissionManager] OnSceneEntered: {sceneName}. CurrentPhase={currentPhase}, giftsCollected={giftsCollected}/{giftsTarget}");

        if (sceneName == "Bosque")
        {
            if (!bosqueCompleted)
            {
                currentPhase = MissionPhase.TalkToNPC;
                ShowMissionText(msgTalkToNPC);
            }
            else
            {
                // Ya completó Bosque, mostrar que regalos fueron encontrados
                currentPhase = MissionPhase.GiftsFound;
                ShowMissionText(msgGiftsFound);
            }
        }
        else if (sceneName == "Exterior")
        {
            // Si ya completaste la fase del bosque, indicar entrega de regalos
            if (bosqueCompleted || giftsCollected > 0)
            {
                currentPhase = MissionPhase.DeliverGifts;
                ShowMissionText(string.Format(msgDeliverGiftsProgress, giftsCollected, giftsTarget));
            }
            else
            {
                HideMissionText();
            }
        }
        else if (sceneName == "Sala")
        {
            // Mostrar progreso al entrar a Sala (si hay progreso)
            if (giftsCollected > 0)
            {
                currentPhase = MissionPhase.DeliverGifts;
                ShowMissionText(string.Format(msgDeliverGiftsProgress, giftsCollected, giftsTarget));
            }
            else
            {
                HideMissionText();
            }
        }
        else if (sceneName == "Cuarto")
        {
            currentPhase = MissionPhase.ClosetOpen;
            ShowMissionText(msgOpenCloset);
        }
        else if (sceneName == "Sotano")
        {
            // Si se espera reparar bici
            currentPhase = MissionPhase.BikeRepair;
            ShowMissionText(msgRepairBike);
        }
        else
        {
            // Otras escenas: ocultar por defecto
            HideMissionText();
        }
    }

    // Evento: el jugador inicia la partida (opcional)
    public void OnGameStart()
    {
        // Si arrancas desde MainMenu puedes llamar a esto para forzar la misión inicial
        OnSceneEntered(SceneManager.GetActiveScene().name);
    }

    // Evento: el jugador habló con el NPC del Bosque (fusible: conectar desde SimpleDialogue.onDialogueFinished)
    public void OnTalkedToNPC()
    {
        Debug.Log("[MissionManager] OnTalkedToNPC called.");
        if (currentPhase == MissionPhase.TalkToNPC || currentPhase == MissionPhase.None)
        {
            currentPhase = MissionPhase.FindGifts;
            giftsCollected = 0;
            collectedGiftIDs.Clear();
            ShowMissionText(msgFindGifts);
            UpdateGiftsProgressUI();
        }
    }

    // Evento: un objeto regalo fue recogido. Pasa el GameObject del regalo (para evitar duplicados)
    // Llama a este método desde el código que efectúa la recolección (ej. InteractableObject.Interact)
    public void OnGiftCollected(GameObject giftObject)
    {
        if (giftObject == null)
        {
            Debug.LogWarning("[MissionManager] OnGiftCollected was called with null giftObject.");
            return;
        }

        int id = giftObject.GetInstanceID();

        // Evitar contar el mismo regalo más de una vez
        if (collectedGiftIDs.Contains(id))
        {
            Debug.Log($"[MissionManager] Gift (id={id}, name={giftObject.name}) ya contabilizado. Ignorando.");
            return;
        }

        // Solo contar si estamos en la fase de búsqueda o si aún no hemos alcanzado el target
        if (currentPhase == MissionPhase.FindGifts || currentPhase == MissionPhase.DeliverGifts || currentPhase == MissionPhase.None)
        {
            collectedGiftIDs.Add(id);
            giftsCollected = Mathf.Clamp(giftsCollected + 1, 0, giftsTarget);
            Debug.Log($"[MissionManager] Contabilizado regalo (id={id}, name={giftObject.name}). Progreso {giftsCollected}/{giftsTarget}");

            // Actualizar UI
            UpdateGiftsProgressUI();

            // Notificar al GameManager si usas su contabilidad (opcional)
            if (GameManager.Instance != null)
            {
                // IMPORTANTE: Asegúrate de que GameManager no esté incrementando regalos en otro sitio
                // para evitar duplicados. Si prefieres que MissionManager sea la fuente de verdad,
                // comenta la línea siguiente.
               // GameManager.Instance.OnRegaloRecolectado();
            }

            if (giftsCollected >= giftsTarget)
            {
                currentPhase = MissionPhase.GiftsFound;
                bosqueCompleted = true;
                ShowMissionText(msgGiftsFound);
            }
        }
        else
        {
            Debug.Log($"[MissionManager] OnGiftCollected llamado pero currentPhase={currentPhase}. No se contabiliza.");
        }
    }

    // Añade progreso de regalo desde otras misiones (closet, bici etc.)
    // Útil cuando una misión entrega "un regalo" como recompensa.
    public void AddGiftProgress(int amount = 1, string optionalMessage = null)
    {
        if (amount <= 0) return;

        giftsCollected = Mathf.Clamp(giftsCollected + amount, 0, giftsTarget);
        Debug.Log($"[MissionManager] AddGiftProgress({amount}). Ahora {giftsCollected}/{giftsTarget}");
        UpdateGiftsProgressUI();

        if (!string.IsNullOrEmpty(optionalMessage))
            ShowMissionText(optionalMessage);

        if (giftsCollected >= giftsTarget)
        {
            currentPhase = MissionPhase.GiftsFound;
            bosqueCompleted = true;
            ShowMissionText(msgGiftsFound);
        }
    }

    // Evento: entrada a Cuarto (trigger o carga de escena)
    public void OnEnterCuarto()
    {
        currentPhase = MissionPhase.ClosetOpen;
        ShowMissionText(msgOpenCloset);
    }

    // Evento: abrir closet (ejemplo: al interactuar con el closet)
    public void OnClosetOpened()
    {
        currentPhase = MissionPhase.ClosetCollect;
        ShowMissionText(msgClosetCollect);
    }

    // Evento: mision de closet finalizada (ejemplo: entregaste peluche)
    public void OnClosetMissionCompleted()
    {
        // La misión de closet entrega un regalo: sumar 1 al progreso (si no excede)
        AddGiftProgress(1, msgClosetDone);
    }

    // Evento: al entrar a Sala (puede llamar a OnSceneEntered("Sala") automáticamente)
    public void OnEnterSala()
    {
        // Mostrar progreso de entrega
        currentPhase = MissionPhase.DeliverGifts;
        ShowMissionText(string.Format(msgDeliverGiftsProgress, giftsCollected, giftsTarget));
    }

    // Evento: entrar a Sotano
    public void OnEnterSotano()
    {
        currentPhase = MissionPhase.BikeRepair;
        ShowMissionText(msgRepairBike);
    }

    // Evento: bici reparada
    public void OnBikeRepaired()
    {
        // Añadir progreso + mostrar mensaje de entrega parcial
        AddGiftProgress(1);
        ShowMissionText(string.Format(msgDeliverGiftsProgress, giftsCollected, giftsTarget));
    }

    // Evento: acercarse al tocadiscos
    public void OnNearTocadiscos()
    {
        ShowMissionText(msgPlaceDisk);
    }

    // Evento: colocar disco -> quitar texto de misiones
    public void OnPlaceDisk()
    {
        currentPhase = MissionPhase.PlaceDisk;
        HideMissionText();
    }

    // ----------------------------
    // Helpers UI
    // ----------------------------
    private void UpdateGiftsProgressUI()
    {
        if (missionText != null)
        {
            missionText.text = string.Format(msgGiftsProgress, giftsCollected, giftsTarget);
            missionText.gameObject.SetActive(true);
            return;
        }

        if (InteractionManager.Instance != null)
        {
            InteractionManager.Instance.ShowInteraction(string.Format(msgGiftsProgress, giftsCollected, giftsTarget));
            return;
        }

        Debug.Log($"[MissionManager] Progreso: {giftsCollected}/{giftsTarget}");
    }

    private void ShowMissionText(string text)
    {
        if (missionText != null)
        {
            missionText.text = text;
            missionText.gameObject.SetActive(true);
            return;
        }

        if (InteractionManager.Instance != null)
        {
            InteractionManager.Instance.ShowInteraction(text);
            return;
        }

        Debug.Log("[MissionManager] " + text);
    }

    private void HideMissionText()
    {
        if (missionText != null)
        {
            missionText.gameObject.SetActive(false);
            return;
        }

        InteractionManager.Instance?.HideMessage();
    }

    // Dentro de MissionManager agrega este método sobrecargado:
    public void OnGiftCollected(GiftData giftData)
    {
        if (giftData == null)
        {
            Debug.LogWarning("[MissionManager] OnGiftCollected called with null GiftData.");
            return;
        }

        // Si quieres evitar duplicados por GiftData, puedes usar giftData.giftName o un ID único.
        // Aquí se asume que cada GiftData representa un objeto único en escena.
        // Puedes adaptar collectedGiftIDs HashSet para usar giftData.giftName o una propiedad ID.

        // Ejemplo simple: aumentar progreso (y evitar pasarse del target)
        giftsCollected = Mathf.Clamp(giftsCollected + 1, 0, giftsTarget);
        UpdateGiftsProgressUI();

        if (giftsCollected >= giftsTarget)
        {
            currentPhase = MissionPhase.GiftsFound;
            bosqueCompleted = true;
            ShowMissionText(msgGiftsFound);
        }
    }
}