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

// ============================================================
// OBJET DE VALEUR
// ============================================================
[CreateAssetMenu(menuName = "BailiffCo/ObjetDef")]
public class ObjetDef : ScriptableObject
{
    [Header("Identité")]
    public string NomObjet;
    [TextArea] public string Description;
    public Sprite IconeUI;
    public GameObject Prefab;

    [Header("Valeur")]
    public float ValeurMin = 500f;
    public float ValeurMax = 5000f;
    // La valeur exacte est tirée au seed de mission
    // Elle n'est révélée qu'après scan téléphone

    [Header("Physique")]
    public float Poids = 1f;          // 1 = normal, 3+ = lourd (ralentit)
    public bool EstFragile = false;
    public bool NecessiteDeuxJoueurs = false;
    public bool EstTresGros = false;   // véhicule / piano — remorque ou signal

    [Header("Scan")]
    public string NomCompletApresScan;  // révélé après 3 sec de scan téléphone
    public string AnneeEdition;
}

// ============================================================
// CACHETTE
// ============================================================
[CreateAssetMenu(menuName = "BailiffCo/CachetteDef")]
public class CachetteDef : ScriptableObject
{
    public TypeCachette Type;
    public bool EstDeplacable;          // le proprio peut la vider et la déplacer
    public string[] TagsSpawn;          // tags Unity sur les GameObjects compatibles
    public string SonOuverture;         // nom du clip AudioSystem
    public GameObject PrefabIndicateurUV; // empreinte visible avec lampe UV
}

// ============================================================
// PIÈGE
// ============================================================
[CreateAssetMenu(menuName = "BailiffCo/PiegeDef")]
public class PiegeDef : ScriptableObject
{
    public TypePiege Type;
    public string NomAffiche;

    [Header("Déclenchement")]
    public string TagDeclencheur;       // ex: "Player", "Player|Voisin"
    public float RayonDetection = 0.5f;

    [Header("Effets")]
    public float DureesEffetSecondes = 5f;
    public float ModifParanoiaSurDeclenchement = 10f;
    public bool AlerteVoisins = false;
    public bool AlertePolice = false;

    [Header("Lisibilité")]
    public string IndicateurVisuelDescription; // pour les designers
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
    public bool EstOffert = false;      // badge, téléphone

    [Header("Niveaux d'upgrade (3 max)")]
    public OutilNiveau[] Niveaux = new OutilNiveau[3];

    [System.Serializable]
    public struct OutilNiveau
    {
        public int PrixUpgrade;
        public string DescriptionEffet;
        public float ValeurNumerique;   // durée, charges, portée selon l'outil
    }
}

// ============================================================
// VÉHICULE
// ============================================================
[CreateAssetMenu(menuName = "BailiffCo/VehiculeDef")]
public class VehiculeDef : ScriptableObject
{
    public string NomVehicule;
    public GameObject Prefab;
    public int CapaciteObjets;
    public float DureeFermetureCoffreSecondes; // 0 = coffre ouvert (vélo)
    public bool CoffreVisible;         // voisin et proprio peuvent y accéder librement

    [TextArea] public string AvantageDescription;
    [TextArea] public string InconvenientDescription;

    [Header("Déblocage")]
    public int NumeroMissionRequis = 0; // 0 = disponible dès départ
    public string ConditionSpeciale;    // ex: "Terminer sans outil"
}

// ============================================================
// PROPRIÉTAIRE
// ============================================================
[CreateAssetMenu(menuName = "BailiffCo/ProprietaireDef")]
public class ProprietaireDef : ScriptableObject
{
    [Header("Identité")]
    public string Nom;
    public int Age;
    public string Profession;
    public Sprite PhotoCartoon;
    public ProprietaireArchetypeType Archetype;

    [Header("Fiche joueur")]
    [TextArea] public string Loisirs;
    [TextArea] public string Backstory;
    public string TraitCaractere;       // 1 mot : Paranoïaque, Rusé, Négligent...
    public string CitationIndice;       // indice ambigu sur cachette principale
    public int NiveauSecurite;          // 1–5 étoiles
    public AnimalEspece[] AnimauxCompagnie;

    [Header("Comportement IA")]
    public float ParanoiaDepart = 0f;   // 0–100
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
    [Header("Identité")]
    public string NomMission;
    public int NumeroMission;
    public ProprietaireDef Proprietaire;
    public string NomSceneUnity;        // nom exact de la scène Unity à charger

    [Header("Objectif")]
    public ObjetDef[] BiensSaisis;      // liste officielle des objets à récupérer
    public float ValeurQuotaMinimum;    // calculée auto si 0 (50% de la valeur totale)

    [Header("Seed")]
    public int SeedFixe = 0;            // 0 = aléatoire à chaque partie
    // Même seed = même disposition de cachettes et d'objets

    [Header("Conditions de score")]
    public float BonusTempMaxSecondes = 600f;  // 10 min pour ★★★
    public int MaxObjetssCassesEtoile2 = 3;
}

// ============================================================
// RÉSULTAT DE MISSION (pas un SO — structure de données)
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
    public int Etoiles;                 // 1, 2 ou 3
    public float ArgentGagne;
}
