using UnityEngine;
using System.Collections;

public class Carousel : MonoBehaviour {
	
	[SerializeField]
	private GameObject m_CarouselAnchor;

	[SerializeField]
	private ThumbBrowser m_ThumbBrowser;

	public Vector3 LocalPos {
		get { return m_CarouselAnchor.transform.localPosition; }
		set { m_CarouselAnchor.transform.localPosition = value; }
	}

	public Transform Transform {
		get { return m_CarouselAnchor.transform; }
	}

	// Update is called once per frame
	void Update () {
		this.transform.position = m_CarouselAnchor.transform.position;
	}
}
