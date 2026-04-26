// ============================================================
// HubUI.cs — Bailiff & Co
// Gère TOUS les panneaux UI du Hub.
// Affichage uniquement — toute logique passe par HubManager.
//
// HIÉRARCHIE CANVAS ATTENDUE :
//   Canvas
//   ├── HUD_Persistant
//   │   ├── Txt_Argent           (TextMeshProUGUI)
//   │   └── Txt_MissionChoisie   (TextMeshProUGUI)
//   ├── Panel_Missions
//   │   ├── (liste de boutons MissionDef)
//   │   └── Panel_FicheMission
//   │       ├── Txt_NomMission
//   │       ├── Txt_NomProprio
//   │       ├── Txt_Trait
//   │       ├── Txt_Securite
//   │       ├── Txt_Citation
//   │       ├── Txt_Quota
//   │       └── Btn_Retour
//   ├── Panel_Boutique
//   ├── Panel_Inventaire
//   ├── Panel_Garage
//   ├── Panel_Vehicule          ← popup location véhicule
//   │   ├── Txt_NomVehicule
//   │   ├── Txt_Prix
//   │   ├── Txt_Capacite
//   │   ├── Txt_Avantage
//   │   ├── Txt_Inconvenient
//   │   ├── Txt_SoldeActuel
//   │   ├── Btn_Louer           → HubManager.ConfirmerLocationEtPartir()
//   │   └── Btn_Annuler         → HubManager.AnnulerLocationVehicule()
//   ├── Popup_Erreur
//   │   ├── Txt_Erreur
//   │   └── Btn_FermerErreur
//   └── Popup_Confirmation (départ)
// ============================================================
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HubUI : MonoBehaviour
{
    // ================================================================
    // HUD PERSISTANT
    // ================================================================

    [Header("HUD Persistant")]
    [SerializeField] private TextMeshProUGUI _txtArgent;
    [SerializeField] private TextMeshProUGUI _txtMissionChoisie;

    // ================================================================
    // PANNEAUX PRINCIPAUX
    // ================================================================

    [Header("Panneaux principaux")]
    [SerializeField] private GameObject _panelMissions;
    [SerializeField] private GameObject _panelBoutique;
    [SerializeField] private GameObject _panelInventaire;
    [SerializeField] private GameObject _panelGarage;

    // ================================================================
    // FICHE MISSION (dans Panel_Missions)
    // ================================================================

    [Header("Fiche Mission")]
    [SerializeField] private GameObject      _panelFicheMission;
    [SerializeField] private TextMeshProUGUI _txtNomMission;
    [SerializeField] private TextMeshProUGUI _txtNomProprio;
    [SerializeField] private TextMeshProUGUI _txtTrait;
    [SerializeField] private TextMeshProUGUI _txtSecurite;
    [SerializeField] private TextMeshProUGUI _txtCitation;
    [SerializeField] private TextMeshProUGUI _txtQuota;
    [SerializeField] private Button          _btnRetourMissions;

    // ================================================================
    // PANEL VÉHICULE — popup location
    // ================================================================

    [Header("Panel Véhicule (popup location)")]
    [SerializeField] private GameObject      _panelVehicule;
    [SerializeField] private TextMeshProUGUI _txtNomVehicule;
    [SerializeField] private TextMeshProUGUI _txtPrixLocation;
    [SerializeField] private TextMeshProUGUI _txtCapacite;
    [SerializeField] private TextMeshProUGUI _txtAvantage;
    [SerializeField] private TextMeshProUGUI _txtInconvenient;
    [SerializeField] private TextMeshProUGUI _txtSoldeActuel;
    [SerializeField] private Button          _btnLouer;
    [SerializeField] private Button          _btnAnnulerVehicule;

    // ================================================================
    // POPUP ERREUR
    // ================================================================

    [Header("Popup Erreur")]
    [SerializeField] private GameObject      _popupErreur;
    [SerializeField] private TextMeshProUGUI _txtErreur;
    [SerializeField] private Button          _btnFermerErreur;

    // ================================================================
    // LIFECYCLE
    // ================================================================

    private void Start()
    {
        FermerTousLesPanneaux();
        BrancherBoutons();
        ActiverCurseur();
    }

    // ================================================================
    // BRANCHEMENT DES BOUTONS
    // ================================================================

    private void BrancherBoutons()
    {
        _btnRetourMissions?.onClick.AddListener(OnRetourMissions);
        _btnLouer?.onClick.AddListener(OnLouer);
        _btnAnnulerVehicule?.onClick.AddListener(OnAnnulerVehicule);
        _btnFermerErreur?.onClick.AddListener(() => _popupErreur?.SetActive(false));
    }

    private void OnDestroy()
    {
        _btnRetourMissions?.onClick.RemoveAllListeners();
        _btnLouer?.onClick.RemoveAllListeners();
        _btnAnnulerVehicule?.onClick.RemoveAllListeners();
        _btnFermerErreur?.onClick.RemoveAllListeners();
    }

    // ================================================================
    // NAVIGATION PANNEAUX — appelé par HubPNJ
    // ================================================================

    public void OuvrirPanelMissions()
    {
        FermerTousLesPanneaux();
        _panelMissions?.SetActive(true);
        ActiverCurseur();
    }

    public void OuvrirPanelBoutique()
    {
        FermerTousLesPanneaux();
        _panelBoutique?.SetActive(true);
        ActiverCurseur();
    }

    public void OuvrirPanelInventaire()
    {
        FermerTousLesPanneaux();
        _panelInventaire?.SetActive(true);
        ActiverCurseur();
    }

    public void OuvrirPanelGarage()
    {
        FermerTousLesPanneaux();
        _panelGarage?.SetActive(true);
        ActiverCurseur();
    }

    public void FermerTousLesPanneaux()
    {
        _panelMissions?.SetActive(false);
        _panelBoutique?.SetActive(false);
        _panelInventaire?.SetActive(false);
        _panelGarage?.SetActive(false);
        _panelFicheMission?.SetActive(false);
        _panelVehicule?.SetActive(false);
        _popupErreur?.SetActive(false);
    }

    // ================================================================
    // FICHE MISSION
    // ================================================================

    public void AfficherFicheMission(MissionDef mission)
    {
        if (mission == null) return;

        _panelFicheMission?.SetActive(true);

        if (_txtNomMission != null)
            _txtNomMission.text = mission.NomMission;

        if (_txtQuota != null)
            _txtQuota.text = $"Quota minimum : {mission.ValeurQuotaMinimum:N0} €";

        var p = mission.Proprietaire;
        if (p == null) return;

        if (_txtNomProprio != null) _txtNomProprio.text = p.Nom;
        if (_txtTrait      != null) _txtTrait.text      = p.TraitCaractere;
        if (_txtCitation   != null) _txtCitation.text   = $"« {p.CitationIndice} »";
        if (_txtSecurite   != null)
            _txtSecurite.text = new string('★', p.NiveauSecurite)
                              + new string('☆', 5 - p.NiveauSecurite);

        // Met à jour le HUD persistant
        if (_txtMissionChoisie != null)
            _txtMissionChoisie.text = $"Mission : {mission.NomMission}";
    }

    // ================================================================
    // PANEL VÉHICULE — popup location
    // ================================================================

    /// <summary>
    /// Affiche le panel de détail du véhicule avec prix de location.
    /// Appelé par HubManager.DemanderLocationVehicule().
    /// </summary>
    public void AfficherPanelVehicule(VehiculeDef vehicule, float prixLocation)
    {
        if (vehicule == null) return;

        // Ferme les autres panneaux sans fermer le panel véhicule
        // (on veut le popup par-dessus le Hub, pas remplacer)
        _panelVehicule?.SetActive(true);
        ActiverCurseur();

        float solde = GameManager.Instance?.Argent ?? 0f;
        bool  peutLouer = solde >= prixLocation;

        if (_txtNomVehicule    != null) _txtNomVehicule.text    = vehicule.NomVehicule;
        if (_txtCapacite       != null) _txtCapacite.text       = $"Capacité : {vehicule.CapaciteObjets} objets";
        if (_txtAvantage       != null) _txtAvantage.text       = vehicule.AvantageDescription;
        if (_txtInconvenient   != null) _txtInconvenient.text   = vehicule.InconvenientDescription;

        if (_txtPrixLocation != null)
            _txtPrixLocation.text = prixLocation <= 0f
                ? "Gratuit"
                : $"Location : {prixLocation:N0} €  /  mission";

        if (_txtSoldeActuel != null)
            _txtSoldeActuel.text = $"Ton solde : {solde:N0} €"
                                 + (peutLouer ? "" : "  ⚠ Fonds insuffisants");

        // Désactive le bouton Louer si pas assez d'argent
        if (_btnLouer != null)
        {
            _btnLouer.interactable = peutLouer;

            var txt = _btnLouer.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null)
                txt.text = prixLocation <= 0f ? "Partir (Gratuit)" : "Louer & Partir";
        }
    }

    public void FermerPanelVehicule()
    {
        _panelVehicule?.SetActive(false);
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
        if (_txtErreur != null) _txtErreur.text = message;
    }

    // ================================================================
    // HUD PERSISTANT
    // ================================================================

    public void MettreAJourArgent(float montant)
    {
        if (_txtArgent != null)
            _txtArgent.text = $"{montant:N0} €";
    }

    // ================================================================
    // HANDLERS BOUTONS
    // ================================================================

    private void OnRetourMissions()
    {
        _panelFicheMission?.SetActive(false);
    }

    private void OnLouer()
    {
        HubManager.Instance?.ConfirmerLocationEtPartir();
    }

    private void OnAnnulerVehicule()
    {
        HubManager.Instance?.AnnulerLocationVehicule();
    }

    // ================================================================
    // UTILITAIRES
    // ================================================================

    private void ActiverCurseur()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    // Utilisé par PlayerController pour bloquer les déplacements
    public bool UnPanneauEstOuvert =>
        (_panelMissions   != null && _panelMissions.activeSelf)   ||
        (_panelBoutique   != null && _panelBoutique.activeSelf)   ||
        (_panelInventaire != null && _panelInventaire.activeSelf) ||
        (_panelGarage     != null && _panelGarage.activeSelf)     ||
        (_panelVehicule   != null && _panelVehicule.activeSelf)   ||
        (_popupErreur     != null && _popupErreur.activeSelf);
}
