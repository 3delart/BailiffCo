// ============================================================
// VehiculeDef.cs — Bailiff & Co
// Créer via : clic droit dans Project → Create → BailiffCo/VehiculeDef
// ============================================================
using UnityEngine;

[CreateAssetMenu(menuName = "BailiffCo/VehiculeDef")]
public class VehiculeDef : ScriptableObject
{
    // -- Identite ---------------------------------------------
    [Header("Identite")]
    public string       NomVehicule;
    public TypeVehicule Type;
    [TextArea(2, 4)]
    public string       Description;
    public Sprite       IllustrationUI;
    public GameObject   Prefab;

    // -- Location ---------------------------------------------
    [Header("Location")]
    [Tooltip("Prix de location pour une mission. 0 = gratuit (velo cargo).")]
    public float PrixLocation = 0f;

    // -- Coffre -----------------------------------------------
    [Header("Coffre")]
    [Tooltip("Nombre maximum d'objets charges dans ce vehicule.")]
    public int CapaciteObjets = 6;

    [Tooltip("0 = coffre toujours ouvert. > 0 = coffre fermable avec animation.")]
    public float DureeFermetureCoffreSecondes = 0.5f;

    [Tooltip("Temps en secondes que met le proprio pour forcer le coffre ferme. 0 = acces immediat.")]
    public float TempsForcageCoffreSecondes = 12f;

    // -- Textes Popup Hub -------------------------------------
    [Header("Textes Popup Hub")]
    [TextArea(2, 4)]
    public string AvantageDescription;

    [TextArea(2, 4)]
    public string InconvenientDescription;

    [TextArea(1, 3)]
    public string AstuceDescription;

    // -- Audio ------------------------------------------------
    [Header("Audio — Coffre")]
    [Tooltip("Son joue a l'ouverture du coffre.")]
    public AudioClip SonOuvertureCoffre;

    [Tooltip("Son joue a la fermeture du coffre.")]
    public AudioClip SonFermetureCoffre;

    [Header("Audio — Sons Speciaux Aleatoires")]
    [Tooltip("Un ou plusieurs clips joues aleatoirement pendant la mission.\n" +
             "Ex : braiment de l'ane, musique du camion de glace, bruit de rotor...\n" +
             "Laisser vide = aucun son special.")]
    public AudioClip[] SonsSpeciaux;

    [Tooltip("Temps minimum entre deux sons speciaux (secondes).")]
    public float IntervalleMinSecondes = 90f;

    [Tooltip("Temps maximum entre deux sons speciaux (secondes).")]
    public float IntervalleMaxSecondes = 150f;
}
