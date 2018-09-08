using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PBP4.DefaultPlugins
{
    public class Offset : Plugin
    {
        public override ApplyRule ApplyRule => ApplyRule.OnChange;

        public override XDataHolder DefaultInput => new XDataHolderFactory()
        {
            { "Position", Vector3.zero },
            { "Rotation", Vector3.zero }
        }.Create();

        public override string Name => "Placement Offset";

        public override void Apply(XDataHolder input)
        {
            AddPiece.PlacementOffset.position = input.ReadVector3("Position");
            AddPiece.PlacementOffset.rotation = 
                Quaternion.Euler(
                    input.ReadVector3("Rotation")
                );
        }

        protected override void OnDisable(bool prevState)
        {
            AddPiece.PlacementOffset.position = Vector3.zero;
            AddPiece.PlacementOffset.rotation = Quaternion.identity;
        } 
    }
}
