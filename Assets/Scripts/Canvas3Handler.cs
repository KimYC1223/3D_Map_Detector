using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Canvas3Handler : MonoBehaviour
{
    public Slider TimeLine;
    public Animator Anim;
    public EditableMeshController Controller;

    private bool playDir = true;
    private bool isAutoPlaying = false;
    private float playSpeed = 1f;

    public void OnTimeLineChange() {
        Controller.OnTimeSliceChange(TimeLine.value);
    }

    public void AutoPlayButton() {
        isAutoPlaying = !isAutoPlaying;
        TimeLine.interactable = !isAutoPlaying;
    }

    public void ReversePlayDir() {
        playDir = !playDir;
    }

    public void ChangeMat() {
        Controller.ChangeMat();
    }

    public void RotateObj() {
        Controller.ModelSpin();
    }

    public void ResetButton() {
        isAutoPlaying = false;
        TimeLine.interactable = true;
        playDir = true;
        Controller.StopSpin();
    }

    public void Update() {
        if(isAutoPlaying) {
            TimeLine.value += ( ( playDir ) ? 1 : -1 ) * ( Time.deltaTime * playSpeed );
            if(playDir) {
                if (TimeLine.value >= 1f) TimeLine.value = 0;
            } else {
                if (TimeLine.value <= 0f) TimeLine.value = 1f;
            }
        }
    }

    public void PrevStep() {
        Anim.SetTrigger("Canvas2");
    }
}
