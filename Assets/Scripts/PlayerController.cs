using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// ReSharper disable IteratorNeverReturns

namespace Assets.Scripts
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private GameObject face;
        [SerializeField] private LayerMask ground, finshLayer;
        [SerializeField] private Image healthBar, overlayDeath;
        private float horizontal, vertical, jumpHeight, fallDistance, playerCurrentHealth, potionCurrentAmount, time;
        private bool jumpRequest, inAir, jumped, grounded, dead, drinking, won, ended, drinkRequest;

        private Vector3 lastVelocity;
        [SerializeField] private AudioClip ughSound;
        [SerializeField] private AudioSource ughSource;
        [SerializeField] private Animator playerAnim;
        [SerializeField] private Text potionText, deathText, deathText2;
        [SerializeField] private Rigidbody rig;
        [SerializeField] private float turnSpeed,
            distToGround,
            speed,
            jumpForce,
            airSpeed,
            deathHeight,
            playerMaxHealth,
            potionMaxAmount;

        // Start is called before the first frame update
        private void Start()
        {
            playerCurrentHealth = playerMaxHealth;
            potionCurrentAmount = potionMaxAmount;
           
        }

        // Update is called once per frame
        private void FixedUpdate()
        {
            if (!drinking)
            {
                if (!dead)
                {
                    MovePlayer();

                    if (jumpRequest)
                    {
                        Jump();
                        jumpRequest = false;
                    }

                   
                    if (drinkRequest)
                    {
                        DrinkPotion();
                        drinkRequest = false;
                    }
                }


                if (!grounded)
                    playerAnim.SetBool("Jump", true);
            }

            grounded = GroundCheck();

            if (!dead)
                won = FinishCheck();
        }

        private void Update()
        {
            
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");


                if (won && !ended)
            {
                StartCoroutine(TextOverlay("YOU WIN",
                    "YOUR TIME: " + Mathf.RoundToInt(Time.timeSinceLevelLoad) + " SECONDS"));
                ended = true;
            }

            if (!drinking)
            {
                if (Input.GetButtonDown("Fire1"))
                    if (potionCurrentAmount > 0 && grounded)
                        drinkRequest = true;

                if (Input.GetButtonDown("Jump"))
                    if (grounded)
                        jumpRequest = true;

                CheckDeathHeight();


                if (CheckInput())
                {
                    TurnPlayer();
                    playerAnim.SetFloat("Speed", 1);
                }
                else
                {
                    playerAnim.SetFloat("Speed", 0);
                }
            }

            if (Input.GetButtonDown("Restart"))
                if (won || dead)
                    ReloadScene();

            UpdateUi();
        }

        private void Death()
        {
            Camera.main.transform.parent = null;
            StartCoroutine(TextOverlay("YOU DIED", "PRESS(R) TO RESTART"));
            dead = true;
        }


        private IEnumerator TextOverlay(string textMain, string textSec)
        {

            deathText.text = textMain;
            deathText2.text = textSec;
            float alpha = 0;
            float size = 1;
            var white = Color.white;
            var black = Color.black;

            while (true)
            {
                size = Mathf.Lerp(size, 1.5f, 0.001f);
                alpha = Mathf.Lerp(alpha, 0.5f, 0.001f);
                overlayDeath.rectTransform.localScale = Vector3.one * size;
                deathText.rectTransform.localScale = Vector3.one * size;
                overlayDeath.color = new Color(black.r, black.g, black.b, alpha);
                deathText.color = new Color(white.r, white.g, white.b, alpha*2);
                deathText2.color = new Color(white.r, white.g, white.b, alpha*2);

                yield return new WaitForEndOfFrame();
            }
            // ReSharper disable once IteratorNeverReturns
        }

        private void DrinkPotion()
        {
            if (potionCurrentAmount <= 0 || !grounded || drinking) return;

            rig.velocity = Vector3.zero;
            StartCoroutine(DrinkingDelay());
        }

        private void ChangeHealth(float changeAmount)
        {
            playerCurrentHealth += changeAmount;
            playerCurrentHealth = Mathf.Clamp(playerCurrentHealth, -10, playerMaxHealth);

            if (playerCurrentHealth < 0 && !dead)
                Death();
        }

        private void UpdateUi()
        {
            var newHealth = playerCurrentHealth / playerMaxHealth;
            healthBar.fillAmount = Mathf.Lerp(healthBar.fillAmount, newHealth, 0.1f);
            potionText.text = potionCurrentAmount.ToString(CultureInfo.InvariantCulture);
        }

        private static void ReloadScene()
        {
            SceneManager.LoadScene(0);
        }

        private void CheckDeathHeight()
        {
            fallDistance = Mathf.Abs(transform.position.y - jumpHeight);

            if (fallDistance > deathHeight && !dead) Death();
        }

        private bool FinishCheck()
        {
            var finishCheck = false;
            var hits = Physics.OverlapSphere(transform.position + Vector3.down * distToGround, 0.35f, finshLayer);
            if (hits.Length > 0) finishCheck = true;

            return finishCheck;
        }

        private bool GroundCheck()
        {
            var grd = false;
            var hits = Physics.OverlapSphere(transform.position + Vector3.down * distToGround, 0.35f, ground);
            if (hits.Length > 0) grd = true;

            return grd;
        }

        private void Jump()
        {

            if (jumped) return;
            playerAnim.SetBool("Jump", true);
            grounded = false;
            rig.velocity = new Vector3(rig.velocity.x, 0, rig.velocity.z);
            rig.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
            StartCoroutine(JumpDelay());
            // playerAnim.SetBool("Jump", true);
        }

        private IEnumerator JumpDelay()
        {
            jumped = true;
            yield return new WaitForSeconds(0.5f);
            jumped = false;
        }

        private IEnumerator DrinkingDelay()
        {
            drinking = true;
            playerAnim.SetBool("Drink", true);
            yield return new WaitForSeconds(0.75f);
            ChangeHealth(20);
            potionCurrentAmount -= 1;
            yield return new WaitForSeconds(0.75f);
            playerAnim.SetBool("Drink", false);
            drinking = false;
        }

        private bool CheckInput()
        {
            var check = Mathf.Abs(horizontal) + Mathf.Abs(vertical) >= 0.1f;

            return check;
        }

        private void TurnPlayer()
        {
            face.transform.rotation =
                Quaternion.Slerp(face.transform.rotation, Quaternion.LookRotation(Direction()), turnSpeed);
        }

        private void Land()
        {
            if (!jumped)
            {
                inAir = false;
                playerAnim.SetBool("Jump", false);
            }

            if (fallDistance > 7)
            {
                ughSource.PlayOneShot(ughSound);
                ChangeHealth(7 - Mathf.Pow(fallDistance, 1.2f));
            }

            jumpHeight = transform.position.y;
            
            
        }

        private void MovePlayer()
        {
            if (grounded)
            {
                if (inAir)
                    Land();

                rig.velocity = MoveSpeed(rig.velocity);
                lastVelocity = MoveSpeed(rig.velocity);
            }
            else
            {
                inAir = true;
                rig.velocity = MoveSpeedAir(rig.velocity);
            }
        }


        private Vector3 MoveSpeed(Vector3 velocity)
        {
            return new Vector3(Direction().x * speed, velocity.y, Direction().z * speed);
        }


        private Vector3 MoveSpeedAir(Vector3 velocity)
        {
            return new Vector3(Mathf.Lerp(lastVelocity.x, Direction().x * speed, airSpeed), velocity.y,
                Mathf.Lerp(lastVelocity.z, Direction().z * speed, airSpeed));
        }


        private Vector3 Direction()
        {
            var movementX = Camera.main.transform.right * horizontal;
            var movementZ = Camera.main.transform.forward * vertical;
            var movement = movementX + movementZ;
            movement = new Vector3(movement.x, 0, movement.z).normalized;

            return movement;
        }
    }
}