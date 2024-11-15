using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[ExecuteInEditMode, System.Serializable]
public class Pastel : MonoBehaviour {
	//Actually Private Vars
	private HallwayComp[] faces;
	public HallwayComp[] Faces { get { return faces; } internal set { faces = value; } }

	//Singleton
	private static Pastel instance;
	public static Pastel Instance { get{ return instance; } private set{ instance = value; } }


	//Stuff that you can actually see in editor / their public gets & sets

	[Header("Parameters")]
	[SerializeField, Range(3, 100)] private int length;
	[SerializeField, Range(3, 100)] private int width;
	[SerializeField, Range(3, 100)] private int height;
	[SerializeField, Min(1)] private float scale = 10;
	[Space]
	[SerializeField, Range(3, 100)] private float maxOffset;

	public int Length { get { return length; } set { length = value; } }
	public int Width { get { return width; } set { width = value; } }
	public int Height { get { return height; } set { height = value; } }
	public float MaxOffset { get { return maxOffset; } set { maxOffset = value; } }
	public float Scale { get{ return scale; } set{ scale = value; }}

	[Header("Faces")]
	[SerializeField] private HallwayComp left;
	[SerializeField] private HallwayComp right;
	[SerializeField] private HallwayComp close;
	[SerializeField] private HallwayComp far;
	[SerializeField] private HallwayComp ceiling;
	[SerializeField] private HallwayComp floor;

	public HallwayComp Left { get { return left; } internal set { left = value; } }
	public HallwayComp Right { get { return right; } internal set { right = value; } }
	public HallwayComp Close { get { return close; } internal set { close = value; } }
	public HallwayComp Far { get { return far; } internal set { far = value; } }
	public HallwayComp Ceiling { get { return ceiling; } internal set { ceiling = value; } }
	public HallwayComp Floor { get { return floor; } internal set { floor = value; } }

	private void OnEnable()
	{
		Instance = this;
	}

	void Start() {
		Faces = new HallwayComp[] { left, right, close, far, ceiling, floor };
	}

	void Update() {
		SetFaces();
	}

	public void SetFaces() {
		for (int i = Faces.Length - 1; i >= 0; i--) {
			HallwayComp obj = Faces[i];
			int pos;

			if (obj != null) {
				switch (obj.Face) {
					case HallFaceEnum.Left:
					case HallFaceEnum.Right:
						//Scale
						obj.gameObject.transform.localScale = new Vector3(Length, 1, Height) / Scale;

						//Position
						pos = (obj.Face == HallFaceEnum.Left) ? -1 : 1;
						obj.gameObject.transform.position = new Vector3(Width * pos, Height, Length) / 2;

						break;

					case HallFaceEnum.Close:
					case HallFaceEnum.Far:
						//Scale
						obj.gameObject.transform.localScale = new Vector3(Width, 1, Height) / Scale;

						//Position
						pos = (obj.Face == HallFaceEnum.Close) ? 0 : 2;
						obj.gameObject.transform.position = new Vector3(0, Height, Length * pos) / 2;

						break;

					case HallFaceEnum.Ceiling:
					case HallFaceEnum.Floor:
						//Scale
						obj.gameObject.transform.localScale = new Vector3(Width, 1, Length) / Scale;

						//Position
						pos = (obj.Face == HallFaceEnum.Ceiling) ? 2 : 0;
						obj.gameObject.transform.position = new Vector3(0, Height * pos, Length) / 2;

						break;
				}
			}
		}
	}
}