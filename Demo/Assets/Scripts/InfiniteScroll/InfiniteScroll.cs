// ----------------------------------------------------------------------------
// The MIT License
// InfiniteScroll https://github.com/mopsicus/infinite-scroll-unity
// Copyright (c) 2018 Mopsicus <mail@mopsicus.ru>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Mopsicus.InfiniteScroll {

	/// <summary>
	/// Infinite scroller for long lists
	/// </summary>
	public class InfiniteScroll : MonoBehaviour, IDropHandler {

		/// <summary>
		/// Coefficient when labels should action
		/// </summary>
		const float PULL_VALUE = 0.05f;

		/// <summary>
		/// Label position offset
		/// </summary>
		const float LABEL_OFFSET = 55f;

		/// <summary>
		/// Load direction
		/// </summary>
		public enum Direction {
			Top = 0,
			Bottom = 1,
			Left = 2,
			Right = 3
		}

		/// <summary>
		/// Delegate for heights
		/// </summary>
		public delegate int HeightItem (int index);

		/// <summary>
		/// Event for get item height
		/// </summary>
		public event HeightItem OnHeight;

		/// <summary>
		/// Delegate for widths
		/// </summary>
		public delegate int WidthtItem (int index);

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
		public Action<Direction> OnPull = delegate { };

		[Header ("Item settings")]
		/// <summary>
		/// Item list prefab
		/// </summary>
		public GameObject Prefab;

		[Header ("Padding")]
		/// <summary>
		/// Top padding
		/// </summary>
		public int TopPadding = 10;

		/// <summary>
		/// Bottom padding
		/// </summary>
		public int BottomPadding = 10;

		[Header ("Padding")]
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
		public int ItemSpacing = 2;

		[Header ("Labels")]
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

		[Header ("Directions")]
		/// <summary>
		/// Can we pull from top
		/// </summary>
		public bool IsPullTop = true;

		/// <summary>
		/// Can we pull from bottom
		/// </summary>
		public bool IsPullBottom = true;

		[Header ("Directions")]
		/// <summary>
		/// Can we pull from left
		/// </summary>
		public bool IsPullLeft = true;

		/// <summary>
		/// Can we pull from right
		/// </summary>
		public bool IsPullRight = true;

		[HideInInspector]
		/// <summary>
		/// Top label
		/// </summary>
		public Text TopLabel;

		[HideInInspector]
		/// <summary>
		/// Bottom label
		/// </summary>
		public Text BottomLabel;

		[HideInInspector]
		/// <summary>
		/// Left label
		/// </summary>
		public Text LeftLabel;

		[HideInInspector]
		/// <summary>
		/// Right label
		/// </summary>
		public Text RightLabel;

		/// <summary>
		/// Type of scroller
		/// </summary>
		[HideInInspector]
		public int Type;

		/// <summary>
		/// Scrollrect cache
		/// </summary>
		private ScrollRect _scroll;

		/// <summary>
		/// Content rect cache
		/// </summary>
		private RectTransform _content;

		/// <summary>
		/// Container rect cache
		/// </summary>
		private Rect _container;

		/// <summary>
		/// All rects cache
		/// </summary>
		private RectTransform[] _rects;

		/// <summary>
		/// All objects cache
		/// </summary>
		private GameObject[] _views;

		/// <summary>
		/// State is can we pull from top
		/// </summary>
		private bool _isCanLoadUp;

		/// <summary>
		/// State is can we pull from bottom
		/// </summary>
		private bool _isCanLoadDown;

		/// <summary>
		/// State is can we pull from left
		/// </summary>
		private bool _isCanLoadLeft;

		/// <summary>
		/// State is can we pull from right
		/// </summary>
		private bool _isCanLoadRight;

		/// <summary>
		/// Previous position
		/// </summary>
		private int _previousPosition = -1;

		/// <summary>
		/// List items count
		/// </summary>
		private int _count;

		/// <summary>
		/// Items heights cache
		/// </summary>
		private Dictionary<int, int> _heights;

		/// <summary>
		/// Items widths cache
		/// </summary>
		private Dictionary<int, int> _widths;

		/// <summary>
		/// Items positions cache
		/// </summary>
		private Dictionary<int, float> _positions;

		/// <summary>
		/// Constructor
		/// </summary>
		void Awake () {
			_container = GetComponent<RectTransform> ().rect;
			_scroll = GetComponent<ScrollRect> ();
			_scroll.onValueChanged.AddListener (OnScrollChange);
			_content = _scroll.viewport.transform.GetChild (0).GetComponent<RectTransform> ();
			_heights = new Dictionary<int, int> ();
			_widths = new Dictionary<int, int> ();
			_positions = new Dictionary<int, float> ();
			CreateLabels ();
		}

		/// <summary>
		/// Main loop to check items positions and heights
		/// </summary>
		void Update () {
			if (Type == 0) {
				UpdateVertical ();
			} else {
				UpdateHorizontal ();
			}
		}

		/// <summary>
		/// Main loop for vertical
		/// </summary>
		void UpdateVertical () {
			if (_count == 0) {
				return;
			}
			float _topPosition = _content.anchoredPosition.y - ItemSpacing;
			if (_topPosition <= 0f && _rects[0].anchoredPosition.y < -TopPadding - 10f) {
				InitData (_count);
				return;
			}
			if (_topPosition < 0f) {
				return;
			}
			float itemPosition = Mathf.Abs (_positions[_previousPosition]) + _heights[_previousPosition];
			int position = (_topPosition > itemPosition) ? _previousPosition + 1 : _previousPosition - 1;
			if (position < 0 || _previousPosition == position || _scroll.velocity.y == 0f) {
				return;
			}
			if (position > _previousPosition) {
				if (position - _previousPosition > 1) {
					position = _previousPosition + 1;
				}
				int newPosition = position % _views.Length;
				newPosition--;
				if (newPosition < 0) {
					newPosition = _views.Length - 1;
				}
				int index = position + _views.Length - 1;
				if (index < _count) {
					Vector2 pos = _rects[newPosition].anchoredPosition;
					pos.y = _positions[index];
					_rects[newPosition].anchoredPosition = pos;
					Vector2 size = _rects[newPosition].sizeDelta;
					size.y = _heights[index];
					_rects[newPosition].sizeDelta = size;
					_views[newPosition].name = index.ToString ();
					OnFill (index, _views[newPosition]);
				}
			} else {
				if (_previousPosition - position > 1) {
					position = _previousPosition - 1;
				}
				int newIndex = position % _views.Length;
				Vector2 pos = _rects[newIndex].anchoredPosition;
				pos.y = _positions[position];
				_rects[newIndex].anchoredPosition = pos;
				Vector2 size = _rects[newIndex].sizeDelta;
				size.y = _heights[position];
				_rects[newIndex].sizeDelta = size;
				_views[newIndex].name = position.ToString ();
				OnFill (position, _views[newIndex]);
			}
			_previousPosition = position;
		}

		/// <summary>
		/// Main loop for horizontal
		/// </summary>
		void UpdateHorizontal () {
			if (_count == 0) {
				return;
			}
			float _leftPosition = _content.anchoredPosition.x * -1f - ItemSpacing;
			if (_leftPosition <= 0f && _rects[0].anchoredPosition.x < -LeftPadding - 10f) {
				InitData (_count);
				return;
			}
			if (_leftPosition < 0f) {
				return;
			}
			float itemPosition = Mathf.Abs (_positions[_previousPosition]) + _widths[_previousPosition];
			int position = (_leftPosition > itemPosition) ? _previousPosition + 1 : _previousPosition - 1;
			if (position < 0 || _previousPosition == position || _scroll.velocity.x == 0f) {
				return;
			}
			if (position > _previousPosition) {
				if (position - _previousPosition > 1) {
					position = _previousPosition + 1;
				}
				int newPosition = position % _views.Length;
				newPosition--;
				if (newPosition < 0) {
					newPosition = _views.Length - 1;
				}
				int index = position + _views.Length - 1;
				if (index < _count) {
					Vector2 pos = _rects[newPosition].anchoredPosition;
					pos.x = _positions[index];
					_rects[newPosition].anchoredPosition = pos;
					Vector2 size = _rects[newPosition].sizeDelta;
					size.x = _widths[index];
					_rects[newPosition].sizeDelta = size;
					_views[newPosition].name = index.ToString ();
					OnFill (index, _views[newPosition]);
				}
			} else {
				if (_previousPosition - position > 1) {
					position = _previousPosition - 1;
				}
				int newIndex = position % _views.Length;
				Vector2 pos = _rects[newIndex].anchoredPosition;
				pos.x = _positions[position];
				_rects[newIndex].anchoredPosition = pos;
				Vector2 size = _rects[newIndex].sizeDelta;
				size.x = _widths[position];
				_rects[newIndex].sizeDelta = size;
				_views[newIndex].name = position.ToString ();
				OnFill (position, _views[newIndex]);
			}
			_previousPosition = position;
		}

		/// <summary>
		/// Handler on scroller
		/// </summary>
		void OnScrollChange (Vector2 vector) {
			if (Type == 0) {
				ScrollChangeVertical (vector);
			} else {
				ScrollChangeHorizontal (vector);
			}
		}

		/// <summary>
		/// Handler on vertical scroll change
		/// </summary>
		void ScrollChangeVertical (Vector2 vector) {
			_isCanLoadUp = false;
			_isCanLoadDown = false;
			if (_views == null) {
				return;
			}
			float y = 0f;
			float z = 0f;
			float coef = _count / _views.Length;
			if (coef >= 1f) {
				if (vector.y > 1f) {
					y = (vector.y - 1f) * coef;
				} else if (vector.y < 0f) {
					y = vector.y * coef;
				}
			} else {
				z = _content.anchoredPosition.y;
			}
			if ((y > PULL_VALUE || z < -LABEL_OFFSET / 2f) && IsPullTop) {
				TopLabel.gameObject.SetActive (true);
				TopLabel.text = TopPullLabel;
				if (y > PULL_VALUE * 2 || z < -LABEL_OFFSET) {
					TopLabel.text = TopReleaseLabel;
					_isCanLoadUp = true;
				}
			} else {
				TopLabel.gameObject.SetActive (false);
			}
			if ((y < -PULL_VALUE || z > LABEL_OFFSET / 2f) && IsPullBottom) {
				BottomLabel.gameObject.SetActive (true);
				BottomLabel.text = BottomPullLabel;
				if (y < -PULL_VALUE * 2 || z > LABEL_OFFSET) {
					BottomLabel.text = BottomReleaseLabel;
					_isCanLoadDown = true;
				}
			} else {
				BottomLabel.gameObject.SetActive (false);
			}
		}

		/// <summary>
		/// Handler on horizontal scroll change
		/// </summary>
		void ScrollChangeHorizontal (Vector2 vector) {
			_isCanLoadLeft = false;
			_isCanLoadRight = false;
			if (_views == null) {
				return;
			}
			float x = 0f;
			float z = 0f;
			float coef = _count / _views.Length;
			if (coef >= 1f) {
				if (vector.x > 1f) {
					x = (vector.x - 1f) * coef;
				} else if (vector.x < 0f) {
					x = vector.x * coef;
				}
			} else {
				z = _content.anchoredPosition.x;
			}
			if ((x > PULL_VALUE || z < -LABEL_OFFSET / 2f) && IsPullRight) {
				RightLabel.gameObject.SetActive (true);
				RightLabel.text = RightPullLabel;
				if (x > PULL_VALUE * 2 || z < -LABEL_OFFSET) {
					RightLabel.text = RightReleaseLabel;
					_isCanLoadRight = true;
				}
			} else {
				RightLabel.gameObject.SetActive (false);
			}
			if ((x < -PULL_VALUE || z > LABEL_OFFSET / 2f) && IsPullLeft) {
				LeftLabel.gameObject.SetActive (true);
				LeftLabel.text = LeftPullLabel;
				if (x < -PULL_VALUE * 2 || z > LABEL_OFFSET) {
					LeftLabel.text = LeftReleaseLabel;
					_isCanLoadLeft = true;
				}
			} else {
				LeftLabel.gameObject.SetActive (false);
			}
		}

		/// <summary>
		/// Hander on scroller drop pull
		/// </summary>
		public void OnDrop (PointerEventData eventData) {
			if (Type == 0) {
				DropVertical ();
			} else {
				DropHorizontal ();
			}
		}

		/// <summary>
		/// Handler on scroller vertical drop
		/// </summary>
		void DropVertical () {
			if (_isCanLoadUp) {
				OnPull (Direction.Top);
			} else if (_isCanLoadDown) {
				OnPull (Direction.Bottom);
			}
			_isCanLoadUp = false;
			_isCanLoadDown = false;
		}

		/// <summary>
		/// Handler on scroller horizontal drop
		/// </summary>
		void DropHorizontal () {
			if (_isCanLoadLeft) {
				OnPull (Direction.Left);
			} else if (_isCanLoadRight) {
				OnPull (Direction.Right);
			}
			_isCanLoadLeft = false;
			_isCanLoadRight = false;
		}

		/// <summary>
		/// Init list
		/// </summary>
		/// <param name="count">Items count</param>
		public void InitData (int count) {
			if (Type == 0) {
				InitVertical (count);
			} else {
				InitHorizontal (count);
			}
		}

		/// <summary>
		/// Init vertical list
		/// </summary>
		/// <param name="count">Item count</param>
		void InitVertical (int count) {
			float height = CalcSizesPositions (count);
			CreateViews ();
			_previousPosition = 0;
			_count = count;
			_content.sizeDelta = new Vector2 (_content.sizeDelta.x, height);
			Vector2 pos = _content.anchoredPosition;
			Vector2 size = Vector2.zero;
			pos.y = 0f;
			_content.anchoredPosition = pos;
			int y = TopPadding;
			bool showed = false;
			for (int i = 0; i < _views.Length; i++) {
				showed = i < count;
				_views[i].gameObject.SetActive (showed);
				if (i + 1 > _count) {
					continue;
				}
				pos = _rects[i].anchoredPosition;
				pos.y = _positions[i];
				pos.x = 0f;
				_rects[i].anchoredPosition = pos;
				size = _rects[i].sizeDelta;
				size.y = _heights[i];
				_rects[i].sizeDelta = size;
				y += ItemSpacing + _heights[i];
				_views[i].name = i.ToString ();
				OnFill (i, _views[i]);
			}
		}

		/// <summary>
		/// Init horizontal list
		/// </summary>
		/// <param name="count">Item count</param>
		void InitHorizontal (int count) {
			float width = CalcSizesPositions (count);
			CreateViews ();
			_previousPosition = 0;
			_count = count;
			_content.sizeDelta = new Vector2 (width, _content.sizeDelta.y);
			Vector2 pos = _content.anchoredPosition;
			Vector2 size = Vector2.zero;
			pos.x = 0f;
			_content.anchoredPosition = pos;
			int x = LeftPadding;
			bool showed = false;
			for (int i = 0; i < _views.Length; i++) {
				showed = i < count;
				_views[i].gameObject.SetActive (showed);
				if (i + 1 > _count) {
					continue;
				}
				pos = _rects[i].anchoredPosition;
				pos.x = _positions[i];
				pos.y = 0f;
				_rects[i].anchoredPosition = pos;
				size = _rects[i].sizeDelta;
				size.x = _widths[i];
				_rects[i].sizeDelta = size;
				x += ItemSpacing + _widths[i];
				_views[i].name = i.ToString ();
				OnFill (i, _views[i]);
			}
		}

		/// <summary>
		/// Calc all items height and positions
		/// </summary>
		/// <returns>Common content height</returns>
		float CalcSizesPositions (int count) {
			return (Type == 0) ? CalcSizesPositionsVertical (count) : CalcSizesPositionsHorizontal (count);
		}

		/// <summary>
		/// Calc all items height and positions
		/// </summary>
		/// <returns>Common content height</returns>
		float CalcSizesPositionsVertical (int count) {
			_heights.Clear ();
			_positions.Clear ();
			float result = 0f;
			for (int i = 0; i < count; i++) {
				_heights[i] = OnHeight (i);
				_positions[i] = -(TopPadding + i * ItemSpacing + result);
				result += _heights[i];
			}
			result += TopPadding + BottomPadding + (count == 0 ? 0 : ((count - 1) * ItemSpacing));
			return result;
		}

		/// <summary>
		/// Calc all items width and positions
		/// </summary>
		/// <returns>Common content width</returns>
		float CalcSizesPositionsHorizontal (int count) {
			_widths.Clear ();
			_positions.Clear ();
			float result = 0f;
			for (int i = 0; i < count; i++) {
				_widths[i] = OnWidth (i);
				_positions[i] = LeftPadding + i * ItemSpacing + result;
				result += _widths[i];
			}
			result += LeftPadding + RightPadding + (count == 0 ? 0 : ((count - 1) * ItemSpacing));
			return result;
		}

		/// <summary>
		/// Update list after load new items
		/// </summary>
		/// <param name="count">Total items count</param>
		/// <param name="newCount">Added items count</param>
		/// <param name="direction">Direction to add</param>
		public void ApplyDataTo (int count, int newCount, Direction direction) {
			if (Type == 0) {
				ApplyDataToVertical (count, newCount, direction);
			} else {
				ApplyDataToHorizontal (count, newCount, direction);
			}
		}

		/// <summary>
		/// Update list after load new items for vertical scroller
		/// </summary>
		/// <param name="count">Total items count</param>
		/// <param name="newCount">Added items count</param>
		/// <param name="direction">Direction to add</param>
		void ApplyDataToVertical (int count, int newCount, Direction direction) {
			_count = count;
			if (_count <= _views.Length) {
				InitData (count);
				return;
			}
			float height = CalcSizesPositions (count);
			_content.sizeDelta = new Vector2 (_content.sizeDelta.x, height);
			Vector2 pos = _content.anchoredPosition;
			if (direction == Direction.Top) {
				float y = 0f;
				for (int i = 0; i < newCount; i++) {
					y += _heights[i] + ItemSpacing;
				}
				pos.y = y;
				_previousPosition = newCount;
			} else {
				float h = 0f;
				for (int i = _heights.Count - 1; i >= _heights.Count - newCount; i--) {
					h += _heights[i] + ItemSpacing;
				}
				pos.y = height - h - _container.height;
			}
			_content.anchoredPosition = pos;
			float _topPosition = _content.anchoredPosition.y - ItemSpacing;
			float itemPosition = Mathf.Abs (_positions[_previousPosition]) + _heights[_previousPosition];
			int position = (_topPosition > itemPosition) ? _previousPosition + 1 : _previousPosition - 1;
			for (int i = 0; i < _views.Length; i++) {
				int newIndex = position % _views.Length;
				_views[newIndex].name = position.ToString ();
				OnFill (position, _views[newIndex]);
				pos = _rects[newIndex].anchoredPosition;
				pos.y = _positions[position];
				_rects[newIndex].anchoredPosition = pos;
				Vector2 size = _rects[newIndex].sizeDelta;
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
		void ApplyDataToHorizontal (int count, int newCount, Direction direction) {
			_count = count;
			if (_count <= _views.Length) {
				InitData (count);
				return;
			}
			float width = CalcSizesPositions (count);
			_content.sizeDelta = new Vector2 (width, _content.sizeDelta.y);
			Vector2 pos = _content.anchoredPosition;
			if (direction == Direction.Left) {
				float x = 0f;
				for (int i = 0; i < newCount; i++) {
					x -= _widths[i] + ItemSpacing;
				}
				pos.x = x;
				_previousPosition = newCount;
			} else {
				float w = 0f;
				for (int i = _widths.Count - 1; i >= _widths.Count - newCount; i--) {
					w += _widths[i] + ItemSpacing;
				}
				pos.x = -width + w + _container.width;
			}
			_content.anchoredPosition = pos;
			float _leftPosition = _content.anchoredPosition.x - ItemSpacing;
			float itemPosition = Mathf.Abs (_positions[_previousPosition]) + _widths[_previousPosition];
			int position = (_leftPosition > itemPosition) ? _previousPosition + 1 : _previousPosition - 1;
			for (int i = 0; i < _views.Length; i++) {
				int newIndex = position % _views.Length;
				_views[newIndex].name = position.ToString ();
				OnFill (position, _views[newIndex]);
				pos = _rects[newIndex].anchoredPosition;
				pos.x = _positions[position];
				_rects[newIndex].anchoredPosition = pos;
				Vector2 size = _rects[newIndex].sizeDelta;
				size.x = _widths[position];
				_rects[newIndex].sizeDelta = size;
				position++;
				if (position == _count) {
					break;
				}
			}
		}

		/// <summary>
		/// Update list after items delete
		/// </summary>
		/// <param name="index">Index to move from</param>
		/// <param name="height">New height</param>
		void MoveDataTo (int index, float height) {
			if (Type == 0) {
				MoveDataToVertical (index, height);
			} else {
				MoveDataToHorizontal (index, height);
			}
		}

		/// <summary>
		/// Update list after items delete for vertical scroller
		/// </summary>
		/// <param name="index">Index to move from</param>
		/// <param name="height">New height</param>
		void MoveDataToVertical (int index, float height) {
			_content.sizeDelta = new Vector2 (_content.sizeDelta.x, height);
			Vector2 pos = _content.anchoredPosition;
			for (int i = 0; i < _views.Length; i++) {
				int newIndex = index % _views.Length;
				_views[newIndex].name = index.ToString ();
				if (index >= _count) {
					_views[newIndex].gameObject.SetActive (false);
					continue;
				} else {
					_views[newIndex].gameObject.SetActive (true);
					OnFill (index, _views[newIndex]);
				}
				pos = _rects[newIndex].anchoredPosition;
				pos.y = _positions[index];
				_rects[newIndex].anchoredPosition = pos;
				Vector2 size = _rects[newIndex].sizeDelta;
				size.y = _heights[index];
				_rects[newIndex].sizeDelta = size;
				index++;
			}
		}

		/// <summary>
		/// Update list after items delete for horizontal scroller
		/// </summary>
		/// <param name="index">Index to move from</param>
		/// <param name="width">New width</param>
		void MoveDataToHorizontal (int index, float width) {
			_content.sizeDelta = new Vector2 (width, _content.sizeDelta.y);
			Vector2 pos = _content.anchoredPosition;
			for (int i = 0; i < _views.Length; i++) {
				int newIndex = index % _views.Length;
				_views[newIndex].name = index.ToString ();
				if (index >= _count) {
					_views[newIndex].gameObject.SetActive (false);
					continue;
				} else {
					_views[newIndex].gameObject.SetActive (true);
					OnFill (index, _views[newIndex]);
				}
				pos = _rects[newIndex].anchoredPosition;
				pos.x = _positions[index];
				_rects[newIndex].anchoredPosition = pos;
				Vector2 size = _rects[newIndex].sizeDelta;
				size.x = _widths[index];
				_rects[newIndex].sizeDelta = size;
				index++;
			}
		}

		/// <summary>
		/// Disable all items in list
		/// </summary>
		public void RecycleAll () {
			_count = 0;
			if (_views == null) {
				return;
			}
			for (int i = 0; i < _views.Length; i++) {
				_views[i].gameObject.SetActive (false);
			}
		}

		/// <summary>
		/// Disable item
		/// </summary>
		/// <param name="index">Index in list data</param>
		public void Recycle (int index) {
			_count--;
			string name = index.ToString ();
			float height = CalcSizesPositions (_count);
			for (int i = 0; i < _views.Length; i++) {
				if (string.CompareOrdinal (_views[i].name, name) == 0) {
					_views[i].gameObject.SetActive (false);
					MoveDataTo (i, height);
					break;
				}
			}
		}

		/// <summary>
		/// Update visible items with new data
		/// </summary>
		public void UpdateVisible () {
			bool showed = false;
			for (int i = 0; i < _views.Length; i++) {
				showed = i < _count;
				_views[i].gameObject.SetActive (showed);
				if (i + 1 > _count) {
					continue;
				}
				int index = int.Parse (_views[i].name);
				OnFill (index, _views[i]);
			}
		}

		/// <summary>
		/// Clear views cache
		/// Needed to recreate views after Prefab change
		/// </summary>
		public void RefreshViews () {
			if (_views == null) {
				return;
			}
			for (int i = 0; i < _views.Length; i++) {
				Destroy (_views[i].gameObject);
			}
			_rects = null;
			_views = null;
			CreateViews ();
		}

		/// <summary>
		/// Create views
		/// </summary>
		void CreateViews () {
			if (Type == 0) {
				CreateViewsVertical ();
			} else {
				CreateViewsHorizontal ();
			}
		}

		/// <summary>
		/// Create view for vertical scroller
		/// </summary>
		void CreateViewsVertical () {
			if (_views != null) {
				return;
			}
			GameObject clone;
			RectTransform rect;
			int height = 0;
			foreach (int item in _heights.Values) {
				height += item;
			}
			height = height / _heights.Count;
			int fillCount = Mathf.RoundToInt (_container.height / height) + 4;
			_views = new GameObject[fillCount];
			for (int i = 0; i < fillCount; i++) {
				clone = (GameObject) Instantiate (Prefab, Vector3.zero, Quaternion.identity);
				clone.transform.SetParent (_content);
				clone.transform.localScale = Vector3.one;
				clone.transform.localPosition = Vector3.zero;
				rect = clone.GetComponent<RectTransform> ();
				rect.pivot = new Vector2 (0.5f, 1f);
				rect.anchorMin = new Vector2 (0f, 1f);
				rect.anchorMax = Vector2.one;
				rect.offsetMax = Vector2.zero;
				rect.offsetMin = Vector2.zero;
				_views[i] = clone;
			}
			_rects = new RectTransform[_views.Length];
			for (int i = 0; i < _views.Length; i++) {
				_rects[i] = _views[i].gameObject.GetComponent<RectTransform> ();
			}
		}

		/// <summary>
		/// Create view for horizontal scroller
		/// </summary>
		void CreateViewsHorizontal () {
			if (_views != null) {
				return;
			}
			GameObject clone;
			RectTransform rect;
			int width = 0;
			foreach (int item in _widths.Values) {
				width += item;
			}
			width = width / _widths.Count;
			int fillCount = Mathf.RoundToInt (_container.width / width) + 4;
			_views = new GameObject[fillCount];
			for (int i = 0; i < fillCount; i++) {
				clone = (GameObject) Instantiate (Prefab, Vector3.zero, Quaternion.identity);
				clone.transform.SetParent (_content);
				clone.transform.localScale = Vector3.one;
				clone.transform.localPosition = Vector3.zero;
				rect = clone.GetComponent<RectTransform> ();
				rect.pivot = new Vector2 (0f, 0.5f);
				rect.anchorMin = Vector2.zero;
				rect.anchorMax = new Vector2 (0f, 1f);
				rect.offsetMax = Vector2.zero;
				rect.offsetMin = Vector2.zero;
				_views[i] = clone;
			}
			_rects = new RectTransform[_views.Length];
			for (int i = 0; i < _views.Length; i++) {
				_rects[i] = _views[i].gameObject.GetComponent<RectTransform> ();
			}
		}

		/// <summary>
		/// Create labels
		/// </summary>
		void CreateLabels () {
			if (Type == 0) {
				CreateLabelsVertical ();
			} else {
				CreateLabelsHorizontal ();
			}
		}

		/// <summary>
		/// Create labels for vertical scroller
		/// </summary>
		void CreateLabelsVertical () {
			GameObject topText = new GameObject ("TopLabel");
			topText.transform.SetParent (_scroll.viewport.transform);
			TopLabel = topText.AddComponent<Text> ();
			TopLabel.font = Resources.GetBuiltinResource<Font> ("Arial.ttf");
			TopLabel.fontSize = 24;
			TopLabel.transform.localScale = Vector3.one;
			TopLabel.alignment = TextAnchor.MiddleCenter;
			TopLabel.text = TopPullLabel;
			RectTransform rect = TopLabel.GetComponent<RectTransform> ();
			rect.pivot = new Vector2 (0.5f, 1f);
			rect.anchorMin = new Vector2 (0f, 1f);
			rect.anchorMax = Vector2.one;
			rect.offsetMax = Vector2.zero;
			rect.offsetMin = new Vector2 (0f, -LABEL_OFFSET);
			rect.anchoredPosition3D = Vector3.zero;
			topText.SetActive (false);
			GameObject bottomText = new GameObject ("BottomLabel");
			bottomText.transform.SetParent (_scroll.viewport.transform);
			BottomLabel = bottomText.AddComponent<Text> ();
			BottomLabel.font = Resources.GetBuiltinResource<Font> ("Arial.ttf");
			BottomLabel.fontSize = 24;
			BottomLabel.transform.localScale = Vector3.one;
			BottomLabel.alignment = TextAnchor.MiddleCenter;
			BottomLabel.text = BottomPullLabel;
			BottomLabel.transform.position = Vector3.zero;
			rect = BottomLabel.GetComponent<RectTransform> ();
			rect.pivot = new Vector2 (0.5f, 0f);
			rect.anchorMin = Vector2.zero;
			rect.anchorMax = new Vector2 (1f, 0f);
			rect.offsetMax = new Vector2 (0f, LABEL_OFFSET);
			rect.offsetMin = Vector2.zero;
			rect.anchoredPosition3D = Vector3.zero;
			bottomText.SetActive (false);
		}

		/// <summary>
		/// Create labels for horizontal scroller
		/// </summary>
		void CreateLabelsHorizontal () {
			GameObject leftText = new GameObject ("LeftLabel");
			leftText.transform.SetParent (_scroll.viewport.transform);
			LeftLabel = leftText.AddComponent<Text> ();
			LeftLabel.font = Resources.GetBuiltinResource<Font> ("Arial.ttf");
			LeftLabel.fontSize = 24;
			LeftLabel.transform.localScale = Vector3.one;
			LeftLabel.alignment = TextAnchor.MiddleCenter;
			LeftLabel.text = LeftPullLabel;
			RectTransform rect = LeftLabel.GetComponent<RectTransform> ();
			rect.pivot = new Vector2 (0f, 0.5f);
			rect.anchorMin = Vector2.zero;
			rect.anchorMax = new Vector2 (0f, 1f);
			rect.offsetMax = Vector2.zero;
			rect.offsetMin = new Vector2 (-LABEL_OFFSET * 2, 0f);
			rect.anchoredPosition3D = Vector3.zero;
			leftText.SetActive (false);
			GameObject rightText = new GameObject ("RightLabel");
			rightText.transform.SetParent (_scroll.viewport.transform);
			RightLabel = rightText.AddComponent<Text> ();
			RightLabel.font = Resources.GetBuiltinResource<Font> ("Arial.ttf");
			RightLabel.fontSize = 24;
			RightLabel.transform.localScale = Vector3.one;
			RightLabel.alignment = TextAnchor.MiddleCenter;
			RightLabel.text = RightPullLabel;
			RightLabel.transform.position = Vector3.zero;
			rect = RightLabel.GetComponent<RectTransform> ();
			rect.pivot = new Vector2 (1f, 0.5f);
			rect.anchorMin = new Vector2 (1f, 0f);
			rect.anchorMax = Vector3.one;
			rect.offsetMax = new Vector2 (LABEL_OFFSET * 2, 0f);
			rect.offsetMin = Vector2.zero;
			rect.anchoredPosition3D = Vector3.zero;
			rightText.SetActive (false);
		}

	}

}