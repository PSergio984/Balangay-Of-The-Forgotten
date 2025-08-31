using TMPro;
using UnityEngine;

// Represents the visual components and data display of a single card
public class CardView : MonoBehaviour
{

    public Card Card { get; private set; }
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
        wrapper.SetActive(false);
        Vector3 pos = new(transform.position.x, -2, 0);
        CardViewHoverSystem.Instance.Show(Card, pos);
    }

    void OnMouseExit()
    {
        CardViewHoverSystem.Instance.Hide();
        wrapper.SetActive(true);
    }

}
