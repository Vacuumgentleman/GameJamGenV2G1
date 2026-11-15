using System.Collections.Generic;
using System.Text;
using GameJamG1.Interaction;
using GameJamG1.Story;
using UnityEditor;
using UnityEngine;

namespace GameJamG1.EditorTools
{
    internal static class InspectorSummary
    {
        public static string FlagNames(SerializedProperty list)
        {
            if (list == null || !list.isArray || list.arraySize == 0)
            {
                return null;
            }

            var names = new List<string>();
            for (var i = 0; i < list.arraySize; i++)
            {
                if (list.GetArrayElementAtIndex(i).objectReferenceValue is StoryFlag flag)
                {
                    names.Add(flag.name);
                }
            }

            return names.Count > 0 ? string.Join(", ", names) : null;
        }

        public static string AssetName(SerializedProperty property)
        {
            return property != null && property.objectReferenceValue != null ? property.objectReferenceValue.name : null;
        }

        public static int EventCalls(SerializedProperty unityEvent)
        {
            var calls = unityEvent?.FindPropertyRelative("m_PersistentCalls.m_Calls");
            return calls != null ? calls.arraySize : 0;
        }
    }

    [CustomEditor(typeof(StoryTrigger))]
    public sealed class StoryTriggerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox(BuildSummary(), MessageType.None);
            DrawDefaultInspector();

            if (Application.isPlaying && GUILayout.Button("Probar: Trigger()"))
            {
                foreach (var t in targets)
                {
                    ((StoryTrigger)t).Trigger();
                }
            }
        }

        private string BuildSummary()
        {
            var activations = new List<string>();

            if (serializedObject.FindProperty("triggerOnZoneEnter").boolValue)
            {
                activations.Add("el jugador entra a la zona");
            }

            if (serializedObject.FindProperty("triggerOnFlagChanged").boolValue)
            {
                activations.Add("cambia una de las flags requeridas");
            }

            var builder = new StringBuilder();
            builder.Append("SE DISPARA: ");
            builder.AppendLine(activations.Count > 0 ? string.Join(" o ", activations) : "solo por codigo o evento (llamando a Trigger()).");

            var required = InspectorSummary.FlagNames(serializedObject.FindProperty("requiredFlags"));
            var all = serializedObject.FindProperty("mode").enumValueIndex == 0;
            builder.Append("CONDICION: ");
            builder.AppendLine(required == null ? "ninguna, siempre pasa." : $"{(all ? "todas" : "alguna")} de: {required}.");

            var dialogue = InspectorSummary.AssetName(serializedObject.FindProperty("dialogueOnTrigger"));
            var events = InspectorSummary.EventCalls(serializedObject.FindProperty("onTriggered"));
            builder.Append("RESULTADO: ");
            builder.Append(dialogue != null ? $"dialogo {dialogue}" : "sin dialogo");
            builder.Append(events > 0 ? $" y {events} evento(s)." : ", sin eventos.");
            builder.AppendLine();
            builder.Append(serializedObject.FindProperty("fireOnce").boolValue ? "Se dispara una sola vez." : "Se puede disparar varias veces.");
            return builder.ToString();
        }
    }

    [CustomEditor(typeof(InteractableBase), true)]
    [CanEditMultipleObjects]
    public sealed class InteractableBaseEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (targets.Length == 1)
            {
                EditorGUILayout.HelpBox(BuildSummary(), MessageType.None);
                DrawWarnings();
            }

            DrawDefaultInspector();
        }

        private void DrawWarnings()
        {
            var component = (Component)target;

            if (component.GetComponentInChildren<Collider>() == null)
            {
                EditorGUILayout.HelpBox("Sin collider: el jugador no podra apuntarlo.", MessageType.Warning);
            }

            if (component.GetComponent<OutlineHighlight>() == null)
            {
                EditorGUILayout.HelpBox("Sin OutlineHighlight: no se resaltara al apuntarlo.", MessageType.Info);
            }
        }

        private string BuildSummary()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"AL APUNTAR: muestra \"{serializedObject.FindProperty("promptText").stringValue}\".");

            var required = InspectorSummary.FlagNames(serializedObject.FindProperty("requiredFlags"));
            builder.AppendLine(required == null ? "REQUISITOS: ninguno." : $"REQUISITOS: necesita todas: {required}.");

            var sets = InspectorSummary.FlagNames(serializedObject.FindProperty("flagsToSetOnInteract"));
            var completion = InspectorSummary.FlagNames(serializedObject.FindProperty("completionFlags"));
            var dialogue = InspectorSummary.AssetName(serializedObject.FindProperty("interactDialogue"));
            var events = InspectorSummary.EventCalls(serializedObject.FindProperty("onInteract"));

            builder.Append("AL USARLO: ");
            builder.Append(sets != null ? $"pone {sets}; " : "no pone flags; ");
            builder.Append(completion != null ? $"al completarse pone {completion}; " : string.Empty);
            builder.Append(dialogue != null ? $"dice {dialogue}; " : "sin dialogo; ");
            builder.Append(events > 0 ? $"{events} evento(s)." : "sin eventos.");
            builder.AppendLine();

            if (serializedObject.FindProperty("hideWhenConsumed").boolValue)
            {
                builder.Append("Se usa una vez y desaparece.");
            }
            else if (serializedObject.FindProperty("oneShot").boolValue)
            {
                builder.Append("Se usa una sola vez.");
            }
            else
            {
                builder.Append("Se puede usar varias veces.");
            }

            return builder.ToString();
        }
    }
}
