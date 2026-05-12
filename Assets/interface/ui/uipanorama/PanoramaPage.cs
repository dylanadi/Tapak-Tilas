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
        StartCoroutine(book.UpdateSprites2(book.currentPage, dir));
    }
}
