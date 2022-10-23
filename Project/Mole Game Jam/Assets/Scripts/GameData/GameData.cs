using UnityEngine;

[CreateAssetMenu(fileName = "GameData", menuName = "ScriptableObjects/GameDataScriptableObject")]
public class GameData : ScriptableObject
{
    [Header("Default Player Settings")]
    public float PlayerHealth;
    public float PlayerStamina;

    [Tooltip("Player's default movement speed")]
    public float MovementSpeed;

    public float[] EncumberedSpeeds; 

    [Tooltip("Duration it takes to dig a hole/worm")]
    public float DigDuration;
    [Tooltip("Player's default digging speed")]
    public float DigStaminaCost;
    [Tooltip("Player's default burrow speed")]
    public float PlayerBurrowSpeed;
    [Tooltip("Player's default digging speed")]
    public float BurrowStaminaCost;

    [Tooltip("Duration of Delay before stamina begins automatically.")]
    public float StaminaRechargeDelay;

    [Header("Gameplay Settings")]
    [Tooltip("Base value for how fast baby hunger value goes down")]
    public float MoleBabyHungerScale;

    [Tooltip("Duration a year last")]
    public float CalandarDuration;
}
