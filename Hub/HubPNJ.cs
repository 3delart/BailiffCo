// ============================================================
// HubPNJ.cs — Bailiff & Co
// À mettre sur chaque capsule PNJ de l'agence.
// Implémente IInteractable — le joueur appuie E pour interagir.
// Affiche un label flottant au-dessus de la tête du PNJ.
//
// SETUP UNITY :
//   1. Capsule PNJ (MeshRenderer + Collider sur Layer Interactable)
//      ├── HubPNJ.cs (ce script)
//      └── LabelCanvas (World Space Canvas)
//          └── LabelTexte (TextMeshPro)
//
//   2. Dans l'Inspector :
//      - _nomPnj       : "Chef", "Secrétaire", "Mécanicien"…
//      - _actionLabel  : "Parler", "Boutique", "Garage"…
//      - _typePanneau  : quel panneau ouvrir
// ============================================================
using TMPro;
using UnityEngine;

public class HubPNJ : MonoBehaviour, IInteractable
{
    // ================================================================
    // TYPES
    // ================================================================

    public enum TypePanneau
    {
        Missions,       // Bureau du Boss → choisir mission
        Boutique,       // Secrétaire → acheter/upgrader outils
        Inventaire,     // Table → sélectionner équipement
        Garage,         // Mécanicien → gérer véhicules (V2)
    }

    // ================================================================
    // SÉRIALISATION
    // ================================================================

    [Header("Identité")]
    [SerializeField] private string      _nomPnj      = "Chef";
    [SerializeField] private string      _actionLabel = "Parler";
    [SerializeField] private TypePanneau _typePanneau = TypePanneau.Missions;

    [Header("Label flottant")]
    [Tooltip("TextMeshPro dans un Canvas World Space enfant de ce GameObject")]
    [SerializeField] private TextMeshPro _labelTexte;
    [SerializeField] private float       _hauteurLabel = 1.2f; // au-dessus de la capsule

    [Header("Référence HubUI")]
    [SerializeField] private HubUI _hubUI;

    // ================================================================
    // LIFECYCLE
    // ================================================================

    private void Awake()
    {
        // Positionne le label au-dessus de la tête
        if (_labelTexte != null)
        {
            _labelTexte.transform.localPosition = Vector3.up * _hauteurLabel;
            MettreAJourLabel();
        }

        // Auto-trouve HubUI si pas assigné
        if (_hubUI == null)
            _hubUI = FindObjectOfType<HubUI>();
    }

    private void Update()
    {
        // Le label fait face à la caméra (billboard)
        if (_labelTexte != null && Camera.main != null)
        {
            _labelTexte.transform.LookAt(
                _labelTexte.transform.position + Camera.main.transform.rotation * Vector3.forward,
                Camera.main.transform.rotation * Vector3.up
            );
        }
    }

    // ================================================================
    // IINTERACTABLE
    // ================================================================

    public bool CanInteract(GameObject interacteur) => true;

    public void Interact(GameObject interacteur)
    {
        if (_hubUI == null)
        {
            Debug.LogWarning($"[HubPNJ] {_nomPnj} : HubUI non trouvé !");
            return;
        }

        switch (_typePanneau)
        {
            case TypePanneau.Missions:   _hubUI.OuvrirPanneauMissions();   break;
            case TypePanneau.Boutique:   _hubUI.OuvrirPanneauBoutique();   break;
            case TypePanneau.Inventaire: _hubUI.OuvrirPanneauInventaire(); break;
            case TypePanneau.Garage:     _hubUI.OuvrirPanneauGarage();     break;
        }
    }

    public string GetInteractionLabel()
        => $"{_nomPnj} — [E] {_actionLabel}";

    // ================================================================
    // UTILITAIRES
    // ================================================================

    private void MettreAJourLabel()
    {
        if (_labelTexte != null)
            _labelTexte.text = $"{_nomPnj}\n<size=70%>[E] {_actionLabel}</size>";
    }
}
