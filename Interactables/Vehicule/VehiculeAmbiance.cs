// ============================================================
// VehiculeAmbiance.cs — Bailiff & Co
// A mettre sur le prefab de chaque vehicule en mission.
// Joue aleatoirement un son special (braiment, musique...)
// dans la fourchette de temps definie dans VehiculeDef.
//
// SETUP UNITY :
//   Prefab vehicule :
//   ├── Vehicule.cs (root)
//   ├── VehiculeAmbiance.cs  ← ce script
//   └── AudioSource          ← assigner dans _audioSource
//       (Spatial Blend = 1 pour audio 3D, Play On Awake = false)
// ============================================================
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class VehiculeAmbiance : MonoBehaviour
{
    [Header("Donnees")]
    [Tooltip("La VehiculeDef de ce vehicule — contient les clips et les intervalles.")]
    [SerializeField] private VehiculeDef _def;

    [Header("Références")]
    [SerializeField] private AudioSource _audioSource;

    // ================================================================
    // LIFECYCLE
    // ================================================================

    private void Awake()
    {
        if (_audioSource == null)
            _audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        // Ne démarre la coroutine que si des sons spéciaux sont définis
        if (_def == null) return;
        if (_def.SonsSpeciaux == null || _def.SonsSpeciaux.Length == 0) return;

        StartCoroutine(BoucleAmbiance());
    }

    // ================================================================
    // COROUTINE PRINCIPALE
    // ================================================================

    private IEnumerator BoucleAmbiance()
    {
        // Attente initiale aléatoire pour ne pas que tous les véhicules
        // jouent exactement au même moment si plusieurs sont présents
        float attenteInitiale = Random.Range(
            _def.IntervalleMinSecondes * 0.5f,
            _def.IntervalleMaxSecondes * 0.5f);
        yield return new WaitForSeconds(attenteInitiale);

        while (true)
        {
            // Choisit un clip aléatoire parmi les sons spéciaux
            AudioClip clip = ChoisirClipAleatoire();
            if (clip != null)
            {
                _audioSource.clip = clip;
                _audioSource.Play();

                // Emet un bruit pour que le ProprietaireAI puisse réagir
                // (portée modérée — le son vient de la rue)
                EventBus<OnBruitEmis>.Raise(new OnBruitEmis
                {
                    Position = transform.position,
                    Portee   = 12f,
                    Niveau   = NiveauBruit.Fort,
                    Source   = gameObject
                });
            }

            // Attend un intervalle aléatoire avant le prochain son
            float attente = Random.Range(
                _def.IntervalleMinSecondes,
                _def.IntervalleMaxSecondes);
            yield return new WaitForSeconds(attente);
        }
    }

    // ================================================================
    // UTILITAIRES
    // ================================================================

    private AudioClip ChoisirClipAleatoire()
    {
        if (_def.SonsSpeciaux == null || _def.SonsSpeciaux.Length == 0)
            return null;

        int index = Random.Range(0, _def.SonsSpeciaux.Length);
        return _def.SonsSpeciaux[index];
    }

    // ================================================================
    // API PUBLIQUE
    // ================================================================

    /// <summary>
    /// Stoppe les sons spéciaux (ex : fin de mission, popup de départ).
    /// </summary>
    public void StopperAmbiance()
    {
        StopAllCoroutines();
        if (_audioSource.isPlaying)
            _audioSource.Stop();
    }
}
