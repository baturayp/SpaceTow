using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using DeltaDNA;
#if UNITY_IPHONE
using UnityEngine.iOS;
#endif

namespace UnityEngine.GameGrowth
{
    /// <summary>
    /// The type of progression.
    /// Up when player is progressing forward, Down when player is losing, Same when progression hasn't changed.
    /// </summary>
    public enum ProgressionState
    {
        Up,
        Down,
        Same
    }

    /// <summary>
    /// The type of reward given.
    /// Consumable is for rewards such as currency which are consumed upon use;
    /// Access is for rewards which unlock content;
    /// Facility is used for rewards that make the game easier for the player, such as avatar enhancements;
    /// Sustenance is used for anything that can mitigate burden, making the game less hard;
    /// Glory is typically used when the reward is score-related or increases points;
    /// SensoryFeedback is used when the reward given is visual/auditory/tactile/affective;
    /// PositiveFeedback is flattery or praise.
    /// </summary>
    public enum RewardType
    {
        Consumable,
        Access,
        Facility,
        Sustenance,
        Glory,
        SensoryFeedback,
        PositiveFeedback
    }

    /// <summary>
    /// The type of ranking change.
    /// If using a ranking system where 1 is 1st place, select SmallNumberIsBetter,
    /// if ranking is based on points or where a higher ranking is better, use BigNumberIsBetter.
    /// </summary>
    public enum RankingType
    {
        BigNumberIsBetter,
        SmallNumberIsBetter
    }

    /// <summary>
    /// The type of social interaction: action taken by the player.
    /// </summary>
    public enum SocialInteractionType
    {
        Login,
        Invite,
        Message,
        JoinClan,
        LeftClan,
        Gameplay,
        SendMoney
    }

    /// <summary>
    /// The action taken by the player in regards to this game's store.
    /// </summary>
    public enum StoreAction
    {
        StoreOpen,
        StoreClose,
        ItemOpen,
        ItemClose
    }

    /// <summary>
    /// This class acts as an API for events sent to DDNA, to ensure required parameters are present, and data is consistent accross games.
    /// </summary>
    public static class GameGrowthDDNAEventsManager
    {
        private static string s_uasUserID;
        private static string s_appleAppStoreID;
        private static string s_googlePlayStoreID;
        private static int s_eventCounter = 0;

        /// <summary>
        /// Sets the User Authentication Service ID (if the game uses an authentication service), the Apple App Store ID and the Google Play ID.
        /// These IDs are required everytime an event is sent, therefore it's important to call this method first before sending any events (at the start of the game)
        /// </summary>
        /// <param name="uasUserID">The User Authentication Service ID. When an authentication service is used in the game, this parameter corresponds to the player's unique ID.</param>
        /// <param name="appleAppStoreID">The Store ID in Apple's App Store. Typically 6-10 numbers.</param>
        /// <param name="googlePlayStoreID">The ID from the Google Play Store. Typically in the form of com.organizationName.gameName </param>
        public static void SetIDParameters(string uasUserID = null, string appleAppStoreID = null, string googlePlayStoreID = null)
        {
            if (string.IsNullOrEmpty(appleAppStoreID) && string.IsNullOrEmpty(googlePlayStoreID))
            {
                throw new ArgumentNullException("storeID", "At least one store ID (appleAppStoreID or googlePlayStoreID) must be set");
            }

            s_uasUserID = uasUserID;
            s_appleAppStoreID = appleAppStoreID;
            s_googlePlayStoreID = googlePlayStoreID;
        }

        /// <summary>
        /// Sends an achievement event.
        /// This event should be sent at the end of each gameplay match. This event will help predict the proficiency of a player.
        /// </summary>
        /// <param name="achievementID">Unique Achievement ID. For example, "bossFightLevel2Difficulty5".</param>
        /// <param name="hasReceivedAchievement">True when the player has succeeded. False when the player failed.</param>
        /// <param name="progressionState">The progression type.</param>
        /// <param name="achievementName">The name of the achievment as determined by the developer. For example, "bossFight".</param>
        /// <param name="rewardType">The type of reward received. See this enum's documentation for more details.</param>
        /// <param name="progression">The new progression of the player. Indicates where the player stands in terms of the entire progression path of the game.
        /// Can be number of points if they're an accurate indication of player progression (for example, 242) or a percentage expressed in integer (such as 48).</param>
        /// <param name="rewardAmount">The amount of the reward received, if applicable.</param>
        public static void SendAchievementEvent(string achievementID, bool hasReceivedAchievement, ProgressionState progressionState, string achievementName, RewardType rewardType, int progression, float? rewardAmount = null)
        {
            if (string.IsNullOrEmpty(achievementID))
            {
                throw new ArgumentNullException("achievementID", "Parameter achievementID cannot be null");
            }

            if (string.IsNullOrEmpty(achievementName))
            {
                throw new ArgumentNullException("achievementName", "Parameter achievementName cannot be null");
            }

            var achievementEvent = new GameEvent("achievement")
                .AddParam("eventVersion", "1.0.0")
                .AddParam("achievementID", achievementID)
                .AddParam("achievementReceived", hasReceivedAchievement)
                .AddParam("progressionState", ToCamelCase(progressionState.ToString()))
                .AddParam("achievementName", achievementName)
                .AddParam("rewardType", ToCamelCase(rewardType.ToString()))                
                .AddParam("progression", progression);

            AddOptionalParam(achievementEvent, "rewardAmount", rewardAmount);

            RecordEvent(achievementEvent);
        }

