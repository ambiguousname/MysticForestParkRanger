using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using Interactions;
using UnityEditor;
using Interactions.Behaviors;
using InkTools;
using Utility;

namespace SceneValidation {
    [TestFixture]
    [TestFixtureSource(typeof(ScenesProvider))]
    public class SceneValidation {
        private string scenePath;

        public SceneValidation(string scenePath) {
            this.scenePath = scenePath;
        } 


        [OneTimeSetUp]
        public void Setup() {
            EditorSceneManager.OpenScene(this.scenePath);
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [Test]
        public void ValidateInteractions() {
            var interactions = GameObject.FindObjectsOfType<Interaction>();
            foreach (var interaction in interactions) {
                Assert.IsTrue(interaction.HasInteractionBehavior(),
                    $"{EditorSceneManager.GetActiveScene().name} invalid interaction {interaction.type} on {interaction.name}");
            }
        }

        [Test]
        public void ValidateInkManager() {
            Assert.IsTrue(GameObject.FindObjectsOfType<InkManager>().Length <= 1, "More than one InkManager in the scene.");
            var manager = GameObject.FindObjectOfType<InkManager>();
            if (manager != null) {
                Assert.IsTrue(manager.hasInkJSON);
            }
        }

        [Test]
        public void ValidateCustomInteractions() {
            var interactions = GameObject.FindObjectsOfType<Interaction>();
            foreach (var interaction in interactions) {
                if (interaction.GetType() == typeof(CustomInteraction)) {
                    CustomInteraction c = (CustomInteraction)interaction.behavior;
                    Assert.IsNotNull(c.targetObject);
                    Assert.IsNotNull(c.onUpdate);
                    Assert.IsFalse(c.onUpdate.IsNull());
                }
            }
        }

        [Test]
        public void ValidateInkInteractions() {
            var interactions = GameObject.FindObjectsOfType<Interaction>();
            foreach (var interaction in interactions) {
                if (interaction.GetType() == typeof(InkInteraction)) {
                    InkInteraction i = (InkInteraction)interaction.behavior;
                    var manager = GameObject.FindFirstObjectByType<InkManager>();
                    Assert.IsNotNull(manager);
                    manager.PathExists(i.inkKnot);
                }
            }
        }

        [OneTimeTearDown]
        public void Teardown() {
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        }
    }

    public class ScenesProvider : IEnumerable {
        public IEnumerator GetEnumerator() {
            foreach (var scene in EditorBuildSettings.scenes) {
                if (!scene.enabled || scene.path == null) {
                    continue;
                }

                yield return scene.path;
            }
        }
    }
}