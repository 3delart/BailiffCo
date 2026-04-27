// ============================================================
// OutilDef.cs — Bailiff & Co
// Créer via : clic droit dans Project → Create → BailiffCo/OutilDef
// ============================================================
using UnityEngine;

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
