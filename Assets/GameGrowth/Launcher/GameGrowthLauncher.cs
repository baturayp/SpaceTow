using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Serialization;
using com.adjust.sdk;
using DeltaDNA;

#if GAMEGROWTH_UNITY_MONETIZATION
using Unity.Mediation;
#endif

#if GAMEGROWTH_PURCHASE_VERIFICATION
using com.adjust.sdk.purchase;
#endif

namespace UnityEngine.GameGrowth
{
    public class GameGrowthLauncher : MonoBehaviour
    {
        public const string appTokenDefaultText = "{Your App Token}";
        public const string androidAppTokenDefaultText = "{Your Android App Token}";
        public const string iosAppTokenDefaultText = "{Your iOS App Token}";

        public const string purchaseEventTokenDefaultText = "{Your Purchase Event Token}";
        public const string androidPurchaseEventTokenDefaultText = "{Your Android Purchase Event Token}";
        public const string iosPurchaseEventTokenDefaultText = "{Your iOS Purchase Event Token}";

        const string k_MissingAdjustAttributionMessage = "Missing adjustAttribution, cannot record adjustAttribution event with Delta DNA";
        const string k_MissingAdImpressionDataMessage = "Missing adImpressionData, cannot record adImpression event with Delta DNA";
        const string k_MissingTransactionDataMessage = "Missing transactionData, cannot record transaction event with Delta DNA";

        static IDataPlatformAccessLayer s_DataPlatformAccessLayer;
        static DeltaDNA.GameEvent s_AdjustAttributionEvent;

        //Game Growth
        [SerializeField]
        GameGrowthConfigurationAsset m_DefaultConfiguration = null;
        [SerializeField]
        GameGrowthConfiguration m_Configuration;

        //Adjust
        [SerializeField]
        bool m_StartAdjustManually;
        [SerializeField]
        bool m_AdjustEventBuffering;
        [SerializeField]
        bool m_AdjustSendInBackground;
        [SerializeField]
        bool m_AdjustLaunchDeferredDeeplink = true;
        [SerializeField]
        string m_AdjustAndroidAppToken = androidAppTokenDefaultText;
        [SerializeField]
        string m_AdjustAndroidPurchaseEventToken = androidPurchaseEventTokenDefaultText;
        [SerializeField]
        string m_AdjustIosAppToken = iosAppTokenDefaultText;
        [SerializeField]
        string m_AdjustIosPurchaseEventToken = iosPurchaseEventTokenDefaultText;

        [SerializeField]
        string m_AdjustAppToken = appTokenDefaultText;
        [SerializeField]
        string m_AdjustPurchaseEventToken = purchaseEventTokenDefaultText;

        [SerializeField]
        bool m_AdjustUseSameToken = true;
        [SerializeField]
        bool m_AdjustOverrideTokens;

        [SerializeField]
        AdjustLogLevel m_AdjustLogLevel = AdjustLogLevel.Info;

#if UNITY_ANDROID
        UnityThreadUtil m_MainThreadCaller;
#endif

#if UNITY_ANDROID || UNITY_IPHONE
        string m_AdjustPlatformAppToken;
        string m_AdjustPlatformPurchaseEventToken;
#endif

        public GameGrowthConfigurationAsset DefaultConfiguration
        {
            get { return m_DefaultConfiguration; }
        }

        public GameGrowthConfiguration configuration
        {
            get => m_Configuration;
            set => m_Configuration = value;
        }

        public bool startAdjustManually
        {
            get => m_StartAdjustManually;
            set => m_StartAdjustManually = value;
        }
        public bool adjustEventBuffering
        {
            get => m_AdjustEventBuffering;
            set => m_AdjustEventBuffering = value;
        }
        public bool adjustSendInBackground
        {
            get => m_AdjustSendInBackground;
            set => m_AdjustSendInBackground = value;
        }
        public bool adjustLaunchDeferredDeepLink
        {
            get => m_AdjustLaunchDeferredDeeplink;
            set => m_AdjustLaunchDeferredDeeplink = value;
        }
        public string adjustAndroidAppToken
        {
            get => m_AdjustAndroidAppToken;
            set => m_AdjustAndroidAppToken = value;
        }

        public string adjustAndroidPurchaseEventToken
        {
            get => m_AdjustAndroidPurchaseEventToken;
            set => m_AdjustAndroidPurchaseEventToken = value;
        }

        public string adjustIosAppToken
        {
            get => m_AdjustIosAppToken;
            set => m_AdjustIosAppToken = value;
        }

