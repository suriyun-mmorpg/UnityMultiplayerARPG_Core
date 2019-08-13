using System.Collections.Generic;

namespace MultiplayerARPG
{
    public class CharacterModelDataManager
    {
        private static WeaponAnimations[] copyingWeaponAnimations;
        private static SkillAnimations[] copyingSkillAnimations;

        public static void CopyWeaponAnimations(WeaponAnimations[] data)
        {
            copyingWeaponAnimations = data;
        }

        public static void CopySkillAnimations(SkillAnimations[] data)
        {
            copyingSkillAnimations = data;
        }

        public static WeaponAnimations[] PasteWeaponAnimations()
        {
            if (copyingWeaponAnimations == null || copyingWeaponAnimations.Length == 0)
                return null;
            return new List<WeaponAnimations>(copyingWeaponAnimations).ToArray();
        }

        public static SkillAnimations[] PasteSkillAnimations()
        {
            if (copyingSkillAnimations == null || copyingSkillAnimations.Length == 0)
                return null;
            return new List<SkillAnimations>(copyingSkillAnimations).ToArray();
        }
    }
}
