using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScanningTerrainHandler : MonoBehaviour
{
    public GameObject EstimatedObject;
    public Mesh EstimatedMesh;
    public bool isOriginMaterial;
    public bool isEstimatedDone = false;
    public GameObject TargetObject;
    public GameObject ScanEffect;
    public GameObject PointPrefabs;
    public GameObject EmptyPrefabs;
    public Material WireframeMaterial;
    public Material ZsenseMaterial;
    public Material OriginMaterial;
    public float Range = 5f;
    public float Density = 0.5f;
    public float ScanSpeed = 3f;
    public float ShowSpeed = 0.5f;

    internal List<List<Vector3>> pointVec;
    private List<List<GameObject>> objectVec;
    private int pointNum = 0;

    public void Start() {
        ScanEffect.SetActive(false);
    }

    Coroutine co1;
    Coroutine co2;

    public void Scan() {
        ResetState();

        if (Density <= 0) {
            Debug.LogError("Density는 0이하가 될 수 없습니다.");
            return;
        }
        pointNum = (int)( Mathf.Floor((Range * 2f)/Density) + 1f);
        pointVec = new List<List<Vector3>>();
        objectVec = new List<List<GameObject>>();
        for(int i = 0; i < pointNum; i++) {
            pointVec.Add(new List<Vector3>());
            objectVec.Add(new List<GameObject>());
            for (int j = 0; j < pointNum; j++) {
                pointVec[i].Add(new Vector3(this.transform.position.x-Range + Density * j, this.transform.position.y, this.transform.position.z - Range + Density * i));
                objectVec[i].Add(null);
            }
        }

        for(int i = 0; i < pointNum; i ++) {
            for(int j = 0; j < pointNum; j++) {
                RaycastHit hit;
                Vector3 shotPosition = new Vector3(this.transform.position.x  - Range + Density * j, this.transform.position.y + 5f, this.transform.position.z - Range + Density * i);
                if (Physics.Raycast(shotPosition, Vector3.down, out hit, 50f)) {
                    pointVec[i][j] = hit.point;
                    GameObject spwanedPoint = Instantiate(PointPrefabs, this.transform);
                    objectVec[i][j] = spwanedPoint;
                    spwanedPoint.transform.position = hit.point;
                    spwanedPoint.gameObject.SetActive(false);
                }
            }
        }
        co1 = StartCoroutine( MoveEffect());
    }

    public IEnumerator MoveEffect() {
        ScanEffect.SetActive(true);
        ScanEffect.transform.localPosition = new Vector3(-Range,2.5f,0f);
        ScanEffect.transform.localScale = new Vector3(0.00001f, 5, Range * 2);
        yield return new WaitForSeconds(0.3f);
        
        //while(ScanEffect.transform.position.x <= this.transform.position.x + Range) {
        //    ScanEffect.transform.position =
        //        new Vector3(ScanEffect.transform.position.x + Time.deltaTime * ScanSpeed,2.5f,0f);
        //    if(ScanEffect.transform.position.x >= pointVec[0][index].x) {
        //        try {
        //            for (int i = 0; i < pointNum; i++) {
        //                if(objectVec[i][index] != null)
        //                    objectVec[i][index].SetActive(true);
        //            }
        //        } catch(System.Exception e) { e.ToString(); }
        //        if(index < pointNum-1)
        //            index++;
        //    }
        //    yield return 0;
        //}

        for(int index = 0; index < pointNum-1; index++) {
            try {
                for (int i = 0; i < pointNum; i++) {
                    if(objectVec[i][index] != null)
                        objectVec[i][index].SetActive(true);
                }
            } catch(System.Exception e) { e.ToString(); }

            while(ScanEffect.transform.position.x < pointVec[0][index+1].x) {
                yield return 0;
                ScanEffect.transform.position =
                    new Vector3(ScanEffect.transform.position.x + Time.deltaTime * ScanSpeed, 2.5f, 0f);
            }

            ScanEffect.transform.position = new Vector3(pointVec[0][index + 1].x, 2.5f, 0f);
            yield return 0;
        }

        for (int i = 0; i < pointNum; i++)
            if (objectVec[i][pointNum-1] != null)
                objectVec[i][pointNum-1].SetActive(true);

        Debug.Log("Scan Done");
        yield return new WaitForSeconds(0.3f);
        ScanEffect.SetActive(false);
        EstimateTerrain();
    }

    public void EstimateTerrain() {
        for(int i = 0; i < pointNum; i++) {
            for(int j = 0; j < pointNum; j++) {
                Destroy(objectVec[i][j]);
            }
        }
        objectVec.Clear();

        int verNum = pointNum * pointNum;
        int triNum = 6 * ( pointNum - 1 ) * ( pointNum - 1 );
        EstimatedObject = Instantiate(EmptyPrefabs, this.transform);
        //EstimatedObject.transform.position = TargetObject.transform.position;
        Vector3[] vertices = new Vector3[verNum];
        int[] triangles = new int[triNum];
        Vector2[] uvs = new Vector2[verNum];

        for(int i = 0; i < pointNum; i ++) {
            for(int j = 0; j < pointNum; j++) {
                //vertices[i * pointNum + j] = pointVec[j][i];
                vertices[i * pointNum + j] = new Vector3(pointVec[i][j].x - this.transform.position.x,
                             pointVec[i][j].y - this.transform.position.y,
                             pointVec[i][j].z - this.transform.position.z);
                uvs[i * pointNum + j] = new Vector2(( (float) j / (pointNum - 1)), ( (float)i / ( pointNum - 1 ) ));
            }
        }

        int sqMax = ( pointNum - 1 ) * ( pointNum - 1 );
        int triIndex = 0;
        for (int sqNum = 0; sqNum < sqMax; sqNum ++) {
            int LeftDown  = sqNum + ( sqNum / ( pointNum - 1 ) );
            int LeftUp    = LeftDown + pointNum;
            int RightDown = LeftDown + 1;
            int RightUp   = LeftUp   + 1;
            //// Up Tri
            //triangles[triIndex] = RightDown;
            //triangles[triIndex+1] = RightUp;
            //triangles[triIndex+2] = LeftUp;
            //triIndex += 3;
            //// DownTri
            //triangles[triIndex] = LeftUp;
            //triangles[triIndex+1] = LeftDown;
            //triangles[triIndex+2] = RightDown;
            //triIndex += 3;

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

        MeshFilter targetMesh = EstimatedObject.GetComponent<MeshFilter>();
        Mesh newMesh = new Mesh();
        newMesh.vertices = vertices;
        newMesh.triangles = triangles;
        newMesh.uv = uvs;
        newMesh.RecalculateBounds();
        newMesh.RecalculateNormals();
        targetMesh.mesh = newMesh;

        OriginMaterial = TargetObject.GetComponent<MeshRenderer>().material;
        if(isOriginMaterial)
            EstimatedObject.GetComponent<MeshRenderer>().materials = new Material[] { OriginMaterial };
        else EstimatedObject.GetComponent<MeshRenderer>().materials = new Material[] { ZsenseMaterial, WireframeMaterial };
        EstimatedMesh = newMesh;

        co2 = StartCoroutine(ShowMesh());
    }

    IEnumerator ShowMesh() {
        EstimatedObject.transform.localPosition = new Vector3(0, 0.1f, 0);

        for(int i = 0; i < 4; i ++) {
            yield return new WaitForSeconds(0.03f);
            EstimatedObject.SetActive(false);
            yield return new WaitForSeconds(0.03f);
            EstimatedObject.SetActive(true);
        }

        while (EstimatedObject.transform.localPosition.y < 1.5f) {
            EstimatedObject.transform.localPosition = new Vector3(0, EstimatedObject.transform.localPosition.y + Time.deltaTime * ShowSpeed, 0);
            yield return 0;
        }

        isEstimatedDone = true;
    }


    public void ChangeMaterials() {
        isOriginMaterial = !isOriginMaterial;

        if (isOriginMaterial)
            EstimatedObject.GetComponent<MeshRenderer>().materials = new Material[] { OriginMaterial };
        else EstimatedObject.GetComponent<MeshRenderer>().materials = new Material[] { ZsenseMaterial, WireframeMaterial };
    }

    public void ResetState() {
        try {
            for (int i = 0; i < pointNum; i++) {
                for (int j = 0; j < pointNum; j++) {
                    Destroy(objectVec[i][j]);
                }
            }
            objectVec.Clear();
        } catch (System.Exception e) { e.ToString(); }

        ScanEffect.SetActive(false);
        ScanEffect.transform.localPosition = new Vector3(-Range, 2.5f, 0f);
        ScanEffect.transform.localScale = new Vector3(0.00001f, 5, Range * 2);
        isEstimatedDone = false;
        TargetObject.transform.localRotation = Quaternion.Euler(new Vector3(-90f, 0, 0));
        if (EstimatedObject != null)
            Destroy(EstimatedObject);
        EstimatedObject = null;
        if (pointVec != null)
            pointVec.Clear();
        pointNum = 0;

        StopAllCoroutines();
        co1 = null;
        co2 = null;
    }
}
