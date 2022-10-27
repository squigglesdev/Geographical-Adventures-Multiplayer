using UnityEngine;

public class MissileTarget : MonoBehaviour {
    public float health;

    void Update() {
        if(health <= 0) {
            Destroy(gameObject);
        }
    }

    /// 'Hits' the target for a certain amount of damage
    public void Hit(float damage) {
        health -= damage;
    }
}