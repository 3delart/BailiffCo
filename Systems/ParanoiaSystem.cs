// ============================================================
// ParanoiaSystem.cs — Bailiff & Co
// SOURCE DE VÉRITÉ UNIQUE pour la paranoïa (0–100).
// Ne modifie rien directement — reçoit des events, calcule,
// émet OnParanoiaChanged et les transitions de palier.
// ============================================================
using UnityEngine;

public class ParanoiaSystem : MonoBehaviour
{
    // ---- Paliers (selon GDD §4.2) ----
    public const float PALIER_CALM      = 0f;
    public const float PALIER_MEFIANT   = 11f;
    public const float PALIER_INQUIET   = 26f;
    public const float PALIER_PANIQUE   = 51f;
    public const float PALIER_FURIEUX   = 76f;
    public const float PALIER_OBSESSION = 91f;

    // ---- Décroissance passive ----
    private const float DECROISSANCE_PAR_SECONDE = 5f / 120f; // 5 pts / 2 min
    private const float DELAI_AVANT_DECROISSANCE = 10f;       // sec sans action

    [Header("État (lecture seule en jeu)")]
    [SerializeField] private float _paranoia = 0f;
    [SerializeField] private int   _palier   = 0;

    private float _timerDepuisDerniereAction = 0f;
    private bool  _joueurVisible = false;
    private bool  _missionActive = false;

    // ---- API publique (lecture) ----
    public float Valeur => _paranoia;
    public int   Palier => _palier;

    // ================================================================
    // LIFECYCLE
    // ================================================================

    private void OnEnable()
    {
        EventBus<OnMissionDemarree>.Subscribe(OnMissionDemarree);
        EventBus<OnMissionTerminee>.Subscribe(OnMissionTerminee);
        EventBus<OnBruitEmis>.Subscribe(OnBruitEmis);
        EventBus<OnObjetCharge>.Subscribe(OnObjetCharge);
        EventBus<OnPiegeDeclenche>.Subscribe(OnPiegeDeclenche);
        EventBus<OnAnimalAboie>.Subscribe(OnAnimalAboie);
    }

    private void OnDisable()
    {
        EventBus<OnMissionDemarree>.Unsubscribe(OnMissionDemarree);
        EventBus<OnMissionTerminee>.Unsubscribe(OnMissionTerminee);
        EventBus<OnBruitEmis>.Unsubscribe(OnBruitEmis);
        EventBus<OnObjetCharge>.Unsubscribe(OnObjetCharge);
        EventBus<OnPiegeDeclenche>.Unsubscribe(OnPiegeDeclenche);
        EventBus<OnAnimalAboie>.Unsubscribe(OnAnimalAboie);
    }

    private void Update()
    {
        if (!_missionActive) return;

        // Décroissance passive si joueur discret
        if (!_joueurVisible)
        {
            _timerDepuisDerniereAction += Time.deltaTime;
            if (_timerDepuisDerniereAction > DELAI_AVANT_DECROISSANCE)
                Modifier(-DECROISSANCE_PAR_SECONDE * Time.deltaTime);
        }
    }

    // ================================================================
    // HANDLERS D'EVENTS
    // ================================================================

    private void OnMissionDemarree(OnMissionDemarree e)
    {
        _paranoia = e.Mission.Proprietaire?.ParanoiaDepart ?? 0f;
        _palier   = CalculerPalier(_paranoia);
        _missionActive = true;
        _timerDepuisDerniereAction = 0f;
    }

    private void OnMissionTerminee(OnMissionTerminee e)
    {
        _missionActive = false;
    }

    private void OnBruitEmis(OnBruitEmis e)
    {
        float delta = e.Niveau switch
        {
            NiveauBruit.Leger    => 3f,
            NiveauBruit.Fort     => 12f,
            NiveauBruit.Tresfort => 25f,
            _                    => 0f
        };
        Modifier(delta);
        ResetTimerDecroissance();
    }

    private void OnObjetCharge(OnObjetCharge e)
    {
        // Chaque objet chargé monte la paranoïa
        float delta = Mathf.Clamp(e.Valeur / 5000f, 3f, 15f);
        Modifier(delta);
        ResetTimerDecroissance();
    }

    private void OnPiegeDeclenche(OnPiegeDeclenche e)
    {
        Modifier(e.Piege.ModifParanoiaSurDeclenchement);
        ResetTimerDecroissance();
    }

    private void OnAnimalAboie(OnAnimalAboie e)
    {
        float delta = Mathf.Lerp(3f, 8f, e.Intensite);
        Modifier(delta);
        ResetTimerDecroissance();
    }

    // ================================================================
    // API PUBLIQUE — pour les systèmes qui doivent modifier manuellement
    // (ex: badge présenté = -5, spray illégal = +15)
    // ================================================================

    public void Modifier(float delta)
    {
        if (!_missionActive) return;

        float ancienne = _paranoia;
        int ancienPalier = _palier;

        _paranoia = Mathf.Clamp(_paranoia + delta, 0f, 100f);
        _palier   = CalculerPalier(_paranoia);

        EventBus<OnParanoiaChanged>.Raise(new OnParanoiaChanged
        {
            NouvelleValeur  = _paranoia,
            AncienneValeur  = ancienne,
            NouveauPalier   = _palier
        });
    }

    public void SetJoueurVisible(bool visible)
    {
        _joueurVisible = visible;
        if (visible) ResetTimerDecroissance();
    }

    // ================================================================
    // UTILITAIRES
    // ================================================================

    private void ResetTimerDecroissance() => _timerDepuisDerniereAction = 0f;

    public static int CalculerPalier(float valeur)
    {
        if (valeur >= PALIER_OBSESSION) return 5;
        if (valeur >= PALIER_FURIEUX)   return 4;
        if (valeur >= PALIER_PANIQUE)   return 3;
        if (valeur >= PALIER_INQUIET)   return 2;
        if (valeur >= PALIER_MEFIANT)   return 1;
        return 0;
    }

    public static string NomPalier(int palier) => palier switch
    {
        0 => "Calm",
        1 => "Méfiant",
        2 => "Inquiet",
        3 => "Paniqué",
        4 => "Furieux",
        5 => "Obsessionnel",
        _ => "?"
    };
}
