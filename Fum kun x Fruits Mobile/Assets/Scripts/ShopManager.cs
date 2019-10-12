using System.Collections;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class ShopManager : MonoBehaviour {

    public PlaneDetail[] planesDetails;
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI videoAdText;
    public GameObject itemDetailHolder;
    public GameObject buyButton;
    public GameObject lowOnCoins;
    public GameObject adPopup;

    private Transform previousSelection;
    private int currentPlaneIndex;
    private Vector3 maxScale = new Vector3(1.05f, 1.05f, 1f);
    private int selectedPrice;

    #region Singleton

    public static ShopManager instance;

    private void Awake() {

        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    #endregion

    private void Start() {
        UpdateCoinDisplay();
        currentPlaneIndex = UserDataManager.Instance.userData.planeIndex;
        AdManager.Instance.OnVideoRewarded += HandReward;
    }

    private void OnDestroy() {
        AdManager.Instance.OnVideoRewarded -= HandReward;
    }

    public void UpdateCoinDisplay() {
        coinText.text = "x" + UserDataManager.Instance.userData.coins;
    }

    public void ShowRewardedVideo() {

        if (AdManager.Instance.IsRewardedAdReady())
            AdManager.Instance.ShowRewardedVideoAd();
        else {
            adPopup.SetActive(false);
            GameManager.instance.PlayButtonSound();
        }           
    }

    public void CheckRewardedVideoStatus() {

        if (AdManager.Instance.IsRewardedAdReady()) {
            videoAdText.text = "Watch a video ad to earn 2 coins";
            return;
        }

        videoAdText.text = "No video ad at the moment, please try again later!";
    }

    private void HandReward(bool success) {

        if (!success) {
            videoAdText.text = "Video ad failed to complete!";
            return;
        }
        
        adPopup.SetActive(false);
        UserDataManager.Instance.SetCoins(2);
        StartCoroutine(AnimateCoinText(.3f));
        GameManager.instance.PlayButtonSound(true);
    }

    IEnumerator AnimateCoinText(float lerpTime) {

        yield return new WaitForSeconds(1f);

        Vector3 curScale = new Vector3(1.4f, 1.4f, 1f);
        coinText.transform.localScale = curScale;
        UpdateCoinDisplay();

        float originalTime = lerpTime;

        while (lerpTime > 0f) {
            lerpTime -= Time.deltaTime;
            coinText.transform.localScale = Vector3.Lerp(Vector3.one, curScale, lerpTime/ originalTime);

            yield return null;
        }

        SetPlanePrice(selectedPrice, previousSelection, currentPlaneIndex);
        coinText.transform.localScale = Vector3.one;
     }

    public void CloseShop () {
        GameManager.instance.PlayButtonSound();
        Player.instance.SetPlaneDetails(UserDataManager.Instance.userData.planeIndex);
    }

    public void ShowShop () {
        GameManager.instance.PlayButtonSound();
        Player.instance.SetPlaneDetails(currentPlaneIndex);
    }

    public void BuyPlane() {
     
        //TODO: CHECK GOLD
        if (selectedPrice > UserDataManager.Instance.userData.coins) {
            GameManager.instance.PlayButtonSound();
            //Debug.Log("Not enough gold!!!");
            return;
        }

        GameManager.instance.PlayButtonSound(true);
        UserDataManager.Instance.SetPlaneToOwned(currentPlaneIndex, -selectedPrice);
        buyButton.SetActive(false);
        UpdateCoinDisplay();
    }

    public void SetPlanePrice(int price, Transform planeUI, int index) {
        
        priceText.text = "x" + price.ToString();

        if (previousSelection)
            previousSelection.localScale = Vector3.one;

        if(planeUI) {
            itemDetailHolder.SetActive(true);
            planeUI.localScale = maxScale;
        }
            
        if (UserDataManager.Instance.IsPlaneOwned(index)) {
            ShowBuyButtonVisibility(false, false);
            UserDataManager.Instance.UsePlane(index);
        }         
        else {

            if (price <= UserDataManager.Instance.userData.coins)
                ShowBuyButtonVisibility(true, false);
            else
                ShowBuyButtonVisibility(false, true);
        }
                
        previousSelection = planeUI;
        currentPlaneIndex = index;
        selectedPrice = price;
    }

    private void ShowBuyButtonVisibility(bool canBuy, bool isLow) {
        buyButton.SetActive(canBuy);
        lowOnCoins.SetActive(isLow);
    }

    public void GetPlaneDetails(SpriteRenderer planeBody, SpriteRenderer planeCanon, int index) {

        planeBody.sprite = planesDetails[index].planeBody;
        planeCanon.sprite = planesDetails[index].planeCanon;
    }
}
