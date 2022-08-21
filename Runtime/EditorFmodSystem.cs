using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD;
using FMOD.Studio;
using System;
using UnityEditor;

namespace Hermes
{
    public class EditorFmodSystem : MonoBehaviour //TODO: Should this be a singleton??
    {

        private static FMOD.Studio.System system;
        private static FMOD.SPEAKERMODE speakerMode;
        private static string encryptionKey;
        private static List<FMOD.Studio.EventInstance> previewEventInstances = new List<FMOD.Studio.EventInstance>();
        private static List<FMOD.Studio.Bank> loadedPreviewBanks = new List<FMOD.Studio.Bank>();
        private static List<string> temporaryBanks = new List<string> { "Master.bank", "Master.strings.bank" }; //TODO: How to get all the banks progrmatically??

        //E:/Unity Projects/Hermes/FMOD Project/Hermes tests/Build/Desktop/
        public string BankPath;
        public static string m_bankPathStatic { get; private set; }

        struct LoadedBank
        {
            public FMOD.Studio.Bank Bank;
            public int RefCount;
        }

        Dictionary<string, LoadedBank> loadedBanks = new Dictionary<string, LoadedBank>();

        public static bool PreviewBanksLoaded
        {
            get { return loadedPreviewBanks.Count > 0; }
        }

        public static FMOD.Studio.System System
        {
            get
            {
                if (!system.isValid())
                {
                    CreateSystem();
                }
                return system;
            }
        }

        private void Awake()
        {
            m_bankPathStatic = BankPath;
        }

        private static void CreateSystem()
        {
            //RuntimeUtils.DebugLog("FMOD Studio: Creating editor system instance");
            RuntimeUtils.EnforceLibraryOrder();

            /*FMOD.RESULT result = FMOD.Debug.Initialize(FMOD.DEBUG_FLAGS.LOG, FMOD.DEBUG_MODE.FILE, null, "fmod_editor.log");
            if (result != FMOD.RESULT.OK)
            {
                RuntimeUtils.DebugLogWarning("FMOD Studio: Cannot open fmod_editor.log. Logging will be disabled for importing and previewing");
            }*/

            CheckResult(FMOD.Studio.System.create(out system));

            FMOD.System lowlevel;
            CheckResult(system.getCoreSystem(out lowlevel));

            // Use play-in-editor speaker mode for event browser preview and metering
            speakerMode = Settings.Instance.GetEditorSpeakerMode();
            CheckResult(lowlevel.setSoftwareFormat(0, speakerMode, 0));

            encryptionKey = Settings.Instance.EncryptionKey;
            if (!string.IsNullOrEmpty(encryptionKey))
            {
                FMOD.Studio.ADVANCEDSETTINGS studioAdvancedSettings = new FMOD.Studio.ADVANCEDSETTINGS();
                CheckResult(system.setAdvancedSettings(studioAdvancedSettings, encryptionKey));
            }

            CheckResult(system.initialize(256, FMOD.Studio.INITFLAGS.ALLOW_MISSING_PLUGINS | FMOD.Studio.INITFLAGS.SYNCHRONOUS_UPDATE, FMOD.INITFLAGS.NORMAL, IntPtr.Zero));

            FMOD.ChannelGroup master;
            CheckResult(lowlevel.getMasterChannelGroup(out master));
            FMOD.DSP masterHead;
            CheckResult(master.getDSP(FMOD.CHANNELCONTROL_DSP_INDEX.HEAD, out masterHead));
            CheckResult(masterHead.setMeteringEnabled(false, true));

            LoadPreviewBanks();
        }

        /*private static void LoadBanks()
        {
            var bankPath1 = "Master";
            var bankPath2 = "Master.strings";
            LoadedBank loadedBank = new LoadedBank();
            system.loadBankFile(bankPath1, FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out loadedBank.Bank);
        }*/

