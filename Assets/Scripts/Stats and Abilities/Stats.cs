using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stats : MonoBehaviour
{
    public Canvas StatDisplay;

    public int focus;//Unique Spell casting ability that enables casting of more spells, allows you to float
    //Level up by hitting perfects in charging abilities
    public int agression;//can execute a bigger veriaty of combos which will increase dammage, allows you to attack while moving
    //Level up by hitting combos
    public int patience;//Allows you to counter spells and attacks, but requires the affinitylevel of atleast B to parry, A to counter and S to destinguish
    //Leveled up by parying, countering or destinguishing.

    public enum Colour
    {
        Blue,//chance to stun an enimy on attack some blue spells can root enimies//Shield Alys//Spells have large area but long casting time
        Red,//stacks a bleed on attack//Strength to alys//Spells have short casting times and smaller area
        Yellow,//Makes the enimy vaulnerable to next attack//Stamina to Alys//spells do less damage but can peirce
        Green,//chance to stun an enimy on attack some blue spells can root enimies and marks enimies//Heals and shields alys
        Orange,//Stacks burning on attack//Stamina and strength to alies//Spells have short casting times and smaller area and can stack
        Purple,//can stun and does decaying dammage while stunned//Shield and Strength to Alies
    }

    public Dictionary<Colour, int> AffinityProgress;
    //Affinity Progress Checkpoints: E: 100, D: 200, C: 400, B: 800, A: 1600, S: 3200
    //You lose your points down to highest check point if you start using a different affinity
    //You can combine any base affinities Yellow, Blue or red to create the other affinities once you have achieved at leas B in the base
    //affinities. This will then the checkpoint of the new affinity at one lower check point than the lowest of the two affinities
    //Affinity changes depending on how much you use an affinity,
    public Colour affinity;
    public int Health;
    public int MaxHealth;
    public int Stamina;
    public int MaxStamina;
    public int Shield;
    public int MaxShield;
    public float strength;
    public float Speed;
    enum SpellType
    {
        Arrow,
        Ball,
        Circle,
        Cone,
        Wall,
        Targeted
    }
    struct Spell
    {
        int dammage;
        int radius;
        int range;
        SpellType type;
    }
    struct Mask
    {
        Colour[] Affinities;
        Spell[] Spells;
    }
    //The affinity with the max level becomes the affinity of that mask
    //only when transforming does the the bearer enherrit all spells and all affinities, but are limmited to only these.
    //Masks can be upgraded wih the cost of it becoming cursed, Only when summoned and defeated can the broken mask be collected and
    //repaired without it having a curse
    //Max of 4 affinities and 8 spells
    //A non cursed spell when summoned will be your minion and will fight for you. If the summon is a higher class than you. It will simply
    //wonder arround untill you become worthy of his obediance. Max summons depend on the players Summoner Level
    Spell[] Spells;
    Spell[] CurrentSpells;
    //Spells unlock with Affinity levels. You can learn a spell by finding a mask that contains the enscryption for that spell.

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log(other.tag);
    }
}
