# Veilbound reusable plate masters

Start with [MASTER_REPORT.md](MASTER_REPORT.md). Ten temporary tier palettes use two approved mesh families with male/female fits. Import Veilbound_Plate_Masters.unitypackage into a review project and use Generated/*_Master.prefab. Keep the current live player until the new review rig is integrated. Runtime API: SetSlotTier(slot,itemId), SetSlotEnabled(slot,bool), EquipWholeTier(itemId). Body masks restore covered source geometry on unequip.

All processing is local; no further Tripo generation is required. Original prototype files remain in the parent folder.
