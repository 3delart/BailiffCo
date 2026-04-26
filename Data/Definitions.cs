// ============================================================
// Definitions.cs — Bailiff & Co
// Tous les ScriptableObjects de données du jeu.
// Créer via : clic droit dans Project → Create → BailiffCo/...
// ============================================================
using UnityEngine;

// ------ ENUMS PARTAGÉS ------

public enum NiveauBruit { Silencieux, Leger, Fort, Tresfort }
public enum AnimalEspece { Chat, ChienCompagnie, ChienGarde, Perroquet, Poisson, Tortue, Lapin, Perruche }
public enum ProprietaireArchetypeType { CollectionneurFou, AncienMilitaire, StarDechu, SavantFou, InfluenceurDechu }
public enum ProprietaireState { Idle, Alert, Investigate, Confront, Panic, Outdoor, Locked, Furious }
public enum TypeCachette { DoubleFond, DerriereTableau, SousTapis, TrappePlancher, CoffreMural, ContientBanal, NainJardin, LitiereAnimal, AppareilCuisine, PiecesSecrete }
public enum TypePiege { SeauEau, FauxPlancher, ColleIndustrielle, AlarmeInfrarouge, CaisseChute, FumeeScene, ChienLache, GazSoporifique, DroneTracking }
public enum TypeVehicule { VeloCargo, Scooter, Pickup, Ane, Fourgon, CamionGlace, Helicoptere, Remorque }

// ============================================================
// OBJET DE VALEUR
// ============================================================
[CreateAssetMenu(menuName = "BailiffCo/ObjetDef")]
public class ObjetDef : ScriptableObject
{
    [Header("Identite")]
    public string NomObjet;
    [TextArea] public string Description;
    public Sprite IconeUI;
    public GameObject Prefab;

    [Header("Valeur")]
    public float ValeurMin = 500f;
    public float ValeurMax = 5000f;

    [Header("Physique")]
    public float Poids = 1f;
    public bool EstFragile = false;
    public bool NecessiteDeuxJoueurs = false;
    public bool EstTresGros = false;

    [Header("Scan")]
    public string NomCompletApresScan;
    public string AnneeEdition;
}

// ============================================================
// CACHETTE
// ============================================================
[CreateAssetMenu(menuName = "BailiffCo/CachetteDef")]
public class CachetteDef : ScriptableObject
{
    public TypeCachette Type;
    public bool EstDeplacable;
    public string[] TagsSpawn;
    public string SonOuverture;
    public GameObject PrefabIndicateurUV;
}

// ============================================================
// PIEGE
// ============================================================
[CreateAssetMenu(menuName = "BailiffCo/PiegeDef")]
public class PiegeDef : ScriptableObject
{
    public TypePiege Type;
    public string NomAffiche;

    [Header("Declenchement")]
    public string TagDeclencheur;
    public float RayonDetection = 0.5f;

    [Header("Effets")]
    public float DureesEffetSecondes = 5f;
    public float ModifParanoiaSurDeclenchement = 10f;
    public bool AlerteVoisins = false;
    public bool AlertePolice = false;

    [Header("Lisibilite")]
    public string IndicateurVisuelDescription;
}

// ============================================================
// OUTIL PERMANENT
// ============================================================
[CreateAssetMenu(menuName = "BailiffCo/OutilDef")]
public class OutilDef : ScriptableObject
{
    public string NomOutil;
    public Sprite Icone;
    public int PrixAchat;
    public bool EstOffert = false;

    [Header("Niveaux upgrade (3 max)")]
    public OutilNiveau[] Niveaux = new OutilNiveau[3];

    [System.Serializable]
    public struct OutilNiveau
    {
        public int PrixUpgrade;
        public string DescriptionEffet;
        public float ValeurNumerique;
    }
}

// ============================================================
// VEHICULE
// Contient uniquement ce qui s'affiche dans le popup Hub
// et ce qui pilote la mecanique de coffre en mission.
// Les effets speciaux (haut-parleur, cage animaux, treuil)
// sont geres directement sur le prefab via Vehicule.cs.
// ============================================================
[CreateAssetMenu(menuName = "BailiffCo/VehiculeDef")]
public class VehiculeDef : ScriptableObject
{
    // -- Identite ---------------------------------------------
    [Header("Identite")]
    public string       NomVehicule;
    public TypeVehicule Type;
    [TextArea(2, 4)]
    public string       Description;
    public Sprite       IllustrationUI;
    public GameObject   Prefab;

