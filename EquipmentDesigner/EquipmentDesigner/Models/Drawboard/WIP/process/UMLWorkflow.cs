using System.Collections.Generic;

namespace EquipmentDesigner.Models
{
    /// <summary>
    /// PackML 상태별 UML 워크플로우를 나타냅니다.
    /// 각 워크플로우는 DrawingElement 목록을 직접 포함하며,
    /// 연결 정보는 각 DrawingElement의 OutgoingArrows에 저장됩니다.
    /// </summary>
    public class UMLWorkflow : IIdentifiable
    {
        public string Id { get; set; }
        public PackMlState State { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string[] ImplementationInstructions { get; set; }

        /// <summary>
        /// 워크플로우에 포함된 모든 DrawingElement 목록.
        /// 연결 정보는 각 Element의 OutgoingArrows 속성에 저장됩니다.
        /// </summary>
        public List<DrawingElement> Steps { get; set; } = new List<DrawingElement>();
    }
}
