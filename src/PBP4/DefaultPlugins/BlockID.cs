using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PBP4.DefaultPlugins
{
    class BlockID : Plugin
    {
        public override string Name => "Block ID Picker";

        public override ApplyRule ApplyRule => ApplyRule.OnChange;

        public override XDataHolder DefaultInput => new XDataHolderFactory()
        {
            {"BlockID", (int?)AddPiece.Instance?.CurrentType??1 }
        }.Create();

        public override void Apply(XDataHolder input)
        {
            if (input.HasKey("BlockID"))
            {
                var blockType = input.ReadInt("BlockID");
                AddPiece.Instance?.SetBlockType((BlockType)blockType);
            }
        }

        public BlockID()
        {
            StatMaster.SelectedBlockChanged += (id) =>
            {
                try
                {
                    PluginManager.Instance.UpdateDataAndUI(
                        this,
                        new XDataHolderFactory()
                        {
                            {"BlockID", (int)id }
                        }.Create()
                    );
                }
                catch (Exception e)
                {
                    ModConsole.Log(
                        "Failed to update UI of BlockID:\n" +
                        "BlockID: {0}\n" +
                        "Exception: {1}",
                        id,
                        e
                    );
                }
            };
        }
    }
}
