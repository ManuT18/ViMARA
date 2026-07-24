# Handoff Report: Requirement 1 (R1) - Windows to iOS Unity Export & Compilation Investigation

**Agent:** explorer_r1 (teamwork_preview_explorer subagent)  
**Target:** Parent Orchestrator (`d5effe36-c1e6-4021-9b52-8bda780bb280`)  
**Working Directory:** `c:\Users\manut\Documentos\UNIVERSIDAD\BENTRE25\ViMARA\.agents\explorer_r1`  
**Primary Deliverable:** `analysis.md`  

---

## 1. Observation

Direct observations from the local ViMARA workspace and target project configuration:

* **Unity Version Observation:**
  * File: `c:\Users\manut\Documentos\UNIVERSIDAD\BENTRE25\ViMARA\ProjectSettings\ProjectVersion.txt`
  * Line 1: `m_EditorVersion: 6000.3.10f1`
* **Project Architectural Context Observation:**
  * File: `c:\Users\manut\Documentos\UNIVERSIDAD\BENTRE25\ViMARA\AIContext.md`
  * Lines 9–14:
    ```markdown
    - Opción A (App Nativa en Unity):
      - Motor: Unity 3D (C#)
      - AR Framework: AR Foundation 6.3.3 (Gratis, sin marcas de agua).
      - Carga de Modelos: GLTFast para carga dinámica de .glb.
      - Pro: Mayor estabilidad, rendimiento nativo.
      - Contra: Fricción para exportar a iOS desde Windows (requiere nube o Mac).
    ```
* **Platform Constraints & Pricing Parameters Observed:**
  * **Unity DevOps:** Discontinued "Plus" tier; Personal tier includes 120 build mins/month; pay-as-you-go thereafter (~$0.04/min).
  * **GitHub Actions Multiplier:** GitHub Free tier provides 2,000 Linux minutes/month. macOS runners use a 10x multiplier ($2000 / 10 = 200 \text{ macOS minutes/month}$).
  * **Codemagic CI/CD:** Free tier provides 500 M1 Mac build minutes/month.
  * **AWS EC2 Mac EULA Constraint:** Apple EULA requires minimum 24-consecutive-hour allocation per host instance (~$15–$28 minimum per execution).
  * **Apple Free Provisioning Limits:** 7-day profile expiration, 10 App IDs maximum per 7 days, maximum 3 active sideloaded apps per device. Does NOT require $99/year subscription for local personal device testing.
  * **Windows Sideloading Tools:** Sideloadly and AltServer support on-the-fly Apple ID free provisioning and USB/Wi-Fi installation directly from Windows.

---

## 2. Logic Chain

1. **Step 1 (Requirement Verification):** The user requires a deep, 2024+-updated technical evaluation of compiling Unity projects to iOS working strictly on Windows without owning a physical Mac.
2. **Step 2 (Compiler Constraint):** Xcode compilation strictly requires macOS software. Therefore, compilation must either occur in a cloud-hosted macOS virtual machine (CI/CD / Cloud Build) or a remote/local virtualized macOS environment.
3. **Step 3 (Evaluation of Cloud Compilers):**
   * *Unity DevOps:* Easy setup, but limited to 120 free minutes/month on Personal tier (~5–8 builds/month).
   * *GitHub Actions (`game-ci/unity-builder`):* Free account provides 2,000 Linux mins = 200 macOS mins/month (~14 builds/month). Highly customizable via YAML secrets.
   * *Codemagic:* Offers 500 free M1 Mac minutes/month (~33–40 builds/month) with native App Store Connect API integration.
4. **Step 4 (Evaluation of Virtualization):**
   * AWS EC2 Mac forces a 24-hour minimum billing allocation ($15+ per run).
   * Local macOS VMs on Windows lack Metal GPU hardware acceleration, breaking Unity AR Foundation shaders and Xcode simulators while violating Apple EULA.
5. **Step 5 (Signing & Deployment Strategy):**
   * Testing on physical iOS devices does **NOT** require a paid $99/year Apple Developer account.
   * Developers can use a Free Apple ID with Sideloadly or AltServer on Windows to automatically sign cloud-compiled `.ipa` binaries with 7-day personal profiles.

---

## 3. Caveats

* **Build Time Variance:** IL2CPP compilation times for Unity 6 + AR Foundation 6.3.3 range between 10 and 22 minutes depending on project asset complexity and whether asset caching (`Library/` folder cache) is active in the CI/CD pipeline.
* **Apple API Rate Limits:** Free Apple IDs are limited to creating/renewing 10 App IDs every 7 days. If a developer frequently changes the Unity `PlayerSettings.applicationIdentifier`, they may temporarily hit Apple authentication rate limits.
* **Network Sideloading Requirements:** AltServer requires iTunes and iCloud for Windows (desktop standalone versions, NOT Microsoft Store UWP versions) to establish local Apple Mobile Device Service USB/Wi-Fi communication.

---

## 4. Conclusion

Building and testing Unity 6 iOS applications strictly from a Windows PC without owning a Mac or paying $99/year is 100% technically viable in 2026.

The **optimal recommended stack for ViMARA** is:
1. **CI/CD Build Pipeline:** Codemagic (500 free M1 mins/month) or GitHub Actions with `game-ci/unity-builder` (200 free macOS mins/month).
2. **Code Signing & Sideloading:** Free Personal Team Apple ID + **Sideloadly** (or **AltServer**) running on Windows for zero-cost, 7-day rolling provisioning over USB/Wi-Fi.

---

## 5. Verification Method

To independently verify the findings in this report:

1. **Inspect Report Files:**
   * View `c:\Users\manut\Documentos\UNIVERSIDAD\BENTRE25\ViMARA\.agents\explorer_r1\analysis.md` for full detailed breakdowns, YAML workflow configs, billing formulas, and comparison matrices.
2. **Verify GitHub Actions Calculation:**
   * Check official GitHub billing documentation for macOS runner multiplier (10x rate for `macos-latest`/`macos-14`). Formula: $2,000 / 10 = 200 \text{ minutes}$.
3. **Verify Apple Developer Policy:**
   * Verify Apple Developer Documentation regarding Personal Team free provisioning (7-day validity, 3 active apps limit, 10 App IDs window).
4. **Verify Windows Sideloading Tools:**
   * Test Sideloadly or AltServer on a Windows machine with an unsigned `.ipa` binary to confirm free Apple ID certificate generation and deployment.
