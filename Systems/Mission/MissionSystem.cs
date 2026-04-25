// ============================================================
// MissionSystem.cs — Bailiff & Co
// Orchestrateur : charge le seed, instancie cachettes/objets/
// proprio/pièges, écoute OnQuotaAtteint, gère la fin de mission.
// C'est le seul système qui connaît la MissionDef en cours.
// ============================================================
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MissionSystem : MonoBehaviour
{
    [Header("Références scène")]
    [SerializeField] private ProprietaireAI _proprietaireAI;
    [SerializeField] private Vehicule  _vehiculeSystem;
    [SerializeField] private Transform[]     _spawnPointsObjets;
    [SerializeField] private Transform[]     _spawnPointsCachettes;

    [Header("État")]
    [SerializeField] private MissionDef  _missionCourante;
    [SerializeField] private bool        _missionActive   = false;
    [SerializeField] private bool        _quotaValide     = false;
    [SerializeField] private float       _tempsDebut      = 0f;

    // Suivi pour le résultat
    private float  _paranoiaMaxAtteinte = 0f;
    private int    _piegesDeclenches    = 0;
    private int    _objetsCasses        = 0;

    // ================================================================
    // API PUBLIQUE — appelée depuis le Hub avant chargement de scène
    // ================================================================

    /// <summary>Démarre une mission. Appelé par le Hub après sélection.</summary>
    private IEnumerator Start()
    {
        if (_missionCourante == null) yield break;
        
        yield return null; // attend une frame que tous les OnEnable() s'abonnent
        DemarrerMission(_missionCourante);
    }
    public void DemarrerMission(MissionDef mission)
    {
        _missionCourante = mission;

        // Seed reproductible (même seed = même mission)
        int seed = mission.SeedFixe != 0 ? mission.SeedFixe : Random.Range(1, 999999);
        Random.InitState(seed);

        EventBus<OnMissionDemarree>.Raise(new OnMissionDemarree
        {
            Mission = mission,
            Seed    = seed
        });

        _missionActive       = true;
        _quotaValide         = false;
        _paranoiaMaxAtteinte = 0f;
        _piegesDeclenches    = 0;
        _objetsCasses        = 0;
        _tempsDebut          = Time.time;

        SpawnerObjets(mission, seed);
    }

    // ================================================================
    // LIFECYCLE
    // ================================================================

    private void OnEnable()
    {
        EventBus<OnQuotaAtteint>.Subscribe(OnQuotaAtteint);
        EventBus<OnParanoiaChanged>.Subscribe(OnParanoiaChanged);
        EventBus<OnPiegeDeclenche>.Subscribe(OnPiegeDeclenche);
        EventBus<OnObjetEndommage>.Subscribe(OnObjetEndommage);
        EventBus<OnTimerUrgenceDéclenche>.Subscribe(OnTimerUrgence);
    }

    private void OnDisable()
    {
        EventBus<OnQuotaAtteint>.Unsubscribe(OnQuotaAtteint);
        EventBus<OnParanoiaChanged>.Unsubscribe(OnParanoiaChanged);
        EventBus<OnPiegeDeclenche>.Unsubscribe(OnPiegeDeclenche);
        EventBus<OnObjetEndommage>.Unsubscribe(OnObjetEndommage);
        EventBus<OnTimerUrgenceDéclenche>.Unsubscribe(OnTimerUrgence);
    }

    // ================================================================
    // HANDLERS
    // ================================================================

    private void OnQuotaAtteint(OnQuotaAtteint e)  => _quotaValide = true;

    private void OnParanoiaChanged(OnParanoiaChanged e)
    {
        if (e.NouvelleValeur > _paranoiaMaxAtteinte)
            _paranoiaMaxAtteinte = e.NouvelleValeur;
    }

    private void OnPiegeDeclenche(OnPiegeDeclenche e)  => _piegesDeclenches++;
    private void OnObjetEndommage(OnObjetEndommage e)   => _objetsCasses++;

    private void OnTimerUrgence(OnTimerUrgenceDéclenche e)
    {
        StartCoroutine(TimerExpulsionCoroutine(e.DureeSecondes));
    }

    // ================================================================
    // DÉPART DU JOUEUR — appelé par Vehicule.cs quand le joueur
    // interagit avec la porte du véhicule
    // ================================================================

    public void JoueurPartAvecVehicule()
    {
        if (!_missionActive) return;
        TerminerMission(depart: true);
    }

    // ================================================================
    // SPAWN DES OBJETS (V1 simplifié)
    // ================================================================

    private void SpawnerObjets(MissionDef mission, int seed)
    {
        if (mission.BiensSaisis == null || _spawnPointsObjets.Length == 0) return;

        // Shuffle des spawn points avec le seed
        var points = ShuffleArray(_spawnPointsObjets, seed);

        for (int i = 0; i < mission.BiensSaisis.Length && i < points.Length; i++)
        {
            ObjetDef def = mission.BiensSaisis[i];
            if (def.Prefab == null) continue;

            GameObject go = Instantiate(def.Prefab, points[i].position, points[i].rotation);

            // Assigner la valeur tirée selon le seed
            float valeur = Random.Range(def.ValeurMin, def.ValeurMax);
            if (go.TryGetComponent<ObjetValeur>(out var ov))
            {
                ov.Initialiser(def, valeur);
            }
        }
    }

    private T[] ShuffleArray<T>(T[] array, int seed)
    {
        var copy = (T[])array.Clone();
        var rng  = new System.Random(seed);
        for (int i = copy.Length - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (copy[i], copy[j]) = (copy[j], copy[i]);
        }
        return copy;
    }

    // ================================================================
    // FIN DE MISSION
    // ================================================================

    private void TerminerMission(bool depart)
    {
        if (!_missionActive) return;
        _missionActive = false;

        float temps = Time.time - _tempsDebut;

        // Calcul des étoiles
        var quotaSys = FindObjectOfType<QuotaSystem>();
        float recupere = quotaSys != null ? quotaSys.ValeurTotale : 0f;
        float cible    = quotaSys != null ? quotaSys.ValeurCible  : 1f;
        int etoiles    = CalculerEtoiles(recupere, cible, _objetsCasses, _piegesDeclenches);

        var result = new MissionResult
        {
            Mission                  = _missionCourante,
            ValeurTotaleRecuperee    = recupere,
            ValeurQuotaCible         = cible,
            NombreObjetsRecuperes    = quotaSys?.ObjetsCharges.Count ?? 0,
            NombreObjetsCasses       = _objetsCasses,
            NombrePiegesDeclenches   = _piegesDeclenches,
            TempsSecondes            = temps,
            ParanoiaMaxAtteinte      = _paranoiaMaxAtteinte,
            MissionReussie           = _quotaValide,
            Etoiles                  = etoiles,
            ArgentGagne              = recupere * 0.85f  // 15% de frais d'agence
        };

        EventBus<OnMissionTerminee>.Raise(new OnMissionTerminee { Resultat = result });

        // Retour au hub après un court délai
        StartCoroutine(RetourHubCoroutine(result));
    }

    private int CalculerEtoiles(float recupere, float cible, int casses, int pieges)
    {
        if (recupere < cible)        return 0;
        if (recupere >= cible * 2f && casses == 0 && pieges == 0) return 3;
        if (recupere >= cible * 1.5f && casses <= 3)              return 2;
        return 1;
    }

    private IEnumerator TimerExpulsionCoroutine(float duree)
    {
        yield return new WaitForSeconds(duree);
        if (_missionActive)
            TerminerMission(depart: false);
    }

    private IEnumerator RetourHubCoroutine(MissionResult result)
    {
        yield return new WaitForSeconds(2f);
        //SceneManager.LoadScene("Hub");
        Debug.Log($"Mission terminée ! Étoiles: {result.Etoiles} | Argent: {result.ArgentGagne}€");
    }
}
