// ============================================================
// ProprietaireDef.cs — Bailiff & Co
// Créer via : clic droit dans Project → Create → BailiffCo/ProprietaireDef
// ============================================================
using UnityEngine;

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
