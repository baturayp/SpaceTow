using System.IO;
using System.Xml;
using System.Xml.Serialization;
using DeltaDNA;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.GameGrowth
{
    public static class DeltaDNAConfigurationHelper
    {
        public static void UpdateEnvironment(bool isDevelopment)
        {
            var ddnaConfiguration = Configuration.GetAssetInstance();
            ddnaConfiguration.environmentKey = isDevelopment ? 0 : 1;
#if UNITY_EDITOR
            EditorUtility.SetDirty(ddnaConfiguration);
#endif
        }

        public static bool IsInDevelopmentMode()
        {
            return Configuration.GetAssetInstance().environmentKey == 0;
        }
    }
}
