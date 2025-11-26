using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;

/// <summary>
/// Probe robusto: busca KinectManager.Instance, luego intenta llamar a IsJointTracked y GetJointPosition
/// con varias combinaciones de parámetros (userId-int/long/ninguno) para descubrir la firma real.
/// Pega la salida de la consola aquí y te doy la llamada concreta a usar en tu script final.
/// </summary>
public class KinectDebugProbe2 : MonoBehaviour
{
    int[] trialIndices = new int[] { 7, 10, 11 }; // índices típicos para manos en distintos wrappers

    void Update()
    {
        Type kmType = Type.GetType("KinectManager") ?? Type.GetType("KinectManager, Assembly-CSharp");
        if (kmType == null)
        {
            Debug.LogError("[KinectDebugProbe2] No se encontró tipo KinectManager.");
            return;
        }

        // Obtener instancia
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
            Debug.LogError("[KinectDebugProbe2] KinectManager.Instance es null.");
            return;
        }

        Debug.Log("[KinectDebugProbe2] KinectManager encontrado: " + kmType.Name);

        MethodInfo miIsInitialized = kmType.GetMethod("IsInitialized");
        MethodInfo miIsUserDetected = kmType.GetMethod("IsUserDetected");
        MethodInfo miGetPrimaryUser = kmType.GetMethod("GetPrimaryUserID") ?? kmType.GetMethod("GetPrimaryUserId");
        MethodInfo miGetUserByIndex = kmType.GetMethod("GetUserIdByIndex") ?? kmType.GetMethod("GetUserIdByIndex", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

        MethodInfo miIsJointTracked = kmType.GetMethod("IsJointTracked");
        MethodInfo miGetJointPos = kmType.GetMethod("GetJointPosition") ?? kmType.GetMethod("GetJointKinectPosition") ?? kmType.GetMethod("GetJoint");

        Debug.LogFormat("[KinectDebugProbe2] Métodos detectados: IsInitialized={0}, IsUserDetected={1}, GetPrimaryUserID={2}, GetUserByIndex={3}, IsJointTracked={4}, GetJointPos={5}",
            miIsInitialized != null, miIsUserDetected != null, miGetPrimaryUser != null, miGetUserByIndex != null, miIsJointTracked != null, miGetJointPos != null);

        bool initialized = miIsInitialized != null && (bool)miIsInitialized.Invoke(kManager, null);
        Debug.Log("[KinectDebugProbe2] IsInitialized = " + initialized);

        bool userDetected = miIsUserDetected != null && (bool)miIsUserDetected.Invoke(kManager, null);
        Debug.Log("[KinectDebugProbe2] IsUserDetected = " + userDetected);

        // Intentos de userId (varios tipos)
        List<object> userIdCandidates = new List<object>();
        if (miGetPrimaryUser != null)
        {
            try { userIdCandidates.Add(miGetPrimaryUser.Invoke(kManager, null)); Debug.Log("[KinectDebugProbe2] GetPrimaryUserID() -> " + userIdCandidates[userIdCandidates.Count - 1]); }
            catch { Debug.Log("[KinectDebugProbe2] GetPrimaryUserID() lanzó excepción al invocar."); }
        }
        if (miGetUserByIndex != null)
        {
            try { var v = miGetUserByIndex.Invoke(kManager, new object[] { 0 }); userIdCandidates.Add(v); Debug.Log("[KinectDebugProbe2] GetUserIdByIndex(0) -> " + v); }
            catch { Debug.Log("[KinectDebugProbe2] GetUserIdByIndex(0) lanzó excepción."); }
        }

        // Agregamos candidatos por defecto frecuentes (0/1) con distintos tipos
        userIdCandidates.Add((int)0);
        userIdCandidates.Add((long)0L);
        userIdCandidates.Add((uint)0u);
        userIdCandidates.Add((short)0);

        // Helper para intentar invocar con varios argumentos y reportar
        object TryInvoke(MethodInfo mi, object target, object[] args, out Exception exception)
        {
            exception = null;
            if (mi == null) return null;
            try
            {
                return mi.Invoke(target, args);
            }
            catch (Exception ex)
            {
                exception = ex;
                return null;
            }
        }

        if (miIsJointTracked == null || miGetJointPos == null)
        {
            Debug.LogWarning("[KinectDebugProbe2] Falta IsJointTracked o GetJointPosition en esta versión del KinectManager.");
            return;
        }

        // Para cada trial index intentamos invocar IsJointTracked y GetJointPosition con varias firmas posibles
        foreach (int idx in trialIndices)
        {
            Debug.Log("---- Probe jointIndex = " + idx + " ----");

            // Posibles combinaciones de args: (userId, idx), (int userIndex, idx), (idx)
            List<object[]> argOptionsIsTracked = new List<object[]>();
            foreach (var uid in userIdCandidates) argOptionsIsTracked.Add(new object[] { uid, idx });
            argOptionsIsTracked.Add(new object[] { idx });

            bool anyTrackedResult = false;
            foreach (var args in argOptionsIsTracked)
            {
                Exception ex;
                object res = TryInvoke(miIsJointTracked, kManager, args, out ex);
                string argDesc = "(" + string.Join(", ", Array.ConvertAll(args, o => o == null ? "null" : (o.ToString() + " [" + o.GetType().Name + "]"))) + ")";
                if (ex != null)
                {
                    Debug.Log("[KinectDebugProbe2] IsJointTracked" + argDesc + " -> Exception: " + ex.GetBaseException().Message);
                    continue;
                }
                if (res != null)
                {
                    Debug.Log("[KinectDebugProbe2] IsJointTracked" + argDesc + " -> " + res + " (type " + res.GetType().Name + ")");
                    anyTrackedResult = true;
                }
            }
            if (!anyTrackedResult) Debug.Log("[KinectDebugProbe2] Ninguna invocación a IsJointTracked devolvió valor (posible firma inesperada).");

            // Ahora probar GetJointPosition con las mismas combinaciones
            List<object[]> argOptionsGetPos = new List<object[]>();
            foreach (var uid in userIdCandidates) argOptionsGetPos.Add(new object[] { uid, idx });
            argOptionsGetPos.Add(new object[] { idx });

            bool anyPos = false;
            foreach (var args in argOptionsGetPos)
            {
                Exception ex;
                object posObj = TryInvoke(miGetJointPos, kManager, args, out ex);
                string argDesc = "(" + string.Join(", ", Array.ConvertAll(args, o => o == null ? "null" : (o.ToString() + " [" + o.GetType().Name + "]"))) + ")";
                if (ex != null)
                {
                    Debug.Log("[KinectDebugProbe2] GetJointPosition" + argDesc + " -> Exception: " + ex.GetBaseException().Message);
                    continue;
                }
                if (posObj == null)
                {
                    Debug.Log("[KinectDebugProbe2] GetJointPosition" + argDesc + " -> null");
                    continue;
                }

                // Intentar castear a Vector3 o leer x,y,z
                try
                {
                    Vector3 p = (Vector3)posObj;
                    Debug.Log("[KinectDebugProbe2] GetJointPosition" + argDesc + " -> Vector3: " + p);
                    anyPos = true;
                }
                catch
                {
                    var t = posObj.GetType();
                    object GetMember(string name)
                    {
                        var f = t.GetField(name);
                        if (f != null) return f.GetValue(posObj);
                        var pr = t.GetProperty(name);
                        if (pr != null) return pr.GetValue(posObj, null);
                        return null;
                    }
                    var ox = GetMember("x"); var oy = GetMember("y"); var oz = GetMember("z");
                    Debug.Log("[KinectDebugProbe2] GetJointPosition" + argDesc + " -> fields: x=" + ox + ", y=" + oy + ", z=" + oz + " (type " + t.Name + ")");
                    anyPos = true;
                }
            }
            if (!anyPos) Debug.Log("[KinectDebugProbe2] Ninguna invocación a GetJointPosition devolvió posición útil para index " + idx);
        } // foreach idx

    } // Update
}
