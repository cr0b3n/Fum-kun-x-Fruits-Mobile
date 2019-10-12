using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class UserDataManager : MonoBehaviour
{

    private const string MAIN_USER_DATA = "userdata";
    private const string INFO_HASH = "fUmFum!g4m3";
    //private const string USER_SETTINGS = "gamesettings";

    [HideInInspector]
    public UserData userData;

    [HideInInspector]
    public bool hasAudio = true;

    //private List<string> achievementIDs;

    #region Singleton

    public static UserDataManager Instance { get; private set; }


    private void Awake() {

        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(this);
        } else
            Destroy(gameObject);

#if UNITY_EDITOR
        //PlayerPrefs.DeleteAll();
        //PlayerPrefs.DeleteKey(SAVE_NAME);
#endif
        //LoadUserSettings();
        LoadUserData();
    }

    #endregion /Singleton

    private void Start() {
        //SetUpAdIDS();
        PlayGamesClientConfiguration playConfig = new PlayGamesClientConfiguration.Builder().Build();
        PlayGamesPlatform.InitializeInstance(playConfig);
        // recommended for debugging: set to true
        //PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
       
        SignIn();
        //PopulateAchievementIDs();
    }

    #region Rating Section

    public void SaveRating() {
        userData.rated = true;
        SaveData();
        //Debug.Log("saving rating in settings");
    }

    public void ShowGameRater(GameObject obj) {

        if (userData.rated || userData.ratingCount < 20 || Application.internetReachability == NetworkReachability.NotReachable) {
            userData.ratingCount++;
            SaveData();
            return;
        }
            
        userData.ratingCount = 0;
        SaveData();
        obj.SetActive(true);
        return;
    }
    #endregion /Rating Section

    #region Player Data

    public bool IsPlaneOwned(int index) {
        //Check if the bit is set
        return (userData.planeOwned & (1 << index)) != 0;
    }

    public void UsePlane(int index) {

        if (!IsPlaneOwned(index)) return;

        userData.planeIndex = index;
        SaveData();
    }

    public void SetPlaneToOwned(int index, int coins) {
        //Toggle on the bit at index
        userData.planeOwned |= 1 << index;
        userData.planeIndex = index;
        SetCoins(coins);
        CheckPlaneAchievement();
        //To toggle off
        //userData.planeOwned ^= 1 << index;
    }

    public void SetCoins(int coins) {

        userData.coins += coins;
        userData.coins = Mathf.Clamp(userData.coins, 0, 99999);
        CheckCoinAchievements(userData.coins);
        SaveData();
    }

    public bool SetBestScore(int score) {

        AddScoreToLeaderboard(score);
        CheckScoreAchievements(score);

        if (score < userData.bestScore) return false;

        userData.bestScore = score;
        SaveData();

        return true;
    }

    public void UserAgreed(GameObject signInButton) {
        userData.hasAgree = true;
        SaveData();
        SignIn(signInButton);
    }

    public void SetSignInPolicyVisibility(GameObject signInButton, GameObject policyPopup) {
        signInButton.SetActive(!IsUserAuthenticated());
        policyPopup.SetActive(!userData.hasAgree);
    }

    //Saving to the player prefs
    private void SaveData() {

        userData.hash = GetHash();
        PlayerPrefs.SetString(MAIN_USER_DATA, JsonUtility.ToJson(userData, true));

        PlayerPrefs.Save();
        //Debug.Log(JsonUtility.ToJson(userSettings, true));
        //Debug.Log(JsonUtility.ToJson(userData, true));
    }

    //Loading player prefs
    private void LoadUserData() {

        //Json VERSION BUT PLAYERPREF SAVE
        if (!PlayerPrefs.HasKey(MAIN_USER_DATA)) {
            userData = new UserData();
            SaveData();
            //Debug.Log("No save file found creating a new one");
            return;
        }

        if (!MatchData()) return;

        //Debug.Log("Back up has been compromise resetting to original data....");
        userData = new UserData();
        SaveData();
    }

    private string GetHash() {
        userData.hash = INFO_HASH;
        //set up hashing
        string dataToHash = JsonUtility.ToJson(userData, true);

        //setup SHA
        SHA256Managed crypt = new SHA256Managed();
        string hash = string.Empty;

        //compute hash
        byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(dataToHash), 0, Encoding.UTF8.GetByteCount(dataToHash));

        //convert to hex
        foreach (byte bit in crypto)
            hash += bit.ToString("x2");

        return hash;
    }

    private bool MatchData() {

        if (!PlayerPrefs.HasKey(MAIN_USER_DATA))
            return true;

        try { //Check if the Json parser has no error

            JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString(MAIN_USER_DATA), userData);

        } catch {
            //Debug.Log("Something is wrong with your save data");
            return true;
        }
        //Temporary store current hash to check if it matches
        string curHash = userData.hash;

        return GetHash() != curHash;
    }

    #endregion /Player Data

    #region Googleplay Services

    void SignIn() {

        if (Application.internetReachability == NetworkReachability.NotReachable || !userData.hasAgree) return;

        Social.localUser.Authenticate((bool success) => {
            // handle success or failure
            //Debug.Log("Trying to sign in...");
            if (success) {
                //SignInSuccessUnlockables();
                //GetLeaderBoardScore();
                //GetUnlockedAchievements();
                //LoadData(); //Data from the cloud combo with local data
            }
        });
    }

    public bool IsUserAuthenticated() {
        return PlayGamesPlatform.Instance.localUser.authenticated;
    }

    public void SignIn(GameObject obj, GameObject obj2 = null) {

        if (IsUserAuthenticated()) {
            obj.gameObject.SetActive(false);

            if (obj2 != null)
                obj2.SetActive(false);

            return;
        }

#if UNITY_EDITOR
        Debug.Log("Logging in....");
#endif

        Social.localUser.Authenticate((bool success) => {
            // handle success or failure
            if (success) {
                obj.SetActive(false);

                if (obj2 != null)
                    obj2.SetActive(false);

                if (userData.planeAchievement == 1)
                    CheckPlaneAchievement();

                SaveData();
                //SignInSuccessUnlockables();
                //GetLeaderBoardScore();
                //GetUnlockedAchievements();
                //LoadData(); //Data from the cloud combo with local data
            }
        });
    }

    //void GetLeaderBoardScore() {

    //    if (!userData.isNew)
    //        return;

    //    PlayGamesPlatform.Instance.LoadScores(
    //         GPGSIds.leaderboard_ninja_blob_top_scorers,
    //         LeaderboardStart.PlayerCentered,
    //         1,
    //         LeaderboardCollection.Public,
    //         LeaderboardTimeSpan.AllTime,
    //     (LeaderboardScoreData data) => {
    //         userData.bestScore = int.Parse(data.PlayerScore.formattedValue);
    //     });
    //    Save();
    //    //PlayGamesPlatform.Instance.LoadScores(
    //    //     GPGSIds.leaderboard_ninja_blob_top_scorers,
    //    //     LeaderboardStart.PlayerCentered,
    //    //     1,
    //    //     LeaderboardCollection.Public,
    //    //     LeaderboardTimeSpan.AllTime,
    //    // (LeaderboardScoreData data) => {
    //    //     Debug.Log(data.Valid);
    //    //     Debug.Log(data.Id);
    //    //     Debug.Log(data.PlayerScore);
    //    //     Debug.Log(data.PlayerScore.userID);
    //    //     Debug.Log(data.PlayerScore.formattedValue);
    //    // });
    //}

    //void GetUnlockedAchievements() {
    //    if (!userData.isNew)
    //        return;

    //    Social.LoadAchievements(achievements => {
    //        if (achievements.Length > 0) {

    //            foreach (IAchievement achievement in achievements) {
    //                if (achievement.completed && achievementIDs.IndexOf(achievement.id) >= 0) {
    //                    userData.achievements[achievementIDs.IndexOf(achievement.id)] = 2;
    //                    Save();
    //                }
    //            }
    //        }
    //    });
    //}

    #region Achievements

    private void CheckScoreAchievements(int score) {

#if UNITY_EDITOR
        Debug.Log("Unlocking score achievement");
#endif

        if (score >= 50) UnlockAchievement(GPGSIds.achievement_konichiwa);
        if (score >= 100) UnlockAchievement(GPGSIds.achievement_magaling);
        if (score >= 200) UnlockAchievement(GPGSIds.achievement_masarap);
        if (score >= 500) UnlockAchievement(GPGSIds.achievement_kumusta);
        if (score >= 1000) UnlockAchievement(GPGSIds.achievement_grabeehhh);
    }

    private void CheckCoinAchievements(int coins) {

#if UNITY_EDITOR
        Debug.Log("Unlocking coin achievement");
#endif
        if (coins >= 10) UnlockAchievement(GPGSIds.achievement_amai);
        if (coins >= 50) UnlockAchievement(GPGSIds.achievement_hey_whats_up);
        if (coins >= 100) UnlockAchievement(GPGSIds.achievement_what_you_recommend);
        if (coins >= 200) UnlockAchievement(GPGSIds.achievement_grabeh_grabeh);
    }

    private void CheckPlaneAchievement() {

        if (userData.planeAchievement >= 2) return;

#if UNITY_EDITOR
        Debug.Log("Unlocking plane achievement");
#endif
        bool isUnlocked = TryUnlockAchievement(GPGSIds.achievement_para_po);

        if(isUnlocked) {
            userData.planeAchievement = 2;
            SaveData();
        } else {
            userData.planeAchievement = 1;
            SaveData();
        }            
    }

    private bool TryUnlockAchievement(string id) {

        if (!IsUserAuthenticated())
            return false;

        bool unlocked = false;

        Social.ReportProgress(id, 100.0f, (bool success) => {
            // handle success or failure
            unlocked = success;
        });

        return unlocked;
    }

    private void UnlockAchievement(string id) {

        if (!IsUserAuthenticated())
            return;

        Social.ReportProgress(id, 100.0f, (bool success) => {
            // handle success or failure
            //if (!success)                           //If the unlock fails then save the ID to try and unlock it on next start of the game
            //    userData.achievements[index] = 1;
            //else {
            //    userData.achievements[index] = 2;
            //    Save();
            //}
        });

    }

    //private void PopulateAchievementIDs() {
    //    achievementIDs.Add(GPGSIds.achievement_konichiwa);
    //    achievementIDs.Add(GPGSIds.achievement_magaling);
    //    achievementIDs.Add(GPGSIds.achievement_masarap);
    //    achievementIDs.Add(GPGSIds.achievement_kumusta);
    //    achievementIDs.Add(GPGSIds.achievement_grabeehhh);
    //    achievementIDs.Add(GPGSIds.achievement_amai);
    //    achievementIDs.Add(GPGSIds.achievement_hey_whats_up);
    //    achievementIDs.Add(GPGSIds.achievement_what_you_recommend);
    //    achievementIDs.Add(GPGSIds.achievement_grabeh_grabeh);
    //    achievementIDs.Add(GPGSIds.achievement_para_po);
    //}

    //public void UnlockAchievements(List<int> achievements) {

    //    for (int i = 0; i < achievements.Count; i++) {
    //        //if (achievements[i] <= achievementID.Count) {
    //        //Debug.Log("error at achievement index " + achievements[i].ToString());
    //        UnlockAchievement(achievementIDs[achievements[i]], achievements[i]);
    //    }
    //}

    //void SignInSuccessUnlockables() {
    //    UnlockAchievement(achievementIDs[1], 1);
    //    //To try and unlocked achievements that failed to unlocked          //NOTE:
    //    for (int i = 0; i < userData.achievements.Length; i++) {            //Make sure that the achievement order is correct
    //        if (userData.achievements[i] == 1) {                            // userData.achievement must match with achivementIDs
    //            UnlockAchievement(achievementIDs[i], i);
    //            //Debug.Log("TRYING TO UNLOCK " + i.ToString());
    //        }
    //    }
    //    AddScoreToLeaderboard(userData.bestScore);
    //}

    //void UnlockAchievement(string id, int index) {

    //    if (userData.achievements[index] >= 2)      //check if the achievement has been unlocked 2 = unlocked, 1 = failed to unlock last time, 0 = locked
    //        return;

    //    Social.ReportProgress(id, 100.0f, (bool success) => {
    //        // handle success or failure
    //        if (!success)                           //If the unlock fails then save the ID to try and unlock it on next start of the game
    //            userData.achievements[index] = 1;
    //        else {
    //            userData.achievements[index] = 2;
    //            Save();
    //        }
    //    });
    //}

    //Uncomment for incremental achievement
    //public void IncrementAchievement(string id, int stepsToIncrement) {

    //    //if (!PlayGamesPlatform.Instance.localUser.authenticated)
    //    //return;

    //    PlayGamesPlatform.Instance.IncrementAchievement(id, stepsToIncrement, success => {
    //        // handle success or failure
    //    });
    //}

    public void ShowAchievementsUI(GameObject errorMessage) {
        if (IsUserAuthenticated())
            Social.ShowAchievementsUI();
        else
            errorMessage.SetActive(true);
        //Social.ShowAchievementsUI();
    }

    #endregion /Achievements

    #region Leaderboards

    public void AddScoreToLeaderboard(int score) {

#if UNITY_EDITOR
        Debug.Log("Adding score to leaderboard...");
#endif

        if (!IsUserAuthenticated())
            return;        

        Social.ReportScore(score, GPGSIds.leaderboard_leaderboard, (bool success) => {
            // handle success or failure
        });
    }

    //public void AddScoreToLeaderboard(string leaderboardId, int score) {

    //    if (!PlayGamesPlatform.Instance.localUser.authenticated)
    //        return;

    //    Social.ReportScore(score, leaderboardId, (bool success) => {
    //        // handle success or failure
    //    });
    //}

    public void ShowLeaderBoardUI(GameObject errorMessage) {
        if (IsUserAuthenticated())
            PlayGamesPlatform.Instance.ShowLeaderboardUI(GPGSIds.leaderboard_leaderboard);
        //Social.ShowLeaderboardUI(); //Lists of leaderboards
        else
            errorMessage.SetActive(true);
        //Social.ShowLeaderboardUI();
    }

    #endregion /Leaderboards
    #endregion /Googleplay services
}
