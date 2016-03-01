using UnityEngine;
using System.Collections;

public class PowerSource : GenerateElectricItem {
	public float _electricGenerationDuration = 1f;

	private bool _enable = false;
	private SpinWhenEnable spinWhenEnable;
	private float _deltaTime;
	private ContextMenu _currentContextMenu;

	void Awake () {
		spinWhenEnable = GetComponent<SpinWhenEnable>();
	}

	void Start () {
		_deltaTime = 0;
	}

	void Update () {
		if (_enable) {
			spinWhenEnable._enable = true;

			_deltaTime += Time.deltaTime;

			if (_deltaTime > _electricGenerationDuration) {
				GenerateElectric();
				_deltaTime = 0;
			}
		} else {
			spinWhenEnable._enable = false;
		}
	}

	public bool IsWorking {
		get { return _enable;}
	}

	protected override void OnTriggerEnter(Collider other) {
		if (other.tag == "Player") {
			ContextMenu contextMenu = GUIManager.Instance.CreatePowerSourceContextMenu();
			_currentContextMenu = contextMenu;
			contextMenu._onEnableButtonClicked = () => {
				_enable = !_enable;
				_currentContextMenu.Close ();
			};
		}
	}
	
	protected override void OnTriggerExit(Collider other) {
		if (other.tag == "Player") {
			if (_currentContextMenu != null) {
				_currentContextMenu.Close ();
				_currentContextMenu = null;
			}
		}
	}
}