        /// <summary>
        /// Sends a rankChange event.
        /// This event should be sent when a player's rank changes in the leaderboard. This event is optional as not all games have leaderboards.
        /// The rank of a player can change when not playing (asynchronous play), and so it should be checked as soon as the player starts a new session and sent accordingly.
        /// </summary>
        /// <param name="leaderboardID">The name of the leaderboard. Games that have only one leaderboard will generate one leaderboard id. For example, "Bronze".</param>
        /// <param name="rankingType">Informs on the polarity of the leaderboard. Some games have a ranking system where 1 is 1st place (SmallNumberIsBetter),
        /// and others are based on points (BigNumberIsBetter), where a higher ranking is better.</param>
        /// <param name="isELO">True when ELO rating, else False. See https://en.wikipedia.org/wiki/Elo_rating_system for details.</param>
        /// <param name="rank">Actual rank of the player.</param>
        public static void SendRankChangeEvent(string leaderboardID, RankingType rankingType, bool isELO, float rank)
        {
            if (string.IsNullOrEmpty(leaderboardID))
            {
                throw new ArgumentNullException("leaderboardID", "Parameter leaderboardID cannot be null");
            }

            var rankChangeEvent = new GameEvent("rankChange")
                .AddParam("eventVersion", "1.0.0")
                .AddParam("leaderboardID", leaderboardID)
                .AddParam("rankingType", ToCamelCase(rankingType.ToString()))
                .AddParam("isELO", isELO)
                .AddParam("rank", rank);

            RecordEvent(rankChangeEvent);
        }

        /// <summary>
        /// Sends a matchmaking event. This event is optional as not all games have multi-player gameplay.
        /// This event should be sent when all players are matched together and the match is about to start.
        /// </summary>
        /// <param name="matchID">Unique Match ID. Can be a GUID. For example, "28618545-a54f-46dd-8bc9-4141103bf45d".</param>
        /// <param name="numberOfPlayers">The number of players participating in this match.</param>
        /// <param name="playerID">Unique player ID.</param>
        /// <param name="timeInSeconds">Time in seconds it took to put together all players in the match.</param>
        public static void SendMatchmakingEvent(string matchID, int numberOfPlayers, string playerID, float timeInSeconds)
        {
            if (string.IsNullOrEmpty(matchID))
            {
                throw new ArgumentNullException("matchID", "Parameter matchID cannot be null");
            }

            if (string.IsNullOrEmpty(playerID))
            {
                throw new ArgumentNullException("playerID", "Parameter playerID cannot be null");
            }

            var matchmakingEvent = new GameEvent("matchmaking")
                .AddParam("eventVersion", "1.0.0")
                .AddParam("matchmakingMatchID", matchID)
                .AddParam("numberOfPlayersInMatch", numberOfPlayers)
                .AddParam("matchmakingPlayerID", playerID)
                .AddParam("matchmakingTimeToMatchSeconds", timeInSeconds);

            RecordEvent(matchmakingEvent);
        }

