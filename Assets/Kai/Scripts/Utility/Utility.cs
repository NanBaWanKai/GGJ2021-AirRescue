using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

namespace IFancing
{
    public static class Utility
    {
        #region Transform
        public static T GetExistTarget<T>(this GameObject gameObject) where T : Component
        {
            var component = gameObject.GetComponent<T>();
            if (component == null)
            {
                component = gameObject.AddComponent<T>();
            }
            return component;

        }
        public static T GetExistTarget<T>(this Transform transform) where T : Component
        {
            return transform.gameObject.GetExistTarget<T>();
        }
        public static T GetTargetInChildren<T>(this GameObject gameObject, string name) where T : Component
        {
            var components = gameObject.GetComponentsInChildren<T>(true);
            foreach (var item in components)
            {
                if (item.name.Equals(name))
                {
                    return item;
                }
            }
            return null;
        }
        public static T GetTargetInChildren<T>(this Transform transform, string name) where T : Component
        {
            return transform.gameObject.GetTargetInChildren<T>(name);
        }
        #endregion
        #region DOTween
        public static void StopAllTweener(this List<Tweener> tweenerList)
        {
            foreach (var item in tweenerList)
            {
                item.Kill();
            }
            tweenerList.Clear();
        }
        public static void PauseAllTweener(this List<Tweener> tweenerList)
        {
            foreach (var item in tweenerList)
            {
                item.Pause();
            }
        }
        public static void ResumeAllTweener(this List<Tweener> tweenerList)
        {
            foreach (var item in tweenerList)
            {
                item.Play();
            }
        }
        #endregion
        #region Coroutine
        public static void Stop(this Coroutine coroutine, MonoBehaviour mono)
        {
            if (coroutine != null)
            {
                mono.StopCoroutine(coroutine);
            }
        }
        #endregion

    }
}
