using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class ItemDescriptor {
	public int _id;
	public int _type;
	public Vector3 _position;
};

public class LevelEditor : EditorWindow {

	private List<ItemDescriptor> _items;
	private int _itemSize;
	private Vector2 _scrollPos;

	private Dictionary<string, GameObject> _prefabMap;

	[MenuItem("Window/EFSTools/Level Editor")]
	public static void ShowWindow() {
		LevelEditor editor = EditorWindow.GetWindow(typeof(LevelEditor)) as LevelEditor;
		editor.Init();
		editor.Show();
	}
	
	void Init() {
		_itemSize = 0;
		_items = new List<ItemDescriptor>();
		_prefabMap = new Dictionary<string, GameObject>();
		PopulateItemPrefab(_prefabMap);
	}

	void PopulateItemPrefab(Dictionary<string, GameObject> prefabMap) {
		string prefabAbsoluteDir = Application.dataPath + "/Prefabs";
		DirectoryInfo dir = new DirectoryInfo(prefabAbsoluteDir);
		FileInfo[] info = dir.GetFiles("*.prefab");
		foreach (FileInfo f in info) {
			string prefabRelativePath = "Assets/Prefabs/" + f.Name;
			GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabRelativePath) as GameObject;
			if (prefab == null) {
				Debug.LogWarning(f.Name + " not loaded by AssetDatabase.LoadAssetAtPath");
				continue;
			}
			prefabMap.Add(f.Name, prefab);
		}

	}

	void OnEnable() {
		EditorApplication.playmodeStateChanged = OnEditorChangedPlayMode;
	}
	
	void OnEditorChangedPlayMode() {
		//Saving off the id and popping it back between plays allows the Waypoint Editor to edit in-game
		//but still not lose the reference of the old selection after coming out of the game.
		if (!EditorApplication.isPlaying) {
			if (EditorApplication.isPlayingOrWillChangePlaymode) {
				;
			} else {
				;
			}
		}
	}
	
	void OnGUI() {

		EditorGUILayout.BeginVertical();
		//EditorGUILayout.HelpBox("Changes will take effect immediately, be careful!", MessageType.Info);

		int newSize = EditorGUILayout.IntField("Item List Size: ", _itemSize, GUILayout.MaxWidth(200));
		if (newSize != _itemSize && newSize >= 0) {
			List<ItemDescriptor> newList = new List<ItemDescriptor>(new ItemDescriptor[newSize]);
			
			for (int i = 0; i < newSize; i++) {
				newList[i] = new ItemDescriptor();
				if (i < _itemSize)
					newList[i] = _items[i];
			}
			
			_itemSize = newSize;
			_items = newList;
		}

		_scrollPos = EditorGUILayout.BeginScrollView(_scrollPos,  GUILayout.Height (300));

		// List Title
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("ID", GUILayout.MinWidth(30), GUILayout.MaxWidth(60));
		EditorGUILayout.LabelField("Type", GUILayout.MinWidth(30), GUILayout.MaxWidth(60));
		EditorGUILayout.LabelField("Position(Integer)");
		EditorGUILayout.EndHorizontal();

		for(int i = 0; i < _itemSize; i++) {
			EditorGUILayout.BeginHorizontal();
			_items[i]._id = EditorGUILayout.IntField(_items[i]._id, GUILayout.MinWidth(30), GUILayout.MaxWidth(60));
			_items[i]._type = EditorGUILayout.IntField(_items[i]._type, GUILayout.MinWidth(30), GUILayout.MaxWidth(60));
			_items[i]._position =
				EditorGUILayout.Vector3Field("", _items[i]._position);			
			EditorGUILayout.EndHorizontal();
		}

		EditorGUILayout.EndScrollView();

		Separator();
		
		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("Create")) {
			Create();
		}
		if (GUILayout.Button("Reset Value")) {
			for(int i = 0; i < _itemSize; i++) {
				_items[i]._id = 0;
				_items[i]._type = 0;
				_items[i]._position = Vector3.zero;
			}
		}
		if (GUILayout.Button ("Clear Scene")) {
			GameObject levelEditor = GameObject.Find ("*LevelEditor*");
			if (levelEditor != null) {
				DestroyImmediate(levelEditor);
			}
		}
		if (GUILayout.Button ("Undo")) {
			Undo.PerformUndo();
		}
		if (GUILayout.Button ("Redo")) {
			Undo.PerformRedo();
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.EndVertical();
	}

	void Separator() {
		string separator = "______________________________________________________________________________";
		EditorGUILayout.LabelField(separator, GUILayout.ExpandWidth(true));
	}

	void OnInspectorUpdate() {
		Repaint();
	}

	void Create() {
		bool sceneDirty = false;
		GameObject levelEditor = null;
		for(int i = 0; i < _itemSize; i++) {
			if (i == 0) {
				levelEditor = GameObject.Find ("*LevelEditor*");
				if (levelEditor == null) {
					levelEditor = new GameObject("*LevelEditor*");
					sceneDirty = true;
					Undo.RegisterCreatedObjectUndo(levelEditor, "Create Level Editor");
				}
			}
			string prefabName = GameSetting.PrefabName(_items[i]._id, _items[i]._type);
			if (prefabName == null || _items[i]._id == 0 || !_prefabMap.ContainsKey(prefabName))
				continue;
			GameObject gamePrefab = _prefabMap[prefabName];
			GameObject gameObject = Instantiate(gamePrefab, _items[i]._position, gamePrefab.transform.rotation) as GameObject;
			if (gameObject == null) {
				Debug.LogError(prefabName + " not instantiated");
				continue;
			}
			// truncate *.prefab from name
			gameObject.name = prefabName.Substring(0, prefabName.Length - 7);
			Undo.RegisterCreatedObjectUndo(gameObject, "Create An Item");
			sceneDirty = true;
			gameObject.transform.SetParent(levelEditor.transform);
		}

		if (sceneDirty)
			EditorApplication.MarkSceneDirty();
	}
}