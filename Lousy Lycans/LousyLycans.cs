// Project:         Lousy Lycans mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2020 Ralzar
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Ralzar

using DaggerfallConnect;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.MagicAndEffects.MagicEffects;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using UnityEngine;
using System;
using DaggerfallWorkshop;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Game.Serialization;
using System.Collections.Generic;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallConnect.Utility;
using DaggerfallWorkshop.Game.Questing;
using DaggerfallConnect.FallExe;

public class LousyLycans : MonoBehaviour
{
    static Mod mod;
    static LousyLycans instance;

    [Invoke(StateManager.StateTypes.Start, 0)]
    public static void Init(InitParams initParams)
    {
        mod = initParams.Mod;
        var go = new GameObject(mod.Title);
        go.AddComponent<LousyLycans>();
        instance = go.AddComponent<LousyLycans>();

        EntityEffectBroker.OnNewMagicRound += WereBuffs_OnNewMagicRound;
        EntityEffectBroker.OnNewMagicRound += WaxingMoon_OnNewMagicRound;
        EntityEffectBroker.OnNewMagicRound += FullMoon_OnNewMagicRound;
    }

    static PlayerEntity playerEntity = GameManager.Instance.PlayerEntity;
    static RaceTemplate playerRace = playerEntity.RaceTemplate;
    static PlayerEnterExit playerEnterExit = GameManager.Instance.PlayerEnterExit;
    static DaggerfallDateTime timeNow = DaggerfallUnity.Instance.WorldTime.Now;
    static EntityEffectManager playerEffectManager = playerEntity.EntityBehaviour.GetComponent<EntityEffectManager>();
    static bool waxWarning = false;
    static bool fullWarning = false;
    static bool fullMoon = false;
    static bool nightVision = false;
    static bool beastWithRing = false;
    static int strAtt = 0;
    static int intAtt = 0;
    static int wilAtt = 0;
    static int agiAtt = 0;
    static int endAtt = 0;
    static int perAtt = 0;
    static int spdAtt = 0;
    static int skillModAmount = 0;
    static int centaurian = 0;
    static int daedric = 0;
    static int dragonish = 0;
    static int etiquette = 0;
    static int giantish = 0;
    static int harpy = 0;
    static int impish = 0;
    static int lockpicking = 0;
    static int nymph = 0;
    static int orcish = 0;
    static int pickpocket = 0;
    static int spriggan = 0;
    static int streetwise = 0;
    //static bool isWere = false;
    static bool killAll = false;
    static int endPosX = 0;
    static int endPosY = 0;
    private static ItemCollection dropCollection;

    private void Awake()
    {
 
    }

    private void Update()
    {
        if (playerEntity.IsInBeastForm)
        {
            if (!nightVision)
            {
                nightVision = true;
                if (GameManager.Instance.PlayerEntity.LightSource != null)
                {
                    GameManager.Instance.PlayerEntity.LightSource = null;
                }
                GameObject player = GameManager.Instance.PlayerObject;
                GameObject lightsNode = new GameObject("NightVision");
                lightsNode.transform.parent = player.transform;
                AddLight(DaggerfallUnity.Instance, player.transform.gameObject, lightsNode.transform);
                DropAllItems();
                DaggerfallUI.MessageBox("You shed your clothes and items.");
            }
        }
        else if (nightVision)
        {
            nightVision = false;
            GameObject lightsNode = GameObject.Find("NightVision");
            Destroy(lightsNode);
        }
    }


