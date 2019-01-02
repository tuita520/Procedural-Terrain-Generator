﻿using Terra.Graph.Biome;
using Terra.Graph.Fields;
using UnityEngine;
using XNodeEditor;

namespace Terra.Graph {
	[CustomNodeEditor(typeof(TreeDetailNode))]
	public class TreeNodeEditor: PreviewableNodeEditor {
		private TreeDetailNode TreeDetailNode {
			get {
				return (TreeDetailNode)target;
			}
		}

	    public override void OnBodyGUI() {
            NodeEditorGUILayout.PortField(TreeDetailNode.GetOutputPort("Output"));
			PlaceableObjectField.Show(this);
	        PreviewField.Show(TreeDetailNode);
        }

		public override void ShouldShowPreviewGenerator() { }

		public override Color GetTint() {
			return Constants.TintValue;
		}

		public override string GetTitle() {
			return "Tree";
		}
	}
}