using System.IO;
using UnityEditor;
using UnityEngine;

namespace Dropbox
{
    public class DropboxUnity
    {

        public static void UnityToDropbox(Object target)
        {
            
            CopyFile(GetUnityAssetFullPath(target), GetDropboxAssetFullPath(target));
        }

        public static void DropboxToUnity(Object target)
        {
            CopyFile(GetDropboxAssetFullPath(target), GetUnityAssetFullPath(target));
            AssetDatabase.Refresh();
        }

        public static void DeleteDropboxAsset(Object target)
        {
            string dropboxPath = GetDropboxAssetFullPath(target);
            if (File.Exists(dropboxPath))
            {
                File.Delete(dropboxPath);
                File.Delete(dropboxPath + ".meta");
            }
                
            
            File.Delete(GetUnityAssetFullPath(target));
            AssetDatabase.Refresh();
        }
        private static string GetUnityAssetFullPath(Object asset)
        {
            string assetpath = AssetDatabase.GetAssetPath(asset);

            return Path.Combine(Application.dataPath, assetpath.Substring(7));
        }

        private static string GetDropboxAssetFullPath(Object asset)
        {
            string assetpath = AssetDatabase.GetAssetPath(asset);

            return Path.Combine(EditorPrefs.GetString(DropboxConstants.UrlPref), assetpath);
        }

        private static void CopyFile(string sourceFilePath, string destinationFilePath)
        {
            string directory = Path.GetDirectoryName(destinationFilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            File.Copy(sourceFilePath, destinationFilePath, true);
            File.Copy(sourceFilePath + ".meta", destinationFilePath + ".meta", true);
        }

        private static void CopyFiles(string sourceFolderPath, string destinationFolderPath)
        {
            if (!Directory.Exists(destinationFolderPath))
            {
                Directory.CreateDirectory(destinationFolderPath);
            }

            string[] files = Directory.GetFiles(sourceFolderPath);
            for (int i = 0; i < files.Length; i++)
            {
                string fileName = files[i];
                
                File.Copy(fileName,Path.Combine(destinationFolderPath, Path.GetFileName(fileName)), true);
            }

        }

        [MenuItem("Window/Dropbox/Setup")]
        public static void InitializeDropBox()
        {

            DropboxIntegrationWindow dropboxWindow = EditorWindow.GetWindow<DropboxIntegrationWindow>();
            DropboxAccount account = new DropboxAccount();
            for (int i = 0; i < DropboxConstants.DropboxInfoJsonUrl.Length; i++)
            {
                string path = System.Environment.ExpandEnvironmentVariables(DropboxConstants.DropboxInfoJsonUrl[i]);
                if (File.Exists(path))
                {
                    
                    EditorJsonUtility.FromJsonOverwrite(File.ReadAllText(path), account);
                    dropboxWindow.Init(account);
                    return;
                }
            }
           
            dropboxWindow.Init(account);
            //EditorUtility.DisplayDialog("Error", "Dropbox app folder not found", "Ok");
        }

        [MenuItem("Window/Dropbox/Pull all files")]
        public static void PullAll()
        {
            string dropboxFolder = EditorPrefs.GetString(DropboxConstants.UrlPref);
            if (string.IsNullOrEmpty(dropboxFolder) || !Directory.Exists(dropboxFolder))
                EditorUtility.DisplayDialog("Error", "Dropbox app folder not found", "Ok");
            else
            {
                Selection.activeGameObject = null;
                CopyDropboxRecursive(dropboxFolder,dropboxFolder, Application.dataPath.Substring(0,Application.dataPath.Length-7));
                AssetDatabase.Refresh();
            }
        }

        [MenuItem("Window/Dropbox/Push All Files")]
        public static void PushAll()
        {
            AssetDatabase.SaveAssets();
            string dropboxFolder = EditorPrefs.GetString(DropboxConstants.UrlPref);
            if (string.IsNullOrEmpty(dropboxFolder) || !Directory.Exists(dropboxFolder))
                EditorUtility.DisplayDialog("Error", "Dropbox app folder not found", "Ok");
            else
            {
                string[] guids = AssetDatabase.FindAssets("*");

                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    
                    try
                    {
                        Object obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                        if ((IDropboxItem) obj != null)
                            UnityToDropbox(obj);
                    }
                    catch (System.Exception)
                    {
                        // ignored
                    }
                    EditorUtility.DisplayProgressBar("Gathering Objects",path,(float)i/guids.Length);
                }
                EditorUtility.ClearProgressBar();
            }
        }
        private static void CopyDropboxRecursive(string currentFolder,string sourceProjectPath,string targetProjectPath)
        {
            string targetFolder = targetProjectPath+currentFolder.Substring(sourceProjectPath.Length);
            CopyFiles(currentFolder,targetFolder);
            string[] directories = Directory.GetDirectories(currentFolder);
            for(int i=0;i<directories.Length;i++)
                CopyDropboxRecursive(directories[i],sourceProjectPath,targetProjectPath);
            //CopyFiles(currentFolder);

        }

        private static bool Ready()
        {
            string dropboxPath = EditorPrefs.GetString(DropboxConstants.UrlPref);
            if (string.IsNullOrEmpty(dropboxPath) || !Directory.Exists(dropboxPath))
                return false;
            else
                return true;
        }

        private static bool IsDropboxItem(Object target)
        {
            if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(target)))
                return false;

            try
            {
                IDropboxItem dropboxItem = (IDropboxItem) target;
                return dropboxItem != null;
            }
            catch (System.Exception)
            {
                return false;
            }

        }

        public static void DropboxUI(Object target)
        {
            if (IsDropboxItem(target))
            {
                if (Ready())
                {
                    GUILayout.Space(4f);
                    GUILayout.Label("Unity Dropbox");
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Push"))
                    {
                        AssetDatabase.SaveAssets();
                        UnityToDropbox(target);
                    }
                    if (GUILayout.Button("Pull"))
                    {
                        DropboxToUnity(target);
                        Selection.activeGameObject = null;
                    }
                    if (GUILayout.Button("Delete"))
                    {
                        Selection.activeGameObject = null;
                        DeleteDropboxAsset(target);
                    }
                    GUILayout.EndHorizontal();
                }
                else
                {
                    if (GUILayout.Button("Setup Dropbox"))
                        InitializeDropBox();
                }
            }
        }
    }
}
