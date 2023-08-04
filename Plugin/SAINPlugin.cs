﻿using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using DrakiaXYZ.VersionChecker;
using SAIN.Components;
using SAIN.Editor;
using SAIN.Helpers;
using SAIN.Plugin;
using SAIN.SAINPreset;
using System;
using UnityEngine;
using static SAIN.PluginInfo;

namespace SAIN
{
    [BepInPlugin(GUID, Name, PluginInfo.Version)]
    [BepInDependency(SPT.GUID, SPT.Version)]
    [BepInDependency(BigBrain.GUID, BigBrain.Version)]
    [BepInDependency(Waypoints.GUID, Waypoints.Version)]
    [BepInProcess("EscapeFromTarkov.exe")]
    public class SAINPlugin : BaseUnityPlugin
    {
        private void Awake()
        {
            if (!TarkovVersion.CheckEftVersion(Logger, Info, Config))
            {
                throw new Exception("Invalid EFT Version");
            }

            OpenEditorButton = Config.Bind("SAIN Editor", "Open Editor", false, "Opens the Editor on press");
            OpenEditorConfigEntry = Config.Bind("SAIN Editor", "Open Editor Shortcut", new KeyboardShortcut(KeyCode.F6), "The keyboard shortcut that toggles editor");
            PauseConfigEntry = Config.Bind("SAIN Editor", "PauseButton", new KeyboardShortcut(KeyCode.Pause), "Pause The Game");

            PresetHandler.Init();

            new Patches.Generic.KickPatch().Enable();
            new Patches.Generic.GetBotController().Enable();
            new Patches.Generic.GetBotSpawnerClass().Enable();
            new Patches.Generic.GrenadeThrownActionPatch().Enable();
            new Patches.Generic.GrenadeExplosionActionPatch().Enable();
            new Patches.Generic.BotGroupAddEnemyPatch().Enable();
            new Patches.Generic.BotMemoryAddEnemyPatch().Enable();

            new Patches.Hearing.InitiateShotPatch().Enable();
            new Patches.Hearing.TryPlayShootSoundPatch().Enable();
            new Patches.Hearing.HearingSensorPatch().Enable();
            new Patches.Hearing.BetterAudioPatch().Enable();

            new Patches.Talk.PlayerTalkPatch().Enable();
            new Patches.Talk.TalkDisablePatch1().Enable();
            new Patches.Talk.TalkDisablePatch2().Enable();
            new Patches.Talk.TalkDisablePatch3().Enable();
            new Patches.Talk.TalkDisablePatch4().Enable();

            new Patches.Vision.NoAIESPPatch().Enable();
            new Patches.Vision.VisionSpeedPatch().Enable();
            new Patches.Vision.VisibleDistancePatch().Enable();
            new Patches.Vision.CheckFlashlightPatch().Enable();

            new Patches.Shoot.AimTimePatch().Enable();
            new Patches.Shoot.AimOffsetPatch().Enable();
            new Patches.Shoot.RecoilPatch().Enable();
            new Patches.Shoot.LoseRecoilPatch().Enable();
            new Patches.Shoot.EndRecoilPatch().Enable();
            new Patches.Shoot.FullAutoPatch().Enable();
            new Patches.Shoot.SemiAutoPatch().Enable();

            BigBrainSAIN.Init();
            VectorHelpers.Init();
        }

        public static ConfigEntry<bool> OpenEditorButton;
        public static ConfigEntry<KeyboardShortcut> OpenEditorConfigEntry;
        public static ConfigEntry<KeyboardShortcut> PauseConfigEntry;

        public static SAINPresetClass LoadedPreset => PresetHandler.LoadedPreset;

        public static readonly SAINEditor SAINEditor = new SAINEditor();

        public static ConfigFile SAINConfig { get; private set; }

        private void Update()
        {
            CheckMods.Update();
            SAINEditor.Update();
            BotControllerHandler.Update();
        }

        private void Start() => SAINEditor.Init();
        private void LateUpdate() => SAINEditor.LateUpdate();
        private void OnGUI() => SAINEditor.OnGUI();

        public static SAINBotController BotController => BotControllerHandler.BotController;
    }

    public static class CheckMods
    {
        static CheckMods()
        {
            ModsCheckTimer = Time.time + 5f;
        }

        public static void Update()
        {
            if (!ModsChecked && ModsCheckTimer < Time.time && ModsCheckTimer > 0)
            {
                ModsChecked = true;

                if (Chainloader.PluginInfos.ContainsKey(RealismMod.GUID))
                {
                    RealismMod.Loaded = true;
                    Logger.LogInfo($"SAIN: {RealismMod.Name} Detected. Auto-Adjusting Recoil for Bots...");
                }
                if (Chainloader.PluginInfos.ContainsKey(LootingBots.GUID))
                {
                    LootingBots.Loaded = true;
                    Logger.LogInfo($"SAIN: {LootingBots.Name} Detected.");
                }
            }
        }

        private static float ModsCheckTimer = -1f;
        private static bool ModsChecked = false;
        public static bool RealismLoaded = false;
        public static bool LootingBotsLoaded = false;
    }
}