using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;

/// <summary>
/// Intenta detectar autom�ticamente un userId v�lido (1..6) y el joint de mano trackeado,
/// y mueve el targetObject usando f�sica (Rigidbody2D) para respetar colisiones del laberinto.
/// �salo para comprobar en tiempo real qu� userId/joint est�n activos en tu Kinect v1 + plugin.
/// </summary>
public class KinectHandAutoDetectMover : MonoBehaviour
{
    public Transform targetObject;
    public bool preferRightHand = true; // intenta primero la mano derecha
    public float scaleX = 5f;
    public float scaleY = 5f;
    public float fixedZ = 0f;
    public float moveSpeed = 3f; // Velocidad de seguimiento

    object kManager = null;
    Type kmType = null;
    MethodInfo miIsInitialized = null;
    MethodInfo miIsUserDetected = null;
    MethodInfo miIsJointTracked = null;
    MethodInfo miGetJointPos = null;

    private Rigidbody2D rb;
    private Vector3 lastValidPosition = Vector3.zero;
    private bool hasValidPosition = false;

    // candidate user ids a probar (UInt32)
    uint[] userCandidates = new uint[] { 1u, 2u, 3u, 4u, 5u, 6u };
    int[] jointCandidates = new int[] { 11, 7 }; // 11 = HandRight, 7 = HandLeft (seg�n wrapper)

    void Start()
    {
        if (targetObject == null)
        {
            Debug.LogError("[KinectHandAutoDetectMover] Asigna targetObject en el Inspector.");
        }
        else
        {
            rb = targetObject.GetComponent<Rigidbody2D>();
            
            // Validaci�n y configuraci�n del Rigidbody2D
            if (rb == null)
            {
                Debug.LogError("�El personaje necesita un Rigidbody2D! Agreg�ndolo autom�ticamente...");
                rb = targetObject.gameObject.AddComponent<Rigidbody2D>();
            }

            // Configuraci�n �ptima para colisiones en laberinto
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;  // Sin gravedad para movimiento 2D top-down
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;  // Evitar rotaci�n
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;  // Mejor detecci�n de colisiones
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;  // Movimiento m�s suave
        }
        
        CacheKinectManager();
    }

    void CacheKinectManager()
    {
        kmType = Type.GetType("KinectManager") ?? Type.GetType("KinectManager, Assembly-CSharp");
        if (kmType == null)
        {
            Debug.LogError("[KinectHandAutoDetectMover] No se encontr� KinectManager en el proyecto.");
            return;
        }

        // obtener instancia
        var prop = kmType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase);
        if (prop != null) kManager = prop.GetValue(null, null);
        else
        {
            var field = kmType.GetField("instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.IgnoreCase);
            if (field != null) kManager = field.GetValue(null);
        }

        if (kManager == null)
        {
            Debug.LogError("[KinectHandAutoDetectMover] KinectManager.Instance es null.");
            return;
        }

        miIsInitialized = kmType.GetMethod("IsInitialized");
        miIsUserDetected = kmType.GetMethod("IsUserDetected");
        miIsJointTracked = kmType.GetMethod("IsJointTracked");
        miGetJointPos = kmType.GetMethod("GetJointPosition") ?? kmType.GetMethod("GetJointKinectPosition") ?? kmType.GetMethod("GetJoint");

        Debug.Log("[KinectHandAutoDetectMover] KinectManager cacheado: " + kmType.Name +
                  " (IsJointTracked=" + (miIsJointTracked != null) + ", GetJointPos=" + (miGetJointPos != null) + ")");
    }

