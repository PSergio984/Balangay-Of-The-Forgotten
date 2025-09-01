using TMPro;
using UnityEngine;

// Represents the visual components and data display of a single card
public class CardView : MonoBehaviour
{

    
    // TEXT COMPONENTS IN THE CARD
    // Text component to display the card's title/name
    [SerializeField] private TMP_Text title;
    // Text component to display the card's description or effect text
    [SerializeField] private TMP_Text description;
    // Text component to display the stamina cost required to play the card
    [SerializeField] private TMP_Text stamina;
    // Text component to display the damage value the card deals
    [SerializeField] private TMP_Text damage;
    // Sprite renderer component to display the card's artwork/image
    [SerializeField] private SpriteRenderer imagesSR;
    // GameObject that wraps/contains all the card's visual elements
    [SerializeField] private GameObject wrapper;
    [SerializeField] private LayerMask dropLayer;

    public Card Card { get; private set; }
    private Vector3 dragStartPosition;
    private Quaternion dragStartRotation;

    public void Setup(Card card)
    {
        Card = card;
        title.text = card.Title;
        description.text = card.Description;
        stamina.text = card.Stamina.ToString();
        imagesSR.sprite = card.image;
    }
    private void OnMouseEnter()
    {
        if (!Interactions.Instance.PlayerCanHover()) return;
        wrapper.SetActive(false);
        Vector3 pos = new(transform.position.x, -2, 0);
        CardViewHoverSystem.Instance.Show(Card, pos);
    }

    void OnMouseExit()
    {
        if (!Interactions.Instance.PlayerCanHover()) return;
        CardViewHoverSystem.Instance.Hide();
        wrapper.SetActive(true);
    }

    void OnMouseDown()
    {
        if (!Interactions.Instance.PlayerCanInteract()) return;
        Interactions.Instance.PlayerIsDragging = true;
        wrapper.SetActive(true);
        CardViewHoverSystem.Instance.Hide();
        dragStartPosition = transform.position;
        dragStartRotation = transform.rotation;
        transform.rotation = Quaternion.Euler(0, 0, 0);
        transform.position = MouseUtil.GetMousePositionInWorldSpace(-1);
    }
    void OnMouseDrag()
    {
        if (!Interactions.Instance.PlayerCanInteract()) return;
         transform.position = MouseUtil.GetMousePositionInWorldSpace(-1);
    }
    void OnMouseUp()
    {
        if (!Interactions.Instance.PlayerCanInteract()) return;
        if (StaminaSystem.Instance.HasEnoughStamina(Card.Stamina) && Physics.Raycast(transform.position, Vector3.forward, out RaycastHit hit, 10f, dropLayer))
        {
            PlayCardsGA playCardGA = new(Card);
            ActionSystem.Instance.Perform(playCardGA);
        }
        else
        {
            transform.position = dragStartPosition;
            transform.rotation = dragStartRotation;
        }

        Interactions.Instance.PlayerIsDragging = false;
    }

}
