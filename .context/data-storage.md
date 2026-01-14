# 데이터 저장소 아키텍처

## 개요

DataService는 **로컬 저장소**와 **리모트 저장소**를 분리하여 관리한다. 두 저장소 모두 현재는 로컬 파일 시스템을 사용하지만, 리모트 저장소는 백엔드 서버의 DB를 Mock하는 구조이다.

## 저장 경로

```
%LocalAppData%/EquipmentDesigner/
├── local/              ← 로컬 저장소 (사용자 작업 중인 데이터)
│   └── process.json
└── remote/             ← 리모트 저장소 Mock (서버 DB 시뮬레이션)
    ├── process.json
    └── uploaded-hardwares.json
```

## 로컬 vs 리모트 구분

| 구분 | 로컬 저장소 | 리모트 저장소 |
|------|------------|--------------|
| 경로 | `local/` | `remote/` |
| 용도 | 미완성 작업 임시 저장 | 서버 업로드된 데이터 |
| 접근 | Repository 직접 사용 | API Service 인터페이스 |
| 구현체 | `LocalProcessRepository` | `MockProcessApiService` |

## 리모트 저장소 Mock 설명

**중요**: `remote/` 폴더의 파일들은 실제 원격 서버가 아닌, **백엔드 서버의 DB를 로컬에서 시뮬레이션**한 것이다.

현재 백엔드 서비스가 미완성이므로:
1. `IHardwareApiService`, `IProcessApiService` 등 REST API 인터페이스를 정의
2. `MockHardwareApiService`, `MockProcessApiService`가 해당 인터페이스를 구현
3. Mock 구현체는 내부적으로 `remote/` 폴더의 JSON 파일을 읽고 쓰며 400ms 네트워크 지연을 시뮬레이션

**향후 계획**: 실제 백엔드 완성 시 Mock 구현체를 HTTP 클라이언트 기반 실제 구현체로 교체하면 된다.

## 핵심 인터페이스

```csharp
// REST API 시맨틱을 가진 인터페이스
IHardwareApiService  → MockHardwareApiService (remote/ 파일 사용)
IProcessApiService   → MockProcessApiService  (remote/ 파일 사용)

// 직접 파일 접근 Repository
ILocalProcessRepository → LocalProcessRepository (local/ 파일 사용)
```
