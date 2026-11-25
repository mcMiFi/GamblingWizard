using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterController))]
public sealed class PlayerAudioController : MonoBehaviour
{
    [Header("Background music")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.3f;

    [Header("Jump")]
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] [Range(0f, 1f)] private float jumpVolume = 0.65f;

    [Header("Footsteps")]
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] [Range(0f, 1f)] private float footstepVolume = 0.55f;
    [SerializeField] [Min(0.1f)] private float walkingStepInterval = 0.48f;
    [SerializeField] [Min(0.1f)] private float sprintingStepInterval = 0.34f;
    [SerializeField] [Min(0.1f)] private float crouchingStepInterval = 0.62f;

    private AudioSource musicSource;
    private AudioSource footstepSource;
    private float timeUntilNextStep;
    private int previousFootstepIndex = -1;
    private bool footstepsConfigured;

    private void Awake()
    {
        musicSource = CreateAudioSource();
        musicSource.clip = backgroundMusic;
        musicSource.loop = true;
        musicSource.volume = musicVolume;

        footstepSource = CreateAudioSource();

        if (backgroundMusic == null)
            Debug.LogError("PlayerAudioController requires a background music clip.", this);
        else
            musicSource.Play();

        if (jumpClip == null)
            Debug.LogError("PlayerAudioController requires a jump sound clip.", this);

        footstepsConfigured = footstepClips != null && footstepClips.Length > 0;
        if (footstepsConfigured)
        {
            foreach (AudioClip clip in footstepClips)
            {
                if (clip == null)
                {
                    footstepsConfigured = false;
                    break;
                }
            }
        }

        if (!footstepsConfigured)
            Debug.LogError("PlayerAudioController requires a non-empty array of valid footstep clips.", this);
    }

    public void PlayJump()
    {
        if (jumpClip != null)
            footstepSource.PlayOneShot(jumpClip, jumpVolume);
    }

    public void ProcessMovementAudio(
        bool isMoving,
        bool isGrounded,
        bool isSprinting,
        bool isCrouching)
    {
        if (!isMoving || !isGrounded)
        {
            timeUntilNextStep = 0f;
            return;
        }

        if (!footstepsConfigured)
            return;

        timeUntilNextStep -= Time.deltaTime;
        if (timeUntilNextStep > 0f)
            return;

        int clipIndex = GetNextFootstepIndex();
        AudioClip clip = footstepClips[clipIndex];
        if (clip == null)
            return;

        footstepSource.PlayOneShot(clip, footstepVolume);
        timeUntilNextStep = isCrouching
            ? crouchingStepInterval
            : isSprinting ? sprintingStepInterval : walkingStepInterval;
    }

    private AudioSource CreateAudioSource()
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        return source;
    }

    private int GetNextFootstepIndex()
    {
        if (footstepClips.Length == 1)
            return 0;

        int index = Random.Range(0, footstepClips.Length);
        if (index == previousFootstepIndex)
            index = (index + 1) % footstepClips.Length;

        previousFootstepIndex = index;
        return index;
    }
}
