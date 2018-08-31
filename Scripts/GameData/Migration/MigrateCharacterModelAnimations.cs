using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MultiplayerARPG;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MigrateCharacterModelAnimations : MonoBehaviour
{
    public WeaponType[] weaponTypes;
    public Skill[] skills;
    public MonsterCharacter[] monsterCharacters;
    public PlayerCharacterEntity[] playerCharacterEntities;

    [ContextMenu("Migrate")]
    public void Migrate()
    {
        // Filter data
        var filterWeaponTypes = new List<WeaponType>();
        var filterSkills = new List<Skill>();
        var filterMonsterCharacters = new List<MonsterCharacter>();
        var filterPlayerCharacterEntities = new List<PlayerCharacterEntity>();

        foreach (var weaponType in weaponTypes)
        {
            if (weaponType == null || filterWeaponTypes.Contains(weaponType)) continue;
            filterWeaponTypes.Add(weaponType);
        }
        foreach (var skill in skills)
        {
            if (skill == null || filterSkills.Contains(skill)) continue;
            filterSkills.Add(skill);
        }
        foreach (var monsterCharacter in monsterCharacters)
        {
            if (monsterCharacter == null || filterMonsterCharacters.Contains(monsterCharacter)) continue;
            filterMonsterCharacters.Add(monsterCharacter);
        }
        foreach (var playerCharacterEntity in playerCharacterEntities)
        {
            if (playerCharacterEntity == null || filterPlayerCharacterEntities.Contains(playerCharacterEntity)) continue;
            filterPlayerCharacterEntities.Add(playerCharacterEntity);
        }

        // Migrate Player Character Entity
        foreach (var playerCharacterEntity in filterPlayerCharacterEntities)
        {
            if (playerCharacterEntity.CharacterModel == null || !(playerCharacterEntity.CharacterModel is CharacterModel))
                continue;
            var model = playerCharacterEntity.CharacterModel as CharacterModel;
            model.weaponAnimations = new WeaponAnimations[filterWeaponTypes.Count];
            var i = 0;
            foreach (var weaponType in filterWeaponTypes)
            {
                var newAnims = new WeaponAnimations();
                newAnims.weaponType = weaponType;
                newAnims.leftHandAttackAnimations = MakeActionAnimList(model, weaponType.leftHandAttackAnimations);
                newAnims.rightHandAttackAnimations = MakeActionAnimList(model, weaponType.rightHandAttackAnimations);
                model.weaponAnimations[i] = newAnims;
                ++i;
            }
            model.skillCastAnimations = new SkillCastAnimations[filterSkills.Count];
            i = 0;
            foreach (var skill in filterSkills)
            {
                var newAnims = new SkillCastAnimations();
                newAnims.skill = skill;
                newAnims.castAnimations = MakeActionAnimList(model, skill.castAnimations);
                model.skillCastAnimations[i] = newAnims;
                ++i;
            }
#if UNITY_EDITOR
            EditorUtility.SetDirty(model.gameObject);
#endif
        }

        // Migrate Monster Character Entity
        foreach (var monsterCharacter in filterMonsterCharacters)
        {
            if (monsterCharacter.entityPrefab == null || monsterCharacter.entityPrefab.CharacterModel == null || !(monsterCharacter.entityPrefab.CharacterModel is CharacterModel))
                continue;
            var model = monsterCharacter.entityPrefab.CharacterModel as CharacterModel;
            model.defaultAttackAnimations = monsterCharacter.attackAnimations;
#if UNITY_EDITOR
            EditorUtility.SetDirty(model.gameObject);
#endif
        }

        Debug.Log(" -- Migration done -- ");
    }

    private ActionAnimation[] MakeActionAnimList(CharacterModel model, ActionAnimation[] fromList)
    {
        var result = new List<ActionAnimation>();
        foreach (var entry in fromList)
        {
            var newAnimData = new ActionAnimation();
            newAnimData.clip = entry.clip;
            newAnimData.triggerDurationRate = entry.triggerDurationRate;
            newAnimData.extraDuration = entry.extraDuration;
            newAnimData.audioClips = entry.audioClips;
            foreach (var overrideEntry in entry.overrideData)
            {
                if (overrideEntry.target != model)
                    continue;
                if (overrideEntry.clip != null)
                    newAnimData.clip = overrideEntry.clip;
                if (overrideEntry.triggerDurationRate > 0)
                    newAnimData.triggerDurationRate = overrideEntry.triggerDurationRate;
                if (overrideEntry.extraDuration > 0)
                    newAnimData.extraDuration = overrideEntry.extraDuration;
                if (overrideEntry.audioClips.Length > 0)
                    newAnimData.audioClips = overrideEntry.audioClips;
            }
            result.Add(newAnimData);
        }
        return result.ToArray();
    }
}
