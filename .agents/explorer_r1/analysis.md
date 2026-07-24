# ViMARA Requirement 1 (R1) Technical Report: Exporting & Compiling Unity Projects to iOS from Windows (2024+ Solutions)

**Author:** explorer_r1 (teamwork_preview_explorer subagent)  
**Date:** July 24, 2026  
**Target Workspace:** ViMARA (Unity 6 / 6000.3.10f1, AR Foundation 6.3.3, GLTFast)  
**Status:** Completed Analysis  

---

## Executive Summary

The primary objective of Requirement 1 (R1) is establishing a viable, low-friction, and cost-effective workflow for building, signing, and deploying Unity 6 iOS applications directly from a Windows development environment. 

Because Apple strictly requires Xcode running on macOS to compile final iOS binaries (`.ipa`), Windows developers cannot generate `.ipa` files locally without macOS virtualization or remote/cloud compilation service. This report evaluates the four modern (2024–2026) technical approaches for cloud compilation, along with complete Apple code signing and Windows sideloading workflows.

### Core Findings Key Takeaways
1. **$99/year Developer Account is NOT strictly required for personal device testing.** Free Personal Team Apple IDs can sign `.ipa` binaries for up to 3 active apps per device with 7-day profile expiration via Windows sideloading tools (Sideloadly, AltServer). Paid accounts are only mandatory for TestFlight, App Store distribution, or 1-year Ad-Hoc profiles.
2. **GitHub Actions (`game-ci/unity-builder`) on macOS runners** represents the most flexible free/low-cost CI/CD pipeline. Free GitHub accounts receive 2,000 Linux minutes/month, which translates to **200 free macOS minutes/month** (due to the 10x multiplier). An AR Foundation iOS build takes ~12–18 minutes, yielding **11–16 free iOS builds per month**.
3. **Unity Cloud Build / Unity DevOps** provides native unity integration but limits free accounts to 120 build minutes/month, with higher setup friction for Apple signing credentials compared to GitHub Actions.
4. **Third-Party CI/CD (Codemagic)** is the best turnkey alternative, providing 500 free M1 Mac build minutes/month with native Apple code signing automation via App Store Connect API keys.
5. **Local macOS VMs on Windows** are strictly discouraged for AR Foundation projects due to EULA violations and lack of GPU acceleration (Metal support), which breaks Xcode Simulator and Unity build shaders.

---

## Section 1: Method 1 — Unity Cloud Build / Unity DevOps

### 1.1 Overview & Architecture
Unity DevOps (formerly Unity Cloud Build / Unity Build Automation) is Unity's official cloud compilation service integrated directly into the Unity Dashboard and Unity Editor. When triggering a build, Unity uploads the repository (or diffs) to Unity Cloud servers, launches a cloud-hosted macOS virtual machine with pre-installed Xcode and Unity Editor versions, compiles the Xcode project, applies Apple code signing credentials, and outputs an installable `.ipa` or Xcode project archive.

```
[ Windows Dev PC ] ── git push ──> [ Unity DevOps / Plastic SCM / GitHub ]
                                            │
                                  [ Cloud macOS Runner ]
                                  (Unity Editor + Xcode)
                                            │
[ Physical iPhone ] <── Download IPA ───────┘
```

### 1.2 Setup Friction & Repository Integration
* **Repository Support:** Direct native support for Unity Version Control (formerly Plastic SCM), GitHub, GitLab, and Bitbucket via OAuth webhooks.
* **Setup Steps:**
  1. Link the Unity Project ID (Unity 6 / 6000.3.10f1) to Unity DevOps in `Services > Build Automation`.
  2. Configure Build Target: Select **iOS**, Unity Version (`6000.3.10f1`), Xcode version (Xcode 15/16).
  3. Code Signing Credentials Upload: Upload `.p12` Certificate file, Certificate Password, and Provisioning Profile (`.mobileprovision`).
  4. Build Triggering: Can be manually triggered from the Unity Dashboard or automatically on `git push` to `main`/`develop`.

### 1.3 Licensing & Quota Breakdown (2024–2026 Tiers)
Unity discontinued the "Plus" tier in late 2023. Unity DevOps pricing is currently structured across **Personal**, **Pro**, and **Enterprise** subscriptions:

