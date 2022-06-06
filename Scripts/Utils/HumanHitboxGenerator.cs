#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace MultiplayerARPG
{
	public class HumanHitboxGenerator : MonoBehaviour
	{
#if UNITY_EDITOR
		[Header("Bones Transform")]
		public Animator targetAnimator;
		public Transform root;
		public Transform hips;
		public Transform spine;
		public Transform chest;
		public Transform head;
		public Transform leftUpperLeg;
		public Transform leftLowerLeg;
		public Transform leftFoot;
		public Transform rightUpperLeg;
		public Transform rightLowerLeg;
		public Transform rightFoot;
		public Transform leftUpperArm;
		public Transform leftLowerArm;
		public Transform leftHand;
		public Transform rightUpperArm;
		public Transform rightLowerArm;
		public Transform rightHand;

		[Header("Component Field Tools")]
		[InspectorButton(nameof(FillBoneTransforms))]
		public bool fillBoneTransforms;

		[Header("Hitboxes Tools")]
		[InspectorButton(nameof(DestroyAllHitBoxComponents))]
		public bool destroyAllHitBoxComponents;
		[InspectorButton(nameof(DestroyAllHitBoxGameObjects))]
		public bool destroyAllHitBoxGameObjects;
		[InspectorButton(nameof(CreateHitBoxes))]
		public bool createHitBoxes;

		public void FillBoneTransforms()
		{
			if (targetAnimator == null)
			{
				EditorUtility.DisplayDialog("Error", "You have to choose target animator", "OK");
				return;
			}

			if (!targetAnimator.isHuman)
			{
				EditorUtility.DisplayDialog("Error", "Target animator must be humanoid", "OK");
				return;
			}

			root = targetAnimator.transform;
			hips = targetAnimator.GetBoneTransform(HumanBodyBones.Hips);
			spine = targetAnimator.GetBoneTransform(HumanBodyBones.Spine);
			chest = targetAnimator.GetBoneTransform(HumanBodyBones.Chest);
			head = targetAnimator.GetBoneTransform(HumanBodyBones.Head);
			leftUpperArm = targetAnimator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
			leftLowerArm = targetAnimator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
			leftHand = targetAnimator.GetBoneTransform(HumanBodyBones.LeftHand);
			rightUpperArm = targetAnimator.GetBoneTransform(HumanBodyBones.RightUpperArm);
			rightLowerArm = targetAnimator.GetBoneTransform(HumanBodyBones.RightLowerArm);
			rightHand = targetAnimator.GetBoneTransform(HumanBodyBones.RightHand);
			leftUpperLeg = targetAnimator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
			leftLowerLeg = targetAnimator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
			leftFoot = targetAnimator.GetBoneTransform(HumanBodyBones.LeftFoot);
			rightUpperLeg = targetAnimator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
			rightLowerLeg = targetAnimator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
			rightFoot = targetAnimator.GetBoneTransform(HumanBodyBones.RightFoot);
		}

		public void DestroyAllHitBoxComponents()
		{
			if (root == null)
			{
				EditorUtility.DisplayDialog("Error", "No root transform assigned", "OK");
				return;
			}

			DamageableHitBox[] hitboxes = root.GetComponentsInChildren<DamageableHitBox>();
			int count = hitboxes.Length;
			for (int i = hitboxes.Length - 1; i >= 0; --i)
			{
				GameObject hitBoxesObj = hitboxes[i].gameObject;
				Rigidbody rb = hitBoxesObj.GetComponent<Rigidbody>();
				if (rb != null)
					DestroyImmediate(rb);
				Rigidbody2D rb2 = hitBoxesObj.GetComponent<Rigidbody2D>();
				if (rb2 != null)
					DestroyImmediate(rb2);
				Collider col = hitBoxesObj.GetComponent<Collider>();
				if (col != null)
					DestroyImmediate(col);
				Collider2D col2 = hitBoxesObj.GetComponent<Collider2D>();
				if (col2 != null)
					DestroyImmediate(col2);
				DestroyImmediate(hitboxes[i]);
			}
			EditorUtility.DisplayDialog("Message", $"{count} Destroyed", "OK");
		}

		public void DestroyAllHitBoxGameObjects()
		{
			if (root == null)
			{
				EditorUtility.DisplayDialog("Error", "No root transform assigned", "OK");
				return;
			}

			DamageableHitBox[] hitboxes = root.GetComponentsInChildren<DamageableHitBox>();
			int count = hitboxes.Length;
			for (int i = hitboxes.Length - 1; i >= 0; --i)
			{
				DestroyImmediate(hitboxes[i].gameObject);
			}
			EditorUtility.DisplayDialog("Message", $"{count} Destroyed", "OK");
		}

		public void CreateHitBoxes()
		{
			if (root == null)
			{
				EditorUtility.DisplayDialog("Error", "No root transform assigned", "OK");
				return;
			}

			// Create hitboxes by uses distance between bones
			Vector3 upperArmToHeadCentroid = GetUpperArmToHeadCentroid();

			Vector3 shoulderDirection = rightUpperArm.position - leftUpperArm.position;
			float torsoWidth = shoulderDirection.magnitude;
			float torsoProportionAspect = 0.6f;

			// Hips
			Vector3 hipsStartPoint = hips.position;

			// Making sure the hip bone is not at the feet
			float toHead = Vector3.Distance(head.position, root.position);
			float toHips = Vector3.Distance(hips.position, root.position);

			if (toHips < toHead * 0.2f)
			{
				hipsStartPoint = Vector3.Lerp(leftUpperLeg.position, rightUpperLeg.position, 0.5f);
			}

			Vector3 lastEndPoint = spine.position;
			hipsStartPoint += (hipsStartPoint - upperArmToHeadCentroid) * 0.1f;
			float hipsWidth = torsoWidth * 0.8f;
			CreateHitBox(hips, hipsStartPoint, lastEndPoint, hipsWidth, torsoProportionAspect, shoulderDirection);

			// Spine
			Vector3 spineStartPoint = lastEndPoint;
			lastEndPoint = chest.position;
			float spineWidth = torsoWidth * 0.75f;
			CreateHitBox(spine, spineStartPoint, lastEndPoint, spineWidth, torsoProportionAspect, shoulderDirection);

			// Chest
			Vector3 chestStartPoint = lastEndPoint;
			lastEndPoint = upperArmToHeadCentroid;
			CreateHitBox(chest, chestStartPoint, lastEndPoint, torsoWidth, torsoProportionAspect, shoulderDirection);

			// Head
			Vector3 headStartPoint = lastEndPoint;
			Vector3 headEndPoint = headStartPoint + (headStartPoint - hipsStartPoint) * 0.45f;
			Vector3 axis = head.TransformVector(GetAxisVectorToDirection(head, headEndPoint - headStartPoint));
			headEndPoint = headStartPoint + Vector3.Project(headEndPoint - headStartPoint, axis).normalized * (headEndPoint - headStartPoint).magnitude;
			CreateHitBox(head, headStartPoint, headEndPoint, Vector3.Distance(headStartPoint, headEndPoint) * 0.8f);

			// Arms
			float armWidthAspect = 0.4f;

			float leftArmWidth = Vector3.Distance(leftUpperArm.position, leftLowerArm.position) * armWidthAspect;
			CreateHitBox(leftUpperArm, leftUpperArm.position, leftLowerArm.position, leftArmWidth);
			CreateHitBox(leftLowerArm, leftLowerArm.position, leftHand.position, leftArmWidth * 0.9f);

			float rightArmWidth = Vector3.Distance(rightUpperArm.position, rightLowerArm.position) * armWidthAspect;
			CreateHitBox(rightUpperArm, rightUpperArm.position, rightLowerArm.position, rightArmWidth);
			CreateHitBox(rightLowerArm, rightLowerArm.position, rightHand.position, rightArmWidth * 0.9f);

			// Legs
			float legWidthAspect = 0.3f;

			float leftLegWidth = Vector3.Distance(leftUpperLeg.position, leftLowerLeg.position) * legWidthAspect;
			CreateHitBox(leftUpperLeg, leftUpperLeg.position, leftLowerLeg.position, leftLegWidth);
			CreateHitBox(leftLowerLeg, leftLowerLeg.position, leftFoot.position, leftLegWidth * 0.9f);

			float rightLegWidth = Vector3.Distance(rightUpperLeg.position, rightLowerLeg.position) * legWidthAspect;
			CreateHitBox(rightUpperLeg, rightUpperLeg.position, rightLowerLeg.position, rightLegWidth);
			CreateHitBox(rightLowerLeg, rightLowerLeg.position, rightFoot.position, rightLegWidth * 0.9f);

			// Hands
			CreateHandHitBox(leftHand, leftLowerArm);
			CreateHandHitBox(rightHand, rightLowerArm);

			// Feet
			CreateFootHitBox(leftFoot, leftUpperLeg, root);
			CreateFootHitBox(rightFoot, rightUpperLeg, root);
		}

		private void CreateHandHitBox(Transform hand, Transform lowerArm)
		{
			Vector3 axis = hand.TransformVector(GetAxisVectorToPoint(hand, GetChildCentroid(hand, lowerArm.position)));

			Vector3 endPoint = hand.position - (lowerArm.position - hand.position) * 0.75f;
			endPoint = hand.position + Vector3.Project(endPoint - hand.position, axis).normalized * (endPoint - hand.position).magnitude;

			CreateHitBox(hand, hand.position, endPoint, Vector3.Distance(endPoint, hand.position) * 0.5f);
		}

		private void CreateFootHitBox(Transform foot, Transform upperLeg, Transform root)
		{
			float legHeight = (upperLeg.position - foot.position).magnitude;
			Vector3 axis = foot.TransformVector(GetAxisVectorToPoint(foot, GetChildCentroid(foot, foot.position + root.forward) + root.forward * legHeight * 0.2f));

			Vector3 endPoint = foot.position + root.forward * legHeight * 0.25f;
			endPoint = foot.position + Vector3.Project(endPoint - foot.position, axis).normalized * (endPoint - foot.position).magnitude;

			float width = Vector3.Distance(endPoint, foot.position) * 0.5f;
			Vector3 startPoint = foot.position;

			Vector3 direction = endPoint - startPoint;
			startPoint -= direction * 0.2f;

			CreateHitBox(foot, startPoint, endPoint, width);
		}

		private void CreateHitBox(Transform transform, Vector3 startPoint, Vector3 endPoint, float width)
		{
			GameObject hitboxObj = new GameObject($"{transform.name}_Hitbox");
			hitboxObj.transform.parent = transform;
			hitboxObj.transform.localScale = Vector3.one;
			hitboxObj.transform.localPosition = Vector3.zero;
			hitboxObj.transform.localRotation = Quaternion.identity;
			Transform t = hitboxObj.transform;

			Vector3 direction = endPoint - startPoint;
			float height = direction.magnitude * 1f;
			Vector3 heightAxis = GetAxisVectorToDirection(t, direction);

			Rigidbody rb = t.gameObject.AddComponent<Rigidbody>();
			rb.useGravity = false;
			rb.isKinematic = true;
			float scaleF = GetScaleF(t);

			Vector3 size = Vector3.Scale(heightAxis, new Vector3(height, height, height));
			if (size.x == 0f) size.x = width;
			if (size.y == 0f) size.y = width;
			if (size.z == 0f) size.z = width;

			BoxCollider box = t.gameObject.AddComponent<BoxCollider>();
			box.size = size / scaleF;
			box.size = new Vector3(Mathf.Abs(box.size.x), Mathf.Abs(box.size.y), Mathf.Abs(box.size.z));
			box.center = t.InverseTransformPoint(Vector3.Lerp(startPoint, endPoint, 0.5f));
			box.isTrigger = true;

			hitboxObj.AddComponent<DamageableHitBox>();
		}


		private void CreateHitBox(Transform transform, Vector3 startPoint, Vector3 endPoint, float width, float proportionAspect, Vector3 widthDirection)
		{
			GameObject hitboxObj = new GameObject($"{transform.name}_Hitbox");
			hitboxObj.transform.parent = transform;
			hitboxObj.transform.localScale = Vector3.one;
			hitboxObj.transform.localPosition = Vector3.zero;
			hitboxObj.transform.localRotation = Quaternion.identity;
			Transform t = hitboxObj.transform;

			Vector3 direction = endPoint - startPoint;
			float height = direction.magnitude * 1f;

			Vector3 heightAxis = GetAxisVectorToDirection(t, direction);
			Vector3 widthAxis = GetAxisVectorToDirection(t, widthDirection);

			if (widthAxis == heightAxis)
			{
				Debug.LogWarning("Width axis = height axis on " + t.name, t);
				widthAxis = new Vector3(heightAxis.y, heightAxis.z, heightAxis.x);
			}

			Rigidbody rb = t.gameObject.AddComponent<Rigidbody>();
			rb.useGravity = false;
			rb.isKinematic = true;

			Vector3 heightAdd = Vector3.Scale(heightAxis, new Vector3(height, height, height));
			Vector3 widthAdd = Vector3.Scale(widthAxis, new Vector3(width, width, width));

			Vector3 size = heightAdd + widthAdd;
			if (size.x == 0f) size.x = width * proportionAspect;
			if (size.y == 0f) size.y = width * proportionAspect;
			if (size.z == 0f) size.z = width * proportionAspect;

			BoxCollider box = t.gameObject.AddComponent<BoxCollider>();
			box.size = size / GetScaleF(t);
			box.center = t.InverseTransformPoint(Vector3.Lerp(startPoint, endPoint, 0.5f));
			box.isTrigger = true;

			hitboxObj.AddComponent<DamageableHitBox>();
		}

		/// <summary>
		/// Returns the local axis of the Transform towards a world space position.
		/// </summary>
		public Vector3 GetAxisVectorToPoint(Transform t, Vector3 worldPosition)
		{
			return GetAxisVectorToDirection(t, worldPosition - t.position);
		}

		/// <summary>
		/// Returns the local axis of the Transform that aligns the most with a direction.
		/// </summary>
		public Vector3 GetAxisVectorToDirection(Transform t, Vector3 direction)
		{
			return GetAxisVectorToDirection(t.rotation, direction);
		}

		/// <summary>
		/// Returns the local axis of a rotation space that aligns the most with a direction.
		/// </summary>
		public Vector3 GetAxisVectorToDirection(Quaternion r, Vector3 direction)
		{
			direction = direction.normalized;
			Vector3 axis = Vector3.right;

			float dotX = Mathf.Abs(Vector3.Dot(Vector3.Normalize(r * Vector3.right), direction));
			float dotY = Mathf.Abs(Vector3.Dot(Vector3.Normalize(r * Vector3.up), direction));
			if (dotY > dotX) axis = Vector3.up;
			float dotZ = Mathf.Abs(Vector3.Dot(Vector3.Normalize(r * Vector3.forward), direction));
			if (dotZ > dotX && dotZ > dotY) axis = Vector3.forward;

			return axis;
		}

		private Vector3 GetChildCentroid(Transform t, Vector3 fallback)
		{
			if (t.childCount == 0) return fallback;

			Vector3 c = Vector3.zero;
			for (int i = 0; i < t.childCount; i++)
			{
				c += t.GetChild(i).position;
			}
			c /= (float)t.childCount;

			return c;
		}

		private Vector3 GetUpperArmToHeadCentroid()
		{
			return Vector3.Lerp(GetUpperArmCentroid(), head.position, 0.5f);
		}

		private Vector3 GetUpperArmCentroid()
		{
			return Vector3.Lerp(leftUpperArm.position, rightUpperArm.position, 0.5f);
		}

		public float GetScaleF(Transform t)
		{
			Vector3 scale = t.lossyScale;
			return (scale.x + scale.y + scale.z) / 3f;
		}
#endif
	}
}
