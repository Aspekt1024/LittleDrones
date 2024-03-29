using System;
using Unity.Collections.LowLevel.Unsafe;
using static Drawing.DrawingData;
using Unity.Mathematics;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine.Profiling;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Rendering;
using System.Runtime.InteropServices;

namespace Drawing {
	/// <summary>
	/// Builder for drawing commands.
	/// You can use this to queue many drawing commands. The commands will be queued for rendering when you call the Dipose method.
	/// It is recommended that you use the <a href="https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/using-statement">using statement</a> which automatically calls the Dispose method.
	///
	/// <code>
	/// // Create a new CommandBuilder
	/// using (var draw = DrawingManager.instance.gizmos.GetBuilder()) {
	///     // Use the exact same API as the global Draw class
	///     draw.WireBox(Vector3.zero, Vector3.one);
	/// }
	/// </code>
	///
	/// Warning: You must call either Dipose or DiscardAndDispose when you are done with this object to avoid memory leaks.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	[BurstCompile]
	public partial struct CommandBuilder : IDisposable {
		// Note: Many fields/methods are explicitly marked as private. This is because doxygen otherwise thinks they are public by default (like struct members are in c++)

		[NativeDisableUnsafePtrRestriction]
		private unsafe UnsafeAppendBuffer* buffer;

		private GCHandle gizmos;

		[NativeSetThreadIndex]
		private int threadIndex;

		private int uniqueID;

		internal CommandBuilder(DrawingData gizmos, Hasher hasher, RedrawScope frameRedrawScope, RedrawScope customRedrawScope, bool isGizmos) {
			// We need to use a GCHandle instead of a normal reference to be able to pass this object to burst compiled function pointers.
			// The NativeSetClassTypeToNullOnSchedule unfortunately only works together with the job system, not with raw functions.
			this.gizmos = GCHandle.Alloc(gizmos, GCHandleType.Normal);

			threadIndex = 0;
			uniqueID = gizmos.data.Reserve();
			gizmos.data.Get(uniqueID).Init(hasher, frameRedrawScope, customRedrawScope, isGizmos);
			unsafe {
				buffer = gizmos.data.Get(uniqueID).bufferPtr;
			}
		}

		/// <summary>Submits this command builder for rendering</summary>
		public void Dispose () {
			if (!gizmos.IsAllocated) throw new System.Exception("You cannot dispose an invalid command builder. Are you trying to dipose it twice?");
			if (gizmos.IsAllocated && gizmos.Target != null) (gizmos.Target as DrawingData).data.Get(uniqueID).Submit(gizmos.Target as DrawingData);
			gizmos.Free();
			this = default;
		}

		/// <summary>
		/// Discards the contents of this command builder without rendering anything.
		/// If you are not going to draw anything (i.e. you do not call the <see cref="Dispose"/> method) then you must call this method to avoid
		/// memory leaks.
		/// </summary>
		public void DiscardAndDispose () {
			if (gizmos.IsAllocated && gizmos.Target != null) (gizmos.Target as DrawingData).data.Release(uniqueID);
			gizmos.Free();
			this = default;
		}


		public void Preallocate (int size) {
			Reserve(size);
		}

		/// <summary>Internal rendering command</summary>
		private enum Command {
			PushColorInline = 1 << 8,
			PushColor = 0,
			PopColor,
			PushMatrix,
			PushSetMatrix,
			PopMatrix,
			Line,
			Circle,
			CircleXZ,
			Box,
			PushPersist,
			PopPersist,
		}

		/// <summary>Holds rendering data for a line</summary>
		private struct LineData {
			public float3 a, b;
		}

		private struct LineDataV3 {
			public Vector3 a, b;
		}

		/// <summary>Holds rendering data for a circle</summary>
		private struct CircleXZData {
			public float3 center;
			public float radius, startAngle, endAngle;
		}

		/// <summary>Holds rendering data for a circle</summary>
		private struct CircleData {
			public float3 center;
			public float3 normal;
			public float radius;
		}

		/// <summary>Holds rendering data for a box</summary>
		private struct BoxData {
			public float3 center;
			public float3 size;
		}

		private struct PersistData {
			public float endTime;
		}

		/// <summary>Ensures the buffer has room for at least N more bytes</summary>
		private void Reserve (int additionalSpace) {
			unsafe {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
				if (buffer == null) throw new System.Exception("CommandBuilder does not have a valid buffer. Is it properly initialized?");
#endif
				if (threadIndex != 0) {
					if (threadIndex < 0 || threadIndex >= JobsUtility.MaxJobThreadCount) throw new System.Exception("Thread index outside the expected range");

					//if (buffer->Size + additionalSpace > buffer->Capacity) throw new System.Exception("Buffer is too small. Preallocate a larger buffer using the CommandBuffer.Preallocate method.");
					buffer += threadIndex;
					threadIndex = 0;
				}

#if ENABLE_UNITY_COLLECTIONS_CHECKS
				if (buffer->Length == 0) {
					// Exploit the fact that right after this package has drawn gizmos the buffers will be empty
					// and the next task is that Unity will render its own internal gizmos.
					// We can therefore easily (and without a high performance cost)
					// trap accidental Draw.* calls from OnDrawGizmos functions
					// by doing this check when the buffer is empty.
					AssertNotRendering();
				}
#endif

				var newLength = buffer->Length + additionalSpace;
				if (newLength > buffer->Capacity) {
					buffer->SetCapacity(math.max(newLength, buffer->Length * 2));
				}
			}
		}

		[BurstDiscard]
		static void AssertNotRendering () {
			// Assumes that outside of play mode the only time the Draw.* functions should be called are
			// inside DrawGizmos functions.
			if (!GizmoContext.drawingGizmos && !JobsUtility.IsExecutingJob && !Application.isPlaying) {
				// Inspect the stack-trace to be able to provide more helpful error messages
				var st = StackTraceUtility.ExtractStackTrace();
				if (st.Contains("OnDrawGizmos")) {
					throw new System.Exception("You are trying to use Draw.* functions from within Unity's OnDrawGizmos function. Use this package's gizmo callbacks instead (see the documentation).");
				} else {
					throw new System.Exception("It seems like you are trying to use the Draw.* functions from within Unity's OnDrawGizmos function or another camera rendering callback. This is not allowed. Use this package's gizmo callbacks instead (see the documentation)");
				}
			}
		}

		private void Reserve<A>() where A : struct {
			Reserve(UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<A>());
		}

		private void Reserve<A, B>() where A : struct where B : struct {
			Reserve(UnsafeUtility.SizeOf<Command>() * 2 + UnsafeUtility.SizeOf<A>() + UnsafeUtility.SizeOf<B>());
		}

		private void Reserve<A, B, C>() where A : struct where B : struct where C : struct {
			Reserve(UnsafeUtility.SizeOf<Command>() * 3 + UnsafeUtility.SizeOf<A>() + UnsafeUtility.SizeOf<B>() + UnsafeUtility.SizeOf<C>());
		}

		private unsafe void Add<T>(T value) where T : struct {
			int num = UnsafeUtility.SizeOf<T>();

			unsafe {
				UnsafeUtility.CopyStructureToPtr(ref value, (void*)((byte*)buffer->Ptr + buffer->Length));
				buffer->Length += num;
			}
		}

		public struct ScopeMatrix : IDisposable {
			internal unsafe UnsafeAppendBuffer* buffer;
			internal GCHandle gizmos;
			internal int uniqueID;
			public void Dispose () {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
				if (gizmos.IsAllocated && gizmos.Target != null && !(gizmos.Target as DrawingData).data.StillExists(uniqueID)) throw new System.InvalidOperationException("The drawing instance this matrix scope belongs to no longer exists. Matrix scopes cannot survive for longer than a frame unless you have a custom drawing instance. Are you using a matrix scope inside a coroutine?");
#endif
				unsafe {
					new CommandBuilder { buffer = buffer, threadIndex = 0 }.PopMatrix();
					buffer = null;
				}
			}
		}

		public struct ScopeColor : IDisposable {
			internal unsafe UnsafeAppendBuffer* buffer;
			internal GCHandle gizmos;
			internal int uniqueID;
			public void Dispose () {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
				if (gizmos.IsAllocated && gizmos.Target != null && !(gizmos.Target as DrawingData).data.StillExists(uniqueID)) throw new System.InvalidOperationException("The drawing instance this color scope belongs to no longer exists. Color scopes cannot survive for longer than a frame unless you have a custom drawing instance. Are you using a color scope inside a coroutine?");
#endif
				unsafe {
					new CommandBuilder { buffer = buffer, threadIndex = 0 }.PopColor();
					buffer = null;
				}
			}
		}

		public struct ScopePersist : IDisposable {
			internal unsafe UnsafeAppendBuffer* buffer;
			internal GCHandle gizmos;
			internal int uniqueID;
			public void Dispose () {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
				if (gizmos.IsAllocated && gizmos.Target != null && !(gizmos.Target as DrawingData).data.StillExists(uniqueID)) throw new System.InvalidOperationException("The drawing instance this persist scope belongs to no longer exists. Persist scopes cannot survive for longer than a frame unless you have a custom drawing instance. Are you using a persist scope inside a coroutine?");
#endif
				unsafe {
					new CommandBuilder { buffer = buffer, threadIndex = 0 }.PopPersist();
					buffer = null;
				}
			}
		}

