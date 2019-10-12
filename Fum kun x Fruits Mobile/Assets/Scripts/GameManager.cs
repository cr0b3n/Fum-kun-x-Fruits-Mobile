using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

[DisallowMultipleComponent]
public class GameManager : MonoBehaviour {
    #region Singleton
    public static GameManager instance;

    private void Awake() {

        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }
    #endregion

    [Header("Text Section")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI bestScoreText;

    [Header("Level Section")]
    public Image levelFill;
    [Range(0.01f, 1.0f)]
    public float levelMultiplier = 0.1f;
    [Range(1.0f, 20.0f)]
    public float requiredLevelExp = 10.0f;

    [Header("Pop Up Section")]
    public RectTransform menuContainer;
    public GameObject shopMenu;
    public GameObject mainMenu;
    public GameObject overlay;
    public GameObject pauseButton;
    public GameObject playbutton;
    public GameObject reviveButton;
    public GameObject trophy;
    public GameObject ratePopup;
    public GameObject policyPopup;
    public GameObject signInButton;

    [Header("Object Pool Section")]
    public ObjectPooler playerBullet;
    public ObjectPooler explosionPool;
    public ObjectPooler scoreTextPool;
    public ObjectPooler coinEffectPool;

    #region Audio Field
    [Header("Audio Section")]
    public GameObject hasAudioButton;
    public GameObject noAudiobutton;
    public AudioSource bgmSource;
    public AudioSource uiSource;
    public AudioSource fruitSource;
    public AudioSource pointsSource;
    public AudioSource bulletSource;
    public AudioClip bgm1;
    public AudioClip bgm2;
    public AudioClip startAudio;
    public AudioClip buttonClip;
    public AudioClip levelupClip;
    public AudioClip explosionClip;
    public AudioClip pointsClip;
    public AudioClip successSound;
    public AudioClip[] bulletHitClips;
    #endregion /Audio Field

    private bool gameOver;
    private float currentExp;
    private int currentLevel;
    private int score;
    private int coins;
    private int preReviveCoins;
    private Camera cam;
    private bool isPause;
    private bool hasRevive;

    public event Action OnLevelUp;
    public event Action OnGameStart;

    private void Start() {
        reviveButton.SetActive(false);
        coins = 0;
        preReviveCoins = 0;
        hasRevive = false;
        SetMenuVisibility(false, true);
        bestScoreText.text = "Best Score: " + UserDataManager.Instance.userData.bestScore;
        isPause = false;
        gameOver = false;
        SetNewLevelRequirements();
        SetScore(0, Vector3.one, false);
        cam = Camera.main;
        Time.timeScale = 1;
        AudioListener.pause = false;
        uiSource.ignoreListenerPause = true;
        AdManager.Instance.ShowVideoAd();
        UserDataManager.Instance.ShowGameRater(ratePopup);
        UpdateAudioDisplayButtons(UserDataManager.Instance.hasAudio);
        UserDataManager.Instance.SetSignInPolicyVisibility(signInButton, policyPopup);
    }

    #region UI Region

    public void PlayOrPause() {

        if (gameOver) return;

        pauseButton.SetActive(isPause);
        playbutton.SetActive(!isPause);
        isPause = !isPause;

        PlayAudio(uiSource, buttonClip);
        AudioListener.pause = isPause;
        overlay.SetActive(isPause);

        if (isPause) {
            Time.timeScale = 0;
        } else {
            Time.timeScale = 1;
        }

        reviveButton.SetActive(false);
        statusText.text = "Paused";
    }

    public void RestartGame() {
        PlayAudio(uiSource, buttonClip);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OpenLink(int index) {

        PlayAudio(uiSource, buttonClip);

        switch (index) {
            default:
            case 0:
                Application.OpenURL("https://www.facebook.com/crobengames");
                break;
            case 1:
                Application.OpenURL("https://twitter.com/crobengames");
                break;
            case 2:
                Application.OpenURL("https://www.youtube.com/channel/UCi7Ofw4sJynsacr_i6uk_lA");
                break;
            case 3:
                Application.OpenURL("https://www.youtube.com/watch?v=OCxzHwJbQ_g");
                break;
            case 4:
                Application.OpenURL("https://www.facebook.com/groups/1970922349788193/");
                break;
            case 5:
                Application.OpenURL("http://www.croben.blogspot.com/p/terms-conditions.html");
                break;
            case 6:
                Application.OpenURL("http://www.croben.blogspot.com/p/privacy-policy.html");
                break;
        }
    }

    public void AgreeToPolicy() {
        //Playgames login handled at userdatamanager
        UserDataManager.Instance.UserAgreed(signInButton);
        policyPopup.SetActive(false);
        PlayButtonSound();
    }

    public void ShowMenu(bool isShop) {

        if (isShop)
            SetMenuVisibility(true, false);
        else
            SetMenuVisibility(false, true);
    }

    private void SetMenuVisibility(bool shop, bool menu) {
        shopMenu.SetActive(shop);
        mainMenu.SetActive(menu);
    }

    public void RateGame() {
        PlayButtonSound();
        ratePopup.SetActive(false);
        UserDataManager.Instance.SaveRating();
        Application.OpenURL("market://details?id=com.croben.fumkunxfruits");
#if UNITY_EDITOR
        Debug.Log("Rating the game");
#endif
    }

    public void CloseGame() {
        Application.Quit();
#if UNITY_EDITOR
        Debug.Log("CLOSING GAME!!!");
#endif
    }

    public void StartGame() {
        OnGameStart?.Invoke();
        PlayAudio(uiSource, startAudio);
        SetBGM(bgm2);
        StartCoroutine(AnimateMenu());
    }

    private IEnumerator AnimateMenu() {

        while (menuContainer.anchoredPosition3D != Vector3.zero) {
            menuContainer.anchoredPosition3D = Vector3.Lerp(menuContainer.anchoredPosition3D, Vector3.zero, 0.1f);
            yield return null;
        }

        SetMenuVisibility(false, false);
    }

    public void ShowHideGameObject(GameObject obj) {
        PlayButtonSound();
        obj.SetActive(!obj.activeSelf);
    }

    public void ShowLeaderBoard(GameObject signIn) {
        PlayButtonSound();
        UserDataManager.Instance.ShowLeaderBoardUI(signIn);
    }

    public void ShowAchievements(GameObject signIn) {
        PlayButtonSound();
        UserDataManager.Instance.ShowAchievementsUI(signIn);
    }

    public void LogInToPlayGames(GameObject popUp) {
        PlayButtonSound();
        UserDataManager.Instance.SignIn(popUp, signInButton);
    }

    #endregion /UI Region

    #region Audio Region

    private void SetBGM(AudioClip newBGM) {
        bgmSource.clip = newBGM;
        bgmSource.loop = true;

        if (!bgmSource.isPlaying)
            bgmSource.Play();
    }

    private void PlayAudio(AudioSource source, AudioClip clip) {
        if (source.isPlaying)
            source.Stop();

        source.PlayOneShot(clip);
    }

    private void PlayAudioUninterupted(AudioSource source, AudioClip clip) {
        source.PlayOneShot(clip);
    }

    public void PlayButtonSound(bool isSuccess = false) {

        if(isSuccess)
            PlayAudio(uiSource, successSound);
        else
            PlayAudio(uiSource, buttonClip);
    }

    public void UpdateAudio(bool isMute) {
        AudioListener.volume = (isMute) ? 1f : 0f;
        UserDataManager.Instance.hasAudio = isMute;
        UpdateAudioDisplayButtons(isMute);
    }

    private void UpdateAudioDisplayButtons(bool hasAudio) {
        hasAudioButton.SetActive(hasAudio);
        noAudiobutton.SetActive(!hasAudio);
    }

    #endregion /Audio Region

    #region Gameplay Region

    private void SetLevelDisplay(float exp) {
        currentExp += exp;
        levelFill.fillAmount = currentExp / requiredLevelExp;

        if (currentExp >= requiredLevelExp) ActivateLevelUpEvents();          
    }

    private void SetNewLevelRequirements() {
        currentLevel++;
        levelText.text = "Level: " + currentLevel.ToString();
        levelFill.fillAmount = 0f;
        currentExp = 0;
        requiredLevelExp += (requiredLevelExp * levelMultiplier);
    }

    private void ActivateLevelUpEvents() {
        PlayAudio(uiSource, levelupClip);
        SetNewLevelRequirements();
        OnLevelUp?.Invoke();
    }

    public void SetScore(int points, Vector3 effectPos, bool hasEffect = true) {        

        if (gameOver) {
            PlayAudio(fruitSource, explosionClip);
            ShowDestroyEffect(effectPos, explosionPool);
            return;
        }

        score += points;
        scoreText.text = "Score: " + score.ToString();
        SetLevelDisplay(points);

        if (!hasEffect) return;

        PlayAudio(fruitSource, explosionClip);
        PlayAudio(pointsSource, pointsClip);

        TextMeshProUGUI text = scoreTextPool.GetPooledObject(cam.WorldToScreenPoint(effectPos), Quaternion.identity)
            .GetComponentInChildren<TextMeshProUGUI>();

        if(text) text.text = "+" + points.ToString();

        ShowDestroyEffect(effectPos, explosionPool);
    }

    public void CoinPickedUp(Vector3 effectPos) {

        ShowDestroyEffect(effectPos, coinEffectPool);
        PlayAudio(pointsSource, pointsClip);

        if (gameOver) return;

        coins++;
        coins = Mathf.Clamp(coins, 0, 99999);
        coinText.text = "x" + coins.ToString();
    }

    public void ShowDestroyEffect(Vector3 pos, ObjectPooler pooler) {
        pooler.GetPooledObject(pos, Quaternion.identity);
    }

    public void ShowDestroyEffect(Vector3 pos, bool playSound) {
        explosionPool.GetPooledObject(pos, Quaternion.identity);
        PlayAudioUninterupted(bulletSource, bulletHitClips[UnityEngine.Random.Range(0, bulletHitClips.Length)]);
    }

    public void DisplayCoins(GameObject obj) {
        TextMeshProUGUI text = obj.GetComponent<TextMeshProUGUI>();

        if(text) text.text = "x" + UserDataManager.Instance.userData.coins.ToString();
    }

    public void Revive(GameObject obj) {

        coins -= 2;
        coins = Mathf.Clamp(coins, 0, 99999);
        coinText.text = "x" + coins.ToString();

        UserDataManager.Instance.SetCoins(-2);
        PlayAudioUninterupted(uiSource, startAudio);
        obj.SetActive(false);
        preReviveCoins = coins;
        SetBGM(bgm2);
        gameOver = false;
        overlay.SetActive(false);
        pauseButton.SetActive(true);
        hasRevive = true;
        Player.instance.Revive();
        trophy.SetActive(false);
        reviveButton.SetActive(false);
    }

    public void EndGame() {
        SetBGM(bgm1);
        statusText.text = "Game Over";
        gameOver = true;
        overlay.SetActive(true);
        pauseButton.SetActive(false);
        playbutton.SetActive(false);

        //Check the amount of counts to be added
        int c = coins - preReviveCoins;
        c = Mathf.Clamp(c, 0, 99999);
        UserDataManager.Instance.SetCoins(c);
        bool isBest = UserDataManager.Instance.SetBestScore(score);

        if(!hasRevive && UserDataManager.Instance.userData.coins >= 2)
            reviveButton.SetActive(true);
            
        if (isBest)
            trophy.SetActive(true);
    }

    #endregion /Gameplay Region
}
