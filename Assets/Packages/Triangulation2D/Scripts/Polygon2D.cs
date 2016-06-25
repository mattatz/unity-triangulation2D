using UnityEngine;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace mattatz.Triangulation2DSystem {

	public class Polygon2D {

		public Vertex2D[] Vertices { get { return vertices.ToArray(); } }
		public Segment2D[] Segments { get { return segments.ToArray(); } }

		List<Vertex2D> vertices;
		List<Segment2D> segments;

		/*
		 * slow incremental approach
		 * O(n * log(n))
		 */
		public static Polygon2D ConvexHull (Vector2[] points) {
			var ordered = points.ToList().OrderBy(p => p.x).ToList();

			var upper = new List<Vector2>();
			upper.Add(ordered[0]);
			upper.Add(ordered[1]);
			for(int i = 2, n = ordered.Count; i < n; i++) {
				upper.Add(ordered[i]);
				int l = upper.Count;
				if(l > 2) {
					var p = upper[l - 3];
					var r = upper[l - 2];
					var q = upper[l - 1];
					if(Utils2D.LeftSide(p, q, r)) { // r is left side of pq
						upper.RemoveAt(l - 2);
					}
				}
			}

			var lower = new List<Vector2>();
			lower.Add(ordered[ordered.Count - 1]);
			lower.Add(ordered[ordered.Count - 2]);
			for(int i = ordered.Count - 3; i >= 0; i--) {
				lower.Add(ordered[i]);
				int l = lower.Count;
				if(l > 2) {
					var p = lower[l - 3];
					var r = lower[l - 2];
					var q = lower[l - 1];
					if(Utils2D.LeftSide(p, q, r)) { // r is left side of pq
						lower.RemoveAt(l - 2);
					}
				}
			}

			lower.RemoveAt(lower.Count - 1);
			lower.RemoveAt(0);

			upper.AddRange (lower);

			return new Polygon2D(upper.ToArray());
		}

		public static Polygon2D Contour (Vector2[] points) {
			var n = points.Length;
			var edges = new List<HalfEdge2D>();
			for(int i = 0; i < n; i++) {
				edges.Add(new HalfEdge2D(points[i]));
			}
			for(int i = 0; i < n; i++) {
				var e = edges[i];
				e.from = edges[(i == 0) ? (n - 1) : (i - 1)];
				e.to = edges[(i + 1) % n];
			}
			edges = Polygon2D.SplitEdges(edges);

			var result = new List<Vector2>();

			HalfEdge2D start = edges[0];
			result.Add(start.p);

			HalfEdge2D current = start;

			while(true) {
				HalfEdge2D from = current, to = current.to;
				HalfEdge2D from2 = to.to, to2 = from2.to;

				bool flag = false;

				while(from2 != start && to2 != from) {
					if(flag = Utils2D.Intersect(from.p, to.p, from2.p, to2.p)) {
						break;
					}
					from2 = to2;
					to2 = to2.to;
				}

				if(!flag) {
					result.Add(to.p);
					current = to; // step to next
				} else {
					result.Add(from2.p);

					// reconnect
					from.to = from2;
					from2.to = from; // invert this edge later

					to.from = to2;
					to.Invert();
					to2.from = to;

					HalfEdge2D e = from2;
					while(e != to) {
						e.Invert();
						e = e.to;
					}

					current = from2;
				}

				if(current == start) break;
			}

			result.RemoveAt(result.Count - 1); // remove last

			return new Polygon2D(result.ToArray());
		}

		// Disable to intersect more than two edges
		static List<HalfEdge2D> SplitEdges (List<HalfEdge2D> edges) {
			HalfEdge2D start = edges[0];
			HalfEdge2D cur = start;

			while(true) {
				HalfEdge2D from = cur, to = from.to;
				HalfEdge2D from2 = to.to, to2 = from2.to;

				int intersections = 0;

				while(to2 != from.from) {
					if(Utils2D.Intersect(from.p, to.p, from2.p, to2.p)) {
						intersections++;
						if(intersections >= 2) {
							break;
						}
					}
					// next
					from2 = from2.to;
					to2 = to2.to;
				}

				if(intersections >= 2) {
					edges.Add(cur.Split());
				} else {
					// next
					cur = cur.to;
					if(cur == start) break;
				}
			}

			return edges;
		}

		// contour must be inversed clockwise order.
		public Polygon2D (Vector2[] contour) {
			vertices = contour.Select(p => new Vertex2D(p)).ToList();
			segments = new List<Segment2D>();
			for(int i = 0, n = vertices.Count; i < n; i++) {
				var v0 = vertices[i];
				var v1 = vertices[(i + 1) % n];
				segments.Add(new Segment2D(v0, v1));
			}
		}

		public bool Contains (Vector2 p) {
			return Utils2D.Contains(p, vertices);
		}

		public void DrawGizmos () {
			segments.ForEach(s => s.DrawGizmos());
		}

	}

}
