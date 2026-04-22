// ============================================================
// ProprietaireAI.cs — Bailiff & Co
// State machine principale du proprio (8 états selon GDD §4.3).
// Ne frappe JAMAIS le joueur. La tension est sociale et juridique.
// Émet des events, ne modifie rien directement dans les autres systèmes.
// ============================================================
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ProprietaireAI : MonoBehaviour
{
    // ================================================================
    // RÉFÉRENCES
    // ================================================================

    [Header("Références")]
    [SerializeField] private ProprietaireDef _def;
    [SerializeField] private Transform       _joueur;
    [SerializeField] private Transform       _vehicule;
    [SerializeField] private Animator        _animator;

    [Header("Détection")]
    [SerializeField] private float _porteeVision   = 8f;
    [SerializeField] private float _porteeOuie     = 5f;
    [SerializeField] private float _angleVision    = 90f;  // demi-angle en degrés

    [Header("Routine (V1 simplifié)")]
    [SerializeField] private Transform[] _pointsPatrouille;

    // ================================================================
    // ÉTAT
    // ================================================================

    [Header("État (lecture seule)")]
    [SerializeField] private ProprietaireState _etatCourant = ProprietaireState.Idle;
    [SerializeField] private float _paranoiaCourante = 0f;
    [SerializeField] private int   _palierCourant    = 0;

    private NavMeshAgent   _agent;
    private ParanoiaSystem _paranoiaSys;
    private int            _indexPatrouille = 0;
    private Vector3        _dernierBruitPos;
    private bool           _locked = false;
    private Coroutine      _stateCoroutine;

    // ================================================================
    // LIFECYCLE
    // ================================================================

    private void Awake()
    {
        _agent     = GetComponent<NavMeshAgent>();
        _paranoiaSys = FindObjectOfType<ParanoiaSystem>();
    }

    private void Start()
    {
        if (_def != null)
        {
            _agent.speed = _def.VitesseDeplacementNormal;
        }
        EntrerEtat(ProprietaireState.Idle);
    }

    private void OnEnable()
    {
        EventBus<OnParanoiaChanged>.Subscribe(OnParanoiaChanged);
        EventBus<OnBruitEmis>.Subscribe(OnBruitEmis);
        EventBus<OnSeuilAtteint>.Subscribe(OnSeuilAtteint);
        EventBus<OnMissionDemarree>.Subscribe(OnMissionDemarree);
    }

    private void OnDisable()
    {
        EventBus<OnParanoiaChanged>.Unsubscribe(OnParanoiaChanged);
        EventBus<OnBruitEmis>.Unsubscribe(OnBruitEmis);
        EventBus<OnSeuilAtteint>.Unsubscribe(OnSeuilAtteint);
        EventBus<OnMissionDemarree>.Unsubscribe(OnMissionDemarree);
    }

    private void Update()
    {
        if (_locked) return;
        VerifierVisionJoueur();
    }

    // ================================================================
    // STATE MACHINE — transitions
    // ================================================================

    public void EntrerEtat(ProprietaireState nouvelEtat)
    {
        if (_etatCourant == nouvelEtat && nouvelEtat != ProprietaireState.Idle) return;

        var ancien = _etatCourant;
        _etatCourant = nouvelEtat;

        if (_stateCoroutine != null) StopCoroutine(_stateCoroutine);

        EventBus<OnProprietaireStateChanged>.Raise(new OnProprietaireStateChanged
        {
            AncienEtat  = ancien,
            NouvelEtat  = nouvelEtat
        });

        _stateCoroutine = StartCoroutine(nouvelEtat switch
        {
            ProprietaireState.Idle        => CoroutineIdle(),
            ProprietaireState.Alert       => CoroutineAlert(),
            ProprietaireState.Investigate => CoroutineInvestigate(),
            ProprietaireState.Confront    => CoroutineConfront(),
            ProprietaireState.Panic       => CoroutinePanic(),
            ProprietaireState.Outdoor     => CoroutineOutdoor(),
            ProprietaireState.Locked      => CoroutineLocked(),
            ProprietaireState.Furious     => CoroutineFurious(),
            _                             => CoroutineIdle()
        });
    }

    // ================================================================
    // COROUTINES PAR ÉTAT
    // ================================================================

    // IDLE — vaque à ses occupations, patrouille
    private IEnumerator CoroutineIdle()
    {
        _agent.speed = _def?.VitesseDeplacementNormal ?? 2.5f;
        _animator?.SetTrigger("Idle");

        while (_etatCourant == ProprietaireState.Idle)
        {
            if (_pointsPatrouille.Length > 0)
            {
                Transform cible = _pointsPatrouille[_indexPatrouille % _pointsPatrouille.Length];
                _agent.SetDestination(cible.position);

                yield return new WaitUntil(() =>
                    !_agent.pathPending && _agent.remainingDistance < 0.5f);

                _indexPatrouille++;
                yield return new WaitForSeconds(Random.Range(1f, 3f));
            }
            else
            {
                yield return new WaitForSeconds(1f);
            }
        }
    }

    // ALERT — s'arrête, regarde autour, cherche l'origine du bruit
    private IEnumerator CoroutineAlert()
    {
        _agent.SetDestination(transform.position); // s'arrête
        _animator?.SetTrigger("Alert");

        yield return new WaitForSeconds(1.5f);

        if (_palierCourant >= 2)
            EntrerEtat(ProprietaireState.Investigate);
        else
            EntrerEtat(ProprietaireState.Idle);
    }

    // INVESTIGATE — va vers la source de bruit ou la dernière position du joueur
    private IEnumerator CoroutineInvestigate()
    {
        _animator?.SetTrigger("Investigate");
        _agent.SetDestination(_dernierBruitPos);

        yield return new WaitUntil(() =>
            !_agent.pathPending && _agent.remainingDistance < 1.5f);

        yield return new WaitForSeconds(2f);

        // Revient à Idle si rien trouvé
        if (_etatCourant == ProprietaireState.Investigate)
            EntrerEtat(ProprietaireState.Idle);
    }

    // CONFRONT — approche le joueur, exige le mandat, argumente
    // NE TOUCHE PAS physiquement le joueur
    private IEnumerator CoroutineConfront()
    {
        _agent.speed = _def?.VitesseDeplacementNormal ?? 2.5f;
        _animator?.SetTrigger("Confront");

        // S'approche du joueur à distance de conversation (2m)
        while (_etatCourant == ProprietaireState.Confront)
        {
            if (_joueur != null)
            {
                float dist = Vector3.Distance(transform.position, _joueur.position);
                if (dist > 2.5f)
                    _agent.SetDestination(_joueur.position);
                else
                    _agent.SetDestination(transform.position); // s'arrête
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    // PANIC — court, appelle des renforts, tend des pièges
    private IEnumerator CoroutinePanic()
    {
        _agent.speed = _def?.VitesseDeplacementNormal * 1.5f ?? 3.5f;
        _animator?.SetTrigger("Panic");

        // Début de panique : appel téléphonique (V1 : juste un délai)
        yield return new WaitForSeconds(3f);

        // TODO V2 : spawner un ami/avocat
        yield break;
    }

    // OUTDOOR — sort de la maison vers le véhicule
    private IEnumerator CoroutineOutdoor()
    {
        _animator?.SetTrigger("Outdoor");
        _agent.speed = _def?.VitesseDeplacementNormal ?? 2.5f;

        EventBus<OnProprietaireSortDeLaMaison>.Raise(new OnProprietaireSortDeLaMaison());

        if (_vehicule != null)
        {
            _agent.SetDestination(_vehicule.position);

            yield return new WaitUntil(() =>
                !_agent.pathPending && _agent.remainingDistance < 2f);

            // Arrive au véhicule : notifie VehiculeSystem
            EventBus<OnVehiculeAttaque>.Raise(new OnVehiculeAttaque
            {
                Attaquant          = gameObject,
                EstLeProprietaire  = true
            });

            // Forçage du coffre (5–15 sec selon le véhicule)
            yield return new WaitForSeconds(Random.Range(5f, 15f));

            // Retour panique après avoir récupéré un objet
            EntrerEtat(ProprietaireState.Panic);
        }
    }

    // LOCKED — immobilisé (menottes, enfermé)
    private IEnumerator CoroutineLocked()
    {
        _locked = true;
        _agent.enabled = false;
        _animator?.SetTrigger("Locked");

        // Durée selon le niveau des menottes (60–120 sec)
        // V1 simplifié : 60 sec fixes
        yield return new WaitForSeconds(60f);

        _locked = false;
        _agent.enabled = true;
        EntrerEtat(ProprietaireState.Panic);
    }

    // FURIOUS — actions multiples simultanées
    private IEnumerator CoroutineFurious()
    {
        _agent.speed = _def?.VitesseDeplacementPanique ?? 4.5f;
        _animator?.SetTrigger("Furious");

        // En V1 : comme Panic mais plus rapide
        // V2 : pose pièges en urgence, multiplie les actions
        yield break;
    }

    // ================================================================
    // HANDLERS D'EVENTS
    // ================================================================

    private void OnParanoiaChanged(OnParanoiaChanged e)
    {
        _paranoiaCourante = e.NouvelleValeur;
        _palierCourant    = e.NouveauPalier;

        // Transitions automatiques selon le palier
        switch (_etatCourant)
        {
            case ProprietaireState.Idle when e.NouveauPalier >= 1:
                EntrerEtat(ProprietaireState.Alert);
                break;
            case ProprietaireState.Alert when e.NouveauPalier >= 2:
                EntrerEtat(ProprietaireState.Investigate);
                break;
        }
    }

    private void OnBruitEmis(OnBruitEmis e)
    {
        float dist = Vector3.Distance(transform.position, e.Position);
        if (dist <= e.Portee + _porteeOuie)
        {
            _dernierBruitPos = e.Position;
            if (_etatCourant == ProprietaireState.Idle)
                EntrerEtat(ProprietaireState.Alert);
        }
    }

    private void OnSeuilAtteint(OnSeuilAtteint e)
    {
        // À 20% du quota : peut sortir vers le véhicule (palier 3 requis)
        if (e.Pourcentage >= 0.20f && _palierCourant >= 3)
        {
            if (_etatCourant != ProprietaireState.Outdoor)
                EntrerEtat(ProprietaireState.Outdoor);
        }

        // À 80% : devient Furieux
        if (e.Pourcentage >= 0.80f)
            EntrerEtat(ProprietaireState.Furious);
    }

    private void OnMissionDemarree(OnMissionDemarree e)
    {
        _def              = e.Mission.Proprietaire;
        _paranoiaCourante = _def?.ParanoiaDepart ?? 0f;
        _palierCourant    = ParanoiaSystem.CalculerPalier(_paranoiaCourante);
        EntrerEtat(ProprietaireState.Idle);
    }

    // ================================================================
    // VISION
    // ================================================================

    private void VerifierVisionJoueur()
    {
        if (_joueur == null) return;

        Vector3 dir  = (_joueur.position - transform.position).normalized;
        float   dist = Vector3.Distance(transform.position, _joueur.position);

        if (dist > _porteeVision) return;

        float angle = Vector3.Angle(transform.forward, dir);
        if (angle > _angleVision) return;

        // Raycast pour les obstacles
        if (Physics.Raycast(transform.position + Vector3.up, dir, out RaycastHit hit, dist))
        {
            if (hit.transform != _joueur) return;
        }

        // Joueur détecté
        _paranoiaSys?.SetJoueurVisible(true);

        if (_etatCourant == ProprietaireState.Idle || _etatCourant == ProprietaireState.Investigate)
            EntrerEtat(ProprietaireState.Confront);
    }

    // ================================================================
    // API PUBLIQUE
    // ================================================================

    public void Immobiliser() => EntrerEtat(ProprietaireState.Locked);

    public void SetJoueur(Transform joueur)   => _joueur   = joueur;
    public void SetVehicule(Transform vehic)  => _vehicule = vehic;
    public ProprietaireState EtatCourant      => _etatCourant;
}
