// ============================================================
// Enums.cs — Bailiff & Co
// Tous les enums partagés + MissionResult (non-ScriptableObject)
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
