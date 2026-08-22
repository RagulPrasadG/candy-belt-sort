# Candy Belt Sort

Unity 6 URP hybrid-casual puzzle for Google Play. Tap candies on a conveyor into matching boxes. Ads first, IAP later.

## Open in Unity

1. Unity Hub → Open → this folder (`PlayStoreGameProjects`).
2. Editor **6000.2.8f1** (already in Hub).
3. Press Play on `Assets/Scenes/SampleScene.unity`. The game boots itself (world map → levels).

Portrait Android package: `com.playstoregames.candybeltsort`

## What's in v1

- 3D conveyor, tap-to-box, win/fail, juice and haptics
- **120 levels** across 10 worlds (generated from seeds, not 120 unique maps)
- World map, coins, daily streak, undo, extra slot, continue
- AdMob-ready rewarded + interstitial with placement rules (mock ads in Editor)
- Store listing copy, privacy policy, Data safety answers, closed-test checklist

## Ads

Editor uses fake ads. For a real Android build:

1. Package Manager → Add package from git URL:  
   `https://github.com/googleads/googleads-mobile-unity.git?path=source/plugin`
2. Menu **Candy Belt Sort → Add GOOGLE_MOBILE_ADS Define (Android)**
3. Replace test IDs in `AdMobProvider.cs` and `Assets/Plugins/Android/AndroidManifest.xml`

Rules already in code: no interstitial on the first 3 levels, every 2–3 wins after that, never back-to-back with a rewarded ad, rewarded only when the player asks (undo / extra slot / continue).

**Do not** target children or enroll in Designed for Families.

## Store kit

See `store/` for listing text, privacy policy, Data safety, creatives brief, and closed-test steps.

Menu **Candy Belt Sort → Export Level Catalog JSON** writes `Assets/Resources/CandyBelt/levels.json`.
