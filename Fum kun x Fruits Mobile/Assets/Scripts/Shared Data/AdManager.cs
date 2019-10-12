using UnityEngine;
using UnityEngine.Advertisements;

[DisallowMultipleComponent]
public class AdManager : MonoBehaviour
{
    #region Singleton

    public static AdManager Instance { get; private set; }

    private void Awake() {

        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(this);
        } else
            Destroy(gameObject);
    }
    #endregion


    public delegate void OnVideoRewardedHandler(bool success);
    public event OnVideoRewardedHandler OnVideoRewarded;

    //public delegate void OnVideoFailedHandler();
    //public event OnVideoFailedHandler OnVideoFailed;

    const string ANDROID_GAME_ID = "2994974";
    const string VIDEO_AD = "video";
    const string REWARDED_VIDEO_AD = "rewardedVideo";

    [SerializeField]
    private bool isTest = true;

    private bool hasReward = false;
    private int adCounter = 0;
    //private bool hasAds;


    private void Start() {

#if UNITY_EDITOR
        isTest = true;
#endif
        //hasAds = UserDataManager.Instance.userData.hasAd;

        //if (hasAds)
        InitializedAds();
    }

    private void InitializedAds() {

        if (Application.internetReachability == NetworkReachability.NotReachable) return;

        Advertisement.Initialize(ANDROID_GAME_ID, isTest);

#if UNITY_EDITOR
        Debug.Log("Ads not initialized. Attepting to initialized with test mode = " + isTest);
#endif
    }

    public void CheckAdsInitialization() {

        //if (!hasAds || Advertisement.isInitialized) return;
        if (Advertisement.isInitialized) return;

        InitializedAds();
    }

    public void ShowVideoAd() {

        CheckAdsInitialization();

        adCounter++;

#if UNITY_EDITOR
        Debug.Log(adCounter);
#endif
        if (adCounter < 3 || !Advertisement.IsReady(VIDEO_AD)) return;

        Advertisement.Show(VIDEO_AD);
        adCounter = 0;
    }

    public void ShowRewardedVideoAd() {

        CheckAdsInitialization();

        if (!Advertisement.IsReady(REWARDED_VIDEO_AD)) return;

        var options = new ShowOptions { resultCallback = HandleShowResult };

        Advertisement.Show(REWARDED_VIDEO_AD, options);
    }

    private void HandleShowResult(ShowResult result) {

        switch (result) {
            case ShowResult.Finished:
                hasReward = true;
                adCounter = 0;
#if UNITY_EDITOR
                Debug.Log("The ad was successfully shown.");
#endif                               
                break;
            case ShowResult.Skipped:
                hasReward = false;
#if UNITY_EDITOR
                Debug.Log("The ad was skipped before reaching the end.");
#endif
                break;
            case ShowResult.Failed:
                hasReward = false;
#if UNITY_EDITOR
                Debug.LogError("The ad failed to be shown.");
#endif
                break;
        }

        HandReward();
    }

    private void HandReward() {
        OnVideoRewarded?.Invoke(hasReward);
    }

    public bool IsRewardedAdReady() {
        return Advertisement.IsReady(REWARDED_VIDEO_AD);
    }

    //private void VideoFailed() {
    //    if (OnVideoFailed != null)
    //        OnVideoFailed();
    //}
}
