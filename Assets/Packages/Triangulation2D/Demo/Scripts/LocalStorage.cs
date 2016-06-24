using UnityEngine;

using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace mattatz.Utils {

	[Serializable]
	public class JsonSerialization<T> {
		[SerializeField] List<T> target;
		public List<T> ToList() { return target; }
		public JsonSerialization(List<T> target) {
			this.target = target;
		}
	}

	public class LocalStorage {

		public static void SaveList<T> (List<T> target, string fileName) {
			var text = JsonUtility.ToJson(new JsonSerialization<T>(target));
			Save(text, fileName);
		}

		public static List<T> LoadList<T> (string fileName) {
			string json = Load(fileName);
			return JsonUtility.FromJson<JsonSerialization<T>>(json).ToList();
		}

		public static void Save (string text, string fileName) {
			string path = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);
			FileInfo fi = new FileInfo(path);
			StreamWriter sw = fi.CreateText();
			sw.WriteLine(text);
			sw.Flush();
			sw.Close();
		}

		public static string Load (string fileName) {
			string path = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);
			return System.IO.File.ReadAllText(path);
		}

	}

}