    private static void WereBuffs_OnNewMagicRound()
    {
        if (!GameManager.IsGamePaused && playerEntity.CurrentHealth > 0 && !GameManager.Instance.EntityEffectBroker.SyntheticTimeIncrease)
        {
            //Code to trigger werewolf infection for testing
            //if (!isWere)
            //{
            //EntityEffectBundle bundle = GameManager.Instance.PlayerEffectManager.CreateLycanthropyDisease(LycanthropyTypes.Werewolf);
            //GameManager.Instance.PlayerEffectManager.AssignBundle(bundle, AssignBundleFlags.SpecialInfection);
            //    isWere = true;
            //}

            //Code to trigger werewolf infection for testing
            //if (!isWere)
            //{
            //    EntityEffectBundle bundle = GameManager.Instance.PlayerEffectManager.CreateVampirismDisease();
            //    GameManager.Instance.PlayerEffectManager.AssignBundle(bundle, AssignBundleFlags.SpecialInfection);
            //    isWere = true;
            //}


            if (GameManager.Instance.PlayerEffectManager.HasLycanthropy())
            {
                LycanthropyEffect lycanthropyEffect = GameManager.Instance.PlayerEffectManager.GetRacialOverrideEffect() as LycanthropyEffect;
                LycanthropyTypes lycanthropyTypes = lycanthropyEffect.InfectionType;

                if (lycanthropyTypes == LycanthropyTypes.Werewolf)
                {
                    if (playerEntity.IsInBeastForm)
                    {
                        strAtt = -20;
                        agiAtt = 0;
                        endAtt = -20;
                        spdAtt = +10;
                    }
                    else
                    {
                        strAtt = -40;
                        agiAtt = -30;
                        endAtt = -40;
                        spdAtt = -30;
                        skillModAmount = -25;
                    }
                }
                else
                {
                    if (playerEntity.IsInBeastForm)
                    {
                        strAtt = 0;
                        agiAtt = -20;
                        endAtt = 0;
                        spdAtt = -20;
                    }
                    else
                    {
                        strAtt = -30;
                        agiAtt = -40;
                        endAtt = -30;
                        spdAtt = -40;
                        skillModAmount = -25;
                    }
                }
                if (playerEntity.IsInBeastForm)
                {
                    centaurian = playerEntity.Skills.GetLiveSkillValue(DFCareer.Skills.Centaurian);
                    daedric = playerEntity.Skills.GetLiveSkillValue(DFCareer.Skills.Daedric);
                    dragonish = playerEntity.Skills.GetLiveSkillValue(DFCareer.Skills.Dragonish);
                    etiquette = playerEntity.Skills.GetLiveSkillValue(DFCareer.Skills.Etiquette);
                    giantish = playerEntity.Skills.GetLiveSkillValue(DFCareer.Skills.Giantish);
                    harpy = playerEntity.Skills.GetLiveSkillValue(DFCareer.Skills.Harpy);
                    impish = playerEntity.Skills.GetLiveSkillValue(DFCareer.Skills.Impish);
                    lockpicking = playerEntity.Skills.GetLiveSkillValue(DFCareer.Skills.Lockpicking);
                    nymph = playerEntity.Skills.GetLiveSkillValue(DFCareer.Skills.Nymph);
                    orcish = playerEntity.Skills.GetLiveSkillValue(DFCareer.Skills.Orcish);
                    pickpocket = playerEntity.Skills.GetLiveSkillValue(DFCareer.Skills.Pickpocket);
                    spriggan = playerEntity.Skills.GetLiveSkillValue(DFCareer.Skills.Spriggan);
                    streetwise = playerEntity.Skills.GetLiveSkillValue(DFCareer.Skills.Streetwise);
                    playerEntity.CurrentMagicka = 0;
                    GameManager.Instance.TransportManager.TransportMode = TransportModes.Foot;
                }
            }
            else
            {
                strAtt = 0;
                intAtt = 0;
                wilAtt = 0;
                agiAtt = 0;
                endAtt = 0;
                perAtt = 0;
                spdAtt = 0;
                skillModAmount = 0;
            } 

            int[] statMods = new int[DaggerfallStats.Count];
            statMods[(int)DFCareer.Stats.Strength] = +strAtt;
            statMods[(int)DFCareer.Stats.Intelligence] = +intAtt;
            statMods[(int)DFCareer.Stats.Willpower] = +wilAtt;
            statMods[(int)DFCareer.Stats.Agility] = +agiAtt;
            statMods[(int)DFCareer.Stats.Endurance] = +endAtt;
            statMods[(int)DFCareer.Stats.Personality] = +perAtt;
            statMods[(int)DFCareer.Stats.Speed] = +spdAtt;
            playerEffectManager.MergeDirectStatMods(statMods);

            int[] skillMods = new int[DaggerfallSkills.Count];
            skillMods[(int)DFCareer.Skills.Swimming] = +skillModAmount;
            skillMods[(int)DFCareer.Skills.Running] = +skillModAmount;
            skillMods[(int)DFCareer.Skills.Stealth] = +skillModAmount;
            skillMods[(int)DFCareer.Skills.CriticalStrike] = +skillModAmount;
            skillMods[(int)DFCareer.Skills.Climbing] = +skillModAmount;
            skillMods[(int)DFCareer.Skills.HandToHand] = +skillModAmount;
            skillMods[(int)DFCareer.Skills.Jumping] = +skillModAmount;
            skillMods[(int)DFCareer.Skills.Centaurian] = -centaurian;
            skillMods[(int)DFCareer.Skills.Daedric] = -daedric;
            skillMods[(int)DFCareer.Skills.Dragonish] = -dragonish;
            skillMods[(int)DFCareer.Skills.Etiquette] = -etiquette;
            skillMods[(int)DFCareer.Skills.Giantish] = -giantish;
            skillMods[(int)DFCareer.Skills.Harpy] = -harpy;
            skillMods[(int)DFCareer.Skills.Impish] = -impish;
            skillMods[(int)DFCareer.Skills.Lockpicking] = -lockpicking;
            skillMods[(int)DFCareer.Skills.Nymph] = -nymph;
            skillMods[(int)DFCareer.Skills.Orcish] = -orcish;
            skillMods[(int)DFCareer.Skills.Pickpocket] = -pickpocket;
            skillMods[(int)DFCareer.Skills.Spriggan] = -spriggan;
            skillMods[(int)DFCareer.Skills.Streetwise] = -streetwise;
            playerEffectManager.MergeDirectSkillMods(skillMods);
        }
    }