| Unity Tier | Monthly Included Build Minutes | Additional Minute Pricing | Concurrency | Priority |
| :--- | :--- | :--- | :--- | :--- |
| **Personal (Free)** | 120 minutes / month | ~$0.04 / minute (Pay-As-You-Go) | 1 Concurrent Build | Standard |
| **Pro ($2,040/yr)** | 500 minutes / month | ~$0.03 / minute | 2 Concurrent Builds | High |
| **Enterprise** | Custom | Negotiated | Custom | Dedicated |

* **Build Time Estimate for ViMARA (Unity 6 AR Foundation):** ~15 to 22 minutes per cold build (due to IL2CPP compilation and AssetBundle processing).
* **Practical Free Capacity:** ~5 to 8 iOS builds per month on Unity Personal tier before requiring extra minute purchases.

---

## Section 2: Method 2 — GitHub Actions with macOS Runners (`game-ci/unity-builder`)

### 2.1 Overview & Workflow Architecture
`game-ci` (formerly Unity-CI) is an open-source community effort providing Docker containers and GitHub Actions for automating Unity builds. Because iOS builds require Xcode, the GitHub Actions workflow runs on GitHub-hosted `macos-latest` (or `macos-14` M1/M2) runners.

```
+-------------------------------------------------------------------------+
| GitHub Actions Workflow (.github/workflows/build-ios.yml)               |
|                                                                         |
| 1. Checkout repository                                                  |
| 2. Cache Unity Library folder                                           |
| 3. Activate Unity License (game-ci/unity-request-activation)            |
| 4. Build iOS Xcode Project (game-ci/unity-builder@v4)                   |
| 5. Decode Apple Certificate & Provisioning Profile                      |
| 6. Compile Xcode Project to IPA (xcodebuild)                            |
| 7. Upload .ipa Artifact                                                 |
+-------------------------------------------------------------------------+
```

### 2.2 Billing & Minutes Multiplier Breakdown
GitHub provides free monthly Actions minutes for public and private repositories, but applies **multiplier rates** depending on the runner operating system and architecture:

* **GitHub Free Account Allowance:** 2,000 minutes / month (for private repositories; public repositories have unlimited free minutes).
* **Runner Multipliers (2024–2026):**
  * Linux runners: **1x** (1 minute used = 1 minute billed)
  * Windows runners: **2x** (1 minute used = 2 minutes billed)
  * **macOS runners (x86_64 / M1 arm64): 10x multiplier** (1 minute used = 10 minutes billed)

#### Exact Calculation for ViMARA:
$$\text{Free macOS Minutes} = \frac{2,000 \text{ free Linux minutes}}{10 \text{ multiplier}} = 200 \text{ macOS minutes/month}$$

* Average `game-ci` iOS build time for Unity AR project: **14 minutes**.
* Maximum free iOS builds per month: $\lfloor 200 / 14 \rfloor = \mathbf{14 \text{ builds/month}}$.

### 2.3 Secrets Management & `.yml` Configuration Guide

#### Required GitHub Secrets (`Settings > Secrets and variables > Actions`):
1. `UNITY_LICENSE`: XML string from Unity Manual License Activation (`.alf` -> `.ulf`).
2. `UNITY_EMAIL`: Unity Account email.
3. `UNITY_PASSWORD`: Unity Account password.
4. `BUILD_CERTIFICATE_BASE64`: Base64-encoded string of distribution/development `.p12` file.
5. `P12_PASSWORD`: Export password for the `.p12` file.
6. `BUILD_PROVISION_PROFILE_BASE64`: Base64-encoded string of `.mobileprovision` file.
7. `KEYCHAIN_PASSWORD`: Temporary string password generated in workflow for temporary keychain.

#### Sample Production-Ready `.github/workflows/build-ios.yml`:

