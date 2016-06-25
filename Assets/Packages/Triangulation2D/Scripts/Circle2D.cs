using UnityEngine;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace mattatz.Triangulation2DSystem {

	public class Circle2D {
		public Vector2 center;
		public float radius;

		public Circle2D (Vector2 c, float r) {
			this.center = c;
			this.radius = r;
		}

		public bool Contains (Vector2 p) {
			return (p - center).magnitude < radius;
		}

		public static Circle2D GetCircumscribedCircle(Triangle2D triangle) {
			var x1 = triangle.a.Coordinate.x;
			var y1 = triangle.a.Coordinate.y;
			var x2 = triangle.b.Coordinate.x;
			var y2 = triangle.b.Coordinate.y;
			var x3 = triangle.c.Coordinate.x;
			var y3 = triangle.c.Coordinate.y;

			float x1_2 = x1 * x1;
			float x2_2 = x2 * x2;
			float x3_2 = x3 * x3;
			float y1_2 = y1 * y1;
			float y2_2 = y2 * y2;
			float y3_2 = y3 * y3;

			// 外接円の中心座標を計算
			float c = 2f * ((x2 - x1) * (y3 - y1) - (y2 - y1) * (x3 - x1));
			float x = ((y3 - y1) * (x2_2 - x1_2 + y2_2 - y1_2) + (y1 - y2) * (x3_2 - x1_2 + y3_2 - y1_2)) / c;
			float y = ((x1 - x3) * (x2_2 - x1_2 + y2_2 - y1_2) + (x2 - x1) * (x3_2 - x1_2 + y3_2 - y1_2)) / c;
			float _x = (x1 - x);
			float _y = (y1 - y);

			float r = Mathf.Sqrt((_x * _x) + (_y * _y));
			return new Circle2D(new Vector2(x, y), r);
		}

		public void DrawGizmos (float step = 0.02f) {

			var points = new List<Vector2>();
			for(float t = 0f; t <= 1f; t += step) {
				var r = t * Mathf.PI * 2f;
				float x = Mathf.Cos(r) * radius;
				float y = Mathf.Sin(r) * radius;
				points.Add(center + new Vector2(x, y));
			}

			for(int i = 0, n = points.Count; i < n; i++) {
				var p0 = points[i];
				var p1 = points[(i + 1) % n];
				Gizmos.DrawLine(p0, p1);
			}

		}

	}


}

