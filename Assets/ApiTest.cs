using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;

[ExecuteAlways]
public class ApiTest : MonoBehaviour
{

    void OnEnable()
    {
        StartCoroutine(Test());
    }

    private IEnumerator Test()
    {
        yield return ApiUtils.ApiRequest(4, null, (s,jo) =>
        {
            var match = Regex.Match(s, @";width:-?([\d\.]+)%;");
            if (float.TryParse(match.Groups[1].ToString(), out float val))
            {
                Debug.Log(val);                
            }
        });
    }
}