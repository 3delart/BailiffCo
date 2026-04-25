// ============================================================
// ZoneCoffreTrigger.cs — Bailiff & Co
// À mettre sur le GameObject enfant ZoneCoffre.
// Relaie les événements trigger à Vehicule.cs sur le parent.
// Le coffre doit être ouvert pour que les objets soient acceptés
// (Vehicule.cs active/désactive ce collider selon l'état du coffre).
// ============================================================
using UnityEngine;

public class ZoneCoffreTrigger : MonoBehaviour
{
    private Vehicule _vehicule;

    private void Awake() => _vehicule = GetComponentInParent<Vehicule>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<ObjetValeur>(out var obj))
            _vehicule?.AjouterObjetZone(obj);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<ObjetValeur>(out var obj))
            _vehicule?.RetirerObjetZone(obj);
    }
}
