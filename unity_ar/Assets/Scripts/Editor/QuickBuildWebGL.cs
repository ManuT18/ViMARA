using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ViMARA.Editor
{
    /// <summary>
    /// Herramienta de compilación optimizada para ViMARA WebAR.
    /// Exporta directamente a public/unity_ar/ sin abrir ventanas del Explorador de archivos.
    /// </summary>
    public static class QuickBuildWebGL
    {
        private const string ScenePath = "Assets/Scenes/WebAR_InstantTracking.unity";
        private const string OutputRelativePath = "../public/unity_ar";

        [MenuItem("ViMARA/⚡ Compilar WebGL Rápido (Silencioso) %#b", priority = 1)]
        public static void BuildWebGLFast()
        {
            // Hereda el modo de desarrollo si está activo en Build Settings, o usa modo rápido
            BuildOptions options = BuildOptions.None;
            if (EditorUserBuildSettings.development)
            {
                options |= BuildOptions.Development;
            }

            PerformBuild(options, "Rápida");
        }

        [MenuItem("ViMARA/📦 Compilar WebGL Release (Producción)", priority = 2)]
        public static void BuildWebGLRelease()
        {
            PerformBuild(BuildOptions.None, "Release / Producción");
        }

        private static void PerformBuild(BuildOptions options, string buildType)
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string outputPath = Path.GetFullPath(Path.Combine(projectRoot, OutputRelativePath));

            Debug.Log($"[ViMARA Build] Iniciando compilación WebGL ({buildType}) hacia: {outputPath}");

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = outputPath,
                target = BuildTarget.WebGL,
                options = options
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"<color=#4ade80><b>[ViMARA Build] ¡Compilación WebGL completada!</b></color> Tiempo: {summary.totalTime.TotalSeconds:F1}s | Tamaño: {summary.totalSize / (1024 * 1024)} MB.");
            }
            else if (summary.result == BuildResult.Failed)
            {
                Debug.LogError($"<color=#ef4444><b>[ViMARA Build] Error durante la compilación:</b></color> {summary.totalErrors} errores encontrados.");
            }
        }
    }
}