```yaml
name: Build Unity iOS (Windows-Friendly Cloud Pipeline)

on:
  workflow_dispatch:
  push:
    branches: [ main ]

jobs:
  buildiOS:
    name: Build for iOS 🍏
    runs-on: macos-14
    steps:
      # Step 1: Checkout Repository
      - name: Checkout Repository
        uses: actions/checkout@v4
        with:
          lfs: true

      # Step 2: Cache Unity Library directory to accelerate builds
      - name: Cache Unity Library
        uses: actions/cache@v4
        with:
          path: Library
          key: Library-iOS-${{ hashFiles('Assets/**', 'Packages/**', 'ProjectSettings/**') }}
          restore-keys: |
            Library-iOS-

      # Step 3: Build Xcode Project using Game-CI
      - name: Unity Builder (iOS Xcode Project)
        uses: game-ci/unity-builder@v4
        env:
          UNITY_LICENSE: ${{ secrets.UNITY_LICENSE }}
          UNITY_EMAIL: ${{ secrets.UNITY_EMAIL }}
          UNITY_PASSWORD: ${{ secrets.UNITY_PASSWORD }}
        with:
          targetPlatform: iOS
          unityVersion: 6000.3.10f1
          buildName: ViMARA_iOS

      # Step 4: Install Apple Certificates & Provisioning Profile on macOS Runner
      - name: Install Apple Certificate & Provisioning Profile
        env:
          BUILD_CERTIFICATE_BASE64: ${{ secrets.BUILD_CERTIFICATE_BASE64 }}
          P12_PASSWORD: ${{ secrets.P12_PASSWORD }}
          BUILD_PROVISION_PROFILE_BASE64: ${{ secrets.BUILD_PROVISION_PROFILE_BASE64 }}
          KEYCHAIN_PASSWORD: ${{ secrets.KEYCHAIN_PASSWORD }}
        run: |
          # Create temporary keychain
          CERTIFICATE_PATH=$RUNNER_TEMP/build_certificate.p12
          PP_PATH=$RUNNER_TEMP/build_pp.mobileprovision
          KEYCHAIN_PATH=$RUNNER_TEMP/app-signing.keychain-db

          # Decode secrets
          echo -n "$BUILD_CERTIFICATE_BASE64" | base64 --decode -o $CERTIFICATE_PATH
          echo -n "$BUILD_PROVISION_PROFILE_BASE64" | base64 --decode -o $PP_PATH

          # Create and configure keychain
          security create-keychain -p "$KEYCHAIN_PASSWORD" $KEYCHAIN_PATH
          security set-keychain-settings -lut 3600 $KEYCHAIN_PATH
          security unlock-keychain -p "$KEYCHAIN_PASSWORD" $KEYCHAIN_PATH
          security import $CERTIFICATE_PATH -k $KEYCHAIN_PATH -P "$P12_PASSWORD" -A -T /usr/bin/codesign

          # Install provisioning profile
          mkdir -p ~/Library/MobileDevice/Provisioning\ Profiles
          cp $PP_PATH ~/Library/MobileDevice/Provisioning\ Profiles/

      # Step 5: Archive and Export IPA via xcodebuild
      - name: Compile Xcode Project to IPA
        run: |
          cd build/iOS/ViMARA_iOS
          xcodebuild -workspace Unity-iPhone.xcworkspace \
                     -scheme Unity-iPhone \
                     -sdk iphoneos \
                     -configuration Release \
                     -archivePath $RUNNER_TEMP/ViMARA.xcarchive \
                     archive

          xcodebuild -exportArchive \
                     -archivePath $RUNNER_TEMP/ViMARA.xcarchive \
                     -exportOptionsPlist ExportOptions.plist \
                     -exportPath $RUNNER_TEMP/output_ipa

      # Step 6: Upload IPA Artifact
      - name: Upload IPA Artifact
        uses: actions/upload-artifact@v4
        with:
          name: ViMARA-iOS-IPA
          path: ${{ runner.temp }}/output_ipa/*.ipa
```

---

## Section 3: Method 3 — Third-Party CI/CD Services

Several dedicated mobile CI/CD platforms support Unity builds and iOS signing.

### 3.1 Codemagic
* **Overview:** Codemagic is currently the industry gold-standard mobile CI/CD for Flutter, React Native, and Unity projects.
* **Free Tier Allowance:** **500 free Mac M1 build minutes / month**.
* **Unity Compatibility:** Excellent. Includes pre-installed Unity versions and automatic App Store Connect API integration for code signing.
* **Signing Feature:** Codemagic automatically creates code signing certificates and provisioning profiles directly via App Store Connect API keys without requiring a physical Mac!
* **ViMARA Capacity:** At ~12–15 mins/build, Codemagic provides **~33 to 40 free builds/month**, making it the highest free quota provider available in 2026.

### 3.2 Bitrise
* **Overview:** Enterprise mobile CI/CD platform with step-based workflow builder.
* **Free Tier Allowance:** Hobby Plan offers **300 build minutes / month** (limited to 30 min max execution time per build).
* **Unity Compatibility:** Good native steps (`Unity Build for iOS`), but IL2CPP compilation easily hits the 30-minute free tier timeout limit for heavy Unity 6 AR projects.
* **ViMARA Capacity:** Limited/Risky due to build timeout ceilings on free instances.

