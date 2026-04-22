// ============================================================
// OuvrableInteractable.cs — Bailiff & Co
// Remplace Porte.cs. Gère tout objet ouvrable/fermable :
//   • Porte          → Mode Rotation   (axe Y, 90°)
//   • Fenêtre à guillotine → Mode TranslationVerticale
//   • Fenêtre coulissante  → Mode TranslationHorizontale
//
// Configurer dans l'Inspector :
//   _modeOuverture   → choisir le mode
//   _nomObjet        → "porte", "fenêtre", "volet"…  (pour les labels)
//   _angleOuverture  → (Rotation uniquement) angle en degrés
//   _axeRotation     → (Rotation uniquement) Vector3.up par défaut
//   _distanceDeplacement → (Translation uniquement) distance en mètres
//   _axeDeplacement  → (Translation uniquement) Vector3.up = vertical, right = horizontal
// ============================================================
using System.Collections;
using UnityEngine;

public class OuvrableInteractable : MonoBehaviour, IInteractable
{
    // ================================================================
    // ENUMS
    // ================================================================

    public enum ModeOuverture
    {
        Rotation,                // porte classique sur charnière
        TranslationVerticale,    // fenêtre guillotine (monte/descend)
        TranslationHorizontale   // fenêtre coulissante (glisse sur le côté)
    }

    public enum EtatOuvrable { Ferme, Ouvert, Verrouille, Bloque }

    // ================================================================
    // SÉRIALISATION
    // ================================================================

    [Header("Identité")]
    [Tooltip("Utilisé dans les labels : 'Ouvrir la [fenêtre]', 'Forcer la [porte]'…")]
    [SerializeField] private string _nomObjet = "porte";

    [Header("Mode & État")]
    [SerializeField] private ModeOuverture _mode   = ModeOuverture.Rotation;
    [SerializeField] private EtatOuvrable  _etat   = EtatOuvrable.Ferme;
    [SerializeField] private bool          _grince = false;

    [Header("Rotation (porte classique)")]
    [Tooltip("Axe de rotation en espace local. Vector3.up = axe Y (porte standard).")]
    [SerializeField] private Vector3 _axeRotation     = Vector3.up;
    [SerializeField] private float   _angleOuverture  = 90f;
    [SerializeField] private float   _dureeAnimation  = 0.4f;

    [Header("Translation (fenêtre)")]
    [Tooltip("Axe de déplacement en espace LOCAL.\nVector3.up = monte (guillotine vertical).\nVector3.right = glisse (coulissant horizontal).")]
    [SerializeField] private Vector3 _axeDeplacement       = Vector3.up;
    [SerializeField] private float   _distanceDeplacement  = 0.6f;
    [SerializeField] private float   _vitesseDeplacement   = 3f;

    [Header("Bruit")]
    [SerializeField] private float _porteeOuverture = 6f;
    [SerializeField] private float _porteeForce     = 20f;

    // ================================================================
    // ÉTAT PRIVÉ
    // ================================================================

    private bool       _enMouvement      = false;
    private Quaternion _rotationFermee;       // mémorisée en Awake
    private Vector3    _positionFermee;       // mémorisée en Awake
    private Vector3    _positionOuverte;

    // Pour les coroutines de translation
    private Vector3    _cibleTranslation;
    private bool       _translationActive = false;

    // ================================================================
    // LIFECYCLE
    // ================================================================

    private void Awake()
    {
        _rotationFermee  = transform.localRotation;
        _positionFermee  = transform.localPosition;
        _positionOuverte = _positionFermee
                         + transform.localRotation * _axeDeplacement.normalized * _distanceDeplacement;
        _cibleTranslation = _positionFermee;
    }

    private void Update()
    {
        if (!_translationActive) return;

        transform.localPosition = Vector3.MoveTowards(
            transform.localPosition, _cibleTranslation, _vitesseDeplacement * Time.deltaTime);

        if (Vector3.Distance(transform.localPosition, _cibleTranslation) < 0.001f)
        {
            transform.localPosition = _cibleTranslation;
            _translationActive = false;
            _enMouvement       = false;
        }
    }

