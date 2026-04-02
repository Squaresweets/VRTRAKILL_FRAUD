using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using Valve.VR;
using Plugin.Systems;
using VRTRAKILL.Utilities;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using Unity.XR.OpenVR;
using System.Collections.Generic;
using System;
using System.Text;
using UnityEngine.XR.OpenXR.Input;
using UnityEngine.SceneManagement;
using System.Collections;
using HarmonyLib;
using System.CodeDom;

namespace Plugin
{
    // note: i will NEVER use transpilers IN THIS LIFETIME!! OVER MY DEAD BODY!!

    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public sealed partial class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log { get; private set; }

        public void Awake()
        {
            Log = Logger;
            Debug.unityLogger.filterLogType = LogType.Warning;

            Prefs.ConfigMaster.Init();
            PatchStuff();
            SceneWorker.Init();

            InitVRLoader();
        }

        private void PatchStuff()
        {
            System.Collections.Generic.List<string> Namespaces = new System.Collections.Generic.List<string>
            {
                typeof(Patches.Controllers.A).Namespace,
                typeof(Patches.Misc.A).Namespace,
                typeof(Patches.ULTRAKILL.A).Namespace,
                typeof(Patches.Visual.A).Namespace,

                typeof(Systems.VRCamera.Patches.A).Namespace,
                typeof(Systems.UI.Patches.A).Namespace,
                typeof(Systems.Movement.Patches.A).Namespace,
            };
            System.Collections.Generic.List<System.Type> Types = new System.Collections.Generic.List<System.Type>
            {
                //typeof(Systems.Input.ControlMessages.Patches),
            };
            //if (Vars.Config.Controllers.EnableHaptics) Types.Add(typeof(Systems.Controllers.Patches.ControllerHaptics));
            //if (Vars.Config.EnableCBS)                 Namespaces.Add(typeof(Systems.Guns.Patches.A).Namespace);
            //if (Vars.Config.EnableMBP)                 Namespaces.Add(typeof(Systems.Arms.Patches.A).Namespace);
            //if (!Vars.Config.MBP.CameraWhiplash)       Namespaces.Add(typeof(Systems.Arms.Patches.Whiplash.A).Namespace);
            //if (Vars.Config.EnableVRBody)              Namespaces.Add(typeof(Systems.VRAvatar.Patches.A).Namespace);

            new Patcher(new HarmonyLib.Harmony(PluginInfo.PLUGIN_GUID))
            {
                Namespaces = Namespaces.ToArray(),
                Types = Types.ToArray(),
                Log = Vars.Log,
            }.PatchAll();
        }

        public static void InitVRLoader()
        {
            Log.LogMessage("INITILIZING");
            SteamVR_Actions.PreInitialize();

            var generalSettings = ScriptableObject.CreateInstance<XRGeneralSettings>();
            var managerSettings = ScriptableObject.CreateInstance<XRManagerSettings>();
            var xrLoader = ScriptableObject.CreateInstance<OpenVRLoader>();

            var settings = OpenVRSettings.GetSettings();
            settings.StereoRenderingMode = OpenVRSettings.StereoRenderingModes.SinglePassInstanced;
            
            generalSettings.Manager = managerSettings;

            ((List<XRLoader>)managerSettings.activeLoaders).Clear();
            ((List<XRLoader>)managerSettings.activeLoaders).Add(xrLoader);
            managerSettings.InitializeLoaderSync();

            Log.LogMessage("Active loader: " + managerSettings.activeLoader);

            SteamVR.Initialize(true);
            Log.LogMessage("SteamVR initialized. Active: " + SteamVR.active + " Connected: " + SteamVR.initializedState);

            XRLoader loader = XRGeneralSettings.Instance.Manager.activeLoader;
            if (loader != null)
            {
                Log.LogMessage("Initilizing:" + loader.Start());
            }
            else Log.LogError("ERROR AAA");

            managerSettings.activeLoader.GetLoadedSubsystem<XRDisplaySubsystem>().Start();

            SteamVR_Input.Initialize();
            Systems.Input.SVRActionsManager.Init();
            Log.LogMessage("DONE");
        }
    }
}