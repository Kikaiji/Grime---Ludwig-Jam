using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    GameObject pauseMenu;

    AudioSource audioSource;

    CameraPos playerCamera;
    Animator animator;
    PlayerInput input;

    GameManager manager;

    bool roped = false;

    [SerializeField] float speed = 1f;
    [SerializeField] float speedMultiplier = 1f;
    [SerializeField] float rapellSpeed = .1f;
    [SerializeField] float launchMultiplier = 1f;
    [SerializeField] float grappleRange = 10f;

    bool moving = false;
    float movement = 0f;

    Rigidbody2D rb2d;

    [SerializeField]
    Transform indicator;
    [SerializeField]
    Transform reticle;

    [SerializeField] LineRenderer lineRenderer;
    List<Vector2> ropePositions = new List<Vector2>();
    bool distanceSet;
    [SerializeField] DistanceJoint2D ropeJoint;
    [SerializeField]
    GameObject ropeHingeAnchor;
    [SerializeField]
    SpriteRenderer ropeHingeSprite;

    private Dictionary<Vector2, int> wrapPointsLookup = new Dictionary<Vector2, int>();
    float rapell;
    bool rapelling = false;

    bool grounded = true;
    bool attached = false;
    bool death = false;

    bool aiming = false;
    float indMovement = 0f;
    float indDegrees = 0f;
    [SerializeField] float indSpeed = 2f;

    public Vector2 lastSafePosition = new Vector2(0, 0);

    [SerializeField]
    Timer time;
    private bool landing;

    public int deaths = 0;
    [SerializeField]
    Text deathCounter;

    private void Awake()
    {
        ropeJoint.enabled = false;
        input = new PlayerInput();
        playerCamera = GameObject.Find("Main Camera").GetComponent<CameraPos>();
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        rb2d = GetComponent<Rigidbody2D>();

        input.Player.AimLeft.performed += ctx => AimMove(ctx.ReadValue<float>());
        input.Player.AimLeft.canceled += __ => StopAim();
        input.Player.Movement.performed += ctx => Move(ctx.ReadValue<Vector2>());
        input.Player.Movement.canceled += ctx => StopMove();
        input.Player.Shoot.performed += __ => ShootGrapple();
        input.Player.Pause.performed += __ => PauseGame();

        manager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    private void OnEnable()
    {
        input.Enable();
    }
    private void OnDisable()
    {
        input.Disable();
    }

    void Update()
    {
        lastSafePosition = transform.position;
        
        indSpeed = manager.retSens * 4;

        if (rb2d.velocity.x > 0)
        { GetComponent<SpriteRenderer>().flipX = false; }
        else if (rb2d.velocity.x < 0) { GetComponent<SpriteRenderer>().flipX = true; }
        animator.SetFloat("Velocity", Mathf.Abs(rb2d.velocity.x));
        animator.SetBool("Grounded", grounded);
        animator.SetBool("Roped", roped);
        animator.SetBool("Death", death);
        if (!death && (manager.gameState == GameState.Play))
        {
            if (!roped)
            {
                lineRenderer.enabled = false;
            }
            else
            {
                Vector2 dir = (ropeHingeAnchor.transform.position - transform.position);
                var rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.x, dir.y)*180/Mathf.PI);
                ropeHingeAnchor.transform.rotation = Quaternion.identity;

                

                if (ropePositions.Count > 0)
                {
                    // 2
                    var lastRopePoint = ropePositions.Last();
                    var playerToCurrentNextHit = Physics2D.Raycast(transform.position, (lastRopePoint - new Vector2(transform.position.x, transform.position.y)).normalized, Vector2.Distance(transform.position, lastRopePoint) - 0.1f, 1 << LayerMask.NameToLayer("Ground"));

                    // 3
                    if (playerToCurrentNextHit)
                    {
                        var colliderWithVertices = playerToCurrentNextHit.collider as CompositeCollider2D;
                        List<Vector2> pathList = new List<Vector2>();
                        /*for(int i = 0; i < colliderWithVertices.pathCount; i++)
                        {
                            List<Vector2> path = new List<Vector2>();
                            foreach(Vector2 i in path)
                            {
                                pathList.Add(i);
                            }
                        }*/
                        colliderWithVertices.GetPath(GetCompositeIndexFromPoint(colliderWithVertices, playerToCurrentNextHit.point), pathList);
                        Debug.Log(playerToCurrentNextHit.collider.GetType());
                        Debug.Log(playerToCurrentNextHit.collider.GetComponent<CompositeCollider2D>().pointCount);
                        Debug.Log(colliderWithVertices.pathCount);
                        
                        //colliderWithVertices.GetPath(colliderWithVertices.pathCount - 1, pathList);

                        Debug.Log(playerToCurrentNextHit.collider.name);
                        if (colliderWithVertices != null)
                        {
                            var closestPointToHit = GetClosestColliderPointFromRaycastHit(playerToCurrentNextHit, colliderWithVertices, pathList);

                            // 4
                            if (wrapPointsLookup.ContainsKey(closestPointToHit))
                            {
                                ResetRope();
                                return;
                            }

                            // 5
                            ropePositions.Add(closestPointToHit);
                            wrapPointsLookup.Add(closestPointToHit, 0);
                            distanceSet = false;
                        }
                    }
                }
            }
            UpdateRopePositions();
            HandleRopeUnwrap();
        }
        else
        {
            ResetRope();
            grounded = false;
            
        }

            if (!grounded && !roped)
            {
                GetComponent<SpriteRenderer>().flipY = true;
                var angle = Mathf.Atan2(rb2d.velocity.x, rb2d.velocity.y) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.back);
            }
            else if (grounded || roped)
            {
                GetComponent<SpriteRenderer>().flipY = false;
                transform.eulerAngles = new Vector3(0, 0, 0);
            }
    }

    private void FixedUpdate()
    {
        if(manager.gameState == GameState.Play) deathCounter.text = (deaths.ToString());

        audioSource.volume = manager.sfxVolume * 4;
        ropeHingeSprite.GetComponent<AudioSource>().volume = manager.sfxVolume * 4;

        if (!death && (manager.gameState == GameState.Play))
        {
            rb2d.simulated = true;
            if ((Physics2D.Raycast(new Vector2(transform.position.x + .54f, transform.position.y - 0.6f), Vector2.down, .6f, 1 << LayerMask.NameToLayer("Ground")) ||
                Physics2D.Raycast(new Vector2(transform.position.x - .54f, transform.position.y - 0.6f), Vector2.down, .6f, 1 << LayerMask.NameToLayer("Ground")) ||
                Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y - 0.6f), Vector2.down, .6f, 1 << LayerMask.NameToLayer("Ground")) ||
                (Physics2D.Raycast(new Vector2(transform.position.x + .54f, transform.position.y - 0.6f), Vector2.down, .6f, 1 << LayerMask.NameToLayer("Platform")) && !rapelling) ||
                (Physics2D.Raycast(new Vector2(transform.position.x - .54f, transform.position.y - 0.6f), Vector2.down, .6f, 1 << LayerMask.NameToLayer("Platform")) && !rapelling) ||
                (Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y - 0.6f), Vector2.down, .6f, 1 << LayerMask.NameToLayer("Platform")) && !rapelling) ||
                (rb2d.velocity.y == 0f && !roped)))
            {
                grounded = true;
            }
            else
            {
                grounded = false;
            }

            if(grounded == false)
            {
                landing = true;
            }

            if(grounded == true && landing == true)
            {
                audioSource.Play();
                landing = false;
            }

            if(grounded && !moving)
            {
                rb2d.velocity /= 2;
            }
            if (!grounded)
            {
                GetComponent<CircleCollider2D>().enabled = true;
                GetComponent<BoxCollider2D>().enabled = false;
            }
            else
            {
                GetComponent<CircleCollider2D>().enabled = false;
                GetComponent<BoxCollider2D>().enabled = true;
            }

            if (grounded == false && roped == true) { attached = true; }
            if (grounded && attached) { ResetRope(); attached = false; rapelling = false; }

            Debug.DrawRay(transform.position, Vector2.up * 10f, Color.blue);
            indicator.transform.eulerAngles = new Vector3(0, 0, indDegrees);
            if (moving == true)
            {
                if (grounded) { rb2d.velocity = new Vector2((movement * speedMultiplier), rb2d.velocity.y); }
                else if (roped) { float currentspd = rb2d.velocity.y; rb2d.velocity += new Vector2((movement / 45), 0); }
                else if (!grounded && !roped) { rb2d.velocity = new Vector2(rb2d.velocity.x + (movement / 45), rb2d.velocity.y); }
            }

            if (rapelling)
            {
                if (grounded)
                {
                    if (ropeHingeAnchor.transform.position.y <= transform.position.y) { rapell = 0; }
                    else if (ropeHingeAnchor.transform.position.y > transform.position.y && rapell > 0) { rapell = 0; }
                }
                if (grounded && rapell > 0) { rapell = 0; }
                if (ropeJoint.distance > 0.5f && rapell < 0 || ropeJoint.distance < grappleRange && rapell > 0)
                    ropeJoint.distance += (rapell / 10);
            }

            if (aiming == true)
            {
                indDegrees += (indMovement);
                if (indDegrees > 360) { indDegrees -= 360; }
                if (indDegrees < 0) { indDegrees += 360; }
            }
        }
        else if(manager.gameState == GameState.Cutscene)
        {
            rb2d.simulated = true;
        }
        else if(manager.gameState == GameState.Pause)
        {
            rb2d.simulated = false;
        }
    }

    private void Move(Vector2 ctx)
    {
        moving = true;
        movement = ctx.x * speed;
  
        if(ctx.y != 0 && roped == true)
        {
            rapelling = true;
            if(ctx.y > 0) { rapell = -rapellSpeed; }
            else if (ctx.y < 0) { rapell = rapellSpeed; }
        }
        else
        {
            rapelling = false;
        }
        
    }

    private void StopMove()
    {
        if (grounded) { rb2d.velocity = new Vector2(rb2d.velocity.x / speedMultiplier, rb2d.velocity.y); }
        moving = false;
        movement = 0f;

        if (roped == true)
        {
            rapelling = false;
        }
    }

    private void AimMove(float axis)
    {
        aiming = true;
        indMovement = (-axis * indSpeed);
    }

    private void StopAim() { aiming = false; }

    private void ShootGrapple()
    {
        if (!roped && !death && (manager.gameState == GameState.Play))
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, reticle.transform.position - transform.position, grappleRange, 1 << LayerMask.NameToLayer("Ground"));
            if (hit.collider != null)
            {
                if (hit.transform.CompareTag("Grapple"))
                {
                    GetComponent<Rigidbody2D>().velocity += new Vector2(0f, 1f);
                    ropeJoint.distance = Vector3.Distance(transform.position, hit.point);
                    Debug.Log("Hit Grapple");
                    roped = true;
                    lineRenderer.enabled = true;
                    ropeHingeAnchor.GetComponent<SpriteRenderer>().enabled = false;
                    
                    if (!ropePositions.Contains(hit.point))
                    {
                        ropePositions.Add(hit.point);
                    }
                    
                    ropeJoint.enabled = true;
                    ropeHingeSprite.GetComponent<AudioSource>().Play();
                    ropeHingeSprite.enabled = true;
                    ropeHingeSprite.transform.position = new Vector3(hit.point.x, hit.point.y, -1f );
                }
                else
                {
                    Debug.Log("Missed Grapple");
                    Debug.Log(hit.transform.name);
                }
            }
        }
        else
        {
            ResetRope();
            ropeHingeSprite.GetComponent<AudioSource>().Play();
            rb2d.velocity *= launchMultiplier;
        }
    }

    private void PauseGame()
    {
        if(manager.gameState == GameState.Pause)
        {
            manager.gameState = GameState.Play;
            pauseMenu.SetActive(false);
        } else if (manager.gameState == GameState.Play)
        {
            manager.gameState = GameState.Pause;
            pauseMenu.SetActive(true);
        }
    }

    private void UpdateRopePositions()
    {
        // 1
        if (!roped)
        {
            return;
        }

        // 2
        lineRenderer.positionCount = ropePositions.Count + 1;

        // 3
        for (var i = lineRenderer.positionCount - 1; i >= 0; i--)
        {
            if (i != lineRenderer.positionCount - 1) // if not the Last point of line renderer
            {
                lineRenderer.SetPosition(i, ropePositions[i]);

                // 4
                if (i == ropePositions.Count - 1 || ropePositions.Count == 1)
                {
                    var ropePosition = ropePositions[ropePositions.Count - 1];
                    if (ropePositions.Count == 1)
                    {
                        ropeHingeAnchor.transform.position = ropePosition;
                        if (!distanceSet)
                        {
                            ropeJoint.distance = Vector2.Distance(transform.position, ropePosition);
                            distanceSet = true;
                        }
                    }
                    else
                    {
                        ropeHingeAnchor.transform.position = ropePosition;
                        if (!distanceSet)
                        {
                            ropeJoint.distance = Vector2.Distance(transform.position, ropePosition);
                            distanceSet = true;
                        }
                    }
                }
                // 5
                else if (i - 1 == ropePositions.IndexOf(ropePositions.Last()))
                {
                    var ropePosition = ropePositions.Last();
                    ropeHingeAnchor.transform.position = ropePosition;
                    if (!distanceSet)
                    {
                        ropeJoint.distance = Vector2.Distance(transform.position, ropePosition);
                        distanceSet = true;
                    }
                }
            }
            else
            {
                // 6
                lineRenderer.SetPosition(i, transform.position);
            }
        }
    }
    
    private int GetCompositeIndexFromPoint(CompositeCollider2D compCollider, Vector2 point)
    {
        int closest = -1;
        float distance = 1000f;
        //Get a list of all the points of every collider group.
        List<List<Vector2>> compositeGroups = new List<List<Vector2>>();
        for( int i = 0; i < compCollider.pathCount; i++)
        {
            List<Vector2> points = new List<Vector2>();
            compCollider.GetPath(i, points);
            compositeGroups.Add(points);
        }

        //loop through all the groups and find the closest point;
        for(int i = 0; i < compositeGroups.Count; i++)
        {
            List<Vector2> group = compositeGroups[i];
            
            foreach(Vector2 o in group)
            {
                if (Vector2.Distance(point, o) < distance)
                {
                    distance = Vector2.Distance(point, o);
                    closest = i;
                }
            }
        }
        List<Vector2> list = new List<Vector2>();
        compCollider.GetPath(0, list);
        compCollider.GetPath(1, list);
        return closest;
    }

    private Vector2 GetClosestColliderPointFromRaycastHit(RaycastHit2D hit, CompositeCollider2D polyCollider, List<Vector2> pathList)
    {
        // 2

        var distanceDictionary = pathList.ToDictionary<Vector2, float, Vector2>(
            position => Vector2.Distance(hit.point, polyCollider.transform.TransformPoint(position)),
            position => polyCollider.transform.TransformPoint(position));

        // 3
        var orderedDictionary = distanceDictionary.OrderBy(e => e.Key);
        return orderedDictionary.Any() ? orderedDictionary.First().Value : Vector2.zero;
    }

    private void ResetRope()
    {
        ropeJoint.enabled = false;
        roped = false;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, transform.position);
        ropePositions.Clear();
        ropeHingeAnchor.GetComponent<SpriteRenderer>().enabled = false;
        wrapPointsLookup.Clear();
        ropeHingeSprite.enabled = false;
    }

    private void HandleRopeUnwrap()
    {
        if (ropePositions.Count <= 1)
        {
            return;
        }
        // 1
        var anchorIndex = ropePositions.Count - 2;
        // 2
        var hingeIndex = ropePositions.Count - 1;
        // 3
        var anchorPosition = ropePositions[anchorIndex];
        // 4
        var hingePosition = ropePositions[hingeIndex];
        // 5
        var hingeDir = hingePosition - anchorPosition;
        // 6
        var hingeAngle = Vector2.Angle(anchorPosition, hingeDir);
        // 7
        var playerDir = new Vector2(transform.position.x, transform.position.y) - anchorPosition;
        // 8
        var playerAngle = Vector2.Angle(anchorPosition, playerDir);

        if (playerAngle < hingeAngle)
        {
            // 1
            if (wrapPointsLookup[hingePosition] == 1)
            {
                UnwrapRopePosition(anchorIndex, hingeIndex);
                return;
            }

            // 2
            wrapPointsLookup[hingePosition] = -1;
        }
        else
        {
            // 3
            if (wrapPointsLookup[hingePosition] == -1)
            {
                UnwrapRopePosition(anchorIndex, hingeIndex);
                return;
            }

            // 4
            wrapPointsLookup[hingePosition] = 1;
        }
    }

    private void UnwrapRopePosition(int anchorIndex, int hingeIndex)
    {
        var newAnchorPosition = ropePositions[anchorIndex];
        wrapPointsLookup.Remove(ropePositions[hingeIndex]);
        ropePositions.RemoveAt(hingeIndex);

        // 2
        ropeHingeAnchor.transform.position = newAnchorPosition;
        distanceSet = false;

        // Set new rope distance joint distance for anchor position if not yet set.
        if (distanceSet)
        {
            return;
        }
        ropeJoint.distance = Vector2.Distance(transform.position, newAnchorPosition);
        distanceSet = true;
    }

    public void KillPlayer()
    {
        Debug.Log("kill");
        playerCamera.attached = false;
        GetComponent<BoxCollider2D>().enabled = false;
        GetComponent<CircleCollider2D>().enabled = false;
        reticle.GetComponent<SpriteRenderer>().enabled = false;
        rb2d.velocity = new Vector2(Random.Range(-2, 2), Random.Range(2, 4));
        death = true;
        deaths++;
        
        StartCoroutine(Wait());
    }

    private void RevivePlayer()
    {
        GetComponent<BoxCollider2D>().enabled = true;
        death = false;
        playerCamera.attached = true;
        transform.position = new Vector3(-28, 3.1f, 0);
        rb2d.velocity = Vector2.zero;
        reticle.GetComponent<SpriteRenderer>().enabled = true;
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(5f);
        RevivePlayer();
    }
    private void OnApplicationQuit()
    {
        Debug.Log("SAVING POSITION " + transform.position + " TIME " + time.currentTime + " VELOCITY " + rb2d.velocity);
        SaveSystem.SavePlayer(GetComponent<PlayerController>(), time);
    }

}

