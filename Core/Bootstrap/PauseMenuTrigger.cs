// ============================================================
// PauseMenuTrigger.cs — Bailiff & Co
// Toujours actif — écoute [Echap] et ouvre/ferme le PauseMenu.
// À mettre sur un GameObject SÉPARÉ du PauseMenu, toujours actif.
//
// SETUP :
//   Canvas PauseMenu (Sort Order 100)
//   ├── PauseMenuTrigger  ← CE SCRIPT ici (toujours actif)
//   └── PauseMenu         ← désactivé au départ
// ============================================================
using UnityEngine;

public class PauseMenuTrigger : MonoBehaviour
{
    [SerializeField] private PauseMenu _pauseMenu;

    private void Awake()
    {
        if (_pauseMenu == null)
            _pauseMenu = GetComponentInChildren<PauseMenu>(includeInactive: true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_pauseMenu == null) return;

            if (_pauseMenu.EstOuvert) _pauseMenu.Fermer();
            else                      _pauseMenu.Ouvrir();
        }
    }
}
