using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;

/// <summary>
/// Escanea userId (0..6) y jointIndex (0..25) para encontrar combos que devuelvan
/// IsJointTracked==true o GetJointPosition != Vector3.zero. Mueve targetObject usando f�sica
/// para respetar colisiones del laberinto.
/// Uso: a�adir al HandController, asignar Target Object y Play.
/// </summary>
public class KinectFullScan : MonoBehaviour
{
    public Transform targetObject;
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
    private bool reachedGoal = false;
    HashSet<string> reported = new HashSet<string>();

    void Start()
    {
        if (targetObject == null)
        {
            Debug.LogWarning("[KinectFullScan] Asigna Target Object en el Inspector.");
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

            // Asegurarse de que tenga un collider NO-trigger
            Collider2D col = targetObject.GetComponent<Collider2D>();
            if (col == null)
            {
                Debug.LogError("[KinectFullScan] El personaje necesita un Collider2D. Agregando CapsuleCollider2D...");
                col = targetObject.gameObject.AddComponent<CapsuleCollider2D>();
            }
            
            if (col.isTrigger)
            {
                Debug.LogWarning("[KinectFullScan] El collider del personaje no debe ser Trigger. Desactivando...");
                col.isTrigger = false;
            }

            // Agregar tag "Player" si no lo tiene
            if (!targetObject.CompareTag("Player"))
            {
                try
                {
                    targetObject.tag = "Player";
                    Debug.Log("[KinectFullScan] Tag 'Player' asignado.");
                }
                catch
                {
                    Debug.LogWarning("[KinectFullScan] No se pudo asignar tag 'Player'.");
                }
            }
        }
        
        CacheKinectManager();
    }

    void CacheKinectManager()
    {
        kmType = Type.GetType("KinectManager") ?? Type.GetType("KinectManager, Assembly-CSharp");
        if (kmType == null)
        {
            Debug.LogError("[KinectFullScan] No se encontr� tipo KinectManager en el proyecto.");
            return;
        }

        // Obtener instancia
        var prop = kmType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase);
        if (prop != null) kManager = prop.GetValue(null, null);
        else
        {
            var field = kmType.GetField("instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.IgnoreCase);
            if (field != null) kManager = field.GetValue(null);
        }
        if (kManager == null)
        {
            Debug.LogError("[KinectFullScan] KinectManager.Instance es null.");
            return;
        }

        miIsInitialized = kmType.GetMethod("IsInitialized");
        miIsUserDetected = kmType.GetMethod("IsUserDetected");
        miIsJointTracked = kmType.GetMethod("IsJointTracked");
        miGetJointPos = kmType.GetMethod("GetJointPosition") ?? kmType.GetMethod("GetJointKinectPosition") ?? kmType.GetMethod("GetJoint");

        Debug.Log("[KinectFullScan] KinectManager cacheado: " + kmType.Name
                  + " (IsJointTracked=" + (miIsJointTracked != null) + ", GetJointPos=" + (miGetJointPos != null) + ")");
    }

    void Update()
    {
        if (kManager == null) { CacheKinectManager(); if (kManager == null) return; }
        if (rb == null) return;

        // Si ya lleg� a la meta, detener control
        if (reachedGoal)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

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
            if (Time.frameCount % 120 == 0) Debug.Log("[KinectFullScan] Waiting for users...");
            return;
        }

        bool foundValidMovement = false;
        Vector3 targetPosition = Vector3.zero;

