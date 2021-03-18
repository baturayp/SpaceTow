using System.IO;
using System.Xml;
using System.Xml.Serialization;
#if GAMEGROWTH_UNITY_MONETIZATION
using UnityEditor.GameGrowth.Helpers.UnityMediation;
#endif
using UnityEngine;
using UnityEngine.GameGrowth;

namespace UnityEditor.GameGrowth
{
    public static class GameGrowthAutomaticConfiguration
    {
        const string k_AssetsFolder = "Assets";
        static readonly string k_CannotAutoConfigureLog = L10n.Tr("Cannot auto-configure Game Growth Sdks.");

        [InitializeOnLoadMethod]
        static void InitializeConfiguration()
        {
            GameGrowthConfigurationAsset.onConfigurationUpdated -= InitializationCallback;
            GameGrowthConfigurationAsset.onConfigurationUpdated += InitializationCallback;
        }

        static void InitializationCallback(GameGrowthConfigurationAsset gameGrowthConfiguration)
        {
            ConfigureSdks(gameGrowthConfiguration);
        }

        public static void ConfigureSdks(GameGrowthConfigurationAsset gameGrowthConfiguration)
        {
            if (gameGrowthConfiguration == null)
            {
                Debug.LogWarning(k_CannotAutoConfigureLog);
                return;
            }

            var isFullIntegration = IntegrationLevelExtensions.IsFull(IntegrationLevelExtensions.GetParsedIntegrationLevel(gameGrowthConfiguration.projectSummary.integrationLevel));
            ApplyDeltaDnaConfiguration(gameGrowthConfiguration);

            if (isFullIntegration)
            {
#if GAMEGROWTH_ADMOB
                ApplyAdMobConfiguration(gameGrowthConfiguration);
#endif

#if GAMEGROWTH_UNITY_MONETIZATION
                EnableUnityMonetizationRequiredAdapters();
                ApplyAdMobConfigurationForUnityMonetization(gameGrowthConfiguration);
#endif
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        static void ApplyDeltaDnaConfiguration(GameGrowthConfigurationAsset gameGrowthConfiguration)
        {
            var config = GetDeltaDnaConfiguration();

            config.environmentKeyDev = gameGrowthConfiguration.projectSummary.providers.ddna.devKey.value;
            config.environmentKeyLive = gameGrowthConfiguration.projectSummary.providers.ddna.liveKey.value;
            config.collectUrl = gameGrowthConfiguration.projectSummary.providers.ddna.collect;
            config.engageUrl = gameGrowthConfiguration.projectSummary.providers.ddna.engage;

            if (string.IsNullOrEmpty(config.clientVersion))
            {
                config.useApplicationVersion = true;
            }

            EditorUtility.SetDirty(config);
        }

        static DeltaDNA.Configuration GetDeltaDnaConfiguration()
        {
            var config = AssetDatabase.LoadAssetAtPath<DeltaDNA.Configuration>(DeltaDNA.Configuration.FULL_ASSET_PATH);

            if (config != null)
            {
                return config;
            }

            // If we couldn't load the asset we should create a new instance.
            config = ScriptableObject.CreateInstance<DeltaDNA.Configuration>();

            if (!AssetDatabase.IsValidFolder(DeltaDNA.Configuration.ASSET_DIRECTORY))
            {
                AssetDatabase.CreateFolder(DeltaDNA.Configuration.RESOURCES_CONTAINER, DeltaDNA.Configuration.RESOURCES_DIRECTORY);
            }
            AssetDatabase.CreateAsset(config, DeltaDNA.Configuration.FULL_ASSET_PATH);
            AssetDatabase.SaveAssets();

            return config;
        }

#if GAMEGROWTH_ADMOB
        static void ApplyAdMobConfiguration(GameGrowthConfigurationAsset gameGrowthConfiguration)
        {
            var config = AdMobConfiguration.LoadMainAsset();
            if (config != null)
            {
                config.androidAppId = gameGrowthConfiguration.projectSummary.providers.admob.appId.androidValue;
                config.iOSAppId = gameGrowthConfiguration.projectSummary.providers.admob.appId.iOSValue;

                EditorUtility.SetDirty(config);
            }
        }

#endif

#if GAMEGROWTH_UNITY_MONETIZATION
        static void EnableUnityMonetizationRequiredAdapters()
        {
            UnityMonetizationAdapterHelper.EnableRequiredAdNetworks();
        }

        static void ApplyAdMobConfigurationForUnityMonetization(GameGrowthConfigurationAsset gameGrowthConfiguration)
        {
            AdMobConfigurationHelper.SetAdMobConfigurationForUnityMonetization(
                gameGrowthConfiguration.projectSummary.providers.admob.appId.androidValue,
                gameGrowthConfiguration.projectSummary.providers.admob.appId.iOSValue);
        }

#endif
    }
}