        public string adjustIosPurchaseEventToken
        {
            get => m_AdjustIosPurchaseEventToken;
            set => m_AdjustIosPurchaseEventToken = value;
        }

        public bool adjustUseSameToken
        {
            get => m_AdjustUseSameToken;
            set => m_AdjustUseSameToken = value;
        }

        public bool adjustOverrideTokens
        {
            get => m_AdjustOverrideTokens;
            set => m_AdjustOverrideTokens = value;
        }

        public string adjustAppToken
        {
            get => m_AdjustAppToken;
            set => m_AdjustAppToken = value;
        }

        public string adjustPurchaseEventToken
        {
            get => m_AdjustPurchaseEventToken;
            set => m_AdjustPurchaseEventToken = value;
        }

        public AdjustLogLevel adjustLogLevel
        {
            get => m_AdjustLogLevel;
            set => m_AdjustLogLevel = value;
        }

        //Delta DNA
        bool m_IsDeltaDNAInDevMode;

        [SerializeField]
        bool m_StartDeltaDnaManually;

        [SerializeField]
        AttributionCallbackHandler m_AttributionChangedCallbackHandler = new AttributionCallbackHandler();
        [SerializeField]
        AdImpressionHandler m_AdImpressionHandler = new AdImpressionHandler();
        [SerializeField]
        TransactionHandler m_TransactionHandler = new TransactionHandler();

        public bool startDeltaDnaManually
        {
            get => m_StartDeltaDnaManually;
            set => m_StartDeltaDnaManually = value;
        }

        public AttributionCallbackHandler attributionChangedCallbackHandler
        {
            get => m_AttributionChangedCallbackHandler;
            set => m_AttributionChangedCallbackHandler = value;
        }

        public AdImpressionHandler adImpressionHandler
        {
            get => m_AdImpressionHandler;
            set => m_AdImpressionHandler = value;
        }

        public TransactionHandler transactionHandler
        {
            get => m_TransactionHandler;
            set => m_TransactionHandler = value;
        }

#if GAMEGROWTH_UNITY_MONETIZATION
        //Mediation
        [SerializeField]
        bool m_UseDefaultImpressionTrackedHandler = true;

        public bool useDefaultImpressionTrackedHandler
        {
            get => m_UseDefaultImpressionTrackedHandler;
            set => m_UseDefaultImpressionTrackedHandler = value;
        }

#endif

        private void Start()
        {
            LogDebug("Start::Begin");
            LogDebug($"Start::environment:{m_Configuration.environment}");
            GameGrowthEnvironmentValidator.LogStatus(m_Configuration.environment);
            LogDebug("Start::End");
        }

        void Awake()
        {
            LogDebug("Awake::Begin");
            DontDestroyOnLoad(this.gameObject);

#if UNITY_ANDROID
            m_MainThreadCaller = gameObject.AddComponent<UnityThreadUtil>();
            m_AdjustPlatformAppToken = adjustAndroidAppToken;
            m_AdjustPlatformPurchaseEventToken = adjustAndroidPurchaseEventToken;
#elif UNITY_IPHONE
            m_AdjustPlatformAppToken = adjustIosAppToken;
            m_AdjustPlatformPurchaseEventToken = adjustIosPurchaseEventToken;
#endif
            SetupDeltaDna();
            SetupAdjust();
#if GAMEGROWTH_PURCHASE_VERIFICATION
            SetupAdjustPurchase();
#endif
#if GAMEGROWTH_UNITY_MONETIZATION
            SetupUnityMediation();
#endif
            s_DataPlatformAccessLayer = new DataPlatformAccessLayer();

            if (!startAdjustManually && !startDeltaDnaManually)
            {
                CollectIds();
            }
            LogDebug("Awake::End");
        }

        void OnApplicationPause(bool pauseStatus)
        {
            LogDebug("OnApplicationPause::Begin");
            LogDebug("OnApplicationPause::End");
        }

        void SetupDeltaDna()
        {
            LogDebug("SetupDeltaDna::Begin");

            if (!startDeltaDnaManually)
            {
                LogDebug("SetupDeltaDna::auto start");
                DDNA.Instance.SetLoggingLevel(configuration.debugLog == GameGrowthDebugLog.Off ? DeltaDNA.Logger.Level.INFO : DeltaDNA.Logger.Level.DEBUG);
                DDNA.Instance.StartSDK();
                m_IsDeltaDNAInDevMode = DeltaDNAConfigurationHelper.IsInDevelopmentMode();
            }
            else
            {
                LogDebug("SetupDeltaDna::Expected to start manually");
            }
            LogDebug("SetupDeltaDna::End");
        }
        
