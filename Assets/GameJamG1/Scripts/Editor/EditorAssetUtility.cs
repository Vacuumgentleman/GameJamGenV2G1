using GameJamG1.Dialogue;
using UnityEditor;
using UnityEngine;

namespace GameJamG1.EditorTools
{
    internal static class EditorAssetUtility
    {
        public static T FindAsset<T>(string assetName) where T : Object
        {
            foreach (var guid in AssetDatabase.FindAssets($"{assetName} t:{typeof(T).Name}"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (System.IO.Path.GetFileNameWithoutExtension(path) == assetName)
                {
                    return AssetDatabase.LoadAssetAtPath<T>(path);
                }
            }

            return null;
        }

        public static void EnsureFolder(string parent, string child)
        {
            if (!AssetDatabase.IsValidFolder($"{parent}/{child}"))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        public static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);

            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, path);
            }

            return asset;
        }

        public static DialogueSequence CreateDialogue(string folder, string assetName, bool lockPlayer, params string[] lines)
        {
            var asset = LoadOrCreate<DialogueSequence>($"{folder}/{assetName}.asset");
            var serialized = new SerializedObject(asset);
            var list = serialized.FindProperty("lines");
            list.arraySize = lines.Length;

            for (var i = 0; i < lines.Length; i++)
            {
                list.GetArrayElementAtIndex(i).FindPropertyRelative("text").stringValue = lines[i];
            }

            serialized.FindProperty("lockPlayer").boolValue = lockPlayer;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
            return asset;
        }

        public static void SetObject(Object target, string field, Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(field).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetObjects(Object target, string field, params Object[] values)
        {
            var serialized = new SerializedObject(target);
            var list = serialized.FindProperty(field);
            list.arraySize = values.Length;

            for (var i = 0; i < values.Length; i++)
            {
                list.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetBool(Object target, string field, bool value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(field).boolValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetString(Object target, string field, string value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(field).stringValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetFloat(Object target, string field, float value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(field).floatValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetInt(Object target, string field, int value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(field).intValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetEnum(Object target, string field, int index)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(field).enumValueIndex = index;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetVector3(Object target, string field, Vector3 value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(field).vector3Value = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
