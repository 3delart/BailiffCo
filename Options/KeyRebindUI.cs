// ============================================================
// KeyRebindUI.cs — Bailiff & Co
// Ligne de rebind : clic → overlay "Appuyez sur une touche"
// → capture → sauvegarde + rafraîchit toutes les lignes.
// Support AZERTY : affichage des noms localisés.
//
// HIÉRARCHIE (prefab KeyRebindRow) :
//   KeyRebindRow (ce script)
//   ├── LabelAction   TMP  → _labelAction   (ex: "Interagir")
//   └── BoutonTouche  Button → _boutonTouche
//       └── TexteTouche TMP → _texteTouche  (ex: "E")
// ============================================================
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KeyRebindUI : MonoBehaviour
{
    [Header("Action associée")]
    [SerializeField] private ActionJeu _action;
    [SerializeField] private string    _nomAffiche = "Action";

    [Header("Références UI")]
    [SerializeField] private TextMeshProUGUI _labelAction;
    [SerializeField] private Button          _boutonTouche;
    [SerializeField] private TextMeshProUGUI _texteTouche;

    // Overlay global (singleton dans la scène, cherché automatiquement)
    private static RebindOverlay _overlay;

    private OptionsUI _optionsUI;

    // ================================================================
    // LIFECYCLE
    // ================================================================

    private void Awake()
    {
        _optionsUI = GetComponentInParent<OptionsUI>(includeInactive: true);

        if (_labelAction != null)
            _labelAction.text = _nomAffiche;

        _boutonTouche?.onClick.AddListener(CommencerRebind);

        MettreAJourAffichage();
    }

    private void OnEnable() => MettreAJourAffichage();

    // ================================================================
    // REBIND
    // ================================================================

    public void CommencerRebind()
    {
        // Cherche ou crée l'overlay
        if (_overlay == null)
            _overlay = FindObjectOfType<RebindOverlay>(includeInactive: true);

        if (_overlay == null)
        {
            Debug.LogWarning("[KeyRebindUI] RebindOverlay introuvable dans la scène !");
            // Fallback : capture directe sans overlay
            StartCoroutine(CaptureDirecte());
            return;
        }

        _overlay.Afficher(_nomAffiche, OnToucheCapturee);
    }

    private IEnumerator CaptureDirecte()
    {
        if (_texteTouche != null) _texteTouche.text = "...";

        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => !Input.anyKey);

        bool captured = false;
        while (!captured)
        {
            if (Input.GetKeyDown(KeyCode.Escape)) { captured = true; break; }

            foreach (KeyCode kc in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (!EstToucheValide(kc)) continue;
                if (Input.GetKeyDown(kc))
                {
                    OnToucheCapturee(kc);
                    captured = true;
                    break;
                }
            }
            yield return null;
        }
        MettreAJourAffichage();
    }

    private void OnToucheCapturee(KeyCode kc)
    {
        if (OptionsManager.Instance == null) return;

        OptionsData data = OptionsManager.Instance.Data;

        // Résolution des conflits par échange
        foreach (ActionJeu action in System.Enum.GetValues(typeof(ActionJeu)))
        {
            if (action == _action) continue;
            if (data.GetTouche(action) == kc)
            {
                KeyCode ancienne = data.GetTouche(_action);
                data.SetTouche(action, ancienne);
                break;
            }
        }

        data.SetTouche(_action, kc);
        _optionsUI?.RafraichirToutesTouches();
    }

    // ================================================================
    // AFFICHAGE
    // ================================================================

    public void MettreAJourAffichage()
    {
        if (OptionsManager.Instance == null || _texteTouche == null) return;
        KeyCode kc = OptionsManager.Instance.Data.GetTouche(_action);
        _texteTouche.text = FormatKeyCode(kc);
    }

    // ================================================================
    // FORMAT AZERTY
    // ================================================================

    public static string FormatKeyCode(KeyCode kc) => kc switch
    {
        // Lettres AZERTY (Unity stocke en QWERTY physique)
        KeyCode.A           => "Q",
        KeyCode.Q           => "A",
        KeyCode.W           => "Z",
        KeyCode.Z           => "W",
        KeyCode.M           => "M",    // reste M
        KeyCode.Semicolon   => "M",

        // Modificateurs
        KeyCode.LeftShift   => "Shift G",
        KeyCode.RightShift  => "Shift D",
        KeyCode.LeftControl => "Ctrl G",
        KeyCode.RightControl=> "Ctrl D",
        KeyCode.LeftAlt     => "Alt G",
        KeyCode.RightAlt    => "AltGr",

        // Navigation
        KeyCode.Return      => "Entrée",
        KeyCode.Backspace   => "Retour",
        KeyCode.Delete      => "Suppr",
        KeyCode.UpArrow     => "↑",
        KeyCode.DownArrow   => "↓",
        KeyCode.LeftArrow   => "←",
        KeyCode.RightArrow  => "→",
        KeyCode.Space       => "Espace",
        KeyCode.Escape      => "Échap",
        KeyCode.Tab         => "Tab",
        KeyCode.CapsLock    => "Verr. Maj",
        KeyCode.PageUp      => "Pg↑",
        KeyCode.PageDown    => "Pg↓",
        KeyCode.Home        => "Début",
        KeyCode.End         => "Fin",

        // Clics souris
        KeyCode.Mouse0      => "Clic G",
        KeyCode.Mouse1      => "Clic D",
        KeyCode.Mouse2      => "Molette",

        _                   => kc.ToString().ToUpper()
    };

    // ================================================================
    // FILTRE TOUCHES VALIDES
    // ================================================================

    public static bool EstToucheValide(KeyCode kc)
    {
        if (kc == KeyCode.None) return false;
        // Exclut joystick
        if ((int)kc >= (int)KeyCode.JoystickButton0) return false;
        return true;
    }

    // ================================================================
    // API PUBLIQUE
    // ================================================================

    public ActionJeu Action => _action;

    public void AnnulerRebind()
    {
        StopAllCoroutines();
        _overlay?.Cacher();
        MettreAJourAffichage();
    }
}