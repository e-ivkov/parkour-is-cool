using UnityEngine;

namespace UnityStandardAssets._2D
{
    [CreateAssetMenu(fileName = "NewTrick", menuName = "Mechanics/Trick")]
    public class Trick : ScriptableObject
    {

        public int m_XP;
        public int m_index;
       // public RuntimeAnimatorController m_TrickAnimator;
        public RuntimeAnimatorController m_TrickFollowColliderAnimator;
        public Vector2 m_AfterPositionVector;
    }
}