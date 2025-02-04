﻿using System;
using UnityEngine;
using Random = UnityEngine.Random;

//[RequireComponent(typeof(CharacterController))]
//[RequireComponent(typeof(AudioSource))]

public class PlayerCtrl : MonoBehaviour
{
    [SerializeField] private bool m_IsWalking;
    [SerializeField] public float m_WalkSpeed;
    [SerializeField] public float m_RunSpeed;
    [SerializeField] [Range(0f, 1f)] private float m_RunstepLenghten;
    [SerializeField] private float m_JumpSpeed;
    [SerializeField] private float m_StickToGroundForce;
    [SerializeField] private float m_GravityMultiplier;
    [SerializeField] private MouseLook m_MouseLook;
    [SerializeField] private bool m_UseFovKick;
    [SerializeField] private FOVKick m_FovKick = new FOVKick();
    [SerializeField] private bool m_UseHeadBob;
    [SerializeField] private CurveControlledBob m_HeadBob = new CurveControlledBob();
    [SerializeField] private LerpControlledBob m_JumpBob = new LerpControlledBob();
    [SerializeField] private float m_StepInterval;
    [SerializeField] private AudioClip[] m_FootstepSounds;    // an array of footstep sounds that will be randomly selected from.
    [SerializeField] private AudioClip m_JumpSound;           // the sound played when character leaves the ground.
    [SerializeField] private AudioClip m_LandSound;           // the sound played when character touches back on ground.

	private Camera m_Camera;
    private bool m_Jump;
    private float m_YRotation;
    private Vector2 m_Input;
    private Vector3 m_MoveDir = Vector3.zero;
    private CharacterController m_CharacterController;
    private CollisionFlags m_CollisionFlags;
    private bool m_PreviouslyGrounded;
    private Vector3 m_OriginalCameraPosition;
    private float m_StepCycle;
    private float m_NextStep;
    private bool m_Jumping;
    private AudioSource m_AudioSource;
    public static Transform myPos;

    // Animatiom
    private Animator m_Anim;
    private int hashWalk = Animator.StringToHash("isWalk");
    private bool WasWalk = false;

    // Ref
    PlayerManager m_PlayerManager;
	public Animator flashLightAnimator;
	public static bool flashLightOn;

    // Crouch
    public float crouchSpeed;
    public float crouchHeight;
    private float playerHeight;
	private int startRunSpeed = 10;
	private int startWalkSpeed = 4;
	private int isHitRunSpeed = 3;
	private int isHitWalkSpeed = 3;

	public static PlayerCtrl Instance
	{
		get
		{
			if (instance != null)
				return instance;
			instance = FindObjectOfType<PlayerCtrl>();
			return instance;
		}
	}
	private static PlayerCtrl instance;

	// Use this for initialization
	private void Start()
    {
        m_CharacterController = GetComponent<CharacterController>();
        m_Camera = Camera.main;
        m_OriginalCameraPosition = m_Camera.transform.localPosition;
        m_FovKick.Setup(m_Camera);
        m_HeadBob.Setup(m_Camera, m_StepInterval);
        m_StepCycle = 0f;
        m_NextStep = m_StepCycle / 2f;
        m_Jumping = false;
        m_AudioSource = GetComponent<AudioSource>();
        m_MouseLook.Init(transform , m_Camera.transform);
        m_Anim = GetComponent<Animator>();
        m_PlayerManager = GetComponent<PlayerManager>();
        // Crouch
        playerHeight = m_CharacterController.height;

    }


    // Update is called once per frame
    private void Update()
    {
		if (Input.GetKeyDown(KeyCode.Space))
		{
			m_WalkSpeed = 4;
			m_RunSpeed = 10;
		}

		// Power Over Whelming
		if (Input.GetKeyDown(KeyCode.P))
		{
			PlayerManager.powerOverWhelming = !PlayerManager.powerOverWhelming;
		}

		// Flash Light
		if (Input.GetKeyDown(KeyCode.V) && ItemManager.Instance.hasFlashLight)
		{
			flashLightOn = !flashLightOn;
			flashLightAnimator.SetBool("LightOn", flashLightOn);
		}

		myPos = transform;
        RotateView();
        // the jump state needs to read here to make sure it is not missed
        if (!m_Jump)
        {
            m_Jump = Input.GetButtonDown("Jump");
		}

        if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
        {
            StartCoroutine(m_JumpBob.DoBobCycle());
            PlayLandingSound();
            m_MoveDir.y = 0f;
            m_Jumping = false;
		}

        if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
        {
            m_MoveDir.y = 0f;
        }

        m_PreviouslyGrounded = m_CharacterController.isGrounded;


        float lastHeight = m_CharacterController.height;
        m_CharacterController.height = Mathf.Lerp(m_CharacterController.height, playerHeight, Time.deltaTime * 9f);
        Vector3 tmpPos = transform.position;
	
	}


	public float newYPos = -0.3f;
	bool isCollidedInfectee = true;

    private void PlayLandingSound()
    {
        m_AudioSource.clip = m_LandSound;
        m_AudioSource.Play();
        m_NextStep = m_StepCycle + .5f;
    }


