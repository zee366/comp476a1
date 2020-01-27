January 27, 2020

Comp 476 NN
Winter 2020
Assignment 1
Jason Brennan
27793928

SPECIAL FEATURES:

- The tagged character is surrounded by a swirling vortex of snow particles to better differentiate them from the other characters. In addition, a spot light follows the tagged character around. The swirling vortex was created using Unity's particle system.

- I created a frozen ice block in blender and animated its destruction in Unity. When the pursuer reaches a target, an ice block is instantiated at the target's location to simulate them being frozen. When a character is unfrozen, the ice block shatters into ice shards.

NOTES ON BEHAVIOUR / CODE:

- Since the tagged character uses a pursue behavior which delegates to seek, they are always moving at maximum velocity when chasing. As such, their cone of perception is always at the minimum. This may result in the tagged character stopping and turning to face their target position as they get closer (depending on angle of approach and the velocity of the target). This doesn't always happen, but this is the reason if you see it.

- The Update() method in AIMovement contains all the decision logic for the characters. I gave the characters several states as boolean members as well as tags in Unity's editor. We then proceed through several nested if statements to determine which behaviour to perform. Once the appropriate code block has been reached, we delegate to one or more of several heuristics to execute the movement (Arrive(target), Seek(target), Align(target), Wander() etc). I would have liked to explore implementing a behaviour tree but lack of knowledge of the subject and time constraints pushed me to just use if statements.

- A GameController script initiates the game by randomly selecting one character to be tagged. When (n - 1) characters have been frozen, we trigger a unity event to reset the game by unfreezing everyone and then randomly selecting another character (maybe the same) to be tagged, and the game continues.

REFERENCES:

The model used for the characters is the Kyle Robot from the first tutorial, also available on the Unity Store.
https://assetstore.unity.com/packages/3d/characters/robots/space-robot-kyle-4696

All other art assets were created by me.