using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class MeshDeformer : MonoBehaviour
{
    public bool useDamage = true;                                               // Use Damage.
    private struct originalMeshVerts { public Vector3[] meshVerts; }    // Struct for Original Mesh Verticies positions.
    private originalMeshVerts[] originalMeshData;                           // Array for struct above.
    /// <summary>
    /// An 
    /// </summary>
    private Vector3[] originalMeshVertices;
    public MeshFilter[] deformableMeshFilters;                              // Deformable Meshes.
    [SerializeField] private MeshCollider meshCollider;
    public LayerMask damageFilter = -1;                                     // LayerMask filter for not taking any damage.
    public float randomizeVertices = 1f;                                            // Randomize Verticies on Collisions for more complex deforms.
    public float damageRadius = .5f;                                                // Verticies in this radius will be effected on collisions.
    private float minimumVertDistanceForDamagedMesh = .002f;        // Comparing Original Vertex Positions Between Last Vertex Positions To Decide Mesh Is Repaired Or Not.
 
    public float maximumDamage = .5f;               // Maximum Vert Distance For Limiting Damage. 0 Value Will Disable The Limit.
    private float minimumCollisionForce = 5f;       // Minimum collision force.
    public float damageMultiplier = 1f;             // Damage multiplier.

    void Start() 
    {
        InitOriginalMeshData();
    }
    private void InitOriginalMeshData()
    {
        originalMeshData = new originalMeshVerts[deformableMeshFilters.Length];

        for (int i = 0; i < deformableMeshFilters.Length; i++)
        {
            originalMeshData[i].meshVerts = deformableMeshFilters[i].mesh.vertices;
        }

    }
    void OnCollisionEnter(Collision collision)
    {
        //NO COLLISION
        if (collision.contacts.Length < 1) 
            return;
        //NOT ENOUGH FORCE
        if (collision.relativeVelocity.magnitude < minimumCollisionForce)
            return;


        Vector3 colRelVel = collision.relativeVelocity;
        Vector3 normalized_colRelVel = colRelVel * (1f - Mathf.Abs(Vector3.Dot(transform.up, collision.contacts[0].normal)));

        float angleOfImpact = Mathf.Abs(Vector3.Dot(collision.contacts[0].normal, colRelVel.normalized));

        //RECHECK FORCE OF IMPACT GREAT ENOUGH
        if (colRelVel.magnitude * angleOfImpact < minimumCollisionForce)
            return;


        Vector3 localImpactVector = transform.InverseTransformDirection(colRelVel) * (damageMultiplier / 50f); //What does the 50 represent?

        for (int i = 0; i < deformableMeshFilters.Length; i++)
            DeformMesh(i, originalMeshData[i].meshVerts, collision, angleOfImpact, localImpactVector);
    }

    private void DeformMesh(int meshIndex, Vector3[] originalMesh, Collision collision, float cos, Vector3 localImpactVector)
    {
        Mesh mesh = deformableMeshFilters[meshIndex].mesh;
        Transform meshTransform = deformableMeshFilters[meshIndex].transform;

        Vector3[] vertices = mesh.vertices;

        for (int i = 0; i < collision.contacts.Length; i++)
        {
            ContactPoint contact = collision.contacts[i];

            Vector3 point = meshTransform.InverseTransformPoint(contact.point);

            for (int v = 0; v < vertices.Length; v++)
            {

                float vertDistanceFromImpact = (point - vertices[v]).magnitude;


                if (vertDistanceFromImpact > damageRadius)
                    continue;


                vertices[v] += /*rot **/Quaternion.LookRotation( collision.relativeVelocity)* ((localImpactVector * (damageRadius - (point - vertices[v]).magnitude) / damageRadius) * cos + (new Vector3(Mathf.Sin(vertices[v].y * 1000), Mathf.Sin(vertices[v].z * 1000), Mathf.Sin(vertices[v].x * 100)).normalized * (randomizeVertices / 500f)));
                if (maximumDamage > 0 && ((vertices[v] - originalMesh[v]).magnitude) > maximumDamage)
                {
                    vertices[v] = originalMesh[v] + (vertices[v] - originalMesh[v]).normalized * (maximumDamage);
                }

            }

        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        Destroy(meshCollider);
        meshCollider = gameObject.AddComponent<MeshCollider>();
    }
}