    private static void WaxingMoon_OnNewMagicRound()
    {
        if (GameManager.Instance.PlayerEffectManager.HasLycanthropy())
        {
            if (timeNow.MassarLunarPhase == LunarPhases.ThreeWax)
            {
                if (timeNow.IsNight && timeNow.Hour < 10 && !waxWarning)
                {
                    waxWarning = true;
                    DaggerfallUI.AddHUDText("Masser is waxing, soon it will be a full moon...");
                }
                else if (timeNow.IsDay)
                {
                    waxWarning = false;
                }
            }
            else if (timeNow.SecundaLunarPhase == LunarPhases.ThreeWax)
            {
                if (timeNow.IsNight && timeNow.Hour < 10 && !waxWarning)
                {
                    waxWarning = true;
                    DaggerfallUI.AddHUDText("Secunda is waxing, soon it will be a full moon...");
                }
                else if (timeNow.IsDay)
                {
                    waxWarning = false;
                }
            }
            else
            {
                waxWarning = false;
            }
        }
    }

    static bool raiseTime = false;
    static bool movePlayer = false;

    private static void FullMoon_OnNewMagicRound()
    {
        if (GameManager.Instance.PlayerEffectManager.HasLycanthropy())
        {
            if (!IsWearingHircineRing() || beastWithRing)
            {
                if (fullMoon)
                {
                    ModManager.Instance.SendModMessage("TravelOptions", "pauseTravel");
                    DaggerfallUI.Instance.FadeBehaviour.SmashHUDToBlack();
                    int timeRaised = 109000 + UnityEngine.Random.Range(10, 400);
                    timeNow.RaiseTime(timeRaised);

                    if (playerEnterExit.IsPlayerInside)
                        playerEnterExit.TransitionExterior();

                    RandomLocation();
                    int roll = UnityEngine.Random.Range(-50, 101);
                    if (roll < playerEntity.Stats.LiveLuck)
                        playerEntity.PreventEnemySpawns = true;
                }

                if (timeNow.SecundaLunarPhase == LunarPhases.Full || timeNow.MassarLunarPhase == LunarPhases.Full)
                {
                    if (!fullWarning)
                    {
                        DaggerfallUI.MessageBox("The moon calls to you. You can feel the change is about to happen.");
                        ModManager.Instance.SendModMessage("TravelOptions", "pauseTravel");
                        fullWarning = true;
                    }
                    else if (GameManager.Instance.PlayerEffectManager.HasLycanthropy() && !fullMoon)
                    {
                        GameManager.Instance.TransportManager.TransportMode = TransportModes.Foot;
                        DaggerfallUI.Instance.FadeBehaviour.SmashHUDToBlack();
                        fullMoon = true;
                        DropAllItems();
                    }
                    else
                    {
                        fullMoon = false;
                    }
                }
                else
                {
                    fullMoon = false;
                    fullWarning = false;
                    beastWithRing = false;
                }
                if (killAll && !GameManager.IsGamePaused)
                {
                    DaggerfallUI.Instance.FadeBehaviour.FadeHUDFromBlack();
                    killAll = false;
                    KillAll();
                }
            }
            else
            {
                if (timeNow.SecundaLunarPhase == LunarPhases.Full || timeNow.MassarLunarPhase == LunarPhases.Full)
                {
                    if (!fullWarning)
                    {
                        DaggerfallUI.MessageBox("The moon calls to you. The Hircine Ring protects you, as long as you stay in your human shape.");
                        ModManager.Instance.SendModMessage("TravelOptions", "pauseTravel");
                        fullWarning = true;
                    }
                    else if (playerEntity.IsInBeastForm)
                    {
                        beastWithRing = true;
                    }
                }
                else
                {
                    fullMoon = false;
                    fullWarning = false;
                    beastWithRing = false;
                }
            }
        }
    }

