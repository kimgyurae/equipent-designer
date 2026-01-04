
# Software Requirements Specification
## Equipment Description UI Application

---

## 1. 프로젝트 개요 (Overview)

본 어플리케이션은 설비(Equipment)의 하드웨어 구성 및 프로세스, IO를 정의하기 위한 **Windows WPF 기반 UI 도구**입니다. 사용자는 계층적 구조를 가진 하드웨어 컴포넌트를 정의하고, 이를 통해 최종적인 Equipment Description 파일을 생성합니다.

### 1.1 하드웨어 계층 구조 (Hierarchy)

데이터는 아래와 같은 부모-자식 관계를 가집니다.

```
Equipment ⊃ System ⊃ Unit ⊃ Device
```

### 1.2 컴포넌트 상태 정의 (Component States)

각 컴포넌트 객체는 다음 상태 값 중 하나를 가집니다.

| 상태 | 설명 |
|------|------|
| **Undefined** | 데이터가 생성되었으나 필수 값이 부족함 |
| **Defined** | 모든 필수 입력 항목이 채워짐 |
| **Uploaded** | 서버에 저장 완료됨 |
| **Validated** | 데이터 검증이 완료됨 |

---

## 2. 공통 UI/UX 요구사항 (General UX)

### 자동 저장 (Auto-save)
- 사용자의 모든 입력 사항은 실시간(또는 포커스 아웃 시)으로 저장되어야 함
- 앱 재시작 시 마지막 작업 위치를 복구해야 함

### 네비게이션 패널 (Left Side Panel)
- 워크플로우 진행 중 좌측에 상시 노출
- 현재 워크플로우에 포함된 모든 컴포넌트 리스트 표시
- **진행률 표시**: 각 컴포넌트별 (작성된 필드 수 / 전체 필드 수)를 아이콘이나 숫자로 표시
- **Quick Jump**: 리스트 클릭 시 해당 정의 단계로 즉시 이동

### 워크플로우 종속성 (Cascading Workflow)
- 상위 컴포넌트 생성 시, 하위 컴포넌트 정의 단계까지 강제로 이어짐
- 예: New System 생성 → System 작성 → Unit 작성 → Device 작성 순으로 진행

---

## 3. 화면별 상세 요구사항

### 3.1 메인 화면 (Home Screen)

#### Dashboard
- 보유 중인 모든 Equipment, System, Unit, Device 리스트 뷰
- 각 리스트는 상태(State) 별 필터링 및 검색 기능 제공

#### Action Buttons
- New Equipment, New System, New Unit, New Device 버튼 배치

#### 작업 이어가기 (Resume Tasks)
- 미완성된 워크플로우(Defined 상태가 아닌 항목들) 리스트 노출

### 3.2 Equipment 정의 화면

#### 입력 항목

| 구분 | 항목 |
|------|------|
| **필수** | Equipment 종류, Equipment 이름 |
| **선택** | Display Name, Subname, Description, 고객사, Process |
| **파일 업로드** | 설계 문서 첨부 (PDF, PPT, MD, DRAWIO 확장자 지원) |

#### 기능
- 서버에서 기존 Equipment 불러오기(Import)

### 3.3 System / Unit / Device 정의 화면 (공통 템플릿)

각 단계는 유사한 폼 구조를 가지며 소속 관계(Parent) 설정이 중요합니다.

| 구분 | 항목 | 상세 내용 |
|------|------|----------|
| **소속 설정** | Parent Selection | 이전 단계에서 생성된 상위 객체 혹은 서버 검색을 통한 선택 (미선택 시 경고) |
| **기본 정보** | Name (필수) | Display Name, Subname, Description, 구현 가이드라인 |
| **공통 기능** | Commands | 1개 이상의 Command 정의 가능 (아래 4절 참조) |
| **특화 필드** | Process / IO | System/Unit은 Process 정보, Device는 IO 정보를 입력 |
| **서버 기능** | Import | 서버에 업로드된 Predefined 컴포넌트 정보를 불러와 필드 자동 완성 |

---

## 4. 데이터 구조 세부 사항 (Commands & Parameters)

System, Unit, Device는 하나 이상의 Command를 가질 수 있으며, 구조는 다음과 같습니다.

### 4.1 Command 객체

| 항목 | 필수 여부 | 설명 |
|------|----------|------|
| 이름 (Name) | 필수 | - |
| 설명 (Description) | 필수 | - |
| 파라미터 (Parameters) | 필수 | 1개 이상 필수 |

### 4.2 Parameter 객체 (Command 내부)

| 항목 | 필수 여부 | 설명 |
|------|----------|------|
| 이름 (Name) | 필수 | - |
| 타입 (Type) | 필수 | String, Int, Float, Bool 등 선택형 |
| 설명 (Description) | 필수 | - |

---

## 5. 워크플로우 로직 (Workflow Logic)

사용자가 선택한 시작점에 따라 시스템은 다음 단계들을 자동으로 체이닝(Chaining)합니다.

| 시작점 | 워크플로우 |
|--------|-----------|
| **New Equipment** | Equipment → System → Unit → Device |
| **New System** | System → Unit → Device |
| **New Unit** | Unit → Device |
| **New Device** | Device 전용 단계 |

### 이동 제어
- 각 단계에서 Next 클릭 시 필수 항목 체크 루틴 실행
- 필수 항목 누락 시 해당 필드 강조(Highlight)
