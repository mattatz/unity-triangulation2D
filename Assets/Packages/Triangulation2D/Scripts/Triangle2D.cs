using UnityEngine;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace mattatz.Triangulation2DSystem {

	public class Triangle2D {

		public Vertex2D a, b, c;
		public Segment2D s0, s1, s2;
		private Circle2D circum;

		public Triangle2D (Segment2D s0, Segment2D s1, Segment2D s2) {
			this.s0 = s0;
			this.s1 = s1;
			this.s2 = s2;
			this.a = s0.a;
			this.b = s0.b;
			this.c = (s2.b == this.a || s2.b == this.b) ? s2.a : s2.b;
		}

		public bool HasPoint (Vertex2D p) {
			return (a == p) || (b == p) || (c == p);
		}

		public bool HasCommonPoint (Triangle2D t) {
			return HasPoint(t.a) || HasPoint(t.b) || HasPoint(t.c);
		}

		public bool HasSegment (Segment2D s) {
			return (s0 == s) || (s1 == s) || (s2 == s);
		}

		public bool HasSegment (Vector2 a, Vector2 b) {
			return (s0.HasPoint(a) && s0.HasPoint(b)) || (s1.HasPoint(a) && s1.HasPoint(b)) || (s2.HasPoint(a) && s2.HasPoint(b));
		}

		public Vertex2D ExcludePoint (Segment2D s) {
			if(!s.HasPoint(a)) return a;
			else if(!s.HasPoint(b)) return b;
			return c;
		}

		public Vertex2D ExcludePoint (Vector2 p0, Vector2 p1) {
			if(p0 != a.Coordinate && p1 != a.Coordinate) return a;
			else if(p0 != b.Coordinate && p1 != b.Coordinate) return b;
			return c;
		}

		public Vertex2D[] ExcludePoint (Vector2 p) {
			if(p == a.Coordinate) return new Vertex2D[] { b, c };
			else if(p == b.Coordinate) return new Vertex2D[] { a, c };
			return new Vertex2D[] { a, b };
		}

		public Segment2D[] ExcludeSegment (Segment2D s) {
			if(s0.Equals(s)) {
				return new Segment2D[] { s1, s2 };
			} else if(s1.Equals(s)) {
				return new Segment2D[] { s0, s2 };
			}
			return new Segment2D[] { s0, s1 };
		}

		public Segment2D CommonSegment (Vertex2D v0, Vertex2D v1) {
			if(s0.HasPoint(v0) && s0.HasPoint(v1)) {
				return s0;
			} else if(s1.HasPoint(v0) && s1.HasPoint(v1)) {
				return s1;
			}
			return s2;
		}

		public Segment2D[] CommonSegments (Vertex2D v) {
			if(s0.HasPoint(v) && s1.HasPoint(v)) {
				return new [] {s0, s1};
			} else if(s1.HasPoint(v) && s2.HasPoint(v)) {
				return new [] {s1, s2};
			}
			return new [] {s0, s2};
		}

		public Vector2 Circumcenter () {
			if(circum == null) {
				circum = Circle2D.GetCircumscribedCircle(this);
			}
			return circum.center;
		}

		public bool ContainsInExternalCircle (Vertex2D v) {
			if(circum == null) {
				circum = Circle2D.GetCircumscribedCircle(this);
			}
			return circum.Contains(v.Coordinate);
		}

		float Angle (Vector2 from, Vector2 to0, Vector2 to1) {
			var v0 = (to0 - from);
			var v1 = (to1 - from);

			// 0 ~ PI
			float acos = Mathf.Acos(Vector2.Dot(v0, v1) / Mathf.Sqrt(v0.sqrMagnitude * v1.sqrMagnitude));
			return acos;
		}

		// angle must to RADIAN
		public bool Skinny (float angle, float threshold) {
			if(s0.Length() <= threshold && s1.Length() <= threshold && s2.Length() <= threshold) return false;

			if(Angle(a.Coordinate, b.Coordinate, c.Coordinate) < angle) return true; // angle bac
			else if(Angle(b.Coordinate, a.Coordinate, c.Coordinate) < angle) return true; // angle abc
			else if(Angle(c.Coordinate, a.Coordinate, b.Coordinate) < angle) return true; // angle acb
			return false;
		}

		public bool Equals (Triangle2D t) {
			return HasPoint(t.a) && HasPoint(t.b) && HasPoint(t.c);
		}

		public void DrawGizmos () {
			s0.DrawGizmos();
			s1.DrawGizmos();
			s2.DrawGizmos();
		}

	}

}
