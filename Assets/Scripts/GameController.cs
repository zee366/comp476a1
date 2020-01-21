using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField]
    GameObject iceBlock;

    private GameObject[] mCharacters;
    private Dictionary<int, GameObject> mIceBlocks;
    
    // Start is called before the first frame update
    void Start()
    {
        mCharacters = GameObject.FindGameObjectsWithTag("Free");
        int initialTag = Random.Range(0, mCharacters.Length);
        mCharacters[initialTag].tag = "Tagged";
        mCharacters[initialTag].GetComponent<AIMovement>().SetTagged(true);

        for(int i = 0; i < mCharacters.Length; i++) {
            if(mCharacters[i].tag.Equals("Free"))
                mCharacters[i].GetComponent<AIMovement>().SetMaterial(Color.green);
            else if(mCharacters[i].tag.Equals("Tagged"))
                mCharacters[i].GetComponent <AIMovement>().SetMaterial(Color.blue);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateIceBlock(GameObject character) {
        mIceBlocks.Add(character.GetInstanceID(), Instantiate(iceBlock, character.transform));
    }

    public void ShatterIceBlock(GameObject character) {
        GameObject block = mIceBlocks[character.GetInstanceID()];
        Animation anim = block.GetComponent<Animation>();
        anim.Play();
    }
}
