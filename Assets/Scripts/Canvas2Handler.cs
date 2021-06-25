using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Canvas2Handler : MonoBehaviour
{
    public ScanningTerrainHandler Scanner;
    public ScanningTerrainHandler Scanner_prev;

    internal Mesh EstiamtedMesh;
    internal GameObject OriginObject;
    internal GameObject EstimatedObject;

    public Animator Anim;

    private bool isSpinning = false;
    private Coroutine spinCoroutine = null;
    private float SpinSpeed = 20f;
    public void ScannButton() {
        StartCoroutine(spinningScan(isSpinning));
    }

    IEnumerator spinningScan(bool isSpinning) {
        Scanner.Range = Scanner_prev.Range;
        Scanner.Density = Scanner_prev.Density;

        if (isSpinning) ModelSpin();
        if (Scanner.EstimatedObject != null)
            Destroy(Scanner.EstimatedObject);
        Scanner.TargetObject.transform.localRotation = Quaternion.Euler(new Vector3(-90f, 0, 0));
        if (isSpinning)
            yield return new WaitForSeconds(0.1f);

        Scanner.isEstimatedDone = false;
        Scanner.Scan();
    }

    public void ChangeMaterial() {
        if (!Scanner.isEstimatedDone) return;

        Scanner.ChangeMaterials();
    }


    public void ModelSpin() {
        if (!Scanner.isEstimatedDone) return;
        isSpinning = !isSpinning;
        if (spinCoroutine != null) { StopCoroutine(spinCoroutine); spinCoroutine = null; } else spinCoroutine = StartCoroutine(TurningObject());
    }

    private IEnumerator TurningObject() {
        while (true) {
            Scanner.TargetObject.transform.localEulerAngles
                = new Vector3(Scanner.TargetObject.transform.localEulerAngles.x,
                              Scanner.TargetObject.transform.localEulerAngles.y,
                              Scanner.TargetObject.transform.localEulerAngles.z - Time.deltaTime * SpinSpeed);

            Scanner.EstimatedObject.transform.localEulerAngles
                = new Vector3(Scanner.EstimatedObject.transform.localEulerAngles.x,
                              Scanner.EstimatedObject.transform.localEulerAngles.y - Time.deltaTime * SpinSpeed,
                              Scanner.EstimatedObject.transform.localEulerAngles.z);
            yield return 0;
        }
    }

    public void NextStep() {
        Anim.SetTrigger("Canvas3");
    }

    public void PrevStep() {
        Anim.SetTrigger("Canvas1");
    }
}
