using UnityEngine;
using System;
using UnityEngine.Advertisements;
using Unity.VisualScripting;

namespace Ads
{
    public class AdsService : MonoBehaviour,
    IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
    {
        [Header("Android")]
        [SerializeField] private string _gameId = "";
        [SerializeField] private string _bannerId = "Banner_Android";
        [SerializeField] private string _interstitialId = "Interstitial_Android";
        [SerializeField] private string _rewardedId = "Rewarded_Android";

        [Header("Settings")]
        [SerializeField] private bool _testMode = true;
        [SerializeField] private float _interstitialInterval = 300f;
        [SerializeField] private float _retryDelay = 10f;

        public bool IsInitialized => Advertisement.isInitialized;
        public bool IsBannerVisible { get; private set; }
        public bool IsInterstitialReady { get; private set; }
        public bool IsRewardedReady { get; private set; }
        public bool IsShowing { get; private set; }
        public float InterstitialInterval => _interstitialInterval;

        private bool _bannerWanted;
        private bool _bannerLoading;

        private Action _onInterstitialClosed;
        private Action _onRewarded;
        private Action _onRewardFailed;

        private void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (IsInitialized) return;
            if (string.IsNullOrEmpty(_gameId))
            {
                Debug.LogError("[Ads] Game ID is not set");
                return;
            }
            Debug.Log($"[Ads] Initialize, gameId={_gameId}");
            Advertisement.Initialize(_gameId, _testMode || Application.isEditor, this);
        }

        public void OnInitializationComplete()
        {
            Debug.Log("Initialization Complete");
            LoadInterstitial();
            LoadRewarded();
            LoadBanner();
        }

        public void OnInitializationFailed(UnityAdsInitializationError error, string message)
        {
            Debug.LogError($"Initialization Failed: {error} {message}");
            Invoke(nameof(Initialize), _retryDelay);
        }

        public void LoadBanner()
        {
            if (!IsInitialized || _bannerLoading || Advertisement.Banner.isLoaded) return;

            _bannerLoading = true;
            Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);
            Advertisement.Banner.Load(_bannerId, new BannerLoadOptions
            {
                loadCallback = () =>
                {
                    _bannerLoading = false;
                    if (_bannerWanted) ShowBanner();
                },
                errorCallback = message =>
                {
                    _bannerLoading = false;
                    Debug.LogWarning($"Load Banner Failed: {message}");
                    Invoke(nameof(LoadBanner), _retryDelay);
                }
            });
        }

        public void ShowBanner()
        {
            _bannerWanted = true;

            if (!Advertisement.Banner.isLoaded)
            {
                LoadBanner();
                return;
            }

            Advertisement.Banner.Show(_bannerId);
            IsBannerVisible = true;
        }

        public void HideBanner()
        {
            _bannerWanted = false;
            Advertisement.Banner.Hide();
            IsBannerVisible = false;
        }

        public void LoadInterstitial()
        {
            if (IsInitialized && !IsInterstitialReady)
                Advertisement.Load(_interstitialId, this);
        }

        public void ShowInterstitial(Action onClosed = null)
        {
            if (!IsInterstitialReady || IsShowing)
            {
                onClosed?.Invoke();
                return;
            }

            _onInterstitialClosed = onClosed;
            IsInterstitialReady = false;
            IsShowing = true;
            Advertisement.Show(_interstitialId, this);
        }

        public void LoadRewarded()
        {
            if (IsInitialized && !IsRewardedReady)
                Advertisement.Load(_rewardedId, this);
        }

        public void ShowRewarded(Action onRewarded, Action onFailed = null)
        {
            if (!IsRewardedReady || IsShowing)
            {
                LoadRewarded();
                onFailed?.Invoke();
                return;
            }

            _onRewarded = onRewarded;
            _onRewardFailed = onFailed;
            IsRewardedReady = false;
            IsShowing = true;
            Advertisement.Show(_rewardedId, this);
        }

        public void OnUnityAdsAdLoaded(string placementId)
        {
            Debug.Log($"Ads AdLoaded: {placementId}");
            if (placementId == _interstitialId) IsInterstitialReady = true;
            else if (placementId == _rewardedId) IsRewardedReady = true;
        }

        public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
        {
            Debug.LogWarning($"Ads Failed To Load {placementId}: {error} {message}");
            if (placementId == _interstitialId) Invoke(nameof(LoadInterstitial), _retryDelay);
            else if (placementId == _rewardedId) Invoke(nameof(LoadRewarded), _retryDelay);
        }

        public void OnUnityAdsShowStart(string placementId) { }
        public void OnUnityAdsShowClick(string placementId) { }

        public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState state)
        {
            Debug.Log($"Ads Show Complete: {placementId}, {state}");
            OnShowFinished(placementId, state == UnityAdsShowCompletionState.COMPLETED);
        }

        public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
        {
            Debug.LogWarning($"Ads Show Failure {placementId}: {error} {message}");
            OnShowFinished(placementId, false);
        }

        private void OnShowFinished(string placementId, bool completed)
        {
            IsShowing = false;

            if (placementId == _rewardedId)
            {
                var callback = completed ? _onRewarded : _onRewardFailed;
                _onRewarded = _onRewardFailed = null;
                callback?.Invoke();
                LoadRewarded();
            }
            else if (placementId == _interstitialId)
            {
                var callback = _onInterstitialClosed;
                _onInterstitialClosed = null;
                callback?.Invoke();
                LoadInterstitial();
            }
        }
    }
}