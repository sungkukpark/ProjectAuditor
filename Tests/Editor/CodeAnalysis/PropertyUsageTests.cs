using System;
using System.Linq;
using NUnit.Framework;
using Unity.ProjectAuditor.Editor;
using Unity.ProjectAuditor.Editor.AssemblyUtils;
using Unity.ProjectAuditor.Editor.Modules;
using Unity.ProjectAuditor.Editor.CodeAnalysis;
using Unity.ProjectAuditor.Editor.Utils;
using UnityEngine;

namespace Unity.ProjectAuditor.EditorTests
{
    class PropertyUsageTests : TestFixtureBase
    {
        TempAsset m_TempAssetObjectName;
        TempAsset m_TempAssetBaseTypePropertyUsage;
#if UNITY_2019_1_OR_NEWER
        TempAsset m_TempAssetUxmlAttributeDescriptionPropertyUsage;
#endif

        [OneTimeSetUp]
        public void SetUp()
        {
            m_TempAssetObjectName = new TempAsset("ObjectNameTest.cs", @"
using UnityEngine;
class ObjectNameTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log(gameObject.name);
        Debug.Log(transform.name);
        Debug.Log(this.name);
    }
}
");

            m_TempAssetBaseTypePropertyUsage = new TempAsset("BaseTypePropertyUsage.cs", @"
using UnityEngine;
class BaseTypePropertyUsage
{
    void Test()
    {
        Material material = null;
        Material[] materials;
        Material[] sharedMaterials;

        // check base class Renderer
        Renderer baseClassRenderer = null;
        material = baseClassRenderer.material;
        materials = baseClassRenderer.materials;
        sharedMaterials = baseClassRenderer.sharedMaterials;

        // check derived class LineRenderer
        LineRenderer subClassRenderer = null;
        material = subClassRenderer.material;
        materials = subClassRenderer.materials;
        sharedMaterials = subClassRenderer.sharedMaterials;
    }
}
");

#if UNITY_2019_1_OR_NEWER
            m_TempAssetUxmlAttributeDescriptionPropertyUsage = new TempAsset("UxmlAttributeDescriptionPropertyUsage.cs", @"
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

class UxmlAttributeDescriptionPropertyUsage
{
    UxmlFloatAttributeDescription desc;
    IEnumerable<string> names;

    void Test()
    {
        names = desc.obsoleteNames;
    }
}
");
#endif
        }

        [Test]
        public void CodeAnalysis_PropertyName_IsReported()
        {
            var issues = AnalyzeAndFindAssetIssues(m_TempAssetObjectName);

            var propertyNameIssues = issues.Where(i => i.descriptor.id == "PAC0231").ToArray();

            Assert.AreEqual(3, propertyNameIssues.Length);
            Assert.True(propertyNameIssues.All(i => i.description.Equals("'UnityEngine.Object.name' usage")));
        }

        [Test]
        public void CodeAnalysis_PropertyOfBaseType_IsReported()
        {
            var issues = AnalyzeAndFindAssetIssues(m_TempAssetBaseTypePropertyUsage);

            var propertyOfBaseTypeIssues = issues.Where(
                i => i.descriptor.id == "PAC0039" || i.descriptor.id == "PAC0084" || i.descriptor.id == "PAC0085")
                .ToArray();

            Assert.AreEqual(6, propertyOfBaseTypeIssues.Length);
        }

#if UNITY_2019_1_OR_NEWER
        [Test]
        public void CodeAnalysis_PropertyUxmlAttributeDescription_IsReported()
        {
            var issues = AnalyzeAndFindAssetIssues(m_TempAssetUxmlAttributeDescriptionPropertyUsage);

            var propertyUxmlAttributeIssues = issues.Where(i => i.descriptor.id == "PAC0191").ToArray();

            Assert.AreEqual(1, propertyUxmlAttributeIssues.Length);
            Assert.AreEqual("'UnityEngine.UIElements.UxmlAttributeDescription.obsoleteNames' usage", propertyUxmlAttributeIssues[0].description);
        }

#endif
    }
}
