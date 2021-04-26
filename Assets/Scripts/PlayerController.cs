using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float turnSpeed, distToGround, speed, jumpForce, airSpeed, deathHeigth, playerMaxHealth,potionMaxAmount;
    [SerializeField] private Rigidbody rig;
    [SerializeField] private GameObject face;
    [SerializeField] private LayerMask ground,finshLayer;
    [SerializeField] private Image healthBar, overlayDeath;
    [SerializeField] private Text potionText,deathText,deathText2;
    [SerializeField] private Animator playerAnim;

    private Vector3 lastVelocity;
    private float horizontal, vertical, jumpHeigth, fallDistance, playerCurrentHealth,potionCurrentAmount,time;
    private bool jumpRequest, inAir,jumped, grounded,dead,drinking,won,ended;
    






    // Start is called before the first frame update
    void Start()
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
            }

            

            if (!grounded)
                playerAnim.SetBool("Jump", true);

        }
        grounded = GroundCheck();

        if(!dead)
            won = FinishCheck();
    }

    void Update()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

       

        if (won && !ended)
            StartCoroutine(WinText());


        if (!drinking)
        {

         

            if (Input.GetButtonDown("Fire1"))
            {
                if (potionCurrentAmount > 0 && grounded)
                    DrinkPotion();
            }

            if (Input.GetButtonDown("Jump"))
            {
                if (grounded)
                {
                    playerAnim.SetBool("Jump", true);
                    jumpRequest = true;
                    grounded = false;
                }
            }

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
        {
            if(won || dead)
                ReloadScene();
        }

        UpdateUI();
    }

    private void Death()
    {
        if (!dead)
        {
            Camera.main.transform.parent = null;
            StartCoroutine(DeathText());
        }
    }

    IEnumerator WinText()
    {
        ended = true;
        deathText.text = "YOU WON";
        deathText2.text = "YOUR TIME: " + Mathf.RoundToInt(Time.timeSinceLevelLoad).ToString() + " SECONDS";
        deathText.enabled = true;
        deathText2.enabled = true;
        float alpha = 0;
        Color white = Color.white;
        Color black = Color.black;

        while (true)
        {

            alpha += 0.0005f;
            overlayDeath.rectTransform.localScale = Vector3.one * (1 + alpha);
            deathText.rectTransform.localScale = Vector3.one * (1 + alpha);
            overlayDeath.color = new Color(black.r, black.g, black.b, alpha / 1.5f);
            deathText.color = new Color(white.r, white.g, white.b, alpha);
            deathText2.color = new Color(white.r, white.g, white.b, alpha);

            yield return new WaitForEndOfFrame();



        }

    }

    IEnumerator DeathText()
    {
        dead = true;
        deathText.enabled = true;
        deathText2.enabled = true;
        float alpha = 0;
        Color white = Color.white;
        Color black = Color.black;

        while (true)
        {

            alpha += 0.0007f;
            overlayDeath.rectTransform.localScale = Vector3.one * (1 + alpha);
            deathText.rectTransform.localScale = Vector3.one * (1+alpha);
            
            overlayDeath.color = new Color(black.r, black.g, black.b, alpha / 1.5f);
            deathText.color = new Color(white.r, white.g, white.b, alpha);
            deathText2.color = new Color(white.r, white.g, white.b, alpha);

            yield return new WaitForEndOfFrame();



        }

    }

    private void DrinkPotion()
    {

        rig.velocity = Vector3.zero;
        
        StartCoroutine(drinkingDelay());

    }

    private void ChangeHealth(float changeAmount)
    {
        
        playerCurrentHealth += changeAmount;
        playerCurrentHealth = Mathf.Clamp(playerCurrentHealth, -10, playerMaxHealth);

        if (playerCurrentHealth < 0)
            Death();

    }

    private void UpdateUI()
    {
        float newHealth = playerCurrentHealth / playerMaxHealth;
        healthBar.fillAmount = Mathf.Lerp(healthBar.fillAmount, newHealth, 0.1f);
        potionText.text = potionCurrentAmount.ToString();

    }

    private void ReloadScene()
    {

        SceneManager.LoadScene(0);
    }

    private void CheckDeathHeight()
    {

        fallDistance = Mathf.Abs(transform.position.y - jumpHeigth);

        if (fallDistance >= deathHeigth)
        {
            Death();
        }


    }

    bool FinishCheck()
    {
        bool won = false;
        Collider[] hits = Physics.OverlapSphere(transform.position + Vector3.down * distToGround, 0.35f, finshLayer);
        if (hits.Length > 0)
        {
            won = true;
        }

        return won;
    }

    bool GroundCheck()
    {
        bool grd = false;
        Collider[] hits = Physics.OverlapSphere(transform.position + Vector3.down * distToGround, 0.35f, ground);
        if(hits.Length > 0)
        {
            grd = true;
        }

        return grd;
    }

    void Jump()
    {
        if (!jumped)
        {
            rig.velocity = new Vector3(rig.velocity.x, 0, rig.velocity.z);
            rig.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
            StartCoroutine(jumpDelay());
           // playerAnim.SetBool("Jump", true);
        }
        
    }

    IEnumerator jumpDelay()
    {
        jumped = true;
        yield return new WaitForSeconds(0.5f);
        jumped = false;
    }

    IEnumerator drinkingDelay()
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

    bool CheckInput()
    {
        bool check = false;
        if (Mathf.Abs(horizontal) + Mathf.Abs(vertical) >= 0.1f)
            check = true;

        return check;
    }

    void TurnPlayer()
    {
        face.transform.rotation = Quaternion.Slerp(face.transform.rotation, Quaternion.LookRotation(Direction()), turnSpeed);
        
    }

    void Land()
    {
        


        if (!jumped)
        {
            inAir = false;
            playerAnim.SetBool("Jump", false);
          
        }

        if (fallDistance > 7)
        {
            ChangeHealth(7-Mathf.Pow(fallDistance,1.2f));
        }
        jumpHeigth = transform.position.y;
      
    }

    void MovePlayer()
    {
        if (grounded)
        {
            if (inAir)
                Land();

            rig.velocity = MoveSpeed(rig.velocity);
            lastVelocity = MoveSpeed(rig.velocity);
            

        } else
        { 
            
            inAir = true;
            rig.velocity = MoveSpeedAir(rig.velocity);
        }


        

    }



    Vector3 MoveSpeed(Vector3 velocity)
    {
        return new Vector3 (Direction().x*speed,velocity.y,Direction().z*speed);
    }


    Vector3 MoveSpeedAir(Vector3 velocity)
    {

        return new Vector3(Mathf.Lerp(lastVelocity.x,Direction().x*speed,airSpeed), velocity.y, Mathf.Lerp(lastVelocity.z, Direction().z*speed, airSpeed));
    }


    Vector3 Direction()
    {
        Vector3 movementX = Camera.main.transform.right * horizontal;
        Vector3 movementZ = Camera.main.transform.forward * vertical;
        Vector3 movement = movementX + movementZ;
        movement = new Vector3(movement.x, 0, movement.z).normalized;

        return movement;
    }

    /*
    Vector3 MoveSpeedAirAngle(Vector3 velocity)
    {

        float angle = Vector3.Angle(new Vector3(lastVelocity.x, 0, lastVelocity.z), face.transform.forward);

        airSpeed = AirSpeedCalculator(angle);

        return new Vector3(Direction().x * airSpeed, velocity.y, Direction().z * airSpeed);
    }

    float AirSpeedCalculator(float angle)
    {
        float speedy = speed / (180 / angle);

        speedy = speed - speedy;
        speedy = Mathf.Clamp(speedy, 0.5f, speed);

        return speedy;
    }
    */
}
