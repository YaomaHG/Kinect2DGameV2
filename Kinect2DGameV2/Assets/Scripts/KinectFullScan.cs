using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;

/// <summary>
/// Escanea userId (0..6) y jointIndex (0..25) para encontrar combos que devuelvan
/// IsJointTracked==true o GetJointPosition != Vector3.zero. Mueve targetObject si encuentra posición válida.
/// Uso: añadir al HandController, asignar Target Object y Play.
/// </summary>
public class KinectFullScan : MonoBehaviour
{
    public Transform targetObject;
    public float scaleX = 5f;
    public float scaleY = 5f;
    public float fixedZ = 0f;

    object kManager = null;
    Type kmType = null;
    MethodInfo miIsInitialized = null;
    MethodInfo miIsUserDetected = null;
    MethodInfo miIsJointTracked = null;
    MethodInfo miGetJointPos = null;

    HashSet<string> reported = new HashSet<string>();

    void Start()
    {
        if (targetObject == null) Debug.LogWarning("[KinectFullScan] Asigna Target Object en el Inspector.");
        CacheKinectManager();
    }

    void CacheKinectManager()
    {
        kmType = Type.GetType("KinectManager") ?? Type.GetType("KinectManager, Assembly-CSharp");
        if (kmType == null)
        {
            Debug.LogError("[KinectFullScan] No se encontró tipo KinectManager en el proyecto.");
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

        bool initialized = (bool)(miIsInitialized?.Invoke(kManager, null) ?? false);
        if (!initialized) return;

        bool userDetected = (bool)(miIsUserDetected?.Invoke(kManager, null) ?? false);
        if (!userDetected)
        {
            if (Time.frameCount % 120 == 0) Debug.Log("[KinectFullScan] Waiting for users...");
            return;
        }

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
                    // si lanza excepción (firma distinta), no cortamos; posObj seguirá null
                    // Debug.Log("[KinectFullScan] GetJointPosition threw for ("+uid+","+j+"): " + ex.GetBaseException().Message);
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

                // consideramos "válido" si tracked==true o pos magnitude>0.001
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
                        // mover target directamente (map XY)
                        float mx = pos.x * scaleX + targetObject.position.x * 0; // simple mapping
                        float my = pos.y * scaleY + targetObject.position.y * 0;
                        targetObject.position = new Vector3(mx, my, fixedZ);
                        // también loguear una vez
                        if (Time.frameCount % 30 == 0)
                            Debug.Log("[KinectFullScan] Moviendo target por userId=" + uid + " joint=" + j + " -> pos " + pos.ToString("F3"));
                    }
                }
            }
        }

        // si no encontró nada todavía, imprime cada 120 frames
        if (reported.Count == 0 && Time.frameCount % 120 == 0) Debug.Log("[KinectFullScan] No se encontraron joints útiles todavía. Asegúrate de estar visible al sensor y manos fuera de la ropa.");
    }
}