        public static void LoadPreviewBanks()
        {
            /*if (PreviewBanksLoaded)
            {
                return;
            }*/

            foreach (var bank in temporaryBanks)
            {
                FMOD.Studio.Bank previewBank;
                var filess = Application.dataPath; //TODO: Find a way to get the fmod bank location for any user...
                FMOD.RESULT result = System.loadBankFile("E:/Unity Projects/Hermes/FMOD Project/Hermes tests/Build/Desktop/" + bank, FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out previewBank);
                if (result != FMOD.RESULT.ERR_EVENT_ALREADY_LOADED) // ignore error when a bank is already loaded, e.g. localized banks.
                {
                    CheckResult(result);
                }
                loadedPreviewBanks.Add(previewBank);
            }
        }

        public static void CheckResult(FMOD.RESULT result)
        {
            if (result != FMOD.RESULT.OK)
            {
                RuntimeUtils.DebugLogError(string.Format("FMOD Studio: Encountered Error: {0} {1}", result, FMOD.Error.String(result)));
            }
        }

        private static void Update()
        {
            // Update the editor system
            if (system.isValid())
            {
                CheckResult(system.update());

                if (speakerMode != Settings.Instance.GetEditorSpeakerMode())
                {
                    RecreateSystem();
                }

                if (encryptionKey != Settings.Instance.EncryptionKey)
                {
                    RecreateSystem();
                }
            }

            for (int i = 0; i < previewEventInstances.Count; i++)
            {
                var instance = previewEventInstances[i];
                if (instance.isValid())
                {
                    FMOD.Studio.PLAYBACK_STATE state;
                    instance.getPlaybackState(out state);
                    if (state == FMOD.Studio.PLAYBACK_STATE.STOPPED)
                    {
                        PreviewStop(instance);
                        i--;
                    }
                }
            }
        }

        private static void RecreateSystem()
        {
            StopAllPreviews();
            DestroySystem();
            CreateSystem();
        }

        private static void DestroySystem()
        {
            if (system.isValid())
            {
                RuntimeUtils.DebugLog("FMOD Studio: Destroying editor system instance");
                UnloadPreviewBanks();
                system.release();
                system.clearHandle();
            }
        }

        public static void UnloadPreviewBanks()
        {
            if (!PreviewBanksLoaded)
            {
                return;
            }

            loadedPreviewBanks.ForEach(x => { x.unload(); x.clearHandle(); });
            loadedPreviewBanks.Clear();
        }

        public static void StopAllPreviews()
        {
            foreach (FMOD.Studio.EventInstance eventInstance in previewEventInstances)
            {
                PreviewStop(eventInstance);
            }
        }

        public static FMOD.Studio.EventInstance PreviewEvent(EventReference eventRef, /*Dictionary<string, float> previewParamValues,*/ float volume = 1)
        {
            FMOD.Studio.EventDescription eventDescription;
            FMOD.Studio.EventInstance eventInstance;

            LoadPreviewBanks();

            CheckResult(System.getEventByID(eventRef.Guid, out eventDescription));
            CheckResult(eventDescription.createInstance(out eventInstance));

            /*foreach (EventReference param in eventRef.Parameters)
            {
                FMOD.Studio.PARAMETER_DESCRIPTION paramDesc;
                CheckResult(eventDescription.getParameterDescriptionByName(param.Name, out paramDesc));
                param.ID = paramDesc.id;
                if (param.IsGlobal)
                {
                    CheckResult(System.setParameterByID(param.ID, previewParamValues[param.Name]));
                }
                else
                {
                    CheckResult(eventInstance.setParameterByID(param.ID, previewParamValues[param.Name]));
                }
            }*/

            CheckResult(eventInstance.setVolume(volume));
            CheckResult(eventInstance.start());

            previewEventInstances.Add(eventInstance);

            return eventInstance;
        }

        public static void PreviewStop(FMOD.Studio.EventInstance eventInstance, FMOD.Studio.STOP_MODE stopMode = FMOD.Studio.STOP_MODE.IMMEDIATE)
        {
            if (previewEventInstances.Contains(eventInstance))
            {
                previewEventInstances.Remove(eventInstance);
                if (eventInstance.isValid())
                {
                    eventInstance.stop(stopMode);
                    eventInstance.release();
                    eventInstance.clearHandle();
                }
            }
        }
    }
}
