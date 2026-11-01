using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.ExceptionServices;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace MultiplayerARPG.Tests
{
    public class EntityMovementVehicleIntegrationTests
    {
        private string _temporaryScene;

        [OneTimeSetUp]
        public void PrepareRunnerScene()
        {
            var scene = SceneManager.GetActiveScene();
            if (!string.IsNullOrEmpty(scene.path)) return;
            // Unity rejects additive editor scenes while an untitled scene is open.
            // Give it a temporary path without unloading or discarding its contents.
            _temporaryScene = AssetDatabase.GenerateUniqueAssetPath("Assets/MovementValidationScene.unity");
            Assert.That(EditorSceneManager.SaveScene(scene, _temporaryScene), Is.True);
        }

        [OneTimeTearDown]
        public void RemoveTemporarySceneAsset()
        {
            if (_temporaryScene == null) return;
            AssetDatabase.DeleteAsset(_temporaryScene);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        public static IEnumerable<TestCaseData> InstalledVehicleChecks()
        {
            // The installed integrations already provide isolated server/owner/observer fixtures,
            // localhost UDP packet delivery, teleport, prediction and physics checks.
            Assembly editorAssembly = typeof(EntityMovementVehicleIntegrationTests).Assembly;
            Type uvc = editorAssembly.GetType("MultiplayerARPG.UVCMultiplayerValidation");
            if (uvc != null)
                yield return new TestCaseData(uvc, "Validate", null).SetName("UVCMovement_ServerOwnerAndObserver");
            Type samples = editorAssembly.GetType("MultiplayerARPG.SampleVehicleValidation");
            if (samples != null)
            {
                foreach (string vehicle in new[] { "Sample_Car", "Sample_Jet", "Sample_Propeller" })
                {
                    yield return new TestCaseData(samples, "ValidateVehicle", vehicle).SetName(vehicle + "Movement_ServerOwnerAndObserver");
                    yield return new TestCaseData(samples, "ValidatePhysics", vehicle).SetName(vehicle + "Movement_Physics");
                }
            }
            if (uvc == null && samples == null)
                yield return new TestCaseData(null, null, null).Ignore("No optional vehicle integrations installed.");
        }

        [TestCaseSource(nameof(InstalledVehicleChecks))]
        public void InstalledVehicleMovementIntegration(Type validator, string methodName, string argument)
        {
            MethodInfo method = validator.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            try
            {
                method.Invoke(null, new object[] { argument });
            }
            catch (TargetInvocationException ex) when (ex.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            }
        }
    }
}
