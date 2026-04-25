// ============================================================
// CharacterCustomizationData.cs — Bailiff & Co
// Données de personnalisation du joueur.
// Stockées dans GameManager, persistées via SaveSystem (V2).
//
// USAGE :
//   var data = GameManager.Instance.Personnalisation;
//   data.IndexCoiffure = 2;
// ============================================================

[System.Serializable]
public class CharacterCustomizationData
{
    // ── VISAGE ───────────────────────────────────────────────
    public int IndexYeux        = 0;
    public int IndexNez         = 0;
    public int IndexBouche      = 0;
    public int IndexSourceils   = 0;
    public int IndexBarbe       = 0;  // 0 = Aucune

    // ── CHEVEUX ──────────────────────────────────────────────
    public int IndexCoiffure        = 0;  // 0 = Chauve
    public int IndexCouleurCheveux  = 0;

    // ── TENUE ────────────────────────────────────────────────
    public int IndexTenue       = 0;
    public int IndexChapeau     = 0;  // 0 = Aucun
    public int IndexAccessoire  = 0;  // 0 = Aucun

    // ── PEAU ─────────────────────────────────────────────────
    public int IndexCouleurPeau = 0;

    // ================================================================
    // UTILITAIRES
    // ================================================================

    /// <summary>Retourne une copie profonde — pour le panneau de perso
    /// (on travaille sur la copie, on sauvegarde seulement au Confirmer).</summary>
    public CharacterCustomizationData Clone()
    {
        return new CharacterCustomizationData
        {
            IndexYeux           = IndexYeux,
            IndexNez            = IndexNez,
            IndexBouche         = IndexBouche,
            IndexSourceils      = IndexSourceils,
            IndexBarbe          = IndexBarbe,
            IndexCoiffure       = IndexCoiffure,
            IndexCouleurCheveux = IndexCouleurCheveux,
            IndexTenue          = IndexTenue,
            IndexChapeau        = IndexChapeau,
            IndexAccessoire     = IndexAccessoire,
            IndexCouleurPeau    = IndexCouleurPeau
        };
    }

    /// <summary>Copie les valeurs d'une autre instance dans celle-ci.</summary>
    public void CopierDepuis(CharacterCustomizationData source)
    {
        IndexYeux           = source.IndexYeux;
        IndexNez            = source.IndexNez;
        IndexBouche         = source.IndexBouche;
        IndexSourceils      = source.IndexSourceils;
        IndexBarbe          = source.IndexBarbe;
        IndexCoiffure       = source.IndexCoiffure;
        IndexCouleurCheveux = source.IndexCouleurCheveux;
        IndexTenue          = source.IndexTenue;
        IndexChapeau        = source.IndexChapeau;
        IndexAccessoire     = source.IndexAccessoire;
        IndexCouleurPeau    = source.IndexCouleurPeau;
    }
}
