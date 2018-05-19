using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attached to all trees. Controls tree-shake animation and bonus text.
/// </summary>
public class TreeController : MonoBehaviour {

    private Animator anim;
    public Text bonusText;
    
    private void Start () {
        anim = GetComponent<Animator>();
        bonusText = transform.GetComponentInChildren<Text>();
    }

    public void GotHit(int amount, Color color)
    {
        transform.Find("collider-shake").GetComponent<Collider2D>().enabled = false;
        Invoke("ActivateCollider", 6.0f);

        anim.SetTrigger("Shake");
        bonusText.color = color;
        bonusText.text = "+" + amount.ToString();

        CancelInvoke();
        Invoke("HideText", 1.0f);
    }

    /// <summary>
    /// Prevent "stuck situations" where player gets countless score (new game loads before collider is active again)
    /// </summary> 
    private void ActivateCollider()
    {
        transform.Find("collider-shake").GetComponent<Collider2D>().enabled = true;
    }

    private void HideText()
    {
        bonusText.text = "";
    }
}
