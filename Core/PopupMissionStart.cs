// ============================================================
// PopupMissionStart.cs — Bailiff & Co
// Popup affiché quand le joueur interagit avec la porte
// conducteur du véhicule dans le Hub.
// S'abonne à OnDemandeFinMission (réutilise l'event existant).
//
// HIÉRARCHIE (enfant du Canvas, désactivé au départ) :
//
// MissionStartPopup                → PopupMissionStart.cs
// └── PanneauFond                  → Image (fond sombre semi-transparent, full screen)
//     └── PanneauCarte             → Image (carte centrée, ~340x260px)
//         ├── TitreMission         → TMP  "Démarrer la mission"
//         ├── NomMission           → TMP  "La Collection de Marcel"
//         ├── VehiculePanel        → HorizontalLayoutGroup
//         │   ├── VehiculeIcone    → Image (sprite véhicule)
//         │   ├── VehiculeNom      → TMP  "Pickup"
//         │   └── VehiculeCapacite → TMP  "8 objets"
//         ├── QuotaText            → TMP  "Quota minimum : 12 000 €"
//         └── BoutonsPanel         → HorizontalLayoutGroup
//             ├── BoutonConfirmer  → Button
//             └── BoutonAnnuler    → Button
//
// SETUP :
//   1. Placer ce script sur MissionStartPopup (racine du popup)
//   2. Assigner les références dans l'Inspector
//   3. HubVehicule.cs émet OnDemandeFinMission via EventBus
//      quand le joueur appuie E sur la porte conducteur
//      → ce popup s'affiche automatiquement
// ============================================================
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupMissionStart : MonoBehaviour
{
    // ================================================================
    // SÉRIALISATION
    // ================================================================

    [Header("Textes")]
    [SerializeField] private TextMeshProUGUI _nomMission;
    [SerializeField] private TextMeshProUGUI _vehiculeNom;
    [SerializeField] private TextMeshProUGUI _vehiculeCapacite;
    [SerializeField] private TextMeshProUGUI _quotaText;

    [Header("Véhicule — icône")]
    [Tooltip("Image qui affiche le sprite du véhicule sélectionné")]
    [SerializeField] private Image _vehiculeIcone;

    [Header("Boutons")]
    [SerializeField] private Button _boutonConfirmer;
    [SerializeField] private Button _boutonAnnuler;

    [Header("Références HubHUD (optionnel — pour mise à jour véhicule)")]
    [SerializeField] private HubHUD _hubHUD;

    // ================================================================
    // LIFECYCLE
    // ================================================================

    private void Awake()
    {
        gameObject.SetActive(false);

        _boutonConfirmer?.onClick.AddListener(OnConfirmer);
        _boutonAnnuler?.onClick.AddListener(OnAnnuler);
    }

    private void OnEnable()
    {
        EventBus<OnDemandeFinMission>.Subscribe(OnDemandeFinMission);
    }

    private void OnDisable()
    {
        EventBus<OnDemandeFinMission>.Unsubscribe(OnDemandeFinMission);
    }

    // ================================================================
    // AFFICHAGE
    // ================================================================

    private void OnDemandeFinMission(OnDemandeFinMission e)
    {
        AfficherPopup();
    }

    private void AfficherPopup()
    {
        var mission  = HubManager.Instance?.MissionSelectionnee;
        var vehicule = HubManager.Instance?.VehiculeSelectionne;

        if (mission == null)
        {
            // Pas de mission sélectionnée : on redirige vers le Chef
            FindObjectOfType<HubUI>()?.AfficherErreur("Choisissez d'abord une mission !");
            return;
        }

        if (vehicule == null)
        {
            FindObjectOfType<HubUI>()?.AfficherErreur("Aucun véhicule sélectionné !");
            return;
        }

        // Remplit les données
        if (_nomMission != null)
            _nomMission.text = mission.NomMission;

        if (_vehiculeNom != null)
            _vehiculeNom.text = vehicule.NomVehicule;

        if (_vehiculeCapacite != null)
            _vehiculeCapacite.text = $"{vehicule.CapaciteObjets} objets";

        if (_quotaText != null)
            _quotaText.text = $"Quota minimum : {mission.ValeurQuotaMinimum:N0} €";

        if (_vehiculeIcone != null && vehicule.Prefab != null)
        {
            // TODO : ajouter un champ Sprite IconeVehicule dans VehiculeDef
            // _vehiculeIcone.sprite = vehicule.Icone;
        }

        // Bloque les déplacements du joueur pendant le popup
        SetCurseurEtMouvement(true);

        gameObject.SetActive(true);
    }

    // ================================================================
    // BOUTONS
    // ================================================================

    private void OnConfirmer()
    {
        gameObject.SetActive(false);
        SetCurseurEtMouvement(false);

        // Émet la confirmation → Vehicule.cs écoute OnConfirmationDepart
        EventBus<OnConfirmationDepart>.Raise(new OnConfirmationDepart { Confirme = true });
    }

    private void OnAnnuler()
    {
        gameObject.SetActive(false);
        SetCurseurEtMouvement(false);

        EventBus<OnConfirmationDepart>.Raise(new OnConfirmationDepart { Confirme = false });
    }

    // ================================================================
    // UTILITAIRES
    // ================================================================

    private void SetCurseurEtMouvement(bool popupOuvert)
    {
        Cursor.lockState = popupOuvert ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible   = popupOuvert;
    }
}
