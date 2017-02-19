using UnityEngine;

public interface IShooter
{
    bool GetTarget(out Vector2 target);

    void OnShoot(Projectile projectile);

    void OnReload();

    void OnReadyToShoot();
}