        string GetAppAdjustToken()
        {
#if UNITY_ANDROID || UNITY_IPHONE
            if (!m_AdjustOverrideTokens)
            {
                LogDebug("GetAppAdjustToken::not overriding tokens");
#if UNITY_ANDROID
                return m_DefaultConfiguration.projectSummary.providers.adjust.appToken.androidValue;
#elif UNITY_IPHONE
                return m_DefaultConfiguration.projectSummary.providers.adjust.appToken.iOSValue;
#endif
            }
            else
            {
                LogDebug("GetAppAdjustToken::overriding tokens");
                return m_AdjustPlatformAppToken;
            }
#else
            return "";
#endif
        }

        void SetupAdjust()
        {
            LogDebug("SetupAdjust::Begin");
#if UNITY_ANDROID || UNITY_IPHONE
            if (!startAdjustManually)
            {
                LogDebug("SetupAdjust::auto start");
                var adjustPlatformAppTokens = GetAppAdjustToken();
                
                LogDebug("SetupAdjust::AdjustConfig contents:"
                    + $"\n environment:{m_Configuration.environment}"
                    + $"\n logLevel:{adjustLogLevel}"
                    + $"\n eventBufferingEnabled:{adjustEventBuffering}"
                    + $"\n sendInBackground:{adjustSendInBackground}"
                    + $"\n launchDeferredDeeplink:{adjustLaunchDeferredDeepLink}"
                );
                var adjustConfig = new AdjustConfig(adjustPlatformAppTokens, AdjustHelper.GetAdjustEnvironment(m_Configuration.environment))
                {
                    logLevel = adjustLogLevel,
                    eventBufferingEnabled = adjustEventBuffering,
                    sendInBackground = adjustSendInBackground,
                    launchDeferredDeeplink = adjustLaunchDeferredDeepLink,
                    needsCost = true
                };

                adjustConfig.setAttributionChangedDelegate(attribution =>
                {
                    attributionChangedCallbackHandler.Invoke(attribution);
                }, name);

                Adjust.start(adjustConfig);
            }
            else
            {
                LogDebug("SetupAdjust::Expected to start manually");
            }
#endif
            LogDebug("SetupAdjust::End");
        }

#if UNITY_IOS
        public void GetNativeAttribution(string attributionData)
        {
            Adjust.GetNativeAttribution(attributionData);
        }

        public void GetNativeEventSuccess(string eventSuccessData)
        {
            Adjust.GetNativeEventSuccess(eventSuccessData);
        }

        public void GetNativeEventFailure(string eventFailureData)
        {
            Adjust.GetNativeEventFailure(eventFailureData);
        }

        public void GetNativeSessionSuccess(string sessionSuccessData)
        {
            Adjust.GetNativeSessionSuccess(sessionSuccessData);
        }

        public void GetNativeSessionFailure(string sessionFailureData)
        {
            Adjust.GetNativeSessionFailure(sessionFailureData);
        }

        public void GetNativeDeferredDeeplink(string deeplinkURL)
        {
            Adjust.GetNativeDeferredDeeplink(deeplinkURL);
        }

        public void GetAuthorizationStatus(string authorizationStatus)
        {
            Adjust.GetAuthorizationStatus(authorizationStatus);
        }

#endif

#if GAMEGROWTH_PURCHASE_VERIFICATION
        void SetupAdjustPurchase()
        {
            LogDebug("SetupAdjustPurchase::Begin");
#if UNITY_ANDROID || UNITY_IPHONE
            var purchaseLogLevel = AdjustHelper.GetPurchaseLogLevel(adjustLogLevel);
            var purchaseEnvironment = AdjustHelper.GetAdjustPurchaseEnvironment(m_Configuration.environment);

            if (!startAdjustManually)
            {
               LogDebug("SetupAdjustPurchase::auto start");
               
               var adjustPlatformAppTokens = GetAppAdjustToken();
               
               LogDebug("SetupAdjustPurchase::config content"
                    + $"\n app token:{adjustPlatformAppTokens}"
                    + $"\n environment:{purchaseEnvironment}"
                );
                AdjustPurchase.Init(new ADJPConfig(adjustPlatformAppTokens, purchaseEnvironment)
                {
                    logLevel = purchaseLogLevel,
                });
            }
            else
            {
                LogDebug("SetupAdjustPurchase::Expected to start manually");
            }
#endif
            LogDebug("SetupAdjustPurchase::End");
        }

#endif

