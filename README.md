# UnityMeshDeformation
Change the vertices of a mesh and its collider at runtime to visualize impact on Collision

This is a coroutine that follows this structure:
1. Detect collision
2. Check that the collision is forceful enough to deform a mesh
3. Synchronously: Call for mesh deformation in the next frame (iterating over a mesh's vertices using Parallelism)
4. Call for the recreation of the object's mesh collider (such that it conforms to its new shape)

I didn't develop the original mesh deformation object but have adapted it to suit my needs.

- Attach "MeshDeformer.cs" script to objects that should be deformed on collision.
  
  *There are fields for manually inserting the meshes to be deformed but you can allow the script to use GetComponent to fill those fields.*
  
- Add mesh filters in the deformable mesh filters array field

- Add mesh collider that detects the collision (and is swapped for a new mesh collider on applicable collisions so as to avoid bumping into invisible colliders)