    private static void DropAllItems()
    {
        GameObject player = GameManager.Instance.PlayerObject;
        dropCollection = new ItemCollection();
        ItemCollection keepItemsCollection = new ItemCollection();

        UnequipAll();

        dropCollection.AddItems(playerEntity.Items.CloneAll());

        for (int i = 0; i < dropCollection.Count; i++)
        {
            DaggerfallUnityItem item = dropCollection.GetItem(i);
            if (item.QuestItemSymbol != null || item.IsQuestItem || item.IsSummoned || item.TemplateIndex == 132 || item.TemplateIndex == 93 || item.TemplateIndex == 94)
            {
                if (item.IsEquipped)
                {
                    item.UnequipItem(playerEntity);
                }
                keepItemsCollection.AddItem(item);
                dropCollection.RemoveItem(item);
            }
        }
        DaggerfallLoot equipPile = GameObjectHelper.CreateDroppedLootContainer(player, DaggerfallUnity.NextUID);
        equipPile.customDrop = true;
        equipPile.playerOwned = true;
        equipPile.Items.AddItems(dropCollection.CloneAll());
        playerEntity.Items.Clear();
        dropCollection.Clear();

        for (int i = 0; i < keepItemsCollection.Count; i++)
        {
            DaggerfallUnityItem item = keepItemsCollection.GetItem(i);
            playerEntity.Items.AddItem(item);
        }
        keepItemsCollection.Clear();
    }

    private static void UnequipAll()
    {
        foreach (ItemCollection playerItems in new ItemCollection[] { playerEntity.Items })
        {
            for (int i = 0; i < playerItems.Count; i++)
            {
                DaggerfallUnityItem item = playerItems.GetItem(i);
                if (item.IsEquipped)
                {
                    item.currentCondition /= 2;
                }
            }
        }

        DaggerfallUnityItem head = playerEntity.ItemEquipTable.GetItem(EquipSlots.Head);
        DaggerfallUnityItem leftArm = playerEntity.ItemEquipTable.GetItem(EquipSlots.LeftArm);
        DaggerfallUnityItem rightArm = playerEntity.ItemEquipTable.GetItem(EquipSlots.RightArm);
        DaggerfallUnityItem leftHand = playerEntity.ItemEquipTable.GetItem(EquipSlots.LeftHand);
        DaggerfallUnityItem rightHand = playerEntity.ItemEquipTable.GetItem(EquipSlots.RightHand);
        DaggerfallUnityItem gloves = playerEntity.ItemEquipTable.GetItem(EquipSlots.Gloves);
        DaggerfallUnityItem chestA = playerEntity.ItemEquipTable.GetItem(EquipSlots.ChestArmor);
        DaggerfallUnityItem chestC = playerEntity.ItemEquipTable.GetItem(EquipSlots.ChestClothes);
        DaggerfallUnityItem cloak1 = playerEntity.ItemEquipTable.GetItem(EquipSlots.Cloak1);
        DaggerfallUnityItem cloak2 = playerEntity.ItemEquipTable.GetItem(EquipSlots.Cloak2);
        DaggerfallUnityItem legsA = playerEntity.ItemEquipTable.GetItem(EquipSlots.LegsArmor);
        DaggerfallUnityItem legsC = playerEntity.ItemEquipTable.GetItem(EquipSlots.LegsClothes);
        DaggerfallUnityItem feet = playerEntity.ItemEquipTable.GetItem(EquipSlots.Feet);

        playerEntity.ItemEquipTable.UnequipItem(EquipSlots.Head);
        playerEntity.ItemEquipTable.UnequipItem(EquipSlots.LeftArm);
        playerEntity.ItemEquipTable.UnequipItem(EquipSlots.RightArm);
        playerEntity.ItemEquipTable.UnequipItem(EquipSlots.LeftHand);
        playerEntity.ItemEquipTable.UnequipItem(EquipSlots.RightHand);
        playerEntity.ItemEquipTable.UnequipItem(EquipSlots.Gloves);
        playerEntity.ItemEquipTable.UnequipItem(EquipSlots.ChestArmor);
        playerEntity.ItemEquipTable.UnequipItem(EquipSlots.ChestClothes);
        playerEntity.ItemEquipTable.UnequipItem(EquipSlots.Cloak1);
        playerEntity.ItemEquipTable.UnequipItem(EquipSlots.Cloak2);
        playerEntity.ItemEquipTable.UnequipItem(EquipSlots.LegsArmor);
        playerEntity.ItemEquipTable.UnequipItem(EquipSlots.LegsClothes);
        playerEntity.ItemEquipTable.UnequipItem(EquipSlots.Feet);

        dropCollection.Transfer(head, playerEntity.Items);
        dropCollection.Transfer(leftArm, playerEntity.Items);
        dropCollection.Transfer(rightArm, playerEntity.Items);
        dropCollection.Transfer(leftHand, playerEntity.Items);
        dropCollection.Transfer(rightHand, playerEntity.Items);
        dropCollection.Transfer(gloves, playerEntity.Items);
        dropCollection.Transfer(chestA, playerEntity.Items);
        dropCollection.Transfer(chestC, playerEntity.Items);
        dropCollection.Transfer(cloak1, playerEntity.Items);
        dropCollection.Transfer(cloak2, playerEntity.Items);
        dropCollection.Transfer(legsA, playerEntity.Items);
        dropCollection.Transfer(legsC, playerEntity.Items);
        dropCollection.Transfer(feet, playerEntity.Items);
    }

