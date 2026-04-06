#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
static class ScriptCreator
{

    [MenuItem("Assets/êVÇµÇ¢MonoçÏê¨ %#m", false, 1)] // Ctrl + Shift + M  ( %#m = Ctrl/Cmd + Shift + M )
    static void CreateCustomScript01()
    {

        string template = Path.Combine(Application.dataPath, "01_Scripts/Utility/MonoBehaviourTemplate.cs.txt");
        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(template, "NewCustomMonoBehaviour.cs");

    }


    [MenuItem("Assets/êVÇµÇ¢ManagerçÏê¨", false, 1)]
    static void CreateCustomScript02()
    {
        string template = Path.Combine(Application.dataPath, "01_Scripts/Utility/SingletonTemplate.cs.txt");
        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(template, "NewCustomMonoBehaviour.cs");
    }



}
#endif