		/// <summary>
		/// Scope to draw multiple things with an implicit matrix transformation.
		/// All coordinates for items drawn inside the scope will be multiplied by the matrix.
		/// If WithMatrix scopes are nested then coordinates are multiplied by all nested matrices in order.
		///
		/// <code>
		/// using (Draw.InLocalSpace(transform)) {
		///     // Draw a box at (0,0,0) relative to the current object
		///     // This means it will show up at the object's position
		///     Draw.WireBox(Vector3.zero, Vector3.one);
		/// }
		///
		/// // Equivalent code using the lower level WithMatrix scope
		/// using (Draw.WithMatrix(transform.localToWorldMatrix)) {
		///     Draw.WireBox(Vector3.zero, Vector3.one);
		/// }
		/// </code>
		///
		/// See: <see cref="InLocalSpace"/>
		/// </summary>
		[BurstDiscard]
		public ScopeMatrix WithMatrix (Matrix4x4 matrix) {
			PushMatrix(matrix);
			// TODO: Keep track of alive scopes and prevent dispose unless all scopes have been disposed
			unsafe {
				return new ScopeMatrix { buffer = buffer, gizmos = gizmos, uniqueID = uniqueID };
			}
		}

		/// <summary>
		/// Scope to draw multiple things with the same color.
		///
		/// <code>
		/// void Update () {
		///     using (Draw.WithColor(Color.red)) {
		///         Draw.Line(new Vector3(0, 0, 0), new Vector3(1, 1, 1));
		///         Draw.Line(new Vector3(0, 0, 0), new Vector3(0, 1, 2));
		///     }
		/// }
		/// </code>
		///
		/// Any command that is passed an explicit color parameter will override this color.
		/// If another color scope is nested inside this one then that scope will override this color.
		/// </summary>
		[BurstDiscard]
		public ScopeColor WithColor (Color color) {
			PushColor(color);
			unsafe {
				return new ScopeColor { buffer = buffer, gizmos = gizmos, uniqueID = uniqueID };
			}
		}

		/// <summary>
		/// Scope to draw multiple things for a longer period of time.
		///
		/// Normally drawn items will only be rendered for a single frame.
		/// Using a persist scope you can make the items be drawn for any amount of time.
		///
		/// <code>
		/// void Update () {
		///     using (Draw.WithDuration(1.0f)) {
		///         var offset = Time.time;
		///         Draw.Line(new Vector3(offset, 0, 0), new Vector3(offset, 0, 1));
		///     }
		/// }
		/// </code>
		///
		/// Warning: This is not recommended inside a DrawGizmos callback since DrawGizmos is called every frame anyway.
		/// </summary>
		/// <param name="duration">How long the drawn items should persist in seconds.</param>

		[BurstDiscard]
		public ScopePersist WithDuration (float duration) {
			PushPersist(duration);
			unsafe {
				return new ScopePersist { buffer = buffer, gizmos = gizmos, uniqueID = uniqueID };
			}
		}

		/// <summary>
		/// Scope to draw multiple things relative to a transform object.
		/// All coordinates for items drawn inside the scope will be multiplied by the transform's localToWorldMatrix.
		///
		/// <code>
		/// void Update () {
		///     using (Draw.InLocalSpace(transform)) {
		///         // Draw a box at (0,0,0) relative to the current object
		///         // This means it will show up at the object's position
		///         // The box is also rotated and scaled with the transform
		///         Draw.WireBox(Vector3.zero, Vector3.one);
		///     }
		/// }
		/// </code>
		///
		/// [Open online documentation to see videos]
		/// </summary>
		[BurstDiscard]
		public ScopeMatrix InLocalSpace (Transform transform) {
			return WithMatrix(transform.localToWorldMatrix);
		}

		/// <summary>
		/// Scope to draw multiple things in screen space of a camera.
		/// If you draw 2D coordinates (i.e. (x,y,0)) they will be projected onto a plane (2*near clip plane of the camera) world units in front of the camera.
		///
		/// The lower left corner of the camera is (0,0,0) and the upper right is (camera.pixelWidth, camera.pixelHeight, 0)
		///
		/// See: <see cref="InLocalSpace"/>
		/// See: <see cref="WithMatrix"/>
		/// </summary>
		[BurstDiscard]
		public ScopeMatrix InScreenSpace (Camera camera) {
			return WithMatrix(camera.cameraToWorldMatrix * camera.projectionMatrix.inverse * Matrix4x4.TRS(new Vector3(-1.0f, -1.0f, 0), Quaternion.identity, new Vector3(2.0f/camera.pixelWidth, 2.0f/camera.pixelHeight, 1)));
		}

		/// <summary>
		/// Multiply all coordinates until the next PopMatrix with the given matrix.
		/// This differs from <see cref="PushSetMatrix"/> in that this stacks with all previously pushed matrices while <see cref="PushSetMatrix"/> does not.
		/// </summary>
		public void PushMatrix (Matrix4x4 matrix) {
			Reserve<float4x4>();
			Add(Command.PushMatrix);
			Add((float4x4)matrix);
		}

		/// <summary>
		/// Multiply all coordinates until the next PopMatrix with the given matrix.
		/// This differs from <see cref="PushMatrix"/> in that this sets the current matrix directly while <see cref="PushMatrix"/> stacks with all previously pushed matrices.
		/// </summary>
		public void PushSetMatrix (Matrix4x4 matrix) {
			Reserve<float4x4>();
			Add(Command.PushSetMatrix);
			Add((float4x4)matrix);
		}

		/// <summary>Pops a matrix from the stack</summary>
		public void PopMatrix () {
			Reserve(4);
			Add(Command.PopMatrix);
		}

		/// <summary>
		/// Draws everything until the next PopColor with the given color.
		/// Any command that is passed an explicit color parameter will override this color.
		/// If another color scope is nested inside this one then that scope will override this color.
		/// </summary>
		public void PushColor (Color color) {
			Reserve<Color32>();
			Add(Command.PushColor);
			Add((Color32)color);
		}

		/// <summary>Pops a color from the stack</summary>
		public void PopColor () {
			Reserve(4);
			Add(Command.PopColor);
		}

		/// <summary>
		/// Draws everything until the next PopPersist for a number of seconds.
		/// Warning: This is not recommended inside a DrawGizmos callback since DrawGizmos is called every frame anyway.
		/// </summary>
		public void PushPersist (float duration) {
			if (Application.isPlaying) {
				Reserve<PersistData>();
				Add(Command.PushPersist);
				Add(new PersistData { endTime = Time.time + duration });
			}
		}

		/// <summary>Pops a persist scope from the stack</summary>
		public void PopPersist () {
			if (Application.isPlaying) {
				Reserve(4);
				Add(Command.PopPersist);
			}
		}

		/// <summary>
		/// Draws a line between two points.
		///
		/// [Open online documentation to see images]
		///
		/// <code>
		/// void Update () {
		///     Draw.Line(Vector3.zero, Vector3.up);
		/// }
		/// </code>
		/// </summary>
		public void Line (float3 a, float3 b) {
			Reserve<LineData>();
			Add(Command.Line);
			Add(new LineData { a = a, b = b });
		}

		public void Line (Vector3 a, Vector3 b) {
			Reserve<LineData>();
			Add(Command.Line);
			Add(new LineDataV3 { a = a, b = b });
		}

		/// <summary>
		/// Draws a ray starting at a point and going in the given direction.
		/// The ray will end at origin + direction.
		///
		/// [Open online documentation to see images]
		///
		/// <code>
		/// Draw.Ray(Vector3.zero, Vector3.up);
		/// </code>
		/// </summary>
		public void Ray (float3 origin, float3 direction) {
			Line(origin, origin + direction);
		}

		/// <summary>
		/// Draws a ray with a given length.
		///
		/// [Open online documentation to see images]
		///
		/// <code>
		/// Draw.Ray(Camera.main.ScreenPointToRay(Vector3.zero), 10);
		/// </code>
		/// </summary>
		public void Ray (Ray ray, float length) {
			Line(ray.origin, ray.origin + ray.direction * length);
		}

		/// <summary>
		/// Draws a circle in the XZ plane.
		/// You can draw an arc by supplying the startAngle and endAngle parameters.
		///
		/// [Open online documentation to see images]
		/// </summary>
		public void CircleXZ (float3 center, float radius, float startAngle = 0f, float endAngle = 2 * Mathf.PI) {
			Reserve<CircleXZData>();
			Add(Command.CircleXZ);
			Add(new CircleXZData { center = center, radius = radius, startAngle = startAngle, endAngle = endAngle });
		}

		static readonly Matrix4x4 XZtoXYPlaneMatrix = Matrix4x4.Rotate(Quaternion.Euler(new Vector3(-90, 0, 0)));
		static readonly Matrix4x4 XZtoYZPlaneMatrix = Matrix4x4.Rotate(Quaternion.Euler(new Vector3(0, 0, 90)));

		/// <summary>
		/// Draws a circle in the XY plane.
		/// You can draw an arc by supplying the startAngle and endAngle parameters.
		///
		/// [Open online documentation to see images]
		/// </summary>
		public void CircleXY (float3 center, float radius, float startAngle = 0f, float endAngle = 2 * Mathf.PI) {
			PushMatrix(XZtoXYPlaneMatrix);
			CircleXZ(new float3(center.x, -center.z, center.y), radius, startAngle, endAngle);
			PopMatrix();
		}

		/// <summary>
		/// Draws a circle in the XY plane.
		///
		/// Note: This overload does not allow you to draw an arc. Use <see cref="CircleXY"/> or <see cref="CircleXZ"/> instead.
		///
		/// [Open online documentation to see images]
		/// </summary>
		public void Circle (float3 center, float3 normal, float radius) {
			Reserve<CircleData>();
			Add(Command.Circle);
			Add(new CircleData { center = center, normal = normal, radius = radius });
		}