        public void CollectIds()
        {
            LogDebug("CollectIds::Begin");
            if (!Analytics.Analytics.playerOptedOut)
            {
#if UNITY_ANDROID
                LogDebug("CollectIds::getGoogleAdId invoked");
                Adjust.getGoogleAdId(googleAdId =>
                {
                    LogDebug("CollectIds::Collected Ids:"
                        + $"\n idfa:{string.Empty}"
                        + $"\n googleAdId:{googleAdId}"
                        + $"\n adjustId:{Adjust.getAdid()}"
                        + $"\n ddnaUserId:{DDNA.Instance.UserID}"
                        + $"\n ddnaEnvKey:{DDNA.Instance.EnvironmentKey}"
                        + $"\n deviceId:{SystemInfo.deviceUniqueIdentifier}"
                        + $"\n analyticsId:{Analytics.AnalyticsSessionInfo.userId}"
                        + $"\n platform:{Application.platform.ToString()}"
                        + $"\n cloudProjectId:{Application.cloudProjectId}"
                    );
                    SendIds(
                        string.Empty,
                        googleAdId,
                        Adjust.getAdid(),
                        DDNA.Instance.UserID,
                        DDNA.Instance.EnvironmentKey,
                        SystemInfo.deviceUniqueIdentifier,
                        Analytics.AnalyticsSessionInfo.userId,
                        Application.platform.ToString(),
                        Application.cloudProjectId);
                });
#elif UNITY_IPHONE
                LogDebug("CollectIds::Collected Ids:"
                    + $"\n idfa:{Adjust.getIdfa()}"
                    + $"\n googleAdId:{string.Empty}"
                    + $"\n adjustId:{Adjust.getAdid()}"
                    + $"\n ddnaUserId:{DDNA.Instance.UserID}"
                    + $"\n ddnaEnvKey:{DDNA.Instance.EnvironmentKey}"
                    + $"\n deviceId:{SystemInfo.deviceUniqueIdentifier}"
                    + $"\n analyticsId:{Analytics.AnalyticsSessionInfo.userId}"
                    + $"\n platform:{Application.platform.ToString()}"
                    + $"\n cloudProjectId:{Application.cloudProjectId}"
                );
                SendIds(
                    Adjust.getIdfa(),
                    string.Empty,
                    Adjust.getAdid(),
                    DDNA.Instance.UserID,
                    DDNA.Instance.EnvironmentKey,
                    SystemInfo.deviceUniqueIdentifier,
                    Analytics.AnalyticsSessionInfo.userId,
                    Application.platform.ToString(),
                    Application.cloudProjectId);
#endif
            }
            else
            {
                LogDebug("CollectIds::Player opted out of analytics");
            }
            LogDebug("CollectIds::End");
        }

        void SendIds(string idfa, string googleAdId, string adid, string ddnaUserId, string ddnaEnvironmentKey, string deviceId, string unityAnalyticsUserId, string platform, string unityProjectId)
        {
            LogDebug("SendIds::Begin");
            s_DataPlatformAccessLayer.SendIds(
                idfa,
                googleAdId,
                adid,
                ddnaUserId,
                ddnaEnvironmentKey,
                deviceId,
                platform,
                unityAnalyticsUserId,
                unityProjectId,
                data =>
                {
                    LogDebug($"SendIds::dataResponseCode{data.responseCode}");
                    if (data.responseCode != Convert.ToInt64(HttpStatusCode.OK))
                    {
                        LogDebug("SendIds::retry");
                        //Do a single retry if the previous attempt failed
                        s_DataPlatformAccessLayer.SendIds(
                            idfa,
                            googleAdId,
                            adid,
                            ddnaUserId,
                            ddnaEnvironmentKey,
                            deviceId,
                            platform,
                            unityAnalyticsUserId,
                            unityProjectId,
                            secondData =>
                            {
                                LogDebug($"SendIds::retry dataResponseCode{secondData.responseCode}");
                            });
                    }
                });
            LogDebug("SendIds::End");
        }

#if GAMEGROWTH_PURCHASE_VERIFICATION
        public void GetNativeVerificationInfo(string stringVerificationInfo)
        {
            LogDebug("GetNativeVerificationInfo::Begin");
            LogDebug($"GetNativeVerificationInfo::stringVerificationInfo{stringVerificationInfo}");
            AdjustPurchase.GetNativeVerificationInfo(stringVerificationInfo);
            LogDebug("GetNativeVerificationInfo::Begin");
        }

#endif

#if GAMEGROWTH_UNITY_MONETIZATION
        void SetupUnityMediation()
        {
            LogDebug("SetupUnityMediation::Begin");
            if (useDefaultImpressionTrackedHandler)
            {
                LogDebug("SetupUnityMediation::useDefaultImpressionTrackedHandler");
                ImpressionEventPublisher.OnImpression += DefaultMediationImpressionTrackedEventHandler;
            }
            LogDebug("SetupUnityMediation::End");
        }

