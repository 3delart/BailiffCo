// ============================================================
// HubVehicule.cs — Bailiff & Co
// À mettre sur chaque véhicule dans le parking du Hub.
// Le joueur s'approche de la PORTE du véhicule et appuie E
// → popup de détail avec prix de location + boutons.
//
// SETUP UNITY :
//   Prefab véhicule dans le parking :
//   ├── Root (HubVehicule.cs)
//   │   └── Mesh du véhicule
//   └── PorteInteraction (BoxCollider — Layer Interactable)
//       └── Ce collider est ce que le joueur vise avec E
//
//   Dans l'Inspector :
//   - _def          : VehiculeDef ScriptableObject
//   - _prixLocation : prix de location pour cette mission (€)
//                     0 = gratuit (vélo cargo)
//   - _colliderPorte : le BoxCollider sur la porte
//
// FONCTIONNEMENT :
//   Joueur vise la porte → label contextuel → E
//   → HubManager.DemanderLocationVehicule()
//   → HubUI.AfficherPanelVehicule() avec :
//       Nom | Prix | Capacité | Avantage | Inconvénient | Solde
//       [Louer & Partir] [Annuler]
// ============================================================
using UnityEngine;

public class HubVehicule : MonoBehaviour, IInteractable
{
    // ================================================================
    // SÉRIALISATION
    // ================================================================

    [Header("Données")]
    [SerializeField] private VehiculeDef _def;

    [Header("Location")]
    [Tooltip("Prix de location pour une mission. 0 = gratuit (vélo).")]
    [SerializeField] private float _prixLocation = 0f;

    [Header("Visuel verrouillé")]
    [Tooltip("Matériau grisé si le véhicule n'est pas disponible (optionnel)")]
    [SerializeField] private Material _materielIndisponible;
    [SerializeField] private Renderer _renderer;

    [Header("Label flottant (optionnel)")]
    [SerializeField] private TMPro.TextMeshPro _labelTexte;
    [SerializeField] private float             _hauteurLabel = 2f;

    // ================================================================
    // ÉTAT
    // ================================================================

    // Un véhicule peut être temporairement indisponible
    // (ex : déjà loué par quelqu'un d'autre en coop — futur)
    private bool _disponible = true;

    // ================================================================
    // LIFECYCLE
    // ================================================================

    private void Awake()
    {
        if (_renderer == null)
            _renderer = GetComponentInChildren<Renderer>();

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

    public bool CanInteract(GameObject interacteur) => _def != null;

    public void Interact(GameObject interacteur)
    {
        if (_def == null) return;

        if (!_disponible)
        {
            FindObjectOfType<HubUI>()?.AfficherErreur("Ce véhicule n'est pas disponible.");
            return;
        }

        // Délègue au HubManager qui vérifie le solde et affiche le panel
        HubManager.Instance?.DemanderLocationVehicule(_def, _prixLocation);
    }

    public string GetInteractionLabel()
    {
        if (_def == null) return "Véhicule";

        if (!_disponible)
            return $"{_def.NomVehicule} — Indisponible";

        float solde = GameManager.Instance?.Argent ?? 0f;
        bool  peutLouer = solde >= _prixLocation;

        string prix = _prixLocation <= 0f ? "Gratuit" : $"{_prixLocation:N0} €/mission";

        if (!peutLouer && _prixLocation > 0f)
            return $"{_def.NomVehicule} ({prix}) — [E] Voir détails  ⚠ Fonds insuffisants";

        return $"{_def.NomVehicule} ({prix}) — [E] Voir détails";
    }

    // ================================================================
    // UTILITAIRES
    // ================================================================

    private void MettreAJourLabel()
    {
        if (_labelTexte == null || _def == null) return;

        if (_labelTexte != null)
            _labelTexte.transform.localPosition = Vector3.up * _hauteurLabel;

        string prix = _prixLocation <= 0f ? "Gratuit" : $"{_prixLocation:N0} €/mission";
        _labelTexte.text = $"{_def.NomVehicule}\n<size=70%>{prix} · {_def.CapaciteObjets} objets</size>";

        _labelTexte.color = _disponible ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);

        // Applique le matériau indisponible si nécessaire
        if (_renderer != null && _materielIndisponible != null && !_disponible)
            _renderer.material = _materielIndisponible;
    }

    // ================================================================
    // API PUBLIQUE
    // ================================================================

    /// <summary>Rend le véhicule indisponible (ex: coop futur).</summary>
    public void SetDisponible(bool disponible)
    {
        _disponible = disponible;
        MettreAJourLabel();
    }

    // ================================================================
    // PROPRIÉTÉS
    // ================================================================

    public VehiculeDef Def          => _def;
    public float       PrixLocation => _prixLocation;
    public bool        Disponible   => _disponible;
    public bool        EstGratuit   => _prixLocation <= 0f;
}
