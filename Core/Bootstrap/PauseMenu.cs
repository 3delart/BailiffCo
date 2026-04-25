// ============================================================
// PauseMenu.cs — Bailiff & Co
// Menu pause universel — fonctionne en Hub ET en Mission.
// [Echap] → ouvre/ferme le menu.
//
// EN HUB :
//   - Reprendre       → ferme le menu
//   - Personnalisation → scène CharacterCustomization
//   - Options         → panneau options (V2)
//   - Menu principal  → GameManager.AllerAuMenu()
//   Pas de bouton "Abandonner" (pas de mission en cours)
//
// EN MISSION :
//   - Reprendre       → ferme le menu + Time.timeScale = 1
//   - Abandonner      → confirme puis retour Hub sans sauvegarder
//   - Options         → panneau options (V2)
//   - Menu principal  → confirme puis GameManager.AllerAuMenu()
//
// SETUP UNITY — Canvas (Screen Space Overlay, Sort Order 100) :
//
//   PauseMenu (ce script)           → désactivé au départ
//   └── Fond                        → Image noire alpha ~0.6, stretch full
//       └── Carte                   → Image centrée ~280x340px
//           ├── Titre               → TMP "PAUSE"
//           ├── BoutonReprendre     → Button  → _boutonReprendre
//           ├── BoutonAbandonner    → Button  → _boutonAbandonner  (caché en Hub)
//           ├── BoutonOptions       → Button  → _boutonOptions     (grisé V1)
//           ├── BoutonPersonnalisation → Button → _boutonPersonnalisation (visible en Hub only)
//           ├── BoutonMenu          → Button  → _boutonMenu
//           └── PopupConfirmation   → GameObject → _popupConfirmation
//               ├── TexteConfirm    → TMP    → _texteConfirmation
//               ├── BoutonOui       → Button → _boutonConfirmerOui
//               └── BoutonNon       → Button → _boutonConfirmerNon
//
// SETUP :
//   1. Placer ce Canvas dans la scène Hub ET dans la scène Mission
//      (ou sur un prefab partagé chargé additif — V2)
//   2. Cocher _estEnMission dans l'Inspector pour la scène Mission
//   3. Sort Order 100 pour passer au-dessus de tout le reste
// ============================================================
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    // ================================================================
    // SÉRIALISATION
    // ================================================================

    [Header("Contexte")]
    [Tooltip("Cocher si ce menu est dans une scène Mission (active Abandonner, met le jeu en pause)")]
    [SerializeField] private bool _estEnMission = false;

    [Header("Boutons")]
    [SerializeField] private Button _boutonReprendre;
    [SerializeField] private Button _boutonAbandonner;       // caché en Hub
    [SerializeField] private Button _boutonPersonnalisation; // caché en Mission
    [SerializeField] private Button _boutonOptions;
    [SerializeField] private Button _boutonMenu;

    [Header("Popup confirmation")]
    [SerializeField] private GameObject      _popupConfirmation;
    [SerializeField] private TextMeshProUGUI _texteConfirmation;
    [SerializeField] private Button          _boutonConfirmerOui;
    [SerializeField] private Button          _boutonConfirmerNon;

    [Header("Options (V2)")]
    [SerializeField] private GameObject _panneauOptions;

    // ================================================================
    // ÉTAT
    // ================================================================

    private bool _ouvert = false;

    // Action mémorisée pour le popup de confirmation
    private System.Action _actionConfirmee;

    // ================================================================
    // LIFECYCLE
    // ================================================================

    private void Awake()
    {
        gameObject.SetActive(false);

        _boutonReprendre?.onClick.AddListener(Fermer);
        _boutonAbandonner?.onClick.AddListener(OnAbandonner);
        _boutonPersonnalisation?.onClick.AddListener(OnPersonnalisation);
        _boutonOptions?.onClick.AddListener(OnOptions);
        _boutonMenu?.onClick.AddListener(OnMenu);

        _boutonConfirmerOui?.onClick.AddListener(OnConfirmerOui);
        _boutonConfirmerNon?.onClick.AddListener(OnConfirmerNon);

        // Adapte les boutons au contexte
        _boutonAbandonner?.gameObject.SetActive(_estEnMission);
        _boutonPersonnalisation?.gameObject.SetActive(!_estEnMission);

        // Options grisées en V1
        if (_boutonPersonnalisation != null)
            _boutonPersonnalisation.interactable = false;

        if (_popupConfirmation != null)
            _popupConfirmation.SetActive(false);
    }

    // ================================================================
    // OUVRIR / FERMER
    // ================================================================

    public void Ouvrir()
    {
        _ouvert = true;
        gameObject.SetActive(true);

        if (_estEnMission)
            Time.timeScale = 0f; // pause le jeu en mission

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        if (_popupConfirmation != null)
            _popupConfirmation.SetActive(false);
    }

    public void Fermer()
    {
        _ouvert = false;
        gameObject.SetActive(false);

        if (_estEnMission)
            Time.timeScale = 1f; // reprend le jeu

        // Rétablit l'état curseur selon le contexte
        if (_estEnMission)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
        }
        else
        {
            // Hub → curseur libre
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
        }
    }

    // ================================================================
    // HANDLERS BOUTONS
    // ================================================================

    private void OnAbandonner()
    {
        DemanderConfirmation(
            "Abandonner la mission ?\nLa progression sera perdue.",
            () =>
            {
                Time.timeScale = 1f;
                GameManager.Instance?.AllerAuHub();
            }
        );
    }

    private void OnPersonnalisation()
    {
        // Disponible uniquement en Hub
        Fermer();
        SceneLoader.Instance?.ChargerScene(SceneNames.PERSONNALISATION, avecFondu: true);
    }

    private void OnOptions()
    {
        if (_panneauOptions != null)
            _panneauOptions.SetActive(!_panneauOptions.activeSelf);
    }


    private void OnMenu()
    {
        if (_estEnMission)
        {
            DemanderConfirmation(
                "Retourner au menu ?\nLa mission sera abandonnée.",
                () =>
                {
                    Time.timeScale = 1f;
                    GameManager.Instance?.AllerAuMenu();
                }
            );
        }
        else
        {
            // En Hub : pas besoin de confirmation
            GameManager.Instance?.AllerAuMenu();
        }
    }

    // ================================================================
    // POPUP CONFIRMATION
    // ================================================================

    private void DemanderConfirmation(string message, System.Action onOui)
    {
        if (_popupConfirmation == null)
        {
            // Pas de popup → exécute directement
            onOui?.Invoke();
            return;
        }

        _actionConfirmee = onOui;

        if (_texteConfirmation != null)
            _texteConfirmation.text = message;

        _popupConfirmation.SetActive(true);
    }

    private void OnConfirmerOui()
    {
        _popupConfirmation.SetActive(false);
        _actionConfirmee?.Invoke();
        _actionConfirmee = null;
    }

    private void OnConfirmerNon()
    {
        _popupConfirmation.SetActive(false);
        _actionConfirmee = null;
    }

    // ================================================================
    // PROPRIÉTÉS
    // ================================================================

    public bool EstOuvert => _ouvert;
}
