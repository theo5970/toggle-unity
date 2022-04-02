using UnityEngine;
using UnityEngine.EventSystems;

namespace Toggle.LevelEditor
{
    public class DeselectPanel : MonoBehaviour, IPointerDownHandler
    {
        public EditorLevelSelectView view;

        public void OnPointerDown(PointerEventData eventData)
        {
            view.Deselect();
        }
    }
}
