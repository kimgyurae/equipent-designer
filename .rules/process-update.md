# Process 저장 규칙

## 핵심 원칙
Process 내부 데이터 변경 시 **반드시** `SaveProcessAsync()`를 명시적으로 호출해야 합니다.

## 이유
- Process 프로퍼티 setter는 **참조 변경 시에만** 저장을 트리거함
- 내부 컬렉션 수정(Steps.Add, OutgoingArrows.Add 등)은 setter를 호출하지 않음
- 동일 객체 참조 패턴으로 메모리상 반영되지만, 파일 저장은 별도 호출 필요

## 저장 호출 필수 위치
- Element 추가/삭제
- Connection 생성/편집/삭제
- Element 이동/크기조절 완료 시
