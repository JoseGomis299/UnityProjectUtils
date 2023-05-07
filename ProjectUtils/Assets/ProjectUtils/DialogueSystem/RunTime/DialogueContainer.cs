using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUtils.DialogueSystem.RunTime
{
   [Serializable]
   public class DialogueContainer : ScriptableObject
   {
      public List<NodeLinkData> NodeLinks = new List<NodeLinkData>();
      public List<DialogueNodeData> DialogueNodeData = new List<DialogueNodeData>();
      public List<ExposedProperty> ExposedProperties = new List<ExposedProperty>();
   }
}
