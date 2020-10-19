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

        GameManager.Instance.RegisterPreventRestCondition(() => { return FullMoonWake(); }, "You awake with a snarl!");
    }

    static PlayerEntity playerEntity = GameManager.Instance.PlayerEntity;
    static RaceTemplate playerRace = playerEntity.RaceTemplate;
    static DaggerfallDateTime timeNow = DaggerfallUnity.Instance.WorldTime.Now;
    static PlayerEnterExit playerEnterExit = GameManager.Instance.PlayerEnterExit;
    static EntityEffectManager playerEffectManager = playerEntity.EntityBehaviour.GetComponent<EntityEffectManager>();
    static bool waxWarning = false;
    static bool fullWarning = false;
    static bool fullMoon = false;
    static bool nightVision = false;
    static bool beastWithRing = false;
    static bool horrorWakeup = false;
    static bool resetBars = false;
    static int strAtt = 0;
    static int intAtt = 0;
    static int wilAtt = 0;
    static int agiAtt = 0;
    static int endAtt = 0;
    static int perAtt = 0;
    static int spdAtt = 0;
    static int skillModAmount = 0;
    static int dodging = 0;
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

    void Awake()
    {
 
    }

    void Update()
    {
        if (!DaggerfallUnity.Instance.IsReady || !playerEnterExit || GameManager.IsGamePaused)
            return;

        if (resetBars)
        {
            resetBars = false;
            playerEntity.CurrentHealth = playerEntity.MaxHealth;
            playerEntity.CurrentFatigue = playerEntity.MaxFatigue;
            playerEntity.CurrentMagicka = playerEntity.MaxMagicka;
        }

        if (playerEntity.IsInBeastForm && !playerEntity.IsResting)
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
                playerEntity.CurrentFatigue = playerEntity.MaxFatigue;
                playerEntity.CurrentMagicka = 0;
                DropAllItems();                
            }
        }
        else if (nightVision)
        {
            nightVision = false;
            GameObject lightsNode = GameObject.Find("NightVision");
            Destroy(lightsNode);
        }

        if (horrorWakeup && GameManager.Instance.IsPlayerOnHUD && !DaggerfallUI.Instance.FadeBehaviour.FadeInProgress)
        {
            horrorWakeup = false;
            WakeUp();
            ResetNeedToKillInnocent();
            resetBars = true;
            playerEntity.CurrentHealth = playerEntity.MaxHealth;
            playerEntity.CurrentFatigue = playerEntity.MaxFatigue;
            playerEntity.CurrentMagicka = playerEntity.MaxMagicka;
        }
    }

    private static void WakeUp()
    {
        DaggerfallUI.Instance.FadeBehaviour.FadeHUDFromBlack();
        DaggerfallMessageBox wakePopUp = new DaggerfallMessageBox(DaggerfallUI.UIManager, DaggerfallUI.UIManager.TopWindow);
        string[] message = {
                            "You awake, disoriented and naked.",
                            " ",
                            "You have lost a day.",
                            "",
                            "What horrors did you commit during the full moon...?"
                        };
        wakePopUp.SetText(message);
        wakePopUp.Show();
        wakePopUp.ClickAnywhereToClose = true;
    }

    private static void WereBuffs_OnNewMagicRound()
    {
        if (!GameManager.IsGamePaused && playerEntity.CurrentHealth > 0 && !GameManager.Instance.EntityEffectBroker.SyntheticTimeIncrease)
        {
            ////Code to trigger werewolf infection for testing
            //if (!GameManager.Instance.PlayerEffectManager.HasLycanthropy())
            //{
            //    EntityEffectBundle bundle = GameManager.Instance.PlayerEffectManager.CreateLycanthropyDisease(LycanthropyTypes.Werewolf);
            //    GameManager.Instance.PlayerEffectManager.AssignBundle(bundle, AssignBundleFlags.SpecialInfection);
            //    Debug.Log("Adding Were");
            //}

            if (GameManager.Instance.PlayerEffectManager.HasLycanthropy())
            {
                LycanthropyEffect lycanthropyEffect = GameManager.Instance.PlayerEffectManager.GetRacialOverrideEffect() as LycanthropyEffect;
                LycanthropyTypes lycanthropyTypes = lycanthropyEffect.InfectionType;
                if (GameManager.Instance.AreEnemiesNearby())
                    PacifyWere(playerEntity.IsInBeastForm);

                if (lycanthropyTypes == LycanthropyTypes.Werewolf)
                {
                    if (playerEntity.IsInBeastForm)
                    {
                        strAtt = -20;
                        agiAtt = 0;
                        endAtt = -20;
                        spdAtt = 0;
                    }
                    else
                    {
                        strAtt = -40;
                        agiAtt = -30;
                        endAtt = -40;
                        spdAtt = -30;                        
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
                    }
                }

                if (playerEntity.IsInBeastForm)
                {
                    skillModAmount = 0;
                    dodging = 30;
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
                    playerEntity.CurrentFatigue = playerEntity.MaxFatigue;
                    GameManager.Instance.TransportManager.TransportMode = TransportModes.Foot;
                    GameObject lightsNode = GameObject.Find("NightVision");
                    if (lightsNode == null)
                    {
                        Debug.Log("lightsNode == null");
                        nightVision = false;
                    }
                }
                else
                {
                    skillModAmount = -25;
                    dodging = 0;
                    centaurian =0;
                    daedric = 0;
                    dragonish = 0;
                    etiquette = 0;
                    giantish = 0;
                    harpy = 0;
                    impish = 0;
                    lockpicking = 0;
                    nymph = 0;
                    orcish = 0;
                    pickpocket = 0;
                    spriggan = 0;
                    streetwise = 0;
                    GameObject lightsNode = GameObject.Find("NightVision");
                    if (lightsNode != null)
                    {
                        nightVision = false;
                        Destroy(lightsNode);
                    }
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
            skillMods[(int)DFCareer.Skills.Dodging] = +dodging;
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

    private static void PacifyWere(bool isBeast)
    {
        DaggerfallEntityBehaviour[] entityBehaviours = FindObjectsOfType<DaggerfallEntityBehaviour>();
        for (int i = 0; i < entityBehaviours.Length; i++)
        {
            DaggerfallEntityBehaviour entityBehaviour = entityBehaviours[i];
            if (entityBehaviour.EntityType == EntityTypes.EnemyMonster)
            {

                EnemyEntity enemyEntity = entityBehaviour.Entity as EnemyEntity;
                EnemySenses enemySenses = entityBehaviour.GetComponent<EnemySenses>();
                EnemyMotor enemyMotor = entityBehaviour.GetComponent<EnemyMotor>();
                if (enemySenses && enemySenses.HasEncounteredPlayer && enemyEntity.MobileEnemy.Team == MobileTeams.Werecreatures && enemyMotor.IsHostile && enemyEntity.MobileEnemy.Team != MobileTeams.PlayerAlly)
                {
                    if (isBeast)
                    {
                        enemyMotor.IsHostile = false;
                        DaggerfallUI.AddHUDText("Pacified " + enemyEntity.Name + " using your lycanthropy.");
                    }
                }
            }
        }
    }

    private static void WaxingMoon_OnNewMagicRound()
    {
        timeNow = DaggerfallUnity.Instance.WorldTime.Now;
        if (timeNow.MinuteOfDay == 720 || timeNow.MinuteOfDay == 1080)
            waxWarning = false;
        if (GameManager.Instance.PlayerEffectManager.HasLycanthropy() && !GameManager.IsGamePaused && !DaggerfallUI.Instance.FadeBehaviour.FadeInProgress)
        {            
            if (FullTonight(true) && !waxWarning)
            {
                waxWarning = true;
                DaggerfallUI.AddHUDText("Masser is waxing...");
                ModManager.Instance.SendModMessage("TravelOptions", "showMessage", "The coming night will have a full moon...");
                DaggerfallUI.AddHUDText("The coming night will have a full moon.");
                ModManager.Instance.SendModMessage("TravelOptions", "showMessage", "The coming night will have a full moon.");
            }
            else if (FullTonight(false) && !waxWarning)
            {
                waxWarning = true;
                DaggerfallUI.AddHUDText("Secunda is waxing...");
                ModManager.Instance.SendModMessage("TravelOptions", "showMessage", "The coming night will have a full moon...");
                DaggerfallUI.AddHUDText("The coming night will have a full moon.");
                ModManager.Instance.SendModMessage("TravelOptions", "showMessage", "The coming night will have a full moon.");
            }
            else if (!FullTonight(true) && !FullTonight(false))
            {
                waxWarning = false;
            }
        }
    }

    static bool raiseTime = false;
    static bool movePlayer = false;

    private static bool FullMoonWake()
    {
        if (GameManager.Instance.PlayerEffectManager.HasLycanthropy() && (timeNow.SecundaLunarPhase == LunarPhases.Full || timeNow.MassarLunarPhase == LunarPhases.Full))
        {
            return true;
        }
        else
            return false;
    }

    private static void FullMoon_OnNewMagicRound()
    {
        if (!DaggerfallUnity.Instance.IsReady || !playerEnterExit || GameManager.IsGamePaused || DaggerfallUI.Instance.FadeBehaviour.FadeInProgress)
            return;
        if (GameManager.Instance.PlayerEffectManager.HasLycanthropy() && !playerEntity.IsResting)
        {
            if (!IsWearingHircineRing() || beastWithRing)
            {
                if (fullMoon)
                {
                    ModManager.Instance.SendModMessage("TravelOptions", "pauseTravel");
                    DaggerfallUI.Instance.FadeBehaviour.SmashHUDToBlack();
                    if (Dice100.SuccessRoll(playerEntity.Stats.LiveLuck))
                        playerEntity.PreventEnemySpawns = true;
                    int timeRaised = 109000 + UnityEngine.Random.Range(10, 400);
                    timeNow.RaiseTime(timeRaised);

                    if (playerEnterExit.IsPlayerInsideDungeon)
                        DungeonMoon();
                    else
                    {
                        if (playerEnterExit.IsPlayerInside)
                            playerEnterExit.TransitionExterior();

                        RandomLocation();
                    }

                    if (Dice100.SuccessRoll(playerEntity.Stats.LiveLuck))
                        playerEntity.PreventEnemySpawns = true;
                }

                if (timeNow.SecundaLunarPhase == LunarPhases.Full || timeNow.MassarLunarPhase == LunarPhases.Full)
                {
                    if (!fullWarning)
                    {
                        DaggerfallUI.MessageBox("The moon calls to you. You are unable to resist its pull.");
                        ModManager.Instance.SendModMessage("TravelOptions", "pauseTravel");
                        fullWarning = true;
                    }
                    else if (GameManager.Instance.PlayerEffectManager.HasLycanthropy() && !fullMoon)
                    {
                        GameManager.Instance.TransportManager.TransportMode = TransportModes.Foot;
                        DaggerfallUI.Instance.FadeBehaviour.SmashHUDToBlack();
                        fullMoon = true;
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
                        DaggerfallUI.MessageBox("The moon calls to you, but the Hircine Ring protects you. As long as you stay in your human shape.");
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
        UnequipAll();

        GameObject player = GameManager.Instance.PlayerObject;
        List<DaggerfallUnityItem> dropList = new List<DaggerfallUnityItem>();
        for (int i = 0; i < playerEntity.Items.Count; i++)
        {

            DaggerfallUnityItem item = playerEntity.Items.GetItem(i);
            if (item.QuestItemSymbol != null || item.IsQuestItem || item.IsSummoned || item.TemplateIndex == 132 || item.TemplateIndex == 93 || item.TemplateIndex == 94)
            {
            }
            else
            {
                if (item.IsEquipped)
                {
                    item.currentCondition /= 2;
                }
                else
                {
                    dropList.Add(item);
                }
            }
        }

        if (dropList.Count >= 1)
        {
            DaggerfallLoot equipPile = GameObjectHelper.CreateDroppedLootContainer(player, DaggerfallUnity.NextUID);
            equipPile.customDrop = true;
            equipPile.playerOwned = true;

            foreach (DaggerfallUnityItem item in dropList)
            {
                equipPile.Items.Transfer(item, playerEntity.Items);
            }
            DaggerfallUI.MessageBox("You tear off your clothes and armor.");
        }
    }

    private static void UnequipAll()
    {
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
        DaggerfallUnityItem feet = playerEntity.ItemEquipTable.GetItem(EquipSlots.Feet);
        DaggerfallUnityItem legsA = playerEntity.ItemEquipTable.GetItem(EquipSlots.LegsArmor);
        DaggerfallUnityItem legsC = playerEntity.ItemEquipTable.GetItem(EquipSlots.LegsClothes);

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
        playerEntity.ItemEquipTable.UnequipItem(EquipSlots.Feet);
        playerEntity.ItemEquipTable.UnequipItem(EquipSlots.LegsArmor);
        playerEntity.ItemEquipTable.UnequipItem(EquipSlots.LegsClothes);
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
        playerEntity.LastTimePlayerAteOrDrankAtTavern = DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime.ToClassicDaggerfallTime() - 250;
        killAll = true;
    }
    private static void KillAll(bool all = true)        
    {
        DaggerfallEntityBehaviour[] entityBehaviours = FindObjectsOfType<DaggerfallEntityBehaviour>();
        int length = entityBehaviours.Length;
        int killed = 0;
        int mobs = 0;
        int luck = playerEntity.Stats.LiveLuck/4;
        for (int i = 0; i < length; i++)
        {
            DaggerfallEntityBehaviour entityBehaviour = entityBehaviours[i];
            if (entityBehaviour.EntityType == EntityTypes.EnemyMonster || entityBehaviour.EntityType == EntityTypes.EnemyClass)
            {
                mobs++;
                if (all)
                {
                    killed++;
                    entityBehaviour.Entity.SetHealth(0);
                }
                else if (Dice100.SuccessRoll(luck))
                {
                    killed++;
                    entityBehaviour.Entity.SetHealth(0);
                }                  
            }
        }
        horrorWakeup = true;
        playerEntity.CurrentHealth = playerEntity.MaxHealth;
        playerEntity.CurrentFatigue = playerEntity.MaxFatigue;
        playerEntity.CurrentMagicka = playerEntity.MaxMagicka;
        DaggerfallUI.Instance.FadeBehaviour.FadeHUDFromBlack();
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

    private static bool FullTonight(bool isMasser)
    {
        int offset = (isMasser) ? 3 : -1;
        int moonRatio = (timeNow.DayOfYear + timeNow.Year * DaggerfallDateTime.MonthsPerYear * DaggerfallDateTime.DaysPerMonth + offset) % 32;
        if (moonRatio == 31)
            return true;

        return false;
    }

    private static void DungeonMoon()
    {
        playerEnterExit.MovePlayerToDungeonStart();
        LycanthropyEffect lycanthropyEffect = GameManager.Instance.PlayerEffectManager.GetRacialOverrideEffect() as LycanthropyEffect;
        lycanthropyEffect.MorphSelf();
        playerEntity.LastTimePlayerAteOrDrankAtTavern = DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime.ToClassicDaggerfallTime() - 250;
        KillAll(false);
    }

    private static void ResetNeedToKillInnocent()
    {
        LycanthropyEffect lycanthropyEffect = GameManager.Instance.PlayerEffectManager.GetRacialOverrideEffect() as LycanthropyEffect;
        lycanthropyEffect.UpdateSatiation();
    }
}
