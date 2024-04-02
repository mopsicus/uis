using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIS {

    /// <summary>
    /// Load direction
    /// </summary>
    public enum ScrollerDirection {
        Top = 0,
        Bottom = 1,
        Left = 2,
        Right = 3
    }

    /// <summary>
    /// Infinite scroller
    /// </summary>
    public class Scroller : MonoBehaviour, IDropHandler {

        /// <summary>
        /// Addon count views
        /// </summary>
        const int ADDON_VIEWS_COUNT = 4;

        /// <summary>
        /// Velocity for scroll to function
        /// </summary>
        Vector2 SCROLL_VELOCITY = new Vector2(0f, 50f);

        /// <summary>
        /// Delegate for heights
        /// </summary>
        public delegate int HeightItem(int index);

        /// <summary>
        /// Event for get item height
        /// </summary>
        public event HeightItem OnHeight;

        /// <summary>
        /// Delegate for widths
        /// </summary>
        public delegate int WidthtItem(int index);

        /// <summary>
        /// Event for get item width
        /// </summary>
        public event HeightItem OnWidth;

        /// <summary>
        /// Callback on item fill
        /// </summary>
        public Action<int, GameObject> OnFill = delegate { };

        /// <summary>
        /// Callback on pull action
        /// </summary>
        public Action<ScrollerDirection> OnPull = delegate { };

        [Header("Item settings")]
        /// <summary>
        /// Item list prefab
        /// </summary>
        public GameObject Prefab = null;

        [Header("Padding")]
        /// <summary>
        /// Top padding
        /// </summary>
        public int TopPadding = 10;

        /// <summary>
        /// Bottom padding
        /// </summary>
        public int BottomPadding = 10;

        [Header("Padding")]
        /// <summary>
        /// Left padding
        /// </summary>
        public int LeftPadding = 10;

        /// <summary>
        /// Right padding
        /// </summary>
        public int RightPadding = 10;

        /// <summary>
        /// Spacing between items
        /// </summary>
        public int ItemSpacing = 10;

        [Header("Labels")]
        /// <summary>
        /// Label font asset
        /// </summary>
        public TMP_FontAsset LabelsFont = null;

        /// <summary>
        /// Label font size
        /// </summary>
        public int FontSize = 30;

        /// <summary>
        /// Label color
        /// </summary>
        public Color FontColor = Color.white;

        /// <summary>
        /// Pull top text label
        /// </summary>
        public string TopPullLabel = "Pull to refresh";

        /// <summary>
        /// Release top text label
        /// </summary>
        public string TopReleaseLabel = "Release to load";

        /// <summary>
        /// Pull bottom text label
        /// </summary>
        public string BottomPullLabel = "Pull to refresh";

        /// <summary>
        /// Release bottom text label
        /// </summary>
        public string BottomReleaseLabel = "Release to load";

        /// <summary>
        /// Pull left text label
        /// </summary>
        public string LeftPullLabel = "Pull to refresh";

        /// <summary>
        /// Release left text label
        /// </summary>
        public string LeftReleaseLabel = "Release to load";

        /// <summary>
        /// Pull right text label
        /// </summary>
        public string RightPullLabel = "Pull to refresh";

        /// <summary>
        /// Release right text label
        /// </summary>
        public string RightReleaseLabel = "Release to load";

        [Header("Directions")]
        /// <summary>
        /// Can we pull from top
        /// </summary>
        public bool IsPullTop = true;

        /// <summary>
        /// Can we pull from bottom
        /// </summary>
        public bool IsPullBottom = true;

        [Header("Directions")]
        /// <summary>
        /// Can we pull from left
        /// </summary>
        public bool IsPullLeft = true;

        /// <summary>
        /// Can we pull from right
        /// </summary>
        public bool IsPullRight = true;

        [Header("Offsets")]
        /// <summary>
        /// Coefficient when labels should action
        /// </summary>
        public float PullValue = 1.5f;

        /// <summary>
        /// Label position offset
        /// </summary>
        public float LabelOffset = 85f;

        [Header("Other")]
        /// <summary>
        /// Container for calc width/height if anchors exists
        /// </summary>
        public RectTransform ParentContainer = null;

        [HideInInspector]
        /// <summary>
        /// Top label
        /// </summary>
        public TextMeshProUGUI TopLabel = null;

        [HideInInspector]
        /// <summary>
        /// Bottom label
        /// </summary>
        public TextMeshProUGUI BottomLabel = null;

        [HideInInspector]
        /// <summary>
        /// Left label
        /// </summary>
        public TextMeshProUGUI LeftLabel = null;

        [HideInInspector]
        /// <summary>
        /// Right label
        /// </summary>
        public TextMeshProUGUI RightLabel = null;

        /// <summary>
        /// Type of scroller
        /// </summary>
        [HideInInspector]
        public int Type = 0;

        /// <summary>
        /// Scrollrect cache
        /// </summary>
        ScrollRect _scroll = null;

        /// <summary>
        /// Content rect cache
        /// </summary>
        RectTransform _content = null;

        /// <summary>
        /// Container rect cache
        /// </summary>
        Rect _container = new Rect();

        /// <summary>
        /// All rects cache
        /// </summary>
        RectTransform[] _rects = null;

        /// <summary>
        /// All objects cache
        /// </summary>
        GameObject[] _views = null;

        /// <summary>
        /// State is can we pull from top
        /// </summary>
        bool _isCanLoadUp = false;

        /// <summary>
        /// State is can we pull from bottom
        /// </summary>
        bool _isCanLoadDown = false;

        /// <summary>
        /// State is can we pull from left
        /// </summary>
        bool _isCanLoadLeft = false;

        /// <summary>
        /// State is can we pull from right
        /// </summary>
        bool _isCanLoadRight = false;

        /// <summary>
        /// Previous position
        /// </summary>
        int _previousPosition = -1;

        /// <summary>
        /// List items count
        /// </summary>
        int _count = 0;

        /// <summary>
        /// Items heights cache
        /// </summary>
        Dictionary<int, int> _heights = null;

        /// <summary>
        /// Items widths cache
        /// </summary>
        Dictionary<int, int> _widths = null;

        /// <summary>
        /// Items positions cache
        /// </summary>
        Dictionary<int, float> _positions = null;

        /// <summary>
        /// Cache for scroll position
        /// </summary>
        float _previousScrollPosition = -1;

        /// <summary>
        /// Cache with item indexes
        /// </summary>
        int[] _indexes = null;

        /// <summary>
        /// Item height or width for non-different lists
        /// </summary>
        float _offsetData = 0f;

        /// <summary>
        /// Check if items has different heights/widths
        /// </summary>
        bool _isComplexList = false;

        /// <summary>
        /// Init list flag
        /// </summary>
        bool _isInited = false;

        /// <summary>
        /// Constructor
        /// </summary>
        void Awake() {
            _container = (ParentContainer != null) ? ParentContainer.rect : GetComponent<RectTransform>().rect;
            _container.width = Mathf.Abs(_container.width);
            _container.height = Mathf.Abs(_container.height);
            _scroll = GetComponent<ScrollRect>();
            _scroll.onValueChanged.AddListener(OnScrollChange);
            _content = _scroll.viewport.transform.GetChild(0).GetComponent<RectTransform>();
            _heights = new Dictionary<int, int>();
            _widths = new Dictionary<int, int>();
            _positions = new Dictionary<int, float>();
            CreateLabels();
        }

        /// <summary>
        /// Is list has been inited
        /// </summary>
        public bool IsInited {
            get {
                return _isInited;
            }
        }

        /// <summary>
        /// Return list views count
        /// </summary>
        public int ViewsCount {
            get {
                return (_views == null) ? 0 : _views.Length;
            }
        }

        /// <summary>
        /// Current normalized position 0..1
        /// </summary>
        public float NormalizedPosition {
            get {
                return (Type == 0) ? _scroll.verticalNormalizedPosition : _scroll.horizontalNormalizedPosition;
            }
        }

        /// <summary>
        /// Main loop to check items positions and heights
        /// </summary>
        void Update() {
            if (Type == 0) {
                UpdateVertical();
            } else {
                UpdateHorizontal();
            }
        }

        /// <summary>
        /// Main loop for vertical
        /// </summary>
        void UpdateVertical() {
            if (_count == 0 || !_isInited) {
                return;
            }
            if (_isComplexList) {
                var topPosition = _content.anchoredPosition.y - ItemSpacing;
                if (topPosition <= 0f && _rects[0].anchoredPosition.y < -TopPadding) {
                    InitData(_count);
                    return;
                }
                if (topPosition < 0f) {
                    return;
                }
                if (!_positions.ContainsKey(_previousPosition) || !_heights.ContainsKey(_previousPosition)) {
                    return;
                }
                var itemPosition = Mathf.Abs(_positions[_previousPosition]) + _heights[_previousPosition];
                var position = (topPosition > itemPosition) ? _previousPosition + 1 : _previousPosition - 1;
                if (position < 0 || _scroll.velocity.y == 0.0f) {
                    return;
                }
                if (position > _previousPosition) {
                    if (position - _previousPosition > 1) {
                        position = _previousPosition + 1;
                    }
                    var newPosition = position % _views.Length;
                    newPosition--;
                    if (newPosition < 0) {
                        newPosition = _views.Length - 1;
                    }
                    var index = position + _views.Length - 1;
                    if (index < _count) {
                        var pos = _rects[newPosition].anchoredPosition;
                        pos.y = _positions[index];
                        _rects[newPosition].anchoredPosition = pos;
                        var size = _rects[newPosition].sizeDelta;
                        size.y = _heights[index];
                        _rects[newPosition].sizeDelta = size;
                        _views[newPosition].name = index.ToString();
                        OnFill(index, _views[newPosition]);
                    }
                } else {
                    if (_previousPosition - position > 1) {
                        position = _previousPosition - 1;
                    }
                    var newIndex = position % _views.Length;
                    var pos = _rects[newIndex].anchoredPosition;
                    pos.y = _positions[position];
                    _rects[newIndex].anchoredPosition = pos;
                    var size = _rects[newIndex].sizeDelta;
                    size.y = _heights[position];
                    _rects[newIndex].sizeDelta = size;
                    _views[newIndex].name = position.ToString();
                    OnFill(position, _views[newIndex]);
                }
                _previousPosition = position;
            } else {
                var topPosition = _content.anchoredPosition.y - ItemSpacing;
                var offset = Mathf.FloorToInt(topPosition / (_offsetData + ItemSpacing));
                for (var i = offset; i < offset + _views.Length; i++) {
                    var index = i % _views.Length;
                    if (i < 0 || i > _count - 1 || _rects == null || !_isInited) {
                        continue;
                    }
                    var position = _rects[index].anchoredPosition;
                    var size = _rects[index].sizeDelta;
                    position.y = _positions[i];
                    _rects[index].anchoredPosition = position;
                    if (_indexes[index] != i) {
                        _indexes[index] = i;
                        size.y = _heights[i];
                        _rects[index].sizeDelta = size;
                        _views[index].name = i.ToString();
                        OnFill(i, _views[index]);
                    }
                }
            }
        }

        /// <summary>
        /// Main loop for horizontal
        /// </summary>
        void UpdateHorizontal() {
            if (_count == 0 || !_isInited) {
                return;
            }
            if (_isComplexList) {
                var _leftPosition = _content.anchoredPosition.x * -1f - ItemSpacing;
                if (_leftPosition <= 0f && _rects[0].anchoredPosition.x < -LeftPadding) {
                    InitData(_count);
                    return;
                }
                if (_leftPosition < 0f) {
                    return;
                }
                if (!_positions.ContainsKey(_previousPosition) || !_widths.ContainsKey(_previousPosition)) {
                    return;
                }
                var itemPosition = Mathf.Abs(_positions[_previousPosition]) + _widths[_previousPosition];
                var position = (_leftPosition > itemPosition) ? _previousPosition + 1 : _previousPosition - 1;
                if (position < 0 || _scroll.velocity.x == 0.0f) {
                    return;
                }
                if (position > _previousPosition) {
                    if (position - _previousPosition > 1) {
                        position = _previousPosition + 1;
                    }
                    var newPosition = position % _views.Length;
                    newPosition--;
                    if (newPosition < 0) {
                        newPosition = _views.Length - 1;
                    }
                    var index = position + _views.Length - 1;
                    if (index < _count) {
                        var pos = _rects[newPosition].anchoredPosition;
                        pos.x = _positions[index];
                        _rects[newPosition].anchoredPosition = pos;
                        var size = _rects[newPosition].sizeDelta;
                        size.x = _widths[index];
                        _rects[newPosition].sizeDelta = size;
                        _views[newPosition].name = index.ToString();
                        OnFill(index, _views[newPosition]);
                    }
                } else {
                    if (_previousPosition - position > 1) {
                        position = _previousPosition - 1;
                    }
                    var newIndex = position % _views.Length;
                    var pos = _rects[newIndex].anchoredPosition;
                    pos.x = _positions[position];
                    _rects[newIndex].anchoredPosition = pos;
                    var size = _rects[newIndex].sizeDelta;
                    size.x = _widths[position];
                    _rects[newIndex].sizeDelta = size;
                    _views[newIndex].name = position.ToString();
                    OnFill(position, _views[newIndex]);
                }
                _previousPosition = position;
            } else {
                var _leftPosition = _content.anchoredPosition.x * -1f - ItemSpacing;
                var offset = Mathf.FloorToInt(_leftPosition / (_offsetData + ItemSpacing));
                for (var i = offset; i < offset + _views.Length; i++) {
                    var index = i % _views.Length;
                    if (i < 0 || i > _count - 1 || _rects == null || !_isInited) {
                        continue;
                    }
                    var position = _rects[index].anchoredPosition;
                    var size = _rects[index].sizeDelta;
                    position.x = _positions[i];
                    _rects[index].anchoredPosition = position;
                    if (_indexes[index] != i) {
                        _indexes[index] = i;
                        size.x = _widths[i];
                        _rects[index].sizeDelta = size;
                        _views[index].name = i.ToString();
                        OnFill(i, _views[index]);
                    }
                }
            }
        }

        /// <summary>
        /// Handler on scroller
        /// </summary>
        void OnScrollChange(Vector2 vector) {
            if (Type == 0) {
                ScrollChangeVertical();
            } else {
                ScrollChangeHorizontal();
            }
        }

        /// <summary>
        /// Handler on vertical scroll change
        /// </summary>
        void ScrollChangeVertical() {
            _isCanLoadUp = false;
            _isCanLoadDown = false;
            if (_views == null) {
                return;
            }
            var z = 0f;
            var isScrollable = _scroll.verticalNormalizedPosition != 1f && _scroll.verticalNormalizedPosition != 0f;
            var y = _content.anchoredPosition.y;
            if (isScrollable) {
                if (_scroll.verticalNormalizedPosition < 0f) {
                    z = y - _previousScrollPosition;
                } else {
                    _previousScrollPosition = y;
                }
            } else {
                z = y;
            }
            if (y < -LabelOffset && IsPullTop) {
                TopLabel.gameObject.SetActive(true);
                TopLabel.text = TopPullLabel;
                if (y < -LabelOffset * PullValue) {
                    TopLabel.text = TopReleaseLabel;
                    _isCanLoadUp = true;
                }
            } else {
                TopLabel.gameObject.SetActive(false);
            }
            if (z > LabelOffset && IsPullBottom) {
                BottomLabel.gameObject.SetActive(true);
                BottomLabel.text = BottomPullLabel;
                if (z > LabelOffset * PullValue) {
                    BottomLabel.text = BottomReleaseLabel;
                    _isCanLoadDown = true;
                }
            } else {
                BottomLabel.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Handler on horizontal scroll change
        /// </summary>
        void ScrollChangeHorizontal() {
            _isCanLoadLeft = false;
            _isCanLoadRight = false;
            if (_views == null) {
                return;
            }
            var z = 0f;
            var isScrollable = _scroll.horizontalNormalizedPosition != 1f && _scroll.horizontalNormalizedPosition != 0f;
            var x = _content.anchoredPosition.x;
            if (isScrollable) {
                if (_scroll.horizontalNormalizedPosition > 1f) {
                    z = x - _previousScrollPosition;
                } else {
                    _previousScrollPosition = x;
                }
            } else {
                z = x;
            }
            if (x > LabelOffset && IsPullLeft) {
                LeftLabel.gameObject.SetActive(true);
                LeftLabel.text = LeftPullLabel;
                if (x > LabelOffset * PullValue) {
                    LeftLabel.text = LeftReleaseLabel;
                    _isCanLoadLeft = true;
                }
            } else {
                LeftLabel.gameObject.SetActive(false);
            }
            if (z < -LabelOffset && IsPullRight) {
                RightLabel.gameObject.SetActive(true);
                RightLabel.text = RightPullLabel;
                if (z < -LabelOffset * PullValue) {
                    RightLabel.text = RightReleaseLabel;
                    _isCanLoadRight = true;
                }
            } else {
                RightLabel.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Hander on scroller drop pull
        /// </summary>
        public void OnDrop(PointerEventData eventData) {
            if (Type == 0) {
                DropVertical();
            } else {
                DropHorizontal();
            }
        }

        /// <summary>
        /// Handler on scroller vertical drop
        /// </summary>
        void DropVertical() {
            if (_isCanLoadUp) {
                OnPull(ScrollerDirection.Top);
            } else if (_isCanLoadDown) {
                OnPull(ScrollerDirection.Bottom);
            }
            _isCanLoadUp = false;
            _isCanLoadDown = false;
        }

        /// <summary>
        /// Handler on scroller horizontal drop
        /// </summary>
        void DropHorizontal() {
            if (_isCanLoadLeft) {
                OnPull(ScrollerDirection.Left);
            } else if (_isCanLoadRight) {
                OnPull(ScrollerDirection.Right);
            }
            _isCanLoadLeft = false;
            _isCanLoadRight = false;
        }

        /// <summary>
        /// Init list
        /// </summary>
        /// <param name="count">Items count</param>
        /// <param name="isOtherSide">Go to bottom or right on init</param>
        public void InitData(int count, bool isOtherSide = false) {
            if (count <= 0) {
                Debug.LogWarning("Can't init empty list!");
                return;
            }
            _isInited = true;
            if (Type == 0) {
                InitVertical(count, isOtherSide);
            } else {
                InitHorizontal(count, isOtherSide);
            }
        }

        /// <summary>
        /// Init vertical list
        /// </summary>
        /// <param name="count">Item count</param>
        /// <param name="isOtherSide">Go to bottom on init</param>
        void InitVertical(int count, bool isOtherSide = false) {
            var height = CalcSizesPositions(count);
            CreateViews();
            _previousPosition = 0;
            _count = count;
            _content.sizeDelta = new Vector2(_content.sizeDelta.x, height);
            var pos = _content.anchoredPosition;
            pos.y = isOtherSide ? height : 0f;
            _content.anchoredPosition = pos;
            for (var i = 0; i < _views.Length; i++) {
                var showed = i < count;
                _views[i].SetActive(showed);
                if (i + 1 > _count) {
                    continue;
                }
                var index = i;
                if (isOtherSide) {
                    index = (count >= _views.Length) ? count - _views.Length + i : i;
                }
                pos = _rects[i].anchoredPosition;
                pos.y = _positions[index];
                pos.x = 0f;
                _rects[i].anchoredPosition = pos;
                var size = _rects[i].sizeDelta;
                size.y = _heights[index];
                _rects[i].sizeDelta = size;
                _views[i].name = i.ToString();
                OnFill(index, _views[i]);
            }
        }

        /// <summary>
        /// Init horizontal list
        /// </summary>
        /// <param name="count">Item count</param>
        /// <param name="isOtherSide">Go to right on init</param>
        void InitHorizontal(int count, bool isOtherSide = false) {
            var width = CalcSizesPositions(count);
            CreateViews();
            _previousPosition = 0;
            _count = count;
            _content.sizeDelta = new Vector2(width, _content.sizeDelta.y);
            var pos = _content.anchoredPosition;
            pos.x = isOtherSide ? width : 0f;
            _content.anchoredPosition = pos;
            for (var i = 0; i < _views.Length; i++) {
                var showed = i < count;
                _views[i].SetActive(showed);
                if (i + 1 > _count) {
                    continue;
                }
                var index = i;
                if (isOtherSide) {
                    index = (count >= _views.Length) ? count - _views.Length + i : i;
                }
                pos = _rects[i].anchoredPosition;
                pos.x = _positions[index];
                pos.y = 0f;
                _rects[i].anchoredPosition = pos;
                var size = _rects[i].sizeDelta;
                size.x = _widths[index];
                _rects[i].sizeDelta = size;
                _views[i].name = i.ToString();
                OnFill(index, _views[i]);
            }
        }

        /// <summary>
        /// Calc all items height and positions
        /// </summary>
        /// <returns>Common content height</returns>
        float CalcSizesPositions(int count) {
            return (Type == 0) ? CalcSizesPositionsVertical(count) : CalcSizesPositionsHorizontal(count);
        }

        /// <summary>
        /// Calc all items height and positions
        /// </summary>
        /// <returns>Common content height</returns>
        float CalcSizesPositionsVertical(int count) {
            _heights.Clear();
            _positions.Clear();
            _offsetData = 0f;
            var result = 0f;
            for (var i = 0; i < count; i++) {
                _heights[i] = OnHeight(i);
                _offsetData += _heights[i];
                _positions[i] = -(TopPadding + i * ItemSpacing + result);
                result += _heights[i];
            }
            _offsetData /= count;
            _isComplexList = _offsetData != _heights[0];
            result += TopPadding + BottomPadding + (count == 0 ? 0 : ((count - 1) * ItemSpacing));
            return result;
        }

        /// <summary>
        /// Calc all items width and positions
        /// </summary>
        /// <returns>Common content width</returns>
        float CalcSizesPositionsHorizontal(int count) {
            _widths.Clear();
            _positions.Clear();
            var result = 0f;
            for (var i = 0; i < count; i++) {
                _widths[i] = OnWidth(i);
                _offsetData += _widths[i];
                _positions[i] = LeftPadding + i * ItemSpacing + result;
                result += _widths[i];
            }
            _offsetData /= count;
            _isComplexList = _offsetData != _widths[0];
            result += LeftPadding + RightPadding + (count == 0 ? 0 : ((count - 1) * ItemSpacing));
            return result;
        }

        /// <summary>
        /// Update list after load new items
        /// </summary>
        /// <param name="count">Total items count</param>
        /// <param name="newCount">Added items count</param>
        /// <param name="direction">Direction to add</param>
        public void ApplyDataTo(int count, int newCount, ScrollerDirection direction) {
            if (!_isInited) {
                return;
            }
            if (Type == 0) {
                ApplyDataToVertical(count, newCount, direction);
            } else {
                ApplyDataToHorizontal(count, newCount, direction);
            }
        }

        /// <summary>
        /// Update list after load new items for vertical scroller
        /// </summary>
        /// <param name="count">Total items count</param>
        /// <param name="newCount">Added items count</param>
        /// <param name="direction">Direction to add</param>
        void ApplyDataToVertical(int count, int newCount, ScrollerDirection direction) {
            if (_count == 0 || count <= _views.Length) {
                InitData(count);
                return;
            }
            _count = count;
            var height = CalcSizesPositions(count);
            _content.sizeDelta = new Vector2(_content.sizeDelta.x, height);
            var pos = _content.anchoredPosition;
            if (direction == ScrollerDirection.Top) {
                var y = 0f;
                for (var i = 0; i < newCount; i++) {
                    y += _heights[i] + ItemSpacing;
                }
                pos.y = y;
                _previousPosition = newCount;
            }
            _content.anchoredPosition = pos;
            var topPosition = _content.anchoredPosition.y - ItemSpacing;
            var itemPosition = Mathf.Abs(_positions[_previousPosition]) + _heights[_previousPosition];
            var position = (topPosition > itemPosition) ? _previousPosition + 1 : _previousPosition - 1;
            if (position < 0) {
                _previousPosition = 0;
                position = 1;
            }
            if (!_isComplexList) {
                for (var i = 0; i < _indexes.Length; i++) {
                    _indexes[i] = -1;
                }
            }
            for (var i = 0; i < _views.Length; i++) {
                var newIndex = position % _views.Length;
                if (newIndex < 0) {
                    continue;
                }
                _views[newIndex].SetActive(true);
                _views[newIndex].name = position.ToString();
                OnFill(position, _views[newIndex]);
                pos = _rects[newIndex].anchoredPosition;
                pos.y = _positions[position];
                _rects[newIndex].anchoredPosition = pos;
                var size = _rects[newIndex].sizeDelta;
                size.y = _heights[position];
                _rects[newIndex].sizeDelta = size;
                position++;
                if (position == _count) {
                    break;
                }
            }
        }

        /// <summary>
        /// Update list after load new items for horizontal scroller
        /// </summary>
        /// <param name="count">Total items count</param>
        /// <param name="newCount">Added items count</param>
        /// <param name="direction">Direction to add</param>
        void ApplyDataToHorizontal(int count, int newCount, ScrollerDirection direction) {
            if (_count == 0 || count <= _views.Length) {
                InitData(count);
                return;
            }
            _count = count;
            var width = CalcSizesPositions(count);
            _content.sizeDelta = new Vector2(width, _content.sizeDelta.y);
            var pos = _content.anchoredPosition;
            if (direction == ScrollerDirection.Left) {
                var x = 0f;
                for (var i = 0; i < newCount; i++) {
                    x -= _widths[i] + ItemSpacing;
                }
                pos.x = x;
                _previousPosition = newCount;
            } else {
                var w = 0f;
                for (var i = _widths.Count - 1; i >= _widths.Count - newCount; i--) {
                    w += _widths[i] + ItemSpacing;
                }
                pos.x = -width + w + _container.width;
            }
            _content.anchoredPosition = pos;
            var _leftPosition = _content.anchoredPosition.x - ItemSpacing;
            var itemPosition = Mathf.Abs(_positions[_previousPosition]) + _widths[_previousPosition];
            var position = (_leftPosition > itemPosition) ? _previousPosition + 1 : _previousPosition - 1;
            if (position < 0) {
                _previousPosition = 0;
                position = 1;
            }
            if (!_isComplexList) {
                for (var i = 0; i < _indexes.Length; i++) {
                    _indexes[i] = -1;
                }
            }
            for (var i = 0; i < _views.Length; i++) {
                var newIndex = position % _views.Length;
                if (newIndex < 0) {
                    continue;
                }
                _views[newIndex].SetActive(true);
                _views[newIndex].name = position.ToString();
                OnFill(position, _views[newIndex]);
                pos = _rects[newIndex].anchoredPosition;
                pos.x = _positions[position];
                _rects[newIndex].anchoredPosition = pos;
                var size = _rects[newIndex].sizeDelta;
                size.x = _widths[position];
                _rects[newIndex].sizeDelta = size;
                position++;
                if (position == _count) {
                    break;
                }
            }
        }

        /// <summary>
        /// Scroll to show item by index
        /// </summary>
        /// <param name="index">Item index</param>
        public void ScrollTo(int index) {
            var gap = 2;
            if (index > _count) {
                index = _count;
            } else if (index < 0) {
                index = 0;
            }
            if (index + _views.Length >= _count) {
                index = _count - _views.Length + ADDON_VIEWS_COUNT;
            }
            for (var i = 0; i < _views.Length; i++) {
                var position = (index < gap) ? index : index + i - gap;
                if (i + 1 > _count || position >= _count) {
                    continue;
                }
                var pos = _rects[i].anchoredPosition;
                pos.y = _positions[position];
                _rects[i].anchoredPosition = pos;
                var size = _rects[i].sizeDelta;
                if (Type == 0) {
                    size.y = _heights[position];
                } else {
                    size.x = _widths[position];
                }
                _rects[i].sizeDelta = size;
                _views[i].SetActive(true);
                _views[i].name = position.ToString();
                OnFill(position, _views[i]);
            }
            var offset = 0f;
            for (var i = 0; i < index; i++) {
                if (Type == 0) {
                    offset += _heights[i] + ItemSpacing;
                } else {
                    offset -= _widths[i] + ItemSpacing;
                }
            }
            _previousPosition = index - _views.Length;
            if (_previousPosition <= 0) {
                InitData(_count);
            }
            var top = _content.anchoredPosition;
            if (Type == 0) {
                top.y = offset;
            } else {
                top.x = offset;
            }
            _content.anchoredPosition = top;
            _scroll.velocity = SCROLL_VELOCITY;
        }

        /// <summary>
        /// Disable all items in list
        /// </summary>
        public void RecycleAll() {
            _count = 0;
            if (_views == null || !_isInited) {
                return;
            }
            for (var i = 0; i < _views.Length; i++) {
                _views[i].SetActive(false);
            }
        }

        /// <summary>
        /// Disable item
        /// </summary>
        /// <param name="index">Index in list data</param>
        public void Recycle(int index) {
            _count--;
            var name = index.ToString();
            if (_count == 0 || !_isInited) {
                RecycleAll();
                return;
            }
            var height = CalcSizesPositions(_count);
            for (var i = 0; i < _views.Length; i++) {
                if (string.CompareOrdinal(_views[i].name, name) == 0) {
                    _views[i].SetActive(false);
                    _content.sizeDelta = new Vector2(_content.sizeDelta.x, height);
                    UpdateVisible();
                    UpdatePositions();
                    break;
                }
            }
        }

        /// <summary>
        /// Update positions for visible items
        /// </summary>
        void UpdatePositions() {
            var pos = Vector2.zero;
            pos.y = 0f;
            for (var i = 0; i < _views.Length; i++) {
                if (i + 1 > _count) {
                    continue;
                }
                var index = int.Parse(_views[i].name);
                if (index < _count) {
                    pos = _rects[i].anchoredPosition;
                    pos.y = _positions[i];
                    pos.x = 0f;
                    _rects[i].anchoredPosition = pos;
                    var size = _rects[i].sizeDelta;
                    if (Type == 0) {
                        size.y = _heights[i];
                    } else {
                        size.x = _widths[i];
                    }
                    _rects[i].sizeDelta = size;
                }
            }
        }

        /// <summary>
        /// Update visible items with new data
        /// </summary>
        public void UpdateVisible() {
            if (!_isInited) {
                return;
            }
            for (var i = 0; i < _views.Length; i++) {
                var showed = i < _count;
                _views[i].SetActive(showed);
                if (i + 1 > _count) {
                    continue;
                }
                var index = int.Parse(_views[i].name);
                if (index < _count) {
                    OnFill(index, _views[i]);
                }
            }
        }

        /// <summary>
        /// Clear views cache
        /// Needed to recreate views after Prefab change
        /// </summary>
        /// <param name="count">Items count</param>
        public void RefreshViews(int count) {
            if (_views == null) {
                return;
            }
            _isInited = false;
            for (var i = _views.Length - 1; i >= 0; i--) {
                Destroy(_views[i]);
            }
            _rects = null;
            _views = null;
            _indexes = null;
            CalcSizesPositions(count);
            CreateViews(true);
        }

        /// <summary>
        /// Get all views in list
        /// </summary>
        /// <returns>Array of views</returns>
        public GameObject[] GetAllViews() {
            return _views;
        }

        /// <summary>
        /// Create views
        /// </summary>
        /// <param name="isForceCreate">Create views anyway</param>
        void CreateViews(bool isForceCreate = false) {
            if (Type == 0) {
                CreateViewsVertical(isForceCreate);
            } else {
                CreateViewsHorizontal(isForceCreate);
            }
        }

        /// <summary>
        /// Create view for vertical scroller
        /// </summary>
        /// <param name="isForceCreate">Create views anyway</param>
        void CreateViewsVertical(bool isForceCreate = false) {
            if (_views != null) {
                return;
            }
            var childs = _content.transform.childCount;
            if (childs > 0 && !isForceCreate) {
                _views = new GameObject[childs];
                for (var i = 0; i < childs; i++) {
                    var item = _content.transform.GetChild(i);
                    _views[i] = item.gameObject;
                }
            } else {
                GameObject clone;
                RectTransform rect;
                var height = 0;
                foreach (var item in _heights.Values) {
                    height += item + ItemSpacing;
                }
                height /= _heights.Count;
                var fillCount = Mathf.RoundToInt(_container.height / height) + ADDON_VIEWS_COUNT;
                _views = new GameObject[fillCount];
                for (var i = 0; i < fillCount; i++) {
                    clone = Instantiate(Prefab, Vector3.zero, Quaternion.identity);
                    clone.transform.SetParent(_content);
                    clone.transform.localScale = Vector3.one;
                    clone.transform.localPosition = Vector3.zero;
                    rect = clone.GetComponent<RectTransform>();
                    rect.pivot = new Vector2(0.5f, 1f);
                    rect.anchorMin = new Vector2(0f, 1f);
                    rect.anchorMax = Vector2.one;
                    rect.offsetMax = Vector2.zero;
                    rect.offsetMin = Vector2.zero;
                    _views[i] = clone;
                }
            }
            _indexes = new int[_views.Length];
            _rects = new RectTransform[_views.Length];
            for (var i = 0; i < _views.Length; i++) {
                _rects[i] = _views[i].GetComponent<RectTransform>();
            }
        }

        /// <summary>
        /// Create view for horizontal scroller
        /// </summary>
        /// <param name="isForceCreate">Create views anyway</param>
        void CreateViewsHorizontal(bool isForceCreate = false) {
            if (_views != null) {
                return;
            }
            var childs = _content.transform.childCount;
            if (childs > 0 && !isForceCreate) {
                _views = new GameObject[childs];
                for (var i = 0; i < childs; i++) {
                    var item = _content.transform.GetChild(i);
                    _views[i] = item.gameObject;
                }
            } else {
                GameObject clone;
                RectTransform rect;
                var width = 0;
                foreach (var item in _widths.Values) {
                    width += item + ItemSpacing;
                }
                width /= _widths.Count;
                var fillCount = Mathf.RoundToInt(_container.width / width) + ADDON_VIEWS_COUNT;
                _views = new GameObject[fillCount];
                for (var i = 0; i < fillCount; i++) {
                    clone = Instantiate(Prefab, Vector3.zero, Quaternion.identity);
                    clone.transform.SetParent(_content);
                    clone.transform.localScale = Vector3.one;
                    clone.transform.localPosition = Vector3.zero;
                    rect = clone.GetComponent<RectTransform>();
                    rect.pivot = new Vector2(0f, 0.5f);
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = new Vector2(0f, 1f);
                    rect.offsetMax = Vector2.zero;
                    rect.offsetMin = Vector2.zero;
                    _views[i] = clone;
                }
            }
            _indexes = new int[_views.Length];
            _rects = new RectTransform[_views.Length];
            for (var i = 0; i < _views.Length; i++) {
                _rects[i] = _views[i].GetComponent<RectTransform>();
            }
        }

        /// <summary>
        /// Create labels
        /// </summary>
        void CreateLabels() {
            if (Type == 0) {
                CreateLabelsVertical();
            } else {
                CreateLabelsHorizontal();
            }
        }

        /// <summary>
        /// Create labels for vertical scroller
        /// </summary>
        void CreateLabelsVertical() {
            var topText = new GameObject("TopLabel");
            topText.transform.SetParent(_scroll.viewport.transform);
            TopLabel = topText.AddComponent<TextMeshProUGUI>();
            TopLabel.font = LabelsFont;
            TopLabel.color = FontColor;
            TopLabel.fontSize = FontSize;
            TopLabel.transform.localScale = Vector3.one;
            TopLabel.alignment = TextAlignmentOptions.Center;
            TopLabel.text = TopPullLabel;
            var rect = TopLabel.GetComponent<RectTransform>();
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = Vector2.one;
            rect.offsetMax = Vector2.zero;
            rect.offsetMin = new Vector2(0f, -LabelOffset);
            rect.anchoredPosition3D = Vector3.zero;
            topText.SetActive(false);
            var bottomText = new GameObject("BottomLabel");
            bottomText.transform.SetParent(_scroll.viewport.transform);
            BottomLabel = bottomText.AddComponent<TextMeshProUGUI>();
            BottomLabel.font = LabelsFont;
            BottomLabel.color = FontColor;
            BottomLabel.fontSize = FontSize;
            BottomLabel.transform.localScale = Vector3.one;
            BottomLabel.alignment = TextAlignmentOptions.Center;
            BottomLabel.text = BottomPullLabel;
            BottomLabel.transform.position = Vector3.zero;
            rect = BottomLabel.GetComponent<RectTransform>();
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = new Vector2(1f, 0f);
            rect.offsetMax = new Vector2(0f, LabelOffset);
            rect.offsetMin = Vector2.zero;
            rect.anchoredPosition3D = Vector3.zero;
            bottomText.SetActive(false);
        }

        /// <summary>
        /// Create labels for horizontal scroller
        /// </summary>
        void CreateLabelsHorizontal() {
            var leftText = new GameObject("LeftLabel");
            leftText.transform.SetParent(_scroll.viewport.transform);
            LeftLabel = leftText.AddComponent<TextMeshProUGUI>();
            LeftLabel.font = LabelsFont;
            LeftLabel.color = FontColor;
            LeftLabel.fontSize = FontSize;
            LeftLabel.transform.localScale = Vector3.one;
            LeftLabel.alignment = TextAlignmentOptions.Center;
            LeftLabel.text = LeftPullLabel;
            var rect = LeftLabel.GetComponent<RectTransform>();
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = new Vector2(0f, 1f);
            rect.offsetMax = Vector2.zero;
            rect.offsetMin = new Vector2(-LabelOffset * 2, 0f);
            rect.anchoredPosition3D = Vector3.zero;
            leftText.SetActive(false);
            var rightText = new GameObject("RightLabel");
            rightText.transform.SetParent(_scroll.viewport.transform);
            RightLabel = rightText.AddComponent<TextMeshProUGUI>();
            RightLabel.font = LabelsFont;
            RightLabel.color = FontColor;
            RightLabel.fontSize = FontSize;
            RightLabel.transform.localScale = Vector3.one;
            RightLabel.alignment = TextAlignmentOptions.Center;
            RightLabel.text = RightPullLabel;
            RightLabel.transform.position = Vector3.zero;
            rect = RightLabel.GetComponent<RectTransform>();
            rect.pivot = new Vector2(1f, 0.5f);
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = Vector3.one;
            rect.offsetMax = new Vector2(LabelOffset * 2, 0f);
            rect.offsetMin = Vector2.zero;
            rect.anchoredPosition3D = Vector3.zero;
            rightText.SetActive(false);
        }
    }
}