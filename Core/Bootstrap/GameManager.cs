// ============================================================
// GameManager.cs — Bailiff & Co
// Singleton persistant (DontDestroyOnLoad).
// Seul objet qui survit entre toutes les scènes.
// Responsabilités :
//   - Stocker la MissionDef sélectionnée dans le Hub
//   - Orchestrer les transitions de scènes via SceneLoader
//   - Stocker la personnalisation du personnage (CharacterCustomizationData)
//   - Donner accès au SaveData global
//
// RÈGLE : Le GameManager ne contient PAS de logique de jeu.
// Il transporte des données et délègue les transitions.
//
// SETUP UNITY :
//   1. Créer un GameObject "GameManager" dans la scène Bootstrap
//   2. Y attacher ce script
//   3. La scène Bootstrap doit être en index 0 dans Build Settings
// ============================================================
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // ================================================================
    // SINGLETON
    // ================================================================

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitialiserDonnees();
    }

    // ================================================================
    // DONNÉES PERSISTANTES
    // ================================================================

    /// <summary>Mission sélectionnée dans le Hub.</summary>
    public MissionDef MissionSelectionnee { get; private set; }

    /// <summary>Argent total du joueur (persiste entre missions).</summary>
    public float Argent { get; private set; } = 0f;

    /// <summary>Numéro de la dernière mission complétée.</summary>
    public int DerniereMissionCompletee { get; private set; } = 0;

    /// <summary>
    /// Personnalisation du personnage — persiste entre toutes les scènes.
    /// Initialisée avec des valeurs par défaut (tout à 0).
    /// Modifiée uniquement via SauvegarderPersonnalisation().
    /// Lue par CharacterPreviewController et CustomizationUI.
    /// </summary>
    public CharacterCustomizationData Personnalisation { get; private set; }

    // ================================================================
    // INITIALISATION
    // ================================================================

    private void InitialiserDonnees()
    {
        // TODO : charger depuis SaveSystem quand il sera implémenté
        Argent                   = 0f;
        DerniereMissionCompletee = 0;
        MissionSelectionnee      = null;
        Personnalisation         = new CharacterCustomizationData(); // valeurs par défaut = tout à 0
    }

    // ================================================================
    // API — PERSONNALISATION
    // ================================================================

    /// <summary>
    /// Sauvegarde les choix de personnalisation confirmés par le joueur.
    /// Appelé depuis CustomizationUI.Confirmer().
    /// Stocke une copie pour éviter les effets de bord.
    /// </summary>
    public void SauvegarderPersonnalisation(CharacterCustomizationData data)
    {
        if (data == null)
        {
            Debug.LogWarning("[GameManager] SauvegarderPersonnalisation : data est null, ignoré.");
            return;
        }

        Personnalisation = data.Clone();
        Debug.Log("[GameManager] Personnalisation sauvegardée.");

        // TODO : persister via SaveSystem
    }

    // ================================================================
    // API — MISSION
    // ================================================================

    public void LancerMission(MissionDef mission)
    {
        if (mission == null)
        {
            Debug.LogError("[GameManager] LancerMission : MissionDef est null !");
            return;
        }

        MissionSelectionnee = mission;
        Debug.Log($"[GameManager] Mission sélectionnée : {mission.NomMission} → {mission.NomSceneUnity}");

        SceneLoader.Instance.ChargerScene(mission.NomSceneUnity);
    }

    public void TerminerMission(MissionResult resultat)
    {
        if (resultat.MissionReussie)
        {
            Argent += resultat.ArgentGagne;
            if (MissionSelectionnee != null &&
                MissionSelectionnee.NumeroMission > DerniereMissionCompletee)
                DerniereMissionCompletee = MissionSelectionnee.NumeroMission;
        }

        MissionSelectionnee = null;

        Debug.Log($"[GameManager] Mission terminée — Argent total : {Argent:N0} €");

        // TODO : sauvegarder via SaveSystem

        SceneLoader.Instance.ChargerScene(SceneNames.HUB, avecFondu: true);
    }

    // ================================================================
    // API — NAVIGATION
    // ================================================================

    public void AllerAuMenu() =>
        SceneLoader.Instance.ChargerScene(SceneNames.MENU, avecFondu: true);

    public void AllerAuHub() =>
        SceneLoader.Instance.ChargerScene(SceneNames.HUB, avecFondu: true);

    public void QuitterJeu()
    {
        Debug.Log("[GameManager] Quitter le jeu.");
        Application.Quit();
    }

    // ================================================================
    // API — ARGENT
    // ================================================================

    public bool PeutPayer(float montant) => Argent >= montant;

    public void Debiter(float montant)
    {
        Argent = Mathf.Max(0f, Argent - montant);
    }

    public void Crediter(float montant)
    {
        Argent += montant;
    }
}



