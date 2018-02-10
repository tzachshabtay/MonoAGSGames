using AGS.API;

namespace LastAndFurious
{
    /// <summary>
    /// Class, depicting a driver character, with name, portrait and car model associated with him/her.
    /// </summary>
    public class DriverCharacter
    {
        /// <summary>
        /// Driver's name.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Driver's portrait.
        /// </summary>
        public IImage Portrait { get; set; }
        /// <summary>
        /// Car model image.
        /// </summary>
        public IImage CarModel { get; set; }
        /// <summary>
        /// The original angle car model image has. This information is required
        /// for synchronizing graphics with the vehicle's physics model.
        /// </summary>
        public float CarModelAngle { get; set; }

        public DriverCharacter(string name, IImage portrait, IImage carmodel, float carmodelAngle)
        {
            Name = name;
            Portrait = portrait;
            CarModel = carmodel;
            CarModelAngle = carmodelAngle;
        }
    }
}