		/// <summary>
		/// Draws a cylinder.
		/// The cylinder's bottom circle will be centered at the bottom parameter and similarly for the top circle.
		///
		/// <code>
		/// // Draw a tilted cylinder between the points (0,0,0) and (1,1,1) with a radius of 0.5
		/// Draw.WireCylinder(Vector3.zero, Vector3.one, 0.5f, Color.magenta);
		/// </code>
		///
		/// [Open online documentation to see images]
		/// </summary>
		public void WireCylinder (float3 bottom, float3 top, float radius) {
			WireCylinder(bottom, top - bottom, math.length(top - bottom), radius);
		}

		/// <summary>
		/// Draws a cylinder.
		/// The cylinder's bottom circle will be centered at the position parameter.
		/// The cylinder's orientation will be determined by the up and height parameters.
		///
		/// <code>
		/// // Draw a two meter tall cylinder at the world origin with a radius of 0.5
		/// Draw.WireCylinder(Vector3.zero, Vector3.up, 2, 0.5f, Color.magenta);
		/// </code>
		///
		/// [Open online documentation to see images]
		/// </summary>
		public void WireCylinder (float3 position, float3 up, float height, float radius) {
			var tangent = math.normalizesafe(math.cross(up, new float3(1, 1, 1)));

			using (WithMatrix(Matrix4x4.TRS(position, Quaternion.LookRotation(tangent, up), new Vector3(radius, height, radius)))) {
				CircleXZ(float3.zero, 1);
				if (height > 0) {
					CircleXZ(new float3(0, 1, 0), 1);
					Line(new float3(1, 0, 0), new float3(1, 1, 0));
					Line(new float3(-1, 0, 0), new float3(-1, 1, 0));
					Line(new float3(0, 0, 1), new float3(0, 1, 1));
					Line(new float3(0, 0, -1), new float3(0, 1, -1));
				}
			}
		}

		/// <summary>
		/// Draws a capsule.
		/// The capsule's lower circle will be centered at the bottom parameter and similarly for the upper circle.
		///
		/// <code>
		/// // Draw a tilted capsule between the points (0,0,0) and (1,1,1) with a radius of 0.5
		/// Draw.WireCapsule(Vector3.zero, Vector3.one, 0.5f, Color.magenta);
		/// </code>
		///
		/// [Open online documentation to see images]
		/// </summary>
		public void WireCapsule (float3 bottom, float3 top, float radius) {
			WireCapsule(bottom, top - bottom, math.length(top - bottom), radius);
		}

		// TODO: Change to center, up, height parametrization
		/// <summary>
		/// Draws a capsule.
		///
		/// <code>
		/// // Draw a tilted capsule between the points (0,0,0) and (1,1,1) with a radius of 0.5
		/// Draw.WireCapsule(Vector3.zero, Vector3.one, 0.5f, Color.magenta);
		/// </code>
		///
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="position">Lowest point of the capsule.</param>
		/// <param name="up">Up direction of the capsule.</param>
		/// <param name="height">Distance between the lowest and highest points of the capsule. The height will be clamped to be at least 2*radius.</param>
		/// <param name="radius">The radius of the capsule.</param>
		public void WireCapsule (float3 position, float3 up, float height, float radius) {
			up = math.normalizesafe(up);
			// Note; second parameter is normalized (1,1,1)
			var tangent = math.cross(up, new float3(0.577350269f, 0.577350269f, 0.577350269f));
			height = math.max(height, radius*2);

			using (WithMatrix(Matrix4x4.TRS(position, Quaternion.LookRotation(tangent, up), Vector3.one))) {
				CircleXZ(new float3(0, radius, 0), radius);
				CircleXY(new float3(0, radius, 0), radius, Mathf.PI, 2 * Mathf.PI);
				PushMatrix(XZtoYZPlaneMatrix);
				CircleXZ(new float3(radius, 0, 0), radius, Mathf.PI*0.5f, Mathf.PI*1.5f);
				PopMatrix();
				if (height > 0) {
					var upperY = height - radius;
					var lowerY = radius;
					CircleXZ(new float3(0, upperY, 0), radius);
					CircleXY(new float3(0, upperY, 0), radius, 0, Mathf.PI);
					PushMatrix(XZtoYZPlaneMatrix);
					CircleXZ(new float3(upperY, 0, 0), radius, -Mathf.PI*0.5f, Mathf.PI*0.5f);
					PopMatrix();
					Line(new float3(radius, lowerY, 0), new float3(radius, upperY, 0));
					Line(new float3(-radius, lowerY, 0), new float3(-radius, upperY, 0));
					Line(new float3(0, lowerY, radius), new float3(0, upperY, radius));
					Line(new float3(0, lowerY, -radius), new float3(0, upperY, -radius));
				}
			}
		}

		/// <summary>
		/// Draws a wire sphere.
		///
		/// [Open online documentation to see images]
		///
		/// <code>
		/// // Draw a wire sphere at the origin with a radius of 0.5
		/// Draw.WireSphere(Vector3.zero, 0.5f, Color.magenta);
		/// </code>
		///
		/// See: <see cref="Circle"/>
		/// </summary>
		public void WireSphere (float3 position, float radius) {
			Circle(position, new float3(1, 0, 0), radius);
			Circle(position, new float3(0, 1, 0), radius);
			Circle(position, new float3(0, 0, 1), radius);
		}

		/// <summary>
		/// Draws lines through a sequence of points.
		///
		/// [Open online documentation to see images]
		/// <code>
		/// // Draw a square
		/// Draw.Polyline(new [] { new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(0, 1, 0) }, true);
		/// </code>
		/// </summary>
		/// <param name="points">Seq u ence of points to draw lines through</param>
		/// <param name="cycle">If true a line will be drawn from the last point in the sequence back to the first point.</param>
		[BurstDiscard]
		public void Polyline (List<Vector3> points, bool cycle = false) {
			for (int i = 0; i < points.Count - 1; i++) {
				Line(points[i], points[i+1]);
			}
			if (cycle && points.Count > 1) Line(points[points.Count - 1], points[0]);
		}

		/// <summary>
		/// Draws lines through a sequence of points.
		///
		/// [Open online documentation to see images]
		/// <code>
		/// // Draw a square
		/// Draw.Polyline(new [] { new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(0, 1, 0) }, true);
		/// </code>
		/// </summary>
		/// <param name="points">Sequence of points to draw lines through</param>
		/// <param name="cycle">If true a line will be drawn from the last point in the sequence back to the first point.</param>
		[BurstDiscard]
		public void Polyline (Vector3[] points, bool cycle = false) {
			for (int i = 0; i < points.Length - 1; i++) {
				Line(points[i], points[i+1]);
			}
			if (cycle && points.Length > 1) Line(points[points.Length - 1], points[0]);
		}

		/// <summary>
		/// Draws lines through a sequence of points.
		///
		/// [Open online documentation to see images]
		/// <code>
		/// // Draw a square
		/// Draw.Polyline(new [] { new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(0, 1, 0) }, true);
		/// </code>
		/// </summary>
		/// <param name="points">Sequence of points to draw lines through</param>
		/// <param name="cycle">If true a line will be drawn from the last point in the sequence back to the first point.</param>
		[BurstDiscard]
		public void Polyline (float3[] points, bool cycle = false) {
			for (int i = 0; i < points.Length - 1; i++) {
				Line(points[i], points[i+1]);
			}
			if (cycle && points.Length > 1) Line(points[points.Length - 1], points[0]);
		}

		/// <summary>
		/// Draws lines through a sequence of points.
		///
		/// [Open online documentation to see images]
		/// <code>
		/// // Draw a square
		/// Draw.Polyline(new [] { new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(0, 1, 0) }, true);
		/// </code>
		/// </summary>
		/// <param name="points">Sequence of points to draw lines through</param>
		/// <param name="cycle">If true a line will be drawn from the last point in the sequence back to the first point.</param>
		public void Polyline (NativeArray<float3> points, bool cycle = false) {
			for (int i = 0; i < points.Length - 1; i++) {
				Line(points[i], points[i+1]);
			}
			if (cycle && points.Length > 1) Line(points[points.Length - 1], points[0]);
		}

		/// <summary>
		/// Draws the outline of an axis aligned box.
		///
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="center">Center of the box</param>
		/// <param name="size">Width of the box along all dimensions</param>
		public void WireBox (float3 center, float3 size) {
			WireBox(new Bounds(center, size));
		}

		/// <summary>
		/// Draws the outline of a box.
		///
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="center">Center of the box</param>
		/// <param name="rotation">Rotation of the box</param>
		/// <param name="size">Width of the box along all dimensions</param>
		public void WireBox (float3 center, Quaternion rotation, float3 size) {
			using (WithMatrix(Matrix4x4.TRS(center, rotation, size))) {
				WireBox(new Bounds(new Vector3(0.5f, 0.5f, 0.5f), Vector3.one));
			}
		}

		/// <summary>
		/// Draws the outline of a box.
		///
		/// [Open online documentation to see images]
		/// </summary>
		public void WireBox (Bounds bounds) {
			var min = bounds.min;
			var max = bounds.max;

			Line(new float3(min.x, min.y, min.z), new float3(max.x, min.y, min.z));
			Line(new float3(max.x, min.y, min.z), new float3(max.x, min.y, max.z));
			Line(new float3(max.x, min.y, max.z), new float3(min.x, min.y, max.z));
			Line(new float3(min.x, min.y, max.z), new float3(min.x, min.y, min.z));

			Line(new float3(min.x, max.y, min.z), new float3(max.x, max.y, min.z));
			Line(new float3(max.x, max.y, min.z), new float3(max.x, max.y, max.z));
			Line(new float3(max.x, max.y, max.z), new float3(min.x, max.y, max.z));
			Line(new float3(min.x, max.y, max.z), new float3(min.x, max.y, min.z));

			Line(new float3(min.x, min.y, min.z), new float3(min.x, max.y, min.z));
			Line(new float3(max.x, min.y, min.z), new float3(max.x, max.y, min.z));
			Line(new float3(max.x, min.y, max.z), new float3(max.x, max.y, max.z));
			Line(new float3(min.x, min.y, max.z), new float3(min.x, max.y, max.z));
		}

