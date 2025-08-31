using UnityEngine;

namespace FantasyColony.UI.Router
{
    public interface IScreen
    {
        void Enter(Transform parent);
        void Exit();
    }

    public abstract class UIScreenBase : IScreen
    {
        protected RectTransform Root;
        public abstract void Enter(Transform parent);
        public abstract void Exit();
    }
}
