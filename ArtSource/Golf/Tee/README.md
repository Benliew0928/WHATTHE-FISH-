# Wooden opening tee

The user-supplied [reference](WoodTee-reference.png) guides only the wooden
support below the ball. The game keeps its existing dimpled Golf ball.

[GolfTeeBuilder](../../../Game/Assets/_Game/Editor/GolfTeeBuilder.cs) is the
editable master: a pointed lathed shaft, rounded collar and shallow spherical
cup. Run **WHATTHE FISH? / Golf / Prepare wooden tee** to regenerate. Normal
Windows and Android builds also prepare the assets. The shared delivery mesh,
64×128 wood-grain texture, material and prefab are kept in Git; no external
model, image path or package is needed in a fresh clone.

The base is planted at each player's opening position, with the point below
the terrain. Only opening Hole 1 uses this support. It remains fixed after a
shot and is removed on round reset or travel. Recovery uses the existing last
shot position, including the supported opening position. Later-hole placement,
ball ownership, scoring and camera rules are unchanged. The static concave
collider supports the existing dynamic ball; character and cart movement are
excluded. All players share the same mesh, texture and material.
