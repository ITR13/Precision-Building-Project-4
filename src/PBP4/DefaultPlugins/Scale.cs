using Modding;
using Modding.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PBP4.DefaultPlugins
{
    public class Scale : Plugin
    {
        public override ApplyRule ApplyRule => ApplyRule.OnChange;

        public override XDataHolder DefaultInput => new XDataHolderFactory()
        {
            { "Scale", Vector3.one }
        }.Create();

        public override string Name => "Scale";

        private Vector3 _scale;

        public override void Apply(XDataHolder input)
        {
            _scale = input.ReadVector3("Scale");
        }

        protected override void OnEnable(bool prevState)
        {
            Events.OnBlockPlaced -= ScaleBlock;
            Events.OnBlockPlaced += ScaleBlock;
        }

        protected override void OnDisable(bool prevState)
        {
            Events.OnBlockPlaced -= ScaleBlock;
        }

        private void ScaleBlock(Block block)
        {
            block.GameObject.transform.localScale = _scale;
        }
    }
}
