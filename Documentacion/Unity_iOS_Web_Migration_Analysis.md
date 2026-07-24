# Project ViMARA: Unity iOS Export & WebAR Migration Technical Synthesis Report

**Metadata:**
- **Proyecto:** ViMARA (Visualizador de Maquetas de Arquitectura en Realidad Aumentada)
- **Entorno:** Unity 6 (6000.3.10f1), AR Foundation 6.3.3, GLTFast, Three.js r160, MindAR.js v1.2
- **Estado:** Informe Técnico de Síntesis Arquitectónica

---

## Executive Summary

1. **Compilación iOS desde Windows (Ruta $0):** Los desarrolladores en Windows no pueden generar binarios iOS (`.ipa`) localmente por el requerimiento de Xcode en macOS. Sin embargo, **NO es obligatorio pagar los $99/año de Apple** para pruebas en dispositivos físicos. Las pruebas físicas en iPhone/iPad se realizan a costo **$0** mediante una **Apple ID Gratuita** combinada con builders CI/CD en la nube (Codemagic o GitHub Actions `game-ci`) y motores de sideloading en Windows (Sideloadly / AltServer).
2. **Ganador CI/CD:** **Codemagic** ofrece **500 minutos M1 Mac/mes gratis** (~33–40 builds/mes), superando a GitHub Actions (200 min macOS/mes tras el multiplicador 10x = ~14 builds/mes) y Unity Cloud Build (120 min/mes = ~5–8 builds/mes).
3. **Veredicto de Reescritura para WebAR:** **SÍ — Migrar ViMARA a un stack WebAR nativo requiere una reescritura del 100% de la lógica y código C#.** Cero código C#, escenas `.unity`, plantillas UI Toolkit (`.uxml`/`.uss`), shaders HLSL o prefabs de Unity pueden reutilizarse directamente en la web.
4. **Inviabilidad de Unity WebGL en AR Móvil:** La compilación Unity WebGL (C# $\rightarrow$ IL2CPP $\rightarrow$ Emscripten $\rightarrow$ WASM) genera paquetes WASM masivos (22–70 MB) con tiempos de carga de 25–55 s en 4G. En iOS Safari, la asignación WASM y texturas desbordan el límite de memoria RAM por pestaña (1.0–1.4 GB), causando cierres brutales (*"This webpage was reloaded because a problem occurred"*). Safari tampoco soporta WebXR `immersive-ar`.
5. **Modelo Híbrido Recomendado:**
   - **Tier 1 (Aplicación Principal):** Nativa móvil con **Unity 6 + AR Foundation 6.3.3**, 60 FPS, detección de planos, carga dinámica de `.glb` vía GLTFast.
   - **Tier 2 (Vista Previa Web):** Vista previa ligera con Google **`<model-viewer>`** (planos) y **MindAR.js + Three.js** (marcadores).

---

# Part 1: Exporting & Compiling Unity Projects to iOS from Windows

```
                                  ┌────────────────────────────────────────────────────────┐
                                  │             WINDOWS DEV PC (UNITY 6 EDITOR)            │
                                  └───────────────────────────┬────────────────────────────┘
                                                              │
                                                        git push / CLI
                                                              │
        ┌────────────────────────────┬────────────────────────┴────────────┬────────────────────────────┐
        ▼                            ▼                                     ▼                            ▼
┌───────────────┐           ┌───────────────────┐                 ┌──────────────────┐         ┌──────────────────┐
┌   METHOD 1    │           │     METHOD 2      │                 │     METHOD 3     │         │     METHOD 4     │
│ Unity DevOps  │           │  GitHub Actions   │                 │ Codemagic CI/CD  │         │  Cloud / Local   │
│ Cloud Build   │           │ (game-ci/builder) │                 │ (M1 Mac Runners) │         │ macOS Virtual VM │
└───────┬───────┘           └─────────┬─────────┘                 └────────┬─────────┘         └────────┬─────────┘
        │                             │                                    │                            │
        │ 120 free min/mo             │ 200 free macOS min/mo              │ 500 free M1 min/mo         │ ~$1/hr o AWS 24h │
        │ (~5-8 builds)               │ (~14 builds)                       │ (~33-40 builds)            │ regla EULA       │
        ▼                             ▼                                    ▼                            ▼
┌─────────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│                                       DESCARGA DE BINARIO .IPA NO FIRMADO                                       │
└────────────────────────────────────────────────────────┬────────────────────────────────────────────────────────┘
                                                         │
                                               Sideloadly / AltServer
                                           Free Apple ID (Personal Team)
                                                         │
                                                         ▼
                                          ┌──────────────────────────────┐
                                          │    IPHONE / IPAD FÍSICO      │
                                          └──────────────────────────────┘
```

---

## 1. Métodos de Compilación remota

### Método 1: Unity Cloud Build / DevOps
- **Unity Personal (Gratis):** **120 minutos de build/mes**. Overages ~$0.04/min. 1 build concurrente.
- **Capacidad:** Builds fríos en Unity 6 AR tomar ~15-22 min (IL2CPP + AssetBundles). Permite **~5 a 8 builds/mes gratis**.

---

### Método 2: GitHub Actions con `game-ci/unity-builder`
- **Multiplicador macOS:** GitHub regala 2,000 min Linux en repos privados. En runners macOS aplica un **multiplicador de 10x**:
  $$\text{Minutos macOS} = \frac{2,000}{10} = \mathbf{200 \text{ minutos/mes}} \implies \left\lfloor \frac{200}{14} \right\rfloor = \mathbf{14 \text{ builds/mes}}$$

#### Producción `.github/workflows/build-ios.yml`:

```yaml
name: ViMARA iOS Cloud Build (Windows Workflow)

on:
  workflow_dispatch:
  push:
    branches: [ main, develop ]

jobs:
  build-ios:
    name: Build & Archive iOS IPA 🍏
    runs-on: macos-14
    steps:
      - name: Checkout Codebase
        uses: actions/checkout@v4
        with:
          lfs: true

      - name: Cache Unity Library Directory
        uses: actions/cache@v4
        with:
          path: Library
          key: Library-iOS-${{ hashFiles('Assets/**', 'Packages/**', 'ProjectSettings/**') }}
          restore-keys: |
            Library-iOS-

      - name: Unity Builder (Generate Xcode Project)
        uses: game-ci/unity-builder@v4
        env:
          UNITY_LICENSE: ${{ secrets.UNITY_LICENSE }}
          UNITY_EMAIL: ${{ secrets.UNITY_EMAIL }}
          UNITY_PASSWORD: ${{ secrets.UNITY_PASSWORD }}
        with:
          targetPlatform: iOS
          unityVersion: 6000.3.10f1
          buildName: ViMARA_iOS

      - name: Configure iOS Code Signing Credentials
        env:
          BUILD_CERTIFICATE_BASE64: ${{ secrets.BUILD_CERTIFICATE_BASE64 }}
          P12_PASSWORD: ${{ secrets.P12_PASSWORD }}
          BUILD_PROVISION_PROFILE_BASE64: ${{ secrets.BUILD_PROVISION_PROFILE_BASE64 }}
          KEYCHAIN_PASSWORD: ${{ secrets.KEYCHAIN_PASSWORD }}
        run: |
          CERT_PATH=$RUNNER_TEMP/build_certificate.p12
          PP_PATH=$RUNNER_TEMP/build_pp.mobileprovision
          KEYCHAIN_PATH=$RUNNER_TEMP/app-signing.keychain-db

          echo -n "$BUILD_CERTIFICATE_BASE64" | base64 --decode -o $CERT_PATH
          echo -n "$BUILD_PROVISION_PROFILE_BASE64" | base64 --decode -o $PP_PATH

          security create-keychain -p "$KEYCHAIN_PASSWORD" $KEYCHAIN_PATH
          security set-keychain-settings -lut 3600 $KEYCHAIN_PATH
          security unlock-keychain -p "$KEYCHAIN_PASSWORD" $KEYCHAIN_PATH
          security import $CERT_PATH -k $KEYCHAIN_PATH -P "$P12_PASSWORD" -A -T /usr/bin/codesign

          mkdir -p ~/Library/MobileDevice/Provisioning\ Profiles
          cp $PP_PATH ~/Library/MobileDevice/Provisioning\ Profiles/

      - name: Compile Xcode Project to Signed IPA
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

      - name: Upload IPA Artifact
        uses: actions/upload-artifact@v4
        with:
          name: ViMARA-iOS-v${{ github.run_number }}
          path: ${{ runner.temp }}/output_ipa/*.ipa
```

---

### Método 3: Servicios CI/CD Móviles de Terceros
- **Codemagic (Ganador):** **500 minutos M1 Mac/mes gratis** (~12-15 min/build M1 = **~33 a 40 builds/mes gratis**).
- **Bitrise:** 300 min/mes. Límite estricto de **30 min por build** (falla con IL2CPP).
- **Appcircle:** 200 min/mes (~12-15 builds/mes).

---

### Método 4: VMs macOS en la Nube y Virtualización Local
- **MacInCloud:** Remote desktop RDP/SSH. ~$1.00/hora (depósito $30) o $20–$45/mes.
- **AWS EC2 Mac (`mac1.metal` / `mac2.metal`):** **Regla de 24 horas obligatoria de Apple EULA**. Mínimo billing por asignación: $15.60 a $28.80 por build. Inviable.
- **VMs Locales (VMware/Proxmox en Windows):** Violación de EULA. **Falta de aceleración Metal GPU** (cuelga compilador de shaders Xcode y Unity Editor).

---

## 2. Firma de Aplicaciones y Sideloading

### Comparativa: Apple ID Gratuita vs. Cuenta de Pago ($99/año)

| Característica | Personal Apple ID (Gratuita) | Paid Apple Developer ($99/año) |
| :--- | :--- | :--- |
| **Costo Anual** | **$0.00 / año** | **$99.00 USD / año** |
| **Expiración Perfil** | **7 Días** (requiere re-firmar) | **1 Año (365 Días)** |
| **Límite App IDs** | **10 App IDs** en ventana de 7 días | Ilimitado |
| **Apps Activas por Dispositivo** | **3 apps activas** | Ilimitado |
| **Pruebas Físicas USB/Wi-Fi** | **Soportado ($0)** | **Soportado** |
| **TestFlight / App Store** | ❌ No Soportado | ✅ Soportado |

---

### Sideloading desde Windows

| Herramienta | Método de Firma | Refresh Inalámbrico | Complejidad Setup |
| :--- | :--- | :--- | :--- |
| **1. Sideloadly** | Free ID al vuelo | Sí (Wi-Fi) | ⭐ Muy Baja (Drag & Drop) |
| **2. AltServer** | Free ID al vuelo | Sí (Background) | ⭐⭐ Baja (requiere iTunes) |
| **3. 3uTools** | Firma integrada | No (Solo USB) | ⭐ Muy Baja |
| **4. Apple Devices**| Perfil Ad-Hoc pre-firmado | No (Solo USB) | ⭐⭐ App Oficial |

---

## 3. Matriz Comparativa Resumen: Windows a iOS

| Método | Setup | Costo Mensual | Velocidad | Free Apple ID | Paid Apple ID | Despliegue | Uso Ideal |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: | :--- |
| **Unity Cloud Build** | 2/5 | $0 (120 min) | 3/5 (~18m) | ⚠️ Manual | ✅ Directo | 2/5 | Proyectos Unity Dashboard |
| **GitHub Actions (`game-ci`)** | 3/5 | **$0** (200 macOS min) | 4/5 (~14m) | ✅ Sideloadly | ✅ Secrets | 2/5 | **Mejor Pipeline CI/CD Git** |
| **Codemagic CI/CD** | **1/5** | **$0** (500 M1 min) | **5/5 (~10m M1)**| ✅ Sideloadly | ✅ API Keys | **1/5** | **Mejor Opción Turnkey** |
| **MacInCloud** | 2/5 | ~$1/hr | 3/5 | ✅ Xcode UI | ✅ Xcode UI | 1/5 | Debugging visual |
| **AWS EC2 Mac** | 5/5 | ~$15–$28 min/run | 4/5 | ✅ Xcode UI | ✅ Xcode UI | 2/5 | Enterprise únicamente |
| **VM macOS Local** | 5/5 | $0 | 1/5 (Sin Metal) | ⚠️ Inestable | ⚠️ Inestable | 4/5 | **NO RECOMENDADO** |

---

# Part 2: Unity to WebAR Migration Analysis

## 1. Veredicto sobre Reescritura de Código

### **SÍ**
**Migrar ViMARA de Unity (C#) a un stack WebAR nativo exige una reescritura del 100% del código de aplicación, lógica de UI y sistemas.**

Solo se salvan assets 3D binarios (`.gltf`/`.glb`), texturas (`.png`/`.jpg`) y fórmulas matemáticas conceptuales. Cero código C#, archivos de escena `.unity`, UI Toolkit (`.uxml`/`.uss`), shaders HLSL o prefabs pueden convertirse automáticamente.

---

## 2. Comparativa Técnica y Salvabilidad

### 2.1 Lenguajes y Runtimes

| Métrica | Unity C# Stack | Native WebAR (JavaScript/TypeScript) |
| :--- | :--- | :--- |
| **Lenguaje** | C# (.NET 8 / C# 12) | JavaScript (ES2023+) / TypeScript (v5+) |
| **Runtime** | Mono JIT / IL2CPP AOT C++ Native | V8 (Chrome) / JavaScriptCore (Safari) JIT |
| **Memoria** | Managed GC (Boehm / Incremental) | Generational V8 / JSC GC |
| **Concurrencia** | Multihilo (Job System, Burst, Threads) | Monohilo Event Loop, Web Workers |

---

### 2.2 Arquitectura C# vs Three.js

#### MonoBehaviour de Unity:
```csharp
using UnityEngine;

public class ModelRotator : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 30.0f;
    private void Update() {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }
}
```

#### Render Loop en Three.js:
```javascript
import * as THREE from 'three';

export class ModelRotator {
    constructor(threeObject, rotationSpeed = 30.0) {
        this.object = threeObject;
        this.rotationSpeed = THREE.MathUtils.degToRad(rotationSpeed);
    }
    update(deltaTime) {
        if (this.object) this.object.rotation.y += this.rotationSpeed * deltaTime;
    }
}

// En el loop de renderizado:
const rotator = new ModelRotator(loadedMesh, 30.0);
const clock = new THREE.Clock();
renderer.setAnimationLoop(() => {
    rotator.update(clock.getDelta());
    renderer.render(scene, camera);
});
```

---

### 2.3 Matriz de Salvabilidad de Assets

| Categoría Asset | ¿Reutilizable en WebAR? | Estado | Acción Requerida |
| :--- | :---: | :---: | :--- |
| **Modelos 3D (`.glb`)** | **SÍ** | **100% Salvable** | Carga directa en Three.js `GLTFLoader`. |
| **Texturas (`.png`, `.jpg`)**| **SÍ** | **100% Salvable** | Carga directa en Three.js `TextureLoader`. |
| **Animaciones Embedded** | **SÍ** | **100% Salvable** | Parseo en Three.js `AnimationMixer`. |
| **Audio (`.wav`, `.mp3`)** | **SÍ** | **100% Salvable** | Reproducción con Web Audio API o `<audio>`. |
| **Fórmulas Matemáticas** | **SÍ (Conceptual)** | **Conceptual** | Reescritura línea por línea a JS/TS. |
| **Código C# (`.cs`)** | **NO** | **0% Salvable** | Reescritura total a JS/TS. |
| **Escenas Unity (`.unity`)** | **NO** | **0% Salvable** | Reconstrucción programática en Three.js. |
| **Prefabs (`.prefab`)** | **NO** | **0% Salvable** | Convertir a archivos `.glb` o Three.js `Group`. |
| **Shaders HLSL** | **NO** | **0% Salvable** | Reescribir a GLSL (`ShaderMaterial`) o `MeshStandardMaterial`. |
| **Plantillas UI (`.uxml`)** | **NO** | **0% Salvable** | Reescribir en HTML5 y CSS3 nativos. |
| **Paquetes UPM** | **NO** | **0% Salvable** | Reemplazar por Web APIs (`WebXR`, `MindAR`, HTML `<input>`). |

---

## 3. Cuellos de Botella de Unity WebGL en Móvil

```
┌───────────────────────────────────────────────────────────────────────────────────┐
│               BUNDLE SIZE & MOBILE LOAD TIME COMPARISON (4G NETWORK)               │
├────────────────────────────────────┬──────────────────────────────────────────────┤
│ UNITY WEBGL EXPORT                 │ NATIVE WEBAR (THREE.JS / MINDAR)             │
├────────────────────────────────────┼──────────────────────────────────────────────┤
│ WASM Engine Core: 12 MB - 25 MB    │ JS Framework Bundle: 600 KB - 1.5 MB         │
│ JS Glue + Data:    5 MB - 15 MB    │ Application Code:    100 KB - 300 KB         │
│ 3D Model Assets:   5 MB - 30 MB    │ 3D Model Assets:     5 MB - 30 MB            │
│ TOTAL DOWNLOAD:   22 MB - 70 MB    │ TOTAL DOWNLOAD:      5.7 MB - 31.8 MB        │
│                                    │                                              │
│ Cold Load Time (4G: 20 Mbps):      │ Cold Load Time (4G: 20 Mbps):                │
│ 25.0s - 55.0 segundos               │ 1.5s - 4.5 segundos                           │
└────────────────────────────────────┴──────────────────────────────────────────────┘
```

1. **Límite RAM iOS Safari & Crashes OOM:** Techo por pestaña de ~1.0–1.4 GB. La pila WASM + texturas 3D descompresas exceden el límite, provocando la recarga forzada del navegador.
2. **Falta de WebXR en iOS Safari:** Safari no soporta `immersive-ar` nativo. Plugins como WebXR Export fallan en iOS Safari.
3. **Estrangulamiento Térmico:** Compilación WASM intensa produce calentamiento de SoC móvil y caída de FPS de 60 a 15-20 FPS tras 3 min.

---

## 4. Matriz Comparativa: Unity WebGL vs Native WebAR

| Dimensión | Unity WebGL Export | Native Three.js WebAR Stack |
| :--- | :--- | :--- |
| **Lenguaje** | C# | JavaScript / TypeScript |
| **Reuso de Código** | 100% (dentro de Unity) | **0% (Reescritura total)** |
| **AR en iOS Safari** | ❌ **Inviable** (Sin WebXR) | ✅ **Funcional** (`<model-viewer>` / MindAR) |
| **AR en Android Chrome**| ⚠️ Parcial (Plugin WebXR) | ✅ **Funcional** (`immersive-ar` nativo) |
| **Peso Engine** | ❌ **15 MB – 40 MB+** | ✅ **< 1.5 MB** |
| **Carga en 4G** | ❌ **25 a 55 segundos** | ✅ **1.5 a 4.0 segundos** |
| **Pico Memoria RAM** | ❌ **800 MB – 1.5 GB+** | ✅ **150 MB – 350 MB** |
| **Costo Licencias ($0)**| ⚠️ Plugins de pago $150+/mes| ✅ **100% Libre (MIT / Apache 2.0)** |
| **Viabilidad Producción**| ❌ **INVIABLE EN AR MÓVIL** | ✅ **VIABLE PARA WEB PREVIEW** |

---

# Part 3: Strategic Recommendations for ViMARA

1. **Pipeline iOS desde Windows ($0 Costo):**
   - Desarrollo local en Unity 6 en Windows.
   - Compilación automatizada en **Codemagic** (500 min M1 gratis/mes).
   - Sideloading en iPhone/iPad con **Sideloadly** y Personal Apple ID Gratuita (renovación 7 días).
2. **Modelo Arquitectónico Híbrido:**
   - **Tier 1 (App Nativa Móvil Principal):** Unity 6 + AR Foundation 6.3.3 + GLTFast + UI Toolkit. Garantiza 60 FPS, seguimiento robusto de planos y marcadores dinámicos offline.
   - **Tier 2 (Módulo Web Secundario):** Google `<model-viewer>` (planos) y MindAR.js + Three.js (marcadores) para previsualización web ultraligera sin instalar apps.
