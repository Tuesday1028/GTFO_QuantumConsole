using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hikaria.QC.UI
{
    public class DraggableUI : MonoBehaviour
    {
        private RectTransform _dragRoot = null;
        private QuantumConsole _quantumConsole = null;
        private bool _lockInScreen = true;
        private ScrollRect _scrollRect = null;

        private UnityEvent _onBeginDrag = null;
        private UnityEvent _onDrag = null;
        private UnityEvent _onEndDrag = null;

        private Vector2 _lastPos = Vector2.zero;
        private bool _isDragging = false;
        private bool _isDraggingScroll = false;

        public void OnPointerDown(PointerEventData eventData)
        {
            _isDragging = _quantumConsole?.KeyConfig?.DragConsoleKey.IsHeld() ?? false;

            _pointerCurrentRaycast = eventData.pointerCurrentRaycast;

            if (_isDragging)
            {
                _onBeginDrag.Invoke();
                _lastPos = eventData.position;
                _scrollRect.enabled = false;
            }
            else
            {
                _isDraggingScroll = true;
                _scrollRect.OnBeginDrag(eventData);
            }
        }

        private RaycastResult _pointerCurrentRaycast;

        public void LateUpdate()
        {
            if (_isDragging)
            {
                Transform root = _dragRoot;
                if (!root) { root = transform as RectTransform; }

                Vector2 pos = InputHelper.GetMousePosition();
                Vector2 delta = pos - _lastPos;
                _lastPos = pos;

                if (_lockInScreen)
                {
                    Vector2 resolution = new(Screen.width, Screen.height);
                    if (pos.x <= 15 || pos.x >= resolution.x - 15) { delta.x = 0; }
                    if (pos.y <= 15 || pos.y >= resolution.y - 15) { delta.y = 0; }
                }

                root.Translate(delta);
                _onDrag.Invoke();
            }
            else if (_isDraggingScroll)
            {
                _scrollRect.OnDrag(new PointerEventData(EventSystem.current)
                {
                    button = PointerEventData.InputButton.Left,
                    position = InputHelper.GetMousePosition(),
                    pointerCurrentRaycast = _pointerCurrentRaycast
                });
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_isDragging)
            {
                _isDragging = false;
                _onEndDrag.Invoke();
                _scrollRect.enabled = true;
            }
            else
            {
                _isDraggingScroll = false;
                _scrollRect.OnEndDrag(eventData);
            }
        }

        public void OnScroll(PointerEventData eventData)
        {
            _scrollRect.OnScroll(eventData);
        }

        internal static void Setup(DraggableUI self, RectTransform containerRect, QuantumConsole quantumConsole, ScrollRect scrollRect)
        {
            self._dragRoot = containerRect;
            self._quantumConsole = quantumConsole;
            self._scrollRect = scrollRect;

            self._onBeginDrag = new();
            self._onDrag = new();
            self._onEndDrag = new();

            self._onBeginDrag.AddListener(new Action(() => { scrollRect.enabled = false; }));
            self._onDrag.AddListener(new Action(() => { }));
            self._onEndDrag.AddListener(new Action(() => { scrollRect.enabled = true; }));

            var eventTrigger = self.gameObject.AddComponent<EventTrigger>();

            var onBeginDragEntry = new EventTrigger.Entry();
            onBeginDragEntry.eventID = EventTriggerType.PointerDown;
            onBeginDragEntry.callback.AddListener(new Action<BaseEventData>((data) => { self.OnPointerDown(data.Cast<PointerEventData>());}));
            eventTrigger.triggers.Add(onBeginDragEntry);

            var onEndDragEntry = new EventTrigger.Entry();
            onEndDragEntry.eventID = EventTriggerType.PointerUp;
            onEndDragEntry.callback.AddListener(new Action<BaseEventData>((data) => { self.OnPointerUp(data.Cast<PointerEventData>());}));
            eventTrigger.triggers.Add(onEndDragEntry);

            var onScrollEntry = new EventTrigger.Entry();
            onScrollEntry.eventID = EventTriggerType.Scroll;
            onScrollEntry.callback.AddListener(new Action<BaseEventData>((data) => { self.OnScroll(data.Cast<PointerEventData>()); }));
            eventTrigger.triggers.Add(onScrollEntry);
        }
    }
}
