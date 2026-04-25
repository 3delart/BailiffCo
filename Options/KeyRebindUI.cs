// ============================================================
// KeyRebindUI.cs — Bailiff & Co
// À mettre sur chaque ligne de la section Touches.
// Affiche la touche actuelle, attend un appui, sauvegarde.
//
// HIÉRARCHIE (prefab KeyRebindRow) :
//   KeyRebindRow (ce script)
//   ├── LabelAction   TMP  → _labelAction   (ex: "Interagir")
//   ├── BoutonTouche  Button → _boutonTouche
//   │   └── TexteTouche TMP → _texteTouche  (ex: "E")
//   └── IconeConflict Image → _iconeConflit (icône ⚠ si conflit)
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
    [SerializeField] private Image           _iconeConflit;

    // État
    private bool    _enAttente = false;
    private Color   _couleurNormale;
    private Color   _couleurAttente = new Color(1f, 0.85f, 0.2f); // jaune

    // Référence vers le panneau parent pour notifier les conflits
    private OptionsUI _optionsUI;

    // ================================================================
    // LIFECYCLE
    // ================================================================

    private void Awake()
    {
        _optionsUI = GetComponentInParent<OptionsUI>();

        if (_labelAction  != null) _labelAction.text  = _nomAffiche;
        if (_boutonTouche != null)
        {
            _couleurNormale = _boutonTouche.image.color;
            _boutonTouche.onClick.AddListener(CommencerRebind);
        }
        if (_iconeConflit != null) _iconeConflit.gameObject.SetActive(false);

        MettreAJourAffichage();
    }

    private void OnEnable()
    {
        MettreAJourAffichage();
    }

    // ================================================================
    // REBIND
    // ================================================================

    public void CommencerRebind()
    {
        if (_enAttente) return;
        StartCoroutine(AttendreInput());
    }

    private IEnumerator AttendreInput()
    {
        _enAttente = true;

        if (_texteTouche  != null) _texteTouche.text       = "...";
        if (_boutonTouche != null) _boutonTouche.image.color = _couleurAttente;

        // Attend qu'aucune touche ne soit enfoncée (évite de capturer la touche du clic)
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => !Input.anyKey);

        // Attend la prochaine touche
        bool captured = false;
        while (!captured)
        {
            // Echap = annuler
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                captured = true;
                break;
            }

            // Parcourt tous les KeyCodes possibles
            foreach (KeyCode kc in System.Enum.GetValues(typeof(KeyCode)))
            {
                // Ignore les touches système / souris / joystick
                if ((int)kc >= (int)KeyCode.Mouse0 && (int)kc <= (int)KeyCode.Mouse6) continue;
                if ((int)kc >= (int)KeyCode.JoystickButton0) continue;
                if (kc == KeyCode.None) continue;

                if (Input.GetKeyDown(kc))
                {
                    AppliquerNouvelleTouche(kc);
                    captured = true;
                    break;
                }
            }

            yield return null;
        }

        _enAttente = false;
        if (_boutonTouche != null) _boutonTouche.image.color = _couleurNormale;
        MettreAJourAffichage();
    }

    private void AppliquerNouvelleTouche(KeyCode kc)
    {
        if (OptionsManager.Instance == null) return;

        // Vérifie les conflits
        OptionsData data = OptionsManager.Instance.Data;
        foreach (ActionJeu action in System.Enum.GetValues(typeof(ActionJeu)))
        {
            if (action == _action) continue;
            if (data.GetTouche(action) == kc)
            {
                // Conflit : échange les touches
                KeyCode ancienne = data.GetTouche(_action);
                data.SetTouche(action, ancienne);
                Debug.Log($"[KeyRebind] Conflit résolu : {action} → {ancienne}");
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

    private string FormatKeyCode(KeyCode kc) => kc switch
    {
        KeyCode.LeftShift    => "Shift G",
        KeyCode.RightShift   => "Shift D",
        KeyCode.LeftControl  => "Ctrl G",
        KeyCode.RightControl => "Ctrl D",
        KeyCode.LeftAlt      => "Alt G",
        KeyCode.RightAlt     => "Alt D",
        KeyCode.Return       => "Entrée",
        KeyCode.Backspace    => "Retour",
        KeyCode.Delete       => "Suppr",
        KeyCode.UpArrow      => "↑",
        KeyCode.DownArrow    => "↓",
        KeyCode.LeftArrow    => "←",
        KeyCode.RightArrow   => "→",
        KeyCode.Space        => "Espace",
        KeyCode.Escape       => "Échap",
        KeyCode.Tab          => "Tab",
        KeyCode.CapsLock     => "Verr. Maj",
        _                    => kc.ToString().ToUpper()
    };

    // ================================================================
    // PROPRIÉTÉS
    // ================================================================

    public ActionJeu Action => _action;
    public bool EnAttente   => _enAttente;
    public void AnnulerRebind()
    {
        if (_enAttente) StopAllCoroutines();
        _enAttente = false;
        if (_boutonTouche != null) _boutonTouche.image.color = _couleurNormale;
        MettreAJourAffichage();
    }
}
