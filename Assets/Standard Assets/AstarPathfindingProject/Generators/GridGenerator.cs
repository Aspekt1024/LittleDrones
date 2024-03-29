using System.Collections.Generic;
using Math = System.Math;
using UnityEngine;
using System.Linq;
#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif


namespace Pathfinding {
	using Pathfinding.Serialization;
	using Pathfinding.Util;
	using Unity.Collections;
	using Unity.Jobs;
	using Unity.Mathematics;
	using Pathfinding.Jobs;
	using Pathfinding.Jobs.Grid;
	using Unity.Burst;
	using Drawing;

	/// <summary>
	/// Generates a grid of nodes.
	/// The GridGraph does exactly what the name implies, generates nodes in a grid pattern.\n
	/// Grid graphs suit well to when you already have a grid based world.
	/// Features:
	/// - You can update the graph during runtime (good for e.g Tower Defence or RTS games)
	/// - Throw any scene at it, with minimal configurations you can get a good graph from it.
	/// - Supports raycast and the funnel algorithm
	/// - Predictable pattern
	/// - Can apply penalty and walkability values from a supplied image
	/// - Perfect for terrain worlds since it can make areas unwalkable depending on the slope
	///
	/// [Open online documentation to see images]
	/// [Open online documentation to see images]
	///
	/// The <b>The Snap Size</b> button snaps the internal size of the graph to exactly contain the current number of nodes, i.e not contain 100.3 nodes but exactly 100 nodes.\n
	/// This will make the "center" coordinate more accurate.\n
	///
	/// <b>Updating the graph during runtime</b>\n
	/// Any graph which implements the IUpdatableGraph interface can be updated during runtime.\n
	/// For grid graphs this is a great feature since you can update only a small part of the grid without causing any lag like a complete rescan would.\n
	/// If you for example just have instantiated a sphere obstacle in the scene and you want to update the grid where that sphere was instantiated, you can do this:\n
	/// <code> AstarPath.active.UpdateGraphs (ob.collider.bounds); </code>
	/// Where ob is the obstacle you just instantiated (a GameObject).\n
	/// As you can see, the UpdateGraphs function takes a Bounds parameter and it will send an update call to all updateable graphs.\n
	/// A grid graph will update that area and a small margin around it equal to \link Pathfinding.GraphCollision.diameter collision testing diameter/2 \endlink
	/// See: graph-updates (view in online documentation for working links) for more info about updating graphs during runtime
	///
	/// <b>Hexagon graphs</b>\n
	/// The graph can be configured to work like a hexagon graph with some simple settings. Since 4.1.x the grid graph has a 'Shape' dropdown.
	/// If you set it to 'Hexagonal' the graph will behave as a hexagon graph.
	/// Often you may want to rotate the graph +45 or -45 degrees.
	/// [Open online documentation to see images]
	///
	/// Note however that the snapping to the closest node is not exactly as you would expect in a real hexagon graph,
	/// but it is close enough that you will likely not notice.
	///
	/// <b>Configure using code</b>\n
	/// <code>
	/// // This holds all graph data
	/// AstarData data = AstarPath.active.data;
	///
	/// // This creates a Grid Graph
	/// GridGraph gg = data.AddGraph(typeof(GridGraph)) as GridGraph;
	///
	/// // Setup a grid graph with some values
	/// int width = 50;
	/// int depth = 50;
	/// float nodeSize = 1;
	///
	/// gg.center = new Vector3(10, 0, 0);
	///
	/// // Updates internal size from the above values
	/// gg.SetDimensions(width, depth, nodeSize);
	///
	/// // Scans all graphs
	/// AstarPath.active.Scan();
	/// </code>
	///
	/// \ingroup graphs
	/// \nosubgrouping
	///
	/// <b>Tree colliders</b>\n
	/// It seems that Unity will only generate tree colliders at runtime when the game is started.
	/// For this reason, the grid graph will not pick up tree colliders when outside of play mode
	/// but it will pick them up once the game starts. If it still does not pick them up
	/// make sure that the trees actually have colliders attached to them and that the tree prefabs are
	/// in the correct layer (the layer should be included in the 'Collision Testing' mask).
	///
	/// See: <see cref="Pathfinding.GraphCollision"/> for documentation on the 'Height Testing' and 'Collision Testing' sections
	/// of the grid graph settings.
	/// </summary>
	[JsonOptIn]
	[Pathfinding.Util.Preserve]
	public class GridGraph : NavGraph, IUpdatableGraph, ITransformedGraph
		, IRaycastableGraph {
		protected override void DisposeUnmanagedData () {
			// Destroy all nodes to make the graph go into an unscanned state
			DestroyAllNodes();

			// Clean up a reference in a static variable which otherwise should point to this graph forever and stop the GC from collecting it
			RemoveGridGraphFromStatic();

			// Dispose of native arrays. This is very important to avoid memory leaks!
			rules.DisposeUnmanagedData();
			if (nodeSurfaceNormals.IsCreated) nodeSurfaceNormals.Dispose();
		}

		protected override void DestroyAllNodes () {
			GetNodes(node => {
				// If the grid data happens to be invalid (e.g we had to abort a graph update while it was running) using 'false' as
				// the parameter will prevent the Destroy method from potentially throwing IndexOutOfRange exceptions due to trying
				// to access nodes outside the graph. It is safe to do this because we are destroying all nodes in the graph anyway.
				// We do however need to clear custom connections in both directions
				(node as GridNodeBase).ClearCustomConnections(true);
				node.ClearConnections(false);
				node.Destroy();
			});
			// Important: so that multiple calls to DestroyAllNodes still works
			nodes = null;
		}

		void RemoveGridGraphFromStatic () {
			GridNode.SetGridGraph(AstarPath.active.data.GetGraphIndex(this), null);
		}

		/// <summary>
		/// This is placed here so generators inheriting from this one can override it and set it to false.
		/// If it is true, it means that the nodes array's length will always be equal to width*depth
		/// It is used mainly in the editor to do auto-scanning calls, setting it to false for a non-uniform grid will reduce the number of scans
		/// </summary>
		public virtual bool uniformWidthDepthGrid {
			get {
				return true;
			}
		}

		/// <summary>
		/// Number of layers in the graph.
		/// For grid graphs this is always 1, for layered grid graphs it can be higher.
		/// The nodes array has the size width*depth*layerCount.
		/// </summary>
		public virtual int LayerCount {
			get {
				return 1;
			}
		}

		public virtual int MaxLayers {
			get {
				return 1;
			}
		}

		public override int CountNodes () {
			return nodes != null ? nodes.Length : 0;
		}

		public override void GetNodes (System.Action<GraphNode> action) {
			if (nodes == null) return;
			for (int i = 0; i < nodes.Length; i++) action(nodes[i]);
		}

		/// <summary>
		/// \name Inspector - Settings
		/// \{
		/// </summary>

		/// <summary>
		/// Determines the layout of the grid graph inspector in the Unity Editor.
		/// This field is only used in the editor, it has no effect on the rest of the game whatsoever.
		///
		/// If you want to change the grid shape like in the inspector you can use the <see cref="SetGridShape"/> method.
		/// </summary>
		[JsonMember]
		public InspectorGridMode inspectorGridMode = InspectorGridMode.Grid;

		/// <summary>
		/// Determines how the size of each hexagon is set in the inspector.
		/// For hexagons the normal nodeSize field doesn't really correspond to anything specific on the hexagon's geometry, so this enum is used to give the user the opportunity to adjust more concrete dimensions of the hexagons
		/// without having to pull out a calculator to calculate all the square roots and complicated conversion factors.
		///
		/// This field is only used in the graph inspector, the <see cref="nodeSize"/> field will always use the same internal units.
		/// If you want to set the node size through code then you can use <see cref="ConvertHexagonSizeToNodeSize"/>.
		///
		/// [Open online documentation to see images]
		///
		/// See: <see cref="InspectorGridHexagonNodeSize"/>
		/// See: <see cref="ConvertHexagonSizeToNodeSize"/>
		/// See: <see cref="ConvertNodeSizeToHexagonSize"/>
		/// </summary>
		[JsonMember]
		public InspectorGridHexagonNodeSize inspectorHexagonSizeMode = InspectorGridHexagonNodeSize.Width;

		/// <summary>Width of the grid in nodes. See: SetDimensions</summary>
		public int width;

		/// <summary>Depth (height) of the grid in nodes. See: SetDimensions</summary>
		public int depth;

		/// <summary>
		/// Scaling of the graph along the X axis.
		/// This should be used if you want different scales on the X and Y axis of the grid
		/// </summary>
		[JsonMember]
		public float aspectRatio = 1F;

		/// <summary>
		/// Angle to use for the isometric projection.
		/// If you are making a 2D isometric game, you may want to use this parameter to adjust the layout of the graph to match your game.
		/// This will essentially scale the graph along one of its diagonals to produce something like this:
		///
		/// A perspective view of an isometric graph.
		/// [Open online documentation to see images]
		///
		/// A top down view of an isometric graph. Note that the graph is entirely 2D, there is no perspective in this image.
		/// [Open online documentation to see images]
		///
		/// For commonly used values see <see cref="StandardIsometricAngle"/> and <see cref="StandardDimetricAngle"/>.
		///
		/// Usually the angle that you want to use is either 30 degrees (alternatively 90-30 = 60 degrees) or atan(1/sqrt(2)) which is approximately 35.264 degrees (alternatively 90 - 35.264 = 54.736 degrees).
		/// You might also want to rotate the graph plus or minus 45 degrees around the Y axis to get the oritientation required for your game.
		///
		/// You can read more about it on the wikipedia page linked below.
		///
		/// See: http://en.wikipedia.org/wiki/Isometric_projection
		/// See: https://en.wikipedia.org/wiki/Isometric_graphics_in_video_games_and_pixel_art
		/// See: rotation
		/// </summary>
		[JsonMember]
		public float isometricAngle;

		/// <summary>Commonly used value for <see cref="isometricAngle"/></summary>
		public static readonly float StandardIsometricAngle = 90-Mathf.Atan(1/Mathf.Sqrt(2))*Mathf.Rad2Deg;

		/// <summary>Commonly used value for <see cref="isometricAngle"/></summary>
		public static readonly float StandardDimetricAngle = Mathf.Acos(1/2f)*Mathf.Rad2Deg;

		/// <summary>
		/// If true, all edge costs will be set to the same value.
		/// If false, diagonals will cost more.
		/// This is useful for a hexagon graph where the diagonals are actually the same length as the
		/// normal edges (since the graph has been skewed)
		/// </summary>
		[JsonMember]
		public bool uniformEdgeCosts;

		/// <summary>Rotation of the grid in degrees</summary>
		[JsonMember]
		public Vector3 rotation;

		/// <summary>Center point of the grid</summary>
		[JsonMember]
		public Vector3 center;

		/// <summary>Size of the grid. Might be negative or smaller than <see cref="nodeSize"/></summary>
		[JsonMember]
		public Vector2 unclampedSize;

		/// <summary>
		/// Size of one node in world units.
		/// See: <see cref="SetDimensions"/>
		/// </summary>
		[JsonMember]
		public float nodeSize = 1;

		/* Collision and stuff */

		/// <summary>Settings on how to check for walkability and height</summary>
		[JsonMember]
		public GraphCollision collision;

		/// <summary>
		/// The max y coordinate difference between two nodes to enable a connection.
		/// Set to 0 to ignore the value.
		///
		/// This affects for example how the graph is generated around ledges and stairs.
		///
		/// See: <see cref="maxStepUsesSlope"/>
		/// Version: Was previously called maxClimb
		/// </summary>
		[JsonMember]
		public float maxStepHeight = 0.4F;

		/// <summary>
		/// The max y coordinate difference between two nodes to enable a connection.
		/// Deprecated: This field has been renamed to <see cref="maxStepHeight"/>
		/// </summary>
		[System.Obsolete("This field has been renamed to maxStepHeight")]
		public float maxClimb {
			get {
				return maxStepHeight;
			}
			set {
				maxStepHeight = value;
			}
		}

		/// <summary>
		/// Take the slope into account for <see cref="maxClimb"/>.
		///
		/// When this is enabled the normals of the terrain will be used to make more accurate estimates of how large the steps are between adjacent nodes.
		///
		/// When this is disabled then calculated step between two nodes is their y coordinate difference. This may be inaccurate, especially at the start of steep slopes.
		///
		/// [Open online documentation to see images]
		///
		/// In the image below you can see an example of what happens near a ramp.
		/// In the topmost image the ramp is not connected with the rest of the graph which is obviously not what we want.
		/// In the middle image an attempt has been made to raise the max step height while keeping <see cref="maxStepUsesSlope"/> disabled. However this causes too many connections to be added.
		/// The agent should not be able to go up the ramp from the side.
		/// Finally in the bottommost image the <see cref="maxStepHeight"/> has been restored to the original value but <see cref="maxStepUsesSlope"/> has been enabled. This configuration handles the ramp in a much smarter way.
		/// Note that all the values in the image are just example values, they may be different for your scene.
		/// [Open online documentation to see images]
		///
		/// See: <see cref="maxStepHeight"/>
		/// </summary>
		[JsonMember]
		public bool maxStepUsesSlope = true;

		/// <summary>The max slope in degrees for a node to be walkable.</summary>
		[JsonMember]
		public float maxSlope = 90;

		/// <summary>
		/// Use heigh raycasting normal for max slope calculation.
		/// True if <see cref="maxSlope"/> is less than 90 degrees.
		/// </summary>
		protected bool useRaycastNormal { get { return Math.Abs(90-maxSlope) > float.Epsilon; } }

		/// <summary>
		/// Erosion of the graph.
		/// The graph can be eroded after calculation.
		/// This means a margin is put around unwalkable nodes or other unwalkable connections.
		/// It is really good if your graph contains ledges where the nodes without erosion are walkable too close to the edge.
		///
		/// Below is an image showing a graph with erode iterations 0, 1 and 2
		/// [Open online documentation to see images]
		///
		/// Note: A high number of erode iterations can seriously slow down graph updates during runtime (GraphUpdateObject)
		/// and should be kept as low as possible.
		/// See: erosionUseTags
		/// </summary>
		[JsonMember]
		public int erodeIterations;

		/// <summary>
		/// Use tags instead of walkability for erosion.
		/// Tags will be used for erosion instead of marking nodes as unwalkable. The nodes will be marked with tags in an increasing order starting with the tag <see cref="erosionFirstTag"/>.
		/// Debug with the Tags mode to see the effect. With this enabled you can in effect set how close different AIs are allowed to get to walls using the Valid Tags field on the Seeker component.
		/// [Open online documentation to see images]
		/// [Open online documentation to see images]
		/// See: erosionFirstTag
		/// </summary>
		[JsonMember]
		public bool erosionUseTags;

		/// <summary>
		/// Tag to start from when using tags for erosion.
		/// See: <see cref="erosionUseTags"/>
		/// See: <see cref="erodeIterations"/>
		/// </summary>
		[JsonMember]
		public int erosionFirstTag = 1;


		/// <summary>
		/// Number of neighbours for each node.
		/// Either four, six, eight connections per node.
		///
		/// Six connections is primarily for emulating hexagon graphs.
		/// </summary>
		[JsonMember]
		public NumNeighbours neighbours = NumNeighbours.Eight;

		/// <summary>
		/// If disabled, will not cut corners on obstacles.
		/// If \link <see cref="neighbours"/> connections \endlink is Eight, obstacle corners might be cut by a connection,
		/// setting this to false disables that. \image html images/cutCorners.png
		/// </summary>
		[JsonMember]
		public bool cutCorners = true;

		/// <summary>
		/// Offset for the position when calculating penalty.
		/// Deprecated: Use the RuleElevationPenalty class instead
		/// See: penaltyPosition
		/// </summary>
		[JsonMember]
		[System.Obsolete("Use the RuleElevationPenalty class instead")]
		public float penaltyPositionOffset;

		/// <summary>
		/// Use position (y-coordinate) to calculate penalty.
		/// Deprecated: Use the RuleElevationPenalty class instead
		/// </summary>
		[JsonMember]
		[System.Obsolete("Use the RuleElevationPenalty class instead")]
		public bool penaltyPosition;

		/// <summary>
		/// Scale factor for penalty when calculating from position.
		/// Deprecated: Use the RuleElevationPenalty class instead
		/// See: penaltyPosition
		/// </summary>
		[JsonMember]
		[System.Obsolete("Use the RuleElevationPenalty class instead")]
		public float penaltyPositionFactor = 1F;

		/// <summary>Deprecated: Use the RuleAnglePenalty class instead</summary>
		[JsonMember]
		[System.Obsolete("Use the RuleAnglePenalty class instead")]
		public bool penaltyAngle;

		/// <summary>
		/// How much penalty is applied depending on the slope of the terrain.
		/// At a 90 degree slope (not that exactly 90 degree slopes can occur, but almost 90 degree), this penalty is applied.
		/// At a 45 degree slope, half of this is applied and so on.
		/// Note that you may require very large values, a value of 1000 is equivalent to the cost of moving 1 world unit.
		///
		/// Deprecated: Use the RuleAnglePenalty class instead
		/// </summary>
		[JsonMember]
		[System.Obsolete("Use the RuleAnglePenalty class instead")]
		public float penaltyAngleFactor = 100F;

		/// <summary>
		/// How much extra to penalize very steep angles.
		///
		/// Deprecated: Use the RuleAnglePenalty class instead
		/// </summary>
		[JsonMember]
		[System.Obsolete("Use the RuleAnglePenalty class instead")]
		public float penaltyAnglePower = 1;

		/// <summary>
		/// Additional rules to use when scanning the grid graph.
		///
		/// <code>
		/// // Get the first grid graph in the scene
		/// var gridGraph = AstarPath.active.data.gridGraph;
		///
		/// gridGraph.rules.rules.Add(new RuleAnglePenalty {
		///     penaltyScale = 10000,
		///     curve = AnimationCurve.Linear(0, 0, 90, 1),
		/// });
		/// </code>
		///
		/// See: <see cref="GridGraphRules"/>
		/// See: <see cref="GridGraphRule"/>
		/// </summary>
		[JsonMember]
		public GridGraphRules rules = new GridGraphRules();

		/// <summary>
		/// Use jump point search to speed up pathfinding.
		/// Jump point search can improve pathfinding performance significantly by making use of some assumptions of the graph.
		/// More specifically it assumes a grid graph with no penalties.
		///
		/// See: https://en.wikipedia.org/wiki/Jump_point_search
		/// </summary>
		[JsonMember]
		public bool useJumpPointSearch;

		/// <summary>Show an outline of the grid nodes in the Unity Editor</summary>
		[JsonMember]
		public bool showMeshOutline = true;

		/// <summary>Show the connections between the grid nodes in the Unity Editor</summary>
		[JsonMember]
		public bool showNodeConnections;

		/// <summary>Show the surface of the graph. Each node will be drawn as a square (unless e.g hexagon graph mode has been enabled).</summary>
		[JsonMember]
		public bool showMeshSurface = true;

		/// <summary>
		/// Holds settings for using a texture as source for a grid graph.
		/// Texure data can be used for fine grained control over how the graph will look.
		/// It can be used for positioning, penalty and walkability control.\n
		/// Below is a screenshot of a grid graph with a penalty map applied.
		/// It has the effect of the AI taking the longer path along the green (low penalty) areas.\n
		/// [Open online documentation to see images]
		/// Color data is got as 0...255 values.
		///
		/// Warning: Can only be used with Unity 3.4 and up
		///
		/// Deprecated: Use the RuleTexture class instead
		/// </summary>
		[JsonMember]
		[System.Obsolete("Use the RuleTexture class instead")]
		public TextureData textureData = new TextureData();

		/// <summary>\}</summary>

		/// <summary>
		/// Size of the grid. Will always be positive and larger than <see cref="nodeSize"/>.
		/// See: <see cref="UpdateTransform"/>
		/// </summary>
		public Vector2 size { get; protected set; }

		/* End collision and stuff */

		/// <summary>
		/// Index offset to get neighbour nodes. Added to a node's index to get a neighbour node index.
		///
		/// <code>
		///         Z
		///         |
		///         |
		///
		///      6  2  5
		///       \ | /
		/// --  3 - X - 1  ----- X
		///       / | \
		///      7  0  4
		///
		///         |
		///         |
		/// </code>
		/// </summary>
		[System.NonSerialized]
		public readonly int[] neighbourOffsets = new int[8];

		/// <summary>Costs to neighbour nodes</summary>
		[System.NonSerialized]
		public readonly uint[] neighbourCosts = new uint[8];

		/// <summary>Offsets in the X direction for neighbour nodes. Only 1, 0 or -1</summary>
		public static readonly int[] neighbourXOffsets = { 0, 1, 0, -1, 1, 1, -1, -1 };

		/// <summary>Offsets in the Z direction for neighbour nodes. Only 1, 0 or -1</summary>
		public static readonly int[] neighbourZOffsets = { -1, 0, 1, 0, -1, 1, 1, -1 };

		/// <summary>Which neighbours are going to be used when <see cref="neighbours"/>=6</summary>
		internal static readonly int[] hexagonNeighbourIndices = { 0, 1, 5, 2, 3, 7 };

		/// <summary>In GetNearestForce, determines how far to search after a valid node has been found</summary>
		public const int getNearestForceOverlap = 2;

		/// <summary>
		/// All nodes in this graph.
		/// Nodes are laid out row by row.
		///
		/// The first node has grid coordinates X=0, Z=0, the second one X=1, Z=0\n
		/// the last one has grid coordinates X=width-1, Z=depth-1.
		///
		/// <code>
		/// var gg = AstarPath.active.data.gridGraph;
		/// int x = 5;
		/// int z = 8;
		/// GridNodeBase node = gg.nodes[z*gg.width + x];
		/// </code>
		///
		/// See: <see cref="GetNode"/>
		/// See: <see cref="GetNodes"/>
		/// </summary>
		public GridNodeBase[] nodes;

		/// <summary>
		/// Surface normal for each node.
		/// This needs to be saved when the <see cref="maxStepUsesSlope"/> option is enabled for graph updates to work.
		/// </summary>
		NativeArray<float4> nodeSurfaceNormals;

		/// <summary>
		/// Determines how the graph transforms graph space to world space.
		/// See: <see cref="UpdateTransform"/>
		/// </summary>
		public GraphTransform transform { get; private set; }

		/// <summary>
		/// Delegate which creates and returns a single instance of the node type for this graph.
		/// This may be set in the constructor for graphs inheriting from the GridGraph to change the node type of the graph.
		/// </summary>
		protected System.Func<GridNodeBase> newGridNodeDelegate = () => new GridNode();

		protected virtual GridNodeBase[] AllocateNodesJob (int size, out JobHandle dependency) {
			var newNodes = new GridNodeBase[size];

			dependency = active.AllocateNodes(newNodes, size, newGridNodeDelegate);
			return newNodes;
		}

		/// <summary>Used for using a texture as a source for a grid graph.</summary>
		public class TextureData {
			public bool enabled;
			public Texture2D source;
			public float[] factors = new float[3];
			public ChannelUse[] channels = new ChannelUse[3];

			Color32[] data;

			/// <summary>Reads texture data</summary>
			public void Initialize () {
				if (enabled && source != null) {
					for (int i = 0; i < channels.Length; i++) {
						if (channels[i] != ChannelUse.None) {
							try {
								data = source.GetPixels32();
							} catch (UnityException e) {
								Debug.LogWarning(e.ToString());
								data = null;
							}
							break;
						}
					}
				}
			}

			/// <summary>Applies the texture to the node</summary>
			public void Apply (GridNode node, int x, int z) {
				if (enabled && data != null && x < source.width && z < source.height) {
					Color32 col = data[z*source.width+x];

					if (channels[0] != ChannelUse.None) {
						ApplyChannel(node, x, z, col.r, channels[0], factors[0]);
					}

					if (channels[1] != ChannelUse.None) {
						ApplyChannel(node, x, z, col.g, channels[1], factors[1]);
					}

					if (channels[2] != ChannelUse.None) {
						ApplyChannel(node, x, z, col.b, channels[2], factors[2]);
					}

					node.WalkableErosion = node.Walkable;
				}
			}

			/// <summary>Applies a value to the node using the specified ChannelUse</summary>
			void ApplyChannel (GridNode node, int x, int z, int value, ChannelUse channelUse, float factor) {
				switch (channelUse) {
				case ChannelUse.Penalty:
					node.Penalty += (uint)Mathf.RoundToInt(value*factor);
					break;
				case ChannelUse.Position:
					node.position = GridNode.GetGridGraph(node.GraphIndex).GraphPointToWorld(x, z, value);
					break;
				case ChannelUse.WalkablePenalty:
					if (value == 0) {
						node.Walkable = false;
					} else {
						node.Penalty += (uint)Mathf.RoundToInt((value-1)*factor);
					}
					break;
				}
			}

			public enum ChannelUse {
				None,
				Penalty,
				Position,
				WalkablePenalty,
			}
		}

		public GridGraph () {
			unclampedSize = new Vector2(10, 10);
			nodeSize = 1F;
			collision = new GraphCollision();
			transform = new GraphTransform(Matrix4x4.identity);
		}

		public override void RelocateNodes (Matrix4x4 deltaMatrix) {
			// It just makes a lot more sense to use the other overload and for that case we don't have to serialize the matrix
			throw new System.Exception("This method cannot be used for Grid Graphs. Please use the other overload of RelocateNodes instead");
		}

		/// <summary>
		/// Relocate the grid graph using new settings.
		/// This will move all nodes in the graph to new positions which matches the new settings.
		/// </summary>
		public void RelocateNodes (Vector3 center, Quaternion rotation, float nodeSize, float aspectRatio = 1, float isometricAngle = 0) {
			var previousTransform = transform;

			this.center = center;
			this.rotation = rotation.eulerAngles;
			this.aspectRatio = aspectRatio;
			this.isometricAngle = isometricAngle;

			SetDimensions(width, depth, nodeSize);

			GetNodes(node => {
				var gnode = node as GridNodeBase;
				var height = previousTransform.InverseTransform((Vector3)node.position).y;
				node.position = GraphPointToWorld(gnode.XCoordinateInGrid, gnode.ZCoordinateInGrid, height);
			});
		}

		/// <summary>
		/// Transform a point in graph space to world space.
		/// This will give you the node position for the node at the given x and z coordinate
		/// if it is at the specified height above the base of the graph.
		/// </summary>
		public Int3 GraphPointToWorld (int x, int z, float height) {
			return (Int3)transform.Transform(new Vector3(x+0.5f, height, z+0.5f));
		}

		public static float ConvertHexagonSizeToNodeSize (InspectorGridHexagonNodeSize mode, float value) {
			if (mode == InspectorGridHexagonNodeSize.Diameter) value *= 1.5f/(float)System.Math.Sqrt(2.0f);
			else if (mode == InspectorGridHexagonNodeSize.Width) value *= (float)System.Math.Sqrt(3.0f/2.0f);
			return value;
		}

		public static float ConvertNodeSizeToHexagonSize (InspectorGridHexagonNodeSize mode, float value) {
			if (mode == InspectorGridHexagonNodeSize.Diameter) value *= (float)System.Math.Sqrt(2.0f)/1.5f;
			else if (mode == InspectorGridHexagonNodeSize.Width) value *= (float)System.Math.Sqrt(2.0f/3.0f);
			return value;
		}

		public int Width {
			get {
				return width;
			}
			set {
				width = value;
			}
		}
		public int Depth {
			get {
				return depth;
			}
			set {
				depth = value;
			}
		}

		public uint GetConnectionCost (int dir) {
			return neighbourCosts[dir];
		}

		/// <summary>Deprecated:</summary>
		[System.Obsolete("Use GridNode.HasConnectionInDirection instead")]
		public GridNode GetNodeConnection (GridNode node, int dir) {
			if (!node.HasConnectionInDirection(dir)) return null;
			if (!node.EdgeNode) {
				return nodes[node.NodeInGridIndex + neighbourOffsets[dir]] as GridNode;
			} else {
				int index = node.NodeInGridIndex;
				//int z = Math.DivRem (index,Width, out x);
				int z = index/Width;
				int x = index - z*Width;

				return GetNodeConnection(index, x, z, dir);
			}
		}

		/// <summary>Deprecated:</summary>
		[System.Obsolete("Use GridNode.HasConnectionInDirection instead")]
		public bool HasNodeConnection (GridNode node, int dir) {
			if (!node.HasConnectionInDirection(dir)) return false;
			if (!node.EdgeNode) {
				return true;
			} else {
				int index = node.NodeInGridIndex;
				int z = index/Width;
				int x = index - z*Width;

				return HasNodeConnection(index, x, z, dir);
			}
		}

		/// <summary>Deprecated:</summary>
		[System.Obsolete("Use GridNode.SetConnectionInternal instead")]
		public void SetNodeConnection (GridNode node, int dir, bool value) {
			int index = node.NodeInGridIndex;
			int z = index/Width;
			int x = index - z*Width;

			SetNodeConnection(index, x, z, dir, value);
		}

		/// <summary>
		/// Get the connecting node from the node at (x,z) in the specified direction.
		/// Returns: A GridNode if the node has a connection to that node. Null if no connection in that direction exists
		///
		/// See: GridNode
		///
		/// Deprecated:
		/// </summary>
		[System.Obsolete("Use GridNode.HasConnectionInDirection instead")]
		private GridNode GetNodeConnection (int index, int x, int z, int dir) {
			if (!nodes[index].HasConnectionInDirection(dir)) return null;

			/// <summary>TODO: Mark edge nodes and only do bounds checking for them</summary>
			int nx = x + neighbourXOffsets[dir];
			if (nx < 0 || nx >= Width) return null; /// <summary>TODO: Modify to get adjacent grid graph here</summary>
			int nz = z + neighbourZOffsets[dir];
			if (nz < 0 || nz >= Depth) return null;
			int nindex = index + neighbourOffsets[dir];

			return nodes[nindex] as GridNode;
		}

		/// <summary>
		/// Set if connection in the specified direction should be enabled.
		/// Note that bounds checking will still be done when getting the connection value again,
		/// so it is not necessarily true that HasNodeConnection will return true just because you used
		/// SetNodeConnection on a node to set a connection to true.
		///
		/// Note: This is identical to Pathfinding.Node.SetConnectionInternal
		///
		/// Deprecated:
		/// </summary>
		/// <param name="index">Index of the node</param>
		/// <param name="x">X coordinate of the node</param>
		/// <param name="z">Z coordinate of the node</param>
		/// <param name="dir">Direction from 0 up to but excluding 8.</param>
		/// <param name="value">Enable or disable the connection</param>
		[System.Obsolete("Use GridNode.SetConnectionInternal instead")]
		public void SetNodeConnection (int index, int x, int z, int dir, bool value) {
			(nodes[index] as GridNode).SetConnectionInternal(dir, value);
		}

		/// <summary>Deprecated:</summary>
		[System.Obsolete("Use GridNode.HasConnectionInDirection instead")]
		public bool HasNodeConnection (int index, int x, int z, int dir) {
			if (!nodes[index].HasConnectionInDirection(dir)) return false;

			/// <summary>TODO: Mark edge nodes and only do bounds checking for them</summary>
			int nx = x + neighbourXOffsets[dir];
			if (nx < 0 || nx >= Width) return false; /// <summary>TODO: Modify to get adjacent grid graph here</summary>
			int nz = z + neighbourZOffsets[dir];
			if (nz < 0 || nz >= Depth) return false;

			return true;
		}

		/// <summary>
		/// Changes the grid shape.
		/// This is equivalent to changing the 'shape' dropdown in the grid graph inspector.
		///
		/// Calling this method will set <see cref="isometricAngle"/>, <see cref="aspectRatio"/>, <see cref="uniformEdgeCosts"/> and <see cref="neighbours"/>
		/// to appropriate values for that shape.
		///
		/// Note: Setting the shape to <see cref="InspectorGridMode.Advanced"/> does not do anything except set the <see cref="inspectorGridMode"/> field.
		///
		/// See: <see cref="inspectorHexagonSizeMode"/>
		/// </summary>
		public void SetGridShape (InspectorGridMode shape) {
			switch (shape) {
			case InspectorGridMode.Grid:
				isometricAngle = 0;
				aspectRatio = 1;
				uniformEdgeCosts = false;
				if (neighbours == NumNeighbours.Six) neighbours = NumNeighbours.Eight;
				break;
			case InspectorGridMode.Hexagonal:
				isometricAngle = StandardIsometricAngle;
				aspectRatio = 1;
				uniformEdgeCosts = true;
				neighbours = NumNeighbours.Six;
				break;
			case InspectorGridMode.IsometricGrid:
				uniformEdgeCosts = false;
				if (neighbours == NumNeighbours.Six) neighbours = NumNeighbours.Eight;
				isometricAngle = StandardIsometricAngle;
				break;
			case InspectorGridMode.Advanced:
			default:
				break;
			}
			inspectorGridMode = shape;
		}
		/// <summary>
		/// Updates <see cref="unclampedSize"/> from <see cref="width"/>, <see cref="depth"/> and <see cref="nodeSize"/> values.
		/// Also \link UpdateTransform generates a new matrix \endlink.
		/// Note: This does not rescan the graph, that must be done with Scan
		///
		/// You should use this method instead of setting the <see cref="width"/> and <see cref="depth"/> fields
		/// as the grid dimensions are not defined by the <see cref="width"/> and <see cref="depth"/> variables but by
		/// the <see cref="unclampedSize"/> and <see cref="center"/> variables.
		///
		/// <code>
		/// var gg = AstarPath.active.data.gridGraph;
		/// var width = 80;
		/// var depth = 60;
		/// var nodeSize = 1.0f;
		///
		/// gg.SetDimensions(width, depth, nodeSize);
		///
		/// // Recalculate the graph
		/// AstarPath.active.Scan();
		/// </code>
		/// </summary>
		public void SetDimensions (int width, int depth, float nodeSize) {
			unclampedSize = new Vector2(width, depth)*nodeSize;
			this.nodeSize = nodeSize;
			UpdateTransform();
		}

		/// <summary>Updates <see cref="unclampedSize"/> from <see cref="width"/>, <see cref="depth"/> and <see cref="nodeSize"/> values. Deprecated: Use <see cref="SetDimensions"/> instead</summary>
		[System.Obsolete("Use SetDimensions instead")]
		public void UpdateSizeFromWidthDepth () {
			SetDimensions(width, depth, nodeSize);
		}

		/// <summary>
		/// Generates the matrix used for translating nodes from grid coordinates to world coordinates.
		/// Deprecated: This method has been renamed to <see cref="UpdateTransform"/>
		/// </summary>
		[System.Obsolete("This method has been renamed to UpdateTransform")]
		public void GenerateMatrix () {
			UpdateTransform();
		}

		/// <summary>
		/// Updates the <see cref="transform"/> field which transforms graph space to world space.
		/// In graph space all nodes are laid out in the XZ plane with the first node having a corner in the origin.
		/// One unit in graph space is one node so the first node in the graph is at (0.5,0) the second one at (1.5,0) etc.
		///
		/// This takes the current values of the parameters such as position and rotation into account.
		/// The transform that was used the last time the graph was scanned is stored in the <see cref="transform"/> field.
		///
		/// The <see cref="transform"/> field is calculated using this method when the graph is scanned.
		/// The width, depth variables are also updated based on the <see cref="unclampedSize"/> field.
		/// </summary>
		public void UpdateTransform () {
			CalculateDimensions(out width, out depth, out nodeSize);
			transform = CalculateTransform();
		}

		/// <summary>
		/// Returns a new transform which transforms graph space to world space.
		/// Does not update the <see cref="transform"/> field.
		/// See: <see cref="UpdateTransform"/>
		/// </summary>
		public GraphTransform CalculateTransform () {
			int newWidth, newDepth;
			float newNodeSize;

			CalculateDimensions(out newWidth, out newDepth, out newNodeSize);

			// Generate a matrix which shrinks the graph along one of the diagonals
			// corresponding to the isometricAngle
			var isometricMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 45, 0), Vector3.one);
			isometricMatrix = Matrix4x4.Scale(new Vector3(Mathf.Cos(Mathf.Deg2Rad*isometricAngle), 1, 1)) * isometricMatrix;
			isometricMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, -45, 0), Vector3.one) * isometricMatrix;

			// Generate a matrix for the bounds of the graph
			// This moves a point to the correct offset in the world and the correct rotation and the aspect ratio and isometric angle is taken into account
			// The unit is still world units however
			var boundsMatrix = Matrix4x4.TRS(center, Quaternion.Euler(rotation), new Vector3(aspectRatio, 1, 1)) * isometricMatrix;

			// Generate a matrix where Vector3.zero is the corner of the graph instead of the center
			// The unit is nodes here (so (0.5,0,0.5) is the position of the first node and (1.5,0,0.5) is the position of the second node)
			// 0.5 is added since this is the node center, not its corner. In graph space a node has a size of 1
			var m = Matrix4x4.TRS(boundsMatrix.MultiplyPoint3x4(-new Vector3(newWidth*newNodeSize, 0, newDepth*newNodeSize)*0.5F), Quaternion.Euler(rotation), new Vector3(newNodeSize*aspectRatio, 1, newNodeSize)) * isometricMatrix;

			// Set the matrix of the graph
			// This will also set inverseMatrix
			return new GraphTransform(m);
		}

		/// <summary>
		/// Calculates the width/depth of the graph from <see cref="unclampedSize"/> and <see cref="nodeSize"/>.
		/// The node size may be changed due to constraints that the width/depth is not
		/// allowed to be larger than 1024 (artificial limit).
		/// </summary>
		void CalculateDimensions (out int width, out int depth, out float nodeSize) {
			var newSize = unclampedSize;

			// Make sure size is positive
			newSize.x *= Mathf.Sign(newSize.x);
			newSize.y *= Mathf.Sign(newSize.y);

			// Clamp the nodeSize so that the graph is never larger than 1024*1024
			nodeSize = Mathf.Max(this.nodeSize, newSize.x/1024F);
			nodeSize = Mathf.Max(this.nodeSize, newSize.y/1024F);

			// Prevent the graph to become smaller than a single node
			newSize.x = newSize.x < nodeSize ? nodeSize : newSize.x;
			newSize.y = newSize.y < nodeSize ? nodeSize : newSize.y;

			size = newSize;

			// Calculate the number of nodes along each side
			width = Mathf.FloorToInt(size.x / nodeSize);
			depth = Mathf.FloorToInt(size.y / nodeSize);

			// Take care of numerical edge cases
			if (Mathf.Approximately(size.x / nodeSize, Mathf.CeilToInt(size.x / nodeSize))) {
				width = Mathf.CeilToInt(size.x / nodeSize);
			}

			if (Mathf.Approximately(size.y / nodeSize, Mathf.CeilToInt(size.y / nodeSize))) {
				depth = Mathf.CeilToInt(size.y / nodeSize);
			}
		}

		public override NNInfoInternal GetNearest (Vector3 position, NNConstraint constraint, GraphNode hint) {
			if (nodes == null || depth*width != nodes.Length) {
				return new NNInfoInternal();
			}

			// Calculate the closest node and the closest point on that node
			position = transform.InverseTransform(position);

			float xf = position.x;
			float zf = position.z;
			int x = Mathf.Clamp((int)xf, 0, width-1);
			int z = Mathf.Clamp((int)zf, 0, depth-1);

			var nn = new NNInfoInternal(nodes[z*width+x]);

			float y = transform.InverseTransform((Vector3)nodes[z*width+x].position).y;
			nn.clampedPosition = transform.Transform(new Vector3(Mathf.Clamp(xf, x, x+1f), y, Mathf.Clamp(zf, z, z+1f)));

			return nn;
		}

		public override NNInfoInternal GetNearestForce (Vector3 position, NNConstraint constraint) {
			if (nodes == null || depth*width != nodes.Length) {
				return new NNInfoInternal();
			}

			// Position in global space
			Vector3 globalPosition = position;

			// Position in graph space
			position = transform.InverseTransform(position);

			// Find the coordinates of the closest node
			float xf = position.x;
			float zf = position.z;
			int x = Mathf.Clamp((int)xf, 0, width-1);
			int z = Mathf.Clamp((int)zf, 0, depth-1);

			// Closest node
			var node = nodes[x+z*width];

			GraphNode minNode = null;
			float minDist = float.PositiveInfinity;
			int overlap = getNearestForceOverlap;

			Vector3 clampedPosition = Vector3.zero;
			var nn = new NNInfoInternal(null);

			// If the closest node was suitable
			if (constraint == null || constraint.Suitable(node)) {
				minNode = node;
				minDist = ((Vector3)minNode.position-globalPosition).sqrMagnitude;
				float y = transform.InverseTransform((Vector3)node.position).y;
				clampedPosition = transform.Transform(new Vector3(Mathf.Clamp(xf, x, x+1f), y, Mathf.Clamp(zf, z, z+1f)));
			}

			if (minNode != null) {
				nn.node = minNode;
				nn.clampedPosition = clampedPosition;

				// We have a node, and we don't need to search more, so just return
				if (overlap == 0) return nn;
				overlap--;
			}

			// Search up to this distance
			float maxDist = constraint == null || constraint.constrainDistance ? AstarPath.active.maxNearestNodeDistance : float.PositiveInfinity;
			float maxDistSqr = maxDist*maxDist;

			// Search a square/spiral pattern around the point
			for (int w = 1;; w++) {
				//Check if the nodes are within distance limit
				if (nodeSize*w > maxDist) {
					break;
				}

				bool anyInside = false;

				int nx;
				int nz = z+w;
				int nz2 = nz*width;

				// Side 1 on the square
				for (nx = x-w; nx <= x+w; nx++) {
					if (nx < 0 || nz < 0 || nx >= width || nz >= depth) continue;
					anyInside = true;
					if (constraint == null || constraint.Suitable(nodes[nx+nz2])) {
						float dist = ((Vector3)nodes[nx+nz2].position-globalPosition).sqrMagnitude;
						if (dist < minDist && dist < maxDistSqr) {
							// Minimum distance so far
							minDist = dist;
							minNode = nodes[nx+nz2];

							// Closest point on the node if the node is treated as a square
							clampedPosition = transform.Transform(new Vector3(Mathf.Clamp(xf, nx, nx+1f), transform.InverseTransform((Vector3)minNode.position).y, Mathf.Clamp(zf, nz, nz+1f)));
						}
					}
				}

				nz = z-w;
				nz2 = nz*width;

				// Side 2 on the square
				for (nx = x-w; nx <= x+w; nx++) {
					if (nx < 0 || nz < 0 || nx >= width || nz >= depth) continue;
					anyInside = true;
					if (constraint == null || constraint.Suitable(nodes[nx+nz2])) {
						float dist = ((Vector3)nodes[nx+nz2].position-globalPosition).sqrMagnitude;
						if (dist < minDist && dist < maxDistSqr) {
							minDist = dist;
							minNode = nodes[nx+nz2];
							clampedPosition = transform.Transform(new Vector3(Mathf.Clamp(xf, nx, nx+1f), transform.InverseTransform((Vector3)minNode.position).y, Mathf.Clamp(zf, nz, nz+1f)));
						}
					}
				}

				nx = x-w;

				// Side 3 on the square
				for (nz = z-w+1; nz <= z+w-1; nz++) {
					if (nx < 0 || nz < 0 || nx >= width || nz >= depth) continue;
					anyInside = true;
					if (constraint == null || constraint.Suitable(nodes[nx+nz*width])) {
						float dist = ((Vector3)nodes[nx+nz*width].position-globalPosition).sqrMagnitude;
						if (dist < minDist && dist < maxDistSqr) {
							minDist = dist;
							minNode = nodes[nx+nz*width];
							clampedPosition = transform.Transform(new Vector3(Mathf.Clamp(xf, nx, nx+1f), transform.InverseTransform((Vector3)minNode.position).y, Mathf.Clamp(zf, nz, nz+1f)));
						}
					}
				}

				nx = x+w;

				// Side 4 on the square
				for (nz = z-w+1; nz <= z+w-1; nz++) {
					if (nx < 0 || nz < 0 || nx >= width || nz >= depth) continue;
					anyInside = true;
					if (constraint == null || constraint.Suitable(nodes[nx+nz*width])) {
						float dist = ((Vector3)nodes[nx+nz*width].position-globalPosition).sqrMagnitude;
						if (dist < minDist && dist < maxDistSqr) {
							minDist = dist;
							minNode = nodes[nx+nz*width];
							clampedPosition = transform.Transform(new Vector3(Mathf.Clamp(xf, nx, nx+1f), transform.InverseTransform((Vector3)minNode.position).y, Mathf.Clamp(zf, nz, nz+1f)));
						}
					}
				}

				// We found a suitable node
				if (minNode != null) {
					// If we don't need to search more, just return
					// Otherwise search for 'overlap' iterations more
					if (overlap == 0) {
						break;
					}
					overlap--;
				}

				// No nodes were inside grid bounds
				// We will not be able to find any more valid nodes
				// so just return
				if (!anyInside) {
					break;
				}
			}

			// Copy fields to the NNInfo struct and return
			nn.node = minNode;
			nn.clampedPosition = clampedPosition;
			return nn;
		}

		/// <summary>
		/// Sets up <see cref="neighbourOffsets"/> with the current settings. <see cref="neighbourOffsets"/>, <see cref="neighbourCosts"/>, <see cref="neighbourXOffsets"/> and <see cref="neighbourZOffsets"/> are set up.\n
		/// The cost for a non-diagonal movement between two adjacent nodes is RoundToInt (<see cref="nodeSize"/> * Int3.Precision)\n
		/// The cost for a diagonal movement between two adjacent nodes is RoundToInt (<see cref="nodeSize"/> * Sqrt (2) * Int3.Precision)
		/// </summary>
		public virtual void SetUpOffsetsAndCosts () {
			// First 4 are for the four directly adjacent nodes the last 4 are for the diagonals
			neighbourOffsets[0] = -width;
			neighbourOffsets[1] = 1;
			neighbourOffsets[2] = width;
			neighbourOffsets[3] = -1;
			neighbourOffsets[4] = -width+1;
			neighbourOffsets[5] = width+1;
			neighbourOffsets[6] = width-1;
			neighbourOffsets[7] = -width-1;

			// The width of a single node, and thus also the distance between two adjacent nodes (axis aligned).
			// For hexagonal graphs the node size is different from the width of a hexaon.
			float nodeWidth = neighbours == NumNeighbours.Six ? ConvertNodeSizeToHexagonSize(InspectorGridHexagonNodeSize.Width, nodeSize) : nodeSize;

			uint straightCost = (uint)Mathf.RoundToInt(nodeWidth*Int3.Precision);

			// Diagonals normally cost sqrt(2) (approx 1.41) times more
			uint diagonalCost = uniformEdgeCosts ? straightCost : (uint)Mathf.RoundToInt(nodeWidth*Mathf.Sqrt(2F)*Int3.Precision);

			neighbourCosts[0] = straightCost;
			neighbourCosts[1] = straightCost;
			neighbourCosts[2] = straightCost;
			neighbourCosts[3] = straightCost;
			neighbourCosts[4] = diagonalCost;
			neighbourCosts[5] = diagonalCost;
			neighbourCosts[6] = diagonalCost;
			neighbourCosts[7] = diagonalCost;

			/*         Z
			 *         |
			 *         |
			 *
			 *      6  2  5
			 *       \ | /
			 * --  3 - X - 1  ----- X
			 *       / | \
			 *      7  0  4
			 *
			 *         |
			 *         |
			 */
		}

		IEnumerable<Progress> ScanInternalBurst (bool async) {
			if (nodeSize <= 0) {
				yield break;
			}

			// Make sure the matrix is up to date
			UpdateTransform();

			if (width > 1024 || depth > 1024) {
				Debug.LogError("One of the grid's sides is longer than 1024 nodes");
				yield break;
			}

#if !ASTAR_JPS
			if (this.useJumpPointSearch) {
				Debug.LogError("Trying to use Jump Point Search, but support for it is not enabled. Please enable it in the inspector (Grid Graph settings).");
			}
#endif

			SetUpOffsetsAndCosts();

			// Set a global reference to this graph so that nodes can find it
			GridNode.SetGridGraph((int)graphIndex, this);

			// Create and initialize the collision class
			if (collision == null) {
				collision = new GraphCollision();
			}
			collision.Initialize(transform, nodeSize);

			// Allocate buffers for jobs
			var dependencyTracker = ObjectPool<JobDependencyTracker>.Claim();

			// Create all nodes
			JobHandle allocateNodesJob;
			var newNodes = AllocateNodesJob(width * depth, out allocateNodesJob);
			var allocationMethod = async ? Allocator.Persistent : Allocator.TempJob;

			var bounds = new IntRect(0, 0, width - 1, depth - 1);

			var updateNodesJob = UpdateAreaBurst(newNodes, new int3(width, LayerCount, depth), bounds, dependencyTracker, allocateNodesJob, allocationMethod, RecalculationMode.RecalculateFromScratch);

			if (async) {
				foreach (var _ in updateNodesJob.CompleteTimeSliced(8.0f)) {
					System.Threading.Thread.Yield();
					yield return new Progress(0.5f, "Scanning grid graph");
				}
			} else {
				updateNodesJob.Complete();
			}

			ObjectPool<JobDependencyTracker>.Release(ref dependencyTracker);
		}

		public struct GridGraphScanData {
			public JobDependencyTracker dependencyTracker;
			public Allocator allocationMethod;
			public int numNodes;
			public IntBounds bounds;
			public int layers;
			public Vector3 up;
			public GraphTransform transform;
			public NativeArray<Vector3> nodePositions;
			public NativeArray<int> nodeConnections;
			public NativeArray<uint> nodePenalties;
			public NativeArray<int> nodeTags;
			public NativeArray<float4> nodeNormals;
			public NativeArray<bool> nodeWalkable;
			public NativeArray<bool> nodeWalkableWithErosion;
			public NativeArray<RaycastHit> heightHits;
			public bool layeredDataLayout;

			public void AllocateBuffers () {
				Profiler.BeginSample("Allocating buffers");
				// Allocate buffers for jobs
				// Allocating buffers with uninitialized memory is much faster if no jobs assume anything about their contents
				nodePositions = dependencyTracker.NewNativeArray<Vector3>(numNodes, allocationMethod, NativeArrayOptions.UninitializedMemory);
				nodeNormals = dependencyTracker.NewNativeArray<float4>(numNodes, allocationMethod, NativeArrayOptions.UninitializedMemory);
				nodeConnections = dependencyTracker.NewNativeArray<int>(numNodes, allocationMethod, NativeArrayOptions.UninitializedMemory);
				nodePenalties = dependencyTracker.NewNativeArray<uint>(numNodes, allocationMethod, NativeArrayOptions.UninitializedMemory);
				nodeWalkable = dependencyTracker.NewNativeArray<bool>(numNodes, allocationMethod, NativeArrayOptions.UninitializedMemory);
				nodeWalkableWithErosion = dependencyTracker.NewNativeArray<bool>(numNodes, allocationMethod, NativeArrayOptions.UninitializedMemory);
				nodeTags = dependencyTracker.NewNativeArray<int>(numNodes, allocationMethod, NativeArrayOptions.ClearMemory);
				Profiler.EndSample();
			}

			public void SetDefaultPenalties (uint initialPenalty) {
				nodePenalties.MemSet(initialPenalty).Schedule(dependencyTracker);
			}

			public void SetDefaultNodePositions (GraphTransform transform) {
				new JobNodePositions {
					graphToWorld = transform.matrix,
					bounds = bounds,
					nodePositions = nodePositions,
				}.Schedule(dependencyTracker);
			}

			public JobHandle HeightCheck (GraphCollision collision, int maxHits, NativeArray<int> outLayerCount, float characterHeight) {
				// For some reason the physics code crashes when allocating raycastCommands with UninitializedMemory, even though I have verified that every
				// element in the array is set to a well defined value before the physics code gets to it... Mysterious.
				var raycastCommands = dependencyTracker.NewNativeArray<RaycastCommand>(bounds.size.x*bounds.size.z, allocationMethod, NativeArrayOptions.ClearMemory);

				heightHits = dependencyTracker.NewNativeArray<RaycastHit>(numNodes * maxHits, allocationMethod, NativeArrayOptions.ClearMemory);

				// Due to floating point inaccuracies we don't want the rays to end *exactly* at the base of the graph
				// The rays may or may not hit colliders with the exact same y coordinate.
				// We extend the rays a bit to ensure they always hit
				const float RayLengthMargin = 0.01f;
				var prepareJob = new JobPrepareGridRaycast {
					graphToWorld = transform.matrix,
					bounds = bounds,
					raycastOffset = up * collision.fromHeight,
					raycastDirection = -up * (collision.fromHeight + RayLengthMargin),
					raycastMask = collision.heightMask,
					raycastCommands = raycastCommands,
				}.Schedule(dependencyTracker);

				if (maxHits > 1) {
					// Skip this distance between each hit.
					// It is pretty arbitrarily chosen, but it must be lower than characterHeight.
					// If it would be set too low then many thin colliders stacked on top of each other could lead to a very large number of hits
					// that will not lead to any walkable nodes anyway.
					float minStep = characterHeight * 0.5f;
					var dependency = new RaycastAllCommand(raycastCommands, heightHits, maxHits, dependencyTracker, minStep).Schedule(prepareJob);

					dependency = new JobMaxHitCount {
						hits = heightHits,
						maxHits = maxHits,
						layerStride = bounds.size.x*bounds.size.z,
						maxHitCount = outLayerCount,
					}.Schedule(dependency);

					return dependency;
				} else {
					dependencyTracker.ScheduleBatch(raycastCommands, heightHits, 2048);
					outLayerCount[0] = 1;
					return default;
				}
			}

			public void CopyHits () {
				// Copy the hit points and normals to separate arrays
				new JobCopyHits {
					hits = heightHits,
					points = nodePositions,
					normals = nodeNormals,
				}.Schedule(dependencyTracker);
			}

			public void CalculateWalkabilityFromHeightData (bool useRaycastNormal, bool unwalkableWhenNoGround, float maxSlope, float characterHeight) {
				new JobNodeWalkable {
					useRaycastNormal = useRaycastNormal,
					unwalkableWhenNoGround = unwalkableWhenNoGround,
					maxSlope = maxSlope,
					up = up,
					nodeNormals = nodeNormals,
					nodeWalkable = nodeWalkable,
					nodePositions = nodePositions.Reinterpret<float3>(),
					characterHeight = characterHeight,
					layerStride = bounds.size.x*bounds.size.z,
				}.Schedule(dependencyTracker);
			}

			public void CollisionCheck (GraphCollision collision) {
				if (collision.type == ColliderType.Ray && !collision.use2D) {
					var collisionCheckResult = dependencyTracker.NewNativeArray<bool>(numNodes, allocationMethod, NativeArrayOptions.UninitializedMemory);
					collision.JobCollisionRay(nodePositions, collisionCheckResult, up, allocationMethod, dependencyTracker);
					nodeWalkable.BitwiseAndWith(collisionCheckResult).Schedule(dependencyTracker);
				} else {
					// This part can unfortunately not be jobified yet
					new JobCheckCollisions {
						nodePositions = nodePositions,
						collisionResult = nodeWalkable,
						collision = collision,
					}.ScheduleManagedInMainThread(dependencyTracker);
				}
			}

			public void Connections (float maxStepHeight, bool maxStepUsesSlope, NumNeighbours neighbours, bool cutCorners, bool use2D, bool useErodedWalkability, float characterHeight) {
				new JobCalculateConnections {
					maxStepHeight = maxStepHeight,
					maxStepUsesSlope = maxStepUsesSlope,
					up = up,
					bounds = bounds,
					neighbours = neighbours,
					use2D = use2D,
					cutCorners = cutCorners,
					nodeWalkable = useErodedWalkability ? nodeWalkableWithErosion : nodeWalkable,
					nodePositions = nodePositions,
					nodeNormals = nodeNormals,
					nodeConnections = nodeConnections,
					characterHeight = characterHeight,
					layeredDataLayout = layeredDataLayout,
				}.ScheduleBatch(bounds.size.z, 20, dependencyTracker);

				new JobFilterDiagonalConnections {
					bounds = bounds,
					neighbours = neighbours,
					cutCorners = cutCorners,
					nodeConnections = nodeConnections,
				}.ScheduleBatch(bounds.size.z, 20, dependencyTracker);
			}

			public void Erosion (NumNeighbours neighbours, int erodeIterations, IntBounds erosionWriteMask) {
				if (!layeredDataLayout) {
					new JobErosion<FlatGridAdjacencyMapper> {
						bounds = bounds,
						writeMask = erosionWriteMask,
						neighbours = neighbours,
						nodeConnections = nodeConnections,
						erosion = erodeIterations,
						// TOOD: Tags
						nodeWalkable = nodeWalkable,
						outNodeWalkable = nodeWalkableWithErosion,
					}.Schedule(dependencyTracker);
				} else {
					new JobErosion<LayeredGridAdjacencyMapper> {
						bounds = bounds,
						writeMask = erosionWriteMask,
						neighbours = neighbours,
						nodeConnections = nodeConnections,
						erosion = erodeIterations,
						// TOOD: Tags
						nodeWalkable = nodeWalkable,
						outNodeWalkable = nodeWalkableWithErosion,
					}.Schedule(dependencyTracker);
				}
			}

			[BurstCompile]
			struct CopyBuffersJob : IJob {
				public struct Buffers {
					public NativeArray<Vector3> nodePositions;
					public NativeArray<int> nodeConnections;
					public NativeArray<uint> nodePenalties;
					public NativeArray<int> nodeTags;
					public NativeArray<float4> nodeNormals;
					public NativeArray<bool> nodeWalkable;
					public NativeArray<bool> nodeWalkableWithErosion;

					public Buffers(ref GridGraphScanData data) {
						nodePositions = data.nodePositions;
						nodeConnections = data.nodeConnections;
						nodePenalties = data.nodePenalties;
						nodeTags = data.nodeTags;
						nodeNormals = data.nodeNormals;
						nodeWalkable = data.nodeWalkable;
						nodeWalkableWithErosion = data.nodeWalkableWithErosion;
					}
				}

				[ReadOnly]
				[DisableUninitializedReadCheck]
				public Buffers input;

				[WriteOnly]
				public Buffers output;

				public int3 outputSize;
				public IntBounds outputBounds;

				public void Execute () {
					var inputSize = outputBounds.size;
					var inputBounds = new IntBounds(new int3(), outputBounds.size);

					// Note: Having a single job that copies all of the buffers avoids a lot of scheduling overhead.
					// We do miss out on parallelization, however for this job it is not that significant.
					JobCopyRectangle<Vector3>.Copy(input.nodePositions, output.nodePositions, inputSize, outputSize, inputBounds, outputBounds);
					JobCopyRectangle<float4>.Copy(input.nodeNormals, output.nodeNormals, inputSize, outputSize, inputBounds, outputBounds);
					JobCopyRectangle<int>.Copy(input.nodeConnections, output.nodeConnections, inputSize, outputSize, inputBounds, outputBounds);
					JobCopyRectangle<uint>.Copy(input.nodePenalties, output.nodePenalties, inputSize, outputSize, inputBounds, outputBounds);
					JobCopyRectangle<int>.Copy(input.nodeTags, output.nodeTags, inputSize, outputSize, inputBounds, outputBounds);
					JobCopyRectangle<bool>.Copy(input.nodeWalkable, output.nodeWalkable, inputSize, outputSize, inputBounds, outputBounds);
					JobCopyRectangle<bool>.Copy(input.nodeWalkableWithErosion, output.nodeWalkableWithErosion, inputSize, outputSize, inputBounds, outputBounds);
				}
			}

			public void ReadFromNodes (GridNodeBase[] nodes, int3 nodeArrayBounds, IntBounds readBounds, JobHandle nodesDependsOn, NativeArray<float4> graphNodeNormals) {
				bounds = readBounds;
				numNodes = readBounds.volume;
				AllocateBuffers();

				// This is a managed type, we need to trick Unity to allow this inside of a job
				var nodesHandle = System.Runtime.InteropServices.GCHandle.Alloc(nodes);

				var job = new JobReadNodeData {
					nodesHandle = nodesHandle,
					nodePositions = nodePositions,
					nodePenalties = nodePenalties,
					nodeTags = nodeTags,
					nodeConnections = nodeConnections,
					nodeWalkableWithErosion = nodeWalkableWithErosion,
					nodeWalkable = nodeWalkable,
					nodeArrayBounds = nodeArrayBounds,
					dataBounds = readBounds,
				}.ScheduleBatch(numNodes, math.max(2000, numNodes/16), dependencyTracker, nodesDependsOn);

				dependencyTracker.DeferFree(nodesHandle, job);

				UnityEngine.Assertions.Assert.IsTrue(graphNodeNormals.IsCreated);

				// Read the normal data from the graphNodeNormals array and copy it to the newData.nodeNormals array
				new JobCopyRectangle<float4> {
					input = graphNodeNormals,
					output = nodeNormals,
					inputSize = nodeArrayBounds,
					outputSize = bounds.size,
					inputBounds = readBounds,
					outputBounds = new IntBounds(new int3(), bounds.size),
				}.Schedule(dependencyTracker);
			}

			public GridGraphScanData ReadFromNodesAndCopy (GridNodeBase[] nodes, int3 nodeArrayBounds, IntBounds readBounds, JobHandle nodesDependsOn, NativeArray<float4> graphNodeNormals) {
				GridGraphScanData newData = this;

				newData.ReadFromNodes(nodes, nodeArrayBounds, readBounds, nodesDependsOn, graphNodeNormals);

				// Overwrite a rectangle in the center with the data from this object.
				// In the end we will have newly calculated data in the middle and data read from nodes along the borders
				var relativeCopyBounds = this.bounds.Offset(-readBounds.min);

				new CopyBuffersJob {
					input = new CopyBuffersJob.Buffers(ref this),
					output = new CopyBuffersJob.Buffers(ref newData),
					outputSize = readBounds.size,
					outputBounds = relativeCopyBounds,
				}.Schedule(dependencyTracker);

				return newData;
			}

			public JobHandle AssignToNodes (GridNodeBase[] nodes, int3 nodeArrayBounds, IntBounds writeMask, uint graphIndex, JobHandle nodesDependsOn, NativeArray<float4> nodeSurfaceNormals) {
				// This is a managed type, we need to trick Unity to allow this inside of a job
				var nodesHandle = System.Runtime.InteropServices.GCHandle.Alloc(nodes);

				// Dirty all the nodes. This cannot be done in parallel.
				var job1 = new JobDirtyNodes {
					nodesHandle = nodesHandle,
					nodeArrayBounds = nodeArrayBounds,
					dataBounds = bounds,
				}.ScheduleManaged(nodesDependsOn);

				// Assign the data to the nodes (in parallel for performance)
				// Note that it is essential that the nodes have earlier been dirtied, otherwise they might be dirtied in parallel which
				// will cause all kinds of thread safety issues.
				var job2 = new JobAssignNodeData {
					nodesHandle = nodesHandle,
					graphIndex = graphIndex,
					nodePositions = nodePositions,
					nodePenalties = nodePenalties,
					nodeTags = nodeTags,
					nodeConnections = nodeConnections,
					nodeWalkableWithErosion = nodeWalkableWithErosion,
					nodeWalkable = nodeWalkable,
					nodeArrayBounds = nodeArrayBounds,
					dataBounds = bounds,
					writeMask = writeMask,
				}.ScheduleBatch(numNodes, math.max(1000, numNodes/16), dependencyTracker, job1);

				dependencyTracker.DeferFree(nodesHandle, job2);

				var job3 = new JobCopyRectangle<float4> {
					input = nodeNormals,
					output = nodeSurfaceNormals,
					inputSize = bounds.size,
					outputSize = nodeArrayBounds,
					inputBounds = new IntBounds(new int3(), bounds.size),
					outputBounds = bounds,
				}.Schedule(dependencyTracker);

				return JobHandle.CombineDependencies(job2, job3);
			}
		}

		struct GridIndexMapper : GraphUpdateObject.INodeIndexMapper {
			public int3 dataSize;
			public IntBounds iterationBounds;
			public int Count => iterationBounds.size.x*iterationBounds.size.y*iterationBounds.size.z;

			public GridIndexMapper(IntBounds dataBounds, IntBounds iterationBounds) {
				UnityEngine.Assertions.Assert.IsTrue(dataBounds.Contains(iterationBounds));
				this.dataSize = dataBounds.size;
				this.iterationBounds = iterationBounds.Offset(-dataBounds.min);
			}

			public int this[int index] {
				get {
					int y = index/(iterationBounds.size.x*iterationBounds.size.z);
					index -= y*(iterationBounds.size.x*iterationBounds.size.z);
					int x = index % iterationBounds.size.x;
					var z = index/iterationBounds.size.x;
					return (y + iterationBounds.min.y)*(dataSize.x*dataSize.z) + (z + iterationBounds.min.z)*dataSize.x + iterationBounds.min.x + x;
				}
			}
		}

		public enum RecalculationMode {
			/// <summary>Recalculates the nodes from scratch. Used when the graph is first scanned</summary>
			RecalculateFromScratch,
			/// <summary>Recalculate the minimal number of nodes necessary to guarantee changes inside the graph update's bounding box are taken into account. Some data may be read from the existing nodes</summary>
			RecalculateMinimal,
			/// <summary>Nodes are not recalculated. Used for graph updates which only set node properties</summary>
			NoRecalculation,
		}

		JobHandleWithMainThreadWork UpdateAreaBurst (GridNodeBase[] newNodes, int3 nodeArrayBounds, IntRect bounds, JobDependencyTracker dependencyTracker, JobHandle nodesDependsOn, Allocator allocationMethod, RecalculationMode recalculationMode, GraphUpdateObject graphUpdateObject = null) {
			return new JobHandleWithMainThreadWork(UpdateAreaBurstCoroutine(newNodes, nodeArrayBounds, bounds, dependencyTracker, nodesDependsOn, allocationMethod, recalculationMode, graphUpdateObject), dependencyTracker);
		}

		IEnumerator<JobHandle> UpdateAreaBurstCoroutine (GridNodeBase[] newNodes, int3 nodeArrayBounds, IntRect rect, JobDependencyTracker dependencyTracker, JobHandle nodesDependsOn, Allocator allocationMethod, RecalculationMode recalculationMode, GraphUpdateObject graphUpdateObject) {
			var originalRect = rect;

			var fullRecalculationRect = rect;

			if (collision.collisionCheck && collision.type != ColliderType.Ray) fullRecalculationRect = fullRecalculationRect.Expand(Mathf.CeilToInt(collision.diameter * 0.5f));

			// Due to erosion a bit more of the graph may be affected by the updates in the fullRecalculationBounds
			var writeMaskRect = fullRecalculationRect.Expand(erodeIterations + 1);

			// Due to how erosion works we need to recalculate erosion in an even larger region to make sure we
			// get the correct result inside the writeMask
			var readRect = writeMaskRect.Expand(erodeIterations + 1);

			if (recalculationMode == RecalculationMode.RecalculateFromScratch) {
				// If we are not allowed to read from the graph we need to recalculate everything that we would otherwise just have read from the graph
				fullRecalculationRect = readRect;
			}

			// Clamp to the grid dimensions
			var gridRect = new IntRect(0, 0, width - 1, depth - 1);
			readRect = IntRect.Intersection(readRect, gridRect);
			fullRecalculationRect = IntRect.Intersection(fullRecalculationRect, gridRect);
			writeMaskRect = IntRect.Intersection(writeMaskRect, gridRect);

			// Check if there is anything to do. The bounds may not even overlap the graph
			if (!writeMaskRect.IsValid()) yield break;

			if (recalculationMode == RecalculationMode.RecalculateMinimal && readRect == fullRecalculationRect) {
				// There is no point reading from the graph since we are recalculating all those nodes anyway.
				// This happens if an update is done to the whole graph.
				// Skipping the read can improve performance quite a lot for that kind of updates.
				// This is purely an optimization and should not change the result.
				recalculationMode = RecalculationMode.RecalculateFromScratch;
			}

#if ASTAR_DEBUG
			var debugMatrix = transform * Matrix4x4.TRS(new Vector3(0.5f, 0, 0.5f), Quaternion.identity, Vector3.one);
			fullRecalculationBounds.DebugDraw(debugMatrix, Color.yellow);
			writeMask.DebugDraw(debugMatrix * Matrix4x4.Translate(Vector3.up*0.1f), Color.magenta);
			bounds.DebugDraw(debugMatrix * Matrix4x4.Translate(Vector3.up*0.2f), Color.blue);
#endif

			var layers = nodeArrayBounds.y;

			// If recalculating a very small number of nodes then disable dependency tracking and just run jobs one after the other.
			// This is faster since dependency tracking has some overhead
			dependencyTracker.SetLinearDependencies(writeMaskRect.Width*writeMaskRect.Height*layers < 500);

			var readBounds = new IntBounds(readRect.xmin, 0, readRect.ymin, readRect.xmax + 1, layers, readRect.ymax + 1);
			var fullRecalculationBounds = new IntBounds(fullRecalculationRect.xmin, 0, fullRecalculationRect.ymin, fullRecalculationRect.xmax + 1, layers, fullRecalculationRect.ymax + 1);

			var context = new GridGraphRules.Context {
				graph = this,
				data = new GridGraphScanData {
					dependencyTracker = dependencyTracker,
					allocationMethod = allocationMethod,
					bounds = fullRecalculationBounds,
					layers = layers,
					numNodes = fullRecalculationBounds.volume,
					transform = transform,
					up = (transform.Transform(Vector3.up) - transform.Transform(Vector3.zero)).normalized,
					layeredDataLayout = (this is LayerGridGraph),
				},
				tracker = dependencyTracker,
			};

			float characterHeight = (this is LayerGridGraph lg ? lg.characterHeight : float.PositiveInfinity);

			if (recalculationMode == RecalculationMode.RecalculateFromScratch || recalculationMode == RecalculationMode.RecalculateMinimal) {
				var heightCheck = collision.heightCheck && !collision.use2D;
				if (heightCheck) {
					var layerCount = dependencyTracker.NewNativeArray<int>(1, allocationMethod, NativeArrayOptions.UninitializedMemory);
					yield return context.data.HeightCheck(collision, MaxLayers, layerCount, characterHeight);
					// Never reduce the layer count of the graph.
					// For (not layered) grid graphs this is always 1.
					// Unless we are recalculating the whole graph: in that case we don't care about the existing layers.
					context.data.layers = recalculationMode == RecalculationMode.RecalculateFromScratch ? layerCount[0] : Mathf.Max(LayerCount, layerCount[0]);
					context.data.bounds.max.y = context.data.layers;
					context.data.numNodes = context.data.bounds.volume;

					// The size of the buffers depend on the height check for layered grid graphs since the number of layers might change
					context.data.AllocateBuffers();
					// Set the positions to be used if the height check ray didn't hit anything
					context.data.SetDefaultNodePositions(transform);
					context.data.CopyHits();
					context.data.CalculateWalkabilityFromHeightData(useRaycastNormal, collision.unwalkableWhenNoGround, maxSlope, characterHeight);
				} else {
					context.data.AllocateBuffers();
					context.data.SetDefaultNodePositions(transform);
					// Mark all nodes as walkable to begin with
					context.data.nodeWalkable.MemSet(true).Schedule(dependencyTracker);
					// Set the normals to point straight up
					context.data.nodeNormals.MemSet(new float4(context.data.up.x, context.data.up.y, context.data.up.z, 0)).Schedule(dependencyTracker);
				}

				context.data.SetDefaultPenalties(initialPenalty);

				// Kick off jobs early while we prepare the rest of them
				JobHandle.ScheduleBatchedJobs();

				rules.RebuildIfNecessary();

				rules.ExecuteRule(GridGraphRule.Pass.BeforeCollision, context);

				if (collision.collisionCheck) context.data.CollisionCheck(collision);

				rules.ExecuteRule(GridGraphRule.Pass.BeforeConnections, context);

				if (recalculationMode == RecalculationMode.RecalculateMinimal) {
					context.data = context.data.ReadFromNodesAndCopy(newNodes, nodeArrayBounds, readBounds, nodesDependsOn, nodeSurfaceNormals);
				}
			} else {
				// If we are not allowed to recalculate the graph then we read all the necessary info from the existing nodes
				context.data.ReadFromNodes(newNodes, nodeArrayBounds, readBounds, nodesDependsOn, nodeSurfaceNormals);
			}

			if (graphUpdateObject != null) {
				// Mark nodes that might be changed
				for (int z = writeMaskRect.ymin; z <= writeMaskRect.ymax; z++) {
					for (int x = writeMaskRect.xmin; x <= writeMaskRect.xmax; x++) {
						graphUpdateObject.WillUpdateNode(nodes[z*width+x]);
					}
				}

				var updateRect = IntRect.Intersection(originalRect, gridRect);
				var updateBounds = new IntBounds(updateRect.xmin, 0, updateRect.ymin, updateRect.xmax + 1, context.data.layers, updateRect.ymax + 1);
				graphUpdateObject.ApplyJob(new GridIndexMapper(context.data.bounds, updateBounds), new GraphUpdateObject.GraphUpdateData {
					nodePositions = context.data.nodePositions,
					nodePenalties = context.data.nodePenalties,
					nodeWalkable = context.data.nodeWalkable,
					nodeTags = context.data.nodeTags,
				}, dependencyTracker);
			}

			// Calculate the connections between nodes and also erode the graph
			context.data.Connections(maxStepHeight, maxStepUsesSlope, neighbours, cutCorners, collision.use2D, false, characterHeight);
			rules.ExecuteRule(GridGraphRule.Pass.AfterConnections, context);

			var writeMaskBounds = new IntBounds(writeMaskRect.xmin, 0, writeMaskRect.ymin, writeMaskRect.xmax + 1, context.data.layers, writeMaskRect.ymax + 1);

			if (erodeIterations > 0) {
				context.data.Erosion(neighbours, erodeIterations, writeMaskBounds);

				// After erosion is done we need to recalculate the node connections
				context.data.Connections(maxStepHeight, maxStepUsesSlope, neighbours, cutCorners, collision.use2D, true, characterHeight);
				rules.ExecuteRule(GridGraphRule.Pass.AfterConnections, context);
			} else {
				// If erosion is disabled we can just copy nodeWalkable to nodeWalkableWithErosion
				// TODO: Can we just do an assignment of the whole array?
				context.data.nodeWalkable.CopyToJob(context.data.nodeWalkableWithErosion).Schedule(dependencyTracker);
			}

			rules.ExecuteRule(GridGraphRule.Pass.PostProcess, context);

			nodeArrayBounds.y = context.data.layers;
			Memory.Resize(ref nodeSurfaceNormals, nodeArrayBounds.x*nodeArrayBounds.y*nodeArrayBounds.z, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			Memory.Realloc(ref newNodes, nodeArrayBounds.x*nodeArrayBounds.y*nodeArrayBounds.z);
			var allocJob = new JobAllocateNodes {
				active = active,
				nodeNormals = context.data.nodeNormals,
				bounds = context.data.bounds,
				nodeArrayBounds = nodeArrayBounds,
				nodes = newNodes,
				newGridNodeDelegate = newGridNodeDelegate,
			}.ScheduleManagedInMainThread(dependencyTracker);
			nodesDependsOn = JobHandle.CombineDependencies(nodesDependsOn, allocJob);

			yield return context.data.AssignToNodes(newNodes, nodeArrayBounds, writeMaskBounds, graphIndex, nodesDependsOn, nodeSurfaceNormals);

			// Assign the new nodes as an atomic operation
			nodes = newNodes;
			// TODO: Remove casting
			if (this is LayerGridGraph layeredGridGraph) layeredGridGraph.layerCount = context.data.layers;
		}

		public static bool USE_BURST = true;
		public static int BATCH_SIZE = 512;

		protected override IEnumerable<Progress> ScanInternal (bool async) {
			if (USE_BURST) {
				foreach (var p in ScanInternalBurst(async)) {
					yield return p;
				}
				yield break;
			}


			if (nodeSize <= 0) {
				yield break;
			}

			// Make sure the matrix is up to date
			UpdateTransform();

			if (width > 1024 || depth > 1024) {
				Debug.LogError("One of the grid's sides is longer than 1024 nodes");
				yield break;
			}

#if !ASTAR_JPS
			if (this.useJumpPointSearch) {
				Debug.LogError("Trying to use Jump Point Search, but support for it is not enabled. Please enable it in the inspector (Grid Graph settings).");
			}
#endif

			SetUpOffsetsAndCosts();

			// Set a global reference to this graph so that nodes can find it
			GridNode.SetGridGraph((int)graphIndex, this);

			yield return new Progress(0.05f, "Creating nodes");

			// Create all nodes
			nodes = new GridNode[width*depth];
			for (int z = 0; z < depth; z++) {
				for (int x = 0; x < width; x++) {
					var index = z*width+x;
					var node = nodes[index] = new GridNode(active);
					node.GraphIndex = graphIndex;
					node.NodeInGridIndex = index;
				}
			}

			// Create and initialize the collision class
			if (collision == null) {
				collision = new GraphCollision();
			}
			collision.Initialize(transform, nodeSize);

			int progressCounter = 0;

			const int YieldEveryNNodes = 1000;

			for (int z = 0; z < depth; z++) {
				// Yield with a progress value at most every N nodes
				if (progressCounter >= YieldEveryNNodes) {
					progressCounter = 0;
					yield return new Progress(Mathf.Lerp(0.1f, 0.7f, z/(float)depth), "Calculating positions");
				}

				progressCounter += width;

				for (int x = 0; x < width; x++) {
					// Updates the position of the node
					// and a bunch of other things
					RecalculateCell(x, z);
				}
			}

			progressCounter = 0;

			for (int z = 0; z < depth; z++) {
				// Yield with a progress value at most every N nodes
				if (progressCounter >= YieldEveryNNodes) {
					progressCounter = 0;
					yield return new Progress(Mathf.Lerp(0.7f, 0.9f, z/(float)depth), "Calculating connections");
				}

				progressCounter += width;

				for (int x = 0; x < width; x++) {
					// Recalculate connections to other nodes
					CalculateConnections(x, z);
				}
			}

			yield return new Progress(0.95f, "Calculating erosion");

			// Apply erosion
			ErodeWalkableArea();
		}

		/// <summary>
		/// Updates position, walkability and penalty for the node.
		/// Assumes that collision.Initialize (...) has been called before this function
		///
		/// Deprecated: Use RecalculateCell instead which works both for grid graphs and layered grid graphs.
		/// </summary>
		[System.Obsolete("Use RecalculateCell instead which works both for grid graphs and layered grid graphs")]
		public virtual void UpdateNodePositionCollision (GridNode node, int x, int z, bool resetPenalty = true) {
			RecalculateCell(x, z, resetPenalty, false);
		}

		/// <summary>
		/// Recalculates single node in the graph.
		///
		/// For a layered grid graph this will recalculate all nodes at a specific (x,z) cell in the grid.
		/// For grid graphs this will simply recalculate the single node at those coordinates.
		///
		/// Note: This must only be called when it is safe to update nodes.
		///  For example when scanning the graph or during a graph update.
		///
		/// Note: This will not recalculate any connections as this method is often run for several adjacent nodes at a time.
		///  After you have recalculated all the nodes you will have to recalculate the connections for the changed nodes
		///  as well as their neighbours.
		///  See: CalculateConnections
		/// </summary>
		/// <param name="x">X coordinate of the cell</param>
		/// <param name="z">Z coordinate of the cell</param>
		/// <param name="resetPenalties">If true, the penalty of the nodes will be reset to the initial value as if the graph had just been scanned
		///      (this excludes texture data however which is only done when scanning the graph).</param>
		/// <param name="resetTags">If true, the tag will be reset to zero (the default tag).</param>
		public virtual void RecalculateCell (int x, int z, bool resetPenalties = true, bool resetTags = true) {
			var node = nodes[z*width + x];

			// Set the node's initial position with a y-offset of zero
			node.position = GraphPointToWorld(x, z, 0);

			RaycastHit hit;

			bool walkable;

			// Calculate the actual position using physics raycasting (if enabled)
			// walkable will be set to false if no ground was found (unless that setting has been disabled)
			Vector3 position = collision.CheckHeight((Vector3)node.position, out hit, out walkable);
			node.position = (Int3)position;

			if (resetPenalties) {
				node.Penalty = initialPenalty;
			}

			if (resetTags) {
				node.Tag = 0;
			}

			// Check if the node is on a slope steeper than permitted
			if (walkable && useRaycastNormal && collision.heightCheck) {
				if (hit.normal != Vector3.zero) {
					// Take the dot product to find out the cosinus of the angle it has (faster than Vector3.Angle)
					float angle = Vector3.Dot(hit.normal.normalized, collision.up);

					// Cosinus of the max slope
					float cosAngle = Mathf.Cos(maxSlope*Mathf.Deg2Rad);

					// Check if the ground is flat enough to stand on
					if (angle < cosAngle) {
						walkable = false;
					}
				}
			}

			// If the walkable flag has already been set to false, there is no point in checking for it again
			// Check for obstacles
			node.Walkable = walkable && collision.Check((Vector3)node.position);

			// Store walkability before erosion is applied. Used for graph updates
			node.WalkableErosion = node.Walkable;
		}

		/// <summary>
		/// True if the node has any blocked connections.
		/// For 4 and 8 neighbours the 4 axis aligned connections will be checked.
		/// For 6 neighbours all 6 neighbours will be checked.
		///
		/// Internal method used for erosion.
		/// </summary>
		protected virtual bool ErosionAnyFalseConnections (GraphNode baseNode) {
			var node = baseNode as GridNode;

			if (neighbours == NumNeighbours.Six) {
				// Check the 6 hexagonal connections
				for (int i = 0; i < 6; i++) {
					if (!node.HasConnectionInDirection(hexagonNeighbourIndices[i])) {
						return true;
					}
				}
			} else {
				// Check the four axis aligned connections
				for (int i = 0; i < 4; i++) {
					if (!node.HasConnectionInDirection(i)) {
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>Internal method used for erosion</summary>
		void ErodeNode (GraphNode node) {
			if (node.Walkable && ErosionAnyFalseConnections(node)) {
				node.Walkable = false;
			}
		}

		/// <summary>Internal method used for erosion</summary>
		void ErodeNodeWithTagsInit (GraphNode node) {
			if (node.Walkable && ErosionAnyFalseConnections(node)) {
				node.Tag = (uint)erosionFirstTag;
			} else {
				node.Tag = 0;
			}
		}

		/// <summary>Internal method used for erosion</summary>
		void ErodeNodeWithTags (GraphNode node, int iteration) {
			var gnode = node as GridNodeBase;

			if (gnode.Walkable && gnode.Tag >= erosionFirstTag && gnode.Tag < erosionFirstTag + iteration) {
				if (neighbours == NumNeighbours.Six) {
					// Check the 6 hexagonal connections
					for (int i = 0; i < 6; i++) {
						var other = gnode.GetNeighbourAlongDirection(hexagonNeighbourIndices[i]);
						if (other != null) {
							uint tag = other.Tag;
							if (tag > erosionFirstTag + iteration || tag < erosionFirstTag) {
								other.Tag = (uint)(erosionFirstTag+iteration);
							}
						}
					}
				} else {
					// Check the four axis aligned connections
					for (int i = 0; i < 4; i++) {
						var other = gnode.GetNeighbourAlongDirection(i);
						if (other != null) {
							uint tag = other.Tag;
							if (tag > erosionFirstTag + iteration || tag < erosionFirstTag) {
								other.Tag = (uint)(erosionFirstTag+iteration);
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Erodes the walkable area.
		/// See: <see cref="erodeIterations"/>
		/// </summary>
		public virtual void ErodeWalkableArea () {
			ErodeWalkableArea(0, 0, Width, Depth);
		}

		/// <summary>
		/// Erodes the walkable area.
		///
		/// xmin, zmin (inclusive)\n
		/// xmax, zmax (exclusive)
		///
		/// See: <see cref="erodeIterations"/>
		/// </summary>
		public void ErodeWalkableArea (int xmin, int zmin, int xmax, int zmax) {
			if (erosionUseTags) {
				if (erodeIterations+erosionFirstTag > 31) {
					Debug.LogError("Too few tags available for "+erodeIterations+" erode iterations and starting with tag " + erosionFirstTag + " (erodeIterations+erosionFirstTag > 31)", active);
					return;
				}
				if (erosionFirstTag <= 0) {
					Debug.LogError("First erosion tag must be greater or equal to 1", active);
					return;
				}
			}

			if (erodeIterations == 0) return;

			// Get all nodes inside the rectangle
			var rect = new IntRect(xmin, zmin, xmax - 1, zmax - 1);
			var nodesInRect = GetNodesInRegion(rect);
			int nodeCount = nodesInRect.Count;
			for (int it = 0; it < erodeIterations; it++) {
				if (erosionUseTags) {
					if (it == 0) {
						for (int i = 0; i < nodeCount; i++) {
							ErodeNodeWithTagsInit(nodesInRect[i]);
						}
					} else {
						for (int i = 0; i < nodeCount; i++) {
							ErodeNodeWithTags(nodesInRect[i], it);
						}
					}
				} else {
					// Loop through all nodes and mark as unwalkble the nodes which
					// have at least one blocked connection to another node
					for (int i = 0; i < nodeCount; i++) {
						ErodeNode(nodesInRect[i]);
					}

					for (int i = 0; i < nodeCount; i++) {
						CalculateConnections(nodesInRect[i] as GridNodeBase);
					}
				}
			}

			// Return the list to the pool
			Pathfinding.Util.ListPool<GraphNode>.Release(ref nodesInRect);
		}

		/// <summary>
		/// Returns true if a connection between the adjacent nodes n1 and n2 is valid.
		/// Also takes into account if the nodes are walkable.
		///
		/// This method may be overriden if you want to customize what connections are valid.
		/// It must however hold that IsValidConnection(a,b) == IsValidConnection(b,a).
		///
		/// This is used for calculating the connections when the graph is scanned or updated.
		///
		/// See: CalculateConnections
		/// </summary>
		public virtual bool IsValidConnection (GridNodeBase node1, GridNodeBase node2) {
			if (!node1.Walkable || !node2.Walkable) {
				return false;
			}

			if (maxStepHeight <= 0 || collision.use2D) return true;

			if (transform.onlyTranslational) {
				// Common case optimization.
				// If the transformation is only translational, that is if the graph is not rotated or transformed
				// in any other way than changing its center. Then we can use this simplified code.
				// This code is hot when scanning so it does have an impact.
				return System.Math.Abs(node1.position.y - node2.position.y) <= maxStepHeight*Int3.Precision;
			} else {
				var p1 = (Vector3)node1.position;
				var p2 = (Vector3)node2.position;
				var up = transform.WorldUpAtGraphPosition(p1);
				return System.Math.Abs(Vector3.Dot(up, p1) - Vector3.Dot(up, p2)) <= maxStepHeight;
			}
		}

		/// <summary>
		/// Calculates the grid connections for a cell as well as its neighbours.
		/// This is a useful utility function if you want to modify the walkability of a single node in the graph.
		///
		/// <code>
		/// AstarPath.active.AddWorkItem(ctx => {
		///     var grid = AstarPath.active.data.gridGraph;
		///     int x = 5;
		///     int z = 7;
		///
		///     // Mark a single node as unwalkable
		///     grid.GetNode(x, z).Walkable = false;
		///
		///     // Recalculate the connections for that node as well as its neighbours
		///     grid.CalculateConnectionsForCellAndNeighbours(x, z);
		/// });
		/// </code>
		/// </summary>
		public void CalculateConnectionsForCellAndNeighbours (int x, int z) {
			CalculateConnections(x, z);
			for (int i = 0; i < 8; i++) {
				int nx = x + neighbourXOffsets[i];
				int nz = z + neighbourZOffsets[i];

				// Check if the new position is inside the grid
				// Bitwise AND (&) is measurably faster than &&
				// (not much, but this code is hot)
				if (nx >= 0 & nz >= 0 & nx < width & nz < depth) {
					CalculateConnections(nx, nz);
				}
			}
		}

		/// <summary>
		/// Calculates the grid connections for a single node.
		/// Deprecated: Use the instance function instead
		/// </summary>
		[System.Obsolete("Use the instance function instead")]
		public static void CalculateConnections (GridNode node) {
			(AstarData.GetGraph(node) as GridGraph).CalculateConnections((GridNodeBase)node);
		}

		/// <summary>
		/// Calculates the grid connections for a single node.
		/// Convenience function, it's slightly faster to use CalculateConnections(int,int)
		/// but that will only show when calculating for a large number of nodes.
		/// This function will also work for both grid graphs and layered grid graphs.
		/// </summary>
		public virtual void CalculateConnections (GridNodeBase node) {
			int index = node.NodeInGridIndex;
			int x = index % width;
			int z = index / width;

			CalculateConnections(x, z);
		}

		/// <summary>
		/// Calculates the grid connections for a single node.
		/// Deprecated: Use CalculateConnections(x,z) or CalculateConnections(node) instead
		/// </summary>
		[System.Obsolete("Use CalculateConnections(x,z) or CalculateConnections(node) instead")]
		public virtual void CalculateConnections (int x, int z, GridNode node) {
			CalculateConnections(x, z);
		}

		/// <summary>
		/// Calculates the grid connections for a single node.
		/// Note that to ensure that connections are completely up to date after updating a node you
		/// have to calculate the connections for both the changed node and its neighbours.
		///
		/// In a layered grid graph, this will recalculate the connections for all nodes
		/// in the (x,z) cell (it may have multiple layers of nodes).
		///
		/// See: CalculateConnections(GridNodeBase)
		/// </summary>
		public virtual void CalculateConnections (int x, int z) {
			var node = nodes[z*width + x] as GridNode;

			// All connections are disabled if the node is not walkable
			if (!node.Walkable) {
				// Reset all connections
				// This makes the node have NO connections to any neighbour nodes
				node.ResetConnectionsInternal();
				return;
			}

			// Internal index of where in the graph the node is
			int index = node.NodeInGridIndex;

			if (neighbours == NumNeighbours.Four || neighbours == NumNeighbours.Eight) {
				// Bitpacked connections
				// bit 0 is set if connection 0 is enabled
				// bit 1 is set if connection 1 is enabled etc.
				int conns = 0;

				// Loop through axis aligned neighbours (down, right, up, left) or (-Z, +X, +Z, -X)
				for (int i = 0; i < 4; i++) {
					int nx = x + neighbourXOffsets[i];
					int nz = z + neighbourZOffsets[i];

					// Check if the new position is inside the grid
					// Bitwise AND (&) is measurably faster than &&
					// (not much, but this code is hot)
					if (nx >= 0 & nz >= 0 & nx < width & nz < depth) {
						var other = nodes[index+neighbourOffsets[i]];

						if (IsValidConnection(node, other)) {
							// Enable connection i
							conns |= 1 << i;
						}
					}
				}

				// Bitpacked diagonal connections
				int diagConns = 0;

				// Add in the diagonal connections
				if (neighbours == NumNeighbours.Eight) {
					if (cutCorners) {
						for (int i = 0; i < 4; i++) {
							// If at least one axis aligned connection
							// is adjacent to this diagonal, then we can add a connection.
							// Bitshifting is a lot faster than calling node.HasConnectionInDirection.
							// We need to check if connection i and i+1 are enabled
							// but i+1 may overflow 4 and in that case need to be wrapped around
							// (so 3+1 = 4 goes to 0). We do that by checking both connection i+1
							// and i+1-4 at the same time. Either i+1 or i+1-4 will be in the range
							// from 0 to 4 (exclusive)
							if (((conns >> i | conns >> (i+1) | conns >> (i+1-4)) & 1) != 0) {
								int directionIndex = i+4;

								int nx = x + neighbourXOffsets[directionIndex];
								int nz = z + neighbourZOffsets[directionIndex];

								if (nx >= 0 & nz >= 0 & nx < width & nz < depth) {
									var other = nodes[index+neighbourOffsets[directionIndex]];

									if (IsValidConnection(node, other)) {
										diagConns |= 1 << directionIndex;
									}
								}
							}
						}
					} else {
						for (int i = 0; i < 4; i++) {
							// If exactly 2 axis aligned connections is adjacent to this connection
							// then we can add the connection
							// We don't need to check if it is out of bounds because if both of
							// the other neighbours are inside the bounds this one must be too
							if ((conns >> i & 1) != 0 && ((conns >> (i+1) | conns >> (i+1-4)) & 1) != 0) {
								var other = nodes[index+neighbourOffsets[i+4]];

								if (IsValidConnection(node, other)) {
									diagConns |= 1 << (i+4);
								}
							}
						}
					}
				}

				// Set all connections at the same time
				node.SetAllConnectionInternal(conns | diagConns);
			} else {
				// Hexagon layout

				// Reset all connections
				// This makes the node have NO connections to any neighbour nodes
				node.ResetConnectionsInternal();

				// Loop through all possible neighbours and try to connect to them
				for (int j = 0; j < hexagonNeighbourIndices.Length; j++) {
					var i = hexagonNeighbourIndices[j];

					int nx = x + neighbourXOffsets[i];
					int nz = z + neighbourZOffsets[i];

					if (nx >= 0 & nz >= 0 & nx < width & nz < depth) {
						var other = nodes[index+neighbourOffsets[i]];
						node.SetConnectionInternal(i, IsValidConnection(node, other));
					}
				}
			}
		}


		public override void OnDrawGizmos (DrawingData gizmos, bool drawNodes, RedrawScope redrawScope) {
			using (var helper = GraphGizmoHelper.GetSingleFrameGizmoHelper(gizmos, active, redrawScope)) {
				// The width and depth fields might not be up to date, so recalculate
				// them from the #unclampedSize field
				int w, d;
				float s;
				CalculateDimensions(out w, out d, out s);
				var bounds = new Bounds();
				bounds.SetMinMax(Vector3.zero, new Vector3(w, 0, d));
				using (helper.builder.WithMatrix(CalculateTransform().matrix)) {
					helper.builder.WireBox(bounds, Color.white);

					int nodeCount = nodes != null ? nodes.Length : -1;

					if (drawNodes && width*depth*LayerCount != nodeCount) {
						var color = new Color(1, 1, 1, 0.2f);
						helper.builder.WireGrid(new float3(w*0.5f, 0, d*0.5f), Quaternion.identity, new int2(w, d), new float2(w, d), color);
					}
				}
			}

			if (!drawNodes) {
				return;
			}

			// Loop through chunks of size chunkWidth*chunkWidth and create a gizmo mesh for each of those chunks.
			// This is done because rebuilding the gizmo mesh (such as when using Unity Gizmos) every frame is pretty slow
			// for large graphs. However just checking if any mesh needs to be updated is relatively fast. So we just store
			// a hash together with the mesh and rebuild the mesh when necessary.
			const int chunkWidth = 32;
			GridNodeBase[] allNodes = ArrayPool<GridNodeBase>.Claim(chunkWidth*chunkWidth*LayerCount);
			for (int cx = width/chunkWidth; cx >= 0; cx--) {
				for (int cz = depth/chunkWidth; cz >= 0; cz--) {
					Profiler.BeginSample("Hash");
					var allNodesCount = GetNodesInRegion(new IntRect(cx*chunkWidth, cz*chunkWidth, (cx+1)*chunkWidth - 1, (cz+1)*chunkWidth - 1), allNodes);
					var hasher = new NodeHasher(active);
					hasher.Add(showMeshOutline);
					hasher.Add(showMeshSurface);
					hasher.Add(showNodeConnections);
					for (int i = 0; i < allNodesCount; i++) {
						hasher.HashNode(allNodes[i]);
					}
					Profiler.EndSample();

					if (!gizmos.Draw(hasher, redrawScope)) {
						Profiler.BeginSample("Rebuild Retained Gizmo Chunk");
						using (var helper = GraphGizmoHelper.GetGizmoHelper(gizmos, active, hasher, redrawScope)) {
							if (showNodeConnections) {
								for (int i = 0; i < allNodesCount; i++) {
									// Don't bother drawing unwalkable nodes
									if (allNodes[i].Walkable) {
										helper.DrawConnections(allNodes[i]);
									}
								}
							}
							if (showMeshSurface || showMeshOutline) CreateNavmeshSurfaceVisualization(allNodes, allNodesCount, helper);
						}
						Profiler.EndSample();
					}
				}
			}
			ArrayPool<GridNodeBase>.Release(ref allNodes);

			if (active.showUnwalkableNodes) DrawUnwalkableNodes(gizmos, nodeSize * 0.3f, redrawScope);
		}

		/// <summary>
		/// Draw the surface as well as an outline of the grid graph.
		/// The nodes will be drawn as squares (or hexagons when using <see cref="neighbours"/> = Six).
		/// </summary>
		void CreateNavmeshSurfaceVisualization (GridNodeBase[] nodes, int nodeCount, GraphGizmoHelper helper) {
			// Count the number of nodes that we will render
			int walkable = 0;

			for (int i = 0; i < nodeCount; i++) {
				if (nodes[i].Walkable) walkable++;
			}

			var neighbourIndices = neighbours == NumNeighbours.Six ? hexagonNeighbourIndices : new [] { 0, 1, 2, 3 };
			var offsetMultiplier = neighbours == NumNeighbours.Six ? 0.333333f : 0.5f;

			// 2 for a square-ish grid, 4 for a hexagonal grid.
			var trianglesPerNode = neighbourIndices.Length-2;
			var verticesPerNode = 3*trianglesPerNode;

			// Get arrays that have room for all vertices/colors (the array might be larger)
			var vertices = ArrayPool<Vector3>.Claim(walkable*verticesPerNode);
			var colors = ArrayPool<Color>.Claim(walkable*verticesPerNode);
			int baseIndex = 0;

			for (int i = 0; i < nodeCount; i++) {
				var node = nodes[i];
				if (!node.Walkable) continue;

				var nodeColor = helper.NodeColor(node);
				// Don't bother drawing transparent nodes
				if (nodeColor.a <= 0.001f) continue;

				for (int dIndex = 0; dIndex < neighbourIndices.Length; dIndex++) {
					// For neighbours != Six
					// n2 -- n3
					// |     |
					// n  -- n1
					//
					// n = this node
					var d = neighbourIndices[dIndex];
					var nextD = neighbourIndices[(dIndex + 1) % neighbourIndices.Length];
					GridNodeBase n1, n2, n3 = null;
					n1 = node.GetNeighbourAlongDirection(d);
					if (n1 != null && neighbours != NumNeighbours.Six) {
						n3 = n1.GetNeighbourAlongDirection(nextD);
					}

					n2 = node.GetNeighbourAlongDirection(nextD);
					if (n2 != null && n3 == null && neighbours != NumNeighbours.Six) {
						n3 = n2.GetNeighbourAlongDirection(d);
					}

					// Position in graph space of the vertex
					Vector3 p = new Vector3(node.XCoordinateInGrid + 0.5f, 0, node.ZCoordinateInGrid + 0.5f);
					// Offset along diagonal to get the correct XZ coordinates
					p.x += (neighbourXOffsets[d] + neighbourXOffsets[nextD]) * offsetMultiplier;
					p.z += (neighbourZOffsets[d] + neighbourZOffsets[nextD]) * offsetMultiplier;

					// Interpolate the y coordinate of the vertex so that the mesh will be seamless (except in some very rare edge cases)
					p.y += transform.InverseTransform((Vector3)node.position).y;
					if (n1 != null) p.y += transform.InverseTransform((Vector3)n1.position).y;
					if (n2 != null) p.y += transform.InverseTransform((Vector3)n2.position).y;
					if (n3 != null) p.y += transform.InverseTransform((Vector3)n3.position).y;
					p.y /= (1f + (n1 != null ? 1f : 0f) + (n2 != null ? 1f : 0f) + (n3 != null ? 1f : 0f));

					// Convert the point from graph space to world space
					// This handles things like rotations, scale other transformations
					p = transform.Transform(p);
					vertices[baseIndex + dIndex] = p;
				}

				if (neighbours == NumNeighbours.Six) {
					// Form the two middle triangles
					vertices[baseIndex + 6] = vertices[baseIndex + 0];
					vertices[baseIndex + 7] = vertices[baseIndex + 2];
					vertices[baseIndex + 8] = vertices[baseIndex + 3];

					vertices[baseIndex + 9] = vertices[baseIndex + 0];
					vertices[baseIndex + 10] = vertices[baseIndex + 3];
					vertices[baseIndex + 11] = vertices[baseIndex + 5];
				} else {
					// Form the last triangle
					vertices[baseIndex + 4] = vertices[baseIndex + 0];
					vertices[baseIndex + 5] = vertices[baseIndex + 2];
				}

				// Set all colors for the node
				for (int j = 0; j < verticesPerNode; j++) {
					colors[baseIndex + j] = nodeColor;
				}

				// Draw the outline of the node
				for (int j = 0; j < neighbourIndices.Length; j++) {
					var other = node.GetNeighbourAlongDirection(neighbourIndices[(j+1) % neighbourIndices.Length]);
					// Just a tie breaker to make sure we don't draw the line twice.
					// Using NodeInGridIndex instead of NodeIndex to make the gizmos deterministic for a given grid layout.
					// This is important because if the graph would be re-scanned and only a small part of it would change
					// then most chunks would be cached by the gizmo system, but the node indices may have changed and
					// if NodeIndex was used then we might get incorrect gizmos at the borders between chunks.
					if (other == null || (showMeshOutline && node.NodeInGridIndex < other.NodeInGridIndex)) {
						helper.builder.Line(vertices[baseIndex + j], vertices[baseIndex + (j+1) % neighbourIndices.Length], other == null ? Color.black : nodeColor);
					}
				}

				baseIndex += verticesPerNode;
			}

			if (showMeshSurface) helper.DrawTriangles(vertices, colors, baseIndex*trianglesPerNode/verticesPerNode);

			ArrayPool<Vector3>.Release(ref vertices);
			ArrayPool<Color>.Release(ref colors);
		}

		/// <summary>
		/// A rect with all nodes that the bounds could touch.
		/// This correctly handles rotated graphs and other transformations.
		/// The returned rect is guaranteed to not extend outside the graph bounds.
		/// </summary>
		protected IntRect GetRectFromBounds (Bounds bounds) {
			// Take the bounds and transform it using the matrix
			// Then convert that to a rectangle which contains
			// all nodes that might be inside the bounds

			bounds = transform.InverseTransform(bounds);
			Vector3 min = bounds.min;
			Vector3 max = bounds.max;

			int minX = Mathf.RoundToInt(min.x-0.5F);
			int maxX = Mathf.RoundToInt(max.x-0.5F);

			int minZ = Mathf.RoundToInt(min.z-0.5F);
			int maxZ = Mathf.RoundToInt(max.z-0.5F);

			var originalRect = new IntRect(minX, minZ, maxX, maxZ);

			// Rect which covers the whole grid
			var gridRect = new IntRect(0, 0, width-1, depth-1);

			// Clamp the rect to the grid
			return IntRect.Intersection(originalRect, gridRect);
		}

		/// <summary>Deprecated: This method has been renamed to GetNodesInRegion</summary>
		[System.Obsolete("This method has been renamed to GetNodesInRegion", true)]
		public List<GraphNode> GetNodesInArea (Bounds bounds) {
			return GetNodesInRegion(bounds);
		}

		/// <summary>Deprecated: This method has been renamed to GetNodesInRegion</summary>
		[System.Obsolete("This method has been renamed to GetNodesInRegion", true)]
		public List<GraphNode> GetNodesInArea (GraphUpdateShape shape) {
			return GetNodesInRegion(shape);
		}

		/// <summary>Deprecated: This method has been renamed to GetNodesInRegion</summary>
		[System.Obsolete("This method has been renamed to GetNodesInRegion", true)]
		public List<GraphNode> GetNodesInArea (Bounds bounds, GraphUpdateShape shape) {
			return GetNodesInRegion(bounds, shape);
		}

		/// <summary>
		/// All nodes inside the bounding box.
		/// Note: Be nice to the garbage collector and pool the list when you are done with it (optional)
		/// See: Pathfinding.Util.ListPool
		///
		/// See: GetNodesInRegion(GraphUpdateShape)
		/// </summary>
		public List<GraphNode> GetNodesInRegion (Bounds bounds) {
			return GetNodesInRegion(bounds, null);
		}

		/// <summary>
		/// All nodes inside the shape.
		/// Note: Be nice to the garbage collector and pool the list when you are done with it (optional)
		/// See: Pathfinding.Util.ListPool
		///
		/// See: GetNodesInRegion(Bounds)
		/// </summary>
		public List<GraphNode> GetNodesInRegion (GraphUpdateShape shape) {
			return GetNodesInRegion(shape.GetBounds(), shape);
		}

		/// <summary>
		/// All nodes inside the shape or if null, the bounding box.
		/// If a shape is supplied, it is assumed to be contained inside the bounding box.
		/// See: GraphUpdateShape.GetBounds
		/// </summary>
		protected virtual List<GraphNode> GetNodesInRegion (Bounds bounds, GraphUpdateShape shape) {
			var rect = GetRectFromBounds(bounds);

			if (nodes == null || !rect.IsValid() || nodes.Length != width*depth) {
				return ListPool<GraphNode>.Claim();
			}

			// Get a buffer we can use
			var inArea = ListPool<GraphNode>.Claim(rect.Width*rect.Height);

			// Loop through all nodes in the rectangle
			for (int x = rect.xmin; x <= rect.xmax; x++) {
				for (int z = rect.ymin; z <= rect.ymax; z++) {
					int index = z*width+x;

					GraphNode node = nodes[index];

					// If it is contained in the bounds (and optionally the shape)
					// then add it to the buffer
					if (bounds.Contains((Vector3)node.position) && (shape == null || shape.Contains((Vector3)node.position))) {
						inArea.Add(node);
					}
				}
			}

			return inArea;
		}

		/// <summary>Get all nodes in a rectangle.</summary>
		/// <param name="rect">Region in which to return nodes. It will be clamped to the grid.</param>
		public virtual List<GraphNode> GetNodesInRegion (IntRect rect) {
			// Clamp the rect to the grid
			// Rect which covers the whole grid
			var gridRect = new IntRect(0, 0, width-1, depth-1);

			rect = IntRect.Intersection(rect, gridRect);

			if (nodes == null || !rect.IsValid() || nodes.Length != width*depth) return ListPool<GraphNode>.Claim(0);

			// Get a buffer we can use
			var inArea = ListPool<GraphNode>.Claim(rect.Width*rect.Height);


			for (int z = rect.ymin; z <= rect.ymax; z++) {
				var zw = z*Width;
				for (int x = rect.xmin; x <= rect.xmax; x++) {
					inArea.Add(nodes[zw + x]);
				}
			}

			return inArea;
		}

		/// <summary>
		/// Get all nodes in a rectangle.
		/// Returns: The number of nodes written to the buffer.
		///
		/// Note: This method is much faster than GetNodesInRegion(IntRect) which returns a list because this method can make use of the highly optimized
		///  System.Array.Copy method.
		/// </summary>
		/// <param name="rect">Region in which to return nodes. It will be clamped to the grid.</param>
		/// <param name="buffer">Buffer in which the nodes will be stored. Should be at least as large as the number of nodes that can exist in that region.</param>
		public virtual int GetNodesInRegion (IntRect rect, GridNodeBase[] buffer) {
			// Clamp the rect to the grid
			// Rect which covers the whole grid
			var gridRect = new IntRect(0, 0, width-1, depth-1);

			rect = IntRect.Intersection(rect, gridRect);

			if (nodes == null || !rect.IsValid() || nodes.Length != width*depth) return 0;

			if (buffer.Length < rect.Width*rect.Height) throw new System.ArgumentException("Buffer is too small");

			int counter = 0;
			for (int z = rect.ymin; z <= rect.ymax; z++, counter += rect.Width) {
				System.Array.Copy(nodes, z*Width + rect.xmin, buffer, counter, rect.Width);
			}

			return counter;
		}

		/// <summary>
		/// Node in the specified cell.
		/// Returns null if the coordinate is outside the grid.
		///
		/// <code>
		/// var gg = AstarPath.active.data.gridGraph;
		/// int x = 5;
		/// int z = 8;
		/// GridNodeBase node = gg.GetNode(x, z);
		/// </code>
		///
		/// If you know the coordinate is inside the grid and you are looking to maximize performance then you
		/// can look up the node in the internal array directly which is slightly faster.
		/// See: <see cref="nodes"/>
		/// </summary>
		public virtual GridNodeBase GetNode (int x, int z) {
			if (x < 0 || z < 0 || x >= width || z >= depth) return null;
			return nodes[x + z*width];
		}

		GraphUpdateThreading IUpdatableGraph.CanUpdateAsync (GraphUpdateObject o) {
			return GraphUpdateThreading.UnityThread;
		}

		void IUpdatableGraph.UpdateAreaInit (GraphUpdateObject o) {}
		void IUpdatableGraph.UpdateAreaPost (GraphUpdateObject o) {}

		protected void CalculateAffectedRegions (GraphUpdateObject o, out IntRect originalRect, out IntRect affectRect, out IntRect physicsRect, out bool willChangeWalkability, out int erosion) {
			// Take the bounds and transform it using the matrix
			// Then convert that to a rectangle which contains
			// all nodes that might be inside the bounds
			var bounds = transform.InverseTransform(o.bounds);
			Vector3 min = bounds.min;
			Vector3 max = bounds.max;

			int minX = Mathf.RoundToInt(min.x-0.5F);
			int maxX = Mathf.RoundToInt(max.x-0.5F);

			int minZ = Mathf.RoundToInt(min.z-0.5F);
			int maxZ = Mathf.RoundToInt(max.z-0.5F);

			//We now have coordinates in local space (i.e 1 unit = 1 node)
			originalRect = new IntRect(minX, minZ, maxX, maxZ);
			affectRect = originalRect;

			physicsRect = originalRect;

			erosion = o.updateErosion ? erodeIterations : 0;

			willChangeWalkability = o.updatePhysics || o.modifyWalkability;

			//Calculate the largest bounding box which might be affected

			if (o.updatePhysics && !o.modifyWalkability) {
				// Add the collision.diameter margin for physics calls
				if (collision.collisionCheck) {
					Vector3 margin = new Vector3(collision.diameter, 0, collision.diameter)*0.5F;

					min -= margin*1.02F;//0.02 safety margin, physics is rarely very accurate
					max += margin*1.02F;

					physicsRect = new IntRect(
						Mathf.RoundToInt(min.x-0.5F),
						Mathf.RoundToInt(min.z-0.5F),
						Mathf.RoundToInt(max.x-0.5F),
						Mathf.RoundToInt(max.z-0.5F)
						);

					affectRect = IntRect.Union(physicsRect, affectRect);
				}
			}

			if (willChangeWalkability || erosion > 0) {
				// Add affect radius for erosion. +1 for updating connectivity info at the border
				affectRect = affectRect.Expand(erosion + 1);
			}
		}

		public static bool USE_BURST_UPDATE = true;

		internal JobHandleWithMainThreadWork UpdateRegion (IntRect bounds, JobDependencyTracker dependencyTracker, Allocator allocator, JobHandle dependsOn = default) {
			if (nodes == null || nodes.Length != width*depth) throw new System.Exception("The Grid Graph is not scanned, cannot update");

			// Allocate buffers for jobs
			return UpdateAreaBurst(nodes, new int3(width, LayerCount, depth), bounds, dependencyTracker, dependsOn, allocator, RecalculationMode.RecalculateMinimal, null);
		}

		/// <summary>Internal function to update an area of the graph</summary>
		void IUpdatableGraph.UpdateArea (GraphUpdateObject o) {
			if (nodes == null || nodes.Length != width*depth*LayerCount) {
				Debug.LogWarning("The Grid Graph is not scanned, cannot update area");
				//Not scanned
				return;
			}

			IntRect originalRect, affectRect, physicsRect;
			bool willChangeWalkability;
			int erosion;
			CalculateAffectedRegions(o, out originalRect, out affectRect, out physicsRect, out willChangeWalkability, out erosion);

			if (USE_BURST_UPDATE) {
				// Allocate buffers for jobs
				var dependencyTracker = ObjectPool<JobDependencyTracker>.Claim();

				var updateNodesJob = UpdateAreaBurst(nodes, new int3(width, LayerCount, depth), originalRect, dependencyTracker, new JobHandle(), Allocator.TempJob, o.updatePhysics ? RecalculationMode.RecalculateMinimal : RecalculationMode.NoRecalculation, o);
				updateNodesJob.Complete();

				ObjectPool<JobDependencyTracker>.Release(ref dependencyTracker);
				return;
			}
#if ASTARDEBUG || true
			var debugMatrix = transform * Matrix4x4.TRS(new Vector3(0.5f, 0, 0.5f), Quaternion.identity, Vector3.one);

			originalRect.DebugDraw(debugMatrix, Color.red);
			physicsRect.DebugDraw(debugMatrix, Color.yellow);
#endif

			// Rect which covers the whole grid
			var gridRect = new IntRect(0, 0, width-1, depth-1);

			// Clamp the rect to the grid bounds
			IntRect clampedRect = IntRect.Intersection(affectRect, gridRect);

			// Mark nodes that might be changed
			for (int z = clampedRect.ymin; z <= clampedRect.ymax; z++) {
				for (int x = clampedRect.xmin; x <= clampedRect.xmax; x++) {
					o.WillUpdateNode(nodes[z*width+x]);
				}
			}

			// Update Physics
			if (o.updatePhysics && !o.modifyWalkability) {
				collision.Initialize(transform, nodeSize);

				clampedRect = IntRect.Intersection(physicsRect, gridRect);

				for (int z = clampedRect.ymin; z <= clampedRect.ymax; z++) {
					for (int x = clampedRect.xmin; x <= clampedRect.xmax; x++) {
						RecalculateCell(x, z, o.resetPenaltyOnPhysics, false);
					}
				}
			}

			//Apply GUO

			clampedRect = IntRect.Intersection(originalRect, gridRect);
			for (int z = clampedRect.ymin; z <= clampedRect.ymax; z++) {
				for (int x = clampedRect.xmin; x <= clampedRect.xmax; x++) {
					int index = z*width+x;

					var node = nodes[index];

					if (o.bounds.Contains((Vector3)node.position)) {
						if (willChangeWalkability) {
							node.Walkable = node.WalkableErosion;
							o.Apply(node);
							node.WalkableErosion = node.Walkable;
						} else {
							o.Apply(node);
						}
					}
				}
			}

#if ASTARDEBUG
			physicsRect.DebugDraw(debugMatrix, Color.blue);
			affectRect.DebugDraw(debugMatrix, Color.black);
#endif

			// Recalculate connections
			if (willChangeWalkability && erosion == 0) {
				clampedRect = IntRect.Intersection(affectRect, gridRect);
				for (int x = clampedRect.xmin; x <= clampedRect.xmax; x++) {
					for (int z = clampedRect.ymin; z <= clampedRect.ymax; z++) {
						CalculateConnections(x, z);
					}
				}
			} else if (willChangeWalkability && erosion > 0) {
				clampedRect = IntRect.Union(originalRect, physicsRect);

				IntRect erosionRect1 = clampedRect.Expand(erosion);
				IntRect erosionRect2 = erosionRect1.Expand(erosion);

				erosionRect1 = IntRect.Intersection(erosionRect1, gridRect);
				erosionRect2 = IntRect.Intersection(erosionRect2, gridRect);

#if ASTARDEBUG || true
				erosionRect1.DebugDraw(debugMatrix, Color.magenta);
				erosionRect2.DebugDraw(debugMatrix, Color.cyan);
#endif


				// * all nodes inside clampedRect might have had their walkability changed
				// * all nodes inside erosionRect1 might get affected by erosion from clampedRect and erosionRect2
				// * all nodes inside erosionRect2 (but outside erosionRect1) will be reset to previous walkability
				//     after calculation since their erosion might not be correctly calculated (nodes outside erosionRect2 might have an effect)

				for (int x = erosionRect2.xmin; x <= erosionRect2.xmax; x++) {
					for (int z = erosionRect2.ymin; z <= erosionRect2.ymax; z++) {
						int index = z*width+x;

						var node = nodes[index];

						bool tmp = node.Walkable;
						node.Walkable = node.WalkableErosion;

						if (!erosionRect1.Contains(x, z)) {
							//Save the border's walkabilty data (will be reset later)
							node.TmpWalkable = tmp;
						}
					}
				}

				for (int x = erosionRect2.xmin; x <= erosionRect2.xmax; x++) {
					for (int z = erosionRect2.ymin; z <= erosionRect2.ymax; z++) {
						CalculateConnections(x, z);
					}
				}

				// Erode the walkable area
				ErodeWalkableArea(erosionRect2.xmin, erosionRect2.ymin, erosionRect2.xmax+1, erosionRect2.ymax+1);

				for (int x = erosionRect2.xmin; x <= erosionRect2.xmax; x++) {
					for (int z = erosionRect2.ymin; z <= erosionRect2.ymax; z++) {
						if (erosionRect1.Contains(x, z)) continue;

						int index = z*width+x;

						var node = nodes[index];

						//Restore temporarily stored data
						node.Walkable = node.TmpWalkable;
					}
				}

				// Recalculate connections of all affected nodes
				for (int x = erosionRect2.xmin; x <= erosionRect2.xmax; x++) {
					for (int z = erosionRect2.ymin; z <= erosionRect2.ymax; z++) {
						CalculateConnections(x, z);
					}
				}
			}
		}

		/// <summary>
		/// Returns if there is an obstacle between from and to on the graph.
		/// This is not the same as Physics.Linecast, this function traverses the graph and looks for collisions.
		///
		/// <code>
		/// var gg = AstarPath.active.data.gridGraph;
		/// bool anyObstaclesInTheWay = gg.Linecast(transform.position, enemy.position);
		/// </code>
		///
		/// [Open online documentation to see images]
		/// </summary>
		public bool Linecast (Vector3 from, Vector3 to) {
			GraphHitInfo hit;

			return Linecast(from, to, null, out hit);
		}

		/// <summary>
		/// Returns if there is an obstacle between from and to on the graph.
		///
		/// This is not the same as Physics.Linecast, this function traverses the graph and looks for collisions.
		///
		/// <code>
		/// var gg = AstarPath.active.data.gridGraph;
		/// bool anyObstaclesInTheWay = gg.Linecast(transform.position, enemy.position);
		/// </code>
		///
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="from">Point to linecast from</param>
		/// <param name="to">Point to linecast to</param>
		/// <param name="hint">This parameter is deprecated. It will be ignored.</param>
		public bool Linecast (Vector3 from, Vector3 to, GraphNode hint) {
			GraphHitInfo hit;

			return Linecast(from, to, hint, out hit);
		}

		/// <summary>
		/// Returns if there is an obstacle between from and to on the graph.
		///
		/// This is not the same as Physics.Linecast, this function traverses the graph and looks for collisions.
		///
		/// <code>
		/// var gg = AstarPath.active.data.gridGraph;
		/// bool anyObstaclesInTheWay = gg.Linecast(transform.position, enemy.position);
		/// </code>
		///
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="from">Point to linecast from</param>
		/// <param name="to">Point to linecast to</param>
		/// <param name="hit">Contains info on what was hit, see GraphHitInfo</param>
		/// <param name="hint">This parameter is deprecated. It will be ignored.</param>
		public bool Linecast (Vector3 from, Vector3 to, GraphNode hint, out GraphHitInfo hit) {
			return Linecast(from, to, hint, out hit, null);
		}

		/// <summary>Magnitude of the cross product a x b</summary>
		protected static float CrossMagnitude (Vector2 a, Vector2 b) {
			return a.x*b.y - b.x*a.y;
		}

		/// <summary>Magnitude of the cross product a x b</summary>
		protected static long CrossMagnitude (Int2 a, Int2 b) {
			return (long)a.x*b.y - (long)b.x*a.y;
		}

		/// <summary>
		/// Clips a line segment in graph space to the graph bounds.
		/// That is (0,0,0) is the bottom left corner of the graph and (width,0,depth) is the top right corner.
		/// The first node is placed at (0.5,y,0.5). One unit distance is the same as nodeSize.
		///
		/// Returns false if the line segment does not intersect the graph at all.
		/// </summary>
		protected bool ClipLineSegmentToBounds (Vector3 a, Vector3 b, out Vector3 outA, out Vector3 outB) {
			// If the start or end points are outside
			// the graph then clamping is needed
			if (a.x < 0 || a.z < 0 || a.x > width || a.z > depth ||
				b.x < 0 || b.z < 0 || b.x > width || b.z > depth) {
				// Boundary of the grid
				var p1 = new Vector3(0, 0,  0);
				var p2 = new Vector3(0, 0,  depth);
				var p3 = new Vector3(width, 0,  depth);
				var p4 = new Vector3(width, 0,  0);

				int intersectCount = 0;

				bool intersect;
				Vector3 intersection;

				intersection = VectorMath.SegmentIntersectionPointXZ(a, b, p1, p2, out intersect);

				if (intersect) {
					intersectCount++;
					if (!VectorMath.RightOrColinearXZ(p1, p2, a)) {
						a = intersection;
					} else {
						b = intersection;
					}
				}
				intersection = VectorMath.SegmentIntersectionPointXZ(a, b, p2, p3, out intersect);

				if (intersect) {
					intersectCount++;
					if (!VectorMath.RightOrColinearXZ(p2, p3, a)) {
						a = intersection;
					} else {
						b = intersection;
					}
				}
				intersection = VectorMath.SegmentIntersectionPointXZ(a, b, p3, p4, out intersect);

				if (intersect) {
					intersectCount++;
					if (!VectorMath.RightOrColinearXZ(p3, p4, a)) {
						a = intersection;
					} else {
						b = intersection;
					}
				}
				intersection = VectorMath.SegmentIntersectionPointXZ(a, b, p4, p1, out intersect);

				if (intersect) {
					intersectCount++;
					if (!VectorMath.RightOrColinearXZ(p4, p1, a)) {
						a = intersection;
					} else {
						b = intersection;
					}
				}

				if (intersectCount == 0) {
					// The line does not intersect with the grid
					outA = Vector3.zero;
					outB = Vector3.zero;
					return false;
				}
			}

			outA = a;
			outB = b;
			return true;
		}

		/// <summary>
		/// Returns if there is an obstacle between from and to on the graph.
		///
		/// This is not the same as Physics.Linecast, this function traverses the graph and looks for collisions.
		///
		/// <code>
		/// var gg = AstarPath.active.data.gridGraph;
		/// bool anyObstaclesInTheWay = gg.Linecast(transform.position, enemy.position);
		/// </code>
		///
		/// Version: In 3.6.8 this method was rewritten to improve accuracy and performance.
		/// Previously it used a sampling approach which could cut corners of obstacles slightly
		/// and was pretty inefficient.
		///
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="from">Point to linecast from</param>
		/// <param name="to">Point to linecast to</param>
		/// <param name="hit">Contains info on what was hit, see GraphHitInfo</param>
		/// <param name="hint">This parameter is deprecated. It will be ignored.</param>
		/// <param name="trace">If a list is passed, then it will be filled with all nodes the linecast traverses</param>
		public bool Linecast (Vector3 from, Vector3 to, GraphNode hint, out GraphHitInfo hit, List<GraphNode> trace) {
			hit = new GraphHitInfo();

			hit.origin = from;

			Vector3 fromInGraphSpace = transform.InverseTransform(from);
			Vector3 toInGraphSpace = transform.InverseTransform(to);

			// Clip the line so that the start and end points are on the graph
			if (!ClipLineSegmentToBounds(fromInGraphSpace, toInGraphSpace, out fromInGraphSpace, out toInGraphSpace)) {
				// Line does not intersect the graph
				// So there are no obstacles we can hit
				hit.point = to;
				return false;
			}

			// Find the closest nodes to the start and end on the part of the segment which is on the graph
			var startNode = GetNearest(transform.Transform(fromInGraphSpace), NNConstraint.None).node as GridNodeBase;
			var endNode = GetNearest(transform.Transform(toInGraphSpace), NNConstraint.None).node as GridNodeBase;

			if (!startNode.Walkable) {
				hit.node = startNode;
				// Hit point is the point where the segment intersects with the graph boundary
				// or just #from if it starts inside the graph
				hit.point = transform.Transform(fromInGraphSpace);
				hit.tangentOrigin = hit.point;
				return true;
			}

			// Throw away components we don't care about (y)
			// Also subtract 0.5 because nodes have an offset of 0.5 (first node is at (0.5,0.5) not at (0,0))
			// And it's just more convenient to remove that term here.
			// The variable names #from and #to are unfortunately already taken, so let's use start and end.
			var start = new Vector2(fromInGraphSpace.x - 0.5f, fromInGraphSpace.z - 0.5f);
			var end = new Vector2(toInGraphSpace.x - 0.5f, toInGraphSpace.z - 0.5f);

			// Couldn't find a valid node
			// This shouldn't really happen unless there are NO nodes in the graph
			if (startNode == null || endNode == null) {
				hit.node = null;
				hit.point = from;
				return true;
			}

			var dir = end - start;

			// Primary direction that we will move in
			// (e.g up and right or down and left)
			var sign = new Vector2(Mathf.Sign(dir.x), Mathf.Sign(dir.y));

			// How much further we move away from (or towards) the line when walking along #sign
			// This isn't an actual distance. It is a signed distance so it can be negative (other side of the line)
			// Also it includes an additional factor, but the same factor is used everywhere
			// and we only check for if the signed distance is greater or equal to zero so it is ok
			var primaryDirectionError = CrossMagnitude(dir, sign)*0.5f;

			/*            Y/Z
			 *             |
			 *  quadrant   |   quadrant
			 *     1              0
			 *             2
			 *             |
			 *   ----  3 - X - 1  ----- X
			 *             |
			 *             0
			 *  quadrant       quadrant
			 *     2       |      3
			 *             |
			 */

			// Some XORing to get the quadrant index as shown in the diagram above
			int quadrant = (dir.y >= 0 ? 0 : 3) ^ (dir.x >= 0 ? 0 : 1);
			// This will be (1,2) for quadrant 0 and (2,3) for quadrant 1 etc.
			// & 0x3 is just the same thing as % 4 but it is faster
			// This is the direction which moves further to the right of the segment (when looking from the start)
			int directionToReduceError = (quadrant + 1) & 0x3;
			// This is the direction which moves further to the left of the segment (when looking from the start)
			int directionToIncreaseError = (quadrant + 2) & 0x3;

			// Current node. Start at n1
			var current = startNode;

			while (current.NodeInGridIndex != endNode.NodeInGridIndex) {
				// We visited #current so add it to the trace
				if (trace != null) {
					trace.Add(current);
				}

				// Position of the node in 2D graph/node space
				// Here the first node in the graph is at (0,0)
				var p = new Vector2(current.XCoordinateInGrid, current.ZCoordinateInGrid);

				// Calculate the error
				// This is proportional to the distance between the line and the node
				var error = CrossMagnitude(dir, p-start);

				// How does the error change we take one step in the primary direction
				var nerror = error + primaryDirectionError;

				// Check if we need to reduce or increase the error (we want to keep it near zero)
				// and pick the appropriate direction to move in
				int ndir = nerror < 0 ? directionToIncreaseError : directionToReduceError;

				// Check we can move in that direction
				var other = current.GetNeighbourAlongDirection(ndir);
				if (other != null) {
					current = other;
				} else {
					// Hit obstacle
					// We know from what direction we moved in
					// so we can calculate the line which we hit

					// Current direction and current direction ±90 degrees
					var d1 = new Vector2(neighbourXOffsets[ndir], neighbourZOffsets[ndir]);
					var d2 = new Vector2(neighbourXOffsets[(ndir-1+4) & 0x3], neighbourZOffsets[(ndir-1+4) & 0x3]);
					Vector2 lineDirection = new Vector2(neighbourXOffsets[(ndir+1) & 0x3], neighbourZOffsets[(ndir+1) & 0x3]);
					Vector2 lineOrigin = p + (d1 + d2) * 0.5f;

					// Find the intersection
					var intersection = VectorMath.LineIntersectionPoint(lineOrigin, lineOrigin+lineDirection, start, end);

					var currentNodePositionInGraphSpace = transform.InverseTransform((Vector3)current.position);

					// The intersection is in graph space (with an offset of 0.5) so we need to transform it to world space
					var intersection3D = new Vector3(intersection.x + 0.5f, currentNodePositionInGraphSpace.y, intersection.y + 0.5f);
					var lineOrigin3D = new Vector3(lineOrigin.x + 0.5f, currentNodePositionInGraphSpace.y, lineOrigin.y + 0.5f);

					hit.point = transform.Transform(intersection3D);
					hit.tangentOrigin = transform.Transform(lineOrigin3D);
					hit.tangent = transform.TransformVector(new Vector3(lineDirection.x, 0, lineDirection.y));
					hit.node = current;

					return true;
				}
			}

			// Add the last node to the trace
			if (trace != null) {
				trace.Add(current);
			}

			// No obstacles detected
			if (current == endNode) {
				hit.point = to;
				hit.node = current;
				return false;
			}

			// Reached node right above or right below n2 but we cannot reach it
			hit.point = (Vector3)current.position;
			hit.tangentOrigin = hit.point;
			return true;
		}

		/// <summary>
		/// Returns if there is an obstacle between from and to on the graph.
		///
		/// This function is different from the other Linecast functions since it snaps the start and end positions to the centers of the closest nodes on the graph.
		/// This is not the same as Physics.Linecast, this function traverses the graph and looks for collisions.
		///
		/// Version: Since 3.6.8 this method uses the same implementation as the other linecast methods so there is no performance boost to using it.
		/// Version: In 3.6.8 this method was rewritten and that fixed a large number of bugs.
		/// Previously it had not always followed the line exactly as it should have
		/// and the hit output was not very accurate
		/// (for example the hit point was just the node position instead of a point on the edge which was hit).
		/// </summary>
		/// <param name="from">Point to linecast from.</param>
		/// <param name="to">Point to linecast to.</param>
		/// <param name="hit">Contains info on what was hit, see GraphHitInfo.</param>
		/// <param name="hint">This parameter is deprecated. It will be ignored.</param>
		public bool SnappedLinecast (Vector3 from, Vector3 to, GraphNode hint, out GraphHitInfo hit) {
			return Linecast(
				(Vector3)GetNearest(from, NNConstraint.None).node.position,
				(Vector3)GetNearest(to, NNConstraint.None).node.position,
				hint,
				out hit
				);
		}

		/// <summary>
		/// Returns if there is an obstacle between the two nodes on the graph.
		/// This method is very similar to the other Linecast methods however it is much faster
		/// due to being able to use only integer math and not having to look up which node is closest to a particular input point.
		///
		/// <code>
		/// var gg = AstarPath.active.data.gridGraph;
		/// var node1 = gg.GetNode(2, 3);
		/// var node2 = gg.GetNode(5, 7);
		/// bool anyObstaclesInTheWay = gg.Linecast(node1, node2);
		/// </code>
		/// </summary>
		public bool Linecast (GridNodeBase fromNode, GridNodeBase toNode) {
			var dir = new Int2(toNode.XCoordinateInGrid - fromNode.XCoordinateInGrid, toNode.ZCoordinateInGrid - fromNode.ZCoordinateInGrid);

			// How much further we move away from (or towards) the line when walking along the primary direction (e.g up and right or down and left).
			// This isn't an actual distance. It is a signed distance so it can be negative (other side of the line)
			// Also it includes an additional factor, but the same factor is used everywhere
			// and we only check for if the signed distance is greater or equal to zero so it is ok
			long primaryDirectionError = CrossMagnitude(dir, new Int2(System.Math.Sign(dir.x), System.Math.Sign(dir.y)));

			/*            Y/Z
			 *             |
			 *  quadrant   |   quadrant
			 *     1              0
			 *             2
			 *             |
			 *   ----  3 - X - 1  ----- X
			 *             |
			 *             0
			 *  quadrant       quadrant
			 *     2       |      3
			 *             |
			 */

			// Calculate the quadrant index as shown in the diagram above (the axes are part of the quadrants after them in the counter clockwise direction)
			int quadrant = 0;

			if (dir.x <= 0 && dir.y > 0) quadrant = 1;
			else if (dir.x < 0 && dir.y <= 0) quadrant = 2;
			else if (dir.x >= 0 && dir.y < 0) quadrant = 3;

			// This will be (1,2) for quadrant 0 and (2,3) for quadrant 1 etc.
			// & 0x3 is just the same thing as % 4 but it is faster
			// This is the direction which moves further to the right of the segment (when looking from the start)
			int directionToReduceError = (quadrant + 1) & 0x3;
			// This is the direction which moves further to the left of the segment (when looking from the start)
			int directionToIncreaseError = (quadrant + 2) & 0x3;
			int directionDiagonal = (dir.x != 0 && dir.y != 0) ? 4 + ((quadrant + 1) & 0x3) : -1;
			Int2 offset = new Int2(0, 0);
			while (fromNode != null && fromNode.NodeInGridIndex != toNode.NodeInGridIndex) {
				// This is proportional to the distance between the line and the node
				var error = CrossMagnitude(dir, offset) * 2;

				// How does the error change we take one step in the primary direction
				var nerror = error + primaryDirectionError;

				// Check if we need to reduce or increase the error (we want to keep it near zero)
				// and pick the appropriate direction to move in
				int ndir = nerror < 0 ? directionToIncreaseError : directionToReduceError;
				if (nerror == 0 && directionDiagonal != -1) ndir = directionDiagonal;

				fromNode = fromNode.GetNeighbourAlongDirection(ndir);
				offset += new Int2(neighbourXOffsets[ndir], neighbourZOffsets[ndir]);
			}
			return fromNode != toNode;
		}

		/// <summary>
		/// Returns if node is connected to it's neighbour in the specified direction.
		/// This will also return true if <see cref="neighbours"/> = NumNeighbours.Four, the direction is diagonal and one can move through one of the adjacent nodes
		/// to the targeted node.
		///
		/// See: neighbourOffsets
		/// </summary>
		[System.Obsolete("Use GridNode.HasConnectionInDirection instead")]
		public bool CheckConnection (GridNode node, int dir) {
			if (neighbours == NumNeighbours.Eight || neighbours == NumNeighbours.Six || dir < 4) {
				return HasNodeConnection(node, dir);
			} else {
				int dir1 = (dir-4-1) & 0x3;
				int dir2 = (dir-4+1) & 0x3;

				if (!HasNodeConnection(node, dir1) || !HasNodeConnection(node, dir2)) {
					return false;
				} else {
					var n1 = nodes[node.NodeInGridIndex+neighbourOffsets[dir1]];
					var n2 = nodes[node.NodeInGridIndex+neighbourOffsets[dir2]];

					if (!n1.Walkable || !n2.Walkable) {
						return false;
					}

					if (!HasNodeConnection(n2 as GridNode, dir1) || !HasNodeConnection(n1 as GridNode, dir2)) {
						return false;
					}
				}
				return true;
			}
		}

		protected override void SerializeExtraInfo (GraphSerializationContext ctx) {
			if (nodes == null) {
				ctx.writer.Write(-1);
				return;
			}

			ctx.writer.Write(nodes.Length);

			for (int i = 0; i < nodes.Length; i++) {
				nodes[i].SerializeNode(ctx);
			}

			for (int i = 0; i < nodes.Length; i++) {
				ctx.SerializeVector3(new Vector3(nodeSurfaceNormals[i].x, nodeSurfaceNormals[i].y, nodeSurfaceNormals[i].z));
			}
		}

		protected override void DeserializeExtraInfo (GraphSerializationContext ctx) {
			int count = ctx.reader.ReadInt32();

			if (count == -1) {
				nodes = null;
				return;
			}

			nodes = new GridNode[count];

			for (int i = 0; i < nodes.Length; i++) {
				nodes[i] = new GridNode(active);
				nodes[i].DeserializeNode(ctx);
			}

			if (nodeSurfaceNormals.IsCreated) nodeSurfaceNormals.Dispose();
			nodeSurfaceNormals = new NativeArray<float4>(nodes.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			if (ctx.meta.version >= AstarSerializer.V4_3_6) {
				var v = ctx.DeserializeVector3();
				for (int i = 0; i < nodes.Length; i++) nodeSurfaceNormals[i] = new float4(v.x, v.y, v.z, 0);
			} else {
				// For backwards compatibility
				nodeSurfaceNormals.MemSet(new float4(0, 1, 0, 0)).Schedule().Complete();
			}
		}

		protected override void DeserializeSettingsCompatibility (GraphSerializationContext ctx) {
			base.DeserializeSettingsCompatibility(ctx);

			aspectRatio = ctx.reader.ReadSingle();
			rotation = ctx.DeserializeVector3();
			center = ctx.DeserializeVector3();
			unclampedSize = (Vector2)ctx.DeserializeVector3();
			nodeSize = ctx.reader.ReadSingle();
			collision.DeserializeSettingsCompatibility(ctx);
			maxStepHeight = ctx.reader.ReadSingle();
			ctx.reader.ReadInt32();
			maxSlope = ctx.reader.ReadSingle();
			erodeIterations = ctx.reader.ReadInt32();
			erosionUseTags = ctx.reader.ReadBoolean();
			erosionFirstTag = ctx.reader.ReadInt32();
			ctx.reader.ReadBoolean(); // Old field
			neighbours = (NumNeighbours)ctx.reader.ReadInt32();
			cutCorners = ctx.reader.ReadBoolean();
#pragma warning disable CS0618 // Type or member is obsolete
			penaltyPosition = ctx.reader.ReadBoolean();
			penaltyPositionFactor = ctx.reader.ReadSingle();
			penaltyAngle = ctx.reader.ReadBoolean();
			penaltyAngleFactor = ctx.reader.ReadSingle();
			penaltyAnglePower = ctx.reader.ReadSingle();
#pragma warning restore CS0618 // Type or member is obsolete
			isometricAngle = ctx.reader.ReadSingle();
			uniformEdgeCosts = ctx.reader.ReadBoolean();
			useJumpPointSearch = ctx.reader.ReadBoolean();
		}

		void HandleBackwardsCompatibility (GraphSerializationContext ctx) {
			// For compatibility
			if (ctx.meta.version <= AstarSerializer.V4_3_2) maxStepUsesSlope = false;

#pragma warning disable CS0618 // Type or member is obsolete
			if (penaltyPosition) {
				penaltyPosition = false;
				// Can't convert it exactly. So assume there are no nodes with an elevation greater than 1000
				rules.rules.Add(new RuleElevationPenalty {
					penaltyScale = Int3.Precision * penaltyPositionFactor * 1000.0f,
					elevationRange = new Vector2(-penaltyPositionOffset/Int3.Precision, -penaltyPositionOffset/Int3.Precision + 1000),
					curve = AnimationCurve.Linear(0, 0, 1, 1),
				});
			}

			if (penaltyAngle) {
				penaltyAngle = false;

				// Approximate the legacy behavior with an animation curve
				var curve = AnimationCurve.Linear(0, 0, 1, 1);
				var keys = new Keyframe[7];
				for (int i = 0; i < keys.Length; i++) {
					var angle = Mathf.PI*0.5f*i/(keys.Length-1);
					var penalty = (1F-Mathf.Pow(Mathf.Cos(angle), penaltyAnglePower))*penaltyAngleFactor;
					var key = new Keyframe(Mathf.Rad2Deg * angle, penalty);
					keys[i] = key;
				}
				var maxPenalty = keys.Max(k => k.value);
				if (maxPenalty > 0) for (int i = 0; i < keys.Length; i++) keys[i].value /= maxPenalty;
				curve.keys = keys;
				for (int i = 0; i < keys.Length; i++) {
					curve.SmoothTangents(i, 0.5f);
				}

				rules.rules.Add(new RuleAnglePenalty {
					penaltyScale = maxPenalty,
					curve = curve,
				});
			}

			if (textureData.enabled) {
				textureData.enabled = false;
				var channelScales = textureData.factors.Select(x => x/255.0f).ToList();
				while (channelScales.Count < 4) channelScales.Add(1000);
				var channels = textureData.channels.Cast<RuleTexture.ChannelUse>().ToList();
				while (channels.Count < 4) channels.Add(RuleTexture.ChannelUse.None);

				rules.rules.Add(new RuleTexture {
					texture = textureData.source,
					channels = channels.ToArray(),
					channelScales = channelScales.ToArray(),
					scalingMode = RuleTexture.ScalingMode.FixedScale,
					nodesPerPixel = 1.0f,
				});
			}
#pragma warning restore CS0618 // Type or member is obsolete
		}

		protected override void PostDeserialization (GraphSerializationContext ctx) {
			HandleBackwardsCompatibility(ctx);

			UpdateTransform();
			SetUpOffsetsAndCosts();
			GridNode.SetGridGraph((int)graphIndex, this);

			// Deserialize all nodes
			if (nodes == null || nodes.Length == 0) return;

			if (width*depth != nodes.Length) {
				Debug.LogError("Node data did not match with bounds data. Probably a change to the bounds/width/depth data was made after scanning the graph just prior to saving it. Nodes will be discarded");
				nodes = new GridNodeBase[0];
				return;
			}

			for (int z = 0; z < depth; z++) {
				for (int x = 0; x < width; x++) {
					var node = nodes[z*width+x];

					if (node == null) {
						Debug.LogError("Deserialization Error : Couldn't cast the node to the appropriate type - GridGenerator");
						return;
					}

					node.NodeInGridIndex = z*width+x;
				}
			}
		}
	}

	/// <summary>
	/// Number of neighbours for a single grid node.
	/// \since The 'Six' item was added in 3.6.1
	/// </summary>
	public enum NumNeighbours {
		Four,
		Eight,
		Six
	}
}