        /// <summary>
        /// Sends a campaignReceived event.
        /// This event should be sent when a popup or a push notification is presented to the player. 
        /// </summary>
        /// <param name="marketingCampaignID">Unique message ID.</param>
        /// <param name="isPush">Is the marketing message a push notification (outside the game), or is it shown inside the game (not a push, but simply a notification)? If neither, send NULL.</param>
        /// <param name="messageText">The actual text message shown (in the language of the player).</param>
        /// <param name="hasMoreThanText">True when the message is enriched by an image, audio or video content. False when text only.</param>
        /// <param name="isOpened">False when the player rejected the offer without opening it (closed the notification).</param>
        /// <param name="partyID">The ID of the first, second or third party partner behind the campaign. Typically the name of the channel from which the campaign has been created.</param>
        /// <param name="fromID">The event ID that triggered this event, when applicable (not applicable for push notifications). For example, "matchMaking" when a popup was shown during matchmaking.</param>
        public static void SendCampaignReceivedEvent(string marketingCampaignID, bool isPush, string messageText, bool hasMoreThanText, bool isOpened, string partyID = "DDNA", string fromID = "")
        {
            if (string.IsNullOrEmpty(marketingCampaignID))
            {
                throw new ArgumentNullException("marketingCampaignID", "Parameter marketingCampaignID cannot be null");
            }

            if (string.IsNullOrEmpty(messageText))
            {
                throw new ArgumentNullException("messageText", "Parameter messageText cannot be null");
            }

            var campaignReceivedEvent = new GameEvent("campaignReceived")
                .AddParam("eventVersion", "1.0.0")
                .AddParam("marketingCampaignID", marketingCampaignID)
                .AddParam("partyID", partyID)
                .AddParam("isPush", isPush)
                .AddParam("messageText", messageText)
                .AddParam("hasMoreThanText", hasMoreThanText)
                .AddParam("isOpened", isOpened);

            AddOptionalParam(campaignReceivedEvent, "fromID", fromID);

            RecordEvent(campaignReceivedEvent);
        }

        /// <summary>
        /// Sends a socialInteraction event. This event is optional as not all games have multi-player gameplay.
        /// This event should be sent when the player takes a social action.
        /// </summary>
        /// <param name="interactionType">The social action taken by the player.</param>
        /// <param name="isMessageSent">True if message was sent to a friend, False if message was received from a friend. Null if no messages were exchanged.</param>
        /// <param name="interactionText">The text message itself, in the language of the player, if a message was sent.</param>
        /// <param name="triggeringEventID">The ID of the event which caused this event to be fired, when applicable. For example, "gameStarted" when the player logged in at the beginning of the game.</param>
        /// <param name="interactionSocialNetworkID">The name or ID of the social network when applicable.</param>
        /// <param name="clanID">The clan's unique identifier. Can be GUID.</param>
        public static void SendSocialInteractionEvent(SocialInteractionType interactionType, string interactionText = "", string triggeringEventID = "", string interactionSocialNetworkID = "", string clanID = "", bool? isMessageSent = null)
        {
            var socialEvent = new GameEvent("socialInteraction")
                .AddParam("eventVersion", "1.0.0")
                .AddParam("socialInteractionType", ToCamelCase(interactionType.ToString()));

            AddOptionalParam(socialEvent, "socialInteractionMessageText", interactionText);
            AddOptionalParam(socialEvent, "socialInteractionTriggeringEvent", triggeringEventID);
            AddOptionalParam(socialEvent, "socialInteractionSocialNetworkID", interactionSocialNetworkID);
            AddOptionalParam(socialEvent, "socialInteractionClanID", clanID);

            if (isMessageSent != null)
            {
                socialEvent.AddParam("socialInteractionMessageSent", Convert.ToBoolean(isMessageSent));
            }

            RecordEvent(socialEvent);
        }

        /// <summary>
        /// Sends a storeActivity event.
        /// This event should be sent for any store or market places activities (when the player opens or closes the store/market place, when the player interacts with an item, etc.)
        /// </summary>
        /// <param name="storeAction">The player's interaction with the store. Can be StoreOpen, StoreClose, ItemOpen or ItemClose.</param>
        /// <param name="storeItemID">The item's unique identifier. If the action is to open the store, this value should correspond to "store".</param>
        /// <param name="isPurchased">True if the item has been purchased.</param>
        /// <param name="isPromotion">True if the item is on promotion.</param>
        /// <param name="currencyAmountConverted">Base currency value for this item, if applicable.</param>
        /// <param name="promotionPercentage">The promotion's percentage if applicable.</param>
        public static void SendStoreActivityEvent(StoreAction storeAction, string storeItemID, bool isPurchased, bool isPromotion, float currencyAmountConverted = 0.0f, float promotionPercentage = 0.0f)
        {
            if (string.IsNullOrEmpty(storeItemID))
            {
                throw new ArgumentNullException("storeItemID", "Parameter storeItemID cannot be null");
            }

            var storeActivityEvent = new GameEvent("storeActivity")
                .AddParam("eventVersion", "1.0.0")
                .AddParam("storeAction", ToCamelCase(storeAction.ToString()))
                .AddParam("storeItemID", storeItemID)
                .AddParam("storeIsPurchased", isPurchased)
                .AddParam("storeIsPromotion", isPromotion);              

            if (promotionPercentage != 0.0f)
            {
                storeActivityEvent.AddParam("storePromotionPercentage", promotionPercentage);
            }

            if (currencyAmountConverted != 0.0f)
            {
                storeActivityEvent.AddParam("storeCurrencyAmountConverted", currencyAmountConverted);
            }


            RecordEvent(storeActivityEvent);
        }

