using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using mattatz.Utils;

namespace mattatz.Triangulation2DSystem.Example {

	public class Demo : MonoBehaviour {

		[SerializeField, Range(10f, 30f)] float angle = 20f;
		[SerializeField, Range(0.2f, 2f)] float threshold = 1.5f;
		[SerializeField] GameObject prefab;
		[SerializeField] Material lineMat;
		[SerializeField] bool debug;

		List<Vector2> points;

		Camera cam;
		float depth = 0f;
		bool dragging;

		void Start () {
			cam = Camera.main;
			depth = Mathf.Abs(cam.transform.position.z - transform.position.z);
			points = new List<Vector2>();

			if(debug) {
				points = LocalStorage.LoadList<Vector2>("points.json");
				Build();
			}
		}

		void Update () {
			if(Input.GetMouseButtonDown(0)) {
				dragging = true;
				Clear();
			} else if(Input.GetMouseButtonUp(0)) {
				// LocalStorage.SaveList<Vector2>(points, "points.json");

				dragging = false;
				Build();
			}

			if(dragging) {
				var screen = Input.mousePosition;
				screen.z = depth;
				var p = cam.ScreenToWorldPoint(screen);
				var p2D = new Vector2(p.x, p.y);
				if(points.Count <= 0 || Vector2.Distance(p2D, points.Last()) > threshold) {
					points.Add(p2D);
				}
			}
		}

		void Build () {
			points = Utils2D.Constrain(points, threshold);
			var polygon = Polygon2D.Contour(points.ToArray());

			var vertices = polygon.Vertices;
			if(vertices.Length < 3) return; // error

			var triangulation = new Triangulation2D(polygon, angle);
			var go = Instantiate(prefab);
			go.transform.SetParent(transform, false);
			go.GetComponent<DemoMesh>().SetTriangulation(triangulation);

			Clear();
		}

		void Clear () {
			points.Clear();
		}

		void OnRenderObject () {
			if(points != null) {
				GL.PushMatrix();
				GL.MultMatrix (transform.localToWorldMatrix);
				lineMat.SetColor("_Color", Color.white);
				lineMat.SetPass(0);
				GL.Begin(GL.LINES);
				for(int i = 0, n = points.Count - 1; i < n; i++) {
					GL.Vertex(points[i]); GL.Vertex(points[i + 1]);
				}
				GL.End();
				GL.PopMatrix();
			}
		}

		void OnDrawGizmos () {
		}

	}

}

