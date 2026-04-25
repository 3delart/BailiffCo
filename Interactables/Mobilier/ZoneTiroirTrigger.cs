// ============================================================
// ZoneTiroirTrigger.cs — Bailiff & Co
// À mettre sur un GameObject ENFANT du tiroir (ex: "ZoneTiroir").
// Le collider trigger détecte les ObjetValeur qui entrent/sortent.
// TiroirInteractable active/désactive ce collider selon l'état.
//
// HIÉRARCHIE RECOMMANDÉE :
//   Commode (MeshRenderer, pas de script)
//   └── Tiroir (TiroirInteractable)
//       └── ZoneTiroir (ZoneTiroirTrigger + BoxCollider IsTrigger)
//           [les ObjetValeur en scène sont mis ici comme enfants]
//
// FONCTIONNEMENT :
//   - Quand le tiroir est FERMÉ : le trigger est désactivé.
//     Les objets enfants bougent avec le tiroir (parentés).
//     Ils ne sont PAS interactables (CanInteract retourne false
//     car ObjetValeur.CanInteract vérifie si _carry == null —
//     mais PlayerInteractor ne les verra pas : LayerMask).
//
//   - Quand le tiroir s'OUVRE : le trigger s'active.
//     TiroirInteractable libère les objets (isKinematic = false,
//     dé-parentage optionnel). Le joueur peut les saisir avec E.
//
// NOTE SUR LES OBJETS :
//   Les ObjetValeur placés dans un tiroir doivent avoir leur
//   Rigidbody.isKinematic = true en départ pour ne pas tomber
//   avant l'ouverture. TiroirInteractable s'occupe de les libérer.
// ============================================================
using UnityEngine;

public class ZoneTiroirTrigger : MonoBehaviour
{
    private TiroirInteractable _tiroir;

    private void Awake()
    {
        _tiroir = GetComponentInParent<TiroirInteractable>();

        // S'assure que le trigger est désactivé au départ (tiroir fermé)
        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Rien à faire — les objets sont déjà enfants du tiroir en scène.
        // Ce trigger sert uniquement à détecter si un joueur dépose
        // un objet dans le tiroir ouvert (usage futur).
    }

    private void OnTriggerExit(Collider other)
    {
        // Idem — suivi passif pour extension future.
    }

    /// <summary>Appelé par TiroirInteractable quand le tiroir finit de s'ouvrir.</summary>
    public void Activer()
    {
        var col = GetComponent<Collider>();
        if (col != null) col.enabled = true;
    }

    /// <summary>Appelé par TiroirInteractable quand le tiroir commence à se fermer.</summary>
    public void Desactiver()
    {
        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }
}
