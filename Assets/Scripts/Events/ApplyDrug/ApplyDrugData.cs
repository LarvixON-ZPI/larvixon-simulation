using System;

namespace Events.ApplyDrug
{
    [Serializable]
    public struct ApplyDrugData
    {
        public Drugs.DrugType drugType;
        public float intensity;
    }
}