    private static void RandomLocation()
    {
        int startX = GameManager.Instance.PlayerGPS.CurrentMapPixel.X;
        int startY = GameManager.Instance.PlayerGPS.CurrentMapPixel.Y;
        if (DaggerfallUnity.Instance.ContentReader.MapFileReader.GetClimateIndex(startX, startY) != (int)MapsFile.Climates.Ocean)
        {
            do
            {
                endPosX = startX + UnityEngine.Random.Range(-1, 2);
                endPosY = startY + UnityEngine.Random.Range(-1, 2);
            }
            while (DaggerfallUnity.Instance.ContentReader.MapFileReader.GetClimateIndex(endPosX, endPosY) == (int)MapsFile.Climates.Ocean);
            GameManager.Instance.StreamingWorld.TeleportToCoordinates(endPosX, endPosY, StreamingWorld.RepositionMethods.DirectionFromStartMarker);
        }
        else
            GameManager.Instance.StreamingWorld.TeleportToCoordinates(startX, startY, StreamingWorld.RepositionMethods.DirectionFromStartMarker);
        LycanthropyEffect lycanthropyEffect = GameManager.Instance.PlayerEffectManager.GetRacialOverrideEffect() as LycanthropyEffect;
        lycanthropyEffect.MorphSelf();
        killAll = true;
    }

    private static void KillAll()
    {
        DaggerfallEntityBehaviour[] entityBehaviours = FindObjectsOfType<DaggerfallEntityBehaviour>();
        int count = 0;
        for (int i = 0; i < entityBehaviours.Length; i++)
        {
            DaggerfallEntityBehaviour entityBehaviour = entityBehaviours[i];
            if (entityBehaviour.EntityType == EntityTypes.EnemyMonster || entityBehaviour.EntityType == EntityTypes.EnemyClass)
            {
                entityBehaviour.Entity.SetHealth(0);
                count++;
            }
        }
    }

    private static GameObject AddLight(DaggerfallUnity dfUnity, GameObject player, Transform parent)
    {
        GameObject go = GameObjectHelper.InstantiatePrefab(dfUnity.Option_DungeonLightPrefab.gameObject, string.Empty, parent, player.transform.position);
        Light light = go.GetComponent<Light>();
        light.range = 20;
        return go;
    }

    private static bool IsWearingHircineRing()
    {
        DaggerfallUnityItem[] equipTable = GameManager.Instance.PlayerEntity.ItemEquipTable.EquipTable;
        if (equipTable == null || equipTable.Length == 0)
            return false;

        return IsHircineRingItem(equipTable[(int)EquipSlots.Ring0]) || IsHircineRingItem(equipTable[(int)EquipSlots.Ring1]);
    }

    private static bool IsHircineRingItem(DaggerfallUnityItem item)
    {
        return
            item != null &&
            item.IsArtifact &&
            item.ContainsEnchantment(EnchantmentTypes.SpecialArtifactEffect, (short)ArtifactsSubTypes.Hircine_Ring);
    }
}
