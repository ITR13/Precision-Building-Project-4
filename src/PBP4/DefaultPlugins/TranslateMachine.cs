using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PBP4.DefaultPlugins
{
    class TransformMachine : Plugin
    {
        public override string Name => "Translate Machine";

        public override ApplyRule ApplyRule => ApplyRule.OnApply;

        public override XDataHolder DefaultInput =>
            new XDataHolderFactory()
            {
                { "Position", Vector3.zero },
                { "Rotation", Vector3.zero },
                { "Scale",    Vector3.one },
            }.Create();

        public override void Apply(XDataHolder input)
        {
            var buildingBlocks = Machine.Active()?.BuildingBlocks;
            var machine = Machine.Active().transform;
            if (buildingBlocks == null)
            {
                return;
            }
            var position = input.ReadVector3("Position");
            var rotation = Quaternion.Euler(input.ReadVector3("Rotation"));
            var scale = input.ReadVector3("Scale");

            var translator = new GameObject().transform;
            translator.position = machine.TransformPoint(position);
            translator.rotation = machine.rotation * rotation;
            translator.localScale = machine.TransformVector(scale);

            Vector3 Translate(Vector3 pos)
            {
                return machine.InverseTransformPoint(
                    translator.TransformPoint(
                        pos
                    )
                );
            }

            foreach (var block in buildingBlocks)
            {
                var dragged = block as GenericDraggedBlock;
                Vector3 start = Vector3.zero, end = Vector3.zero;
                if (dragged)
                {
                    start = dragged.startPoint.position;
                    end = dragged.endPoint.position;

                    start = Translate(start);
                    end = Translate(end);
                }

                
                var transform = block.transform;
                block.SetPosition(Translate(transform.position));
                block.SetRotation(rotation * transform.rotation);
                block.SetScale(
                    transform.InverseTransformVector(
                        Vector3.Scale(
                            transform.TransformVector(
                                transform.localScale
                            ),
                            scale
                        )
                    )
                );

                if (dragged)
                {
                    var parentEuler = dragged.ParentMachine
                                             .boundingBoxController
                                             .transform
                                             .eulerAngles;

                    dragged.SetPositionsGlobal(
                        start,
                        dragged.startPoint.eulerAngles - parentEuler,
                        end,
                        dragged.startPoint.eulerAngles - parentEuler,
                        true
                    );
                }
            }
        }
    }
}