    // -- Location ---------------------------------------------
    [Header("Location")]
    [Tooltip("Prix de location pour une mission. 0 = gratuit (velo cargo).")]
    public float PrixLocation = 0f;

    // -- Coffre -----------------------------------------------
    [Header("Coffre")]
    [Tooltip("Nombre maximum d'objets charges dans ce vehicule.")]
    public int CapaciteObjets = 6;

    [Tooltip("0 = coffre toujours ouvert. > 0 = coffre fermable avec animation.")]
    public float DureeFermetureCoffreSecondes = 0.5f;

    [Tooltip("Temps en secondes que met le proprio pour forcer le coffre ferme. 0 = acces immediat.")]
    public float TempsForcageCoffreSecondes = 12f;

    // -- Textes Popup Hub -------------------------------------
    [Header("Textes Popup Hub")]
    [TextArea(2, 4)]
    public string AvantageDescription;

    [TextArea(2, 4)]
    public string InconvenientDescription;

    [TextArea(1, 3)]
    public string AstuceDescription;

    // -- Audio ------------------------------------------------
    [Header("Audio — Coffre")]
    [Tooltip("Son joue a l'ouverture du coffre.")]
    public AudioClip SonOuvertureCoffre;

    [Tooltip("Son joue a la fermeture du coffre.")]
    public AudioClip SonFermetureCoffre;

    [Header("Audio — Sons Speciaux Aleatoires")]
    [Tooltip("Un ou plusieurs clips joues aleatoirement pendant la mission.\n" +
             "Ex : braiment de l'ane, musique du camion de glace, bruit de rotor...\n" +
             "Laisser vide = aucun son special.")]
    public AudioClip[] SonsSpeciaux;

    [Tooltip("Temps minimum entre deux sons speciaux (secondes).")]
    public float IntervalleMinSecondes = 90f;

    [Tooltip("Temps maximum entre deux sons speciaux (secondes).")]
    public float IntervalleMaxSecondes = 150f;
}

// ============================================================
// PROPRIETAIRE
// ============================================================
[CreateAssetMenu(menuName = "BailiffCo/ProprietaireDef")]
public class ProprietaireDef : ScriptableObject
{
    [Header("Identite")]
    public string Nom;
    public int Age;
    public string Profession;
    public Sprite PhotoCartoon;
    public ProprietaireArchetypeType Archetype;

    [Header("Fiche joueur")]
    [TextArea] public string Loisirs;
    [TextArea] public string Backstory;
    public string TraitCaractere;
    public string CitationIndice;
    public int NiveauSecurite;
    public AnimalEspece[] AnimauxCompagnie;

    [Header("Comportement IA")]
    public float ParanoiaDepart = 0f;
    public float VitesseDeplacementNormal = 2.5f;
    public float VitesseDeplacementPanique = 4.5f;
    public bool AppelleAvocatAutomatique = false;
    public int PalierAppelAvocat = 76;
}

// ============================================================
// MISSION
// ============================================================
[CreateAssetMenu(menuName = "BailiffCo/MissionDef")]
public class MissionDef : ScriptableObject
{
    [Header("Identite")]
    public string NomMission;
    public int NumeroMission;
    public ProprietaireDef Proprietaire;
    public string NomSceneUnity;

    [Header("Objectif")]
    public ObjetDef[] BiensSaisis;
    public float ValeurQuotaMinimum;

    [Header("Seed")]
    public int SeedFixe = 0;

    [Header("Conditions de score")]
    public float BonusTempMaxSecondes = 600f;
    public int MaxObjetssCassesEtoile2 = 3;
}

// ============================================================
// RESULTAT DE MISSION
// ============================================================
[System.Serializable]
public class MissionResult
{
    public MissionDef Mission;
    public float ValeurTotaleRecuperee;
    public float ValeurQuotaCible;
    public int NombreObjetsRecuperes;
    public int NombreObjetsCasses;
    public int NombrePiegesDeclenches;
    public float TempsSecondes;
    public float ParanoiaMaxAtteinte;
    public bool MissionReussie;
    public int Etoiles;
    public float ArgentGagne;
}
