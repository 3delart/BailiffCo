// ============================================================
// GameEvents.cs — Bailiff & Co
// Définition de TOUS les événements du jeu sous forme de structs.
// Ajouter un event ici = le rendre disponible dans tout le projet
// sans aucune dépendance directe entre systèmes.
// ============================================================
using UnityEngine;

// ------ OBJETS ------

/// <summary>Un objet a été chargé dans le véhicule.</summary>
public struct OnObjetCharge
{
    public ObjetDef Objet;
    public float Valeur;        // Valeur réelle au moment du chargement
    public bool EstFragile;
}

/// <summary>Un objet a été cassé ou endommagé.</summary>
public struct OnObjetEndommage
{
    public ObjetDef Objet;
    public float ValeurPerdue;
    public Vector3 Position;
}

// ------ PARANOÏA ------

/// <summary>La paranoïa du proprio a changé.</summary>
public struct OnParanoiaChanged
{
    public float NouvelleValeur;   // 0–100
    public float AncienneValeur;
    public int NouveauPalier;      // 0=Calm, 1=Méfiant, 2=Inquiet, 3=Paniqué, 4=Furieux, 5=Obsessionnel
}

/// <summary>Une source de bruit a été émise (pas, porte, objet lâché…).</summary>
public struct OnBruitEmis
{
    public Vector3 Position;
    public float Portee;           // en mètres
    public NiveauBruit Niveau;
    public GameObject Source;
}

// ------ QUOTA ------

/// <summary>La valeur totale dans le véhicule a changé.</summary>
public struct OnQuotaChanged
{
    public float ValeurTotale;
    public float ValeurCible;
    public float PourcentageAtteint; // 0–1
}

/// <summary>Le quota minimum est atteint — mission validable.</summary>
public struct OnQuotaAtteint { }

/// <summary>Un seuil de pourcentage a été franchi (ex: 20% = proprio peut sortir).</summary>
public struct OnSeuilAtteint
{
    public float Pourcentage;  // ex: 0.2f pour 20%
}

// ------ PROPRIO ------

/// <summary>L'état de la state machine du proprio a changé.</summary>
public struct OnProprietaireStateChanged
{
    public ProprietaireState AncienEtat;
    public ProprietaireState NouvelEtat;
}

/// <summary>Le proprio sort de la maison vers le véhicule.</summary>
public struct OnProprietaireSortDeLaMaison { }

/// <summary>Le proprio a récupéré un objet dans le véhicule.</summary>
public struct OnProprietaireRecupereObjet
{
    public ObjetDef Objet;
    public float Valeur;
}

// ------ VÉHICULE ------

/// <summary>Le véhicule est attaqué (proprio ou voisin voleur).</summary>
public struct OnVehiculeAttaque
{
    public GameObject Attaquant;
    public bool EstLeProprietaire;
}

// ------ PIÈGES ------

/// <summary>Un piège a été déclenché.</summary>
public struct OnPiegeDeclenche
{
    public PiegeDef Piege;
    public Vector3 Position;
    public GameObject VicTime;     // qui l'a déclenché
}

// ------ MISSION ------

/// <summary>Une mission démarre.</summary>
public struct OnMissionDemarree
{
    public MissionDef Mission;
    public int Seed;
}

/// <summary>La mission est terminée (quota atteint, expulsion ou départ volontaire).</summary>
public struct OnMissionTerminee
{
    public MissionResult Resultat;
}

// ------ ANIMAUX ------

/// <summary>Un animal aboie ou fait du bruit.</summary>
public struct OnAnimalAboie
{
    public Vector3 Position;
    public float Intensite;        // 0–1 : chihuahua vs berger allemand
    public AnimalEspece Espece;
}

/// <summary>Le perroquet a dit quelque chose.</summary>
public struct OnPerroquetParle
{
    public string Phrase;
    public bool EstIndice;         // si true, CachetteSystem doit écouter
}

// ------ SCANNER ------

/// <summary>Un scan rayon X a révélé des objets cachés.</summary>
public struct OnScanEffectue
{
    public Vector3 PositionScan;
    public ObjetDef[] ObjetsRévéles;
}

// ------ TIMER URGENCE ------

/// <summary>La police a été appelée — timer de fin.</summary>
public struct OnTimerUrgenceDéclenche
{
    public float DureeSecondes;    // temps avant expulsion
}


/// <summary>Le joueur interagit avec la porte conducteur — demande confirmation de fin de mission.</summary>
public struct OnDemandeFinMission { }
 
/// <summary>Réponse du joueur au popup de confirmation de départ.</summary>
public struct OnConfirmationDepart
{
    public bool Confirme; // true = Oui, false = Non
}
 
/// <summary>Déclenche le fondu noir — HUDSystem gère l'animation.</summary>
public struct OnFondNoir
{
    public float DureeSecondes;
}
 