// ============================================================
// HubManager.cs — Bailiff & Co
// Orchestre toute la logique du Hub.
// Stocke la mission et le véhicule sélectionnés.
// Gère le départ en mission.
//
// SETUP UNITY :
//   Créer un GameObject "HubManager" dans la scène Hub.
//   Assigner les références dans l'Inspector.
// ============================================================
using UnityEngine;

public class HubManager : MonoBehaviour
{
    // ================================================================
    // SINGLETON (local à la scène)
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

    [Header("Véhicules disponibles dans le parking")]
    [SerializeField] private HubVehicule[] _vehiculesDuParking;

    // ================================================================
    // ÉTAT
    // ================================================================

    private MissionDef   _missionSelectionnee;
    private VehiculeDef  _vehiculeSelectionne;

    // ================================================================
    // LIFECYCLE
    // ================================================================

    private void Start()
    {
        // Affiche l'argent du joueur dès l'arrivée au Hub
        _hubUI?.MettreAJourArgent(GameManager.Instance?.Argent ?? 0f);

        // Sélectionne le véhicule de base par défaut (index 0 si débloqué)
        SelectionnerVehiculeParDefaut();
    }

    // ================================================================
    // SÉLECTION MISSION
    // ================================================================

    public void SelectionnerMission(MissionDef mission)
    {
        _missionSelectionnee = mission;
        Debug.Log($"[HubManager] Mission sélectionnée : {mission.NomMission}");
        _hubUI?.AfficherFicheMission(mission);
    }

    // ================================================================
    // SÉLECTION VÉHICULE
    // ================================================================

    public void SelectionnerVehicule(VehiculeDef vehicule)
    {
        _vehiculeSelectionne = vehicule;
        Debug.Log($"[HubManager] Véhicule sélectionné : {vehicule.NomVehicule}");
        _hubUI?.MettreAJourVehiculeSelectionne(vehicule);
    }

    private void SelectionnerVehiculeParDefaut()
    {
        foreach (var hv in _vehiculesDuParking)
        {
            if (hv != null && hv.EstDebloque)
            {
                SelectionnerVehicule(hv.Def);
                return;
            }
        }
    }

    // ================================================================
    // DÉPART EN MISSION
    // ================================================================

    /// <summary>
    /// Appelé quand le joueur confirme le départ.
    /// Vérifie que tout est en ordre avant de lancer.
    /// </summary>
    public void DemanderDepart()
    {
        if (_missionSelectionnee == null)
        {
            _hubUI?.AfficherErreur("Aucune mission sélectionnée !");
            return;
        }
        if (_vehiculeSelectionne == null)
        {
            _hubUI?.AfficherErreur("Aucun véhicule sélectionné !");
            return;
        }

        _hubUI?.AfficherPopupConfirmationDepart(_missionSelectionnee, _vehiculeSelectionne);
    }

    /// <summary>Appelé par le popup de confirmation — le joueur a dit Oui.</summary>
    public void ConfirmerDepart()
    {
        if (_missionSelectionnee == null) return;

        // TODO : transmettre le véhicule sélectionné à MissionSystem
        // Pour l'instant on stocke dans GameManager
        Debug.Log($"[HubManager] Départ → {_missionSelectionnee.NomMission} avec {_vehiculeSelectionne?.NomVehicule}");

        GameManager.Instance?.LancerMission(_missionSelectionnee);
    }

    // ================================================================
    // PROPRIÉTÉS
    // ================================================================

    public MissionDef  MissionSelectionnee  => _missionSelectionnee;
    public VehiculeDef VehiculeSelectionne  => _vehiculeSelectionne;
    public bool        PretAPartir          => _missionSelectionnee != null && _vehiculeSelectionne != null;
}
