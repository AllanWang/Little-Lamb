using System.Collections.Generic;
using UnityEngine;

namespace LittleLamb
{
    public class GameDataSource : MonoBehaviour
    {
       
        /// <summary>
        /// static accessor for all GameData.
        /// </summary>
        public static GameDataSource Instance { get; private set; }
        private void Awake()
        {
            if (Instance != null)
            {
                throw new System.Exception("Multiple GameDataSources defined!");
            }

            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
    }
}
