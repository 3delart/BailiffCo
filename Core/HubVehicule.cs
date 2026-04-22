// ============================================================
// HubVehicule.cs — Bailiff & Co
// À mettre sur chaque véhicule dans le parking du Hub.
// Véhicule débloqué → sélectionnable.
// Véhicule verrouillé → affiche le prix/condition de déblocage.
//
// SETUP UNITY :
//   Prefab véhicule dans le parking :
//   ├── Mesh du véhicule (MeshRenderer)
//   ├── Collider (Layer Interactable)
//   ├── HubVehicule.cs
//   └── LabelCanvas (World Space) → TextMeshPro label
// ============================================================
using TMPro;
using UnityEngine;

public class HubVehicule : MonoBehaviour, IInteractable
{
    // ================================================================
    // SÉRIALISATION
    // ================================================================

    [Header("Données")]
    [SerializeField] private VehiculeDef _def;
    [SerializeField] private bool        _debloque = false;

    [Header("Label flottant")]
    [SerializeField] private TextMeshPro _labelTexte;
    [SerializeField] private float       _hauteurLabel = 1.5f;

    [Header("Visuel verrouillé")]
    [Tooltip("Matériau grisé appliqué si le véhicule n'est pas débloqué")]
    [SerializeField] private Material _materielVerrouille;
    [SerializeField] private Renderer _renderer;

    // ================================================================
    // LIFECYCLE
    // ================================================================

    private void Awake()
    {
        if (_renderer == null)
            _renderer = GetComponentInChildren<Renderer>();

        AppliquerVisuels();
        MettreAJourLabel();
    }

    private void Update()
    {
        // Billboard label
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

    public bool CanInteract(GameObject interacteur) => true; // toujours — pour afficher le label

    public void Interact(GameObject interacteur)
    {
        if (_def == null) return;

        if (_debloque)
        {
            HubManager.Instance?.SelectionnerVehicule(_def);
            Debug.Log($"[HubVehicule] {_def.NomVehicule} sélectionné.");
        }
        else
        {
            // Affiche condition de déblocage
            string condition = string.IsNullOrEmpty(_def.ConditionSpeciale)
                ? $"Mission {_def.NumeroMissionRequis} requise"
                : _def.ConditionSpeciale;

            FindObjectOfType<HubUI>()?.AfficherErreur($"Véhicule verrouillé — {condition}");
            Debug.Log($"[HubVehicule] {_def.NomVehicule} verrouillé : {condition}");
        }
    }

    public string GetInteractionLabel()
    {
        if (_def == null) return "Véhicule";

        if (_debloque)
        {
            bool estSelectionne = HubManager.Instance?.VehiculeSelectionne == _def;
            return estSelectionne
                ? $"✓ {_def.NomVehicule} (sélectionné)"
                : $"{_def.NomVehicule} — [E] Sélectionner";
        }
        else
        {
            string condition = string.IsNullOrEmpty(_def.ConditionSpeciale)
                ? $"Mission {_def.NumeroMissionRequis}"
                : _def.ConditionSpeciale;
            return $"🔒 {_def.NomVehicule} — {condition}";
        }
    }

    // ================================================================
    // VISUELS
    // ================================================================

    private void AppliquerVisuels()
    {
        if (_renderer == null || _materielVerrouille == null) return;
        if (!_debloque)
            _renderer.material = _materielVerrouille;
    }

    private void MettreAJourLabel()
    {
        if (_labelTexte == null || _def == null) return;

        if (_labelTexte != null)
            _labelTexte.transform.localPosition = Vector3.up * _hauteurLabel;

        _labelTexte.text = _debloque
            ? $"{_def.NomVehicule}\n<size=70%>Capacité : {_def.CapaciteObjets} objets</size>"
            : $"🔒 {_def.NomVehicule}\n<size=70%>Verrouillé</size>";
    }

    // ================================================================
    // API PUBLIQUE
    // ================================================================

    /// <summary>Appelé par SaveSystem au chargement si le véhicule a été débloqué.</summary>
    public void Debloquer()
    {
        _debloque = true;
        AppliquerVisuels();
        MettreAJourLabel();
    }

    public VehiculeDef Def        => _def;
    public bool        EstDebloque => _debloque;
}
