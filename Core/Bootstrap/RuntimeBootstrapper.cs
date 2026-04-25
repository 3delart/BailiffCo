// RuntimeBootstrapper.cs
// Peu importe la scène ouverte dans l'éditeur,
// appuyer sur Play démarre TOUJOURS depuis Bootstrap.
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class RuntimeBootstrapper
{
    static RuntimeBootstrapper()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        // Juste avant de passer en Play
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            // Si on n'est pas déjà sur Bootstrap, on y va
            if (EditorSceneManager.GetActiveScene().name != SceneNames.BOOTSTRAP)
            {
                // Propose de sauvegarder la scène courante si elle a des modifications
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

                // Charge Bootstrap comme scène active pour le Play Mode
                EditorSceneManager.OpenScene("Assets/Scenes/Bootstrap.unity");

                Debug.Log("[RuntimeBootstrapper] Démarrage forcé depuis Bootstrap.");
            }
        }
    }
}
#endif
