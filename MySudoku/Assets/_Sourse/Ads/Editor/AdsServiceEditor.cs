using UnityEditor;
using UnityEngine;

namespace Ads
{
    public class AdsServiceEditor : Editor
    {
        public override bool RequiresConstantRepaint()
        {
            return Application.isPlaying;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Test Ads", EditorStyles.boldLabel);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Buttons available in Play Mode.", MessageType.Info);
                return;
            }

            var ads = (AdsService)target;

            EditorGUILayout.LabelField("Initialized", ads.IsInitialized.ToString());
            EditorGUILayout.LabelField("Interstitial ready", ads.IsInterstitialReady.ToString());
            EditorGUILayout.LabelField("Rewarded ready", ads.IsRewardedReady.ToString());
            EditorGUILayout.LabelField("Banner visible", ads.IsBannerVisible.ToString());

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Show Banner")) ads.ShowBanner();
                if (GUILayout.Button("Hide Banner")) ads.HideBanner();
            }

            if (GUILayout.Button("Show Interstitial"))
                ads.ShowInterstitial(() => Debug.Log("[Ads] Interstitial closed"));

            if (GUILayout.Button("Show Rewarded"))
                ads.ShowRewarded(
                    onRewarded: () => Debug.Log("[Ads] Reward given"),
                    onFailed: () => Debug.Log("[Ads] No reward"));
        }
    }
}