using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Yoma.ThirdPerson
{
    public class PlayerController : MonoBehaviour
    {
        public const string MessageUpdateCharge = "Update Charge";

        [Header("MOVEMENT REFERENCES")]
        [SerializeField]
        private float _speed = 6f;
        [SerializeField]
        private float _runSpeed = 12f;
        [SerializeField]
        private float _jumpForce = 10f;
        [SerializeField]
        private float _gravity = 2f;
        float movementSpeed;
        Vector3 _playerVelocity;

        [Header("TURNING REFERENCES")]
        [SerializeField]
        private float _turnSmoothTime = 0.1f;
        [SerializeField]
        private Transform _orientation;
        private Transform _camPos;

        [Header("COMBAT REFERENCSE")]
        [SerializeField]
        private GameObject _laughBall;
        [SerializeField]
        private float _chargeSpeed;
        [SerializeField]
        private float _currentCharge;
        [SerializeField]
        private EnemyManager _currentEnemy;

        [Header("EFFECTS")]
        [SerializeField]
        private GameObject _chargeFx;

        [Header("OPTIONAL")]
        [SerializeField]
        private bool canJump;

        [Header("ANIMATION")]
        [SerializeField]
        private Animator _anim;
        const string IDLE = "idle";
        const string WALK = "walk";
        const string RUN = "run";
        const string JUMP = "jump";
        const string CHARGE = "cast";
        const string SPELL = "spell";
        string _currentState;

        private CharacterController _controller;
        float horizontal;
        float vertical;
        bool isMove;
        bool isJump;
        bool isCharge;
        bool isAttack;

        private void Awake()
        {
            MessagingCenter.Subscribe<EnemyManager>(this, EnemyManager.MessageOnPlayerSelected, (sender) =>
            {
                _currentEnemy = sender;
                sender.SetOutline(true);
            });

            MessagingCenter.Subscribe<EnemyManager>(this, EnemyManager.MessageOnPlayerDeselected, (sender) =>
            {
                _currentEnemy = null;
            });
        }

        private void OnDestroy()
        {
            MessagingCenter.Unsubscribe<EnemyManager>(this, EnemyManager.MessageOnPlayerSelected);
            MessagingCenter.Unsubscribe<EnemyManager>(this, EnemyManager.MessageOnPlayerDeselected);
        }

        private void Start()
        {
            _controller = GetComponent<CharacterController>();
            _camPos = Camera.main.gameObject.transform;

            MessagingCenter.Send(this, MessageUpdateCharge, _currentCharge);
        }

        void Update()
        {
            InputMovement();
            Movement();
            CharacterAnimation();
            SpellCast();
        }

        void InputMovement()
        {
            horizontal = Input.GetAxisRaw("Horizontal");
            vertical = Input.GetAxisRaw("Vertical");
        }

        void Movement()
        {
            if (isCharge || isAttack) return;

            Vector3 movementInput = Quaternion.Euler(0, _camPos.eulerAngles.y, 0) * new Vector3(horizontal, 0, vertical);
            Vector3 movementDirection = movementInput.normalized;
            movementSpeed = Input.GetKey(KeyCode.LeftShift) ? _runSpeed : _speed;

            _controller.Move(movementDirection * movementSpeed * Time.deltaTime); //MOVEMENT CONTROLLER

            if (movementDirection != Vector3.zero) //ROTATION
            {
                isMove = true;
                Quaternion desiredRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, _turnSmoothTime * Time.deltaTime);
            }
            else
            {
                isMove = false;
            }

            if (canJump)
            {
                if (Input.GetButtonDown("Jump") && CheckGrounded())
                {
                    _playerVelocity.y = Mathf.Sqrt(_jumpForce * -3.0f * _gravity);
                    Debug.Log("Jump");
                }
            }

            _playerVelocity.y += _gravity * Time.deltaTime;
            _controller.Move(_playerVelocity * Time.deltaTime); //JUMP CONTROLLER
        }

        bool CheckGrounded()
        {
            return Physics.Raycast(transform.position, Vector3.down, 1f, 1 << LayerMask.NameToLayer("Ground"));
        }

        void CharacterAnimation()
        {
            if (_controller.isGrounded)
            {
                if (isMove)
                    ChangeAnimation(Input.GetKey(KeyCode.LeftShift) ? RUN : WALK);
                if (!isMove && (!isCharge || !isAttack))
                    ChangeAnimation(IDLE);
            }
            else
            {
                ChangeAnimation(JUMP);
            }
        }

        void ChangeAnimation(string newState)
        {
            if (_currentState == newState) return;

            switch (newState)
            {
                case JUMP:
                    _anim.CrossFade(newState, 0.5f);
                    break;
                default:
                    _anim.CrossFade(newState, 0.1f);
                    break;
            }

            _currentState = newState;
        }

        private void SpellCast()
        {
            if (_currentEnemy == null) return;

            if (Input.GetMouseButton(0))
            {
                isCharge = true;
                _chargeFx.SetActive(true);
                ChangeAnimation(CHARGE);

                _currentCharge += _chargeSpeed * Time.deltaTime;
                if (_currentCharge >= 100f) _currentCharge = 100f;

                MessagingCenter.Send(this, MessageUpdateCharge, _currentCharge);
            }

            if (Input.GetMouseButtonUp(0))
            {
                isCharge = false;
                _chargeFx.SetActive(false);
                if (_currentCharge >= 40f)
                {
                    StartCoroutine(SpellDeploy(_currentCharge));
                }

                _currentCharge = 0;
                MessagingCenter.Send(this, MessageUpdateCharge, _currentCharge);
            }

            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                if (isCharge) return;

                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, 100.0f))
                {
                    if (hit.transform.CompareTag("Enemy"))
                    {
                        return;
                    }

                    _currentEnemy.SetOutline(false);
                    _currentEnemy = null;
                }
            }
        }

        private IEnumerator SpellDeploy(float damage)
        {
            if (_currentEnemy == null) yield break;

            isAttack = true;
            ChangeAnimation(SPELL);

            Transform target = _currentEnemy.transform;
            GameObject ball = Instantiate(_laughBall, transform.position + transform.forward + transform.up, Quaternion.identity);

            if(damage >= 75)
            {
                ball.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            }
            else if (damage >= 100)
            {
                ball.transform.localScale = new Vector3(2, 2, 2);
            }

            while (ball != null)
            {
                ball.transform.position = Vector3.Lerp(ball.transform.position, target.position, 3 * Time.deltaTime);
                yield return null;
            }

            yield return new WaitForSeconds(0.5f);
            isAttack = false;
        }
    }
}
