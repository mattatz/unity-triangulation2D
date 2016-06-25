using UnityEngine;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace mattatz.Triangulation2DSystem {

	public class Segment2D {

		public int ReferenceCount { get { return reference; } }
		public Vertex2D a, b;

		int reference;
		float length;

		public Segment2D (Vertex2D a, Vertex2D b) {
			this.a = a;
			this.b = b;
		}

		public Vector2 Midpoint () {
			return (a.Coordinate + b.Coordinate) * 0.5f;
		}

		public float Length () {
			if(length <= 0f) {
				length = (a.Coordinate - b.Coordinate).magnitude;
			}
			return length;
		}

		/*
		 * check a given point "p" lies within diametral circle of segment(a, b) 
		 */
		public bool EncroachedUpon (Vector2 p) {
			if(p == a.Coordinate || p == b.Coordinate) return false;
			var radius = (a.Coordinate - b.Coordinate).magnitude * 0.5f;
			return (Midpoint() - p).magnitude < radius;
		}

		public bool HasPoint (Vertex2D v) {
			return (a == v) || (b == v);
		}

		public bool HasPoint (Vector2 p) {
			return (a.Coordinate == p) || (b.Coordinate == p);
		}

		public int Increment () {
			a.Increment();
			b.Increment();
			return ++reference;
		}

		public int Decrement () {
			a.Decrement();
			b.Decrement();
			return --reference;
		}

		public void DrawGizmos () {
			Gizmos.DrawLine(a.Coordinate, b.Coordinate);
		}
	}

}

