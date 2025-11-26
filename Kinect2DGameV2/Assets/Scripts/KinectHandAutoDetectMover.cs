using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;

/// <summary>
/// Intenta detectar automáticamente un userId válido (1..6) y el joint de mano trackeado,
/// y mueve el targetObject directamente con esa posición (map XY, fixed Z).
/// Úsalo para comprobar en tiempo real qué userId/joint están activos en tu Kinect v1 + plugin.
/// </summary>
public class KinectHandAutoDetectMover : MonoBehaviour
{
    public Transform targetObject;
    public bool preferRightHand = true; // intenta primero la mano derecha
    public float scaleX = 5f;
    public float scaleY = 5f;
    public float fixedZ = 0f;

    object kManager = null;
    Type kmType = null;
    MethodInfo miIsInitialized = null;
    MethodInfo miIsUserDetected = null;
    MethodInfo miIsJointTracked = null;
    MethodInfo miGetJointPos = null;

    // candidate user ids a probar (UInt32)
    uint[] userCandidates = new uint[] { 1u, 2u, 3u, 4u, 5u, 6u };
    int[] jointCandidates = new int[] { 11, 7 }; // 11 = HandRight, 7 = HandLeft (según wrapper)

    void Start()
    {
        if (targetObject == null)
        {
            Debug.LogError("[KinectHandAutoDetectMover] Asigna targetObject en el Inspector.");
        }
        CacheKinectManager();
    }

    void CacheKinectManager()
    {
        kmType = Type.GetType("KinectManager") ?? Type.GetType("KinectManager, Assembly-CSharp");
        if (kmType == null)
        {
            Debug.LogError("[KinectHandAutoDetectMover] No se encontró KinectManager en el proyecto.");
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

        bool initialized = (bool)(miIsInitialized?.Invoke(kManager, null) ?? false);
        if (!initialized) return;

        bool userDetected = (bool)(miIsUserDetected?.Invoke(kManager, null) ?? false);
        if (!userDetected)
        {
            // si no hay usuario, no seguimos
            // Debug.Log("[KinectHandAutoDetectMover] Waiting for users.");
            return;
        }

        // intentamos detectar un userId y joint válidos
        uint foundUser = 0u;
        int foundJoint = -1;
        Vector3 foundPos = Vector3.zero;
        bool found = false;

        // priorizar mano derecha o izquierda según preferRightHand
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
                        // está trackeado: ahora pedir posición
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
                                // si la posición no es cero, la consideramos válida
                                if (p != Vector3.zero)
                                {
                                    foundUser = uid;
                                    foundJoint = j;
                                    foundPos = p;
                                    found = true;
                                    Debug.Log("[KinectHandAutoDetectMover] Encontrado userId=" + uid + " joint=" + j + " pos=" + p);
                                    break;
                                }
                                else
                                {
                                    // si es Vector3.zero, todavía puede estar detectando pero no trackeado completamente
                                    Debug.Log("[KinectHandAutoDetectMover] user " + uid + " joint " + j + " pos = (0,0,0) -> no usable");
                                }
                            }
                        }
                        else
                        {
                            Debug.Log("[KinectHandAutoDetectMover] user " + uid + " joint " + j + " => GetJointPosition devolvió null");
                        }
                    }
                    else
                    {
                        // res false o null
                        //Debug.Log("[KinectHandAutoDetectMover] IsJointTracked(" + uid + "," + j + ") -> " + res);
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log("[KinectHandAutoDetectMover] Excepción al invocar IsJointTracked(" + uid + "," + j + "): " + ex.GetBaseException().Message);
                }
            }
            if (found) break;
        }

        if (found && targetObject != null)
        {
            // mapeo simple y directo (sin suavizado)
            float mappedX = foundPos.x * scaleX;
            float mappedY = foundPos.y * scaleY;
            targetObject.position = new Vector3(mappedX, mappedY, fixedZ);
        }
        else
        {
            // no encontrado: imprime una vez por segundo para no spamear
            if (Time.frameCount % 60 == 0) Debug.Log("[KinectHandAutoDetectMover] No se encontró userId/joint usable aún. Asegúrate de estar frente al sensor.");
        }
    }
}
