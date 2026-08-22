# Play Console Data safety form — Candy Belt Sort

Fill **App content → Data safety** to match this. Update if you add analytics, IAP, or extra SDKs.

## Overview answers

- Does your app collect or share user data? **Yes** (advertising SDK)
- Is all user data encrypted in transit? **Yes** (HTTPS via Google Play services / AdMob)
- Do you provide a way for users to request data deletion? **Yes** — uninstall + contact email; AdMob data via Google account ads settings

## Data types collected / shared

### App activity
- App interactions: **not collected by us**. AdMob may collect ad interactions. Mark **shared** with third parties, purpose **Advertising or marketing**.

### App info and performance
- Crash logs: **No** at launch (add Firebase later if you want)
- Diagnostics: **No** at launch

### Device or other IDs
- Device or other IDs: **Yes, shared**
  - Collected: Advertising ID
  - Purpose: Advertising or marketing
  - Optional vs required: **Required** for ads-supported free app (or mark optional if you later add a paid ad-free SKU)
  - Collected / shared: **Shared** with Google AdMob
  - Encrypted in transit: Yes
  - Users can request deletion: Yes (reset advertising ID; uninstall)

### Location
- Approximate location: AdMob may infer from IP. If the form requires it, mark **shared**, purpose Advertising, not sold.
- Precise location: **No**

### Personal info (name, email)
- **No**

### Financial / IAP
- **No** at launch. When IAP ships, add Purchase history collected by Google Play, purpose App functionality.

### Photos, audio, contacts, calendar
- **No**

## Advertising ID declaration

Play Console → Policy → Advertising ID: **Yes, we use advertising ID** for ads.

## Families

Do **not** complete Designed for Families. Target age **13+ / Teen**.
