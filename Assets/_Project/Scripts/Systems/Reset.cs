using UnityEngine;

namespace InternetShowdown.Systems
{
    // нужна чтобы сбрасывать поля ассетов при остановке плеймода (чтобы гит не видел изменения бесполезные)
    public class Reset : LocalSystem<Reset>
    {
        [SerializeField] private Material _speedlines;

        protected override void OnStop()
        {
            _speedlines.SetFloat("_Alpha", 0f);
        }
    }
}