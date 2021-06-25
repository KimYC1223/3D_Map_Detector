using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Canvas1Handler : MonoBehaviour
{
    public ScanningTerrainHandler Scanner;

    internal Mesh EstiamtedMesh;
    internal GameObject OriginObject;
    internal GameObject EstimatedObject;

    public Slider RangeSlider;
    public Slider DensitySlider;
    public TMP_Text RangeText;
    public TMP_Text DensityText;
    public Animator Anim;

    private bool isSpinning = false;
    private Coroutine spinCoroutine = null;
    private float SpinSpeed = 20f;
    private bool editable = true;
    public void ScannButton() {
        StartCoroutine(spinningScan(isSpinning));
    }

    IEnumerator spinningScan(bool isSpinning) {
        if(isSpinning) ModelSpin();
        if (Scanner.EstimatedObject != null)
            Destroy(Scanner.EstimatedObject);
        Scanner.TargetObject.transform.localRotation = Quaternion.Euler(new Vector3(-90f, 0, 0));
        if(isSpinning)
            yield return new WaitForSeconds(0.1f);

        editable = false;
        Scanner.isEstimatedDone = false;
        Scanner.Scan();
        RangeSlider.interactable = false;
        DensitySlider.interactable = false;
    }

    public void ChangeMaterial() {
        if (!Scanner.isEstimatedDone) return;

        Scanner.ChangeMaterials();
    }


    public void Update() {
        if(!editable && Scanner.isEstimatedDone) {
            editable = true;
            RangeSlider.interactable = true;
            DensitySlider.interactable = true;
        }
    }
    public void OnRangeSliderChange() {
        Scanner.Range = RangeSlider.value;
        DensitySlider.maxValue = RangeSlider.value;
        RangeText.text = RangeSlider.value.ToString();
        DensityText.text = "";
        string str = DensitySlider.value.ToString();
        for (int i = 0; i < str.Length && i < 4; i++)
            DensityText.text += str[i];
    }

    public void OnDensitySliderChange() {
        Scanner.Density = DensitySlider.value;
        DensityText.text = "";
        string str = DensitySlider.value.ToString();
        for (int i = 0; i < str.Length && i < 4; i++)
            DensityText.text += str[i];
    }

    public void ModelSpin() {
        if (!Scanner.isEstimatedDone) return;
        isSpinning = !isSpinning;
        if (spinCoroutine != null) { StopCoroutine(spinCoroutine); spinCoroutine = null; }
        else spinCoroutine = StartCoroutine(TurningObject());
    }

    private IEnumerator TurningObject() {
        while(true) {
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
        Anim.SetTrigger("Canvas2");
    }
}
