using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

public class MeshDeformer : MonoBehaviour
{
    public bool useDamage = true;                                               // Use Damage.
    private struct originalMeshVerts { public Vector3[] meshVerts; }    // Struct for Original Mesh Verticies positions.
    private originalMeshVerts[] originalMeshData;                           // Array for struct above.

    public MeshFilter[] deformableMeshFilters;                              // Deformable Meshes.
    [SerializeField] private MeshCollider meshCollider;
    public LayerMask damageFilter = -1;                                     // LayerMask filter for not taking any damage.
    public float randomizeVertices = 1f;                                            // Randomize Verticies on Collisions for more complex deforms.
    public float damageRadius = .5f;                                                // Verticies in this radius will be effected on collisions.
   
    public float maximumDamage = .5f;               // Maximum Vert Distance For Limiting Damage. 0 Value Will Disable The Limit.
    private float minimumCollisionForce = 5f;       // Minimum collision force.
    public float deformMultiplier = 1f;             // Damage multiplier.

    void Start() 
    {
        if (deformableMeshFilters.Length == 0)
            deformableMeshFilters = new MeshFilter[] { GetComponent<MeshFilter>() };
        if (meshCollider == null)
            meshCollider = GetComponent<MeshCollider>();

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
        {
            return;
        }
        //NOT ENOUGH FORCE
        if (collision.relativeVelocity.magnitude < minimumCollisionForce)
        {
            print("Escaped collision: First force check");
            return;
        }


        Vector3 colRelVel = collision.relativeVelocity;
        Vector3 normalized_colRelVel = colRelVel * (1f - Mathf.Abs(Vector3.Dot(transform.up, collision.contacts[0].normal)));

        float angleOfImpact = Mathf.Abs(Vector3.Dot(collision.contacts[0].normal, colRelVel.normalized));

        //RECHECK FORCE OF IMPACT GREAT ENOUGH
        if (colRelVel.magnitude * angleOfImpact < minimumCollisionForce)
        {
            print("Escaped collision: SECOND force check");
            return;
        }


        Vector3 localImpactVector = transform.InverseTransformDirection(colRelVel) * (deformMultiplier / 50f); //What does the 50 represent?


        StartCoroutine(SynchronizedMeshDeformation(collision, angleOfImpact, localImpactVector));
    }

    private IEnumerator SynchronizedMeshDeformation(Collision collision, float angleOfImpact, Vector3 localImpactVector) 
    {
        yield return new WaitForEndOfFrame();

        for (int i = 0; i < deformableMeshFilters.Length; i++)
            DeformMesh(i, originalMeshData[i].meshVerts, collision, angleOfImpact, localImpactVector);




        yield return new WaitForEndOfFrame();

        ReplaceCollider();
    }

    private void ReplaceCollider() 
    {
        Destroy(meshCollider);
        meshCollider = gameObject.AddComponent<MeshCollider>();
    }

    private void DeformMesh(int meshIndex, Vector3[] originalMesh, Collision collision, float angleOfImpact, Vector3 localImpactVector)
    {
        Mesh mesh = deformableMeshFilters[meshIndex].mesh;
        Transform meshTransform = deformableMeshFilters[meshIndex].transform;

        Vector3[] vertices = mesh.vertices;

        for (int i = 0; i < collision.contacts.Length; i++)
        {
            Vector3 contactPoint = collision.contacts[i].point;

            Vector3 point = meshTransform.InverseTransformPoint(contactPoint);

            Parallel.For(0, vertices.Length, v => 
            {
                float vertDistanceFromImpact = (point - vertices[v]).magnitude;


                if (vertDistanceFromImpact > damageRadius)
                {
                    return; //same as continue in Parallel.For
                }


                vertices[v] += Quaternion.LookRotation(collision.relativeVelocity)
                    * ((localImpactVector * (damageRadius - (point - vertices[v]).magnitude) / damageRadius)
                    * angleOfImpact
                    + (new Vector3(
                        Mathf.Sin(vertices[v].y),
                        Mathf.Sin(vertices[v].z),
                        Mathf.Sin(vertices[v].x)).normalized)
                        * .002f);

                if (maximumDamage > 0 && ((vertices[v] - originalMesh[v]).magnitude) > maximumDamage)
                {
                    vertices[v] = originalMesh[v] + (vertices[v] - originalMesh[v]).normalized * (maximumDamage);
                }
            });
            /*
            for (int v = 0; v < vertices.Length; v++)
            {

                float vertDistanceFromImpact = (point - vertices[v]).magnitude;


                if (vertDistanceFromImpact > damageRadius)
                {
                    print("Escaped collision: Distance check");
                    continue;
                }


                vertices[v] += Quaternion.LookRotation(collision.relativeVelocity)
                    * ((localImpactVector * (damageRadius - (point - vertices[v]).magnitude) / damageRadius)
                    * angleOfImpact
                    + (new Vector3(
                        Mathf.Sin(vertices[v].y),
                        Mathf.Sin(vertices[v].z),
                        Mathf.Sin(vertices[v].x)).normalized) 
                        * .002f);
                
                if (maximumDamage > 0 && ((vertices[v] - originalMesh[v]).magnitude) > maximumDamage)
                {
                    vertices[v] = originalMesh[v] + (vertices[v] - originalMesh[v]).normalized * (maximumDamage);
                }

            }*/

        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        
    }
}
