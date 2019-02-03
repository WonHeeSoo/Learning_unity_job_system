using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Collections;

public class CubeMovementJob : MonoBehaviour
{
	public int count = 3000; // 생성될 큐브의 개수
	public float speed = 20;
	public int spawnRange = 50; // 생성 될 수 있는 범위
	public bool useJob; // C# Job을 사용할지 여부

	private Transform[] transforms;
	private Vector3[] targets;
	private List<GameObject> cubes = new List<GameObject>();
	private TransformAccessArray transAccArr;
	private NativeArray<Vector3> nativeTargets;

	// 모든 작업은 구조체입니다.
	// IJob, IJobParallelFor, IJobParallelForTransform에서 상속해야 합니다.
	// 개체를 이동하게 되므로 IJobParallelForTransform에서 수행 할 수 있습니다.
	struct MovementJob : IJobParallelForTransform
	{
		public float delaTime;
		public NativeArray<Vector3> Targets;
		public float Speed;
		public void Execute(int i, TransformAccess transfrom)
		{
			transfrom.position = Vector3.Lerp(transfrom.position, Targets[i], delaTime / Speed);
		}
	}

	private MovementJob job;
	private JobHandle newJobHandle;

	private void Start()
	{
		transforms = new Transform[count];
		for (int i = 0; i < count; i++)
		{
			GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
			cubes.Add(obj);
			obj.transform.position = new Vector3(Random.Range(-spawnRange, spawnRange), 
												Random.Range(-spawnRange, spawnRange), 
												Random.Range(-spawnRange, spawnRange));
			obj.GetComponent<MeshRenderer>().material.color = Color.green;
			transforms[i] = obj.transform;
		}
		targets = new Vector3[transforms.Length];
		StartCoroutine(GenerateTargets());
	}

	private void Update()
	{
		transAccArr = new TransformAccessArray(transforms);
		nativeTargets = new NativeArray<Vector3>(targets, Allocator.TempJob);
		if (useJob == true)
		{
			job = new MovementJob();
			job.delaTime = Time.deltaTime;
			job.Targets = nativeTargets;
			job.Speed = speed;
			newJobHandle = job.Schedule(transAccArr);
		}
		else
		{
			for (int i = 0; i < transAccArr.length; i++)
				cubes[i].transform.position = Vector3.Lerp(cubes[i].transform.position, targets[i], Time.deltaTime / speed);
		}
	}

	private void LateUpdate()
	{
		newJobHandle.Complete();
		transAccArr.Dispose();
		nativeTargets.Dispose();
	}

	public IEnumerator GenerateTargets()
	{
		for (int i = 0; i < targets.Length; i++)
			targets[i] = new Vector3(Random.Range(-spawnRange, spawnRange),
									Random.Range(-spawnRange, spawnRange),
									Random.Range(-spawnRange, spawnRange));
		yield return new WaitForSeconds(2);
	}
}
