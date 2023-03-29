using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class EditorScreenShots : MonoBehaviour
{
}

#if UNITY_EDITOR
[CustomEditor(typeof(EditorScreenShots))]
public class EditorScreenShotsEditor : Editor
{
    public string textureName = "Minimap_";
    static int counter;
 
    public override void OnInspectorGUI()
    {
        textureName = EditorGUILayout.TextField("Name:", textureName);
        if(GUILayout.Button(new GUIContent("Capture", "Use capture button to take a capture - Set on Game window the texture size")))
        {
            Screenshot();
        }
    }
 
    //[MenuItem("Screenshot/Take screenshot")]
    void Screenshot()
    {
        ScreenCapture.CaptureScreenshot(Application.dataPath + "/Screenshots/" + textureName + counter + ".png");
        counter++;
    }
}
#endif
