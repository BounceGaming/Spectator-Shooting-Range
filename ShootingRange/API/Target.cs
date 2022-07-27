using AdminToys;
using Exiled.API.Features.Toys;

namespace ShootingRange.API
{
    public class Target
    {
        public Target(ShootingTargetToy toy, Light light)
        {
            TargetToy = toy;
            LightToy = light;
        }
        
        public ShootingTargetToy TargetToy { get; }
        public Light LightToy { get; }
    }
}