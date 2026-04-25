// ============================================================
// PlayerInteractor.cs — Bailiff & Co
// Raycast vers IInteractable, affiche le label contextuel,
// déclenche Interact() sur pression de E.
// Gère aussi le E maintenu pour MeublePousse.
// Détecte quel collider enfant est visé (ex: portes du véhicule).
// ============================================================
using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private float     _porteeInteraction = 2.5f;
    [SerializeField] private LayerMask _layerInteractable;
    [SerializeField] private Transform _camera;

    private IInteractable _cibleCourante;
    private Collider      _colliderVise;

    // Référence au meuble en cours de pousse
    private MeublePousse  _meublePousse;

    private void Update()
    {
        DetecterCible();
        GererInteraction();
        GererPousse();
    }

    // ================================================================
    // DÉTECTION CIBLE
    // ================================================================

    private void DetecterCible()
    {
        // Si on pousse un meuble, pas besoin de chercher une autre cible
        if (_meublePousse != null) return;

        Transform origine = _camera != null ? _camera : transform;

        if (Physics.Raycast(origine.position, origine.forward,
            out RaycastHit hit, _porteeInteraction, _layerInteractable))
        {
            _colliderVise = hit.collider;

            var interactable = hit.collider.GetComponentInParent<IInteractable>();
            if (interactable != null && interactable.CanInteract(gameObject))
            {
                _cibleCourante = interactable;
                return;
            }
        }

        _cibleCourante = null;
        _colliderVise  = null;
    }

    // ================================================================
    // INTERACTION NORMALE (E pressé)
    // ================================================================

    private void GererInteraction()
    {
        if (_meublePousse != null) return;
    
        KeyCode toucheInteragir = OptionsManager.Instance != null
            ? OptionsManager.Instance.GetTouche(ActionJeu.Interagir)
            : KeyCode.E;
    
        if (_cibleCourante != null && Input.GetKeyDown(toucheInteragir))
        {
            if (_cibleCourante.CanInteract(gameObject))
            {
                if (_cibleCourante is Vehicule vehicule)
                    vehicule.SetColliderVise(_colliderVise);
    
                if (_cibleCourante is MeublePousse meuble)
                {
                    _meublePousse = meuble;
                    _meublePousse.CommencerPousse(gameObject);
                    return;
                }
    
                _cibleCourante.Interact(gameObject);
            }
        }
    }


    // ================================================================
    // POUSSE MEUBLE (E maintenu → continue, E relâché → stop)
    // ================================================================

    private void GererPousse()
    {
        if (_meublePousse == null) return;
    
        KeyCode toucheInteragir = OptionsManager.Instance != null
            ? OptionsManager.Instance.GetTouche(ActionJeu.Interagir)
            : KeyCode.E;
    
        if (Input.GetKeyUp(toucheInteragir))
        {
            _meublePousse.StopperPousse();
            _meublePousse = null;
        }
    }


    // ================================================================
    // LABEL HUD
    // ================================================================

    /// <summary>Retourné au PlayerController pour réduire la vitesse du joueur.</summary>
    public float MultiplicateurVitesseMeuble
        => _meublePousse != null ? _meublePousse.MultiplicateurVitesse : 1f;

    public string GetLabelCourant()
    {
        // Label spécial pendant la pousse
        if (_meublePousse != null)
            return _meublePousse.GetInteractionLabel();

        if (_cibleCourante == null) return string.Empty;

        if (_cibleCourante is Vehicule vehicule)
            vehicule.SetColliderVise(_colliderVise);

        return _cibleCourante.GetInteractionLabel();
    }
}
