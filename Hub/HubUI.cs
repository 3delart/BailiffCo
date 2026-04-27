// ============================================================
// HubUI.cs — Bailiff & Co
// ============================================================
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HubUI : MonoBehaviour
{
    [Header("HUD Persistant")]
    [SerializeField] private TextMeshProUGUI _txtArgent;
    [SerializeField] private TextMeshProUGUI _txtMissionChoisie;

    [Header("Panneaux principaux")]
    [SerializeField] private GameObject _panelMissions;
    [SerializeField] private GameObject _panelBoutique;
    [SerializeField] private GameObject _panelInventaire;
    [SerializeField] private GameObject _panelGarage;

    [Header("Fiche Mission")]
    [SerializeField] private GameObject      _panelFicheMission;
    [SerializeField] private TextMeshProUGUI _txtNomMission;
    [SerializeField] private TextMeshProUGUI _txtNomProprio;
    [SerializeField] private TextMeshProUGUI _txtTrait;
    [SerializeField] private TextMeshProUGUI _txtSecurite;
    [SerializeField] private TextMeshProUGUI _txtCitation;
    [SerializeField] private TextMeshProUGUI _txtQuota;
    [SerializeField] private Button          _btnRetourMissions;

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

    [Header("Popup Erreur")]
    [SerializeField] private GameObject      _popupErreur;
    [SerializeField] private TextMeshProUGUI _txtErreur;
    [SerializeField] private Button          _btnFermerErreur;

    private GameObject _panelAvantVehicule;

    // ================================================================
    // LIFECYCLE
    // ================================================================

    private void Start()
    {
        FermerTousLesPanneaux();
    }

    private void OnDestroy()
    {
        _btnRetourMissions?.onClick.RemoveAllListeners();
        _btnLouer?.onClick.RemoveAllListeners();
        _btnAnnulerVehicule?.onClick.RemoveAllListeners();
        _btnFermerErreur?.onClick.RemoveAllListeners();
    }

    // ================================================================
    // NAVIGATION PANNEAUX
    // ================================================================

    public void OuvrirPanelMissions()
    {
        FermerTousLesPanneaux();
        _panelMissions?.SetActive(true);
    }

    public void OuvrirPanelBoutique()
    {
        FermerTousLesPanneaux();
        _panelBoutique?.SetActive(true);
    }

    public void OuvrirPanelInventaire()
    {
        FermerTousLesPanneaux();
        _panelInventaire?.SetActive(true);
    }

    public void OuvrirPanelGarage()
    {
        FermerTousLesPanneaux();
        _panelGarage?.SetActive(true);
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
        _panelAvantVehicule = null;
    }

    // ================================================================
    // FICHE MISSION
    // ================================================================

    public void AfficherFicheMission(MissionDef mission)
    {
        if (mission == null) return;
        _panelFicheMission?.SetActive(true);

        if (_txtNomMission != null) _txtNomMission.text = mission.NomMission;
        if (_txtQuota      != null) _txtQuota.text      = $"Quota minimum : {mission.ValeurQuotaMinimum:N0} €";

        var p = mission.Proprietaire;
        if (p == null) return;

        if (_txtNomProprio != null) _txtNomProprio.text = p.Nom;
        if (_txtTrait      != null) _txtTrait.text      = p.TraitCaractere;
        if (_txtCitation   != null) _txtCitation.text   = $"« {p.CitationIndice} »";
        if (_txtSecurite   != null)
            _txtSecurite.text = new string('★', p.NiveauSecurite)
                              + new string('☆', 5 - p.NiveauSecurite);

        if (_txtMissionChoisie != null)
            _txtMissionChoisie.text = $"Mission : {mission.NomMission}";
    }

    // ================================================================
    // PANEL VÉHICULE
    // ================================================================

    public void AfficherPanelVehicule(VehiculeDef vehicule, float prixLocation)
    {
        if (vehicule == null) return;

        _panelAvantVehicule = TrouverPanelActif();
        Debug.Log($"[HubUI] AfficherPanelVehicule — panel mémorisé : {(_panelAvantVehicule != null ? _panelAvantVehicule.name : "aucun (joueur dans le parking)")}");

        _panelVehicule?.SetActive(true);

        float solde     = GameManager.Instance?.Argent ?? 0f;
        bool  peutLouer = solde >= prixLocation;

        if (_txtNomVehicule  != null) _txtNomVehicule.text  = vehicule.NomVehicule;
        if (_txtCapacite     != null) _txtCapacite.text     = $"Capacité : {vehicule.CapaciteObjets} objets";
        if (_txtAvantage     != null) _txtAvantage.text     = vehicule.AvantageDescription;
        if (_txtInconvenient != null) _txtInconvenient.text = vehicule.InconvenientDescription;

        if (_txtPrixLocation != null)
            _txtPrixLocation.text = prixLocation <= 0f
                ? "Gratuit"
                : $"Location : {prixLocation:N0} €  /  mission";

        if (_txtSoldeActuel != null)
            _txtSoldeActuel.text = $"Ton solde : {solde:N0} €"
                                 + (peutLouer ? "" : "  ⚠ Fonds insuffisants");

        if (_btnLouer != null)
        {
            _btnLouer.interactable = peutLouer;
            var txt = _btnLouer.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null)
                txt.text = prixLocation <= 0f ? "Partir (Gratuit)" : "Louer & Partir";
        }
    }

    public void FermerPanelVehiculeEtRevenirGarage()
    {
        Debug.Log($"[HubUI] FermerPanelVehiculeEtRevenirGarage — panelAvant={(_panelAvantVehicule != null ? _panelAvantVehicule.name : "null")} | _panelVehicule={(_panelVehicule != null ? _panelVehicule.activeSelf.ToString() : "null")}");

        _panelVehicule?.SetActive(false);

        // Si un panel était ouvert avant (ex: Garage via PNJ), on le rétablit
        // Si le joueur vient du parking directement (pas de panel ouvert), on ferme juste
        if (_panelAvantVehicule != null)
            _panelAvantVehicule.SetActive(true);
        // sinon : rien à rouvrir, le joueur retourne au Hub normalement

        _panelAvantVehicule = null;
    }

    public void FermerPanelVehicule()
    {
        _panelVehicule?.SetActive(false);
        _panelAvantVehicule = null;
    }

    // ================================================================
    // POPUP ERREUR
    // ================================================================

    public void AfficherErreur(string message)
    {
        if (_popupErreur == null) { Debug.LogWarning($"[HubUI] Erreur : {message}"); return; }
        _popupErreur.SetActive(true);
        if (_txtErreur != null) _txtErreur.text = message;
    }

    public void FermerErreur() => _popupErreur?.SetActive(false);

    // ================================================================
    // HUD PERSISTANT
    // ================================================================

    public void MettreAJourArgent(float montant)
    {
        if (_txtArgent != null)
            _txtArgent.text = $"{montant:N0} €";
    }

    // ================================================================
    // HANDLERS BOUTONS — public pour branchement Inspector Unity
    // ================================================================

    public void OnRetourMissions()
    {
        _panelFicheMission?.SetActive(false);
    }

    /// <summary>Brancher sur BtnLouer → On Click () dans l'Inspector.</summary>
    public void OnLouer()
    {
        Debug.Log("[HubUI] OnLouer appelé");
        HubManager.Instance?.ConfirmerLocationEtPartir();
    }

    /// <summary>Brancher sur BtnAnnuler → On Click () dans l'Inspector.</summary>
    public void OnAnnulerVehicule()
    {
        Debug.Log("[HubUI] OnAnnulerVehicule appelé");
        HubManager.Instance?.AnnulerLocationVehicule();
    }

    // ================================================================
    // UTILITAIRES
    // ================================================================

    private GameObject TrouverPanelActif()
    {
        if (_panelGarage     != null && _panelGarage.activeSelf)     return _panelGarage;
        if (_panelMissions   != null && _panelMissions.activeSelf)   return _panelMissions;
        if (_panelBoutique   != null && _panelBoutique.activeSelf)   return _panelBoutique;
        if (_panelInventaire != null && _panelInventaire.activeSelf) return _panelInventaire;
        return null;
    }

    public bool UnPanneauEstOuvert =>
        (_panelMissions   != null && _panelMissions.activeSelf)   ||
        (_panelBoutique   != null && _panelBoutique.activeSelf)   ||
        (_panelInventaire != null && _panelInventaire.activeSelf) ||
        (_panelGarage     != null && _panelGarage.activeSelf)     ||
        (_panelVehicule   != null && _panelVehicule.activeSelf)   ||
        (_popupErreur     != null && _popupErreur.activeSelf);
}
