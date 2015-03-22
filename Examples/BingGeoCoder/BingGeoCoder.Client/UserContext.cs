using System;

namespace BingGeoCoder.Client
{
    /// <summary>
    /// Used to provide more accurate results based on the user's locations
    /// Typically only one of the properties will be set
    /// </summary>
    public class UserContext
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ip">The user's ip address</param>
        public UserContext(string ip)
        {
            IPAddress = ip;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="location">The user's location</param>
        public UserContext(Tuple<double, double> location)
        {
            Location = location;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mapView">bounding box of a map view</param>
        public UserContext(Tuple<double, double, double, double> mapView)
        {
            MapView = mapView;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ip">The user's ip address</param>
        /// <param name="location">The user's location</param>
        /// <param name="mapView">bounding box of a map view</param>
        public UserContext(string ip, Tuple<double, double> location, Tuple<double, double, double, double> mapView)
        {
            IPAddress = ip;
            Location = location;
            MapView = mapView;
        }

        /// <summary>
        /// ip address of the user
        /// </summary>
        public string IPAddress { get; private set; }

        /// <summary>
        /// Location of the user
        /// </summary>
        public Tuple<double, double> Location { get; private set; }

        /// <summary>
        /// The bounding box of a map view
        /// </summary>
        public Tuple<double, double, double, double> MapView { get; private set; }
    }
}