        /// <summary>
        /// Sends a currencyExchange event.
        /// This event should be sent when a currency transfer is made.
        /// It should be triggered by other events that alter the player's currency wallet,
        /// and should capture every currency transaction and resources made by a player - everytime some transfer of currency occurs.
        /// </summary>
        /// <param name="itemID">ID of the item purchased.</param>
        /// <param name="isSoftCurrency">True when soft currency, False when hard currency is being exchanged.</param>
        /// <param name="currencyName">Name of currency that is being exchanged.</param>
        /// <param name="currencyAmount">Amount of the currency being transfered, as shown in the game.</param>
        /// <param name="convertedCurrencyAmount">Base currency value for this item.</param>
        /// <param name="currencyExchangeTriggerID">The ID of the event which caused this event to be fired. For example, "rewardBigLevel2" when the player received currency from a reward.</param>
        public static void SendCurrencyExchangeEvent(string itemID, bool isSoftCurrency, string currencyName, float currencyAmount, float convertedCurrencyAmount, string currencyExchangeTriggerID)
        {
            if (string.IsNullOrEmpty(itemID))
            {
                throw new ArgumentNullException("itemID", "Parameter itemID cannot be null");
            }

            if (string.IsNullOrEmpty(currencyName))
            {
                throw new ArgumentNullException("currencyName", "Parameter currencyName cannot be null");
            }

            if (string.IsNullOrEmpty(currencyExchangeTriggerID))
            {
                throw new ArgumentNullException("currencyExchangeTriggerID", "Parameter currencyExchangeTriggerID cannot be null");
            }

            var currencyExchangeEvent = new GameEvent("currencyExchange")
                .AddParam("eventVersion", "1.0.0")
                .AddParam("currencyExchangeItem", itemID)
                .AddParam("currencyExchangeSoftCurrency", isSoftCurrency)
                .AddParam("currencyExchangeCurrencyName", currencyName)
                .AddParam("currencyExchangeCurrencyAmount", currencyAmount)
                .AddParam("currencyExchangeConvertedCurrencyAmount", convertedCurrencyAmount)
                .AddParam("currencyExchangeTrigger", currencyExchangeTriggerID);

            RecordEvent(currencyExchangeEvent);
        }

        private static void RecordEvent(GameEvent gameEvent)
        {
            Debug.Log($"Sending {gameEvent} event");

            var connectionType = "";
            switch (Application.internetReachability)
            {
                case NetworkReachability.NotReachable:                    
                    break;
                case NetworkReachability.ReachableViaCarrierDataNetwork:
                    connectionType = "data";
                    break;
                case NetworkReachability.ReachableViaLocalAreaNetwork:
                    connectionType = "wifi";
                    break;
            }

            var buildGUID = Application.buildGUID;

#if UNITY_EDITOR
            buildGUID = "UnityEditor";
#endif

            gameEvent
                .AddParam("gameProjectID", Application.cloudProjectId)
                .AddParam("gameBundleID", Application.identifier)
                .AddParam("deviceVolume", SystemInfo.systemMemorySize)
                .AddParam("batteryLoad", SystemInfo.batteryLevel)
                .AddParam("connectionType", connectionType)
                .AddParam("timezoneOffset", "-00")
                .AddParam("playerEventSequence", ++s_eventCounter)
                .AddParam("buildGUID", buildGUID)
                .AddParam("clientVersion", DDNA.Instance.ClientVersion);

#if UNITY_IPHONE
            gameEvent            
            .AddParam("idfv", Device.vendorIdentifier)
            .AddParam("gameStoreID", s_appleAppStoreID);

            AddOptionalParam(gameEvent, "idfa", Device.advertisingIdentifier);
#elif UNITY_ANDROID
            gameEvent.AddParam("gameStoreID", s_googlePlayStoreID);

            Application.RequestAdvertisingIdentifierAsync((string advertisingID, bool isTrackingEnabled, string error) =>
            {
                if (string.IsNullOrEmpty(error))
                {
                    AddOptionalParam(gameEvent, "idfa", advertisingID);
                }
            });
#elif UNITY_EDITOR
            gameEvent.AddParam("gameStoreID", "UnityEditor");
#endif

            AddOptionalParam(gameEvent, "uasUserID", s_uasUserID);

            DDNA.Instance.RecordEvent(gameEvent).Run();
        }

        private static void AddOptionalParam(GameEvent gameEvent, string paramName, object value)
        {
            var stringValue = value as string;            

            if (value != null || !string.IsNullOrEmpty(stringValue))
            {
                gameEvent.AddParam(paramName, value);
            }            
        }

        private static string ToCamelCase(string text)
        {
            var textWithLowerChar = text.Select((character, position) => position == 0 ? char.ToLower(character) : character);
            return new string(textWithLowerChar.ToArray());
        }
    }
}
