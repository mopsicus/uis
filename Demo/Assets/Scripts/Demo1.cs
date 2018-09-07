using Mopsicus.InfiniteScroll;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Demo1 : MonoBehaviour {

	[SerializeField]
	private InfiniteScroll Scroll;

	[SerializeField]
	private int Count = 100;

	void Start () {
		Scroll.OnFill += OnFillItem;
		Scroll.OnHeight += OnHeightItem;

		Scroll.InitData (Count);
	}

	void OnFillItem (int index, GameObject item) {
		item.GetComponentInChildren<Text> ().text = index.ToString ();
	}

	int OnHeightItem (int index) {
		return 150;
	}

	public void SceneLoad (int index) {
		SceneManager.LoadScene (index);
	}

}