    void Update()
    {
        if (kManager == null) { CacheKinectManager(); if (kManager == null) return; }
        if (rb == null) return;

        bool initialized = (bool)(miIsInitialized?.Invoke(kManager, null) ?? false);
        if (!initialized)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        bool userDetected = (bool)(miIsUserDetected?.Invoke(kManager, null) ?? false);
        if (!userDetected)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // intentamos detectar un userId y joint v�lidos
        uint foundUser = 0u;
        int foundJoint = -1;
        Vector3 foundPos = Vector3.zero;
        bool found = false;

        // priorizar mano derecha o izquierda seg�n preferRightHand
        int[] jointOrder = preferRightHand ? new int[] { 11, 7 } : new int[] { 7, 11 };

        foreach (uint uid in userCandidates)
        {
            foreach (int j in jointOrder)
            {
                // llamar a IsJointTracked(uid, j)
                object[] args = new object[] { uid, j };
                try
                {
                    object res = miIsJointTracked?.Invoke(kManager, args);
                    if (res is bool && (bool)res)
                    {
                        // est� trackeado: ahora pedir posici�n
                        object posObj = null;
                        try
                        {
                            posObj = miGetJointPos?.Invoke(kManager, new object[] { uid, j });
                        }
                        catch (Exception ex)
                        {
                            Debug.Log("[KinectHandAutoDetectMover] GetJointPos threw: " + ex.GetBaseException().Message);
                        }

                        if (posObj != null)
                        {
                            // intentar castear a Vector3 directamente
                            Vector3 p;
                            bool ok = false;
                            try
                            {
                                p = (Vector3)posObj;
                                ok = true;
                            }
                            catch
                            {
                                // leer campos x,y,z
                                var t = posObj.GetType();
                                object gx = t.GetField("x") != null ? t.GetField("x").GetValue(posObj) : null;
                                object gy = t.GetField("y") != null ? t.GetField("y").GetValue(posObj) : null;
                                object gz = t.GetField("z") != null ? t.GetField("z").GetValue(posObj) : null;
                                if (gx != null && gy != null && gz != null)
                                {
                                    p = new Vector3(Convert.ToSingle(gx), Convert.ToSingle(gy), Convert.ToSingle(gz));
                                    ok = true;
                                }
                                else
                                {
                                    // intentar propiedades
                                    var px = t.GetProperty("x"); var py = t.GetProperty("y"); var pz = t.GetProperty("z");
                                    if (px != null && py != null && pz != null)
                                    {
                                        p = new Vector3(Convert.ToSingle(px.GetValue(posObj, null)),
                                                        Convert.ToSingle(py.GetValue(posObj, null)),
                                                        Convert.ToSingle(pz.GetValue(posObj, null)));
                                        ok = true;
                                    }
                                    else
                                    {
                                        p = Vector3.zero;
                                    }
                                }
                            }

                            if (ok)
                            {
                                // si la posici�n no es cero, la consideramos v�lida
                                if (p != Vector3.zero)
                                {
                                    foundUser = uid;
                                    foundJoint = j;
                                    foundPos = p;
                                    found = true;
                                    
                                    if (!hasValidPosition)
                                    {
                                        Debug.Log("[KinectHandAutoDetectMover] Encontrado userId=" + uid + " joint=" + j + " pos=" + p);
                                        hasValidPosition = true;
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Silenciar para evitar spam
                }
            }
            if (found) break;
        }

        if (found && targetObject != null)
        {
            // Calcular posici�n objetivo
            float mappedX = foundPos.x * scaleX;
            float mappedY = foundPos.y * scaleY;
            Vector3 targetPosition = new Vector3(mappedX, mappedY, fixedZ);
            
            // Calcular direcci�n hacia el objetivo
            Vector2 currentPos = new Vector2(targetObject.position.x, targetObject.position.y);
            Vector2 targetPos2D = new Vector2(targetPosition.x, targetPosition.y);
            Vector2 direction = (targetPos2D - currentPos).normalized;
            
            // Aplicar velocidad hacia el objetivo - RESPETA COLISIONES
            float distanceToTarget = Vector2.Distance(currentPos, targetPos2D);
            if (distanceToTarget > 0.1f)
            {
                rb.linearVelocity = direction * moveSpeed;
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
            }
            
            lastValidPosition = targetPosition;
        }
        else
        {
            // No encontrado: detener movimiento
            rb.linearVelocity = Vector2.zero;
            
            if (Time.frameCount % 60 == 0 && hasValidPosition == false)
            {
                Debug.Log("[KinectHandAutoDetectMover] No se encontr� userId/joint usable a�n. Aseg�rate de estar frente al sensor.");
            }
        }
    }

    void LateUpdate()
    {
        if (targetObject != null)
        {
            // Mantener Z fijo despu�s de todos los c�lculos de f�sica
            Vector3 p = targetObject.position;
            if (p.z != fixedZ)
            {
                p.z = fixedZ;
                targetObject.position = p;
            }
        }
    }

    // Detectar colisiones para debug
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("[KinectHandAutoDetectMover] Colisi�n detectada con: " + collision.gameObject.name);
    }
}
