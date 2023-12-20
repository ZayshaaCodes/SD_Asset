using System.Text;
using UnityEditor;
using UnityEngine;

public class Grad : MonoBehaviour
{
    public Gradient gradient;
}

[CustomEditor(typeof(Grad))]
public class GradEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        Grad grad = (Grad) target;
        if (GUILayout.Button("Print Gradient"))
        {
            StringBuilder gradientString = new StringBuilder();
            gradientString.Append("@g_gradient = Gradient({ \n");

            for (int i = 0; i < grad.gradient.colorKeys.Length; i++)
            {
                Color c = grad.gradient.colorKeys[i].color;
                float a = grad.gradient.alphaKeys[i].alpha;
                float t = grad.gradient.colorKeys[i].time;
                gradientString.Append(string.Format("\tkeyColor({0:f2}f, vec4({1:f2},{2:f2},{3:f2},{4:f2})),\n", 
                                                    t, c.r, c.g, c.b, a));
            }

            gradientString.Append("});");

            Debug.Log(gradientString.ToString());
        }

        if (GUILayout.Button("Create Gradient From Example"))
        {
            grad.gradient = CreateGradientFromExample();
        }
        
    }
    
    private Gradient CreateGradientFromExample()
    {
        Gradient gradient = new Gradient();

        GradientColorKey[] colorKeys = new GradientColorKey[6];
        colorKeys[0]                      = new GradientColorKey(new Color(0,    1, 1), 0);
        colorKeys[1]                      = new GradientColorKey(new Color(0,    1, 1), 0.6f);
        colorKeys[2]                      = new GradientColorKey(new Color(0.3f, 1, 0.3f), 0.65f);
        colorKeys[3]                      = new GradientColorKey(new Color(1,    1, 0.3f), 0.66f);
        colorKeys[4]                      = new GradientColorKey(new Color(1,    1, 0.3f), 0.8f);
        colorKeys[5]                      = new GradientColorKey(new Color(1,    0, 0), 1);
        
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[6];
        alphaKeys[0]                 = new GradientAlphaKey(0.2f, 0);
        alphaKeys[1]                 = new GradientAlphaKey(0.5f, 0.6f);
        alphaKeys[2]                 = new GradientAlphaKey(0.7f, 0.65f);
        alphaKeys[3]                 = new GradientAlphaKey(0.7f, 0.66f);
        alphaKeys[4]                 = new GradientAlphaKey(0.7f, 0.8f);
        alphaKeys[5]                 = new GradientAlphaKey(0.5f, 1);

        gradient.colorKeys = colorKeys;
        gradient.alphaKeys = alphaKeys;
        return gradient;
    }
}