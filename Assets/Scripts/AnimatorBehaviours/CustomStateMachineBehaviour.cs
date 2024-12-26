using UnityEngine;
#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
#endif

namespace AnimatorBehaviours
{
    public abstract class CustomStateMachineBehaviour : StateMachineBehaviour
    {
    }

    #region <====================| Editors |====================>
#if UNITY_EDITOR
    
    [CustomEditor(typeof(CustomStateMachineBehaviour), true)]
    public class CustomStateMachineBehaviourEditor : Editor
    {
        private AnimationClip _previewClip;
        private float         _previewTime;
        private float         _previewTimeNormalized;

        private bool _isPreviewing;

        private void OnDisable()
        {
            AnimationMode.StopAnimationMode();
            EnforceTPose();
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            CustomStateMachineBehaviour stateBehaviour = (CustomStateMachineBehaviour)target;

            if (TryValidate(stateBehaviour, out string errorMessage))
            {
                GUILayout.Space(10);

                if (_isPreviewing)
                {
                    if (GUILayout.Button("Stop Preview"))
                    {
                        EnforceTPose();
                        _isPreviewing = false;
                        AnimationMode.StopAnimationMode();
                    }
                    else
                    {
                        _previewTimeNormalized = EditorGUILayout.Slider("Time", _previewTimeNormalized, 0f, 1f);
                        _previewTime           = (_previewClip?.length ?? 1) * _previewTimeNormalized;
                        PreviewAnimationClip(stateBehaviour);
                    }
                }
                else if (GUILayout.Button("Preview"))
                {
                    _isPreviewing = true;
                    AnimationMode.StartAnimationMode();
                }

                GUILayout.Label($"Previewing at {_previewTime:F2}s", EditorStyles.helpBox);
            }
            else
            {
                EditorGUILayout.HelpBox(errorMessage, MessageType.Info);
            }
        }

        void PreviewAnimationClip(CustomStateMachineBehaviour stateBehaviour) { AnimationMode.SampleAnimationClip(Selection.activeGameObject, _previewClip, _previewTime); }

        bool TryValidate(CustomStateMachineBehaviour stateBehaviour, out string errorMessage)
        {
            if (Application.isPlaying)
            {
                errorMessage = "Previewing is not supported in Play Mode.";
                return false;
            }

            AnimatorController animatorController = GetValidAnimatorController(out errorMessage);
            if (!animatorController) return false;

            ChildAnimatorState matchingState = animatorController
               .layers
               .SelectMany(layer => layer.stateMachine.states)
               .FirstOrDefault(state => state.state.behaviours.Contains(stateBehaviour));

            if (matchingState.state)
            {
                var animationClips = GetAnimationOnBlendTree(new List<AnimationClip>(), matchingState.state.motion);

                var idx = 0;
                if (_previewClip != null)
                    idx           = animationClips.FindIndex(child => child == _previewClip);
                else _previewClip = animationClips[idx];

                var clipIdx = EditorGUILayout.Popup("Animations", idx, animationClips
                       .Select(child => child.name)
                       .ToArray()
                    );
                if (clipIdx != idx)
                    _previewClip = animationClips[clipIdx];

                if (!_previewClip)
                {
                    errorMessage = "No valid AnimationClip found for the current state.";
                    return false;
                }
            }

            return true;
        }

        List<AnimationClip> GetAnimationOnBlendTree(List<AnimationClip> results, Motion motion)
        {
            switch (motion)
            {
                case BlendTree blendTree:
                {
                    foreach (var childMotion in blendTree.children)
                    {
                        GetAnimationOnBlendTree(results, childMotion.motion);
                    }
                    break;
                }

                case AnimationClip clip:
                    results.Add(clip);
                    break;
            }

            return results;
        }

        AnimatorController GetValidAnimatorController(out string errorMessage)
        {
            errorMessage = string.Empty;

            GameObject targetGameObject = Selection.activeGameObject;
            if (!targetGameObject || !targetGameObject.activeSelf)
            {
                errorMessage = "Please select a GameObject Actived with an Animator to preview.";
                return null;
            }

            Animator animator = targetGameObject.GetComponent<Animator>();
            if (!animator)
            {
                errorMessage = "The selected GameObject does not have an Animator component.";
                return null;
            }

            AnimatorController animatorController = animator.runtimeAnimatorController as AnimatorController;
            if (animatorController == null)
            {
                errorMessage = "The selected Animator does not have a valid AnimatorController.";
                return null;
            }

            return animatorController;
        }

        [MenuItem("GameObject/Enforce T-Pose", false, 0)]
        public static void EnforceTPose()
        {
            GameObject selected = Selection.activeGameObject;
            if (!selected || !selected.TryGetComponent(out Animator animator) || !animator.avatar) return;

            var skeletonBones = animator.avatar.humanDescription.skeleton;

            if (!animator.avatar.isHuman || skeletonBones.Length == 0) return;
            foreach (HumanBodyBones hbb in System.Enum.GetValues(typeof(HumanBodyBones)))
            {
                if (hbb == HumanBodyBones.LastBone) continue;

                Transform boneTransform = animator.GetBoneTransform(hbb);
                if (!boneTransform) continue;

                SkeletonBone skeletonBone = skeletonBones.FirstOrDefault(sb => sb.name == boneTransform.name);
                if (skeletonBone.name == null) continue;

                if (hbb == HumanBodyBones.Hips) boneTransform.localPosition = skeletonBone.position;
                boneTransform.localRotation = skeletonBone.rotation;
            }
        }
    }

#endif    
    #endregion <=============================================>
}