        public void DefaultMediationImpressionTrackedEventHandler(object sender, ImpressionEventArgs e)
        {
            LogDebug("DefaultMediationImpressionTrackedEventHandler::Begin");
            AdImpressionData gameGrowthAdImpressionData;

            if (e?.ImpressionData == null)
            {
                LogDebug("DefaultMediationImpressionTrackedEventHandler::ImpressionData null");
                gameGrowthAdImpressionData = new AdImpressionData(AdjustConfig.AdjustAdRevenueSourceUnity, null, AdCompletionStatus.Completed);
            }
            else
            {
                LogDebug("DefaultMediationImpressionTrackedEventHandler::ImpressionData not null");
                var adImpressionData = e.ImpressionData;
                var impressionDataJson = JsonUtility.ToJson(e.ImpressionData, true);
                LogDebug($"DefaultMediationImpressionTrackedEventHandler::ImpressionDataJson\n{impressionDataJson}");

                float.TryParse(adImpressionData.PublisherRevenuePerImpression, out var adEcpmUsd);
                adEcpmUsd *= 1000;

                var adSourceName = adImpressionData.AdSourceName ?? "N/A";
                var adUnitId = adImpressionData.AdUnitId ?? "N/A";

                LogDebug("DefaultMediationImpressionTrackedEventHandler::content:"
                    + $"\n AdjustAdRevenueSourceUnity:{AdjustConfig.AdjustAdRevenueSourceUnity}"
                    + $"\n AdCompletionStatus:{AdCompletionStatus.Completed}"
                    + $"\n adEcpmUsd:{adEcpmUsd}"
                    + $"\n adProvider:{adSourceName}"
                    + $"\n placementId:{adUnitId}"
                    + $"\n placementName:{adImpressionData.AdUnitName}"
                    + $"\n placementType:{adImpressionData.AdUnitFormat}"
                );

                gameGrowthAdImpressionData = new AdImpressionData(AdjustConfig.AdjustAdRevenueSourceMopub, impressionDataJson, AdCompletionStatus.Completed)
                {
                    adEcpmUsd = adEcpmUsd,
                    adProvider = adSourceName,
                    placementId = adUnitId,
                    placementName = adImpressionData.AdUnitName,
                    placementType = adImpressionData.AdUnitFormat
                };
            }

            adImpressionHandler.Invoke(gameGrowthAdImpressionData);
            LogDebug("DefaultMediationImpressionTrackedEventHandler::End");
        }

#endif

