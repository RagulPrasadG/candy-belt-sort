using UnityEngine;
using UnityEngine.EventSystems;

namespace CandyBeltSort
{
    public class TapInput : MonoBehaviour
    {
        public System.Action<CandyItem> OnCandyTapped;
        Camera _cam;

        public void SetCamera(Camera cam) => _cam = cam;

        void Update()
        {
            if (!PressedThisFrame(out var screen)) return;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
            if (_cam == null) _cam = Camera.main;
            if (_cam == null) return;

            var ray = _cam.ScreenPointToRay(screen);
            if (!Physics.Raycast(ray, out var hit, 80f)) return;
            var candy = hit.collider.GetComponentInParent<CandyItem>();
            if (candy != null) OnCandyTapped?.Invoke(candy);
        }

        static bool PressedThisFrame(out Vector3 screen)
        {
            screen = Vector3.zero;
            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    screen = touch.position;
                    return true;
                }
                return false;
            }

            if (Input.GetMouseButtonDown(0))
            {
                screen = Input.mousePosition;
                return true;
            }

            return false;
        }
    }
}
