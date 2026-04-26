// ============================================================
// MeubleInteractable.cs — Bailiff & Co
// Le meuble suit exactement le delta joueur.
// Sa masse réduit la vitesse du joueur via _multiplicateurVitesse.
// ============================================================
using UnityEngine;

public class MeubleInteractable : MonoBehaviour, IInteractable
{
    [Header("Résistance")]
    [Tooltip("Masse simulée du meuble en kg — réduit la vitesse du joueur")]
    [SerializeField] private float _masseKg         = 30f;
    [Tooltip("Vitesse joueur minimale même pour un meuble très lourd (0-1)")]
    [SerializeField] private float _multiplicateurMin = 0.25f;

    [Header("Bruit selon sol")]
    [SerializeField] private float _porteeParquet   = 10f;
    [SerializeField] private float _porteeMoquette  = 4f;
    [SerializeField] private float _porteeCarrelage = 12f;
    [SerializeField] private float _porteeDefaut    = 8f;
    [SerializeField] private float _intervalBruit   = 0.3f;

    // ── Privés ───────────────────────────────────────────────
    private bool               _estSaisi     = false;
    private GameObject         _joueur;
    private PlayerNoiseEmitter _noise;
    private Rigidbody          _rb;
    private float              _dernierBruit  = 0f;
    private Vector3            _posJoueurPrecedente;

    // Multiplicateur exposé au PlayerController
    // Formule : 1 / (1 + masse/20) → 10kg=0.67, 30kg=0.4, 60kg=0.25
    public float MultiplicateurVitesse => _estSaisi
        ? Mathf.Max(_multiplicateurMin, 1f / (1f + _masseKg / 20f))
        : 1f;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        if (_rb != null)
        {
            _rb.isKinematic = true;
            _rb.constraints = RigidbodyConstraints.FreezeRotation
                            | RigidbodyConstraints.FreezePositionY;
        }
    }

    private void Update()
    {
        if (!_estSaisi || _joueur == null) return;

        Vector3 posActuelle = _joueur.transform.position;
        Vector3 delta       = posActuelle - _posJoueurPrecedente;
        delta.y             = 0f;

        if (delta.sqrMagnitude > 0.00001f)
        {
            transform.position += delta;
            EmettreBruitGlissement();
        }

        _posJoueurPrecedente = posActuelle;
    }

    // ================================================================
    // BRUIT
    // ================================================================

    private void EmettreBruitGlissement()
    {
        if (_noise == null) return;
        if (Time.time - _dernierBruit < _intervalBruit) return;
        _dernierBruit = Time.time;

        string tagSol = DetecterTagSol();
        float portee = tagSol switch
        {
            "Parquet"   => _porteeParquet,
            "Moquette"  => _porteeMoquette,
            "Carrelage" => _porteeCarrelage,
            _           => _porteeDefaut
        };
        _noise.EmettreBruit(NiveauBruit.Fort, portee);
    }

    private string DetecterTagSol()
    {
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f,
            Vector3.down, out RaycastHit hit, 0.5f,
            Physics.AllLayers, QueryTriggerInteraction.Ignore))
            return hit.collider.tag;
        return "";
    }

    // ================================================================
    // IInteractable
    // ================================================================

    public bool CanInteract(GameObject joueur) => true;

    public void Interact(GameObject joueur)
    {
        if (!_estSaisi) CommencerPousse(joueur);
    }

    public void CommencerPousse(GameObject joueur)
    {
        _estSaisi            = true;
        _joueur              = joueur;
        _noise               = joueur.GetComponent<PlayerNoiseEmitter>();
        _posJoueurPrecedente = joueur.transform.position;
        if (_rb != null) _rb.isKinematic = true;
    }

    public void StopperPousse()
    {
        _estSaisi = false;
        _joueur   = null;
    }

    public string GetInteractionLabel()
        => _estSaisi ? "Relâcher E pour lâcher" : "[E maintenu] Pousser / Tirer";

    public bool EstSaisi => _estSaisi;
}
