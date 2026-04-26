// ============================================================
// HubManager.cs — Bailiff & Co
// Orchestrateur du Hub. Source de vérité locale pour la session.
// ============================================================
using UnityEngine;

public class HubManager : MonoBehaviour
{
    // ================================================================
    // SINGLETON LOCAL
    // ================================================================

    public static HubManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ================================================================
    // RÉFÉRENCES
    // ================================================================

    [Header("UI")]
    [SerializeField] private HubUI _hubUI;

    [Header("TEST — Retirer en production")]
    [Tooltip("Glisse ici une MissionDef pour tester sans passer par le Chef PNJ.")]
    [SerializeField] private MissionDef _missionTest;

    // ================================================================
    // ÉTAT SESSION
    // ================================================================

    private MissionDef  _missionSelectionnee;
    private VehiculeDef _vehiculeSelectionne;
    private float       _prixLocationVehicule;

    // ================================================================
    // LIFECYCLE
    // ================================================================

    private void Start()
    {
        if (_hubUI == null)
            _hubUI = FindObjectOfType<HubUI>();

        _hubUI?.MettreAJourArgent(GameManager.Instance?.Argent ?? 0f);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        // Auto-sélectionne la mission de test si renseignée
        if (_missionTest != null)
        {
            _missionSelectionnee = _missionTest;
            Debug.Log($"[HubManager] Mission test auto-sélectionnée : {_missionTest.NomMission}");
        }
    }

    // ================================================================
    // SÉLECTION MISSION
    // ================================================================

    public void SelectionnerMission(MissionDef mission)
    {
        if (mission == null) return;
        _missionSelectionnee = mission;
        Debug.Log($"[HubManager] Mission sélectionnée : {mission.NomMission}");
        _hubUI?.AfficherFicheMission(mission);
    }

    // ================================================================
    // LOCATION VÉHICULE
    // ================================================================

    public void DemanderLocationVehicule(VehiculeDef vehicule, float prixLocation)
    {
        if (vehicule == null) return;
        _vehiculeSelectionne  = vehicule;
        _prixLocationVehicule = prixLocation;
        _hubUI?.AfficherPanelVehicule(vehicule, prixLocation);
    }

    public void ConfirmerLocationEtPartir()
    {
        if (_missionSelectionnee == null)
        {
            _hubUI?.AfficherErreur("Aucune mission sélectionnée !\nParle au Chef d'abord.");
            return;
        }

        if (_vehiculeSelectionne == null)
        {
            _hubUI?.AfficherErreur("Aucun véhicule sélectionné.");
            return;
        }

        float argent = GameManager.Instance?.Argent ?? 0f;
        if (argent < _prixLocationVehicule)
        {
            _hubUI?.AfficherErreur(
                $"Fonds insuffisants.\n" +
                $"Location : {_prixLocationVehicule:N0} €\n" +
                $"Ton solde : {argent:N0} €");
            return;
        }

        GameManager.Instance?.Debiter(_prixLocationVehicule);

        Debug.Log($"[HubManager] Départ → {_missionSelectionnee.NomMission} " +
                  $"avec {_vehiculeSelectionne.NomVehicule} ({_prixLocationVehicule:N0} €)");

        GameManager.Instance?.LancerMission(_missionSelectionnee);
    }

    public void AnnulerLocationVehicule()
    {
        _vehiculeSelectionne  = null;
        _prixLocationVehicule = 0f;
        _hubUI?.FermerPanelVehicule();
    }

    // ================================================================
    // PROPRIÉTÉS
    // ================================================================

    public MissionDef  MissionSelectionnee => _missionSelectionnee;
    public VehiculeDef VehiculeSelectionne => _vehiculeSelectionne;
    public bool        MissionChoisie      => _missionSelectionnee != null;

    public void MettreAJourAffichageArgent()
        => _hubUI?.MettreAJourArgent(GameManager.Instance?.Argent ?? 0f);
}
