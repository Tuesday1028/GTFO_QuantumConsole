﻿using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Hikaria.QC.Utilities
{
    public static class SceneUtilities
    {
        public static IEnumerable<Scene> GetScenesInBuild()
        {
            int sceneCount = SceneManager.sceneCountInBuildSettings;
            for (int i = 0; i < sceneCount; i++)
            {
                yield return SceneManager.GetSceneByBuildIndex(i);
            }
        }

        public static IEnumerable<Scene> GetLoadedScenes()
        {
            int sceneCount = SceneManager.sceneCount;
            for (int i = 0; i < sceneCount; i++)
            {
                yield return SceneManager.GetSceneAt(i);
            }
        }

        public static IEnumerable<Scene> GetAllScenes()
        {
            return GetScenesInBuild();
        }

        public static IEnumerable<string> GetAllScenePaths()
        {
            int sceneCount = SceneManager.sceneCountInBuildSettings;
            for (int i = 0; i < sceneCount; i++)
            {
                yield return SceneUtility.GetScenePathByBuildIndex(i);
            }
        }

        public static IEnumerable<string> GetAllSceneNames()
        {
            return GetAllScenePaths().Select(Path.GetFileNameWithoutExtension);
        }

        public static AsyncOperation LoadSceneAsync(string sceneName, LoadSceneMode mode)
        {
#if UNITY_EDITOR
            string scenePath = sceneName;
            Scene scene = SceneManager.GetSceneByName(sceneName);

            if (scene.IsValid())
            {
                scenePath = scene.path;
            }
            else if (!Path.HasExtension(sceneName))
            {
                scenePath = GetAllScenePaths()
                    .FirstOrDefault(x => Path.GetFileNameWithoutExtension(x) == sceneName);
            }

            if (!File.Exists(scenePath))
            {
                throw new InvalidOperationException(
                    $"Cannot load scene '{sceneName}' as it is not present in the build settings or the AssetDatabase");
            }

            LoadSceneParameters parameters = new LoadSceneParameters {loadSceneMode = mode};
            return EditorSceneManager.LoadSceneAsyncInPlayMode(scenePath, parameters);
#else
            return SceneManager.LoadSceneAsync(sceneName, mode);
#endif
        }
    }
}