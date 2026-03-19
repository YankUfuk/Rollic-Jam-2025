using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace __Scripts
{
	/**
 * Sample script to demonstrate how you can initialize a cube of a specific size and expose/listen to different cube events.
 */
	public class CustomRubikCubeInitializer : MonoBehaviour
	{

		[Header("General")] [SerializeField] private CustomRubikCube rubikCubePrefab;
    
		[Tooltip("How many rows and columns should the cube have by default?")]
		[SerializeField] private int dimensions = 3;

		[Tooltip("Which prefab do we use for each cublet? Note that no matter whether we are using a centerpiece, edgepiece or cornerpiece, we use the same prefab for all of them.")]
		[SerializeField] private GameObject cubletPrefab;
		[Tooltip("Normally the side pieces of a real cube don't have stickers, would you like to hide those here as well?")]
		[SerializeField] private bool hideInvisibleSides = true;
		[Tooltip("What is our max history size? Set to 0 to disable the history.")]
		[SerializeField] private int maxHistorySize = 50;
		
		[Header("Pipe System")]
		
		[SerializeField] private PipeData[] pipeData;

		[Header("Shuffle settings")]

		[SerializeField] private bool shuffleOnStart = true;
		[SerializeField] private int shuffleCount = 20;
		[SerializeField] private float shuffleSpeed = 5;

		//reference to the actual cube and disc rotation script so that we can disable it when we solved the cube
		private CustomRubikCube _rubikCube;
		private CustomDiscRotator _discRotator;

		[Header("Respawn settings")]
		[Tooltip("Can we press the 2,3,4 - 9 keys to spawn a new cube of a different size?")]
		[SerializeField] private bool allowRespawning = true;
		[Tooltip("Which cube sizes are allowed?")]
		[SerializeField] private Vector2 minMaxCubeSize = new Vector2(2, 9);
		[Tooltip("How many units should we zoom in or out extra per cube unit? (We zoom out as we increase the cube size to make sure it still fits on the screen.)")]
		[SerializeField] private float zoomFactorPerUnit = 1;
		[SerializeField] private CameraMouseOrbit cameraMouseOrbit;
		private float _baseDistance;

		[Header("Debug settings")]
		[SerializeField] private AxisDisplay axisDisplay;

		[Header("Debug settings")]
		[SerializeField] private Text debugText;

		//allow generic event handling
		[Serializable]
		public class CustomRubikCubeEvent : UnityEvent<CustomRubikCube> { }

		[Header("Cube events")]

		public CustomRubikCubeEvent onNewCubeBeforeInitialize;
		public CustomRubikCubeEvent onNewCubeAfterInitialize;
		public CustomRubikCubeEvent onCurrentCubeBeforeDestroy;
		public UnityEvent onCurrentCubeAfterDestroy;

		//called when any disc changes on the cube
		public CustomRubikCubeEvent onCubeChanged;
		public CustomRubikCubeEvent onCubeSolved;

		// Start is called before the first frame update
		void Start()
		{
			if (cameraMouseOrbit != null)
			{
				//set up the basic zoom distance
				_baseDistance = cameraMouseOrbit.targetDistance - dimensions * zoomFactorPerUnit;
			}

			SpawnNewCube(dimensions);
		}

		private void SpawnNewCube(int dims)
		{
			DestroyCurrentCubeIfPresent();

			_rubikCube = Instantiate(rubikCubePrefab, transform);
			_discRotator = _rubikCube.GetComponent<CustomDiscRotator>();

			//make sure we show the local axis of the Rubik's cube in the top right
			if (axisDisplay != null) axisDisplay.copyFrom = _rubikCube.transform;

			onNewCubeBeforeInitialize?.Invoke(_rubikCube);
        
			_rubikCube.Initialize(dims, cubletPrefab, maxHistorySize, hideInvisibleSides);
			
			foreach (var t in pipeData)
			{
				_rubikCube.Test(t.PipePrefab, t.PipePosX, t.PipePosY, t.PipePosZ, t.Rotation);
			}
			
			StartCoroutine(SetupCubeCoroutine());

			if (cameraMouseOrbit != null)
			{
				cameraMouseOrbit.targetDistance = _baseDistance + dims * zoomFactorPerUnit;
			}
		}

		private void DestroyCurrentCubeIfPresent()
		{
			if (_rubikCube != null)
			{
				_rubikCube.OnChanged -= OnCubeChangedCallback;
				_rubikCube.OnSolved -= OnCubeSolvedCallback;

				onCurrentCubeBeforeDestroy?.Invoke(_rubikCube);
				Destroy(_rubikCube.gameObject);
				onCurrentCubeAfterDestroy?.Invoke();
			}
		}

		private IEnumerator SetupCubeCoroutine()
		{
			yield return _rubikCube.ShuffleCoroutine(shuffleOnStart?shuffleCount:0, shuffleSpeed);
			_rubikCube.OnChanged += OnCubeChangedCallback;
			_rubikCube.OnSolved += OnCubeSolvedCallback;

			onNewCubeAfterInitialize?.Invoke(_rubikCube);
		}

		private void OnCubeChangedCallback()
		{
			Debug.Log("Cube changed");
			onCubeChanged?.Invoke(_rubikCube);
		}

		private void OnCubeSolvedCallback()
		{
			Debug.Log("Cube solved");
			onCubeSolved?.Invoke(_rubikCube);
		}

		private void OnDrawGizmos()
		{
			Gizmos.DrawCube(transform.position, Vector3.one * dimensions * 0.9f);
		}

		public void SetDiscRotationEnabled (bool pAllowDiscRotation)
		{
			if (_rubikCube != null)
			{
				_rubikCube.GetComponent<CustomDiscRotator>().enabled = pAllowDiscRotation;
			}
		}

		public void SetCubeRotationEnabled(bool pAllowCubeRotation)
		{
			if (_rubikCube != null)
			{
				_rubikCube.GetComponent<CustomCubeRotator>().enabled = pAllowCubeRotation;
			}
		}

		private void Update()
		{
			if (debugText != null && _discRotator != null)
			{
				debugText.text = _discRotator.GetDebugInfo() + "\n" + Application.platform;

			}
        
			if (allowRespawning && Input.anyKeyDown && Input.inputString.Length == 1)
			{
				//Turn '0', '1', etc. into 0, 1, etc.
				int value = Input.inputString[0] - '0';
				if (value >= minMaxCubeSize.x && value <= minMaxCubeSize.y)
				{
					SpawnNewCube(value);
				}
			}
		}

	}
	
	[Serializable]
	public class PipeData
	{
		[field: SerializeField] public GameObject PipePrefab { get; private set; }
		[field: SerializeField] public int PipePosX { get; private set; }
		[field: SerializeField] public int PipePosY { get; private set; }
		[field: SerializeField] public int PipePosZ { get; private set; }
		[field: SerializeField] public Quaternion Rotation { get; private set; }
	}
}
