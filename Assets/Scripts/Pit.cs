using System.Collections.Generic;
using UnityEngine;

public class Pit : MonoBehaviour
{
    [SerializeField]
    private int pitIndex;
    [SerializeField]
    private GameObject stonePrefab;
    [SerializeField]
    private List<Sprite> stoneSprites;

    private Board board;

    void Start()
    {
        board = GetComponentInParent<Board>();

        int stoneSpriteIndex = Random.Range(0, stoneSprites.Count);
        for (int i = 0; i < 4; ++i)
        {
            GameObject stone = Instantiate(stonePrefab, transform);
            var sr = stone.GetComponent<SpriteRenderer>();
            sr.sprite = stoneSprites[stoneSpriteIndex];
            stoneSpriteIndex = (stoneSpriteIndex + 1) % stoneSprites.Count;
            // Stone position random offset from center of pit
            stone.transform.localPosition = new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f), 0);
            // Assign a different sorting order to each stone mask to prevent masks interacting with each other
            sr.sortingOrder = pitIndex * 4 + i;
            var mask = stone.GetComponentInChildren<SpriteMask>();
            mask.isCustomRangeActive = true;
            mask.frontSortingOrder = pitIndex * 4 + i;
            mask.backSortingOrder = pitIndex * 4 + i - 1;
        }
    }

    void OnMouseUp()
    {
        if (pitIndex < 6)
        {
            StartCoroutine(board.TryPlayerMove(pitIndex));
        }
    }
}