using UnityEngine;
using Random = UnityEngine.Random;

using System.Collections;
using System.Collections.Generic;

namespace mattatz.Triangulation2DSystem.Example {

	[RequireComponent (typeof(MeshFilter))]
	[RequireComponent (typeof(Rigidbody))]
	public class DemoMesh : MonoBehaviour {

		[SerializeField] Material lineMat;

		Triangle2D[] triangles;

		void Start () {
			var body = GetComponent<Rigidbody>();
			body.AddForce(Vector3.forward * Random.Range(150f, 160f));
			body.AddTorque(Random.insideUnitSphere * Random.Range(10f, 20f));
		}

		void Update () {}

		public void SetTriangulation (Triangulation2D triangulation) {
			var mesh = triangulation.Build();
			GetComponent<MeshFilter>().sharedMesh = mesh;
			this.triangles = triangulation.Triangles;
		}

		void OnRenderObject () {
			if(triangles == null) return;

			GL.PushMatrix();
			GL.MultMatrix (transform.localToWorldMatrix);

			lineMat.SetColor("_Color", Color.black);
			lineMat.SetPass(0);
			GL.Begin(GL.LINES);
			for(int i = 0, n = triangles.Length; i < n; i++) {
				var t = triangles[i];
				GL.Vertex(t.s0.a.Coordinate); GL.Vertex(t.s0.b.Coordinate);
				GL.Vertex(t.s1.a.Coordinate); GL.Vertex(t.s1.b.Coordinate);
				GL.Vertex(t.s2.a.Coordinate); GL.Vertex(t.s2.b.Coordinate);
			}
			GL.End();
			GL.PopMatrix();
		}

	}

}

