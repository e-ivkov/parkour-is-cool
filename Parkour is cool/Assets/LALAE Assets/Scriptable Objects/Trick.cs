using UnityEngine;

namespace UnityStandardAssets._2D
{
    [CreateAssetMenu(fileName = "NewTrick", menuName = "Mechanics/Trick")]
    public class Trick : ScriptableObject
    {

        public int m_XP;
        public RuntimeAnimatorController m_TrickAnimator;
        public Vector2 m_AfterPositionVector;
    }
}