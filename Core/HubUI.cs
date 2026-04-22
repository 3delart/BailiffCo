// ============================================================
// HubUI.cs — Bailiff & Co
// Gère tous les panneaux UI du Hub.
// Un seul panneau visible à la fois.
// S'abonne à HubManager pour afficher les bonnes données.
//
// SETUP UNITY :
//   Canvas (Screen Space Overlay)
//   ├── PanneauMissions
//   ├── PanneauBoutique
//   ├── PanneauInventaire
//   ├── PanneauGarage
//   ├── PanneauFicheMission
//   ├── PopupConfirmationDepart
//   ├── PopupErreur
//   └── ArgentDisplay (TextMeshPro)
// ============================================================
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HubUI : MonoBehaviour
{
    // ================================================================
    // SÉRIALISATION
    // ================================================================

    [Header("Panneaux principaux")]
    [SerializeField] private GameObject _panneauMissions;
    [SerializeField] private GameObject _panneauBoutique;
    [SerializeField] private GameObject _panneauInventaire;
    [SerializeField] private GameObject _panneauGarage;

    [Header("Fiche mission (dans PanneauMissions)")]
    [SerializeField] private GameObject      _panneauFicheMission;
    [SerializeField] private TextMeshProUGUI _ficheNomMission;
    [SerializeField] private TextMeshProUGUI _ficheNomPropio;
    [SerializeField] private TextMeshProUGUI _ficheTraitCaractere;
    [SerializeField] private TextMeshProUGUI _ficheNiveauSecurite;
    [SerializeField] private TextMeshProUGUI _ficheCitationIndice;
    [SerializeField] private TextMeshProUGUI _ficheValeurEstimee;
    [SerializeField] private Button          _boutonLancerMission;
    [SerializeField] private Button          _boutonRetourMissions;

    [Header("Popup confirmation départ")]
    [SerializeField] private GameObject      _popupConfirmation;
    [SerializeField] private TextMeshProUGUI _popupTexteConfirmation;
    [SerializeField] private Button          _boutonConfirmerOui;
    [SerializeField] private Button          _boutonConfirmerNon;

    [Header("Popup erreur")]
    [SerializeField] private GameObject      _popupErreur;
    [SerializeField] private TextMeshProUGUI _popupTexteErreur;
    [SerializeField] private Button          _boutonFermerErreur;

    [Header("HUD persistant")]
    [SerializeField] private TextMeshProUGUI _argentDisplay;
    [SerializeField] private TextMeshProUGUI _vehiculeDisplay;

    [Header("Boutons de navigation")]
    [SerializeField] private Button _boutonRetourMenu;

    // ================================================================
    // LIFECYCLE
    // ================================================================

    private void Start()
    {
        // Ferme tout au départ
        FermerTousLesPanneaux();

        // Branche les boutons
        _boutonLancerMission?.onClick.AddListener(OnLancerMission);
        _boutonRetourMissions?.onClick.AddListener(OnRetourMissions);
        _boutonConfirmerOui?.onClick.AddListener(OnConfirmerOui);
        _boutonConfirmerNon?.onClick.AddListener(OnConfirmerNon);
        _boutonFermerErreur?.onClick.AddListener(() => _popupErreur?.SetActive(false));
        _boutonRetourMenu?.onClick.AddListener(() => GameManager.Instance?.AllerAuMenu());

        // Déverrouille le curseur (on est dans le Hub, pas en jeu)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    // ================================================================
    // NAVIGATION PANNEAUX
    // ================================================================

    public void OuvrirPanneauMissions()
    {
        FermerTousLesPanneaux();
        _panneauMissions?.SetActive(true);
        ActiverCurseur();
    }

    public void OuvrirPanneauBoutique()
    {
        FermerTousLesPanneaux();
        _panneauBoutique?.SetActive(true);
        ActiverCurseur();
    }

    public void OuvrirPanneauInventaire()
    {
        FermerTousLesPanneaux();
        _panneauInventaire?.SetActive(true);
        ActiverCurseur();
    }

    public void OuvrirPanneauGarage()
    {
        FermerTousLesPanneaux();
        _panneauGarage?.SetActive(true);
        ActiverCurseur();
    }

    public void FermerTousLesPanneaux()
    {
        _panneauMissions?.SetActive(false);
        _panneauBoutique?.SetActive(false);
        _panneauInventaire?.SetActive(false);
        _panneauGarage?.SetActive(false);
        _panneauFicheMission?.SetActive(false);
        _popupConfirmation?.SetActive(false);
        _popupErreur?.SetActive(false);

        // Quand aucun panneau n'est ouvert → curseur libre pour se balader
        // mais toujours visible pour interagir avec les PNJ
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    // ================================================================
    // FICHE MISSION
    // ================================================================

    public void AfficherFicheMission(MissionDef mission)
    {
        if (mission == null) return;

        _panneauFicheMission?.SetActive(true);

        if (_ficheNomMission    != null) _ficheNomMission.text    = mission.NomMission;
        if (_ficheValeurEstimee != null)
            _ficheValeurEstimee.text = $"Quota minimum : {mission.ValeurQuotaMinimum:N0} €";

        var proprio = mission.Proprietaire;
        if (proprio != null)
        {
            if (_ficheNomPropio      != null) _ficheNomPropio.text      = proprio.Nom;
            if (_ficheTraitCaractere != null) _ficheTraitCaractere.text = proprio.TraitCaractere;
            if (_ficheCitationIndice != null) _ficheCitationIndice.text = $"« {proprio.CitationIndice} »";
            if (_ficheNiveauSecurite != null)
                _ficheNiveauSecurite.text = "Sécurité : " + new string('★', proprio.NiveauSecurite)
                                          + new string('☆', 5 - proprio.NiveauSecurite);
        }
    }

    // ================================================================
    // POPUP CONFIRMATION DÉPART
    // ================================================================

    public void AfficherPopupConfirmationDepart(MissionDef mission, VehiculeDef vehicule)
    {
        if (_popupConfirmation == null) return;

        _popupConfirmation.SetActive(true);

        if (_popupTexteConfirmation != null)
            _popupTexteConfirmation.text =
                $"Partir en mission ?\n\n" +
                $"<b>{mission.NomMission}</b>\n" +
                $"Véhicule : {vehicule.NomVehicule}\n" +
                $"Quota : {mission.ValeurQuotaMinimum:N0} €";
    }

    // ================================================================
    // POPUP ERREUR
    // ================================================================

    public void AfficherErreur(string message)
    {
        if (_popupErreur == null)
        {
            Debug.LogWarning($"[HubUI] Erreur : {message}");
            return;
        }

        _popupErreur.SetActive(true);
        if (_popupTexteErreur != null)
            _popupTexteErreur.text = message;
    }

    // ================================================================
    // MISE À JOUR AFFICHAGES PERSISTANTS
    // ================================================================

    public void MettreAJourArgent(float montant)
    {
        if (_argentDisplay != null)
            _argentDisplay.text = $"💰 {montant:N0} €";
    }

    public void MettreAJourVehiculeSelectionne(VehiculeDef vehicule)
    {
        if (_vehiculeDisplay != null)
            _vehiculeDisplay.text = vehicule != null
                ? $"🚗 {vehicule.NomVehicule}"
                : "Aucun véhicule";
    }

    // ================================================================
    // HANDLERS BOUTONS
    // ================================================================

    private void OnLancerMission()
    {
        HubManager.Instance?.DemanderDepart();
    }

    private void OnRetourMissions()
    {
        _panneauFicheMission?.SetActive(false);
    }

    private void OnConfirmerOui()
    {
        _popupConfirmation?.SetActive(false);
        HubManager.Instance?.ConfirmerDepart();
    }

    private void OnConfirmerNon()
    {
        _popupConfirmation?.SetActive(false);
    }

    // ================================================================
    // UTILITAIRES
    // ================================================================

    private void ActiverCurseur()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    /// <summary>
    /// Appelé par le PlayerController du Hub quand un panneau est ouvert
    /// pour bloquer les déplacements.
    /// </summary>
    public bool UnPanneauEstOuvert =>
        (_panneauMissions    != null && _panneauMissions.activeSelf)    ||
        (_panneauBoutique    != null && _panneauBoutique.activeSelf)    ||
        (_panneauInventaire  != null && _panneauInventaire.activeSelf)  ||
        (_panneauGarage      != null && _panneauGarage.activeSelf)      ||
        (_popupConfirmation  != null && _popupConfirmation.activeSelf);
}
