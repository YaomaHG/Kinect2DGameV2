using UnityEngine;
using System;
using System.Reflection;

public class KinectDebugProbe : MonoBehaviour
{
    void Start()
    {
        Debug.Log("[KinectDebugProbe] Script cargado y Start() ejecutado.");
    }
    void Update()
    {
        // Intentar obtener KinectManager.Instance
        Type kmType = Type.GetType("KinectManager") ?? Type.GetType("KinectManager, Assembly-CSharp");
        if (kmType == null)
        {
            Debug.LogError("[KinectDebugProbe] No se encontró tipo KinectManager en el proyecto.");
            return;
        }

        // Obtener instancia (propiedad Instance o campo instance)
        object kManager = null;
        var prop = kmType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase);
        if (prop != null) kManager = prop.GetValue(null, null);
        else
        {
            var field = kmType.GetField("instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.IgnoreCase);
            if (field != null) kManager = field.GetValue(null);
        }

        if (kManager == null)
        {
            Debug.LogError("[KinectDebugProbe] KinectManager.Instance es null (no hay instancia activa).");
            return;
        }

        Debug.Log("[KinectDebugProbe] KinectManager encontrado: " + kmType.FullName);

        // métodos clave a buscar
        MethodInfo miIsInitialized = kmType.GetMethod("IsInitialized");
        MethodInfo miIsUserDetected = kmType.GetMethod("IsUserDetected");
        MethodInfo miGetPrimaryUser = kmType.GetMethod("GetPrimaryUserID") ?? kmType.GetMethod("GetPrimaryUserId");
        MethodInfo miGetUserByIndex = kmType.GetMethod("GetUserIdByIndex");
        MethodInfo miIsJointTracked = kmType.GetMethod("IsJointTracked");
        MethodInfo miGetJointPos = kmType.GetMethod("GetJointPosition") ?? kmType.GetMethod("GetJointKinectPosition");

        Debug.LogFormat("[KinectDebugProbe] Métodos: IsInitialized={0}, IsUserDetected={1}, GetPrimaryUserID={2}, GetUserByIndex={3}, IsJointTracked={4}, GetJointPos={5}",
            miIsInitialized != null, miIsUserDetected != null, miGetPrimaryUser != null, miGetUserByIndex != null, miIsJointTracked != null, miGetJointPos != null);

        bool initialized = miIsInitialized != null && (bool)miIsInitialized.Invoke(kManager, null);
        Debug.Log("[KinectDebugProbe] IsInitialized = " + initialized);

        bool userDetected = miIsUserDetected != null && (bool)miIsUserDetected.Invoke(kManager, null);
        Debug.Log("[KinectDebugProbe] IsUserDetected = " + userDetected);

        long userId = -1;
        if (miGetPrimaryUser != null)
        {
            var v = miGetPrimaryUser.Invoke(kManager, null);
            if (v != null) userId = Convert.ToInt64(v);
            Debug.Log("[KinectDebugProbe] GetPrimaryUserID() -> " + userId);
        }
        else if (miGetUserByIndex != null)
        {
            var v = miGetUserByIndex.Invoke(kManager, new object[] { 0 });
            if (v != null) userId = Convert.ToInt64(v);
            Debug.Log("[KinectDebugProbe] GetUserIdByIndex(0) -> " + userId);
        }
        else Debug.LogWarning("[KinectDebugProbe] No hay método para obtener userId.");

        if (userId < 0) return;

        // probar índices de joint comunes (handLeft/Right)
        int[] trialIndices = new int[] { 7, 10, 11 }; // pruebas típicas (puede variar)
        foreach (int idx in trialIndices)
        {
            bool isTracked = miIsJointTracked != null && (bool)miIsJointTracked.Invoke(kManager, new object[] { userId, idx });
            Debug.Log("[KinectDebugProbe] JointIndex " + idx + " tracked? " + isTracked);

            if (isTracked && miGetJointPos != null)
            {
                object posObj = miGetJointPos.Invoke(kManager, new object[] { userId, idx });
                if (posObj != null)
                {
                    // intentar castear a Vector3
                    try
                    {
                        Vector3 pos = (Vector3)posObj;
                        Debug.Log("[KinectDebugProbe] JointIndex " + idx + " pos Vector3: " + pos);
                    }
                    catch
                    {
                        // intentar leer x,y,z por reflexión
                        var t = posObj.GetType();
                        object Get(string name)
                        {
                            var f = t.GetField(name);
                            if (f != null) return f.GetValue(posObj);
                            var p = t.GetProperty(name);
                            if (p != null) return p.GetValue(posObj, null);
                            return null;
                        }
                        var ox = Get("x"); var oy = Get("y"); var oz = Get("z");
                        Debug.Log("[KinectDebugProbe] JointIndex " + idx + " pos fields: x=" + ox + ", y=" + oy + ", z=" + oz);
                    }
                }
                else Debug.Log("[KinectDebugProbe] GetJointPosition devolvió null para index " + idx);
            }
        }

        // fin
    }
}
