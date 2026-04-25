// ============================================================
// HubUI.cs — Bailiff & Co
// Gère tous les panneaux UI du Hub + HUD persistant.
// Un seul panneau visible à la fois.
//
// SETUP UNITY — Canvas (Screen Space Overlay) :
//
// ── HUD PERSISTANT (toujours visible) ──
//   ├── Curseur                     Image sprite "+" centré
//   ├── ArgentGlobal                HorizontalLayoutGroup
//   │   ├── ArgentLabel             TMP "Argent :"
//   │   └── ArgentValue             TMP → _argentDisplay
//   ├── VehiculeActif               HorizontalLayoutGroup
//   │   ├── VehiculeLabel           TMP "Véhicule :"
//   │   └── VehiculeValue           TMP → _vehiculeDisplay
//   ├── CodeHubPanel                HorizontalLayoutGroup
//   │   ├── CodeLabel               TMP "Hub :"
//   │   ├── CodeValue               TMP → _codeHubValue
//   │   └── BoutonCopierCode        Button → _boutonCopierCode
//   ├── CoopPanel                   HorizontalLayoutGroup
//   │   ├── Joueur1Dot              Image → _joueur1Dot
//   │   ├── Joueur1Nom              TMP   → _joueur1Nom
//   │   ├── Joueur2Dot              Image → _joueur2Dot
//   │   └── Joueur2Nom              TMP   → _joueur2Nom
//   └── LabelInteractionPanel       → _labelInteractionPanel
//       ├── KeyBadge                TMP "[E]"
//       └── LabelText               TMP → _labelInteractionText
//
// ── PANNEAUX PNJ (un seul visible à la fois) ──
//   ├── PanneauMissions             → _panneauMissions
//   ├── PanneauBoutique             → _panneauBoutique
//   ├── PanneauInventaire           → _panneauInventaire
//   ├── PanneauGarage               → _panneauGarage
//   ├── PanneauFicheMission         → _panneauFicheMission
//   │   ├── FicheNomMission         TMP → _ficheNomMission
//   │   ├── FicheNomPropio          TMP → _ficheNomPropio
//   │   ├── FicheTraitCaractere     TMP → _ficheTraitCaractere
//   │   ├── FicheNiveauSecurite     TMP → _ficheNiveauSecurite
//   │   ├── FicheCitationIndice     TMP → _ficheCitationIndice
//   │   ├── FicheValeurEstimee      TMP → _ficheValeurEstimee
//   │   ├── BoutonLancerMission     Button → _boutonLancerMission
//   │   └── BoutonRetourMissions    Button → _boutonRetourMissions
//
// ── POPUPS ──
//   ├── PopupMissionStart           → _popupMissionStart
//   │   ├── MSNomMission            TMP → _msNomMission
//   │   ├── MSVehiculeNom           TMP → _msVehiculeNom
//   │   ├── MSVehiculeCapacite      TMP → _msVehiculeCapacite
//   │   ├── MSQuotaText             TMP → _msQuotaText
//   │   ├── BoutonConfirmerDepart   Button → _boutonConfirmerDepart
//   │   └── BoutonAnnulerDepart     Button → _boutonAnnulerDepart
//   ├── PopupErreur                 → _popupErreur
//   │   ├── PopupTexteErreur        TMP → _popupTexteErreur
//   │   └── BoutonFermerErreur      Button → _boutonFermerErreur
//   └── PopupConfirmation           → _popupConfirmation (legacy HubManager flow)
//       ├── PopupTexteConfirmation  TMP → _popupTexteConfirmation
//       ├── BoutonConfirmerOui      Button → _boutonConfirmerOui
//       └── BoutonConfirmerNon      Button → _boutonConfirmerNon
// ============================================================
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HubUI : MonoBehaviour
{
    // ================================================================
    // SÉRIALISATION — HUD PERSISTANT
    // ================================================================

    [Header("Argent & Véhicule")]
    [SerializeField] private TextMeshProUGUI _argentDisplay;
    [SerializeField] private TextMeshProUGUI _vehiculeDisplay;

    [Header("Code Hub (coop)")]
    [SerializeField] private TextMeshProUGUI _codeHubValue;
    [SerializeField] private Button          _boutonCopierCode;

    [Header("Statut Coop")]
    [SerializeField] private Image           _joueur1Dot;
    [SerializeField] private TextMeshProUGUI _joueur1Nom;
    [SerializeField] private Image           _joueur2Dot;
    [SerializeField] private TextMeshProUGUI _joueur2Nom;
    [SerializeField] private Color           _couleurConnecte   = new Color(0.36f, 0.68f, 0.43f);
    [SerializeField] private Color           _couleurDeconnecte = new Color(0.53f, 0.53f, 0.53f);

    [Header("Label Interaction")]
    [SerializeField] private GameObject      _labelInteractionPanel;
    [SerializeField] private TextMeshProUGUI _labelInteractionText;

    // ================================================================
    // SÉRIALISATION — PANNEAUX PNJ
    // ================================================================

    [Header("Panneaux principaux")]
    [SerializeField] private GameObject _panneauMissions;
    [SerializeField] private GameObject _panneauBoutique;
    [SerializeField] private GameObject _panneauInventaire;
    [SerializeField] private GameObject _panneauGarage;

    [Header("Fiche mission")]
    [SerializeField] private GameObject      _panneauFicheMission;
    [SerializeField] private TextMeshProUGUI _ficheNomMission;
    [SerializeField] private TextMeshProUGUI _ficheNomPropio;
    [SerializeField] private TextMeshProUGUI _ficheTraitCaractere;
    [SerializeField] private TextMeshProUGUI _ficheNiveauSecurite;
    [SerializeField] private TextMeshProUGUI _ficheCitationIndice;
    [SerializeField] private TextMeshProUGUI _ficheValeurEstimee;
    [SerializeField] private Button          _boutonLancerMission;
    [SerializeField] private Button          _boutonRetourMissions;

    // ================================================================
    // SÉRIALISATION — POPUPS
    // ================================================================

    [Header("Popup MissionStart (interaction véhicule)")]
    [SerializeField] private GameObject      _popupMissionStart;
    [SerializeField] private TextMeshProUGUI _msNomMission;
    [SerializeField] private TextMeshProUGUI _msVehiculeNom;
    [SerializeField] private TextMeshProUGUI _msVehiculeCapacite;
    [SerializeField] private TextMeshProUGUI _msQuotaText;
    [SerializeField] private Button          _boutonConfirmerDepart;
    [SerializeField] private Button          _boutonAnnulerDepart;

    [Header("Popup Erreur")]
    [SerializeField] private GameObject      _popupErreur;
    [SerializeField] private TextMeshProUGUI _popupTexteErreur;
    [SerializeField] private Button          _boutonFermerErreur;

    [Header("Popup Confirmation (flow HubManager — legacy)")]
    [SerializeField] private GameObject      _popupConfirmation;
    [SerializeField] private TextMeshProUGUI _popupTexteConfirmation;
    [SerializeField] private Button          _boutonConfirmerOui;
    [SerializeField] private Button          _boutonConfirmerNon;

    [Header("Navigation")]
    [SerializeField] private Button _boutonRetourMenu;

    // ================================================================
    // ÉTAT PRIVÉ
    // ================================================================

    private string           _codeHub = "";
    private PlayerInteractor _interactor;

    // ================================================================
    // LIFECYCLE
    // ================================================================

    private void Start()
    {
        _interactor = FindObjectOfType<PlayerInteractor>();

        FermerTousLesPanneaux();
        GenererCodeHub();

        // Boutons panneaux PNJ
        _boutonLancerMission?.onClick.AddListener(OnLancerMission);
        _boutonRetourMissions?.onClick.AddListener(OnRetourMissions);

        // Boutons popup MissionStart (interaction véhicule)
        _boutonConfirmerDepart?.onClick.AddListener(OnConfirmerDepart);
        _boutonAnnulerDepart?.onClick.AddListener(OnAnnulerDepart);

        // Boutons popup legacy HubManager
        _boutonConfirmerOui?.onClick.AddListener(OnConfirmerOui);
        _boutonConfirmerNon?.onClick.AddListener(OnConfirmerNon);

        // Boutons communs
        _boutonFermerErreur?.onClick.AddListener(() => _popupErreur?.SetActive(false));
        _boutonRetourMenu?.onClick.AddListener(() => GameManager.Instance?.AllerAuMenu());
        _boutonCopierCode?.onClick.AddListener(CopierCodeHub);

        // Affichage initial
        MettreAJourArgent(GameManager.Instance?.Argent ?? 0f);
        MettreAJourVehiculeSelectionne(null);
        MettreAJourCoop(true, "Hôte", false, "");

        if (_labelInteractionPanel != null)
            _labelInteractionPanel.SetActive(false);

        // Hub → curseur libre
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    private void Update()
    {
        MettreAJourLabelInteraction();
    }

    private void OnEnable()
    {
        EventBus<OnDemandeFinMission>.Subscribe(OnDemandeFinMission);
        EventBus<OnMissionTerminee>.Subscribe(OnRetourDeMission);
    }

    private void OnDisable()
    {
        EventBus<OnDemandeFinMission>.Unsubscribe(OnDemandeFinMission);
        EventBus<OnMissionTerminee>.Unsubscribe(OnRetourDeMission);
    }

    // ================================================================
    // CODE HUB
    // ================================================================

    private void GenererCodeHub()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var rng = new System.Random();
        char[] code = new char[9];
        for (int i = 0; i < 4; i++) code[i] = chars[rng.Next(chars.Length)];
        code[4] = '-';
        for (int i = 5; i < 9; i++) code[i] = chars[rng.Next(chars.Length)];
        _codeHub = new string(code);
        if (_codeHubValue != null) _codeHubValue.text = _codeHub;
    }

    private void CopierCodeHub()
    {
        GUIUtility.systemCopyBuffer = _codeHub;
        Debug.Log($"[HubUI] Code Hub copié : {_codeHub}");
    }

    // ================================================================
    // LABEL INTERACTION
    // ================================================================

    private void MettreAJourLabelInteraction()
    {
        if (_interactor == null || _labelInteractionPanel == null) return;

        // Ne pas afficher le label si un panneau bloquant est ouvert
        if (UnPanneauEstOuvert)
        {
            _labelInteractionPanel.SetActive(false);
            return;
        }

        string label = _interactor.GetLabelCourant();
        bool actif   = !string.IsNullOrEmpty(label);
        _labelInteractionPanel.SetActive(actif);
        if (actif && _labelInteractionText != null)
            _labelInteractionText.text = label;
    }

    // ================================================================
    // NAVIGATION PANNEAUX PNJ
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
        _popupMissionStart?.SetActive(false);
        _popupErreur?.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    // ================================================================
    // FICHE MISSION (panneau Missions)
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
                _ficheNiveauSecurite.text = "Sécurité : "
                    + new string('★', proprio.NiveauSecurite)
                    + new string('☆', 5 - proprio.NiveauSecurite);
        }
    }

    // ================================================================
    // POPUP MISSION START (interaction porte conducteur véhicule)
    // ================================================================

    private void OnDemandeFinMission(OnDemandeFinMission e)
    {
        var mission  = HubManager.Instance?.MissionSelectionnee;
        var vehicule = HubManager.Instance?.VehiculeSelectionne;

        if (mission == null)  { AfficherErreur("Choisissez d'abord une mission !"); return; }
        if (vehicule == null) { AfficherErreur("Aucun véhicule sélectionné !");    return; }

        if (_msNomMission       != null) _msNomMission.text       = mission.NomMission;
        if (_msVehiculeNom      != null) _msVehiculeNom.text      = vehicule.NomVehicule;
        if (_msVehiculeCapacite != null) _msVehiculeCapacite.text = $"{vehicule.CapaciteObjets} objets";
        if (_msQuotaText        != null) _msQuotaText.text        = $"Quota : {mission.ValeurQuotaMinimum:N0} €";

        _popupMissionStart?.SetActive(true);
        ActiverCurseur();
    }

    private void OnConfirmerDepart()
    {
        _popupMissionStart?.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
        EventBus<OnConfirmationDepart>.Raise(new OnConfirmationDepart { Confirme = true });
    }

    private void OnAnnulerDepart()
    {
        _popupMissionStart?.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
        EventBus<OnConfirmationDepart>.Raise(new OnConfirmationDepart { Confirme = false });
    }

    // ================================================================
    // POPUP CONFIRMATION DÉPART (flow legacy HubManager)
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
        if (_popupErreur == null) { Debug.LogWarning($"[HubUI] Erreur : {message}"); return; }
        _popupErreur.SetActive(true);
        if (_popupTexteErreur != null) _popupTexteErreur.text = message;
    }

    // ================================================================
    // HUD — MISES À JOUR
    // ================================================================

    public void MettreAJourArgent(float montant)
    {
        if (_argentDisplay != null)
            _argentDisplay.text = $"{montant:N0} €";
    }

    public void MettreAJourVehiculeSelectionne(VehiculeDef vehicule)
    {
        if (_vehiculeDisplay != null)
            _vehiculeDisplay.text = vehicule != null ? vehicule.NomVehicule : "—";
    }

    /// <summary>
    /// Met à jour les indicateurs de connexion coop.
    /// En V1 : j1 = hôte local (toujours connecté), j2 = toujours déconnecté.
    /// </summary>
    public void MettreAJourCoop(bool j1Connecte, string j1Nom, bool j2Connecte, string j2Nom)
    {
        if (_joueur1Dot != null) _joueur1Dot.color = j1Connecte ? _couleurConnecte : _couleurDeconnecte;
        if (_joueur1Nom != null) _joueur1Nom.text  = j1Connecte ? j1Nom : "—";
        if (_joueur2Dot != null) _joueur2Dot.color = j2Connecte ? _couleurConnecte : _couleurDeconnecte;
        if (_joueur2Nom != null) _joueur2Nom.text  = j2Connecte ? j2Nom : "—";
    }

    // ================================================================
    // HANDLERS BOUTONS — PANELS PNJ
    // ================================================================

    private void OnLancerMission()   => HubManager.Instance?.DemanderDepart();
    private void OnRetourMissions()  => _panneauFicheMission?.SetActive(false);

    private void OnConfirmerOui()
    {
        _popupConfirmation?.SetActive(false);
        HubManager.Instance?.ConfirmerDepart();
    }

    private void OnConfirmerNon() => _popupConfirmation?.SetActive(false);

    // ================================================================
    // RETOUR DE MISSION
    // ================================================================

    private void OnRetourDeMission(OnMissionTerminee e)
    {
        MettreAJourArgent(GameManager.Instance?.Argent ?? 0f);
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
    /// Vérifié par PlayerController pour bloquer les déplacements.
    /// Inclut maintenant aussi les deux popups.
    /// </summary>
    public bool UnPanneauEstOuvert =>
        (_panneauMissions    != null && _panneauMissions.activeSelf)    ||
        (_panneauBoutique    != null && _panneauBoutique.activeSelf)    ||
        (_panneauInventaire  != null && _panneauInventaire.activeSelf)  ||
        (_panneauGarage      != null && _panneauGarage.activeSelf)      ||
        (_popupConfirmation  != null && _popupConfirmation.activeSelf)  ||
        (_popupMissionStart  != null && _popupMissionStart.activeSelf);
}
