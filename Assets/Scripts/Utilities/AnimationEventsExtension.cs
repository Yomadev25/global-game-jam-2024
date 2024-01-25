using UnityEngine;

public static class AnimationEventsExtension
{
    public static void AddAnimationEvent(this AnimationClip clip, float time, string functionName, int intParameter = 0, float floatParameter = 0, string stringParameter = "")
    {
        float clipDuration = clip.length;

        if (time < 0f)
        {
            Debug.LogError("Event time must be greater than 0.0f seconds");
        }
        else if (time > clipDuration)
        {
            Debug.LogError("Event time must be less than the clip's duration: " + clipDuration + "f seconds");
        }

        AnimationEvent animationEvent = new AnimationEvent();
        animationEvent.time = time;
        animationEvent.functionName = functionName;
        animationEvent.intParameter = intParameter;
        animationEvent.floatParameter = floatParameter;
        animationEvent.stringParameter = stringParameter;

        clip.AddEvent(animationEvent);
    }
}
