using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// On recycle item, the index is the recycled item index in internal list,
/// ugly implementation that exposing internal information to the outside
/// world, but it just simply works and more efficient than other method.
/// </summary>
public delegate void OnRecycleItem(int index);

/// <summary>
/// Manage the items in the scene, responsible for electric path generation
/// </summary>
public class LevelManager : Singleton<LevelManager> {
	public OnRecycleItem _onRecycleItem;

	private Wire[] _wires;
	private PowerSource _powerSource;
	private GraphPath _wireGraph;
	private GameObject _electricCurrentGameObjectHolder;
	private GameObject _dynamicObjectHolder;
	private GameObject _backpackItemGameObjectHolder;
	private HeroController _heroController;

	[HideInInspector] public HeroController Player { get { return _heroController; } }

	// level name => list of item identities
	public static Dictionary<string, List<ItemIdentity>> _backpackItemsEachLevel = 
		new Dictionary<string, List<ItemIdentity>>() {
			{"1", new List<ItemIdentity>() {
					new ItemIdentity(60, 0),
					new ItemIdentity(280, 0),}
			},
			{"2", new List<ItemIdentity>() {
					new ItemIdentity(70, 0),
					new ItemIdentity(50, 0),
					new ItemIdentity(60, 0),
					new ItemIdentity(280, 0),}
			},
			{"3", new List<ItemIdentity>() {
					new ItemIdentity(60, 0),
					new ItemIdentity(280, 0),}
			},
		};

	private List<GameObject> _currentItemsInBackpack;

	protected override void Awake () {
		base.Awake();
		_wires = GameObject.FindObjectsOfType<Wire>() as Wire[];

		_powerSource = GameObject.FindObjectOfType<PowerSource>() as PowerSource;
		if (_powerSource == null)
			Debug.LogError("One and only one power source must be in the scene");

		// scan the wires and powersource, generate graph(s)
		_wireGraph = GraphPath.Build(_wires);

		_electricCurrentGameObjectHolder = GameObject.Find ("*ElectricCurrentHolder*");
		if (_electricCurrentGameObjectHolder == null)
			_electricCurrentGameObjectHolder = new GameObject("*ElectricCurrentHolder*");

		_dynamicObjectHolder = GameObject.Find ("*DynamicObjectHolder*");
		if (_dynamicObjectHolder == null)
			_dynamicObjectHolder = new GameObject("*DynamicObjectHolder*");

		_backpackItemGameObjectHolder = GameObject.Find ("*BackpackItemHolder");
		if (_backpackItemGameObjectHolder == null)
			_backpackItemGameObjectHolder = new GameObject("*BackpackItemHolder");

		_currentItemsInBackpack = SpawnBackpackDefaultItems(Application.loadedLevelName);
		_heroController = FindObjectOfType<HeroController>() as HeroController;
	}

	public GraphPath GetWireGraph() {
		return _wireGraph;
	}

	public PowerSource GetPowerSource () {
		return _powerSource;
	}

	public GameObject GetElectricCurrentGameObjectHolder () {
		return _electricCurrentGameObjectHolder;
	}

	public GameObject GetDynamicGameObjectHolder () {
		return _dynamicObjectHolder;
	}

	private List<GameObject> SpawnBackpackDefaultItems (string loadedLevelName) {
		List<GameObject> defaultItems = new List<GameObject>();
		List<ItemIdentity> itemIdentities;
		_backpackItemsEachLevel.TryGetValue(loadedLevelName, out itemIdentities);
		if (itemIdentities != null) {
			foreach(ItemIdentity identity in itemIdentities) {
				GameObject prefab = GameSetting.Instance.SearchPrefab(identity._id, identity._type);
				if (prefab != null) {
					GameObject gameObject = Instantiate(prefab);
					gameObject.transform.SetParent(_backpackItemGameObjectHolder.transform, true);
					gameObject.SetActive(false);
					defaultItems.Add (gameObject);
				}
			}
		}
		return defaultItems;
	}

	public List<GameObject> GetCurrentItemsInBackpack() {
		return _currentItemsInBackpack;
	}

	public void RemoveItemFromBackpack(GameObject gameObject) {
		_currentItemsInBackpack.Remove(gameObject);
	}

	public void RecycleGameObject (GameObject gameObject) {
		for(int i = 0; i < _currentItemsInBackpack.Count; i++) {
			GameObject gObj = _currentItemsInBackpack[i];
			if (gObj == gameObject) {
				gObj.SetActive(false);
				if (_onRecycleItem != null)
					_onRecycleItem(i);
			}
		}
	}

	public bool GameObjectExistInBackpack (GameObject gameObject) {
		for(int i = 0; i < _currentItemsInBackpack.Count; i++) {
			GameObject gObj = _currentItemsInBackpack[i];
			if (gObj == gameObject)
				return true;
		}
		return false;
	}
	
}
