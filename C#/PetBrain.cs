    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class PetBrain : MonoBehaviour
    {
        [Header("TWEAK")]
        public Vector2 stateDuration;
        private float stateTimer;
        private float petDir;
        [Space(10)]
        public float eyeLength;
        public LayerMask boundaryLayer;
        public float feetRadius;
        public LayerMask groundLayer;

        [Header("FEEDING")]
        public float currSpeed;
        public float walkSpeed;
        public float runSpeed;
        [Space(10)]
        public float jumpForce;
        public float unreachableHeight;
        [Space(10)]
        public float stopThreshold = .2f;
        public float jumpThreshold = 1.5f;
        [Space(10)]
        public float detectionRadius;
        public float feedRadius;
        public LayerMask foodLayer;

        [Header("REFERENCES")]
        public EdgeCollider2D mapBoundary;
        public BasicNeeds bn;
        public Rigidbody2D rb;
        public SpriteRenderer sr;

        private Vector3 offset;
        private bool isHeld;
        private bool isGrounded;
        private bool isEating;
        private bool isWaiting;
        private GameObject currentFood;

        [HideInInspector]
        private bool hasJumped;
        private float defaultGravityScale;

        public enum State {
            Idle,
            Sleep,
            Move,
            ChaseFood
        }
        public State state;

        void Start(){
            defaultGravityScale = rb.gravityScale;
        }

        void Update(){
            HandleTouchInput();
            HandlePetMovement();
        }

        #region PETTING LOGIC
        void HandleTouchInput(){
            if(Input.touchCount > 0){
                Touch touch = Input.GetTouch(0);
                Vector3 touchPos = Camera.main.ScreenToWorldPoint(touch.position);
                touchPos.z = 0;

                if(touch.phase == TouchPhase.Began){
                    Collider2D hit = Physics2D.OverlapPoint(touchPos);
                    if(hit != null && hit.gameObject == gameObject){
                        isHeld = true;
                        offset = transform.position - touchPos;
                    }
                }
                else if(touch.phase == TouchPhase.Moved && isHeld){
                    transform.position = touchPos + offset;
                    rb.gravityScale = 0f;
                    rb.velocity = Vector2.zero;
                }
                else if(touch.phase == TouchPhase.Ended){
                    isHeld = false;
                    rb.gravityScale = defaultGravityScale;
                    rb.velocity = new Vector2(rb.velocity.x, -1f);
                }
            }
        }
        #endregion

        #region STATE LOGIC
        void HandlePetMovement(){
            if(isHeld) return;

            if(stateTimer > 0f) stateTimer -= Time.deltaTime;
            else getRandomState();

            currentFood = FindcurrentFood();
            if(currentFood != null) state = State.ChaseFood;

            Vector2 feet = new Vector2(sr.bounds.center.x, sr.bounds.min.y - .1f);
            isGrounded = Physics2D.OverlapCircle(feet, feetRadius, groundLayer);
            if(isGrounded) hasJumped = false;

            switch(state){
                case State.Idle:
                    currSpeed = 0;

                    break;
                case State.Sleep:
                    currSpeed = 0;
                    
                    break;
                case State.Move:
                    Move();
                    break;
                case State.ChaseFood:
                    ChaseFood();
                    break;
            }
        }

        void getRandomState(){
            state = (State)Random.Range(0, 3);
            stateTimer = Random.Range(stateDuration.x, stateDuration.y);
            petDir = (Random.value < .5f) ? 1 : -1;
            currSpeed = (Random.value < .5f) ? walkSpeed : runSpeed;
            eyeLength = Random.Range(1f, 3.5f);
        }

        void Move(){
            Vector2 direction = (petDir > 0) ? Vector2.right : Vector2.left;
            Vector2 face =  (petDir > 0) ? 
            new Vector2(sr.bounds.max.x + .1f, sr.bounds.center.y) : 
            new Vector2(sr.bounds.min.x - .1f, sr.bounds.center.y);

            RaycastHit2D hitFront = Physics2D.Raycast(face, direction, eyeLength, boundaryLayer);
            Debug.DrawRay(face, direction * eyeLength, Color.green);

            if(hitFront.collider != null) petDir *= -1;
            if(isGrounded) rb.velocity = new Vector2(petDir * currSpeed, rb.velocity.y);
        }
        #endregion

        #region FEEDING LOGIC
        void ChaseFood(){
            if(currentFood == null) return;

            float foodX = currentFood.transform.position.x;
            float foodY = currentFood.transform.position.y;
            float petX = transform.position.x;
            float petY = transform.position.y;

            float distanceX = Mathf.Abs(foodX - petX);
            float distanceY = Mathf.Abs(foodY - petY);

            /*Stop gerak kalau udh di threshold*/
            if(distanceX < stopThreshold){
                rb.velocity = new Vector2(0f, rb.velocity.y);
                
                /*Makan kalau reachable*/ 
                if(distanceY < jumpThreshold) StartCoroutine(EatFood());
                /*Lompat kalau makanan tinggi*/ 
                else if(distanceY < unreachableHeight && !hasJumped) TryJump();
                /*Muter kalau makanan terlalu tinggi*/ 
                else{
                    if(!isWaiting) StartCoroutine(WaitForFood());
                }
            }else{
                petDir = (foodX > petX) ? 1 : -1;
                currSpeed = runSpeed;

                if(isGrounded) rb.velocity = new Vector2(petDir * currSpeed, rb.velocity.y);
            }
        }

        void TryJump(){
            if(isGrounded){
                rb.velocity = new Vector2(rb.velocity.x, 0f);
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                hasJumped = true;
            }
        }

        IEnumerator EatFood(){
            isEating = true;

            float eatDuration = (isGrounded) ? .75f : 0f;
            float elapsedTime = 0f;

            while(elapsedTime < eatDuration){
                if(currentFood == null || Vector2.Distance(transform.position, currentFood.transform.position) > feedRadius){
                    isEating = false;
                    state = State.Idle;
                    yield break;
                }
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            if(currentFood != null){
                bn.FeedPet(currentFood.GetComponent<DraggableItem>().foodData);
                currentFood.SetActive(false);
                currentFood = null;
            }

            isEating = false;
            state = State.Idle;
        }

        IEnumerator WaitForFood(){
            isWaiting = true;

            while(currentFood != null){
                yield return null;
            }

            isWaiting = false;
        }

        GameObject FindcurrentFood(){
            Collider2D[] foodObjects = Physics2D.OverlapCircleAll(transform.position, detectionRadius, foodLayer);
            GameObject closestFood = null;
            float minDistance = Mathf.Infinity;

            foreach(Collider2D food in foodObjects){
                float distance = Vector2.Distance(transform.position, food.transform.position);
                if(distance < minDistance){
                    minDistance = distance;
                    closestFood = food.gameObject;
                }
            }

            return closestFood;
        }
        #endregion

        #region VISUALIZATIONS
        void OnDrawGizmos(){
            // Gizmos.color = Color.white;
            // Gizmos.DrawWireSphere(transform.position, detectionRadius);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * jumpThreshold);
            
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * unreachableHeight);

            Vector2 feet = new Vector2(sr.bounds.center.x, sr.bounds.min.y - .1f);
            Gizmos.DrawWireSphere(feet, feetRadius);
        }
        #endregion
    }
