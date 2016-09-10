/**
 * @author mattatz / http://mattatz.github.io
 *
 * Ruppert's Delaunay Refinement Algorithm
 * Jim Ruppert. A Delaunay Refinement Algorithm for Quality 2-Dimensional Mesh Generation / http://www.cis.upenn.edu/~cis610/ruppert.pdf
 * The Quake Group at Carnegie Mellon University / https://www.cs.cmu.edu/~quake/tripaper/triangle3.html
 * Wikipedia / https://en.wikipedia.org/wiki/Ruppert%27s_algorithm
 * ETH zurich CG13 Chapter 7 / http://www.ti.inf.ethz.ch/ew/Lehre/CG13/lecture/Chapter%207.pdf
 */

using UnityEngine;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace mattatz.Triangulation2DSystem {

	public class Triangulation2D {

		const float kAngleMax = 30f;

		public Polygon2D Polygon { get { return PSLG; } }
		public Triangle2D[] Triangles { get { return T.ToArray(); } }
		public List<Segment2D> Edges { get { return E; } }
		public List<Vertex2D> Points { get { return P; } }

		List<Vertex2D> V = new List<Vertex2D>(); // vertices in PSLG
		List<Segment2D> S = new List<Segment2D>(); // segments in PSLG

		List<Vertex2D> P = new List<Vertex2D>(); // vertices in DT
		List<Segment2D> E = new List<Segment2D>(); // segments in DT

		Polygon2D PSLG;
		List<Triangle2D> T = new List<Triangle2D>();

		public Triangulation2D (Polygon2D polygon, float angle = 20f, float threshold = 0.1f) {
			angle = Mathf.Min(angle, kAngleMax) * Mathf.Deg2Rad;

			PSLG = polygon;
			V = PSLG.Vertices.ToList();
			S = PSLG.Segments.ToList();
			Triangulate (polygon.Vertices.Select(v => v.Coordinate).ToArray(), angle, threshold);
		}

		public Mesh Build () {
			return Build((Vertex2D v) => {
				var xy = v.Coordinate;
				return new Vector3(xy.x, xy.y, 0f);
			});
		}

		public Mesh Build (Func<Vertex2D, Vector3> coord) {
			var mesh = new Mesh();

			var vertices = P.Select(p => { 
				return coord(p);
			}).ToArray();

			var triangles = new List<int>();
			for(int i = 0, n = T.Count; i < n; i++) {
				var t = T[i];
				int a = P.IndexOf(t.a), b = P.IndexOf(t.b), c = P.IndexOf(t.c);
				if(a < 0 || b < 0 || c < 0) {
					// Debug.Log(a + " : " + b + " : " + c);
					continue;
				}
				if(Utils2D.LeftSide(t.a.Coordinate, t.b.Coordinate, t.c.Coordinate)) {
					triangles.Add(a); triangles.Add(c); triangles.Add(b);
				} else {
					triangles.Add(a); triangles.Add(b); triangles.Add(c);
				}
			}

			mesh.vertices = vertices;
			mesh.SetTriangles(triangles.ToArray(), 0);
			mesh.RecalculateNormals();

			return mesh;
		}

		int FindVertex (Vector2 p, List<Vertex2D> Vertices) {
			return Vertices.FindIndex (v => { 
				return v.Coordinate == p;
				// return Mathf.Approximately(v.Coordinate.x, p.x) && Mathf.Approximately(v.Coordinate.y, p.y);
			});
		}

		int FindSegment (Vertex2D a, Vertex2D b, List<Segment2D> Segments) {
			return Segments.FindIndex (s => (s.a == a && s.b == b) || (s.a == b && s.b == a));
		}

		public Vertex2D CheckAndAddVertex (Vector2 coord) {
			var idx = FindVertex(coord, P);
			if(idx < 0) {
				var v = new Vertex2D(coord);
				P.Add(v);
				return v;
			}
			return P[idx];
		}

		public Segment2D CheckAndAddSegment (Vertex2D a, Vertex2D b) {
			var idx = FindSegment(a, b, E);
			Segment2D s;
			if(idx < 0) {
				s = new Segment2D(a, b);
				E.Add(s);
			} else {
				s = E[idx];
			}
			s.Increment();
			return s;
		}

		public Triangle2D AddTriangle (Vertex2D a, Vertex2D b, Vertex2D c) {
			var s0 = CheckAndAddSegment(a, b);
			var s1 = CheckAndAddSegment(b, c);
			var s2 = CheckAndAddSegment(c, a);
			var t = new Triangle2D(s0, s1, s2);
			T.Add(t);
			return t;
		}

		public void RemoveTriangle (Triangle2D t) {
			var idx = T.IndexOf(t);
			if(idx < 0) return;

			T.RemoveAt(idx);
			if(t.s0.Decrement() <= 0) RemoveSegment (t.s0);
			if(t.s1.Decrement() <= 0) RemoveSegment (t.s1);
			if(t.s2.Decrement() <= 0) RemoveSegment (t.s2);
		}

		public void RemoveTriangle (Segment2D s) {
			T.FindAll(t => t.HasSegment(s)).ForEach(t => RemoveTriangle(t));
		}

		public void RemoveSegment (Segment2D s) {
			E.Remove(s);
			if(s.a.ReferenceCount <= 0) P.Remove(s.a);
			if(s.b.ReferenceCount <= 0) P.Remove(s.b);
		}

		void Bound (Vector2[] points, out Vector2 min, out Vector2 max) {
			min = Vector2.one * float.MaxValue;
			max = Vector2.one * float.MinValue;
			for(int i = 0, n = points.Length; i < n; i++) {
				var p = points[i];
				min.x = Mathf.Min (min.x, p.x);
				min.y = Mathf.Min (min.y, p.y);
				max.x = Mathf.Max (max.x, p.x);
				max.y = Mathf.Max (max.y, p.y);
			}
		}

		public Triangle2D AddExternalTriangle (Vector2 min, Vector2 max) {
			var center = (max + min) * 0.5f;
			var diagonal = (max - min).magnitude;
			var dh = diagonal * 0.5f;
			var rdh = Mathf.Sqrt(3f) * dh;
			return AddTriangle(
				CheckAndAddVertex(center + new Vector2(-rdh, -dh) * 3f),
				CheckAndAddVertex(center + new Vector2(rdh, -dh) * 3f),
				CheckAndAddVertex(center + new Vector2(0f, diagonal) * 3f)
			);
		}

		void Triangulate (Vector2[] points, float angle, float threshold) {
			Vector2 min, max;
			Bound(points, out min, out max);

			AddExternalTriangle(min, max);

			for(int i = 0, n = points.Length; i < n; i++) {
				var v = points[i];
				UpdateTriangulation (v);
			}

			Refine (angle, threshold);
			RemoveExternalPSLG ();
		}	

		void RemoveCommonTriangles (Triangle2D target) {
			for(int i = 0, n = T.Count; i < n; i++) {
				var t = T[i];
				if(t.HasCommonPoint(target)) {
					RemoveTriangle (t);
					i--;
					n--;
				}
			}
		}

		void RemoveExternalPSLG () {
			for(int i = 0, n = T.Count; i < n; i++) {
				var t = T[i];
				if(ExternalPSLG(t) || HasOuterSegments(t)) {
				// if(ExternalPSLG(t)) {
					RemoveTriangle (t);
					i--;
					n--;
				}
			}
		}

		bool ContainsSegments (Segment2D s, List<Segment2D> segments) {
			return segments.FindIndex (s2 => 
				(s2.a.Coordinate == s.a.Coordinate && s2.b.Coordinate == s.b.Coordinate) ||
				(s2.a.Coordinate == s.b.Coordinate && s2.b.Coordinate == s.a.Coordinate)
			) >= 0;
		}

		bool HasOuterSegments (Triangle2D t) {
			if(!ContainsSegments(t.s0, S)) {
				return ExternalPSLG(t.s0);
			}
			if(!ContainsSegments(t.s1, S)) {
				return ExternalPSLG(t.s1);
			}
			if(!ContainsSegments(t.s2, S)) {
				return ExternalPSLG(t.s2);
			}
			return false;
		}

		void UpdateTriangulation (Vector2 p) {
			var tmpT = new List<Triangle2D>();
			var tmpS = new List<Segment2D>();

			var v = CheckAndAddVertex(p);
			tmpT = T.FindAll(t => t.ContainsInExternalCircle(v));
			tmpT.ForEach(t => {
				tmpS.Add(t.s0);
				tmpS.Add(t.s1);
				tmpS.Add(t.s2);

				AddTriangle(t.a, t.b, v);
				AddTriangle(t.b, t.c, v);
				AddTriangle(t.c, t.a, v);
				RemoveTriangle (t);
			});

			while(tmpS.Count != 0) {
				var s = tmpS.Last();
				tmpS.RemoveAt(tmpS.Count - 1);
				
				var commonT = T.FindAll(t => t.HasSegment(s));
				if(commonT.Count <= 1) continue;
				
				var abc = commonT[0];
				var abd = commonT[1];
				
				if(abc.Equals(abd)) {
					RemoveTriangle (abc);
					RemoveTriangle (abd);
					continue;
				}
				
				var a = s.a;
				var b = s.b;
				var c = abc.ExcludePoint(s);
				var d = abd.ExcludePoint(s);
				
				var ec = Circle2D.GetCircumscribedCircle(abc);
				if(ec.Contains(d.Coordinate)) {
					RemoveTriangle (abc);
					RemoveTriangle (abd);
					
					AddTriangle(a, c, d); // add acd
					AddTriangle(b, c, d); // add bcd

					var segments0 = abc.ExcludeSegment(s);
					tmpS.Add(segments0[0]);
					tmpS.Add(segments0[1]);
					
					var segments1 = abd.ExcludeSegment(s);
					tmpS.Add(segments1[0]);
					tmpS.Add(segments1[1]);
				}
			}

		}

		bool FindAndSplit (float threshold) {
			for(int i = 0, n = S.Count; i < n; i++) {
				var s = S[i];
				if(s.Length() < threshold) continue;

				for(int j = 0, m = P.Count; j < m; j++) {
					if(s.EncroachedUpon(P[j].Coordinate)) {
						SplitSegment(s);
						return true;
					}
				}
			}
			return false;
		}

		bool ExternalPSLG (Vector2 p) {
			return !Utils2D.Contains(p, V);
		}

		bool ExternalPSLG (Segment2D s) {
			return ExternalPSLG(s.Midpoint());
		}

		bool ExternalPSLG (Triangle2D t) {
			// return ExternalPSLG(t.s0) && ExternalPSLG(t.s1) && ExternalPSLG(t.s2);
			return 
				ExternalPSLG(t.a.Coordinate) ||
				ExternalPSLG(t.b.Coordinate) ||
				ExternalPSLG(t.c.Coordinate)
			;
		}

		void Refine (float angle, float threshold)  {
			while(T.Any(t => !ExternalPSLG(t) && t.Skinny(angle, threshold))) {
				RefineSubRoutine(angle, threshold);
			}
		}

		void RefineSubRoutine (float angle, float threshold) {

			while(true) { 
				if(!FindAndSplit(threshold)) break; 
			}

			var skinny = T.Find (t => !ExternalPSLG(t) && t.Skinny(angle, threshold));
			var p = skinny.Circumcenter();

			var segments = S.FindAll(s => s.EncroachedUpon(p));
			if(segments.Count > 0) {
				segments.ForEach(s => SplitSegment(s));
			} else {
				SplitTriangle(skinny);
			}
		}

		void SplitTriangle (Triangle2D t) {
			var c = t.Circumcenter();
			UpdateTriangulation(c);
		}

		void SplitSegment (Segment2D s) {
			Vertex2D a = s.a, b = s.b;
			var mv = new Vertex2D(s.Midpoint());

			// add mv to V 
			// the index is between a and b.
			var idxA = V.IndexOf(a);
			var idxB = V.IndexOf(b);
			if(Mathf.Abs(idxA - idxB) == 1) {
				var idx = (idxA > idxB) ? idxA : idxB;
				V.Insert(idx, mv);
			} else {
				V.Add(mv);
			}

			UpdateTriangulation(mv.Coordinate);

			// Add two halves to S
			var sidx = S.IndexOf(s);
			S.RemoveAt(sidx);

			S.Add(new Segment2D(s.a, mv));
			S.Add(new Segment2D(mv, s.b));
		}

		bool CheckUnique () {
			var flag = false;

			for(int i = 0, n = P.Count; i < n; i++) {
				var v0 = P[i];
				for(int j = i + 1; j < n; j++) {
					var v1 = P[j];
					if(Utils2D.CheckEqual(v0, v1)) {
						Debug.LogWarning("vertex " + i + " equals " + j);
						flag = true;
					}
				}
			}

			for(int i = 0, n = E.Count; i < n; i++) {
				var s0 = E[i];
				for(int j = i + 1; j < n; j++) {
					var s1 = E[j];
					if(Utils2D.CheckEqual(s0, s1)) {
						Debug.LogWarning("segment " + i + " equals " + j);
						flag = true;
					}
				}
			}

			for(int i = 0, n = T.Count; i < n; i++) {
				var t0 = T[i];
				for(int j = i + 1; j < n; j++) {
					var t1 = T[j];
					if(Utils2D.CheckEqual(t0, t1)) {
						Debug.LogWarning("triangle " + i + " equals " + j);
						flag = true;
					}
				}
			}

			for(int i = 0, n = T.Count; i < n; i++) {
				var t = T[i];
				if(Utils2D.CheckEqual(t.s0, t.s1) || Utils2D.CheckEqual(t.s0, t.s2) || Utils2D.CheckEqual(t.s1, t.s2)) {
					Debug.LogWarning("triangle " + i + " has duplicated segments");
					flag = true;
				}
			}

			return flag;
		}

		public void DrawGizmos () {
			T.ForEach(t => t.DrawGizmos());
		}

	}

}