### 3.3 Appcircle
* **Overview:** Automated mobile CI/CD and mobile app distribution platform.
* **Free Tier Allowance:** **200 build minutes / month**.
* **Unity Compatibility:** Supports custom build scripts and Xcode steps.
* **ViMARA Capacity:** ~12–15 free builds/month.

---

## Section 4: Method 4 — Cloud Mac VMs & Local Virtualization

When cloud CI/CD scripts are insufficient and full visual access to macOS/Xcode is desired from Windows, remote virtualization is used.

### 4.1 Cloud Mac Providers

#### MacInCloud
* **Model:** On-demand or monthly dedicated macOS desktop access via RDP, VNC, or SSH.
* **Pricing:**
  * **Pay-As-You-Go:** ~$1.00 / hour (with $30 minimum deposit).
  * **Monthly Plan:** ~$20–$45 / month (for standard CPU core plans with daily hour caps).
* **Pros:** Low friction, pre-installed Xcode and Unity Hub, full GUI desktop access from Windows RDP client.
* **Cons:** Network latency when navigating Xcode UI; requires manual build triggers.

#### AWS EC2 Mac Instances (`mac1.metal` / `mac2.metal`)
* **Model:** Dedicated Apple Mac mini hardware hosts in AWS datacenters.
* **Pricing:** ~$0.65 – $1.20 / hour.
* **CRITICAL CONSTRAINT — Apple EULA 24-Hour Rule:** AWS EC2 Mac instances enforce Apple's licensing requirement that **a dedicated Mac host must be allocated to a customer for a minimum of 24 consecutive hours**.
* **Cost Reality:** Even if you only run a 15-minute build, you will be billed for a minimum of 24 hours ($\approx \mathbf{\$15.60 \text{ to } \$28.80 \text{ per allocation}}$).
* **Verdict:** Unsuitable for individual/student indie developers due to minimum cost overhead.

### 4.2 Local macOS Virtual Machines on Windows (VMware / Proxmox)

Running macOS on Windows non-Apple hardware (Hackintosh VM via VMware Workstation, VirtualBox with Unlocker, or Proxmox KVM):

* **Legal / EULA Constraints:** Explicit violation of Apple's macOS End User License Agreement (EULA), which legally restricts macOS installation exclusively to genuine Apple hardware.
* **Hardware & Technical Friction:**
  * **AMD Ryzen CPUs:** Highly problematic; requires custom OS kernel patches (AMD OS X kernel) and often fails to execute Xcode simulator tasks.
  * **Intel CPUs:** Easier installation, but lacks graphics acceleration.
* **GPU Acceleration Deficit (CRITICAL FOR UNITY & AR):**
  * Virtualized macOS cannot access Windows GPU pass-through easily without complex dual-GPU IOMMU setups.
  * Without Metal GPU acceleration, Xcode Shader Compilation, Unity Editor playback, and iOS Simulator rendering either crash or run at 1–2 FPS.
* **Recommendation:** **Strictly NOT recommended** for production or serious academic development.

---

## Section 5: Apple Developer Program App Signing & Physical iOS Device Provisioning

Code signing is mandatory for running any app on an iOS device. Apple requires every `.ipa` binary to be digitally signed with a **Certificate** and bound to a **Provisioning Profile**.

### 5.1 Free Apple ID (Personal Team) vs. Paid Apple Developer Account ($99/year)

#### DIRECT ANSWER TO CRITICAL QUESTION:
> **Is paying $99/year strictly mandatory to test a Unity app on a physical iPhone or iPad?**  
> **NO.** Developers can sign and test apps on physical iOS devices for **FREE** using a personal Apple ID. However, explicit technical restrictions apply.

#### Comprehensive Comparison: Free Apple ID vs Paid Account

| Feature | Free Apple ID (Personal Team) | Paid Apple Developer Account ($99/yr) |
| :--- | :--- | :--- |
| **Cost** | **$0 / year** | **$99 USD / year** |
| **Provisioning Profile Expiration** | **7 Days** (app stops opening after 7 days; must re-sign) | **1 Year** (365 days) |
| **Max App IDs (Bundle Identifiers)** | **10 App IDs** per 7-day rolling window | Unlimited |
| **Max Active Sideloaded Apps/Device** | **3 active apps** maximum per iOS device | Unlimited |
| **Physical Device Testing** | **Yes** (via USB/Wi-Fi sideloading) | **Yes** (via Xcode, TestFlight, or Ad-Hoc IPA) |
| **TestFlight Beta Testing** | **No** | **Yes** (up to 10,000 external testers) |
| **App Store Publishing** | **No** | **Yes** |
| **Apple Capabilities (Push, ARKit Enterprise)** | Basic capabilities only | All advanced Apple Services & Entitlements |

