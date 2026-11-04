using UnityEngine;
namespace Veilbound.ArmourProduction
{
    // Matches Gear.Slots: head, shoulders, chest, hands, belt, bracers, legs, feet, cape.
    // Prototype only: live Gear/settlement wiring and body occlusion masks remain separate.
    public sealed class ModularPlateSlots : MonoBehaviour
    {
        public GameObject[] slots = new GameObject[9];
        public int runtimeMetalItemId;
        public void SetSlotEnabled(int slot, bool equipped)
        {
            if (slot < 0 || slot >= slots.Length) throw new System.ArgumentOutOfRangeException(nameof(slot));
            if (slots[slot]) slots[slot].SetActive(equipped);
        }
    }
}
