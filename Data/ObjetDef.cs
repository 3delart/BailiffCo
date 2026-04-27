// ============================================================
// ObjetDef.cs — Bailiff & Co
// Créer via : clic droit dans Project → Create → BailiffCo/ObjetDef
// ============================================================
using UnityEngine;

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
