using UnityEngine;
using UnityEditor;

namespace Pathfinding {
	[CustomEditor(typeof(RecastMeshObj))]
	[CanEditMultipleObjects]
	public class RecastMeshObjEditor : EditorBase {
		protected override void Inspector () {
			var modeProp = FindProperty("mode");
			var areaProp = FindProperty("surfaceID");

			PropertyField(modeProp, "Surface Type");
			var mode = (RecastMeshObj.Mode)modeProp.enumValueIndex;

			if (areaProp.intValue < 0) {
				areaProp.intValue = 0;
			}

			if (!modeProp.hasMultipleDifferentValues) {
				switch (mode) {
				case RecastMeshObj.Mode.ExcludeFromGraph:
					EditorGUILayout.HelpBox("This object will be completely ignored by the graph. Even if it would otherwise be included due to its layer or tag.", MessageType.None);
					break;
				case RecastMeshObj.Mode.UnwalkableSurface:
					EditorGUILayout.HelpBox("All surfaces on this mesh will be made unwalkable", MessageType.None);
					break;
				case RecastMeshObj.Mode.WalkableSurface:
					EditorGUILayout.HelpBox("All surfaces on this mesh will be walkable", MessageType.None);
					break;
				case RecastMeshObj.Mode.WalkableSurfaceWithCustomID:
					EditorGUILayout.HelpBox("All surfaces on this mesh will be walkable and a " +
						"seam will be created between the surfaces on this mesh and the surfaces on other meshes (with a different surface id)", MessageType.None);
					EditorGUI.indentLevel++;
					PropertyField(areaProp, "Surface ID");
					EditorGUI.indentLevel--;
					break;
				}
			}

			var dynamicProp = FindProperty("dynamic");
			PropertyField(dynamicProp, "Dynamic", "Setting this value to false will give better scanning performance, but you will not be able to move the object during runtime");
			if (!dynamicProp.hasMultipleDifferentValues && !dynamicProp.boolValue) {
				EditorGUILayout.HelpBox("This object must not be moved during runtime since 'dynamic' is set to false", MessageType.Info);
			}
		}
	}
}
