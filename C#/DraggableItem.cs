using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraggableItem : MonoBehaviour
{
    public FoodItem foodData;

    private Vector3 offset;
    private bool isDragging = false;

    private Rigidbody2D rb;
    private Collider2D currPetCollider;

    [HideInInspector]
    private float defaultGravityScale;

    void Start(){
        rb = GetComponent<Rigidbody2D>();

        defaultGravityScale = rb.gravityScale;
    }

    void Update(){
        if(Input.touchCount > 0){
            Touch touch = Input.GetTouch(0);
            Vector3 touchPos = Camera.main.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, Camera.main.nearClipPlane));
            //Seinget gw, panggil Camera.main itu expensive, tapi gw keknya pernah liat post ada yang bilang unity udh improve performance nya

            switch(touch.phase){    
                case TouchPhase.Began:
                    if(IsTouchingObject(touchPos)){
                        isDragging = true;
                        offset = transform.position - touchPos;
                    }

                    break;

                case TouchPhase.Moved:
                    if(isDragging) transform.position = touchPos + offset;
                    rb.gravityScale = 0f;
                    rb.velocity = Vector2.zero;

                    break;

                case TouchPhase.Ended:
                    if(isDragging && currPetCollider != null) FeedPet();
                    rb.gravityScale = defaultGravityScale;
                    rb.velocity = new Vector2(rb.velocity.x, -1f);

                    isDragging = false;
                    CheckInteraction();

                    break;
            }
        }
    }

    bool IsTouchingObject(Vector3 touchPos){
        RaycastHit2D hit = Physics2D.Raycast(touchPos, Vector2.zero);
        if(hit.collider != null) return hit.transform == transform;
        return false;
    }

    void CheckInteraction(){
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 0.5f);
        foreach(Collider2D col in colliders){
            if(col.CompareTag("Pet")){
                currPetCollider = col;
                return;
            }
        }

        currPetCollider = null;
    }

    void FeedPet(){
        BasicNeeds pet = currPetCollider.GetComponent<BasicNeeds>();
        if(pet == null) return;

        pet.FeedPet(foodData);
        this.gameObject.SetActive(false);
    }
}
