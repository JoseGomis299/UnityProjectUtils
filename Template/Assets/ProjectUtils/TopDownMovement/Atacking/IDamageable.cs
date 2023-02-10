using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUtils.Attacking
{
    public interface IDamageable
    {
        public void ReceiveDamage(Damage dmg);
    }
}
