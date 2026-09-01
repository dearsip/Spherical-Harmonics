using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace SphericalHarmonics.Editor
{
    public static class BuildWebGL
    {
        public static void Perform()
        {
            string[] scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray();
            BuildReport report=BuildPipeline.BuildPlayer(scenes,"build/WebGL",BuildTarget.WebGL,BuildOptions.None);
            if(report.summary.result!=BuildResult.Succeeded)throw new System.Exception($"WebGL build failed: {report.summary.result}");
        }
    }
}
