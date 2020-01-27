using UnityEngine;

public class GameController : MonoBehaviour
{
    private GameObject[] mCharacters;

    void Start()
    {
        mCharacters = GameObject.FindGameObjectsWithTag("Free");
        Init();
    }

    // Unfreeze all characters and start the round over
    public void ResetRound() {
        for(int i = 0; i < mCharacters.Length; i++)
            mCharacters[i].GetComponent<AIMovement>().UnFreeze();
        Init();
    }

    // Pick a random character to be the tagged character
    private void Init() {
        Random.InitState(System.DateTime.Now.Millisecond);
        int initialTag = Random.Range(0, mCharacters.Length);
        mCharacters[initialTag].GetComponent<AIMovement>().SetTagged(true);
        mCharacters[initialTag].tag = "Tagged";
    }
}
