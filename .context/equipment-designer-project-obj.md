
## 목표
이 프로젝트의 목표는 `examples\yamls` 폴더에 있는 yaml포맷의 설비 설계 문서 생성을 위한 WPF 어플리케이션을 만드는 것임.

## 설계 문서
|설계 문서|설명|예제파일|
|---|---|---|
|equipment description files|최상위 설비 설계 문서로, 하위 설계 문서의 경로를 제공함.|`equipment-description-file`
|hardware description files|하드웨어 설계 문서. System, Unit, Device 계층으로 이뤄진 설비를 묘사하는 파일.|`hardware-description-file`
|io description files|hardware description files에 정의된 하드웨어의 입력/출력을 묘사하는 파일.|`io-description-file`
|process description files|hardware description file에 정의된 하드웨어의 PACK ML 프로토콜의 정의하는 17개 State별 프로세스(workflow)를 묘사하는 파일|`process-description-file`

## 필요성
- YAML 파일을 설계자가 직접 작성하는 것은 Human Error를 발생시킬 확률이 매우 높고 오래걸리기 때문에 설계자가 쉽게 설비 설계 문서를 생성할 수 있는 좋은 UX를 가진 어플리케이션이 필요함.


## 의사결정 기준
UX > Design: UX가 Design보다 중요함.
UX > LoCL UX가 코드의 양 보다 중요함.