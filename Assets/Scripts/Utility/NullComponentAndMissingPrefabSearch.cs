#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[ExecuteAlways]
public class NullComponentAndMissingPrefabSearch : MonoBehaviour
{
    [Tooltip("click to log all missing/null components and prefabs in this hierarchy")]
    public bool search = false;

    private List<string> results = new List<string>();
    private void AppendComponentResult(string childPath, int index)
    {
        results.Add("Missing Component " + index + " of " + childPath);
    }
    private void AppendTransformResult(string childPath, string name)
    {
        results.Add("Missing Prefab for \"" + name + "\" of " + childPath);
    }
    public void Search()
    {
        results.Clear();
        Traverse(transform);
        Debug.Log("> Total Results: " + results.Count);
        foreach (string result in results) Debug.Log("> " + result);
    }
    private void Update()
    {
        if (search)
        {
            Search();
            search = false;
        }
    }

    private void Traverse(Transform transform, string path = "")
    {
        string thisPath = path + "/" + transform.name;
        Component[] components = transform.GetComponents<Component>();
        for (int i = 0; i < components.Length; i++)
        {
            if (components[i] == null) AppendComponentResult(thisPath, i);
        }
        for (int c = 0; c < transform.childCount; c++)
        {
            Transform t = transform.GetChild(c);
            PrefabAssetType pt = PrefabUtility.GetPrefabAssetType(t.gameObject);
            if (pt == PrefabAssetType.MissingAsset)
            {
                AppendTransformResult(path + "/" + transform.name, t.name);
            }
            else
            {
                Traverse(t, thisPath);
            }
        }
    }
}
#endif