		/// <summary>
		/// Draws a solid mesh with the given vertices.
		///
		/// Note: This method is not thread safe and must not be used from the Unity Job System.
		/// TODO: Are matrices handled?
		/// </summary>
		[BurstDiscard]
		public void SolidMesh (List<Vector3> vertices, List<int> triangles, List<Color> colors) {
			if (vertices.Count != colors.Count) throw new System.ArgumentException("Number of colors must be the same as the number of vertices");

			// TODO: Is this mesh getting recycled at all?
			var g = gizmos.Target as DrawingData;
			var mesh = g.GetMesh();

			// Set all data on the mesh
			mesh.SetVertices(vertices);
			mesh.SetTriangles(triangles, 0);
			mesh.SetColors(colors);
			// Upload all data
			mesh.UploadMeshData(false);
			g.data.Get(uniqueID).meshes.Add(mesh);
		}

		/// <summary>
		/// Draws a solid mesh with the given vertices.
		///
		/// Note: This method is not thread safe and must not be used from the Unity Job System.
		/// TODO: Are matrices handled?
		/// </summary>
		[BurstDiscard]
		public void SolidMesh (Vector3[] vertices, int[] triangles, Color[] colors, int vertexCount, int indexCount) {
			if (vertices.Length != colors.Length) throw new System.ArgumentException("Number of colors must be the same as the number of vertices");

			// TODO: Is this mesh getting recycled at all?
			var g = gizmos.Target as DrawingData;
			var mesh = g.GetMesh();

			// Set all data on the mesh
			mesh.SetVertices(vertices, 0, vertexCount);
			mesh.SetTriangles(triangles, 0, indexCount, 0);
			mesh.SetColors(colors, 0, vertexCount);
			// Upload all data
			mesh.UploadMeshData(false);
			g.data.Get(uniqueID).meshes.Add(mesh);
		}

		/// <summary>
		/// Draws a cross.
		///
		/// [Open online documentation to see images]
		/// </summary>
		public void Cross (float3 position, float size = 1) {
			size *= 0.5f;
			Line(position - new float3(size, 0, 0), position + new float3(size, 0, 0));
			Line(position - new float3(0, size, 0), position + new float3(0, size, 0));
			Line(position - new float3(0, 0, size), position + new float3(0, 0, size));
		}

		/// <summary>
		/// Draws a cross in the XZ plane.
		///
		/// [Open online documentation to see images]
		/// </summary>
		public void CrossXZ (float3 position, float size = 1) {
			size *= 0.5f;
			Line(position - new float3(size, 0, 0), position + new float3(size, 0, 0));
			Line(position - new float3(0, 0, size), position + new float3(0, 0, size));
		}

		/// <summary>
		/// Draws a cross in the XY plane.
		///
		/// [Open online documentation to see images]
		/// </summary>
		public void CrossXY (float3 position, float size = 1) {
			size *= 0.5f;
			Line(position - new float3(size, 0, 0), position + new float3(size, 0, 0));
			Line(position - new float3(0, size, 0), position + new float3(0, size, 0));
		}

		/// <summary>Returns a point on a cubic bezier curve. t is clamped between 0 and 1</summary>
		public static float3 EvaluateCubicBezier (float3 p0, float3 p1, float3 p2, float3 p3, float t) {
			t = math.clamp(t, 0, 1);
			float t2 = 1-t;
			return t2*t2*t2 * p0 + 3 * t2*t2 * t * p1 + 3 * t2 * t*t * p2 + t*t*t * p3;
		}

		/// <summary>
		/// Draws a cubic bezier curve.
		///
		/// [Open online documentation to see images]
		///
		/// [Open online documentation to see images]
		///
		/// TODO: Currently uses a fixed resolution of 20 segments. Resolution should depend on the distance to the camera.
		///
		/// See: https://en.wikipedia.org/wiki/Bezier_curve
		/// </summary>
		/// <param name="p0">Start point</param>
		/// <param name="p1">First control point</param>
		/// <param name="p2">Second control point</param>
		/// <param name="p3">End point</param>
		public void Bezier (float3 p0, float3 p1, float3 p2, float3 p3) {
			float3 prev = p0;

			for (int i = 1; i <= 20; i++) {
				float t = i/20.0f;
				float3 p = EvaluateCubicBezier(p0, p1, p2, p3, t);
				Line(prev, p);
				prev = p;
			}
		}

		/// <summary>
		/// Draws an arrow between two points.
		/// [Open online documentation to see images]
		///
		/// See: <see cref="ArrowheadArc"/>
		/// </summary>
		public void Arrow (float3 from, float3 to) {
			Arrow(from, to, new float3(0, 1, 0), 0.2f);
		}

		/// <summary>
		/// Draws an arrow between two points.
		///
		/// [Open online documentation to see images]
		///
		/// See: <see cref="ArrowheadArc"/>
		/// </summary>
		/// <param name="from">Start point of the arrow</param>
		/// <param name="to">End point of the arrow</param>
		/// <param name="up">Up direction of the world, the arrowhead plane will be as perpenicular as possible to this direction.</param>
		/// <param name="headFraction">The length of the arrowhead is the distance between from and to multiplied by this fraction. Should be between 0 and 1.</param>

		public void Arrow (float3 from, float3 to, float3 up, float headFraction) {
			Line(from, to);
			var dir = to - from;
			var normal = math.cross(dir, up);
			Line(to, to - (dir + normal) * headFraction);
			Line(to, to - (dir - normal) * headFraction);
		}

		/// <summary>
		/// Draws an arrowhead at a point.
		///
		/// <code>
		/// Draw.WireTriangle(Vector3.zero, Quaternion.identity, 0.5f, col);
		/// </code>
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="center">Center of the arrowhead.</param>
		/// <param name="direction">Direction the arrow is pointing.</param>
		/// <param name="radius">Distance from the center to each corner of the arrowhead.</param>
		public void Arrowhead (float3 center, float3 direction, float radius) {
			Arrowhead(center, direction, new float3(0, 1, 0), radius);
		}

		/// <summary>
		/// Draws an arrowhead at a point.
		///
		/// <code>
		/// Draw.WireTriangle(Vector3.zero, Quaternion.identity, 0.5f, col);
		/// </code>
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="center">Center of the arrowhead.</param>
		/// <param name="direction">Direction the arrow is pointing.</param>
		/// <param name="up">The arrowhead's surface normal will be aligned to the up direction.</param>
		/// <param name="radius">Distance from the center to each corner of the arrowhead.</param>
		public void Arrowhead (float3 center, float3 direction, float3 up, float radius) {
			if (math.all(direction == 0)) return;
			direction = math.normalizesafe(direction);
			var normal = math.cross(direction, up);
			const float SinPiOver3 = 0.866025f;
			const float CosPiOver3 = 0.5f;
			var circleCenter = center - radius * (1 - CosPiOver3)*0.5f * direction;
			var p1 = circleCenter + radius * direction;
			var p2 = circleCenter - radius * CosPiOver3 * direction + radius * SinPiOver3 * normal;
			var p3 = circleCenter - radius * CosPiOver3 * direction - radius * SinPiOver3 * normal;
			Line(p1, p2);
			Line(p2, circleCenter);
			Line(circleCenter, p3);
			Line(p3, p1);
		}

		/// <summary>
		/// Draws an arrowhead centered around a circle.
		///
		/// This can be used to for example show the direction a character is moving in.
		///
		/// [Open online documentation to see images]
		///
		/// Note: In the image above the arrowhead is the only part that is drawn by this method. The cylinder is only included for context.
		///
		/// See: <see cref="Arrow"/>
		/// </summary>
		/// <param name="origin">Point around which the arc is centered</param>
		/// <param name="direction">Direction the arrow is pointing</param>
		/// <param name="offset">Distance from origin that the arrow starts.</param>
		/// <param name="width">Width of the arrowhead in degrees (defaults to 60). Should be between 0 and 90.</param>
		public void ArrowheadArc (float3 origin, float3 direction, float offset, float width = 60) {
			if (!math.any(direction)) return;
			if (offset < 0) throw new System.ArgumentOutOfRangeException(nameof(offset));
			if (offset == 0) return;

			var up = new Vector3(0, 1, 0);
			var rot = Quaternion.LookRotation(direction, up);
			PushMatrix(Matrix4x4.TRS(origin, rot, Vector3.one));
			var a1 = math.PI * 0.5f - width * (0.5f * Mathf.Deg2Rad);
			var a2 = math.PI * 0.5f + width * (0.5f * Mathf.Deg2Rad);
			CircleXZ(float3.zero, offset, a1, a2);
			var p1 = new float3(math.cos(a1), 0, math.sin(a1)) * offset;
			var p2 = new float3(math.cos(a2), 0, math.sin(a2)) * offset;
			const float sqrt2 = 1.4142f;
			var p3 = new float3(0, 0, sqrt2 * offset);
			Line(p1, p3);
			Line(p2, p3);
			PopMatrix();
		}

