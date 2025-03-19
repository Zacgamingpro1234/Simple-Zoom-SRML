using UnityEngine;
using System.Collections;

namespace SimpleZoom
{
    public class CoroutineHelper : MonoBehaviour
    {
        private static CoroutineHelper _instance;

        public static CoroutineHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    // Create a new GameObject to host the CoroutineHelper
                    GameObject obj = new GameObject("CoroutineHelper");
                    _instance = obj.AddComponent<CoroutineHelper>();
                    DontDestroyOnLoad(obj); // Ensure it persists across scenes
                }
                return _instance;
            }
        }

        public Coroutine StartCoroutineHelper(IEnumerator routine)
        {
            return StartCoroutine(routine);
        }

        public void StopCoroutineHelper(Coroutine routine)
        {
            StopCoroutine(routine);
        }
    }
}