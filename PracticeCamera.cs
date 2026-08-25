using Il2CppCinemachine;
using Il2CppSpeed.Cameras;
using Il2CppSpeed.Level;
using UnityEngine;
using Object = UnityEngine.Object;

namespace WtcPractice;

public static class PracticeCamera
{
    private static AGameplayCameraController controller;
    private static ThisIsThePlayerVehicle vehicle;

    public static void OnCarWarped(Vector3 delta)
    {
        ResetFollows();
        NotifyCinemachine(delta);
        Reposition();
    }

    public static void Forget()
    {
        controller = null;
        vehicle = null;
    }

    private static void ResetFollows()
    {
        if (EnsureVehicle())
            vehicle.ResetCameraFollows();
    }

    private static void NotifyCinemachine(Vector3 delta)
    {
        if (!EnsureVehicle())
            return;

        Warp(vehicle.follow, delta);

        if (vehicle.lookAt != vehicle.follow)
            Warp(vehicle.lookAt, delta);
    }

    private static void Warp(Transform target, Vector3 delta)
    {
        if (target != null)
            CinemachineCore.Instance.OnTargetObjectWarped(target, delta);
    }

    private static void Reposition()
    {
        if (EnsureController())
            controller.InstantlyReposition();
    }

    private static bool EnsureController()
    {
        if (controller == null)
            controller = Object.FindObjectOfType<AGameplayCameraController>();

        return controller != null;
    }

    private static bool EnsureVehicle()
    {
        if (vehicle == null)
            vehicle = Object.FindObjectOfType<ThisIsThePlayerVehicle>();

        return vehicle != null;
    }
}
