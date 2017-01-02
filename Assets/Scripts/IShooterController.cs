using UnityEngine;

public interface IShooterController
{
    bool GetTarget(out Vector2 target);

    void OnShoot();

    void OnReload();
}

