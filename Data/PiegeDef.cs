// ============================================================
// PiegeDef.cs — Bailiff & Co
// Créer via : clic droit dans Project → Create → BailiffCo/PiegeDef
// ============================================================
using UnityEngine;

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