        public void DefaultAdjustAttributionCallback(AdjustAttribution adjustAttribution)
        {
            LogDebug("DefaultAdjustAttributionCallback::Begin");
            if (adjustAttribution == null)
            {
                LogDebug("DefaultAdjustAttributionCallback::adjustAttribution is null");
                Debug.LogError(k_MissingAdjustAttributionMessage);
                return;
            }

            float? costAmount = null;
            if (adjustAttribution.costAmount.HasValue)
            {
                costAmount = Convert.ToSingle(adjustAttribution.costAmount.Value);
            }

            LogDebug("DefaultMediationImpressionTrackedEventHandler::content:"
                + $"\n gameEvent:{AdjustAttributionExtensions.adjustAttributionEventName}"
                + $"\n {AdjustAttributionExtensions.acquisitionChannelParam}:{adjustAttribution.network + "::" + adjustAttribution.campaign}"
                + $"\n {AdjustAttributionExtensions.adGroupParam}:{adjustAttribution.adgroup}"
                + $"\n {AdjustAttributionExtensions.campaignParam}:{adjustAttribution.campaign}"
                + $"\n {AdjustAttributionExtensions.creativeParam}:{adjustAttribution.creative}"
                + $"\n {AdjustAttributionExtensions.networkParam}:{adjustAttribution.network}"
                + $"\n {AdjustAttributionExtensions.trackerNameParam}:{adjustAttribution.trackerName}"
                + $"\n {AdjustAttributionExtensions.trackerTokenParam}:{adjustAttribution.trackerToken}"
                + $"\n {AdjustAttributionExtensions.costAmountParam}:{costAmount}"
                + $"\n {AdjustAttributionExtensions.costCurrencyParam}:{adjustAttribution.costCurrency}"
                + $"\n {AdjustAttributionExtensions.costTypeParam}:{adjustAttribution.costType}"
                + $"\n {AdjustAttributionExtensions.activityKindParam}:{string.Empty}"
            );

            //Track attribution with Delta DNA.
            var adjustAttributionEvent = new GameEvent(AdjustAttributionExtensions.adjustAttributionEventName);
            AddStringParam(adjustAttributionEvent, AdjustAttributionExtensions.acquisitionChannelParam, adjustAttribution.network + "::" + adjustAttribution.campaign);
            AddStringParam(adjustAttributionEvent, AdjustAttributionExtensions.adGroupParam, adjustAttribution.adgroup);
            AddStringParam(adjustAttributionEvent, AdjustAttributionExtensions.campaignParam, adjustAttribution.campaign);
            AddStringParam(adjustAttributionEvent, AdjustAttributionExtensions.creativeParam, adjustAttribution.creative);
            AddStringParam(adjustAttributionEvent, AdjustAttributionExtensions.networkParam, adjustAttribution.network);
            AddStringParam(adjustAttributionEvent, AdjustAttributionExtensions.trackerNameParam, adjustAttribution.trackerName);
            AddStringParam(adjustAttributionEvent, AdjustAttributionExtensions.trackerTokenParam, adjustAttribution.trackerToken);
            // No value assigned for activityKind.
            AddStringParam(adjustAttributionEvent, AdjustAttributionExtensions.activityKindParam, string.Empty);
            // UA Cost
            AddOptionalFloatParam(adjustAttributionEvent, AdjustAttributionExtensions.costAmountParamName, costAmount);
            AddStringParam(adjustAttributionEvent, AdjustAttributionExtensions.costCurrencyParam, adjustAttribution.costCurrency);
            AddStringParam(adjustAttributionEvent, AdjustAttributionExtensions.costTypeParam, adjustAttribution.costType);
            s_AdjustAttributionEvent = adjustAttributionEvent;

#if UNITY_ANDROID
            m_MainThreadCaller.RunOnMainThread(RecordDeltaDnaAttributionEvent);
#else
            RecordDeltaDnaAttributionEvent();
#endif

            //Since this is a callback from Adjust, we don't need to track anything with Adjust
            LogDebug("DefaultAdjustAttributionCallback::End");
        }

        void RecordDeltaDnaAttributionEvent()
        {
            LogDebug("RecordDeltaDnaAttributionEvent::Begin");
            DDNA.Instance.RecordEvent(s_AdjustAttributionEvent).Run();
            //For attribution, for the upload of the event now to prevent missed attribution on game closing too soon.
            DDNA.Instance.Upload();
            s_AdjustAttributionEvent = null;
            LogDebug("RecordDeltaDnaAttributionEvent::End");
        }

        public void DefaultRecordAdImpression(AdImpressionData adImpressionData)
        {
            LogDebug("DefaultRecordAdImpression::Begin");
            if (adImpressionData == null)
            {
                LogDebug("DefaultRecordAdImpression::adImpressionData null");
                Debug.LogError(k_MissingAdImpressionDataMessage);
                return;
            }

            LogDebug("DefaultRecordAdImpression::content:"
                + $"\n gameEvent:{AdImpressionData.adImpressionEventName}"
                + $"\n {AdImpressionData.adCompletionStatusParamName}:{adImpressionData.adCompletionStatus.ExportForEvent()}"
                + $"\n {AdImpressionData.adEcpmUsdParamName}:{adImpressionData.adEcpmUsd}"
                + $"\n {AdImpressionData.adProviderParamName}:{adImpressionData.adProvider}"
                + $"\n {AdImpressionData.placementIdParamName}:{adImpressionData.placementId}"
                + $"\n {AdImpressionData.placementNameParamName}:{adImpressionData.placementName}"
                + $"\n {AdImpressionData.placementTypeParamName}:{adImpressionData.placementType}"
            );

            //Track impression with Delta DNA
            var adImpressionEvent = new GameEvent(AdImpressionData.adImpressionEventName)
                .AddParam(AdImpressionData.adCompletionStatusParamName, adImpressionData.adCompletionStatus.ExportForEvent());
            AddOptionalFloatParam(adImpressionEvent, AdImpressionData.adEcpmUsdParamName, adImpressionData.adEcpmUsd);
            AddOptionalStringParam(adImpressionEvent, AdImpressionData.adProviderParamName, adImpressionData.adProvider);
            AddOptionalStringParam(adImpressionEvent, AdImpressionData.placementIdParamName, adImpressionData.placementId);
            AddOptionalStringParam(adImpressionEvent, AdImpressionData.placementNameParamName, adImpressionData.placementName);
            AddOptionalStringParam(adImpressionEvent, AdImpressionData.placementTypeParamName, adImpressionData.placementType);
            DDNA.Instance.RecordEvent(adImpressionEvent).Run();

            //Track revenue with Adjust
            var adjustImpressionDataJson = AdjustImpressionDataHelper.ConvertMonetizationToAdjustImpressionData(adImpressionData.impressionJsonData ?? "{}");
            Adjust.trackAdRevenue(adImpressionData.adRevenueSource, adjustImpressionDataJson);
            LogDebug("DefaultRecordAdImpression::End");
        }

