using UnityEngine;

namespace UnityStandardAssets._2D
{
    [CreateAssetMenu(fileName = "NewTrick", menuName = "Mechanics/Trick")]
    public class Trick : ScriptableObject
    {

        public int m_XP;
        public Animator m_TrickAnimation;
        public float m_Timing;
        public Vector2 m_TransitionVector;

    }
}