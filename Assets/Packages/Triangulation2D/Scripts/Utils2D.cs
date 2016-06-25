using UnityEngine;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace mattatz.Triangulation2DSystem {

	public class Utils2D {

		// constrain a distance between two points to "threshold" length
		public static List<Vector2> Constrain (List<Vector2> points, float threshold = 1f) {
			var result = new List<Vector2>();

			var n = points.Count;
			for(int i = 0, j = 1; i < n && j < n; j++) {
				var from = points[i];
				var to = points[j];
				if(Vector2.Distance(from, to) > threshold) {
					result.Add(from);
					i = j;
				}
			}

			var p0 = result.Last();
			var p1 = result.First();
			if(Vector2.Distance(p0, p1) > threshold) {
				result.Add((p0 + p1) * 0.5f);
			}

			return result;
		}

		// http://stackoverflow.com/questions/563198/how-do-you-detect-where-two-line-segments-intersect
		// check intersection segment (p0, p1) to segment (p2, p3)
		public static bool Intersect (Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3) {
			var s1 = p1 - p0;
			var s2 = p3 - p2;
			var s = (-s1.y * (p0.x - p2.x) + s1.x * (p0.y - p2.y)) / (-s2.x * s1.y + s1.x * s2.y);
			var t = ( s2.x * (p0.y - p2.y) - s2.y * (p0.x - p2.x)) / (-s2.x * s1.y + s1.x * s2.y);
			return (s >= 0 && s <= 1 && t >= 0 && t <= 1);
		}

		// http://stackoverflow.com/questions/217578/how-can-i-determine-whether-a-2d-point-is-within-a-polygon
		public static bool Contains (Vector2 p, List<Vertex2D> vertices) {
			var n = vertices.Count;
			bool c = false;
			for(int i = 0, j = n - 1; i < n; j = i++) {
				if(vertices[i].Coordinate == p) return true;
				if (
					((vertices[i].Coordinate.y > p.y) != (vertices[j].Coordinate.y > p.y)) &&
					(p.x < (vertices[j].Coordinate.x - vertices[i].Coordinate.x) * (p.y - vertices[i].Coordinate.y) / (vertices[j].Coordinate.y - vertices[i].Coordinate.y) + vertices[i].Coordinate.x)
				) {
					c = !c;
				}
			}
			// c == true means odd, c == false means even
			return c;
		}

		// check p is left side of segment (from, to)
		public static bool LeftSide (Vector2 from, Vector2 to, Vector2 p) {
			float cross = ((to.x - from.x) * (p.y - from.y) - (to.y - from.y) * (p.x - from.x));
			return (cross > 0f);
		}

		public static bool CheckEqual (Vertex2D v0, Vertex2D v1) {
			return (v0.Coordinate == v1.Coordinate);
		}

		public static bool CheckEqual (Segment2D s0, Segment2D s1) {
			return 
				(CheckEqual(s0.a, s1.a) && CheckEqual(s0.b, s1.b)) ||
				(CheckEqual(s0.a, s1.b) && CheckEqual(s0.b, s1.a));
		}

		public static bool CheckEqual (Triangle2D t0, Triangle2D t1) {
			// 0,1,2 == 0,1,2
			// 0,1,2 == 0,2,1
			// 0,1,2 == 1,0,2
			// 0,1,2 == 1,2,0
			// 0,1,2 == 2,0,1
			// 0,1,2 == 2,1,0
			return 
				(CheckEqual(t0.s0, t1.s0) && CheckEqual(t0.s1, t1.s1) && CheckEqual(t0.s2, t1.s2)) ||
				(CheckEqual(t0.s0, t1.s0) && CheckEqual(t0.s1, t1.s2) && CheckEqual(t0.s2, t1.s1)) ||

				(CheckEqual(t0.s0, t1.s1) && CheckEqual(t0.s1, t1.s0) && CheckEqual(t0.s2, t1.s2)) ||
				(CheckEqual(t0.s0, t1.s1) && CheckEqual(t0.s1, t1.s2) && CheckEqual(t0.s2, t1.s0)) ||

				(CheckEqual(t0.s0, t1.s2) && CheckEqual(t0.s1, t1.s0) && CheckEqual(t0.s2, t1.s1)) ||
				(CheckEqual(t0.s0, t1.s2) && CheckEqual(t0.s1, t1.s1) && CheckEqual(t0.s2, t1.s0));
		}

	}

}