        public void DefaultRecordTransaction(TransactionData transactionData)
        {
            LogDebug("DefaultRecordTransaction::Begin");
            if (transactionData == null)
            {
                LogDebug("DefaultRecordTransaction::transactionData is null");
                Debug.LogError(k_MissingTransactionDataMessage);
                return;
            }

            LogDebug("DefaultRecordTransaction::transaction content"
                + $"\n transactionName:{transactionData.transactionName}"
                + $"\n transactionType:{transactionData.transactionType.ExportForEvent()}"
                + $"\n productsReceived:{transactionData.productsReceived}"
                + $"\n productsSpent:{transactionData.productsSpent}"
                + $"\n {TransactionData.amazonPurchaseTokenParamName}:{transactionData.amazonPurchaseToken}"
                + $"\n {TransactionData.amazonUserIdParamName}:{transactionData.amazonUserId}"
                + $"\n {TransactionData.engagementIdParamName}:{transactionData.engagementId}"
                + $"\n {TransactionData.isInitiatorParamName}:{transactionData.isInitiator}"
                + $"\n {TransactionData.paymentCountryParamName}:{transactionData.paymentCountry}"
                + $"\n {TransactionData.productIdParamName}:{transactionData.productId}"
                + $"\n {TransactionData.revenueValidatedParamName}:{transactionData.revenueValidated}"
                + $"\n {TransactionData.sdkVersionParamName}:{transactionData.sdkVersion}"
                + $"\n {TransactionData.transactionIdParamName}:{transactionData.transactionId}"
                + $"\n {TransactionData.transactionReceiptParamName}:{transactionData.transactionReceipt}"
                + $"\n {TransactionData.transactionReceiptSignatureParamName}:{transactionData.transactionReceiptSignature}"
                + $"\n {TransactionData.transactionServerParamName}:{transactionData.transactionServer}"
                + $"\n {TransactionData.userLevelParamName}:{transactionData.userLevel}"
                + $"\n {TransactionData.userScoreParamName}:{transactionData.userScore}"
                + $"\n {TransactionData.userXpParamName}:{transactionData.userXp}"
            );
            //Track purchase with Delta DNA
            var transactionEvent = new Transaction(
                transactionData.transactionName,
                transactionData.transactionType.ExportForEvent(),
                transactionData.productsReceived,
                transactionData.productsSpent);

            AddOptionalStringParam(transactionEvent, TransactionData.amazonPurchaseTokenParamName, transactionData.amazonPurchaseToken);
            AddOptionalStringParam(transactionEvent, TransactionData.amazonUserIdParamName, transactionData.amazonUserId);
            AddOptionalIntParam(transactionEvent, TransactionData.engagementIdParamName, transactionData.engagementId);
            AddOptionalBoolParam(transactionEvent, TransactionData.isInitiatorParamName, transactionData.isInitiator);
            AddOptionalPaymentCountryParam(transactionEvent, TransactionData.paymentCountryParamName, transactionData.paymentCountry);
            AddOptionalStringParam(transactionEvent, TransactionData.productIdParamName, transactionData.productId);
            AddOptionalIntParam(transactionEvent, TransactionData.revenueValidatedParamName, transactionData.revenueValidated);
            AddOptionalStringParam(transactionEvent, TransactionData.sdkVersionParamName, transactionData.sdkVersion);

#if UNITY_IPHONE
            if (!m_IsDeltaDNAInDevMode)
            {
                AddOptionalStringParam(transactionEvent, TransactionData.transactionIdParamName, transactionData.transactionId);
            }
#elif UNITY_ANDROID
            AddOptionalStringParam(transactionEvent, TransactionData.transactionIdParamName, transactionData.transactionId);
#endif

            AddOptionalStringParam(transactionEvent, TransactionData.transactionReceiptParamName, transactionData.transactionReceipt);
            AddOptionalStringParam(transactionEvent, TransactionData.transactionReceiptSignatureParamName, transactionData.transactionReceiptSignature);
            AddOptionalTransactionServerParam(transactionEvent, TransactionData.transactionServerParamName, transactionData.transactionServer);
            AddOptionalIntParam(transactionEvent, TransactionData.userLevelParamName, transactionData.userLevel);
            AddOptionalIntParam(transactionEvent, TransactionData.userScoreParamName, transactionData.userScore);
            AddOptionalIntParam(transactionEvent, TransactionData.userXpParamName, transactionData.userXp);

            DDNA.Instance.RecordEvent(transactionEvent).Run();

#if UNITY_ANDROID || UNITY_IPHONE
            
            var adjustPlatformPurchaseEventToken = "";

            if (!m_AdjustOverrideTokens)
            {
                LogDebug("AdjustPlatformPurchaseEventToken::not overriding tokens");
                adjustPlatformPurchaseEventToken = m_DefaultConfiguration.projectSummary.providers.adjust.purchaseToken;
            }
            else
            {
                LogDebug("AdjustPlatformPurchaseEventToken::overriding tokens");
                adjustPlatformPurchaseEventToken = m_AdjustPlatformPurchaseEventToken;
            }

            LogDebug("DefaultRecordTransaction::adjustEvent content"
                + $"\n eventToken:{adjustPlatformPurchaseEventToken}"
                + $"\n localizedPrice:{transactionData.localizedPrice}"
                + $"\n isoCurrencyCode:{transactionData.isoCurrencyCode}"
                + $"\n transactionId:{transactionData.transactionId}"
                + $"\n receipt:{transactionData.transactionReceipt}"
                + $"\n isReceiptSet:{true}"
            );

            //Track purchase with Adjust
            var adjustEvent = new AdjustEvent(adjustPlatformPurchaseEventToken);
            adjustEvent.setRevenue(transactionData.localizedPrice, transactionData.isoCurrencyCode);
            adjustEvent.setTransactionId(transactionData.transactionId);
            adjustEvent.receipt = transactionData.transactionReceipt;
            adjustEvent.isReceiptSet = true;
            Adjust.trackEvent(adjustEvent);
#endif
            LogDebug("DefaultRecordTransaction::End");
        }

