using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graphics : MonoBehaviour
{
    void Start()
    {
        QualitySettings.vSyncCount = 1;
        QualitySettings.maxQueuedFrames = 3;
    }
}
