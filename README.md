# UnityMeshDeformation
Change the vertices of a mesh and its collider at runtime to visualize impact on Collision

I didn't develop the original mesh deformation object but have adapted it to suit my needs.

- Attach "MeshDeformer.cs" script to objects that should be deformed on collision.
  
  *Right now there are fields you have to fill with the mesh filters to be deformed and the mesh collider that's used for the collision detection*
  
- Add mesh filters in the deformable mesh filters array field

- Add mesh collider that detects the collision (and is swapped for a new mesh collider on applicable collisions so as to avoid bumping into invisible colliders)
