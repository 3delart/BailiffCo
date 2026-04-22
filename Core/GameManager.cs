// ============================================================
// GameManager.cs — Bailiff & Co
// Singleton persistant (DontDestroyOnLoad).
// Seul objet qui survit entre toutes les scènes.
// Responsabilités :
//   - Stocker la MissionDef sélectionnée dans le Hub
//   - Orchestrer les transitions de scènes via SceneLoader
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
        // Pattern singleton : une seule instance, persiste entre scènes
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance   = this;
        DontDestroyOnLoad(gameObject);

        InitialiserDonnees();
    }

    // ================================================================
    // DONNÉES PERSISTANTES
    // ================================================================

    /// <summary>
    /// Mission sélectionnée dans le Hub.
    /// MissionSystem la lit au Start() de la scène Mission.
    /// </summary>
    public MissionDef MissionSelectionnee { get; private set; }

    /// <summary>Argent total du joueur (persiste entre missions).</summary>
    public float Argent { get; private set; } = 0f;

    /// <summary>Numéro de la dernière mission complétée.</summary>
    public int DerniereMissionCompletee { get; private set; } = 0;

    // ================================================================
    // INITIALISATION
    // ================================================================

    private void InitialiserDonnees()
    {
        // TODO : charger depuis SaveSystem quand il sera implémenté
        Argent                  = 0f;
        DerniereMissionCompletee = 0;
        MissionSelectionnee     = null;
    }

    // ================================================================
    // API — SÉLECTION DE MISSION (appelé depuis le Hub)
    // ================================================================

    /// <summary>
    /// Enregistre la mission choisie et lance le chargement de la scène.
    /// À appeler depuis le Hub quand le joueur confirme sa mission.
    /// </summary>
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

    /// <summary>
    /// Appelé par MissionSystem quand la mission est terminée.
    /// Enregistre le résultat et retourne au Hub.
    /// </summary>
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
    // API — NAVIGATION GÉNÉRALE
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
    // API — ARGENT (pour la boutique)
    // ================================================================

    /// <summary>Retourne true si le joueur peut payer le montant.</summary>
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
