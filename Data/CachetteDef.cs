// ============================================================
// CachetteDef.cs — Bailiff & Co
// Créer via : clic droit dans Project → Create → BailiffCo/CachetteDef
// ============================================================
using UnityEngine;

[CreateAssetMenu(menuName = "BailiffCo/CachetteDef")]
public class CachetteDef : ScriptableObject
{
    public TypeCachette Type;
    public bool EstDeplacable;
    public string[] TagsSpawn;
    public string SonOuverture;
    public GameObject PrefabIndicateurUV;
}
