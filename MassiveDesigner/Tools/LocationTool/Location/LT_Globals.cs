using System.Collections.Generic;
using UnityEngine;


namespace CodeCreatePlay
{
    namespace LocationTool
    {
        public class LT_Globals : MonoBehaviour
        {
            public List<LocationBase> Locations { get; } = new List<LocationBase>();

            public LocationBase GetRandomLocation(LocationCategory c)
            {
                List<LocationBase> randLocations = new ();
                foreach (var item in Locations)
                {
                    if (item.category == c)
                        randLocations.Add(item);
                }

                if (randLocations.Count > 0)
                    return randLocations[Random.Range(0, randLocations.Count-1)];

                return null;
            }

            public LocationBase GetNearestLocation(Vector3 pos, LocationCategory c)
            {
                return null;
            }

            public LocationBase GetLocation(string name)
            {
                foreach (var item in Locations)
                    if (item.locationName == name)
                        return item;

                return null;
            }

        }
    }
}
