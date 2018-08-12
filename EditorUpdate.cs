#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEditor;

public static class EditorUpdate
{

    public static double EditorDeltaTime => EditorApplication.timeSinceStartup - lastTimeSinceStartup;
    private static double lastTimeSinceStartup;

    private static List<Action<double>> editorActions=new List<Action<double>>();

    private static List<Action<double>> actionsToRemove = new List<Action<double>>();


    public static void EditorUpdatePlug(Action<double> action)
    {
        editorActions.Add(action);
    }

    public static void RemoveEditorUpdatePlug(Action<double> action)
    {
        actionsToRemove.Add(action);
    }

    [InitializeOnLoadMethod]
    private static void PlugUpdate()
    {
        EditorApplication.update -= UpdateLogic;
        EditorApplication.update += UpdateLogic;
        lastTimeSinceStartup = EditorApplication.timeSinceStartup;
    }

    private static void UpdateLogic()
    {
        foreach (var action in editorActions)
        {
            action(EditorDeltaTime);
        }
      
        foreach (var action in actionsToRemove)
        {
            editorActions.Remove(action);
        }

        actionsToRemove.Clear();

        lastTimeSinceStartup = EditorApplication.timeSinceStartup;
    }
}

#endif