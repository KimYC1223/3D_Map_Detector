using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditableMeshController : MonoBehaviour
{
    public static EditableMeshController Instance;
    public ScanningTerrainHandler Start;
    public ScanningTerrainHandler End;
    public Material[] OriginalMaterial;
    public Material[] SpecialMaterial;
    public GameObject EmptyPrefab;
    public float PlaySpeed = 1f;
    public bool isOriginMat = false;

    private GameObject spwanedObject;
    private bool isSpinning = false;
    private Coroutine spinCoroutine = null;
    private float SpinSpeed = 20f;
    private List<List<Vector3>> pointVecStart;
    private List<List<Vector3>> pointVecEnd;
    private List<List<Vector3>> pointVecTemp;
    private int pointNum;

    public void Awake() { Instance = this; }

    public void Init() {
        if (spwanedObject != null)
            Destroy(spwanedObject);

        spwanedObject = Instantiate(EmptyPrefab, this.transform);
        spwanedObject.GetComponent<MeshFilter>().mesh = Start.EstimatedMesh;
        spwanedObject.GetComponent<MeshRenderer>().materials =
                                    ( isOriginMat ) ? OriginalMaterial : SpecialMaterial;
        pointVecStart = Start.pointVec;
        pointVecEnd = End.pointVec;

        if(pointVecStart.Count != pointVecEnd.Count ||
           pointVecEnd[0].Count != pointVecStart[0].Count) {
            Debug.LogError("Error 1");
            return;
        }

        pointVecTemp = new List<List<Vector3>>();
        for(int i = 0; i < pointVecStart.Count; i++) {
            pointVecTemp.Add(new List<Vector3>());
            for (int j = 0; j < pointVecEnd.Count; j++) {
                pointVecTemp[i].Add(new Vector3(0, 0, 0));
            }
        }
        pointNum = (int)( Mathf.Floor(( Start.Range * 2f ) / Start.Density) + 1f );
        for (int i = 0; i < pointVecTemp.Count; i++) {
            for (int j = 0; j < pointVecTemp.Count; j++) {
                pointVecStart[i][j] = new Vector3(pointVecStart[i][j].x - Start.transform.position.x + this.transform.position.x,
                                                  pointVecStart[i][j].y - Start.transform.position.y + this.transform.position.y,
                                                  pointVecStart[i][j].z - Start.transform.position.z + this.transform.position.z);

                pointVecEnd[i][j] = new Vector3(pointVecEnd[i][j].x - End.transform.position.x + this.transform.position.x,
                                                pointVecEnd[i][j].y - End.transform.position.y + this.transform.position.y,
                                                pointVecEnd[i][j].z - End.transform.position.z + this.transform.position.z);
            }
        }
    }

    public void StopSpin() {
        if (isSpinning) ModelSpin();
        spwanedObject.transform.localEulerAngles = new Vector3(0, 0, 0);
    }

    public void ModelSpin() {
        isSpinning = !isSpinning;
        if (spinCoroutine != null) { StopCoroutine(spinCoroutine); spinCoroutine = null; }
        else spinCoroutine = StartCoroutine(TurningObject());
    }

    private IEnumerator TurningObject() {
        while (true) {
            spwanedObject.transform.localEulerAngles
                = new Vector3(spwanedObject.transform.localEulerAngles.x,
                              spwanedObject.transform.localEulerAngles.y - Time.deltaTime * SpinSpeed,
                              spwanedObject.transform.localEulerAngles.z);
            yield return 0;
        }
    }

    public void ChangeMat() {
        isOriginMat = !isOriginMat;
        spwanedObject.GetComponent<MeshRenderer>().materials =
                                   ( isOriginMat ) ? OriginalMaterial : SpecialMaterial;
    }

    public void ModifyingVertices(float timeSlice) {
        for(int i = 0; i < pointVecTemp.Count; i++) {
            for(int j = 0; j < pointVecTemp.Count; j++) {
                Vector3 offset = pointVecEnd[i][j] - pointVecStart[i][j];
                pointVecTemp[i][j] = new Vector3( ( offset.x * timeSlice ) + pointVecStart[i][j].x,
                                                  ( offset.y * timeSlice ) + pointVecStart[i][j].y,
                                                  ( offset.z * timeSlice ) + pointVecStart[i][j].z);
            }
        }
    }


    public void OnTimeSliceChange(float timeSlice) {
        ModifyingVertices(timeSlice);

        int verNum = pointNum * pointNum;
        int triNum = 6 * ( pointNum - 1 ) * ( pointNum - 1 );
        Vector3[] vertices = new Vector3[verNum];
        int[] triangles = new int[triNum];
        Vector2[] uvs = new Vector2[verNum];

        for (int i = 0; i < pointNum; i++) {
            for (int j = 0; j < pointNum; j++) {
                //vertices[i * pointNum + j] = pointVec[j][i];
                vertices[i * pointNum + j] = new Vector3(pointVecTemp[i][j].x - this.transform.position.x,
                             pointVecTemp[i][j].y - this.transform.position.y,
                             pointVecTemp[i][j].z - this.transform.position.z);
                uvs[i * pointNum + j] = new Vector2(( (float)j / ( pointNum - 1 ) ), ( (float)i / ( pointNum - 1 ) ));
            }
        }

        int sqMax = ( pointNum - 1 ) * ( pointNum - 1 );
        int triIndex = 0;

        for (int sqNum = 0; sqNum < sqMax; sqNum++) {
            int LeftDown = sqNum + ( sqNum / ( pointNum - 1 ) );
            int LeftUp = LeftDown + pointNum;
            int RightDown = LeftDown + 1;
            int RightUp = LeftUp + 1;

            // Up Tri
            triangles[triIndex] = LeftUp;
            triangles[triIndex + 1] = RightUp;
            triangles[triIndex + 2] = RightDown;
            triIndex += 3;
            // DownTri
            triangles[triIndex] = LeftUp;
            triangles[triIndex + 1] = RightDown;
            triangles[triIndex + 2] = LeftDown;
            triIndex += 3;
        }

        MeshFilter targetMesh = spwanedObject.GetComponent<MeshFilter>();
        Mesh newMesh = new Mesh();
        newMesh.vertices = vertices;
        newMesh.triangles = triangles;
        newMesh.uv = uvs;
        newMesh.RecalculateBounds();
        newMesh.RecalculateNormals();
        targetMesh.mesh = newMesh;
    }
}