		/// <summary>
		/// Draws a grid of lines.
		///
		/// <code>
		/// Draw.WireGrid(Vector3.zero, Quaternion.identity, new int2(3, 3), new float2(1, 1), col);
		/// </code>
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="center">Center of the grid</param>
		/// <param name="rotation">Rotation of the grid. The grid will be aligned to the X and Z axes of the rotation.</param>
		/// <param name="cells">Number of cells of the grid. Should be greater than 0.</param>
		/// <param name="totalSize">Total size of the grid along the X and Z axes.</param>
		public void WireGrid (float3 center, Quaternion rotation, int2 cells, float2 totalSize) {
			cells = math.max(cells, new int2(1, 1));
			PushMatrix(Matrix4x4.TRS(center, rotation, new Vector3(totalSize.x, 0, totalSize.y)));
			int w = cells.x;
			int h = cells.y;
			for (int i = 0; i <= w; i++) Line(new float3(i/(float)w - 0.5f, 0, -0.5f), new float3(i/(float)w - 0.5f, 0, 0.5f));
			for (int i = 0; i <= h; i++) Line(new float3(-0.5f, 0, i/(float)h - 0.5f), new float3(0.5f, 0, i/(float)h - 0.5f));
			PopMatrix();
		}

		/// <summary>
		/// Draws a triangle outline.
		///
		/// <code>
		/// Draw.WireTriangle(new Vector3(-0.5f, 0, 0), new Vector3(0, 1, 0), new Vector3(0.5f, 0, 0), Color.magenta);
		/// </code>
		/// [Open online documentation to see images]
		///
		/// See: <see cref="WireTriangle(float3,Quaternion,float)"/>
		/// See: <see cref="WirePolygon"/>
		/// </summary>
		/// <param name="a">First corner of the triangle</param>
		/// <param name="b">Second corner of the triangle</param>
		/// <param name="c">Third corner of the triangle</param>
		public void WireTriangle (float3 a, float3 b, float3 c) {
			Line(a, b);
			Line(b, c);
			Line(c, a);
		}

		/// <summary>
		/// Draws a rectangle outline.
		/// The rectangle will be aligned to the X and Z axes.
		///
		/// <code>
		/// Draw.WireRectangleXZ(new Vector3(0f, 0, 0), new Vector3(1, 1, 1), Color.magenta);
		/// </code>
		/// [Open online documentation to see images]
		///
		/// See: <see cref="WirePolygon"/>
		/// </summary>
		public void WireRectangleXZ (float3 center, float3 size) {
			WireRectangle(center, Quaternion.identity, size);
		}

		/// <summary>
		/// Draws a rectangle outline.
		/// The rectangle will be oriented along the rotation's X and Z axes.
		///
		/// <code>
		/// Draw.WireRectangle(new Vector3(0f, 0, 0), Quaternion.identity, new Vector3(1, 1, 1), Color.magenta);
		/// </code>
		/// [Open online documentation to see images]
		///
		/// See: <see cref="WirePolygon"/>
		/// </summary>
		public void WireRectangle (float3 center, Quaternion rotation, float3 size) {
			PushMatrix(Matrix4x4.TRS(center, rotation, size));
			Line(new float3(-0.5f, 0, -0.5f), new float3(+0.5f, 0, -0.5f));
			Line(new float3(+0.5f, 0, -0.5f), new float3(+0.5f, 0, +0.5f));
			Line(new float3(+0.5f, 0, +0.5f), new float3(-0.5f, 0, +0.5f));
			Line(new float3(-0.5f, 0, +0.5f), new float3(-0.5f, 0, -0.5f));
			PopMatrix();
		}

		/// <summary>
		/// Draws a rectangle outline.
		/// The rectangle corners are assumed to be in XY space.
		/// This is particularly useful when combined with <see cref="InScreenSpace"/>.
		///
		/// <code>
		/// using (Draw.InScreenSpace(Camera.main)) {
		///     Draw.WireRectangle(new Rect(10, 10, 100, 100), Color.magenta);
		/// }
		/// </code>
		/// [Open online documentation to see images]
		///
		/// See: <see cref="WireRectangleXZ"/>
		/// See: <see cref="WirePolygon"/>
		/// </summary>
		public void WireRectangle (Rect rect) {
			float2 min = rect.min;
			float2 max = rect.max;

			Line(new float3(min.x, min.y, 0), new float3(max.x, min.y, 0));
			Line(new float3(max.x, min.y, 0), new float3(max.x, max.y, 0));
			Line(new float3(max.x, max.y, 0), new float3(min.x, max.y, 0));
			Line(new float3(min.x, max.y, 0), new float3(min.x, min.y, 0));
		}

		/// <summary>
		/// Draws a triangle outline.
		///
		/// <code>
		/// Draw.WireTriangle(Vector3.zero, Quaternion.identity, 0.5f, col);
		/// </code>
		/// [Open online documentation to see images]
		///
		/// Note: This is a convenience wrapper for <see cref="WirePolygon(float3,int,Quaternion,float)"/>
		///
		/// See: <see cref="WireTriangle(float3,float3,float3)"/>
		/// </summary>
		/// <param name="center">Center of the triangle.</param>
		/// <param name="rotation">Rotation of the triangle. The first vertex will be radius units in front of center as seen from the rotation's point of view.</param>
		/// <param name="radius">Distance from the center to each vertex.</param>
		public void WireTriangle (float3 center, Quaternion rotation, float radius) {
			WirePolygon(center, 3, rotation, radius);
		}

		/// <summary>
		/// Draws a pentagon outline.
		///
		/// <code>
		/// Draw.WirePentagon(Vector3.zero, Quaternion.identity, 0.5f, col);
		/// </code>
		/// [Open online documentation to see images]
		///
		/// Note: This is a convenience wrapper for <see cref="WirePolygon(float3,int,Quaternion,float)"/>
		/// </summary>
		/// <param name="center">Center of the polygon.</param>
		/// <param name="rotation">Rotation of the polygon. The first vertex will be radius units in front of center as seen from the rotation's point of view.</param>
		/// <param name="radius">Distance from the center to each vertex.</param>
		public void WirePentagon (float3 center, Quaternion rotation, float radius) {
			WirePolygon(center, 5, rotation, radius);
		}

		/// <summary>
		/// Draws a hexagon outline.
		///
		/// <code>
		/// Draw.WireHexagon(Vector3.zero, Quaternion.identity, 0.5f, col);
		/// </code>
		/// [Open online documentation to see images]
		///
		/// Note: This is a convenience wrapper for <see cref="WirePolygon(float3,int,Quaternion,float)"/>
		/// </summary>
		/// <param name="center">Center of the polygon.</param>
		/// <param name="rotation">Rotation of the polygon. The first vertex will be radius units in front of center as seen from the rotation's point of view.</param>
		/// <param name="radius">Distance from the center to each vertex.</param>
		public void WireHexagon (float3 center, Quaternion rotation, float radius) {
			WirePolygon(center, 6, rotation, radius);
		}

		/// <summary>
		/// Draws a regular polygon outline.
		///
		/// <code>
		/// Draw.WirePolygon(new Vector3(-0.5f, 0, +0.5f), 3, Quaternion.identity, 0.4f, col);
		/// Draw.WirePolygon(new Vector3(+0.5f, 0, +0.5f), 4, Quaternion.identity, 0.4f, col);
		/// Draw.WirePolygon(new Vector3(-0.5f, 0, -0.5f), 5, Quaternion.identity, 0.4f, col);
		/// Draw.WirePolygon(new Vector3(+0.5f, 0, -0.5f), 6, Quaternion.identity, 0.4f, col);
		/// </code>
		/// [Open online documentation to see images]
		///
		/// See: <see cref="WireTriangle"/>
		/// See: <see cref="WirePentagon"/>
		/// See: <see cref="WireHexagon"/>
		/// </summary>
		/// <param name="center">Center of the polygon.</param>
		/// <param name="vertices">Number of corners (and sides) of the polygon.</param>
		/// <param name="rotation">Rotation of the polygon. The first vertex will be radius units in front of center as seen from the rotation's point of view.</param>
		/// <param name="radius">Distance from the center to each vertex.</param>
		public void WirePolygon (float3 center, int vertices, Quaternion rotation, float radius) {
			PushMatrix(Matrix4x4.TRS(center, rotation, Vector3.one * radius));
			float3 prev = new float3(0, 0, 1);
			for (int i = 1; i <= vertices; i++) {
				float a = 2 * math.PI * (i / (float)vertices);
				var p = new float3(math.sin(a), 0, math.cos(a));
				Line(prev, p);
				prev = p;
			}
			PopMatrix();
		}

		/// <summary>
		/// Draws a solid box.
		///
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="center">Center of the box</param>
		/// <param name="size">Width of the box along all dimensions</param>
		public void SolidBox (float3 center, float3 size) {
			Reserve<BoxData>();
			Add(Command.Box);
			Add(new BoxData { center = center, size = size });
		}

		/// <summary>
		/// Draws a solid box.
		///
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="bounds">Bounding box of the box</param>
		public void SolidBox (Bounds bounds) {
			SolidBox(bounds.center, bounds.size);
		}

		/// <summary>
		/// Draws a solid box.
		///
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="center">Center of the box</param>
		/// <param name="rotation">Rotation of the box</param>
		/// <param name="size">Width of the box along all dimensions</param>
		public void SolidBox (float3 center, Quaternion rotation, float3 size) {
			using (WithMatrix(Matrix4x4.TRS(center, rotation, size))) {
				SolidBox(new Vector3(0.5f, 0.5f, 0.5f), Vector3.one);
			}
		}

		/// <summary>
		/// Helper for determining how large a pixel is at a given depth.
		/// A a distance D from the camera a pixel corresponds to roughly value.x * D + value.y world units.
		/// Where value is the return value from this function.
		/// </summary>
		private static float2 CameraDepthToPixelSize (Camera camera) {
			if (camera.orthographic) {
				return new float2(0.0f, camera.orthographicSize / camera.pixelHeight);
			} else {
				return new float2(Mathf.Tan(camera.fieldOfView * Mathf.Deg2Rad * 0.5f) / (0.5f * camera.pixelHeight), 0.0f);
			}
		}

