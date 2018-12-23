﻿using System;
using UnityEngine;
using XNode;

namespace Terra.Graph {
	[Serializable]
	public abstract class PreviewableNode: Node {
		public Texture2D PreviewTexture;
		public int PreviewTextureSize = 100;

		[SerializeField]
		public bool IsPreviewDropdown;

		public abstract Texture2D DidRequestTextureUpdate();
	}
}