﻿using OWML.Common;
using OWML.ModHelper;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using NewHorizons.Utility;
using System.Collections;
using UnityEngine.UIElements;
using NewHorizons.OtherMods;
using UnityEngine.Playables;

namespace AstralCodex
{
    internal class CodexDispenser : MonoBehaviour
    {
        const string AnimatorState = "DispenseCodec";

        bool animationStarted = false;

        //Timing information
        [SerializeField] float totalDuration = 68; //Total duration of the animation
        [SerializeField] float sinkDuration = 5; //How long it takes the probe to sink into the dispenser
        [SerializeField] float probeMaxRotationSpeed = 500; //How fast the probe rotates once it's inside the dispenser
        [SerializeField] AnimationCurve probeRotationSpeedCurve; //The curve used to update the probe's rotation speed
        [SerializeField] float particleBurstTime = 29; //The time when the particle burst occurs

        SurveyorProbe probe;
        ProbeLauncher probeLauncher;
        OWRigidbody owRigidbody;
        ProbePromptReceiver probePrompt;
        Animator animator;
        GameObject addendumDialogueTrigger;

        void Start()
        {
            //Initialize callback
            Locator.GetProbe().GetAnchor().OnAnchorToSurface += ProbeAnchored;

            //Component references
            probe = Locator.GetProbe();
            probeLauncher = Locator.GetPlayerCamera().GetComponentInChildren<ProbeLauncher>();
            owRigidbody = GetComponent<OWRigidbody>();
            probePrompt = GetComponent<ProbePromptReceiver>();
            animator = transform.parent.parent.GetComponentInChildren<Animator>();
            addendumDialogueTrigger = transform.Find("CodecAddendumDialogue").gameObject;

            //Disable the addendum dialogue trigger
            addendumDialogueTrigger.SetActive(false);
        }

        void ProbeAnchored()
        {
            //Check if the probe is now a child of this object
            for (int i=0; i<transform.childCount; i++)
            {
                if (transform.GetChild(i) == probe.transform)
                {
                    StartCoroutine(nameof(CodecAnimation));
                }
            }
        }

        IEnumerator CodecAnimation()
        {
            animationStarted = true;
            Main.modHelper.Console.WriteLine("STARTING CODEC ANIMATION");

            //Prevent player from recalling scout
            probeLauncher._isRetrieving = true;

            //Disable probe launch prompt
            Destroy(probePrompt);

            //Start the animation
            animator.Play(AnimatorState);

            //Probe sinks into dispenser
            float sinkStartTime = Time.time;
            Vector3 probeStartPosition = probe.GetAnchor()._localImpactPos;
            while (Time.time - sinkStartTime < sinkDuration)
            {
                Main.modHelper.Console.WriteLine("PROBE SHOULD BE SINKING NOW");

                float t = (Time.time - sinkStartTime) / sinkDuration;
                probe.GetAnchor()._localImpactPos = Vector3.Lerp(probeStartPosition, Vector3.zero, t);

                yield return new WaitForEndOfFrame();
            }

            //Wait until the animation is complete
            yield return new WaitForSeconds(totalDuration);

            //Enable the dialogue trigger
            addendumDialogueTrigger.SetActive(true);
            InteractReceiver addendumDialogueInteractReceiver = addendumDialogueTrigger.GetComponent<InteractReceiver>();
            addendumDialogueInteractReceiver._screenPrompt._text = "<CMD> Write Addendum"; //These will need to be translated
            addendumDialogueInteractReceiver._noCommandIconPrompt._text = "Write Addendum";

            yield return null;
        }
    }
}