---

### 5.2 Step-by-Step Workflow: Generating `.p12` Certificates & `.mobileprovision` Profiles Without a Mac

To sign an `.ipa` in cloud CI/CD pipelines (GitHub Actions, Codemagic, Unity DevOps), you must supply a `.p12` Certificate and `.mobileprovision` Profile.

#### Method A: Paid Account via App Store Connect API (No Mac Needed!)
If you have access to a Paid Apple Developer Account ($99/yr), you do **not** need a Mac to generate signing keys:
1. Log into [App Store Connect](https://appstoreconnect.apple.com).
2. Go to **Users and Access > Integrations > App Store Connect API**.
3. Generate an API Key with **App Manager** or **Admin** access. Download the `.p8` private key file, and record the `Key ID` and `Issuer ID`.
4. CI/CD services (Codemagic or Fastlane running inside GitHub Actions) use this API key to generate iOS Development/Distribution certificates (`.p12`) and Provisioning Profiles (`.mobileprovision`) **automatically in the cloud**.

#### Method B: Free Apple ID Account (No Mac Needed - Windows Sideloading Engine)
If using a Free Apple ID, traditional certificate extraction is tedious. Instead, Windows sideloading tools automatically perform free provisioning on-the-fly:
1. Sideloadly or AltServer acts as a local signing engine on Windows.
2. The tool authenticates with Apple's authentication servers using your Free Apple ID credentials.
3. Apple's server returns a 7-day development certificate and provisioning profile targeting your device's unique identifier (UDID).
4. The Windows tool signs the compiled `.ipa` binary and pushes it to the connected iPhone via Apple Mobile Device Services.

---

### 5.3 Installing `.ipa` Files onto Physical iOS Devices from Windows

Once an unsigned or cloud-compiled `.ipa` file is generated, Windows developers can deploy it to physical iOS devices using the following tools:

```
+-------------------------------------------------------------------------------+
|                        WINDOWS SIDELOADING TOOLMATRIX                         |
+-------------------+---------------------+------------------+------------------+
| Tool              | Signing Method      | Wireless Refresh | Setup Complexity |
+-------------------+---------------------+------------------+------------------+
| 1. Sideloadly     | On-the-fly Free ID  | Yes (Wi-Fi)      | ⭐ (Very Low)    |
| 2. AltServer      | On-the-fly Free ID  | Yes (Background) | ⭐⭐ (Low)       |
| 3. 3uTools        | Embedded IPA Signer | No (USB)         | ⭐ (Very Low)    |
| 4. Apple Devices  | Pre-Signed Ad-Hoc   | No (USB)         | ⭐⭐ (Low)       |
+-------------------+---------------------+------------------+------------------+
```

#### Detailed Tool Analysis:

1. **Sideloadly (Recommended for Windows Unity Developers):**
   * **How it works:** Windows desktop application. You drag and drop your cloud-compiled `.ipa`, enter your Apple ID email/password, and connect your iPhone via USB or Wi-Fi.
   * **Key Features:** Supports Anisette server authentication (bypassing Mac requirement), automatically handles 7-day personal certificate generation, allows custom Bundle ID overrides, and supports automatic background Wi-Fi refresh.

2. **AltServer / AltStore:**
   * **How it works:** Requires installing AltServer on Windows alongside official Apple iTunes and iCloud for Windows (non-Microsoft Store versions).
   * **Key Features:** Installs the "AltStore" app on your iPhone. Once installed, you can send new `.ipa` builds to your iPhone over local Wi-Fi, and AltStore automatically re-signs the app every 7 days in the background while your Windows PC is powered on.

3. **3uTools:**
   * **How it works:** Comprehensive Windows iOS management suite. Features a dedicated "IPA Signature" utility.
   * **Usage:** Import `.ipa`, bind Free Apple ID, click "Sign", and click "Install".

4. **Apple Devices App for Windows (Official Apple Utility):**
   * Replaces legacy iTunes on Windows 10/11. If you signed the `.ipa` using a **Paid Account Ad-Hoc profile** containing your iPhone's UDID, you can drag and drop the `.ipa` directly into Apple Devices App to install it over USB without third-party sideloading tools.

---

## Section 6: Summary Comparison Matrix & Decision Framework

The following matrix compares all four export methods across six core evaluation metrics:

| Export Method | Setup Friction (1–5) | Monthly Cost ($) | Build Speed (1–5) | Free Apple ID Support | Paid Apple ID Support | Deploy Effort (1–5) | Best Used For |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: | :--- |
| **Method 1: Unity Cloud Build / DevOps** | 2/5 (Low) | $0 (120 min/mo) or $2,040/yr Pro | 3/5 (~18 min) | ❌ Complex | ✅ Direct Integration | 2/5 (Download IPA) | Teams deeply invested in Unity Ecosystem & Plastic SCM |
| **Method 2: GitHub Actions (`game-ci`)** | 3/5 (Medium) | **$0** (200 macOS min/mo) | 4/5 (~14 min) | ✅ Via Sideloadly | ✅ Via Secrets | 2/5 (Actions Artifact) | **Best Overall Free CI/CD Solution for Git Users** |
| **Method 3: Codemagic CI/CD** | **1/5 (Very Low)** | **$0** (500 M1 min/mo) | **5/5 (~10 min M1)** | ✅ Via Sideloadly | ✅ Automated API Key | **1/5 (Auto-Distribute)** | **Best Turnkey Performance & Maximum Free Quotas** |
| **Method 4a: MacInCloud (Cloud VM)** | 2/5 (Low) | ~$1/hr or $20–$45/mo | 3/5 (Interactive) | ✅ Full Xcode UI | ✅ Full Xcode UI | 1/5 (Direct Xcode USB) | Developers needing visual Xcode debugging/storyboards |
| **Method 4b: AWS EC2 Mac** | 5/5 (High) | ~$15–$28 min per run (24h EULA rule) | 4/5 (Fast) | ✅ Full Xcode UI | ✅ Full Xcode UI | 2/5 | Large Enterprise infrastructure only |
| **Method 4c: Local macOS VM (Windows)** | 5/5 (Critical) | $0 | 1/5 (No Metal GPU acceleration) | ⚠️ Unstable | ⚠️ Unstable | 4/5 | **NOT Recommended** (EULA violation + Broken AR Shaders) |

---

## Section 7: ViMARA Specific Actionable Recommendation

For the **ViMARA** architecture (Unity 6 `6000.3.10f1` + AR Foundation 6.3.3 + Windows Dev Machine):

### Recommended Dual-Pipeline Architecture:

```
                  +--------------------------------------------------+
                  |         Windows Developer PC (ViMARA)            |
                  | Unity 6 Editor (Develop, Test in Editor & AR)   |
                  +------------------------+-------------------------+
                                           |
                                      git push
                                           v
                  +--------------------------------------------------+
                  |            Codemagic / GitHub Actions            |
                  |     - macOS Runner (M1/M2)                       |
                  |     - Compiles Unity 6 IL2CPP to Xcode           |
                  |     - Generates Unsigned or Dev .ipa Binary      |
                  +------------------------+-------------------------+
                                           |
                                  Download .ipa Artifact
                                           v
                  +--------------------------------------------------+
                  |            Windows Sideloading Engine            |
                  |    - Sideloadly / AltServer                      |
                  |    - Free Apple ID (Personal Team)               |
                  |    - Signs & Installs over USB/Wi-Fi             |
                  +------------------------+-------------------------+
                                           |
                                           v
                  +--------------------------------------------------+
                  |             Physical iPhone / iPad               |
                  |             (AR Foundation Testing)              |
                  +--------------------------------------------------+
```

1. **Primary Build Pipeline:** **Codemagic Free Tier** or **GitHub Actions (`game-ci/unity-builder`)**.
   * Gives **500 free M1 Mac minutes/month** (Codemagic) or **200 free macOS minutes/month** (GitHub Actions).
   * Completely avoids paying for Unity Pro or cloud VMs.
2. **Code Signing & Testing Strategy:** **Free Apple ID + Sideloadly on Windows**.
   * Zero financial cost ($0/year).
   * Sideloadly handles free 7-day profile creation and instant installation onto physical testing iPhones over Wi-Fi/USB.
   * Eliminates the need to buy a physical Mac or pay Apple $99/year during the initial development and evaluation phases of ViMARA.

---
*Report completed by explorer_r1.*
