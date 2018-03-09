﻿using Terra.CoherentNoise;
using System;
using Terra.GraphEditor;
using Terra.GraphEditor.Sockets;
using UnityEngine;

namespace Terra.Nodes.Generation {
	public abstract class AbstractGeneratorNode: Node {
		[NonSerialized]
		protected OutputSocket OutSocket;
		[NonSerialized]
		protected Texture2D PreviewTexture;
		[NonSerialized]
		protected bool TextureNeedsUpdating = true;
		[NonSerialized]
		protected Generator Generator;

		[NonSerialized]
		private Rect ErrorMessageLabel;

		protected AbstractGeneratorNode(int id, Graph parent) : base(id, parent) {
			OutSocket = new OutputSocket(this, typeof(AbstractGeneratorNode));
			Sockets.Add(OutSocket);
		}

		public abstract Generator GetGenerator();

		public static Generator GetInputGenerator(InputSocket socket) {
			if (socket.Type == typeof(AbstractGeneratorNode)) {
				if (!socket.IsConnected()) return null;

				return ((AbstractGeneratorNode)socket.GetConnectedSocket().Parent).GetGenerator();
			} else {
				Debug.LogError("InputSocket is not of type AbstractGeneratorNode");
				return null;
			}
		}
	}
}