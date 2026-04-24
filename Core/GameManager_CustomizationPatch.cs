// ============================================================
// GameManager_CustomizationPatch.cs — Bailiff & Co
// PATCH à intégrer dans ton GameManager.cs existant.
//
// INSTRUCTIONS :
//   1. Ouvre ton GameManager.cs
//   2. Ajoute les blocs marqués [AJOUTER] aux endroits indiqués.
//   3. Ce fichier n'est PAS à importer tel quel dans Unity —
//      c'est un guide de patch.
// ============================================================

/*
─────────────────────────────────────────────────────────────────
[AJOUTER] Dans la région "DONNÉES PERSISTANTES" de GameManager.cs
─────────────────────────────────────────────────────────────────

    /// <summary>Données de personnalisation du personnage.</summary>
    public CharacterCustomizationData Personnalisation { get; private set; }
        = new CharacterCustomizationData();


─────────────────────────────────────────────────────────────────
[AJOUTER] Dans InitialiserDonnees() de GameManager.cs
─────────────────────────────────────────────────────────────────

    private void InitialiserDonnees()
    {
        Argent                   = 0f;
        DerniereMissionCompletee = 0;
        MissionSelectionnee      = null;
        Personnalisation         = new CharacterCustomizationData(); // ← AJOUTER
    }


─────────────────────────────────────────────────────────────────
[AJOUTER] Nouvelle méthode publique dans GameManager.cs
─────────────────────────────────────────────────────────────────

    // ================================================================
    // API — PERSONNALISATION
    // ================================================================

    /// <summary>
    /// Sauvegarde les choix de personnalisation du joueur.
    /// Appelé par CustomizationUI.Confirmer().
    /// </summary>
    public void SauvegarderPersonnalisation(CharacterCustomizationData data)
    {
        if (data == null) return;
        Personnalisation.CopierDepuis(data);
        Debug.Log("[GameManager] Personnalisation sauvegardée.");
        // TODO V2 : appeler SaveSystem.Sauvegarder() ici
    }


─────────────────────────────────────────────────────────────────
[AJOUTER] Dans SceneNames.cs — nouvelle constante de scène
─────────────────────────────────────────────────────────────────

    public const string PERSONNALISATION = "CharacterCustomization";


─────────────────────────────────────────────────────────────────
[MODIFIER] Dans MenuUI.cs — brancher le bouton Personnalisation
─────────────────────────────────────────────────────────────────

    // Cherche le bouton _boutonPersonnalisation (déjà visible dans ton screenshot)
    // et ajoute dans Start() :

    _boutonPersonnalisation?.onClick.AddListener(OnPersonnalisation);

    // Puis la méthode :
    private void OnPersonnalisation()
    {
        SceneLoader.Instance?.ChargerScene(SceneNames.PERSONNALISATION, avecFondu: true);
    }

*/

// ============================================================
// FIN DU PATCH
// ============================================================
