using System.Collections;
using UnityEngine;

public class DoorSequence : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Animator doorAnimator;

    public void PlayFromAndClose(GameObject doorGameObject, float startAtSeconds, float playSeconds)
    {
        StartCoroutine(DisableDoorAfterSound(doorGameObject, startAtSeconds, playSeconds));
    }

    private IEnumerator DisableDoorAfterSound(GameObject doorGameObject, float startAt, float duration)
    {
        if (audioSource)
        {
            audioSource.time = Mathf.Clamp(startAt, 0f, audioSource.clip.length - 0.01f);
            audioSource.Play();
            doorAnimator.SetTrigger("Open");
        }

        yield return new WaitForSecondsRealtime(duration);

        if (audioSource) audioSource.Stop();


    }
}

