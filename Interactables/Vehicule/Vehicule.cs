// ============================================================
// Vehicule.cs — Bailiff & Co
// Coffre (porte animée + zone trigger), porte conducteur,
// cage animaux. Sur le root du prefab véhicule.
//
// ARCHITECTURE :
//  - ZoneCoffreTrigger.cs (enfant) relaie OnTriggerEnter/Exit
//    → AjouterObjetZone / RetirerObjetZone
//  - Les objets dans _objetsDansZone sont "dans le coffre"
//    visuellement ; ils sont convertis en quota UNIQUEMENT
//    au moment où le joueur confirme le départ.
//  - Le proprio/voisin appellent TakeRandom() depuis leurs
//    propres scripts pour voler un objet.
//  - La porte conducteur émet OnDemandeFinMission via EventBus ;
//    HUDSystem affiche le popup de confirmation.
// ============================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vehicule : MonoBehaviour, IInteractable
{
    // ================================================================
    // SÉRIALISATION
    // ================================================================

    [Header("Configuration")]
    [SerializeField] private VehiculeDef _def;

    [Header("Porte conducteur")]
    [SerializeField] private Collider  _colliderPorteConducteur;

    [Header("Coffre — porte animée")]
    [SerializeField] private Transform _porteCoffre;
    [SerializeField] private Collider  _colliderPorteCoffre;
    [SerializeField] private Vector3   _axeRotationCoffre    = Vector3.right;
    [SerializeField] private float     _angleOuvertureCoffre = 90f;
    [SerializeField] private float     _dureeAnimationCoffre = 0.4f;

    [Header("Zone trigger du coffre")]
    [SerializeField] private Collider _zoneTrigger; // BoxCollider Is Trigger sur l'enfant ZoneCoffre

    [Header("Cage à animaux (optionnelle)")]
    [SerializeField] private Transform _cageAnimaux;
    [SerializeField] private Collider  _colliderCage;
    [SerializeField] private float     _dureeAnimationCage = 0.3f;

    [Header("État (lecture seule dans l'Inspector)")]
    [SerializeField] private bool _coffreOuvert  = false;
    [SerializeField] private bool _cageOuverte   = false;
    [SerializeField] private bool _antivol       = false;
    [SerializeField] private int  _objetsCharges = 0;   // compteur définitif après départ

    // ================================================================
    // ÉTAT PRIVÉ
    // ================================================================

    // Objets actuellement dans la zone trigger du coffre
    private readonly HashSet<ObjetValeur> _objetsDansZone = new();

    // Flags d'animation — un par porte pour ne pas se bloquer mutuellement
    private bool _coffreEnMouvement = false;
    private bool _cageEnMouvement   = false;

    // Cage
    private bool        _animalEnCage = false;
    private ObjetValeur _animalEnCage_ref = null;

    // Rotation initiale de la porte coffre (mémorisée en Awake)
    private Quaternion _rotationFermeeCoffre;
    private Quaternion _rotationFermeeCage;

    // Collider actuellement visé par le rayon du joueur
    private Collider _colliderVise;

    // Cache des références scène
    private MissionSystem _missionSys;
    private PlayerCarry   _playerCarry;    // mis en cache pour éviter FindObjectOfType en Update
    private QuotaSystem   _quotaSys;

    // État du popup de confirmation
    private bool _confirmationEnAttente = false;

    // ================================================================
    // LIFECYCLE
    // ================================================================

    private void Awake()
    {
        _missionSys  = FindObjectOfType<MissionSystem>();
        _playerCarry = FindObjectOfType<PlayerCarry>();
        _quotaSys    = FindObjectOfType<QuotaSystem>();

        // Zone trigger désactivée par défaut — coffre fermé
        if (_zoneTrigger != null) _zoneTrigger.enabled = false;

        // Mémorise les rotations initiales des portes
        if (_porteCoffre  != null) _rotationFermeeCoffre = _porteCoffre.localRotation;
        if (_cageAnimaux  != null) _rotationFermeeCage   = _cageAnimaux.localRotation;
    }

    private void OnEnable()
    {
        EventBus<OnVehiculeAttaque>.Subscribe(OnVehiculeAttaque);
        EventBus<OnConfirmationDepart>.Subscribe(OnConfirmationDepart);
    }

    private void OnDisable()
    {
        EventBus<OnVehiculeAttaque>.Unsubscribe(OnVehiculeAttaque);
        EventBus<OnConfirmationDepart>.Unsubscribe(OnConfirmationDepart);
    }

    // ================================================================
    // ZONE DE DÉPÔT — appelé par ZoneCoffreTrigger.cs
    // ================================================================

    public void AjouterObjetZone(ObjetValeur obj)
    {
        _objetsDansZone.Add(obj);
        // Le HUD peut s'abonner à OnQuotaChanged pour afficher le contenu —
        // ici on émet juste l'event de mise à jour du label si nécessaire.
    }

    public void RetirerObjetZone(ObjetValeur obj)
    {
        _objetsDansZone.Remove(obj);
    }

    // ================================================================
    // IINTERACTABLE
    // ================================================================

    public void SetColliderVise(Collider col) => _colliderVise = col;

    public bool CanInteract(GameObject interacteur)
    {
        // Porte conducteur : toujours interactable (mains vides ou non — le popup bloque si besoin)
        if (_colliderVise == _colliderPorteConducteur)
            return true;

        // Cage : interactable si mains vides OU si le joueur porte quelque chose à déposer
        if (_colliderVise == _colliderCage && _cageAnimaux != null)
            return !_cageEnMouvement;

        // Coffre : mains vides obligatoire
        if (_colliderVise == _colliderPorteCoffre)
        {
            if (_coffreEnMouvement) return false;
            if (interacteur.TryGetComponent<PlayerCarry>(out var carry) && carry.EstEnTrain)
                return false;
            return true;
        }

        return true;
    }

    public void Interact(GameObject interacteur)
    {
        if (_colliderVise == _colliderPorteCoffre)
        {
            if (_coffreOuvert) FermerCoffre();
            else               OuvrirCoffre();
        }
        else if (_colliderVise == _colliderPorteConducteur)
        {
            DemanderConfirmationDepart();
        }
        else if (_colliderVise == _colliderCage && _cageAnimaux != null)
        {
            if (interacteur.TryGetComponent<PlayerCarry>(out var carry) && carry.EstEnTrain)
                DeposerAnimalEnCage(carry);
            else if (_cageOuverte) FermerCage();
            else                   OuvrirCage();
        }
    }

    public string GetInteractionLabel()
    {
        bool mainsPleines = _playerCarry != null && _playerCarry.EstEnTrain;

        if (_colliderVise == _colliderPorteCoffre)
        {
            if (mainsPleines) return "Pose l'objet d'abord";
            return GetLabelCoffre();
        }

        if (_colliderVise == _colliderPorteConducteur)
            return GetLabelPorte();

        if (_colliderVise == _colliderCage && _cageAnimaux != null)
            return GetLabelCage(depotPossible: mainsPleines);

        return "";
    }

    // ================================================================
    // COFFRE
    // ================================================================

    public void OuvrirCoffre()
    {
        if (_coffreOuvert || _coffreEnMouvement) return;
        _coffreOuvert = true;

        // Active la zone trigger : les objets peuvent maintenant entrer
        if (_zoneTrigger != null) _zoneTrigger.enabled = true;

        if (_porteCoffre != null)
            StartCoroutine(AnimerPorte_Coffre(ouvrir: true));
    }

    public void FermerCoffre()
    {
        if (!_coffreOuvert || _coffreEnMouvement) return;
        _coffreOuvert = false;

        // Désactive la zone trigger : plus aucun objet ne peut entrer
        if (_zoneTrigger != null) _zoneTrigger.enabled = false;

        if (_porteCoffre != null)
            StartCoroutine(AnimerPorte_Coffre(ouvrir: false));
    }

    // ================================================================
    // CAGE À ANIMAUX
    // ================================================================

    private void OuvrirCage()
    {
        if (_cageOuverte || _cageEnMouvement) return;
        _cageOuverte = true;
        StartCoroutine(AnimerPorte_Cage(ouvrir: true));
    }

    private void FermerCage()
    {
        if (!_cageOuverte || _cageEnMouvement) return;
        _cageOuverte = false;
        StartCoroutine(AnimerPorte_Cage(ouvrir: false));
    }

    private void DeposerAnimalEnCage(PlayerCarry carry)
    {
        if (_animalEnCage)
        {
            Debug.Log("[Vehicule] La cage est déjà occupée.");
            return;
        }

        if (!_cageOuverte) OuvrirCage();

        _animalEnCage_ref = carry.ObjetEnMain;
        carry.Poser(doux: true);
        _animalEnCage = true;

        StartCoroutine(FermerCageApresDelai(0.5f));
    }

    private IEnumerator FermerCageApresDelai(float delai)
    {
        yield return new WaitForSeconds(delai);
        FermerCage();
    }

    // ================================================================
    // DÉPART — popup de confirmation via EventBus
    // ================================================================

    private void DemanderConfirmationDepart()
    {
        if (_confirmationEnAttente) return;
        _confirmationEnAttente = true;

        // HUDSystem écoute cet event et affiche le popup Oui/Non.
        // Le jeu continue à tourner pendant ce temps.
        EventBus<OnDemandeFinMission>.Raise(new OnDemandeFinMission());
    }

    // Réponse du joueur depuis le HUDSystem
    private void OnConfirmationDepart(OnConfirmationDepart e)
    {
        _confirmationEnAttente = false;

        if (e.Confirme)
            StartCoroutine(PartirCoroutine());
        // Si refusé : rien à faire, le jeu continue normalement
    }

    private IEnumerator PartirCoroutine()
    {
        // Convertit les objets présents dans la zone en quota au moment du départ
        ConvertirObjetsEnQuota();

        // Fondu noir géré par HUDSystem via event
        EventBus<OnFondNoir>.Raise(new OnFondNoir { DureeSecondes = 1f });

        yield return new WaitForSeconds(1f);
        _missionSys?.JoueurPartAvecVehicule();
    }

    // ================================================================
    // CONVERSION DES OBJETS AU DÉPART
    // ================================================================

    private void ConvertirObjetsEnQuota()
    {
        if (_def == null) return;

        var aCharger = new List<ObjetValeur>(_objetsDansZone);
        foreach (var obj in aCharger)
        {
            if (_objetsCharges >= _def.CapaciteObjets) break;
            obj.ChargerDansVehicule(); // → émet OnObjetCharge, Destroy(gameObject)
            _objetsCharges++;
        }
        _objetsDansZone.Clear();
    }

    // ================================================================
    // VOLER UN OBJET — appelé par ProprietaireAI ou VoisinSystem
    // ================================================================

    /// <summary>
    /// Retire et retourne un objet aléatoire du coffre.
    /// À appeler depuis ProprietaireAI ou VoisinSystem uniquement.
    /// Retourne null si le coffre est vide ou protégé par antivol.
    /// </summary>
    public ObjetValeur TakeRandom()
    {
        if (_antivol) return null;
        if (_objetsDansZone.Count == 0) return null;

        // Choisit un objet aléatoire dans le HashSet
        int index = Random.Range(0, _objetsDansZone.Count);
        ObjetValeur cible = null;
        int i = 0;
        foreach (var obj in _objetsDansZone)
        {
            if (i == index) { cible = obj; break; }
            i++;
        }

        if (cible != null)
        {
            _objetsDansZone.Remove(cible);
            EventBus<OnProprietaireRecupereObjet>.Raise(new OnProprietaireRecupereObjet
            {
                Objet  = cible.Def,
                Valeur = cible.ValeurReelle
            });
        }

        return cible; // Le script appelant repositionne l'objet dans la maison
    }

    // ================================================================
    // ANTIVOL CONSOMMABLE (durée limitée)
    // ================================================================

    public void ActiverAntivol(float dureeSecondes = 0f)
    {
        _antivol = true;
        if (dureeSecondes > 0f)
            StartCoroutine(DesactiverAntivolApres(dureeSecondes));
    }

    private IEnumerator DesactiverAntivolApres(float duree)
    {
        yield return new WaitForSeconds(duree);
        _antivol = false;
    }

    // ================================================================
    // ANIMATIONS — un coroutine par porte, flags séparés
    // ================================================================

    private IEnumerator AnimerPorte_Coffre(bool ouvrir)
    {
        _coffreEnMouvement = true;

        Quaternion debut = _porteCoffre.localRotation;
        Quaternion fin   = ouvrir
            ? _rotationFermeeCoffre * Quaternion.AngleAxis(_angleOuvertureCoffre, _axeRotationCoffre)
            : _rotationFermeeCoffre; // retour précis à la rotation mémorisée

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / _dureeAnimationCoffre;
            _porteCoffre.localRotation = Quaternion.Lerp(debut, fin, Mathf.Clamp01(t));
            yield return null;
        }
        _porteCoffre.localRotation = fin;
        _coffreEnMouvement = false;
    }

    private IEnumerator AnimerPorte_Cage(bool ouvrir)
    {
        _cageEnMouvement = true;

        Quaternion debut = _cageAnimaux.localRotation;
        Quaternion fin   = ouvrir
            ? _rotationFermeeCage * Quaternion.AngleAxis(90f, Vector3.up)
            : _rotationFermeeCage;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / _dureeAnimationCage;
            _cageAnimaux.localRotation = Quaternion.Lerp(debut, fin, Mathf.Clamp01(t));
            yield return null;
        }
        _cageAnimaux.localRotation = fin;
        _cageEnMouvement = false;
    }

    // ================================================================
    // LABELS
    // ================================================================

    private string GetLabelCoffre()
    {
        if (_coffreEnMouvement) return "...";
        return _coffreOuvert
            ? $"Fermer le coffre ({_objetsDansZone.Count} objet(s))"
            : "Ouvrir le coffre";
    }

    private string GetLabelPorte()
    {
        if (_quotaSys == null) return "Partir";
        return _quotaSys.QuotaAtteint
            ? $"Partir ✓ — {_quotaSys.ValeurTotale:N0} €"
            : $"Partir — {_quotaSys.ValeurTotale:N0} / {_quotaSys.ValeurCible:N0} €";
    }

    private string GetLabelCage(bool depotPossible)
    {
        if (_animalEnCage)
            return $"Cage occupée — {_animalEnCage_ref?.Def?.NomObjet ?? "Animal"}";
        if (depotPossible)
            return "Déposer l'animal dans la cage";
        return _cageOuverte ? "Fermer la cage" : "Ouvrir la cage";
    }

    // ================================================================
    // EVENTS
    // ================================================================

    private void OnVehiculeAttaque(OnVehiculeAttaque e)
    {
        // Implémentation complète dans ProprietaireAI / VoisinSystem
        // qui appellent TakeRandom() directement.
        // Cet event reste pour les notifications HUD futures.
    }

    // ================================================================
    // PROPRIÉTÉS PUBLIQUES
    // ================================================================

    public bool EstPlein     => _def != null && _objetsCharges >= _def.CapaciteObjets;
    public bool CoffreOuvert => _coffreOuvert;
    public bool CageOuverte  => _cageOuverte;
    public bool AnimalEnCage => _animalEnCage;
    public bool Antivol      => _antivol;
    public int  NbObjetsEnCoffre => _objetsDansZone.Count;

    /// <summary>Lecture seule pour le voisin/proprio : y a-t-il des objets accessibles ?</summary>
    public bool CoffreAccessible => _coffreOuvert && !_antivol && _objetsDansZone.Count > 0;
}
