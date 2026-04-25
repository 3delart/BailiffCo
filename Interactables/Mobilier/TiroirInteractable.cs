// ============================================================
// TiroirInteractable.cs — Bailiff & Co
// Tiroir qui s'ouvre/ferme en translation sur son axe local.
// Contient des ObjetValeur placés comme enfants en scène.
//
// HIÉRARCHIE DANS LA SCÈNE :
//   Tiroir (ce script + BoxCollider IsTrigger sur Layer Interactable)
//   └── ZoneTiroir (ZoneTiroirTrigger + BoxCollider IsTrigger)
//   └── ObjetValeur_01  ← placé ici comme enfant
//   └── ObjetValeur_02  ← placé ici comme enfant
//
// WORKFLOW :
//   1. Placer les ObjetValeur comme enfants du Tiroir dans la scène.
//   2. Leurs Rigidbody.isKinematic doit être TRUE au départ
//      (réglé dans ObjetValeur.Awake ou via l'Inspector).
//   3. À l'ouverture : TiroirInteractable libère les Rigidbody
//      et active la zone trigger → les objets sont saisissables.
//   4. À la fermeture : si l'objet est encore là (pas pris),
//      on le re-kinematise et il suit le tiroir.
//   5. Si l'objet a été pris par le joueur, il n'est plus enfant
//      → le tiroir se referme normalement sans lui.
// ============================================================
using System.Collections.Generic;
using UnityEngine;

public class TiroirInteractable : MonoBehaviour, IInteractable
{
    [Header("Paramètres")]
    [SerializeField] private float _distanceOuverture = 0.4f;  // mètres
    [SerializeField] private float _vitesseGlissement = 3f;
    [SerializeField] private float _chanceGrincement  = 0.35f; // 35% de grincer

    [Header("Bruit")]
    [SerializeField] private float _porteeOuverture   = 4f;
    [SerializeField] private float _porteeGrincement  = 7f;

    // ── Privés ───────────────────────────────────────────────
    private bool    _estOuvert    = false;
    private bool    _enMouvement  = false;
    private Vector3 _positionFermee;
    private Vector3 _positionOuverte;
    private Vector3 _cible;

    private PlayerNoiseEmitter  _noise;
    private ZoneTiroirTrigger   _zoneTrigger;

    // Cache des objets présents dans le tiroir à l'ouverture
    private readonly List<ObjetValeur> _objetsInitiaux = new();

    // ================================================================
    // LIFECYCLE
    // ================================================================

    private void Awake()
    {
        _positionFermee  = transform.localPosition;
        // Le tiroir s'ouvre vers l'avant local (Z+)
        _positionOuverte = _positionFermee
                         + transform.localRotation * Vector3.forward * _distanceOuverture;
        _cible = _positionFermee;

        _zoneTrigger = GetComponentInChildren<ZoneTiroirTrigger>();

        // Inventorie les ObjetValeur déjà présents comme enfants
        // et s'assure qu'ils sont bien kinematiques au départ
        foreach (var ov in GetComponentsInChildren<ObjetValeur>())
        {
            _objetsInitiaux.Add(ov);
            var rb = ov.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity  = false;
            }
        }
    }

    private void Update()
    {
        if (!_enMouvement) return;

        transform.localPosition = Vector3.MoveTowards(
            transform.localPosition, _cible, _vitesseGlissement * Time.deltaTime);

        if (Vector3.Distance(transform.localPosition, _cible) < 0.001f)
        {
            transform.localPosition = _cible;
            _enMouvement = false;

            // Fin d'animation : appliquer les effets selon l'état
            if (_estOuvert)
                OnOuvertureFinie();
            else
                OnFermetureFinie();
        }
    }

    // ================================================================
    // CALLBACKS FIN D'ANIMATION
    // ================================================================

    /// <summary>Le tiroir vient de finir de s'ouvrir.</summary>
    private void OnOuvertureFinie()
    {
        // Active le trigger visuel/futur dépôt
        _zoneTrigger?.Activer();

        // Libère les Rigidbody des objets encore présents
        foreach (var ov in GetObjetsPresents())
        {
            var rb = ov.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity  = true;
            }
        }
    }

    /// <summary>Le tiroir vient de finir de se fermer.</summary>
    private void OnFermetureFinie()
    {
        _zoneTrigger?.Desactiver();
        // Les objets restants (non pris) sont déjà enfants → ils ont suivi le tiroir.
        // On s'assure qu'ils sont bien kinematiques pour ne pas tomber au prochain mouvement.
        foreach (var ov in GetObjetsPresents())
        {
            var rb = ov.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity  = false;
                rb.velocity    = Vector3.zero;
            }
        }
    }

    /// <summary>
    /// Retourne uniquement les ObjetValeur encore enfants de ce tiroir
    /// (les objets pris par le joueur ont été dé-parentés par PlayerCarry).
    /// </summary>
    private List<ObjetValeur> GetObjetsPresents()
    {
        var liste = new List<ObjetValeur>();
        foreach (var ov in _objetsInitiaux)
        {
            // L'objet est encore là si son parent direct ou indirect est ce tiroir
            if (ov != null && ov.transform.IsChildOf(transform))
                liste.Add(ov);
        }
        return liste;
    }

    // ================================================================
    // IInteractable
    // ================================================================

    public bool CanInteract(GameObject joueur) => !_enMouvement;

    public void Interact(GameObject joueur)
    {
        _estOuvert   = !_estOuvert;
        _cible       = _estOuvert ? _positionOuverte : _positionFermee;
        _enMouvement = true;

        // Si on ferme alors qu'un objet est libre, on le re-kinematise immédiatement
        // pour qu'il suive le tiroir sans glisser
        if (!_estOuvert)
        {
            _zoneTrigger?.Desactiver();
            foreach (var ov in GetObjetsPresents())
            {
                var rb = ov.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                    rb.useGravity  = false;
                    rb.velocity    = Vector3.zero;
                }
            }
        }

        // Récupère le NoiseEmitter du joueur
        if (_noise == null)
            _noise = joueur.GetComponent<PlayerNoiseEmitter>();

        // Grincement aléatoire
        bool grince   = Random.value < _chanceGrincement;
        float portee  = grince ? _porteeGrincement : _porteeOuverture;
        NiveauBruit niveau = grince ? NiveauBruit.Leger : NiveauBruit.Silencieux;

        if (grince)
            _noise?.EmettreBruit(niveau, portee);
    }

    public string GetInteractionLabel()
    {
        if (_enMouvement) return "...";

        int nb = GetObjetsPresents().Count;
        if (_estOuvert && nb > 0)
            return $"Fermer le tiroir ({nb} objet{(nb > 1 ? "s" : "")})";
        return _estOuvert ? "Fermer le tiroir" : "Ouvrir le tiroir";
    }

    // ================================================================
    // PROPRIÉTÉS
    // ================================================================
    public bool EstOuvert    => _estOuvert;
    public int  NbObjets     => GetObjetsPresents().Count;
}