    // ================================================================
    // IINTERACTABLE
    // ================================================================

    public bool CanInteract(GameObject interacteur) =>
        _etat != EtatOuvrable.Bloque && !_enMouvement;

    public void Interact(GameObject interacteur)
    {
        switch (_etat)
        {
            case EtatOuvrable.Ferme:      Ouvrir();                     break;
            case EtatOuvrable.Ouvert:     Fermer();                     break;
            case EtatOuvrable.Verrouille: TenterForcer(interacteur);    break;
        }
    }

    public string GetInteractionLabel()
    {
        string nom = char.ToUpper(_nomObjet[0]) + _nomObjet.Substring(1); // "Porte" / "Fenêtre"
        return _etat switch
        {
            EtatOuvrable.Ferme      => $"Ouvrir la {_nomObjet}",
            EtatOuvrable.Ouvert     => $"Fermer la {_nomObjet}",
            EtatOuvrable.Verrouille => $"Forcer la {_nomObjet} (pied-de-biche) / Crocheter",
            EtatOuvrable.Bloque     => $"{nom} bloquée",
            _                       => nom
        };
    }

    // ================================================================
    // ACTIONS
    // ================================================================

    private void Ouvrir()
    {
        _etat = EtatOuvrable.Ouvert;
        LancerAnimation(ouvrir: true);

        if (_grince && Random.value < 0.4f)
            EmettreBruit(_porteeOuverture, NiveauBruit.Leger);
    }

    private void Fermer()
    {
        _etat = EtatOuvrable.Ferme;
        LancerAnimation(ouvrir: false);
    }

    private void TenterForcer(GameObject interacteur)
    {
        var inv = interacteur.GetComponent<InventaireSystem>();
        if (inv != null && inv.PossedePiedDeBiche())
            ForceOuvrir();
    }

    public void ForceOuvrir()
    {
        _etat = EtatOuvrable.Ouvert;
        LancerAnimation(ouvrir: true);
        EmettreBruit(_porteeForce, NiveauBruit.Tresfort);
    }

    // ================================================================
    // DISPATCH ANIMATION selon le mode
    // ================================================================

    private void LancerAnimation(bool ouvrir)
    {
        if (_enMouvement) return;
        _enMouvement = true;

        switch (_mode)
        {
            case ModeOuverture.Rotation:
                StartCoroutine(AnimerRotation(ouvrir));
                break;

            case ModeOuverture.TranslationVerticale:
            case ModeOuverture.TranslationHorizontale:
                _cibleTranslation  = ouvrir ? _positionOuverte : _positionFermee;
                _translationActive = true;
                // _enMouvement sera remis à false dans Update quand on arrive à la cible
                break;
        }
    }

    // ================================================================
    // COROUTINE ROTATION
    // ================================================================

    private IEnumerator AnimerRotation(bool ouvrir)
    {
        Quaternion debut = transform.localRotation;
        Quaternion fin   = ouvrir
            ? _rotationFermee * Quaternion.AngleAxis(_angleOuverture, _axeRotation)
            : _rotationFermee;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / _dureeAnimation;
            transform.localRotation = Quaternion.Lerp(debut, fin, Mathf.Clamp01(t));
            yield return null;
        }
        transform.localRotation = fin;
        _enMouvement = false;
    }

    // ================================================================
    // BRUIT
    // ================================================================

    private void EmettreBruit(float portee, NiveauBruit niveau)
    {
        EventBus<OnBruitEmis>.Raise(new OnBruitEmis
        {
            Position = transform.position,
            Portee   = portee,
            Niveau   = niveau
        });
    }

    // ================================================================
    // API PUBLIQUE
    // ================================================================

    public void Bloquer()    => _etat = EtatOuvrable.Bloque;
    public void Debloquer()  => _etat = EtatOuvrable.Ferme;
    public void Verrouiller()=> _etat = EtatOuvrable.Verrouille;

    public bool EstOuvert   => _etat == EtatOuvrable.Ouvert;
    public bool EstVerrouille => _etat == EtatOuvrable.Verrouille;
    public EtatOuvrable Etat => _etat;
}
