// ============================================================
// MissionDef.cs — Bailiff & Co
// Créer via : clic droit dans Project → Create → BailiffCo/MissionDef
// ============================================================
using UnityEngine;

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