        static void AddOptionalStringParam(Transaction transaction, string paramName, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                transaction.AddParam(paramName, value);
            }
        }

        static void AddOptionalIntParam(Transaction transaction, string paramName, int? value)
        {
            if (value.HasValue)
            {
                transaction.AddParam(paramName, value.Value);
            }
        }

        static void AddOptionalBoolParam(Transaction transaction, string paramName, bool? value)
        {
            if (value.HasValue)
            {
                transaction.AddParam(paramName, value.Value);
            }
        }

        static void AddOptionalPaymentCountryParam(Transaction transaction, string paramName, PaymentCountry? value)
        {
            if (value.HasValue)
            {
                transaction.AddParam(paramName, value.Value.ToString());
            }
        }

        static void AddOptionalTransactionServerParam(Transaction transaction, string paramName, TransactionServer? value)
        {
            if (value.HasValue)
            {
                transaction.AddParam(paramName, value.Value.ExportForEvent());
            }
        }

        static void AddOptionalFloatParam(GameEvent gameEvent, string paramName, float? value)
        {
            if (value.HasValue)
            {
                gameEvent.AddParam(paramName, value.Value);
            }
        }

        static void AddOptionalStringParam(GameEvent gameEvent, string paramName, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                gameEvent.AddParam(paramName, value);
            }
        }

        static void AddStringParam(GameEvent gameEvent, DeltaDnaEventParam param, string value)
        {
            // Unavailable data should be sent as empty string parameters
            gameEvent.AddParam(param.name, param.GetStringValue(value));
        }

        void LogDebug(string message)
        {
            if (configuration.debugLog == GameGrowthDebugLog.On)
            {
                Debug.Log($"GameGrowth::{message}");
            }
        }
    }
}
