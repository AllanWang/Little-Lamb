using System;
using UnityEngine;

namespace LittleLamb
{
    /// <summary>
    /// Class which registers a transform to an associated TransformVariable ScriptableObject.
    /// </summary>
    public class TransformRegister : MonoBehaviour
    {
        [SerializeField]
        TransformVariable m_TransformVariable;

        void OnEnable()
        {
            m_TransformVariable.Value = transform;
        }

        void OnDisable()
        {
            m_TransformVariable.Value = null;
        }
    }
}
