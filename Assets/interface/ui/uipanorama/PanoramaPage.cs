using UnityEngine;

public class PanoramaPage : MonoBehaviour
{
    public PanoramaBook book;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateSprite(int dir)
    {
        Debug.Log("book: " + book.currentPage + " dir: " + dir);
        StartCoroutine(book.UpdateSprites2(book.currentPage, dir));
    }

    public void FixSprite(int dir)
    {
        Debug.Log("book2: " + book.currentPage + " dir2: " + dir);
        book.FixSprite();
    }

    public void FixBtn()
    {
        book.FixBtn();
    }
}
