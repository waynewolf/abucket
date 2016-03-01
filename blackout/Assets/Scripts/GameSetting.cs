using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameSetting : Singleton<GameSetting> {

	public static Dictionary<int, string> _itemMap = new Dictionary<int, string>() {
		{0, "Unknown"},
		{1, "Hero"},
		{10, "Floor"},
		{50, "LightBulb"},
		{70, "SolarPanel"},
		{60, "Fan"},
		{61, "Fan"},
		{120, "Wire"},
		{280, "Windmill"},
		{310, "PowerSource"},
		{320, "Exit"},
	};

	[System.Serializable]
	public class PreloadItem {
		public GameObject _prefab;		// attached script component contains the id and type information, so
		public int _id;					// these two fields are not necessary, but use if for easy searching
		public int _type;				// these two fields are not necessary, but use if for easy searching
	}

	// Populate in editor inspector, ideally to populate this list programmabally,
	// but how to do this in runtime without using Resource.Load, in which way all
	// prefabs have to be put in the Assets/Resources Folder.
	public List<PreloadItem> _preloadItems;
	private List<GameObject> _allPrefabs;

	public static string PrefabName (int id, int type){
		if (_itemMap.ContainsKey(id))
			return id.ToString() + "_" + _itemMap[id] + "_" + type.ToString() + ".prefab";
		return null;
	}

	public GameObject SearchPrefab(int id, int type) {
		if (_preloadItems == null)
			return null;
		foreach(PreloadItem item in _preloadItems) {
			if (item._id == id && item._type == type)
				return item._prefab;
		}
		return null;
	}

	public List<GameObject> GetAllPrefabs() {
		if (_allPrefabs == null && _preloadItems != null) {
			_allPrefabs = new List<GameObject>();
			foreach(PreloadItem item in _preloadItems) {
				_allPrefabs.Add (item._prefab);
			}
		}
		return _allPrefabs;
	}
}
