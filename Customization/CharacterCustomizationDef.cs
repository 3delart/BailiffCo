// ============================================================
// CharacterCustomizationDef.cs — Bailiff & Co
// ScriptableObject central listant toutes les options de
// personnalisation. 4 onglets, 12 sous-catégories.
//
// CRÉER : clic droit dans Project → Create → BailiffCo/CharacterCustomizationDef
// Un seul asset à remplir dans l'Inspector, référencé par
// CharacterPreviewController et CustomizationUI.
// ============================================================
using UnityEngine;

[CreateAssetMenu(menuName = "BailiffCo/CharacterCustomizationDef")]
public class CharacterCustomizationDef : ScriptableObject
{
    // ── ONGLET 1 : VISAGE ────────────────────────────────────
    [Header("=== VISAGE ===")]
    public FaceOption[] Yeux;
    public FaceOption[] Nez;
    public FaceOption[] Bouches;
    public FaceOption[] Sourcils;
    public FaceOption[] Barbes;       // index 0 = "Aucune" (NomObjetEnfant vide)

    // ── ONGLET 2 : CHEVEUX ───────────────────────────────────
    [Header("=== CHEVEUX ===")]
    public HairOption[] Coiffures;    // index 0 = "Chauve / Aucun"
    public Color[]      CouleursCheveux;

    // ── ONGLET 3 : TENUE ─────────────────────────────────────
    [Header("=== TENUE ===")]
    public OutfitOption[]    Tenues;
    public HatOption[]       Chapeaux;     // index 0 = "Aucun"
    public AccessoryOption[] Accessoires;  // index 0 = "Aucun"

    // ── ONGLET 4 : PEAU ──────────────────────────────────────
    [Header("=== PEAU ===")]
    public Color[] CouleursPeau;
}

// ================================================================
// TYPES D'OPTIONS
// ================================================================

/// <summary>
/// Option de visage générique : yeux, nez, bouche, sourcils, barbe.
/// On swap soit par GameObject enfant (mesh swap),
/// soit par matériau sur le FaceRenderer.
/// </summary>
[System.Serializable]
public class FaceOption
{
    public string  Nom;
    public Sprite  IconeUI;
    [Tooltip("Nom du GameObject enfant à activer sur le mannequin.\n" +
             "Ex: 'Eyes_Almond', 'Nose_Large', 'Beard_Full'\n" +
             "Laisser vide = aucun (ex: Barbe 'Aucune')")]
    public string  NomObjetEnfant;
    [Tooltip("Matériau optionnel swappé sur le slot correspondant du FaceRenderer")]
    public Material Materiau;
}

/// <summary>Option de coiffure.</summary>
[System.Serializable]
public class HairOption
{
    public string Nom;
    public Sprite IconeUI;
    [Tooltip("Nom du GameObject enfant à activer.\n" +
             "Ex: 'Hair_Short', 'Hair_Long', 'Hair_Afro'\n" +
             "Laisser vide = chauve")]
    public string NomObjetEnfant;
}

/// <summary>Option de tenue complète (haut + bas ou combinaison).</summary>
[System.Serializable]
public class OutfitOption
{
    public string    Nom;
    public Sprite    IconeUI;
    [Tooltip("Matériaux appliqués sur le SkinnedMeshRenderer corps, dans l'ordre des slots.\n" +
             "Slot 0 = corps principal, slot 1 = détails, etc.")]
    public Material[] Materiaux;
    [Tooltip("GameObject enfant accessoire de tenue à activer (capuche, ceinture…).\n" +
             "Laisser vide si la tenue n'a pas d'accessoire 3D.")]
    public string    NomObjetEnfant;
}

/// <summary>Option de chapeau / couvre-chef.</summary>
[System.Serializable]
public class HatOption
{
    public string Nom;
    public Sprite IconeUI;
    [Tooltip("Nom du GameObject enfant chapeau.\n" +
             "Ex: 'Hat_Cap', 'Hat_Beanie', 'Hat_Fedora'\n" +
             "Laisser vide = aucun chapeau")]
    public string NomObjetEnfant;
    [Tooltip("Si true, désactive le mesh de cheveux quand ce chapeau est équipé.\n" +
             "À activer pour les chapeaux qui couvrent totalement la tête.")]
    public bool   CacheCheveux;
}

/// <summary>Option d'accessoire : lunettes, boucles d'oreilles, montre, collier…</summary>
[System.Serializable]
public class AccessoryOption
{
    public string Nom;
    public Sprite IconeUI;
    [Tooltip("Nom du GameObject enfant accessoire.\n" +
             "Ex: 'Glasses_Round', 'Earring_Hoop', 'Watch_Gold'\n" +
             "Laisser vide = aucun accessoire")]
    public string NomObjetEnfant;
}
