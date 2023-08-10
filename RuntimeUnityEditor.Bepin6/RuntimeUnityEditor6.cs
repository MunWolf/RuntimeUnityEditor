using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using RuntimeUnityEditor.Core;
using RuntimeUnityEditor.Core.Utils.Abstractions;
using UnityEngine;

namespace RuntimeUnityEditor.Bepin6
{
    /// <summary>
    /// This is a loader plugin for BepInEx5.
    /// When referencing RuntimeUnityEditor from other code it's recommended to not reference this assembly and instead reference RuntimeUnityEditorCore directly.
    /// If you need your code to run after RUE is initialized, add a <code>[BepInDependency(RuntimeUnityEditorCore.GUID)]</code> attribute to your plugin.
    /// You can see if RuntimeUnityEditor has finished loading with <code>RuntimeUnityEditorCore.IsInitialized()</code>.
    /// </summary>
    [Obsolete("It's recommended to reference RuntimeUnityEditorCore directly")]
    [BepInPlugin(RuntimeUnityEditorCore.GUID, "Runtime Unity Editor", RuntimeUnityEditorCore.Version)]
    public class RuntimeUnityEditor6 : BaseUnityPlugin
    {
        public static RuntimeUnityEditorCore Instance { get; private set; }

        private void Start()
        {
            if (!TomlTypeConverter.CanConvert(typeof(Rect)))
            {
                var converter = RuntimeUnityEditor.Core.Utils.TomlTypeConverter.GetConverter(typeof(Rect));
                TomlTypeConverter.AddConverter(typeof(Rect), new TypeConverter { ConvertToObject = converter.ConvertToObject, ConvertToString = converter.ConvertToString });
            }

            Instance = new RuntimeUnityEditorCore(new Bep6InitSettings(this));
        }

        private void Update()
        {
            Instance.Update();
        }

        private void LateUpdate()
        {
            Instance.LateUpdate();
        }

        private void OnGUI()
        {
            Instance.OnGUI();
        }

        private sealed class Bep6InitSettings : InitSettings
        {
            private readonly RuntimeUnityEditor6 _instance;

            public Bep6InitSettings(RuntimeUnityEditor6 instance)
            {
                _instance = instance;
                LoggerWrapper = new Logger6(_instance.Logger);
            }

            public override Action<T> RegisterSetting<T>(string category, string name, T defaultValue, string description, Action<T> onValueUpdated)
            {
                var s = _instance.Config.Bind(category, name, defaultValue, description);
                s.SettingChanged += (sender, args) => onValueUpdated(s.Value);
                onValueUpdated(s.Value);
                return x => s.Value = x;
            }

            public override MonoBehaviour PluginMonoBehaviour => _instance;
            public override ILoggerWrapper LoggerWrapper { get; }
            public override string ConfigPath => Paths.ConfigPath;
        }

        private sealed class Logger6 : ILoggerWrapper
        {
            private readonly ManualLogSource _logger;

            public Logger6(ManualLogSource logger)
            {
                _logger = logger;
            }

            public void Log(Core.Utils.Abstractions.LogLevel logLevel, object content)
            {
                _logger.Log((BepInEx.Logging.LogLevel)logLevel, content);
            }
        }
    }
}