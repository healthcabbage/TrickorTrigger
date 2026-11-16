using System.Collections.Generic;
using UnityEngine;

public class PlayerAuxiliaryWeaponController : MonoBehaviour
{
    public const int MaxEquippedWeapons = 2;
    public const int InitialChoiceCount = 1;

    [SerializeField] private AuxiliaryWeaponStats[] weaponStates = new AuxiliaryWeaponStats[4];
    private AuxiliaryWeapon[] availableWeapons;
    private readonly List<AuxiliaryWeapon> equippedWeapons = new List<AuxiliaryWeapon>(MaxEquippedWeapons);
    private bool gameplayActive;

    public AuxiliaryWeapon[] AvailableWeapons => availableWeapons;
    public int EquippedCount => equippedWeapons.Count;
    public IReadOnlyList<AuxiliaryWeapon> EquippedWeapons => equippedWeapons;

    public bool IsEquipped(AuxiliaryWeapon weapon)
    {
        return equippedWeapons.Contains(weapon);
    }

    private void Awake()
    {
        PlayerController player = GetComponent<PlayerController>();
        Pumpkin pumpkin = FindObjectOfType<Pumpkin>();

        availableWeapons = new AuxiliaryWeapon[]
        {
            EnsureWeapon<OrbitingAttackAuxiliaryWeapon>(),
            EnsureWeapon<PumpkinRandomProjectileAuxiliaryWeapon>(),
            EnsureWeapon<PumpkinShockwaveAuxiliaryWeapon>(),
            EnsureWeapon<ProtectiveBarrierAuxiliaryWeapon>()
        };
        for (int i = 0; i < availableWeapons.Length; i++)
        {
            AuxiliaryWeapon weapon = availableWeapons[i];
            AuxiliaryWeaponStats stats = weaponStates != null && i < weaponStates.Length
                ? weaponStates[i]
                : null;
            if (stats == null)
                Debug.LogError($"{weapon.GetType().Name}의 AuxiliaryWeaponStats가 연결되지 않았습니다.", this);
            weapon.Initialize(stats, transform, pumpkin, player);
            weapon.SetEquipped(false);
        }
    }

    private void Update()
    {
        if (!gameplayActive)
            return;

        foreach (AuxiliaryWeapon weapon in equippedWeapons)
            weapon.Tick(Time.deltaTime);
    }

    public bool TryEquip(int index)
    {
        if (availableWeapons == null || index < 0 || index >= availableWeapons.Length ||
            equippedWeapons.Count >= MaxEquippedWeapons || equippedWeapons.Contains(availableWeapons[index]))
        {
            return false;
        }

        AuxiliaryWeapon weapon = availableWeapons[index];
        weapon.ResetProgress();
        equippedWeapons.Add(weapon);
        weapon.SetEquipped(true);
        return true;
    }

    public AuxiliaryWeapon GetEquippedAt(int slotIndex)
    {
        return slotIndex >= 0 && slotIndex < equippedWeapons.Count ? equippedWeapons[slotIndex] : null;
    }

    public bool ReplaceEquipped(int slotIndex, int newWeaponIndex)
    {
        if (availableWeapons == null || slotIndex < 0 || slotIndex >= equippedWeapons.Count ||
            newWeaponIndex < 0 || newWeaponIndex >= availableWeapons.Length)
            return false;

        AuxiliaryWeapon replacement = availableWeapons[newWeaponIndex];
        if (equippedWeapons.Contains(replacement))
            return false;

        AuxiliaryWeapon oldWeapon = equippedWeapons[slotIndex];
        oldWeapon.SetEquipped(false);
        oldWeapon.ResetProgress();
        replacement.ResetProgress();
        equippedWeapons[slotIndex] = replacement;
        replacement.SetEquipped(true);
        return true;
    }

    public void SetGameplayActive(bool active)
    {
        gameplayActive = active;
    }

    private T EnsureWeapon<T>() where T : AuxiliaryWeapon
    {
        T weapon = GetComponent<T>();
        return weapon != null ? weapon : gameObject.AddComponent<T>();
    }
}
