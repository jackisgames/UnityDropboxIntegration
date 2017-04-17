using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Dropbox
{
    internal class DropboxIntegrationWindow:EditorWindow
    {

        private DropboxAccount dropboxAccount;
        public void Init(DropboxAccount account)
        {
            dropboxAccount = account;
            titleContent = new GUIContent("Unity Dropbox Settings");
        }

        private void OnGUI()
        {
            if (dropboxAccount == null)
            {
                if (GUILayout.Button("Setuo"))
                {
                    DropboxUnity.InitializeDropBox();
                }
            }
            else
            {
                if (dropboxAccount.personal.path != null && GUILayout.Button("Personal account"))
                {
                    UpdateDropboxFolder(dropboxAccount.personal.path);
                }
                if (dropboxAccount.business.path != null && GUILayout.Button("Business account"))
                {
                    UpdateDropboxFolder(dropboxAccount.business.path);
                }
                if(GUILayout.Button("Manual setup path"))
                {
                    UpdateDropboxFolder(String.Empty);
                }
            }
        }

        private void UpdateDropboxFolder(string dropboxPath)
        {
            string path = EditorUtility.OpenFolderPanel("Select target folder", dropboxPath, Application.productName);
            EditorPrefs.SetString(DropboxConstants.UrlPref, path);
            Close();
        }
    }

    [System.Serializable]
    internal class DropboxAccount
    {
        public DropboxAccountInfo business;
        public DropboxAccountInfo personal;
    }
    [System.Serializable]
    internal class DropboxAccountInfo
    {
        public string path;
        //public string host;
    }
}
