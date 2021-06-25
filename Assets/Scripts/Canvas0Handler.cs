using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Canvas0Handler : MonoBehaviour {
    public ScanningTerrainHandler Scanner;

    public Transform Cube;
    public Slider XScaler;
    public Slider YScaler;
    public Animator Anim;

    internal Mesh EstiamtedMesh;
    internal GameObject OriginObject;
    internal GameObject EstimatedObject;

    private bool isSpinning = false;
    private Coroutine spinCoroutine = null;
    private float SpinSpeed = 20f;
    public bool isEditable = true;

    public void ScannButton() {
        Scanner.TargetObject.transform.localRotation = Quaternion.Euler(new Vector3(-90f, 0, 0));
        if (isSpinning) ModelSpin();
        isEditable = false;
        XScaler.interactable = isEditable;
        YScaler.interactable = isEditable;
        Scanner.Scan();
    }
    

    public void OnXScaleChange() {
        Cube.localScale = new Vector3(XScaler.value, Cube.localScale.y, Cube.localScale.z);
    }

    public void OnYScaleChange() {
        Cube.localScale = new Vector3(Cube.localScale.x, YScaler.value, Cube.localScale.z);
    }

    public void ChangeMaterial() {
        if (!Scanner.isEstimatedDone) return;

        Scanner.ChangeMaterials();
    }

    public void ResetEnv() {
        if (!Scanner.isEstimatedDone) return;
        Scanner.TargetObject.transform.localRotation = Quaternion.Euler(new Vector3(-90f, 0, 0));
        if (isSpinning)
            ModelSpin();
        isEditable = true;
        XScaler.interactable = isEditable;
        YScaler.interactable = isEditable;
        Scanner.ResetState();
    }

    public void ModelSpin() {
        if (!Scanner.isEstimatedDone) return;
        isSpinning = !isSpinning;
        if (spinCoroutine != null) { StopCoroutine(spinCoroutine); spinCoroutine = null; }
        else spinCoroutine = StartCoroutine(TurningObject());
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
        Anim.SetTrigger("Canvas1");
    }
}
