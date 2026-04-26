// ============================================================
// HubManager.cs — Bailiff & Co
// Orchestrateur du Hub. Source de vérité locale pour la session.
// Gère : sélection mission, location véhicule, départ.
//
// SETUP UNITY :
//   GameObject "HubManager" dans la scène Hub.
//   Assigner les références dans l'Inspector.
//   Le HubManager ne persiste PAS entre les scènes —
//   c'est GameManager (DontDestroyOnLoad) qui transporte
//   la MissionDef vers la scène Mission.
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

        // Affiche l'argent du joueur dès l'arrivée
        _hubUI?.MettreAJourArgent(GameManager.Instance?.Argent ?? 0f);

        // Déverrouille le curseur — on est dans le Hub
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    // ================================================================
    // SÉLECTION MISSION — appelé par HubPNJ (Chef)
    // ================================================================

    public void SelectionnerMission(MissionDef mission)
    {
        if (mission == null) return;
        _missionSelectionnee = mission;
        Debug.Log($"[HubManager] Mission sélectionnée : {mission.NomMission}");
        _hubUI?.AfficherFicheMission(mission);
    }

    // ================================================================
    // LOCATION VÉHICULE — appelé par HubVehicule
    // ================================================================

    /// <summary>
    /// Appelé quand le joueur interagit avec la porte d'un véhicule.
    /// Affiche le panel de détail + boutons Louer / Annuler.
    /// </summary>
    public void DemanderLocationVehicule(VehiculeDef vehicule, float prixLocation)
    {
        if (vehicule == null) return;

        _vehiculeSelectionne  = vehicule;
        _prixLocationVehicule = prixLocation;

        _hubUI?.AfficherPanelVehicule(vehicule, prixLocation);
    }

    /// <summary>
    /// Appelé par HubUI bouton "Louer & Partir".
    /// Vérifie le solde, déduit, lance la mission.
    /// </summary>
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

        // Déduit la location
        GameManager.Instance?.Debiter(_prixLocationVehicule);
        Debug.Log($"[HubManager] Location {_vehiculeSelectionne.NomVehicule} " +
                  $"({_prixLocationVehicule:N0} €) — Solde restant : " +
                  $"{GameManager.Instance?.Argent:N0} €");

        // Lance la mission via GameManager
        GameManager.Instance?.LancerMission(_missionSelectionnee);
    }

    /// <summary>Appelé par HubUI bouton "Annuler" sur le panel véhicule.</summary>
    public void AnnulerLocationVehicule()
    {
        _vehiculeSelectionne  = null;
        _prixLocationVehicule = 0f;
        _hubUI?.FermerPanelVehicule();
    }

    // ================================================================
    // PROPRIÉTÉS PUBLIQUES
    // ================================================================

    public MissionDef  MissionSelectionnee  => _missionSelectionnee;
    public VehiculeDef VehiculeSelectionne  => _vehiculeSelectionne;
    public float       PrixLocation         => _prixLocationVehicule;
    public bool        MissionChoisie       => _missionSelectionnee != null;
    public bool        VehiculeChoisi       => _vehiculeSelectionne != null;

    // ================================================================
    // UTILITAIRES — appelés par HubUI
    // ================================================================

    public void MettreAJourAffichageArgent()
        => _hubUI?.MettreAJourArgent(GameManager.Instance?.Argent ?? 0f);
}
