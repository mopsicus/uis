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
			Top,
			Bottom
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

		[Header ("Directions")]
		/// <summary>
		/// Can we pull from top
		/// </summary>
		public bool IsPullTop = true;

		/// <summary>
		/// Can we pull from bottom
		/// </summary>
		public bool IsPullBottom = true;

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
			_positions = new Dictionary<int, float> ();
			CreateLabels ();
		}

		/// <summary>
		/// Main loop to check items positions and heights
		/// </summary>
		void Update () {
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
				_views[newIndex].name = position.ToString ();
				OnFill (position, _views[newIndex]);
			}
			_previousPosition = position;
		}

		/// <summary>
		/// Handler on scroller
		/// </summary>
		void OnScrollChange (Vector2 vector) {
			_isCanLoadUp = false;
			_isCanLoadDown = false;
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
		/// Hander on scroller drop pull
		/// </summary>
		public void OnDrop (PointerEventData eventData) {
			if (_isCanLoadUp) {
				OnPull (Direction.Top);
			} else if (_isCanLoadDown) {
				OnPull (Direction.Bottom);
			}
			_isCanLoadUp = false;
			_isCanLoadDown = false;
		}

		/// <summary>
		/// Init list
		/// </summary>
		/// <param name="count">Items count</param>
		public void InitData (int count) {
			float height = CalcSizesPositions (count);
			CreateViews ();
			_previousPosition = 0;
			_count = count;
			_content.sizeDelta = new Vector2 (_content.sizeDelta.x, height);
			Vector2 pos = _content.anchoredPosition;
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
				y += ItemSpacing + _heights[i];
				_views[i].name = i.ToString ();
				OnFill (i, _views[i]);
			}
		}

		/// <summary>
		/// Calc all items height and positions
		/// </summary>
		/// <returns>Common content height</returns>
		float CalcSizesPositions (int count) {
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
		/// Update list after load new items
		/// </summary>
		/// <param name="count">Total items count</param>
		/// <param name="newCount">Added items count</param>
		/// <param name="direction">Direction to add</param>
		public void ApplyDataTo (int count, int newCount, Direction direction) {
			_count = count;
			if (_count <= _views.Length) {
				AddRowToEnd ();
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
				position++;
				if (position == _count) {
					break;
				}
			}
		}

		/// <summary>
		/// Add (active) last item, if it less than container height
		/// </summary>
		public void AddRowToEnd () {
			float height = CalcSizesPositions (_count);
			_content.sizeDelta = new Vector2 (_content.sizeDelta.x, height);
			Vector2 pos = _content.anchoredPosition;
			pos.y = height - (_heights[_heights.Count - 1] + ItemSpacing) - _container.height;
			_content.anchoredPosition = pos;
			int index = _count - 1;
			for (int i = 0; i < _views.Length; i++) {
				if (!_views[i].activeSelf) {
					_views[i].gameObject.SetActive (true);
					int newIndex = index % _views.Length;
					_views[newIndex].name = index.ToString ();
					pos = _rects[newIndex].anchoredPosition;
					pos.y = _positions[newIndex];
					_rects[newIndex].anchoredPosition = pos;
					OnFill (index, _views[newIndex]);
					break;
				}
			}
		}

		/// <summary>
		/// Update list after items delete
		/// </summary>
		/// <param name="index">Index to move from</param>
		/// <param name="count">New total item count</param>
		void MoveDataTo (int index, int count) {
			_count = count;
			float height = CalcSizesPositions (_count);
			_content.sizeDelta = new Vector2 (_content.sizeDelta.x, height);
			Vector2 pos = _content.anchoredPosition;
			for (int i = 0; i < _views.Length; i++) {
				int newIndex = index % _views.Length;
				_views[newIndex].name = index.ToString ();
				if (index >= _count) {
					_views[newIndex].gameObject.SetActive (false);
				} else {
					OnFill (index, _views[newIndex]);
				}
				pos = _rects[newIndex].anchoredPosition;
				pos.y = _positions[index];
				_rects[newIndex].anchoredPosition = pos;
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
			string name = index.ToString ();
			for (int i = 0; i < _views.Length; i++) {
				if (string.CompareOrdinal (_views[i].name, name) == 0) {
					_views[i].gameObject.SetActive (false);
					_count--;
					MoveDataTo (i, _count);
					break;
				}
			}
		}

		/// <summary>
		/// Create views
		/// </summary>
		void CreateViews () {
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
			int fillCount = Mathf.RoundToInt (_container.height / height) + 3;
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
				rect.offsetMin = new Vector2 (0f, -height);
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

	}

}