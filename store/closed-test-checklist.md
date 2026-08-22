# Closed testing checklist (Play Console)

Google requires a closed test before production access for new personal developer accounts.

## Before you upload

- [ ] Unity Android build (AAB) with package `com.playstoregames.candybeltsort`
- [ ] Target API level required by Play (Unity 6 default is fine; confirm in Play Console warnings)
- [ ] 16 KB page-size compatibility (Unity 6000.2.8 handles this)
- [ ] Replace AdMob **test** App ID in `Assets/Plugins/Android/AndroidManifest.xml` if you are showing real ads; keep test IDs until the AdMob app is approved
- [ ] Privacy policy hosted on HTTPS; URL pasted in App content
- [ ] Data safety form completed (`store/data-safety.md`)
- [ ] Content rating questionnaire completed
- [ ] Ads declaration: app contains ads
- [ ] Target audience 13+ (not children)
- [ ] Store listing: name, short/full description (`store/listing.md`)
- [ ] Icon 512×512 (`store/art/icon.png`)
- [ ] Feature graphic 1024×500 (`store/art/feature-graphic.png` — crop the 16:9 export if needed)
- [ ] At least 2 phone screenshots (capture from device or **Candy Belt Sort → Capture Game View PNG** while playing)
- [ ] Payments profile if you will add IAP later (not required for ads-only)

## Closed test

1. Play Console → Testing → Closed testing → Create track
2. Add email list of **12+ testers** (Gmail accounts)
3. Upload AAB, release to the closed track
4. Testers opt in via the track link and **install from Play**
5. Keep the test live **14 days** (current production-access rule for new accounts)
6. Confirm testers stay opted in; Play checks real installs

## Production

- Soft launch in 1–2 countries
- Watch crash-free rate, ANRs, and D1 retention
- Then roll out worldwide
- Only then spend a small UA budget ($50–150) to test CPI vs retention

## IAP later (do not ship in v1)

Hints pack, extra slot, remove-ads, starter pack, belt/box cosmetics. Add Play Billing, update Data safety, and a new content rating if needed.
