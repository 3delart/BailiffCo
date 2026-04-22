// ============================================================
// ObjetValeur.cs — Bailiff & Co
// IInteractable : saisir, scanner, dégrader.
// ============================================================
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ObjetValeur : MonoBehaviour, IInteractable
{
    [Header("Données")]
    [SerializeField] private ObjetDef _def;
    [SerializeField] private float    _valeurReelle;
    [SerializeField] private bool     _scanne = false;

    private Rigidbody   _rb;
    private PlayerCarry _carry;
    private float       _vitesseImpact;

    public void Initialiser(ObjetDef def, float valeur)
    {
        _def          = def;
        _valeurReelle = valeur;
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        // Si pas initialisé par MissionSystem, tire une valeur aléatoire depuis le def
        if (_valeurReelle == 0f && _def != null)
            _valeurReelle = Random.Range(_def.ValeurMin, _def.ValeurMax);
    }

    // ================================================================
    // IINTERACTABLE
    // ================================================================

    public bool CanInteract(GameObject interacteur) => _carry == null;

    public void Interact(GameObject interacteur)
    {
        if (interacteur.TryGetComponent<PlayerCarry>(out var carry))
        {
            carry.Saisir(this);
            _carry = carry;
        }
    }

    public string GetInteractionLabel()
    {
        if (!_scanne) return $"Saisir ({(_def != null ? _def.NomObjet : "Objet")})";
        return $"Saisir — {_valeurReelle:N0} €";
    }

    // ================================================================
    // SCAN
    // ================================================================

    public void Scanner()
    {
        _scanne = true;
    }

    // ================================================================
    // DÉGRADATION
    // ================================================================

    private void OnCollisionEnter(Collision col)
    {
        // Ne pas traiter les collisions quand l'objet est porté
        if (_carry != null) return;

        _vitesseImpact = col.relativeVelocity.magnitude;
        if (_def == null || !_def.EstFragile) return;
        if (_vitesseImpact < 2f) return;

        float perte = _vitesseImpact > 4f ? _valeurReelle * 0.8f : _valeurReelle * 0.5f;
        _valeurReelle -= perte;
        _valeurReelle  = Mathf.Max(0f, _valeurReelle);

        EventBus<OnObjetEndommage>.Raise(new OnObjetEndommage
        {
            Objet        = _def,
            ValeurPerdue = perte,
            Position     = transform.position
        });

        if (_vitesseImpact > 6f)
        {
            EventBus<OnBruitEmis>.Raise(new OnBruitEmis
            {
                Position = transform.position,
                Portee   = 8f,
                Niveau   = NiveauBruit.Fort
            });
        }
    }

    // ================================================================
    // CHARGEMENT DANS LE VÉHICULE
    // ================================================================

    public void ChargerDansVehicule()
    {
        EventBus<OnObjetCharge>.Raise(new OnObjetCharge
        {
            Objet      = _def,
            Valeur     = _valeurReelle,
            EstFragile = _def?.EstFragile ?? false
        });
        Destroy(gameObject);
    }

    // ================================================================
    // PROPRIÉTÉS
    // ================================================================

    public ObjetDef Def          => _def;
    public float    ValeurReelle => _valeurReelle;
    public bool     EstScanne    => _scanne;
    public void     LiberPorteur() => _carry = null;
}