    private void FixedUpdate()
    {
        float speed;
        GetInput(out speed);
        // always move along the camera forward as it is the direction that it being aimed at
        Vector3 desiredMove = transform.forward * m_Input.y + transform.right * m_Input.x;

        // get a normal for the surface that is being touched to move along it
        RaycastHit hitInfo;
        Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
                           m_CharacterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
        desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

        m_MoveDir.x = desiredMove.x * speed;
        m_MoveDir.z = desiredMove.z * speed;


        if (m_CharacterController.isGrounded)
        {
            m_MoveDir.y = -m_StickToGroundForce;

            if (m_Jump)
            {
                m_MoveDir.y = m_JumpSpeed;
                PlayJumpSound();
				m_Jump = false;
                m_Jumping = true;
            }
        }
        else
        {
            m_MoveDir += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
        }

        if( m_PlayerManager.TeleportFlag )
        {
            m_PlayerManager.TeleportFlag = false;
            transform.position = m_PlayerManager.TeleportPos;
            m_PlayerManager.TeleportPos = Vector3.zero;
        }
        else
            m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);

        ProgressStepCycle(speed);
        UpdateCameraPosition(speed);

        m_MouseLook.UpdateCursorLock();
    }


    private void PlayJumpSound()
    {
        m_AudioSource.clip = m_JumpSound;
        m_AudioSource.Play();
    }


    private void ProgressStepCycle(float speed)
    {
        if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
        {
            m_StepCycle += (m_CharacterController.velocity.magnitude + (speed * (m_IsWalking ? 1f : m_RunstepLenghten))) *
                         Time.fixedDeltaTime;
        }
        if (!(m_StepCycle > m_NextStep))
        {
            return;
        }

        m_NextStep = m_StepCycle + m_StepInterval;
        
        PlayFootStepAudio();
    }


    private void PlayFootStepAudio()
    {
        if (!m_CharacterController.isGrounded)
        {
            return;
        }
        // pick & play a random footstep sound from the array,
        // excluding sound at index 0
        int n = Random.Range(1, m_FootstepSounds.Length);
        m_AudioSource.clip = m_FootstepSounds[n];
        m_AudioSource.PlayOneShot(m_AudioSource.clip);
        // move picked sound to index 0 so it's not picked next time
        m_FootstepSounds[n] = m_FootstepSounds[0];
        m_FootstepSounds[0] = m_AudioSource.clip;
    }


    private void UpdateCameraPosition(float speed)
    {
        Vector3 newCameraPosition;
        if (!m_UseHeadBob)
        {
            return;
        }
        if (m_CharacterController.velocity.magnitude > 0 && m_CharacterController.isGrounded)
        {
            m_Camera.transform.localPosition =
                m_HeadBob.DoHeadBob(m_CharacterController.velocity.magnitude +
                                  (speed * (m_IsWalking ? 1f : m_RunstepLenghten)));
            newCameraPosition = m_Camera.transform.localPosition;
            newCameraPosition.y = m_Camera.transform.localPosition.y - m_JumpBob.Offset();
        }
        else
        {
            newCameraPosition = m_Camera.transform.localPosition;
            newCameraPosition.y = m_OriginalCameraPosition.y - m_JumpBob.Offset();
        }
        m_Camera.transform.localPosition = newCameraPosition;
    }


    private void GetInput(out float speed)
    {
        // Read input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        bool waswalking = m_IsWalking;

#if !MOBILE_INPUT
        // On standalone builds, walk/run speed is modified by a key press.
        // keep track of whether or not the character is walking or running
        m_IsWalking = !Input.GetKey(KeyCode.LeftShift);
#endif
        // set the desired speed to be walking or running
        // Crouching Speed
        speed = m_IsWalking ? m_WalkSpeed : m_RunSpeed;

        m_Input = new Vector2(horizontal, vertical);

        if (m_Input.sqrMagnitude != 0)
        {
            if (speed == m_WalkSpeed)
            {
                //m_anim.SetBool(hashWalk, true);
                WasWalk = true;
            }
        }
        else
        {
            if (WasWalk)
            {
                //m_anim.SetBool(hashWalk, false);
                WasWalk = false;
            }
        }
        // normalize input if it exceeds 1 in combined length:
        if (m_Input.sqrMagnitude > 1)
        {
            m_Input.Normalize();
        }
        //else
        //    m_anim.SetBool(hashWalk, false);

        // handle speed change to give an fov kick
        // only if the player is going to a run, is running and the fovkick is to be used
        if (m_IsWalking != waswalking && m_UseFovKick && m_CharacterController.velocity.sqrMagnitude > 0)
        {
            StopAllCoroutines();
            StartCoroutine(!m_IsWalking ? m_FovKick.FOVKickUp() : m_FovKick.FOVKickDown());
        }

        // running forward only
        if (!m_IsWalking )
        {
            speed = m_Input.y <= 0 ? m_WalkSpeed : m_RunSpeed;
            //if (transform.GetComponent<PlayerManager>().stamina <= 0) speed = m_WalkSpeed;
        }
    }

    private void RotateView()
    {
        m_MouseLook.LookRotation (transform, m_Camera.transform);
    }


    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;



		if (hit.collider.name == "Charger")
		{
			m_PlayerManager.ApplyDamage(100f);
		}


			// dont move the rigidbody if the character is on top of it
		if (m_CollisionFlags == CollisionFlags.Below)
        {
            return;
        }

        if (body == null || body.isKinematic)
        {
            return;
        }

		body.AddForce(m_CharacterController.velocity * 0.0001f, ForceMode.Impulse);
	
	}
}
