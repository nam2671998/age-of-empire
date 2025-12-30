using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CommandExecutor))]
public class CommandExecutorEditor : Editor
{
    private FieldInfo currentCommandField;
    private FieldInfo commandQueueField;

    private GUIStyle currentCommandStyle;
    private GUIStyle queuedCommandStyle;

    private void OnEnable()
    {
        var executorType = typeof(CommandExecutor);
        currentCommandField = executorType.GetField("currentCommand", BindingFlags.Instance | BindingFlags.NonPublic);
        commandQueueField = executorType.GetField("commandQueue", BindingFlags.Instance | BindingFlags.NonPublic);

        currentCommandStyle = new GUIStyle(EditorStyles.label) { normal = { textColor = Color.green } };
        queuedCommandStyle = new GUIStyle(EditorStyles.label) { normal = { textColor = Color.yellow } };
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var executor = (CommandExecutor)target;
        if (executor == null)
            return;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Commands", EditorStyles.boldLabel);

        DrawCurrentCommand(executor);
        DrawQueuedCommands(executor);

        if (Application.isPlaying)
        {
            Repaint();
        }
    }

    private void DrawCurrentCommand(CommandExecutor executor)
    {
        var command = GetFieldValue<ICommand>(executor, currentCommandField);
        var description = GetCommandDescription(command);
        EditorGUILayout.LabelField("Current", description, currentCommandStyle);
    }

    private void DrawQueuedCommands(CommandExecutor executor)
    {
        var queueObj = commandQueueField?.GetValue(executor);

        if (!(queueObj is IEnumerable enumerable))
        {
            EditorGUILayout.LabelField("Queue", "Unavailable");
            return;
        }

        int index = 0;
        foreach (var item in enumerable)
        {
            var command = item as ICommand;
            var description = GetCommandDescription(command);

            if (index == 0)
            {
                EditorGUILayout.LabelField("Queue", description, queuedCommandStyle);
            }
            else
            {
                EditorGUILayout.LabelField(string.Empty, description, queuedCommandStyle);
            }

            index++;
        }

        if (index == 0)
        {
            EditorGUILayout.LabelField("Queue", "Empty", queuedCommandStyle);
        }
    }

    private static string GetCommandDescription(ICommand command)
    {
        if (command == null)
            return "None";

        try
        {
            var description = command.GetDescription();
            if (!string.IsNullOrWhiteSpace(description))
                return description;
        }
        catch (Exception)
        {
        }

        return command.GetType().Name;
    }

    private static T GetFieldValue<T>(object instance, FieldInfo field)
    {
        if (instance == null || field == null)
            return default;

        var value = field.GetValue(instance);
        if (value is T tValue)
            return tValue;

        return default;
    }
}
