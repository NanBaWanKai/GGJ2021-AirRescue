using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IFancing
{
    public interface ISceneLoader
    {
        /// <summary>
        /// 加载关卡Level
        /// </summary>
        /// <param name="sceneName">关卡名</param>
        /// <param name="useLoading">是否使用加载场景</param>
        /// <param name="delay">加载延迟</param>
        void LoadScene(string sceneName);
        /// <summary>
        /// 当加载开始时回调
        /// </summary>
        event Action<string> OnLoadStart;
        /// <summary>
        /// 当加载结束时回调
        /// </summary>
        event Action<string> OnLoadEnd;
    }

    public class SceneLoader : MonoBehaviour, ISceneLoader
    {

        private string m_currentSceneName = null;

        public event Action<string> OnLoadStart;
        /// <summary>
        /// 当卸载关卡结束时回调
        /// </summary>
        public event Action<string> OnLoadEnd;

        #region PUBLIC
        public void LoadScene(string sceneName)
        {
            StartCoroutine(LoadSceneEnumerator(sceneName));
        }
        private IEnumerator LoadSceneEnumerator(string sceneName)
        {
            if (m_currentSceneName != null)
            {
                yield return SceneManager.UnloadSceneAsync(m_currentSceneName);
            }
            if (sceneName != null)
            {
                m_currentSceneName = sceneName;
                OnLoadStart?.Invoke(sceneName);
                yield return SceneManager.LoadSceneAsync(m_currentSceneName);
                OnLoadEnd?.Invoke(sceneName);
            }
        }
        #endregion
    }
}