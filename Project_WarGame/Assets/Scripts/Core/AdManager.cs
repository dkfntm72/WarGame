using UnityEngine;
using GoogleMobileAds.Api;

public class AdManager : MonoBehaviour
{
    public static AdManager Instance { get; private set; }

    // 실 배포 시 AdMob 콘솔에서 발급한 ID로 교체
    private const string AdUnitId = "ca-app-pub-3940256099942544/5224354917"; // 테스트용 보상형

    public const int RewardGold     = 100;
    public const int MaxAdsPerStage = 3;

    private RewardedAd _rewardedAd;
    private int        _adsWatched;

    public bool CanShowAd    => _adsWatched < MaxAdsPerStage && _rewardedAd != null;
    public int  AdsRemaining => MaxAdsPerStage - _adsWatched;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        MobileAds.Initialize(_ => LoadAd());
    }

    private void LoadAd()
    {
        _rewardedAd?.Destroy();
        _rewardedAd = null;

        RewardedAd.Load(AdUnitId, new AdRequest(), (ad, error) =>
        {
            if (error != null)
            {
                Debug.LogWarning("[AdManager] 광고 로드 실패: " + error);
                return;
            }
            _rewardedAd = ad;
            GameUI.Instance?.RefreshAdButton();
        });
    }

    public void ShowRewardedAd()
    {
        if (!CanShowAd) return;

        var ad = _rewardedAd;
        _rewardedAd = null; // 광고 표시 중 중복 호출 방지

        ad.OnAdFullScreenContentClosed += () => LoadAd();

        ad.Show(reward =>
        {
            _adsWatched++;
            ResourceManager.Instance.AddGold(Faction.Player, RewardGold);
            Debug.Log($"[AdManager] 보상 지급: +{RewardGold}G (남은 횟수: {AdsRemaining})");
        });
    }
}
