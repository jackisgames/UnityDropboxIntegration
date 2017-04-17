using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Dropbox
{
    [CustomEditor(typeof(UnityEngine.Object), true)]
    public class DropboxItemEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            DropboxUnity.DropboxUI(target);
        }
    }
}