using UnityEngine;

public class GameController : MonoBehaviour
{
    private GameObject[] mCharacters;

    void Start()
    {
        mCharacters = GameObject.FindGameObjectsWithTag("Free");
        Init();
    }

    public void ResetRound() {
        for(int i = 0; i < mCharacters.Length; i++)
            mCharacters[i].GetComponent<AIMovement>().UnFreeze();
        Init();
    }

    private void Init() {
        Random.InitState(System.DateTime.Now.Millisecond);
        int initialTag = Random.Range(0, mCharacters.Length);
        mCharacters[initialTag].GetComponent<AIMovement>().SetTagged(true);
        mCharacters[initialTag].tag = "Tagged";
    }
}
