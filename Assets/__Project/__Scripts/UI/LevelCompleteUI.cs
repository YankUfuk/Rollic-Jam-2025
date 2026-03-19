using System.Collections;
using UnityEngine;
using TMPro; // If you use TextMeshPro. If not, swap to UnityEngine.UI.Text

public class LevelCompleteUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform container; // panel anchored to top
    [SerializeField] private TextMeshProUGUI messageText;

    [Header("Display Settings")]
    [SerializeField] private string defaultMessage = "Level Completed!";
    [SerializeField] private float fadeInDuration = 0.25f;
    [SerializeField] private float stayDuration = 1.25f;
    [SerializeField] private float fadeOutDuration = 0.35f;

    private Coroutine _playRoutine;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        // Start hidden
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

    }

    private void Start()
    {
        if (EventManager.Instance != null)
        {
            Debug.Log("LevelCompleteUI: Registering to WinEvent");
            EventManager.Instance.Register<WinEvent>(OnWinEvent);
        }
        else
        {
            Debug.LogWarning("LevelCompleteUI: EventManager instance not found!");
        }
    }

    private void OnEnable()
    {

    }

    private void OnDisable()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.Unregister<WinEvent>(OnWinEvent);
        }
    }

    private void OnWinEvent(WinEvent winEvent)
    {
        // Optional: customize message with stage info
        if (messageText != null)
        {
            messageText.text = defaultMessage;
            // Example if you want to show stage:
            // messageText.text = $"{defaultMessage}\nStage: {winEvent.StageId}";
        }

        if (_playRoutine != null)
        {
            StopCoroutine(_playRoutine);
        }

        _playRoutine = StartCoroutine(PlayEffectRoutine());
    }

    private IEnumerator PlayEffectRoutine()
    {
        if (canvasGroup == null)
            yield break;

        // Enable raycast if you want it to block input while visible (usually no for a banner)
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        // Fade in
        float t = 0f;
        while (t < fadeInDuration)
        {
            t += Time.deltaTime;
            float lerp = Mathf.Clamp01(t / fadeInDuration);
            canvasGroup.alpha = lerp;
            yield return null;
        }

        canvasGroup.alpha = 1f;

        // Stay
        yield return new WaitForSeconds(stayDuration);

        // Fade out
        t = 0f;
        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            float lerp = Mathf.Clamp01(t / fadeOutDuration);
            canvasGroup.alpha = 1f - lerp;
            yield return null;
        }

        canvasGroup.alpha = 0f;
    }
}