        // Barrido: userId 0..6 (UInt32), joints 0..25
        for (uint uid = 0; uid <= 6; uid++)
        {
            for (int j = 0; j <= 25; j++)
            {
                bool tracked = false;
                try
                {
                    if (miIsJointTracked != null)
                    {
                        object res = miIsJointTracked.Invoke(kManager, new object[] { uid, j });
                        if (res is bool) tracked = (bool)res;
                    }
                }
                catch (Exception ex)
                {
                    // ignora excepciones de firma, sigue probando
                }

                object posObj = null;
                try
                {
                    // Intentamos invocar GetJointPosition(uid, j)
                    if (miGetJointPos != null)
                    {
                        posObj = miGetJointPos.Invoke(kManager, new object[] { uid, j });
                    }
                }
                catch (Exception ex)
                {
                    // si lanza excepci�n (firma distinta), no cortamos; posObj seguir� null
                }

                Vector3 pos = Vector3.zero;
                bool hasPos = false;
                if (posObj != null)
                {
                    // intentar castear a Vector3
                    try
                    {
                        pos = (Vector3)posObj;
                        hasPos = true;
                    }
                    catch
                    {
                        var t = posObj.GetType();
                        object gx = t.GetField("x") != null ? t.GetField("x").GetValue(posObj) : (t.GetProperty("x") != null ? t.GetProperty("x").GetValue(posObj, null) : null);
                        object gy = t.GetField("y") != null ? t.GetField("y").GetValue(posObj) : (t.GetProperty("y") != null ? t.GetProperty("y").GetValue(posObj, null) : null);
                        object gz = t.GetField("z") != null ? t.GetField("z").GetValue(posObj) : (t.GetProperty("z") != null ? t.GetProperty("z").GetValue(posObj, null) : null);
                        if (gx != null && gy != null && gz != null)
                        {
                            pos = new Vector3(Convert.ToSingle(gx), Convert.ToSingle(gy), Convert.ToSingle(gz));
                            hasPos = true;
                        }
                    }
                }

                // consideramos "v�lido" si tracked==true o pos magnitude>0.001
                if (tracked || (hasPos && pos.magnitude > 0.001f))
                {
                    string key = uid + "_" + j;
                    if (!reported.Contains(key))
                    {
                        reported.Add(key);
                        Debug.Log("[KinectFullScan] FOUND userId=" + uid + " joint=" + j + " tracked=" + tracked + " pos=" + (hasPos ? pos.ToString("F3") : "null"));
                    }

                    if (hasPos && pos.magnitude > 0.001f && targetObject != null)
                    {
                        // Calcular posici�n objetivo
                        float mx = pos.x * scaleX;
                        float my = pos.y * scaleY;
                        targetPosition = new Vector3(mx, my, fixedZ);
                        foundValidMovement = true;
                        
                        // Loguear una vez cada 30 frames
                        if (Time.frameCount % 30 == 0)
                            Debug.Log("[KinectFullScan] Moviendo target por userId=" + uid + " joint=" + j + " -> pos " + pos.ToString("F3"));
                        
                        // Usar el primer joint v�lido encontrado y salir
                        break;
                    }
                }
            }
            if (foundValidMovement) break;
        }

        // Aplicar movimiento usando f�sica si encontramos posici�n v�lida
        if (foundValidMovement && targetObject != null)
        {
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
        }
        else
        {
            // No encontr� posici�n v�lida: detener
            rb.linearVelocity = Vector2.zero;
            
            if (reported.Count == 0 && Time.frameCount % 120 == 0)
            {
                Debug.Log("[KinectFullScan] No se encontraron joints �tiles todav�a. Aseg�rate de estar visible al sensor y manos fuera de la ropa.");
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

    // Detectar meta
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (reachedGoal) return;

        if (other.gameObject.name == "Meta" || other.CompareTag("Goal"))
        {
            reachedGoal = true;
            rb.linearVelocity = Vector2.zero;
            Debug.Log("[KinectFullScan] ¡Meta alcanzada!");
        }
    }

    // Método público para notificación externa
    public void ReachGoal()
    {
        if (reachedGoal) return;
        reachedGoal = true;
        rb.linearVelocity = Vector2.zero;
        Debug.Log("[KinectFullScan] Meta alcanzada (notificado externamente)");
    }

    // Detectar colisiones para debug
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("[KinectFullScan] Colisi�n detectada con: " + collision.gameObject.name);
    }
}
