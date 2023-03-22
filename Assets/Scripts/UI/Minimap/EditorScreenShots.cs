using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EditorScreenShots : MonoBehaviour
{
}
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
