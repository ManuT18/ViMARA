using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ViMARA.Editor
{
    /// <summary>
    /// Herramienta de compilación rápida para ViMARA WebAR.
    /// Exporta directamente a public/unity_ar/ con 1 solo clic o atajo de teclado,
    /// SIN abrir ventanas del Explorador de archivos de Windows.
    /// </summary>
    public static class QuickBuildWebGL
    {
        private const string ScenePath = "Assets/Scenes/WebAR_InstantTracking.unity";
        private const string OutputRelativePath = "../public/unity_ar";

        [MenuItem("ViMARA/⚡ Compilar WebGL a Web (Silencioso) %#b", priority = 1)]
        public static void BuildWebGLDirect()
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string outputPath = Path.GetFullPath(Path.Combine(projectRoot, OutputRelativePath));

            Debug.Log($"[ViMARA Build] Iniciando compilación WebGL directa hacia: {outputPath}");

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = outputPath,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"<color=#4ade80><b>[ViMARA Build] ¡Compilación WebGL completada con éxito!</b></color> Tamaño: {summary.totalSize / (1024 * 1024)} MB en {summary.totalTime.TotalSeconds:F1}s.");
            }
            else if (summary.result == BuildResult.Failed)
            {
                Debug.LogError($"<color=#ef4444><b>[ViMARA Build] Error durante la compilación:</b></color> {summary.totalErrors} errores encontrados.");
            }
        }
    }
}
