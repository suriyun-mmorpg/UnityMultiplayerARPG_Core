using System;

namespace MultiplayerARPG
{
    /// <summary>Local indexes for calculating amounts. Slots are never saved or sent over the network.</summary>
    public static class RuntimeGameDataSlots
    {
        private static readonly Attribute[] s_attributes = new Attribute[AttributeAmounts.Capacity];
        private static readonly DamageElement[] s_damageElements = new DamageElement[DamageElementFloatAmounts.Capacity];
        private static int s_attributeCount;
        // Slot zero is reserved for the default damage element, including before it is registered.
        private static int s_damageElementCount = 1;

        public static int AttributeCount => s_attributeCount;
        public static int DamageElementCount => s_damageElementCount;

        public static int GetSlot(Attribute attribute)
        {
            if (attribute == null)
                throw new ArgumentNullException(nameof(attribute));
            int slot = attribute.RuntimeSlot;
            if (slot >= 0 && slot < s_attributeCount && ReferenceEquals(s_attributes[slot], attribute))
                return slot;
            Register(attribute);
            return attribute.RuntimeSlot;
        }

        public static int GetSlot(DamageElement damageElement)
        {
            if (damageElement == null)
                throw new ArgumentNullException(nameof(damageElement));
            int slot = damageElement.RuntimeSlot;
            if (slot >= 0 && slot < s_damageElementCount && ReferenceEquals(s_damageElements[slot], damageElement))
                return slot;
            if (GameInstance.Singleton != null && ReferenceEquals(GameInstance.Singleton.DefaultDamageElement, damageElement))
                RegisterDefaultDamageElement(damageElement);
            else
                Register(damageElement);
            return damageElement.RuntimeSlot;
        }

        public static void Register(Attribute attribute)
        {
            if (attribute == null)
                return;
            int oldSlot = attribute.RuntimeSlot;
            if (oldSlot >= 0 && oldSlot < s_attributeCount && ReferenceEquals(s_attributes[oldSlot], attribute))
                return;
            if (s_attributeCount >= AttributeAmounts.Capacity)
                throw new InvalidOperationException($"Registered attributes exceed the {AttributeAmounts.Capacity} available slots. Define ATTRIBUTE_AMOUNTS_16 or ATTRIBUTE_AMOUNTS_32.");
            int slot = s_attributeCount++;
            s_attributes[slot] = attribute;
            attribute.RuntimeSlot = slot;
        }

        public static void Register(DamageElement damageElement)
        {
            if (damageElement == null)
                return;
            int oldSlot = damageElement.RuntimeSlot;
            if (oldSlot >= 0 && oldSlot < s_damageElementCount && ReferenceEquals(s_damageElements[oldSlot], damageElement))
                return;
            if (s_damageElementCount >= DamageElementFloatAmounts.Capacity)
                throw new InvalidOperationException($"Registered damage elements exceed the {DamageElementFloatAmounts.Capacity} available slots, including the default element. Define DAMAGE_ELEMENT_AMOUNTS_16 or DAMAGE_ELEMENT_AMOUNTS_32.");
            int slot = s_damageElementCount++;
            s_damageElements[slot] = damageElement;
            damageElement.RuntimeSlot = slot;
        }

        public static void RegisterDefaultDamageElement(DamageElement damageElement)
        {
            if (damageElement == null)
                return;
            if (ReferenceEquals(s_damageElements[0], damageElement))
                return;
            int oldSlot = damageElement.RuntimeSlot;
            if (oldSlot > 0 && oldSlot < s_damageElementCount && ReferenceEquals(s_damageElements[oldSlot], damageElement))
                throw new InvalidOperationException("The default damage element was registered after other elements. Register it before loading game data.");
            s_damageElements[0] = damageElement;
            damageElement.RuntimeSlot = 0;
        }

        public static Attribute GetAttribute(int slot)
        {
            if (slot < 0 || slot >= s_attributeCount)
                throw new IndexOutOfRangeException($"Invalid attribute slot: {slot}");
            return s_attributes[slot];
        }

        public static DamageElement GetDamageElement(int slot)
        {
            if (slot < 0 || slot >= s_damageElementCount || s_damageElements[slot] == null)
                throw new IndexOutOfRangeException($"Invalid damage element slot: {slot}");
            return s_damageElements[slot];
        }

        public static void Clear()
        {
            for (int i = 0; i < s_attributeCount; ++i)
            {
                if (s_attributes[i] != null)
                    s_attributes[i].RuntimeSlot = -1;
                s_attributes[i] = null;
            }
            for (int i = 0; i < s_damageElementCount; ++i)
            {
                if (s_damageElements[i] != null)
                    s_damageElements[i].RuntimeSlot = -1;
                s_damageElements[i] = null;
            }
            s_attributeCount = 0;
            s_damageElementCount = 1;
        }
    }
}
