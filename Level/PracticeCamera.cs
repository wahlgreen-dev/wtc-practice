using Il2CppCinemachine;
using Il2CppSpeed.Cameras;
using Il2CppSpeed.Level;
using UnityEngine;
using Object = UnityEngine.Object;

namespace WtcPractice;

public class PracticeCamera
{
    private AGameplayCameraController controller;
    private ThisIsThePlayerVehicle vehicle;

    public void OnCarWarped(Vector3 delta)
    {
        if (EnsureVehicle())
        {
            vehicle.ResetCameraFollows();
            Warp(vehicle.follow, delta);

            if (vehicle.lookAt != vehicle.follow)
                Warp(vehicle.lookAt, delta);
        }

        if (EnsureController())
            controller.InstantlyReposition();
    }

    public void Forget()
    {
        controller = null;
        vehicle = null;
    }

    private static void Warp(Transform target, Vector3 delta)
    {
        if (target != null)
            CinemachineCore.Instance.OnTargetObjectWarped(target, delta);
    }

    private bool EnsureController()
    {
        if (controller == null)
            controller = Object.FindObjectOfType<AGameplayCameraController>();

        return controller != null;
    }

    private bool EnsureVehicle()
    {
        if (vehicle == null)
            vehicle = Object.FindObjectOfType<ThisIsThePlayerVehicle>();

        return vehicle != null;
    }
}
