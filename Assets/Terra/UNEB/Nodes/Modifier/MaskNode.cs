﻿using Terra.CoherentNoise;
using Terra.CoherentNoise.Generation.Combination;
using System;
using Terra.Nodes.Generation;
using UNEB;
using Assets.Terra.UNEB.Utility;
using UnityEngine;

namespace Terra.Nodes.Modifier {
	[Serializable, GraphContextMenuItem("Modifier", "Mask")]
	public class MaskNode: AbstractTwoModNode {
		[SerializeField]
		private NodeInput InputMaskGenerator;	

		public override void Init() {
			base.Init();

			InputMaskGenerator = AddInput("Mask");
			FitKnobs();

			bodyRect.height += 5f;
		}

		public override Generator GetGenerator() {
			if (InputMaskGenerator == null || !InputMaskGenerator.HasOutputConnected() || 
				!(InputMaskGenerator.GetOutput(0).ParentNode is AbstractGeneratorNode)) {
				return null;
			}

			Generator BlendGenerator = (InputMaskGenerator.GetOutput(0).ParentNode as AbstractGeneratorNode).GetGenerator();
			return BlendGenerator == null || Generator1 == null || Generator2 == null ?
				null : new Blend(Generator1, Generator2, BlendGenerator);
		}

		public override string GetName() {
			return "Mask";
		}

		public override void OnBodyGUI() {
			base.OnBodyGUI();
		}
	}
}
 