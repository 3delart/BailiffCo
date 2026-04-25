// ============================================================
// QuotaSystem.cs — Bailiff & Co
// Calcule la valeur totale chargée dans le véhicule.
// Ne connaît pas le proprio. Ne connaît pas le joueur.
// Émet OnQuotaChanged, OnQuotaAtteint, OnSeuilAtteint.
// ============================================================
using System.Collections.Generic;
using UnityEngine;

public class QuotaSystem : MonoBehaviour
{
    [Header("État (lecture seule)")]
    [SerializeField] private float _valeurTotale   = 0f;
    [SerializeField] private float _valeurCible    = 0f;
    [SerializeField] private bool  _quotaAtteint   = false;

    // Seuils à surveiller (selon GDD : 20% = proprio peut sortir)
    private static readonly float[] SEUILS_SURVEILLES = { 0.20f, 0.50f, 0.60f, 0.80f, 1.00f };
    private readonly HashSet<float> _seuilsDejaDeclenchees = new();

    // Détail par objet pour l'écran de résultat
    private readonly List<(ObjetDef objet, float valeur)> _objetsCharges = new();

    // ---- API publique ----
    public float ValeurTotale     => _valeurTotale;
    public float ValeurCible      => _valeurCible;
    public float Pourcentage      => _valeurCible > 0 ? _valeurTotale / _valeurCible : 0f;
    public bool  QuotaAtteint     => _quotaAtteint;
    public IReadOnlyList<(ObjetDef, float)> ObjetsCharges => _objetsCharges;

    // ================================================================
    // LIFECYCLE
    // ================================================================

    private void OnEnable()
    {
        // Désabonne d'abord pour éviter les doublons
        EventBus<OnMissionDemarree>.Unsubscribe(OnMissionDemarree);
        EventBus<OnObjetCharge>.Unsubscribe(OnObjetCharge);
        EventBus<OnProprietaireRecupereObjet>.Unsubscribe(OnObjetRecupereParProprio);

        EventBus<OnMissionDemarree>.Subscribe(OnMissionDemarree);
        EventBus<OnObjetCharge>.Subscribe(OnObjetCharge);
        EventBus<OnProprietaireRecupereObjet>.Subscribe(OnObjetRecupereParProprio);
    }

    private void OnDisable()
    {
        EventBus<OnMissionDemarree>.Unsubscribe(OnMissionDemarree);
        EventBus<OnObjetCharge>.Unsubscribe(OnObjetCharge);
        EventBus<OnProprietaireRecupereObjet>.Unsubscribe(OnObjetRecupereParProprio);
    }

    // ================================================================
    // HANDLERS
    // ================================================================

    private void OnMissionDemarree(OnMissionDemarree e)
    {
        _valeurTotale = 0f;
        _quotaAtteint = false;
        _objetsCharges.Clear();
        _seuilsDejaDeclenchees.Clear();

        // Calcule la valeur cible (quota minimum)
        if (e.Mission.ValeurQuotaMinimum > 0)
        {
            _valeurCible = e.Mission.ValeurQuotaMinimum;
        }
        else
        {
            // Fallback : 50% de la valeur max possible des biens saisis
            float total = 0;
            foreach (var obj in e.Mission.BiensSaisis)
                total += obj.ValeurMax;
            _valeurCible = total * 0.5f;
        }

        PublierChangement();
    }

    private void OnObjetCharge(OnObjetCharge e)
    {
        _valeurTotale += e.Valeur;
        _objetsCharges.Add((e.Objet, e.Valeur));

        PublierChangement();
        VerifierSeuils();

        if (!_quotaAtteint && _valeurTotale >= _valeurCible)
        {
            _quotaAtteint = true;
            EventBus<OnQuotaAtteint>.Raise(new OnQuotaAtteint());
        }
    }

    private void OnObjetRecupereParProprio(OnProprietaireRecupereObjet e)
    {
        // Le proprio a vidé un objet du coffre — on recalcule
        bool trouve = false;
        for (int i = _objetsCharges.Count - 1; i >= 0; i--)
        {
            if (_objetsCharges[i].objet == e.Objet)
            {
                _valeurTotale -= _objetsCharges[i].valeur;
                _objetsCharges.RemoveAt(i);
                trouve = true;
                break;
            }
        }

        if (trouve)
        {
            _valeurTotale = Mathf.Max(0f, _valeurTotale);
            _quotaAtteint = _valeurTotale >= _valeurCible;
            PublierChangement();
        }
    }

    // ================================================================
    // UTILITAIRES
    // ================================================================

    private void PublierChangement()
    {
        EventBus<OnQuotaChanged>.Raise(new OnQuotaChanged
        {
            ValeurTotale       = _valeurTotale,
            ValeurCible        = _valeurCible,
            PourcentageAtteint = Pourcentage
        });
    }

    private void VerifierSeuils()
    {
        float pct = Pourcentage;
        foreach (float seuil in SEUILS_SURVEILLES)
        {
            if (pct >= seuil && !_seuilsDejaDeclenchees.Contains(seuil))
            {
                _seuilsDejaDeclenchees.Add(seuil);
                EventBus<OnSeuilAtteint>.Raise(new OnSeuilAtteint { Pourcentage = seuil });
            }
        }
    }
}
