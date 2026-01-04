using System;
using UnityEngine;

public class AutoRecycleFx : MonoBehaviour
{
    [SerializeField] private GameObject holder;
    private void OnParticleSystemStopped()
    {
        holder.Recycle();
    }
}