		private static NativeArray<T> ConvertExistingDataToNativeArray<T>(UnsafeAppendBuffer data) where T : struct {
			unsafe {
				var arr = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(data.Ptr, data.Length / UnsafeUtility.SizeOf<T>(), Allocator.Invalid);;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
				NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref arr, AtomicSafetyHandle.GetTempMemoryHandle());
#endif
				return arr;
			}
		}

		private static readonly VertexAttributeDescriptor[] MeshLayout = {
			new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
			new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
			new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4),
			new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
		};

		private static readonly CustomSampler samplerConvert = CustomSampler.Create("Convert");
		private static readonly CustomSampler samplerLayout = CustomSampler.Create("SetLayout");
		private static readonly CustomSampler samplerUpdate = CustomSampler.Create("Update");
		private static readonly CustomSampler samplerSubmesh = CustomSampler.Create("Submesh");

		private static void AssignMeshData (Mesh mesh, Bounds bounds, UnsafeAppendBuffer vertices, UnsafeAppendBuffer triangles) {
			samplerConvert.Begin();
			var verticesView = ConvertExistingDataToNativeArray<Builder.Vertex>(vertices);
			var trianglesView = ConvertExistingDataToNativeArray<int>(triangles);
			samplerConvert.End();

			samplerLayout.Begin();
			// Resize the vertex buffer if necessary
			if (mesh.vertexCount < verticesView.Length) mesh.SetVertexBufferParams(verticesView.Length, MeshLayout);
			mesh.SetIndexBufferParams(trianglesView.Length, IndexFormat.UInt32);
			samplerLayout.End();

			samplerUpdate.Begin();
			// Update the mesh data
			mesh.SetVertexBufferData(verticesView, 0, 0, verticesView.Length);
			mesh.SetIndexBufferData(trianglesView, 0, 0, trianglesView.Length);
			samplerUpdate.End();

			samplerSubmesh.Begin();
			mesh.subMeshCount = 1;
			var submesh = new SubMeshDescriptor(0, trianglesView.Length, MeshTopology.Triangles) {
				vertexCount = verticesView.Length,
				bounds = bounds
			};
			mesh.SetSubMesh(0, submesh, MeshUpdateFlags.DontRecalculateBounds);
			mesh.bounds = bounds;
			samplerSubmesh.End();
		}

		internal static unsafe JobHandle Build (DrawingData gizmos, ProcessedBuilderData.MeshBuffers* buffers, Camera camera, JobHandle dependency) {
			return new Builder {
					   buffers = &buffers->splitterOutput,
					   outlineVertices = &buffers->vertices,
					   outlineTriangles = &buffers->triangles,
					   solidVertices = &buffers->solidVertices,
					   solidTriangles = &buffers->solidTriangles,
					   currentMatrix = Matrix4x4.identity,
					   currentColor = (Color32)Color.white,
					   outBounds = &buffers->bounds,
					   cameraPosition = camera != null ? camera.transform.position : Vector3.zero,
					   cameraDepthToPixelSize = camera != null? CameraDepthToPixelSize(camera) : 0,
			}.Schedule(dependency);
		}

		internal static unsafe void BuildMesh (DrawingData gizmos, List<MeshWithType> meshes, ProcessedBuilderData.MeshBuffers* inputBuffers) {
			if (inputBuffers->triangles.Length > 0) {
				var mesh = gizmos.GetMesh();
				AssignMeshData(mesh, inputBuffers->bounds, inputBuffers->vertices, inputBuffers->triangles);
				meshes.Add(new MeshWithType { mesh = mesh, lines = true });
			}

			if (inputBuffers->solidTriangles.Length > 0) {
				var mesh = gizmos.GetMesh();
				AssignMeshData(mesh, inputBuffers->bounds, inputBuffers->solidVertices, inputBuffers->solidTriangles);
				meshes.Add(new MeshWithType { mesh = mesh, lines = false });
			}
		}

		[BurstCompile]
		internal struct PersistentFilterJob : IJob {
			[NativeDisableUnsafePtrRestriction]
			public unsafe UnsafeAppendBuffer* buffer;
			public float time;

			public void Execute () {
				var stackPersist = new NativeArray<bool>(Builder.MaxStackSize, Allocator.Temp, NativeArrayOptions.ClearMemory);

				// Size of all commands in bytes
				var commandSizes = new NativeArray<int>(20, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

				commandSizes[(int)Command.PushColor] = UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<Color32>();
				commandSizes[(int)Command.PopColor] = UnsafeUtility.SizeOf<Command>() + 0;
				commandSizes[(int)Command.PushMatrix] = UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<float4x4>();
				commandSizes[(int)Command.PushSetMatrix] = UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<float4x4>();
				commandSizes[(int)Command.PopMatrix] = UnsafeUtility.SizeOf<Command>() + 0;
				commandSizes[(int)Command.Line] = UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<LineData>();
				commandSizes[(int)Command.CircleXZ] = UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<CircleXZData>();
				commandSizes[(int)Command.Circle] = UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<CircleData>();
				commandSizes[(int)Command.Box] = UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<BoxData>();
				commandSizes[(int)Command.PushPersist] = UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<PersistData>();
				commandSizes[(int)Command.PopPersist] = UnsafeUtility.SizeOf<Command>();

				unsafe {
					// Store in local variables for performance (makes it possible to use registers for a lot of fields)
					var bufferPersist = *buffer;

					long writeOffset = 0;
					long readOffset = 0;
					bool shouldWrite = false;
					int stackSize = 0;

					while (readOffset < bufferPersist.Length) {
						var cmd = *(Command*)((byte*)bufferPersist.Ptr + readOffset);

						int size = commandSizes[(int)cmd & 0xFF] + ((cmd & Command.PushColorInline) != 0 ? UnsafeUtility.SizeOf<Color32>() : 0);

						if (cmd == Command.PushPersist) {
							// Very pretty way of reading the PersistData struct right after the command label
							var data = *(PersistData*)(((Command*)(byte*)bufferPersist.Ptr + readOffset) + 1);

							stackPersist[stackSize] = shouldWrite;
							stackSize++;
							if (stackSize >= stackPersist.Length) throw new System.Exception("Too many push commands");
							// Scopes only survive if this condition is true
							shouldWrite = time <= data.endTime;
						}

						if (shouldWrite) {
							if (writeOffset != readOffset) {
								// We need to use memmove instead of memcpy because the source and destination regions may overlap
								UnsafeUtility.MemMove((byte*)bufferPersist.Ptr + writeOffset, (byte*)bufferPersist.Ptr + readOffset, size);
							}
							writeOffset += size;
						}
						readOffset += size;

						if (cmd == Command.PopPersist) {
							stackSize--;
							if (stackSize < 0) throw new System.Exception("Too many pop commands");
							shouldWrite = stackPersist[stackSize];
						}
					}

					bufferPersist.Length = (int)writeOffset;
					if (stackSize != 0) throw new System.Exception("Inconsistent push/pop commands. Are your push and pop commands properly matched?");
					*buffer = bufferPersist;
				}
			}
		}

		[BurstCompile]
		internal struct StreamSplitter : IJob {
			public NativeArray<UnsafeAppendBuffer> inputBuffers;
			[NativeDisableUnsafePtrRestriction]
			public unsafe UnsafeAppendBuffer* staticBuffer, dynamicBuffer, persistentBuffer;

			static readonly int PushCommands = (1 << (int)Command.PushColor) | (1 << (int)Command.PushMatrix) | (1 << (int)Command.PushSetMatrix) | (1 << (int)Command.PushPersist);
			static readonly int PopCommands = (1 << (int)Command.PopColor) | (1 << (int)Command.PopMatrix) | (1 << (int)Command.PopPersist);
			static readonly int MetaCommands = PushCommands | PopCommands;
			static readonly int DynamicCommands = (1 << (int)Command.CircleXZ) | (1 << (int)Command.Circle) | MetaCommands;
			static readonly int StaticCommands = (1 << (int)Command.Line) | (1 << (int)Command.Box) | MetaCommands;

			public void Execute () {
				var lastWriteStatic = -1;
				var lastWriteDynamic = -1;
				var lastWritePersist = -1;
				var stackStatic = new NativeArray<int>(Builder.MaxStackSize, Allocator.Temp, NativeArrayOptions.ClearMemory);
				var stackDynamic = new NativeArray<int>(Builder.MaxStackSize, Allocator.Temp, NativeArrayOptions.ClearMemory);
				var stackPersist = new NativeArray<int>(Builder.MaxStackSize, Allocator.Temp, NativeArrayOptions.ClearMemory);

				// Size of all commands in bytes
				var commandSizes = new NativeArray<int>(20, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

				commandSizes[(int)Command.PushColor] = UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<Color32>();
				commandSizes[(int)Command.PopColor] = UnsafeUtility.SizeOf<Command>() + 0;
				commandSizes[(int)Command.PushMatrix] = UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<float4x4>();
				commandSizes[(int)Command.PushSetMatrix] = UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<float4x4>();
				commandSizes[(int)Command.PopMatrix] = UnsafeUtility.SizeOf<Command>() + 0;
				commandSizes[(int)Command.Line] = UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<LineData>();
				commandSizes[(int)Command.CircleXZ] = UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<CircleXZData>();
				commandSizes[(int)Command.Circle] = UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<CircleData>();
				commandSizes[(int)Command.Box] = UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<BoxData>();
				commandSizes[(int)Command.PushPersist] = UnsafeUtility.SizeOf<Command>() + UnsafeUtility.SizeOf<PersistData>();
				commandSizes[(int)Command.PopPersist] = UnsafeUtility.SizeOf<Command>();

				unsafe {
					// Store in local variables for performance (makes it possible to use registers for a lot of fields)
					var bufferStatic = *staticBuffer;
					var bufferDynamic = *dynamicBuffer;
					var bufferPersist = *persistentBuffer;

					bufferStatic.Reset();
					bufferDynamic.Reset();
					bufferPersist.Reset();

					for (int i = 0; i < inputBuffers.Length; i++) {
						int stackSize = 0;
						int persist = 0;
						var reader = inputBuffers[i].AsReader();

						// Guarantee we have enough space for copying the whole buffer
						if (bufferStatic.Capacity < bufferStatic.Length + reader.Size) bufferStatic.SetCapacity(math.ceilpow2(bufferStatic.Length + reader.Size));
						if (bufferDynamic.Capacity < bufferDynamic.Length + reader.Size) bufferDynamic.SetCapacity(math.ceilpow2(bufferDynamic.Length + reader.Size));
						if (bufferPersist.Capacity < bufferPersist.Length + reader.Size) bufferPersist.SetCapacity(math.ceilpow2(bufferPersist.Length + reader.Size));

						// To ensure that even if exceptions are thrown the output buffers still point to valid memory regions
						*staticBuffer = bufferStatic;
						*dynamicBuffer = bufferDynamic;
						*persistentBuffer = bufferPersist;

						while (reader.Offset < reader.Size) {
							var cmd = *(Command*)((byte*)reader.Ptr + reader.Offset);
							var cmdBit = 1 << ((int)cmd & 0xFF);
							int size = commandSizes[(int)cmd & 0xFF] + ((cmd & Command.PushColorInline) != 0 ? UnsafeUtility.SizeOf<Color32>() : 0);
							bool isMeta = (cmdBit & MetaCommands) != 0;

							if ((cmdBit & DynamicCommands) != 0 && persist == 0) {
								if (!isMeta) lastWriteDynamic = bufferDynamic.Length;
								UnsafeUtility.MemCpy((byte*)bufferDynamic.Ptr + bufferDynamic.Length, (byte*)reader.Ptr + reader.Offset, size);
								bufferDynamic.Length += size;
							}

							if ((cmdBit & StaticCommands) != 0 && persist == 0) {
								if (!isMeta) lastWriteStatic = bufferStatic.Length;
								UnsafeUtility.MemCpy((byte*)bufferStatic.Ptr + bufferStatic.Length, (byte*)reader.Ptr + reader.Offset, size);
								bufferStatic.Length += size;
							}

							if ((cmdBit & MetaCommands) != 0 || persist > 0) {
								if (persist > 0) lastWritePersist = bufferPersist.Length;
								UnsafeUtility.MemCpy((byte*)bufferPersist.Ptr + bufferPersist.Length, (byte*)reader.Ptr + reader.Offset, size);
								bufferPersist.Length += size;
							}

							if ((cmdBit & PushCommands) != 0) {
								stackStatic[stackSize] = bufferStatic.Length - size;
								stackDynamic[stackSize] = bufferDynamic.Length - size;
								stackPersist[stackSize] = bufferPersist.Length - size;
								stackSize++;
								if ((cmd & (Command)0xFF) == Command.PushPersist) {
									persist++;
								}
								if (stackSize >= stackStatic.Length) throw new System.Exception("Push commands are too deeply nested. This can happen if you have deeply nested WithMatrix or WithColor scopes.");
							} else if ((cmdBit & PopCommands) != 0) {
								stackSize--;
								if (stackSize < 0) throw new System.Exception("Trying to issue a pop command but there is no corresponding push command");
								if (lastWriteStatic < stackStatic[stackSize]) {
									bufferStatic.Length = stackStatic[stackSize];
								}
								if (lastWriteDynamic < stackDynamic[stackSize]) {
									bufferDynamic.Length = stackDynamic[stackSize];
								}
								if (lastWritePersist < stackPersist[stackSize]) {
									bufferPersist.Length = stackPersist[stackSize];
								}
								if ((cmd & (Command)0xFF) == Command.PopPersist) {
									persist--;
									if (persist < 0) throw new System.Exception("Too many PopPersist commands. Are your PushPersist/PopPersist calls matched?");
								}
							}

							reader.Offset += size;
						}

						if (stackSize != 0) throw new System.Exception("Too few pop commands and too many push commands. Are your push and pop commands properly matched?");
						if (reader.Offset != reader.Size) throw new System.Exception("Did not end up at the end of the buffer. This is a bug.");
					}

					if (bufferStatic.Length > bufferStatic.Capacity) throw new System.Exception("Buffer overrun. This is a bug");
					if (bufferDynamic.Length > bufferDynamic.Capacity) throw new System.Exception("Buffer overrun. This is a bug");
					if (bufferPersist.Length > bufferPersist.Capacity) throw new System.Exception("Buffer overrun. This is a bug");

					*staticBuffer = bufferStatic;
					*dynamicBuffer = bufferDynamic;
					*persistentBuffer = bufferPersist;
				}
			}
		}

		[BurstCompile(FloatMode = FloatMode.Fast)]
		internal struct Builder : IJob {
			[NativeDisableUnsafePtrRestriction]
			public unsafe UnsafeAppendBuffer* buffers;
			public Color32 currentColor;
			public float4x4 currentMatrix;
			float3 minBounds;
			float3 maxBounds;
			public float3 cameraPosition;
			public float2 cameraDepthToPixelSize;

			[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
			public struct Vertex {
				public float3 position;
				public float3 uv2;
				public Color32 color;
				public float2 uv;
			}

			[NativeDisableUnsafePtrRestriction]
			public unsafe Bounds* outBounds;

			[NativeDisableUnsafePtrRestriction]
			public unsafe UnsafeAppendBuffer* outlineVertices;

			[NativeDisableUnsafePtrRestriction]
			public unsafe UnsafeAppendBuffer* outlineTriangles;

			[NativeDisableUnsafePtrRestriction]
			public unsafe UnsafeAppendBuffer* solidVertices;

			[NativeDisableUnsafePtrRestriction]
			public unsafe UnsafeAppendBuffer* solidTriangles;

			static unsafe void Add<T>(UnsafeAppendBuffer* buffer, T value) where T : struct {
				int num = UnsafeUtility.SizeOf<T>();

				UnsafeUtility.WriteArrayElement((byte*)buffer->Ptr + buffer->Length, 0, value);
				buffer->Length += num;
			}

			static unsafe void Reserve (UnsafeAppendBuffer* buffer, int size) {
				var newSize = buffer->Length + size;

				if (newSize > buffer->Capacity) {
					buffer->SetCapacity(math.max(newSize, buffer->Capacity * 2));
				}
			}

			static float3 PerspectiveDivide (float4 p) {
				return p.xyz / p.w;
			}

			void AddLine (LineData line) {
				// Store the line direction in the vertex.
				// A line consists of 4 vertices. The line direction will be used to
				// offset the vertices to create a line with a fixed pixel thickness
				var a = PerspectiveDivide(math.mul(currentMatrix, new float4(line.a, 1.0f)));
				var b = PerspectiveDivide(math.mul(currentMatrix, new float4(line.b, 1.0f)));
				var lineDir = b - a;

				if (math.any(math.isnan(lineDir))) throw new Exception("Nan line coordinates");

				// Update the bounding box
				minBounds = math.min(minBounds, math.min(a, b));
				maxBounds = math.max(maxBounds, math.max(a, b));

				unsafe {
					// Make sure there is enough allocated capacity for 4 more vertices
					Reserve(outlineVertices, 4 * UnsafeUtility.SizeOf<Vertex>());

					// Insert 4 vertices
					// Doing it with pointers is faster, and this is the hottest
					// code of the whole gizmo drawing process.
					var ptr = (Vertex*)((byte*)outlineVertices->Ptr + outlineVertices->Length);
					outlineVertices->Length += 4 * UnsafeUtility.SizeOf<Vertex>();
					*ptr++ = new Vertex {
						position = a,
						color = currentColor,
						uv = new Vector2(0, 0),
						uv2 = lineDir,
					};
					*ptr++ = new Vertex {
						position = a,
						color = currentColor,
						uv = new Vector2(1, 0),
						uv2 = lineDir,
					};

					*ptr++ = new Vertex {
						position = b,
						color = currentColor,
						uv = new Vector2(0, 1),
						uv2 = lineDir,
					};
					*ptr++ = new Vertex {
						position = b,
						color = currentColor,
						uv = new Vector2(1, 1),
						uv2 = lineDir,
					};
				}
			}

			/// <summary>Calculate number of steps to use for drawing a circle at the specified point and radius to get less than the specified pixel error.</summary>
			int CircleSteps (float3 center, float radius, float maxPixelError) {
				var cc = PerspectiveDivide(math.mul(currentMatrix, new float4(center, 1.0f)));
				// TODO: Can improve performance
				var realWorldRadius = math.length(PerspectiveDivide(math.mul(currentMatrix, new float4(center, 1.0f) + new float4(radius, 0, 0, 0))) - cc);
				var distance = math.length(cc - cameraPosition);

				var pixelSize = cameraDepthToPixelSize.x * distance + cameraDepthToPixelSize.y;
				var cosAngle = 1 - (maxPixelError * pixelSize) / realWorldRadius;
				int steps = cosAngle < 0 ? 3 : (int)math.ceil(math.PI / (math.acos(cosAngle)));

				return steps;
			}

			void AddCircle (CircleData circle) {
				// If the circle has a zero normal then just ignore it
				if (math.all(circle.normal == 0)) return;

				const float MaxPixelError = 0.5f;
				var steps = CircleSteps(circle.center, circle.radius, MaxPixelError);

				// Round up to nearest multiple of 3 (required for the SIMD to work)
				steps = ((steps + 2) / 3) * 3;

				circle.normal = math.normalize(circle.normal);
				float3 tangent1;
				if (math.all(math.abs(circle.normal - new float3(0, 1, 0)) < 0.001f)) {
					// The normal was (almost) identical to (0, 1, 0)
					tangent1 = new float3(0, 0, 1);
				} else {
					// Common case
					tangent1 = math.cross(circle.normal, new float3(0, 1, 0));
				}

				var oldMatrix = currentMatrix;
				currentMatrix = math.mul(currentMatrix, Matrix4x4.TRS(circle.center, Quaternion.LookRotation(circle.normal, tangent1), new Vector3(circle.radius, circle.radius, circle.radius)));

				float invSteps = 1.0f / steps;
				for (int i = 0; i < steps; i += 3) {
					var i4 = math.lerp(0, 2*Mathf.PI, new float4(i, i + 1, i + 2, i + 3) * invSteps);
					// Calculate 4 sines and cosines at the same time using SIMD
					math.sincos(i4, out float4 sin, out float4 cos);
					AddLine(new LineData { a = new float3(cos.x, sin.x, 0), b = new float3(cos.y, sin.y, 0) });
					AddLine(new LineData { a = new float3(cos.y, sin.y, 0), b = new float3(cos.z, sin.z, 0) });
					AddLine(new LineData { a = new float3(cos.z, sin.z, 0), b = new float3(cos.w, sin.w, 0) });
				}

				currentMatrix = oldMatrix;
			}

			void AddCircle (CircleXZData circle) {
				const float MaxPixelError = 0.5f;
				var steps = CircleSteps(circle.center, circle.radius, MaxPixelError);

				// Round up to nearest multiple of 3 (required for the SIMD to work)
				steps = ((steps + 2) / 3) * 3;

				while (circle.startAngle > circle.endAngle) circle.startAngle -= 2 * Mathf.PI;
				var oldMatrix = currentMatrix;
				currentMatrix = math.mul(currentMatrix, Matrix4x4.Translate(circle.center) * Matrix4x4.Scale(new Vector3(circle.radius, circle.radius, circle.radius)));

				float invSteps = 1.0f / steps;
				for (int i = 0; i < steps; i += 3) {
					var i4 = math.lerp(circle.startAngle, circle.endAngle, new float4(i, i + 1, i + 2, i + 3) * invSteps);
					// Calculate 4 sines and cosines at the same time using SIMD
					math.sincos(i4, out float4 sin, out float4 cos);
					AddLine(new LineData { a = new float3(cos.x, 0, sin.x), b = new float3(cos.y, 0, sin.y) });
					AddLine(new LineData { a = new float3(cos.y, 0, sin.y), b = new float3(cos.z, 0, sin.z) });
					AddLine(new LineData { a = new float3(cos.z, 0, sin.z), b = new float3(cos.w, 0, sin.w) });
				}

				currentMatrix = oldMatrix;
			}

			static readonly float4[] BoxVertices = {
				new float4(-1, -1, -1, 1),
				new float4(-1, -1, +1, 1),
				new float4(-1, +1, -1, 1),
				new float4(-1, +1, +1, 1),
				new float4(+1, -1, -1, 1),
				new float4(+1, -1, +1, 1),
				new float4(+1, +1, -1, 1),
				new float4(+1, +1, +1, 1),
			};

			static readonly int[] BoxTriangles = {
				// Bottom two triangles
				0, 1, 5,
				0, 5, 4,

				// Top
				7, 3, 2,
				7, 2, 6,

				// -X
				0, 1, 3,
				0, 3, 2,

				// +X
				4, 5, 7,
				4, 7, 6,

				// +Z
				1, 3, 7,
				1, 7, 5,

				// -Z
				0, 2, 6,
				0, 6, 4,
			};

			void AddBox (BoxData box) {
				unsafe {
					Reserve(solidVertices, BoxVertices.Length * UnsafeUtility.SizeOf<Vertex>());
					Reserve(solidTriangles, BoxTriangles.Length * UnsafeUtility.SizeOf<int>());

					var matrix = math.mul(currentMatrix, Matrix4x4.Translate(box.center) * Matrix4x4.Scale(box.size * 0.5f));

					var mn = minBounds;
					var mx = maxBounds;
					int vertexCount = solidVertices->Length / UnsafeUtility.SizeOf<Vertex>();
					for (int i = 0; i < BoxVertices.Length; i++) {
						var p = PerspectiveDivide(math.mul(matrix, BoxVertices[i]));
						// Update the bounding box
						mn = math.min(mn, p);
						mx = math.max(mx, p);

						Add(solidVertices, new Vertex {
							position = p,
							color = currentColor,
							uv = new float2(0, 0),
							uv2 = new float3(0, 0, 0),
						});
					}

					minBounds = mn;
					maxBounds = mx;

					for (int i = 0; i < BoxTriangles.Length; i++) {
						Add(solidTriangles, vertexCount + BoxTriangles[i]);
					}
				}
			}

			public void Next (ref UnsafeAppendBuffer.Reader reader, ref NativeArray<float4x4> matrixStack, ref NativeArray<Color32> colorStack, ref int matrixStackSize, ref int colorStackSize) {
				var fullCmd = reader.ReadNext<Command>();
				var cmd = fullCmd & (Command)0xFF;
				Color32 oldColor = default;

				if ((fullCmd & Command.PushColorInline) != 0) {
					oldColor = currentColor;
					currentColor = reader.ReadNext<Color32>();
				}

				switch (cmd) {
				case Command.PushColor:
					if (colorStackSize >= colorStack.Length) throw new System.Exception("Too deeply nested PushColor calls");
					colorStack[colorStackSize] = currentColor;
					colorStackSize++;
					currentColor = reader.ReadNext<Color32>();
					break;
				case Command.PopColor:
					if (colorStackSize <= 0) throw new System.Exception("PushColor and PopColor are not matched");
					colorStackSize--;
					currentColor = colorStack[colorStackSize];
					break;
				case Command.PushMatrix:
					if (matrixStackSize >= matrixStack.Length) throw new System.Exception("Too deeply nested PushMatrix calls");
					matrixStack[matrixStackSize] = currentMatrix;
					matrixStackSize++;
					currentMatrix = math.mul(currentMatrix, reader.ReadNext<float4x4>());
					break;
				case Command.PushSetMatrix:
					if (matrixStackSize >= matrixStack.Length) throw new System.Exception("Too deeply nested PushMatrix calls");
					matrixStack[matrixStackSize] = currentMatrix;
					matrixStackSize++;
					currentMatrix = reader.ReadNext<float4x4>();
					break;
				case Command.PopMatrix:
					if (matrixStackSize <= 0) throw new System.Exception("PushMatrix and PopMatrix are not matched");
					matrixStackSize--;
					currentMatrix = matrixStack[matrixStackSize];
					break;
				case Command.Line:
					AddLine(reader.ReadNext<LineData>());
					break;
				case Command.CircleXZ:
					AddCircle(reader.ReadNext<CircleXZData>());
					break;
				case Command.Circle:
					AddCircle(reader.ReadNext<CircleData>());
					break;
				case Command.Box:
					AddBox(reader.ReadNext<BoxData>());
					break;
				case Command.PushPersist:
					// This command does not need to be handled by the builder
					reader.ReadNext<PersistData>();
					break;
				case Command.PopPersist:
					// This command does not need to be handled by the builder
					break;
				default:
					throw new System.Exception("Unknown command");
				}

				if ((fullCmd & Command.PushColorInline) != 0) {
					currentColor = oldColor;
				}
			}

			void CreateTriangles () {
				// Create triangles for all lines
				// A triangle consists of 3 indices
				// A line (4 vertices) consists of 2 triangles, so 6 triangle indices
				unsafe {
					var vertexCount = outlineVertices->Length / UnsafeUtility.SizeOf<Vertex>();
					// Each line is made out of 4 vertices
					var lineCount = vertexCount / 4;
					var trianglesSizeInBytes = lineCount * 6 * UnsafeUtility.SizeOf<int>();
					if (trianglesSizeInBytes >= outlineTriangles->Capacity) {
						outlineTriangles->SetCapacity(math.ceilpow2(trianglesSizeInBytes));
					}

					int* ptr = (int*)outlineTriangles->Ptr;
					for (int i = 0, vi = 0; i < lineCount; i++, vi += 4) {
						// First triangle
						*ptr++ = vi + 0;
						*ptr++ = vi + 1;
						*ptr++ = vi + 2;

						// Second triangle
						*ptr++ = vi + 1;
						*ptr++ = vi + 3;
						*ptr++ = vi + 2;
					}
					outlineTriangles->Length = trianglesSizeInBytes;
				}
			}

			public const int MaxStackSize = 32;

			public void Execute () {
				unsafe {
					outlineVertices->Reset();
					outlineTriangles->Reset();
					solidVertices->Reset();
					solidTriangles->Reset();
				}

				minBounds = new float3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
				maxBounds = new float3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

				var matrixStack = new NativeArray<float4x4>(MaxStackSize, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
				var colorStack = new NativeArray<Color32>(MaxStackSize, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
				int matrixStackSize = 0;
				int colorStackSize = 0;

				unsafe {
					var reader = (*buffers).AsReader();
					while (reader.Offset < reader.Size) Next(ref reader, ref matrixStack, ref colorStack, ref matrixStackSize, ref colorStackSize);
					if (reader.Offset != reader.Size) throw new Exception("Didn't reach the end of the buffer");
				}

				CreateTriangles();

				unsafe {
					*outBounds = new Bounds((minBounds + maxBounds) * 0.5f, maxBounds - minBounds);

					if (math.any(math.isnan(outBounds->min)) && (outlineVertices->Length > 0 || solidVertices->Length > 0)) throw new Exception("NaN bounds. A Draw.* command may have been given NaN coordinates.");
				}
			}
		}
	}
}
