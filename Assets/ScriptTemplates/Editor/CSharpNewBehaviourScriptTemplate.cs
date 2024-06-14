using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace Trarizon.Library.Unity.ScriptTemplates
{
    public class CSharpNewBehaviourScriptTemplate : AssetModificationProcessor
    {
        private const string ScriptPath = @"Assets\Scripts";

        // Called when about to create .meta file
        public static void OnWillCreateAsset(string assetName)
        {
            var filePath = assetName.Replace(".meta", "");

            if (filePath.EndsWith(".cs"))
            {
                // Read the template already created by Unity
                var contents = File.ReadAllText(filePath)
                    .Replace(NamespacePlaceholder, Namespace(filePath));

                File.WriteAllText(filePath, contents);
                AssetDatabase.Refresh();
            }
        }

        #region Namespace
        private const string NamespacePlaceholder = "#NAMESPACE#";
        private const string MainNamespace = "Echoin";
        private static string Namespace(string filePath)
        {
            return string.Join('.', GetSplitNamespace(filePath));

            static IEnumerable<string> GetSplitNamespace(string filePath)
            {
                if (!string.IsNullOrEmpty(MainNamespace))
                    yield return MainNamespace;

                var dir = Path.GetDirectoryName(filePath);
                if (dir == null) yield break;

                var relatedPath = Path.GetRelativePath(ScriptPath, dir);
                if (relatedPath == ".") yield break; // Root

                foreach (string name in relatedPath.Split(Path.DirectorySeparatorChar))
                    yield return name.Replace(" ", "");
            }
        }
        #endregion
    }
}

