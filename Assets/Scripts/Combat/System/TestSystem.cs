using UnityEngine;

// Test system for managing card creation and addition to hand during gameplay
public class TestSystem : MonoBehaviour
{
    // Reference to the hand view component that displays cards in the player's hand
    [SerializeField] private HandView handView;
    [SerializeField] private CardData cardData;

    // Update is called once per frame to check for player input
    void Update()
    {
        // Check if the spacebar key is pressed down this frame
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Card card = new Card(cardData);
            // Create a new card view at the current transform position with no rotation
            CardView cardView = CardViewCreator.Instance.CreateCardView(card,transform.position, Quaternion.identity);
            // Start coroutine to add the newly created card to the player's hand
            StartCoroutine(handView.AddCard(cardView));
        }
    }
}
