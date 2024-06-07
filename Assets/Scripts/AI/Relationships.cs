using System.Collections.Generic;
using UnityEngine;

public class Relationships : MonoBehaviour
{
    public List<Relation> relationship;

    [System.Serializable]
    public class Relation
    {
        public CountrySettings country;

        public int relationship;
        public int relationshipID;

        public bool war;
        public bool vassal;
        public bool union;
        public bool trade;
        public bool pact;
        public bool right;
    }
}
