#if(UNITY_EDITOR)
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

namespace QuickSpawnPool
{
    public class XMLUtility
    {
        /// <summary>
        /// Returns path to folder containing XML's with pre-spawn data
        /// </summary>
        /// <returns>Path</returns>
        public static string
        GetPath()
        {
            return "Assets/Add-ons/QuickSpawnPool/Scenes/";
        }

        /// <summary>
        /// Возвращает имя xml-файла с данными о предзагрузке
        /// </summary>
        /// <param name="sceneName">Имя текущей сцены</param>
        /// <returns>Имя XML файла</returns>
        public static string
        GetFileName(string sceneName)
        {
            return sceneName + "_PrespawnData.xml";
        }

        /// <summary>
        /// Наполняет XML-файлы данными о префабах, которые использовались с игре на текущей карте
        /// </summary>
        /// <param name="SceneName">Имя текущей сцены</param>
        /// <param name="TransformStorage">Словарь Transform'ов, загруженных через QuickPoolManager</param>
        /// <param name="PoolableStorage">Словарь IPoolable'ов, загруженных через QuickPoolManager</param>
        public static void
        SavePoolInstancesStatistics(
            string SceneName, 
            Dictionary<int, PoolStatistics.PoolElementData> TransformStorage,
            Dictionary<int, PoolStatistics.PoolElementData> PoolableStorage)
        {
            if(string.IsNullOrEmpty(SceneName))
            {
                Debug.LogError("[QuickSpawnPoolXMLUtility] SavePoolInstancesStatistics() SceneName empty or equals null");
                return;
            }

            string xmlFullPath = GetPath() + GetFileName(SceneName);

            Dictionary<string, int> transformsInstancesData = new Dictionary<string, int>();
            Dictionary<string, int> poolablesInstancesData = new Dictionary<string, int>();

            ReadXML(xmlFullPath, ref transformsInstancesData, ref poolablesInstancesData);

            CompareXMLAndRuntimeData(TransformStorage, ref transformsInstancesData);
            CompareXMLAndRuntimeData(PoolableStorage, ref poolablesInstancesData);

            WriteXML(xmlFullPath, transformsInstancesData, poolablesInstancesData);
        }

        /// <summary>
        /// Считывает xml-файл с данными о предзагрузке
        /// </summary>
        /// <param name="fullPath">Полный путь у xml-файлу</param>
        /// <param name="transformsInstancesData">Ссылка на словарь с данными о Transform'ах</param>
        /// <param name="poolablesInstancesData">Ссылка на словарь с данными о IPoolable'ах</param>
        public static void
        ReadXML(
            string fullPath,
            ref Dictionary<string, int> transformsInstancesData,
            ref Dictionary<string, int> poolablesInstancesData)
        {
            if(!File.Exists(fullPath))
                return;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(fullPath);
            XmlNodeList transformNodes = xmlDoc.SelectNodes("//Level/Transforms/Element");
            XmlNodeList ipoolableNodes = xmlDoc.SelectNodes("//Level/IPoolables/Element");
            ReadXMLToInstancesDictonary(transformNodes, ref transformsInstancesData);
            ReadXMLToInstancesDictonary(ipoolableNodes, ref poolablesInstancesData);
        }

        private static void 
        CompareXMLAndRuntimeData(
            Dictionary<int, PoolStatistics.PoolElementData> storage, 
            ref Dictionary<string, int> instacesData)
        {
            foreach (var data in storage)
            {
                if(string.IsNullOrEmpty(data.Value.path))
                {
                    Debug.LogWarning("[XMLUtility] Prefab with name '" + data.Key + "' has no path in project so it can't be pre-spawned");
                    continue;
                }

                if(instacesData.ContainsKey(data.Value.path))
                {
                    int count = instacesData[data.Value.path];
                    if(count < data.Value.instances)
                    {
                        instacesData[data.Value.path] = data.Value.instances;
                    }
                }
                else
                {
                    instacesData.Add(data.Value.path, data.Value.instances);
                }
            }
        }

        private static void 
        ReadXMLToInstancesDictonary(XmlNodeList nodes, ref Dictionary<string, int> instancesData)
        {
            try
            {
                foreach (XmlNode node in nodes)
                {
                    XmlAttribute transformAttribute = node.Attributes["name"];
                    string prefabName = transformAttribute.Value;
                    int instancesCount = int.Parse(node.InnerText);
                    instancesData.Add(prefabName, instancesCount);
                }
            }
            catch(Exception e)
            {
                instancesData = new Dictionary<string, int>();
                Debug.LogError(e.Message);
            }
        }

        private static void 
        WriteXML(
            string xmlFullPath, 
            Dictionary<string, int> transformsInstancesData, 
            Dictionary<string, int> poolablesInstancesData)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                XmlNode rootNode = xmlDoc.CreateElement("Level");
                xmlDoc.AppendChild(rootNode);
                {
                    XmlNode transformsNode = xmlDoc.CreateElement("Transforms");
                    WriteXMLElements(ref xmlDoc, ref transformsNode, transformsInstancesData);
                    rootNode.AppendChild(transformsNode);

                    XmlNode poolablesNode = xmlDoc.CreateElement("IPoolables");
                    WriteXMLElements(ref xmlDoc, ref poolablesNode, poolablesInstancesData);
                    rootNode.AppendChild(poolablesNode);
                }
                xmlDoc.Save(xmlFullPath);
            }
            catch(Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        private static void 
        WriteXMLElements(
            ref XmlDocument xmlDoc, 
            ref XmlNode parentNode, 
            Dictionary<string, int> storage)
        {
            foreach (var data in storage)
            {
                XmlNode transformNode = xmlDoc.CreateElement("Element");
                XmlAttribute tranasformAttribute = xmlDoc.CreateAttribute("name");
                tranasformAttribute.Value = data.Key;
                transformNode.Attributes.Append(tranasformAttribute);
                transformNode.InnerText = data.Value.ToString();
                parentNode.AppendChild(transformNode);
            }
        } 
    }
}
#endif