// ============================================================
// SceneLoader.cs — Bailiff & Co
// Singleton persistant. Gère TOUTES les transitions de scènes.
// Fondu noir via CanvasGroup sur un panneau UI noir.
//
// SETUP UNITY (sur le même GameObject que GameManager) :
//   1. Attacher ce script sur le GameObject "GameManager" (Bootstrap)
//   2. Créer un Canvas enfant "FonduCanvas" :
//        Canvas (Screen Space Overlay, Sort Order 999)
//        └── PanneauNoir (Image noire, stretch full screen)
//            └── CanvasGroup (alpha = 0 au départ)
//   3. Assigner _canvasGroupFondu dans l'Inspector
//
// USAGE :
//   SceneLoader.Instance.ChargerScene("Hub");
//   SceneLoader.Instance.ChargerScene("MissionTest", avecFondu: true);
// ============================================================
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    // ================================================================
    // SINGLETON
    // ================================================================

    public static SceneLoader Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // ← AJOUTE CES LIGNES
        // Force le panneau noir transparent au démarrage
        if (_canvasGroupFondu != null)
        {
            _canvasGroupFondu.alpha          = 0f;
            _canvasGroupFondu.blocksRaycasts = false;
            _canvasGroupFondu.interactable   = false;
            var gr = _canvasGroupFondu.GetComponentInParent<UnityEngine.UI.GraphicRaycaster>();
            if (gr) gr.enabled = false;
        }
    }

    // ================================================================
    // SÉRIALISATION
    // ================================================================

    [Header("Fondu noir")]
    [Tooltip("CanvasGroup sur le panneau noir — alpha 0 = transparent, 1 = noir total")]
    [SerializeField] private CanvasGroup _canvasGroupFondu;
    [SerializeField] private float       _dureeFonduOut = 0.5f;  // transparent → noir
    [SerializeField] private float       _dureeFonduIn  = 0.5f;  // noir → transparent

    // ================================================================
    // ÉTAT
    // ================================================================

    private bool _enTransition = false;

    // ================================================================
    // API PUBLIQUE
    // ================================================================

    /// <summary>
    /// Charge une scène, avec ou sans fondu noir.
    /// Si avecFondu = false : chargement instantané (debug uniquement).
    /// </summary>
    public void ChargerScene(string nomScene, bool avecFondu = true)
    {
        if (_enTransition)
        {
            Debug.LogWarning($"[SceneLoader] Transition déjà en cours, ignoré : {nomScene}");
            return;
        }

        if (avecFondu && _canvasGroupFondu != null)
            StartCoroutine(TransitionAvecFondu(nomScene));
        else
            StartCoroutine(ChargerDirectement(nomScene));
    }

    /// <summary>
    /// Fondu vers le noir uniquement (sans changer de scène).
    /// Utilisé par HUDSystem pour OnFondNoir.
    /// </summary>
    public void FondNoir(float duree = 1f)
    {
        if (_canvasGroupFondu != null)
            StartCoroutine(AnimerFondu(0f, 1f, duree));
    }

    /// <summary>Retourne true si une transition est en cours.</summary>
    public bool EnTransition => _enTransition;

    // ================================================================
    // COROUTINES
    // ================================================================

    private IEnumerator TransitionAvecFondu(string nomScene)
    {
        _enTransition = true;

        // 1 — Fondu vers le noir
        yield return StartCoroutine(AnimerFondu(0f, 1f, _dureeFonduOut));

        // 2 — Chargement asynchrone de la scène
        AsyncOperation op = SceneManager.LoadSceneAsync(nomScene);
        op.allowSceneActivation = false;

        // Attend que la scène soit prête (90%)
        while (op.progress < 0.9f)
            yield return null;

        // Active la scène
        op.allowSceneActivation = true;
        yield return null; // attend une frame que la scène soit active

        // 3 — Fondu depuis le noir
        yield return StartCoroutine(AnimerFondu(1f, 0f, _dureeFonduIn));

        _enTransition = false;
    }

    private IEnumerator ChargerDirectement(string nomScene)
    {
        _enTransition = true;
        yield return SceneManager.LoadSceneAsync(nomScene);
        _enTransition = false;
    }

    private IEnumerator AnimerFondu(float alphaDebut, float alphaFin, float duree)
    {
        if (_canvasGroupFondu == null) yield break;

        var gr = _canvasGroupFondu.GetComponentInParent<UnityEngine.UI.GraphicRaycaster>();

        _canvasGroupFondu.alpha          = alphaDebut;
        _canvasGroupFondu.blocksRaycasts = alphaDebut >= 1f;
        if (gr) gr.enabled               = alphaDebut >= 1f;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duree;
            _canvasGroupFondu.alpha = Mathf.Lerp(alphaDebut, alphaFin, Mathf.Clamp01(t));
            yield return null;
        }

        _canvasGroupFondu.alpha          = alphaFin;
        _canvasGroupFondu.blocksRaycasts = alphaFin >= 1f;
        if (gr) gr.enabled               = alphaFin >= 1f;
    }

    // ================================================================
    // ABONNEMENT AUX EVENTS — fondu noir déclenché depuis le jeu
    // ================================================================

    private void OnEnable()
    {
        EventBus<OnFondNoir>.Subscribe(OnFondNoir);
        EventBus<OnMissionTerminee>.Subscribe(OnMissionTerminee);
    }

    private void OnDisable()
    {
        EventBus<OnFondNoir>.Unsubscribe(OnFondNoir);
        EventBus<OnMissionTerminee>.Unsubscribe(OnMissionTerminee);
    }

    private void OnFondNoir(OnFondNoir e)
    {
        FondNoir(e.DureeSecondes);
    }

    private void OnMissionTerminee(OnMissionTerminee e)
    {
        // Délai pour laisser le temps à l'écran de résultats de s'afficher
        StartCoroutine(RetourHubApresDelai(e.Resultat, 3f));
    }

    private IEnumerator RetourHubApresDelai(MissionResult resultat, float delai)
    {
        yield return new WaitForSeconds(delai);
        GameManager.Instance?.TerminerMission(resultat);
    }
}
