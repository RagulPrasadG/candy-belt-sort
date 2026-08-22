using UnityEngine;
using UnityEngine.EventSystems;

namespace CandyBeltSort
{
    public class TapInput : MonoBehaviour
    {
        public System.Action<CandyItem> OnCandyTapped;
        public System.Action<SortBox> OnBoxTapped;
        Camera _cam;

        public void SetCamera(Camera cam) => _cam = cam;

        void Update()
        {
            if (!PressedThisFrame(out var screen, out int pointerId)) return;
            if (IsPointerOverUi(pointerId)) return;
            if (_cam == null) _cam = Camera.main;
            if (_cam == null || !_cam.enabled) return;

            var ray = _cam.ScreenPointToRay(screen);
            if (!Physics.Raycast(ray, out var hit, 80f)) return;
            var candy = hit.collider.GetComponentInParent<CandyItem>();
            if (candy != null)
            {
                OnCandyTapped?.Invoke(candy);
                return;
            }

            var box = hit.collider.GetComponentInParent<SortBox>();
            if (box != null) OnBoxTapped?.Invoke(box);
        }

        static bool PressedThisFrame(out Vector3 screen, out int pointerId)
        {
            screen = Vector3.zero;
            pointerId = -1;
            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    screen = touch.position;
                    pointerId = touch.fingerId;
                    return true;
                }
                return false;
            }

            if (Input.GetMouseButtonDown(0))
            {
                screen = Input.mousePosition;
                pointerId = -1;
                return true;
            }

            return false;
        }

        static bool IsPointerOverUi(int pointerId)
        {
            var es = EventSystem.current;
            if (es == null) return false;
            try
            {
                if (pointerId >= 0) return es.IsPointerOverGameObject(pointerId);
                return es.IsPointerOverGameObject();
            }
            catch (System.Exception)
            {
                return false;
            }
        }
    }
}
