﻿using OWML.Common;
using OWML.ModHelper;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace AstralCodex
{
    class Tesseract : MonoBehaviour
    {
        List<Transform> points;
        List<LineRenderer> lineRenderers;
        float time;
        List<Vector3> trackPoints;
        List<int> timeOffsets;
        GameObject fourDParticles;
        bool entered = false;

        void Awake()
        {
            //Constants
            trackPoints = new List<Vector3>() { new Vector3(-1, 1, 1), new Vector3(1, 1, 1), new Vector3(2, 2, 2), new Vector3(-2, 2, 2) };
            timeOffsets = new List<int>() { 0, 1, 1, 0, 0, 1, 1, 0, 3, 2, 2, 3, 3, 2, 2, 3 };
        }

        void Start()
        {
            points = new List<Transform>(transform.Find("Points").GetComponentsInChildren<Transform>());
            points.RemoveAt(0);
            lineRenderers = new List<LineRenderer>(GetComponentsInChildren<LineRenderer>());
            time = 0;
            fourDParticles = transform.Find("4DParticles").gameObject;
        }

        void Update()
        {
            //Ensure 4D remains visible
            if (entered == true && Camera.main!=null && (Camera.main.cullingMask & (1 << 22)) == 0) Camera.main.cullingMask += (1 << 22);

            //Animate tesseract
            time += Time.deltaTime;
            for (int i = 0; i < points.Count; ++i)
            {
                //Loop variables
                LineRenderer lr = lineRenderers[i];
                Transform p = points[i];
                Vector3[] positions = new Vector3[3];

                //Move points
                float t = (time + timeOffsets[i]) % 4;
                float tt = t % 1;
                int track = Mathf.FloorToInt(t);
                Vector3 trackStart = trackPoints[track];
                Vector3 trackEnd;
                if (track >= trackPoints.Count - 1) trackEnd = trackPoints[0];
                else trackEnd = trackPoints[track + 1];
                Vector3 newPosition = new Vector3(Mathf.Lerp(trackStart.x, trackEnd.x, tt), Mathf.Lerp(trackStart.y, trackEnd.y, tt), Mathf.Lerp(trackStart.z, trackEnd.z, tt));
                newPosition.y *= Mathf.Sign(p.transform.localPosition.y);
                newPosition.z *= Mathf.Sign(p.transform.localPosition.z);
                p.transform.localPosition = newPosition;

                //Render lines
                //Own position
                positions[1] = p.position;
                //Next position
                if ((i + 1) % 4 == 0) positions[0] = points[i - 3].position;
                else positions[0] = points[i + 1].position;
                //Connecting positions
                int connectingIndex = i + 4;
                if (connectingIndex >= points.Count) connectingIndex -= points.Count;
                positions[2] = points[connectingIndex].position;
                //Apply positions
                lr.SetPositions(positions);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            //Move into 4D
            if (other.gameObject.CompareTag("Player"))
            {
                Main.modHelper.Console.WriteLine($"ENTERED TESSERACT", MessageType.Success);
                if (entered == false)
                {
                    //Make ghost matter visible
                    Camera.main.cullingMask += (1 << 22);
                    
                    //Disable probe launcher overlay
                    Transform[] probeLauncherRenderers = GameObject.Find("Props_HEA_ProbeLauncher_ProbeCamera").GetComponentsInChildren<Transform>();
                    foreach (Transform r in probeLauncherRenderers) r.gameObject.layer = 28;

                    //Instantiate effect
                    fourDParticles.SetActive(true);

                    entered = true;
                }
            }
        }
    }
}
